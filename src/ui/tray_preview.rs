//! Settings uses the same selection and pixels as the tray, with event-driven desktop updates.
use super::tray;
use crate::{
    platform::{
        desktop::{DesktopManager, Snapshot},
        events::{Watcher, DESKTOP_CHANGED},
        wide,
    },
    settings::Settings,
};
use windows_reactor::*;
use windows_sys::Win32::{
    Foundation::*, System::LibraryLoader::GetModuleHandleW, UI::WindowsAndMessaging::*,
};

pub struct Preview {
    manager: Option<DesktopManager>,
    snapshot: Option<Snapshot>,
    observer: Option<Observer>,
    images: Vec<(u32, EncodedImage)>,
    metrics: tray::Metrics,
}
impl Preview {
    pub fn new(settings: &Settings, changed: impl Fn(bool) + 'static) -> Self {
        let mut result = Self {
            manager: None,
            snapshot: None,
            observer: Observer::new(changed).ok(),
            images: Vec::new(),
            metrics: tray::metrics(),
        };
        result.refresh(settings, false);
        result
    }
    pub fn refresh(&mut self, settings: &Settings, reconnect: bool) {
        if reconnect {
            self.manager = None;
            if let Some(observer) = &mut self.observer {
                observer.restart();
            }
        }
        if self.manager.is_none() {
            self.manager = DesktopManager::connect().ok();
        }
        self.snapshot = self.manager.as_ref().and_then(|m| m.snapshot().ok());
        if self.snapshot.is_none() {
            self.manager = None;
        }
        self.render(settings);
    }
    pub fn render(&mut self, settings: &Settings) {
        self.metrics = tray::metrics();
        self.images = self
            .snapshot
            .as_ref()
            .map(|snap| {
                tray::icon_specs(settings, snap)
                    .into_iter()
                    .map(|(id, label, enabled)| {
                        let pixels = tray::pixels(&label, settings, enabled, id >= 3, self.metrics);
                        (id, EncodedImage::new(bitmap(self.metrics.size, &pixels)))
                    })
                    .collect()
            })
            .unwrap_or_default();
    }
    pub fn desktop_name(&self) -> &str {
        self.snapshot
            .as_ref()
            .map(|s| s.active().name.as_str())
            .unwrap_or("Desktop preview")
    }
    pub fn view(&self) -> View {
        let Some(snapshot) = &self.snapshot else {
            return TextBlock::new()
                .text("Tray preview is unavailable until the desktop connection is restored.")
                .text_wrapping(TextWrapping::Wrap)
                .into();
        };
        let size = self.metrics.size as f64 * 96. / self.metrics.dpi as f64;
        StackPanel::new().spacing(12.).children((
            TextBlock::new().text("Current tray icons").font_size(20.).font_weight(FontWeight::SEMI_BOLD),
            TextBlock::new().text(format!("Desktop {} of {} · {}",snapshot.current+1,snapshot.desktops.len(),snapshot.active().name)).text_wrapping(TextWrapping::Wrap),
            Border::new().corner_radius(8.).padding(Thickness::xy(12.,8.)).horizontal_alignment(HorizontalAlignment::Left)
                .background(if self.metrics.dark {Color::rgb(32,32,32)}else{Color::rgb(243,243,243)})
                .content(StackPanel::new().orientation(Orientation::Horizontal).keyed_children(self.images.iter().map(|(id,data)| (
                    *id as u64,Border::new().width(32.).height(32.).content(
                        Image::new().source_data(data.clone()).width(size).height(size).stretch(Stretch::Uniform)
                            .horizontal_alignment(HorizontalAlignment::Center).vertical_alignment(VerticalAlignment::Center)
                    )
                )))),
            TextBlock::new().text("Your current symbols, fonts, colors and enabled icons. Windows controls their order and spacing in the taskbar.").font_size(12.).opacity(0.72).text_wrapping(TextWrapping::Wrap)
        ))
    }
}

/// BMP V5 keeps alpha when decoded by WinUI/WIC. HICON is premultiplied;
/// WIC's encoded BGRA input is straight alpha, so undo premultiplication here.
fn bitmap(size: i32, pixels: &[u32]) -> Vec<u8> {
    let mut bytes = vec![0u8; 138];
    bytes[..2].copy_from_slice(b"BM");
    let mut field =
        |offset: usize, value: u32| bytes[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
    field(2, 138 + pixels.len() as u32 * 4);
    field(10, 138);
    field(14, 124);
    field(18, size as u32);
    field(22, (-size) as u32);
    field(26, 1 | (32 << 16));
    field(30, 3);
    field(34, pixels.len() as u32 * 4);
    field(54, 0x00ff0000);
    field(58, 0x0000ff00);
    field(62, 0x000000ff);
    field(66, 0xff000000);
    field(70, 0x73524742); // LCS_sRGB
    field(122, 4); // LCS_GM_IMAGES
    for pixel in pixels {
        let alpha = pixel >> 24;
        for shift in [0, 8, 16] {
            bytes.push(
                ((((pixel >> shift) & 255) * 255 + alpha / 2)
                    .checked_div(alpha)
                    .unwrap_or(0))
                .min(255) as u8,
            );
        }
        bytes.push(alpha as u8);
    }
    bytes
}

struct Observer {
    hwnd: HWND,
    watcher: Option<Watcher>,
}
struct Callback(Box<dyn Fn(bool)>);
impl Observer {
    fn new(changed: impl Fn(bool) + 'static) -> Result<Self, String> {
        unsafe {
            let class = wide("WVDH.SettingsPreviewObserver");
            let instance = GetModuleHandleW(std::ptr::null());
            RegisterClassW(&WNDCLASSW {
                lpfnWndProc: Some(window_proc),
                hInstance: instance,
                lpszClassName: class.as_ptr(),
                ..std::mem::zeroed()
            });
            // Set user data after creation so failed creation has a single owner.
            let hwnd = CreateWindowExW(
                0,
                class.as_ptr(),
                std::ptr::null(),
                0,
                0,
                0,
                0,
                0,
                std::ptr::null_mut(),
                std::ptr::null_mut(),
                instance,
                std::ptr::null(),
            );
            if hwnd.is_null() {
                return Err(std::io::Error::last_os_error().to_string());
            }
            SetWindowLongPtrW(
                hwnd,
                GWLP_USERDATA,
                Box::into_raw(Box::new(Callback(Box::new(changed)))) as _,
            );
            Ok(Self {
                hwnd,
                watcher: Watcher::start(hwnd).ok(),
            })
        }
    }
    fn restart(&mut self) {
        self.watcher = None;
        self.watcher = Watcher::start(self.hwnd).ok();
    }
}
impl Drop for Observer {
    fn drop(&mut self) {
        self.watcher = None;
        unsafe {
            DestroyWindow(self.hwnd);
        }
    }
}
unsafe extern "system" fn window_proc(hwnd: HWND, msg: u32, w: WPARAM, l: LPARAM) -> LRESULT {
    let ptr = GetWindowLongPtrW(hwnd, GWLP_USERDATA) as *mut Callback;
    if !ptr.is_null() {
        match msg {
            DESKTOP_CHANGED | WM_SETTINGCHANGE | WM_THEMECHANGED | WM_DISPLAYCHANGE => {
                ((*ptr).0)(msg == DESKTOP_CHANGED && w != 0)
            }
            WM_NCDESTROY => {
                SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
                drop(Box::from_raw(ptr));
            }
            _ => {}
        }
    }
    DefWindowProcW(hwnd, msg, w, l)
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn preview_encoding_preserves_alpha_and_color() {
        let bytes = bitmap(2, &[0, 0x80402010, 0xffffffff, 0xff000000]);
        assert_eq!(&bytes[..2], b"BM");
        assert_eq!(&bytes[138..142], &[0, 0, 0, 0]);
        assert_eq!(&bytes[142..146], &[32, 64, 128, 128]);
        assert_eq!(&bytes[146..150], &[255, 255, 255, 255]);
    }
}

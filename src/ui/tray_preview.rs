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
    Foundation::*, Graphics::Gdi::*, System::LibraryLoader::GetModuleHandleW,
    UI::WindowsAndMessaging::*,
};

pub struct Preview {
    manager: Option<DesktopManager>,
    snapshot: Option<Snapshot>,
    observer: Option<Observer>,
    images: Vec<(u32, EncodedImage)>,
    metrics: tray::Metrics,
}
impl Preview {
    pub fn new(settings: &Settings, changed: impl Fn(Option<bool>) + 'static) -> Self {
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
            TextBlock::new()
                .text("Current tray icons")
                .font_size(20.)
                .font_weight(FontWeight::SEMI_BOLD),
            TextBlock::new()
                .text(format!(
                    "Desktop {} of {} · {}",
                    snapshot.current + 1,
                    snapshot.desktops.len(),
                    snapshot.active().name
                ))
                .text_wrapping(TextWrapping::Wrap),
            Border::new()
                .corner_radius(8.)
                .padding(Thickness::xy(12., 8.))
                .horizontal_alignment(HorizontalAlignment::Left)
                .background(if self.metrics.dark {
                    Color::rgb(32, 32, 32)
                } else {
                    Color::rgb(243, 243, 243)
                })
                .content(
                    StackPanel::new()
                        .orientation(Orientation::Horizontal)
                        .keyed_children(self.images.iter().map(|(id, data)| {
                            (
                                *id as u64,
                                Border::new().width(32.).height(32.).content(
                                    Image::new()
                                        .source_data(data.clone())
                                        .width(size)
                                        .height(size)
                                        .stretch(Stretch::Uniform)
                                        .horizontal_alignment(HorizontalAlignment::Center)
                                        .vertical_alignment(VerticalAlignment::Center),
                                ),
                            )
                        })),
                ),
            self.enlarged_icons(),
        ))
    }
    fn enlarged_icons(&self) -> View {
        StackPanel::new().spacing(8.).children((
            TextBlock::new()
                .text("Icon detail")
                .font_size(12.)
                .opacity(0.65),
            StackPanel::new()
                .orientation(Orientation::Horizontal)
                .spacing(12.)
                .keyed_children(self.images.iter().map(|(id, data)| {
                    (
                        *id as u64,
                        Border::new()
                            .width(56.)
                            .height(56.)
                            .corner_radius(8.)
                            .background(if self.metrics.dark {
                                Color::rgb(32, 32, 32)
                            } else {
                                Color::rgb(243, 243, 243)
                            })
                            .content(
                                Image::new()
                                    .source_data(data.clone())
                                    .width(32.)
                                    .height(32.)
                                    .stretch(Stretch::Uniform),
                            ),
                    )
                })),
        ))
    }
    pub fn desktop_view(&self, settings: &Settings) -> View {
        // The primary monitor's real work area anchors both previews and overlays.
        let (screen, mut work) = unsafe {
            let mut info: MONITORINFO = std::mem::zeroed();
            info.cbSize = std::mem::size_of_val(&info) as u32;
            let monitor = MonitorFromPoint(POINT { x: 0, y: 0 }, MONITOR_DEFAULTTOPRIMARY);
            if GetMonitorInfoW(monitor, &mut info) != 0 {
                (info.rcMonitor, info.rcWork)
            } else {
                (
                    RECT {
                        left: 0,
                        top: 0,
                        right: 1920,
                        bottom: 1080,
                    },
                    RECT {
                        left: 0,
                        top: 0,
                        right: 1920,
                        bottom: 1032,
                    },
                )
            }
        };
        let width = (screen.right - screen.left) as f64;
        let height = (screen.bottom - screen.top) as f64;
        let dark = self.metrics.dark;
        // Enlarge details in the miniature without changing real overlay settings.
        const DETAIL_SCALE: f64 = 2.25;
        let edge = |distance: i32, limit: i32| {
            ((distance as f64 * DETAIL_SCALE).round() as i32).min(limit / 4)
        };
        work.left = screen.left + edge(work.left - screen.left, screen.right - screen.left);
        work.right = screen.right - edge(screen.right - work.right, screen.right - screen.left);
        work.top = screen.top + edge(work.top - screen.top, screen.bottom - screen.top);
        work.bottom = screen.bottom - edge(screen.bottom - work.bottom, screen.bottom - screen.top);
        let mut layers: Vec<View> = vec![
            Border::new()
                .width(width)
                .height(height)
                .background(Color::rgb(28, 57, 89))
                .into(),
            Border::new()
                .width(width * 0.6)
                .height(height * 0.7)
                .corner_radius(height * 0.25)
                .canvas_left(width * 0.2)
                .canvas_top(height * 0.15)
                .background(Color::rgb(38, 87, 139))
                .into(),
        ];
        // Keep the taskbar on its real edge, enlarged with its icons for readability.
        let taskbar = if work.bottom < screen.bottom {
            RECT {
                left: screen.left,
                top: work.bottom,
                right: screen.right,
                bottom: screen.bottom,
            }
        } else if work.top > screen.top {
            RECT {
                left: screen.left,
                top: screen.top,
                right: screen.right,
                bottom: work.top,
            }
        } else if work.left > screen.left {
            RECT {
                left: screen.left,
                top: screen.top,
                right: work.left,
                bottom: screen.bottom,
            }
        } else if work.right < screen.right {
            RECT {
                left: work.right,
                top: screen.top,
                right: screen.right,
                bottom: screen.bottom,
            }
        } else {
            RECT {
                left: screen.left,
                top: screen.bottom - 2,
                right: screen.right,
                bottom: screen.bottom,
            }
        };
        let taskbar_height = (taskbar.bottom - taskbar.top) as f64;
        let dpi_scale = self.metrics.dpi as f64 / 96.;
        let detail_scale = dpi_scale * DETAIL_SCALE;
        let icons = StackPanel::new()
            .orientation(Orientation::Horizontal)
            .keyed_children(self.images.iter().map(|(id, data)| {
                (
                    *id as u64,
                    Border::new()
                        .width(32. * detail_scale)
                        .height(taskbar_height)
                        .content(
                            Image::new()
                                .source_data(data.clone())
                                .width(self.metrics.size as f64 * DETAIL_SCALE)
                                .height(self.metrics.size as f64 * DETAIL_SCALE)
                                .stretch(Stretch::Uniform),
                        ),
                )
            }));
        layers.push(
            Border::new()
                .canvas_left((taskbar.left - screen.left) as f64)
                .canvas_top((taskbar.top - screen.top) as f64)
                .width((taskbar.right - taskbar.left) as f64)
                .height(taskbar_height)
                .background(if dark {
                    Color::rgb(32, 32, 32)
                } else {
                    Color::rgb(243, 243, 243)
                })
                .content(
                    Grid::new().children((
                        FontIcon::new()
                            .glyph("\u{E782}")
                            .width(24. * detail_scale)
                            .height(24. * detail_scale)
                            .horizontal_alignment(HorizontalAlignment::Center)
                            .vertical_alignment(VerticalAlignment::Center),
                        Border::new()
                            .horizontal_alignment(HorizontalAlignment::Right)
                            .margin(Thickness::new(0., 0., 100. * detail_scale, 0.))
                            .content(icons),
                        TextBlock::new()
                            .text("12:00")
                            .font_size(12. * detail_scale)
                            .foreground(if dark {
                                Color::rgb(240, 240, 240)
                            } else {
                                Color::rgb(25, 25, 25)
                            })
                            .horizontal_alignment(HorizontalAlignment::Right)
                            .vertical_alignment(VerticalAlignment::Center)
                            .margin(Thickness::new(0., 0., 24. * detail_scale, 0.)),
                    )),
                ),
        );
        for status in [false, true] {
            let feature = if status {
                "feature.showDesktopStatusOverlay"
            } else {
                "feature.showDesktopSwitchOverlay"
            };
            if !settings.bool(feature) {
                continue;
            }
            let mut appearance = super::overlay::Appearance::new(settings, status);
            appearance.width = (appearance.width as f64 * DETAIL_SCALE).round() as i32;
            appearance.height = (appearance.height as f64 * DETAIL_SCALE).round() as i32;
            appearance.radius = (appearance.radius as f64 * DETAIL_SCALE).round() as i32;
            appearance.font_size *= DETAIL_SCALE;
            let bounds = appearance.bounds(work, self.metrics.dpi);
            // Render at 2x the miniature resolution for crisp text, using the real painter.
            let raster_scale = (1200. / width).min(1.);
            let w = (((bounds.right - bounds.left) as f64 * raster_scale).round() as i32).max(1);
            let h = (((bounds.bottom - bounds.top) as f64 * raster_scale).round() as i32).max(1);
            let pixels = super::overlay::preview_pixels(
                &appearance,
                self.desktop_name(),
                w,
                h,
                dpi_scale * raster_scale,
            );
            layers.push(
                Image::new()
                    .source_data(EncodedImage::new(bitmap_rect(w, h, &pixels)))
                    .stretch(Stretch::Fill)
                    .canvas_left((bounds.left - screen.left) as f64)
                    .canvas_top((bounds.top - screen.top) as f64)
                    .width((bounds.right - bounds.left) as f64)
                    .height((bounds.bottom - bounds.top) as f64)
                    .into(),
            );
        }
        StackPanel::new().spacing(14.).children((
            TextBlock::new()
                .text("Desktop appearance")
                .font_size(20.)
                .font_weight(FontWeight::SEMI_BOLD),
            Border::new()
                .corner_radius(8.)
                .border_thickness(1.)
                .border_brush(ThemeBrush::CardStroke)
                .content(
                    Viewbox::new()
                        .max_height(300.)
                        .stretch(Stretch::Uniform)
                        .slots([SlotView::new(
                            ViewboxSlot::Child,
                            Canvas::new().width(width).height(height).keyed_children(
                                layers.into_iter().enumerate().map(|(i, v)| (i as u64, v)),
                            ),
                        )]),
                ),
            TextBlock::new()
                .text(
                    self.snapshot
                        .as_ref()
                        .map(|snapshot| {
                            format!(
                                "Enlarged preview · Desktop {} of {}",
                                snapshot.current + 1,
                                snapshot.desktops.len()
                            )
                        })
                        .unwrap_or_else(|| "Desktop preview".into()),
                )
                .font_size(12.)
                .opacity(0.7),
        ))
    }
}

/// BMP V5 keeps alpha when decoded by WinUI/WIC. HICON is premultiplied;
/// WIC's encoded BGRA input is straight alpha, so undo premultiplication here.
fn bitmap(size: i32, pixels: &[u32]) -> Vec<u8> {
    bitmap_rect(size, size, pixels)
}
fn bitmap_rect(width: i32, height: i32, pixels: &[u32]) -> Vec<u8> {
    let mut bytes = vec![0u8; 138];
    bytes[..2].copy_from_slice(b"BM");
    let mut field =
        |offset: usize, value: u32| bytes[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
    field(2, 138 + pixels.len() as u32 * 4);
    field(10, 138);
    field(14, 124);
    field(18, width as u32);
    field(22, (-height) as u32);
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
struct Callback(Box<dyn Fn(Option<bool>)>);
impl Observer {
    fn new(changed: impl Fn(Option<bool>) + 'static) -> Result<Self, String> {
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
                ((*ptr).0)(Some(msg == DESKTOP_CHANGED && w != 0))
            }
            super::settings_process::SHOW_ABOUT => ((*ptr).0)(None),
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

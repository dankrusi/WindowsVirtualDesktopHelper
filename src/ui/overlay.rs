use crate::{platform::wide, settings::Settings};
use windows_sys::Win32::{
    Foundation::*,
    Graphics::{Dwm::*, Gdi::*},
    System::LibraryLoader::GetModuleHandleW,
    UI::{HiDpi::*, WindowsAndMessaging::*},
};
/// One appearance model for real overlay windows and the Settings preview.
pub struct Appearance {
    pub width: i32,
    pub height: i32,
    pub offset: i32,
    pub position: String,
    pub fg: u32,
    pub bg: u32,
    pub font: String,
    pub font_size: f64,
    pub opacity: u8,
    pub radius: i32,
    pub border: bool,
}
impl Appearance {
    pub fn new(settings: &Settings, status: bool) -> Self {
        let section = if status {
            "theme.status"
        } else {
            "theme.overlay"
        };
        let feature = if status {
            "feature.showDesktopStatusOverlay"
        } else {
            "feature.showDesktopSwitchOverlay"
        };
        let theme = if super::dark(settings) {
            "dark"
        } else {
            "light"
        };
        Self {
            width: settings.int(&format!("{section}.width")),
            height: settings.int(&format!("{section}.height")),
            offset: settings.int(&format!("{section}.offset")),
            position: settings.get(&format!("{feature}.position")).into(),
            fg: super::color(
                settings.get(&format!("{section}.overlayFG.{theme}")),
                0xffffff,
            ),
            bg: super::color(settings.get(&format!("{section}.overlayBG.{theme}")), 0),
            font: settings.get(&format!("{section}.font")).into(),
            font_size: settings.number(&format!("{section}.fontSize")),
            opacity: if settings.bool(&format!("{feature}.translucent")) {
                153
            } else {
                255
            },
            radius: if settings.bool(&format!("{section}.roundedCorners")) {
                8
            } else {
                0
            },
            border: settings.bool(&format!("{section}.border")),
        }
    }
    pub fn bounds(&self, work: RECT, dpi: u32) -> RECT {
        let w = scale(self.width, dpi).min(work.right - work.left);
        let h = scale(self.height, dpi).min(work.bottom - work.top);
        let (x, y) = position(work, w, h, scale(self.offset, dpi), &self.position);
        RECT {
            left: x,
            top: y,
            right: x + w,
            bottom: y + h,
        }
    }
}
/// Rasterize the same GDI content at preview resolution; keep alpha for WinUI.
pub fn preview_pixels(
    appearance: &Appearance,
    label: &str,
    width: i32,
    height: i32,
    scale: f64,
) -> Vec<u32> {
    unsafe {
        let dc = CreateCompatibleDC(std::ptr::null_mut());
        let mut info: BITMAPINFO = std::mem::zeroed();
        info.bmiHeader.biSize = std::mem::size_of::<BITMAPINFOHEADER>() as u32;
        info.bmiHeader.biWidth = width;
        info.bmiHeader.biHeight = -height;
        info.bmiHeader.biPlanes = 1;
        info.bmiHeader.biBitCount = 32;
        let mut bits = std::ptr::null_mut();
        let bitmap = CreateDIBSection(
            dc,
            &info,
            DIB_RGB_COLORS,
            &mut bits,
            std::ptr::null_mut(),
            0,
        );
        if bitmap.is_null() || bits.is_null() {
            if !bitmap.is_null() {
                DeleteObject(bitmap);
            }
            DeleteDC(dc);
            return vec![0; (width * height) as usize];
        }
        let old = SelectObject(dc, bitmap);
        let state = State {
            label: label.into(),
            fg: appearance.fg,
            bg: appearance.bg,
            font: super::font(
                &appearance.font,
                (appearance.font_size * 96. / 72. * scale).round().max(1.) as i32,
            ),
            alpha: appearance.opacity,
            target: appearance.opacity,
            border: appearance.border,
            radius: (appearance.radius as f64 * scale).round() as i32,
            stroke: scale.round().max(1.) as i32,
        };
        paint(
            dc,
            RECT {
                left: 0,
                top: 0,
                right: width,
                bottom: height,
            },
            &state,
        );
        GdiFlush();
        let pixels = std::slice::from_raw_parts(bits as *const u32, (width * height) as usize);
        let radius = (state.radius as f64)
            .min(width as f64 / 2.)
            .min(height as f64 / 2.);
        let result = pixels
            .iter()
            .enumerate()
            .map(|(i, color)| {
                let x = (i as i32 % width) as f64 + 0.5;
                let y = (i as i32 / width) as f64 + 0.5;
                let dx = (radius - x).max(x - (width as f64 - radius)).max(0.);
                let dy = (radius - y).max(y - (height as f64 - radius)).max(0.);
                let coverage = if radius == 0. {
                    1.
                } else {
                    (radius + 0.5 - dx.hypot(dy)).clamp(0., 1.)
                };
                let alpha = (appearance.opacity as f64 * coverage).round() as u32;
                let mut pixel = alpha << 24;
                for shift in [0, 8, 16] {
                    pixel |= (((color >> shift) & 255) * alpha / 255) << shift;
                }
                pixel
            })
            .collect();
        SelectObject(dc, old);
        DeleteObject(bitmap);
        DeleteDC(dc);
        result
    }
}

pub struct Overlays {
    windows: Vec<HWND>,
}
impl Overlays {
    pub fn new() -> Self {
        Self {
            windows: Vec::new(),
        }
    }
    pub fn clear(&mut self) {
        for hwnd in self.windows.drain(..) {
            unsafe {
                if IsWindow(hwnd) != 0 {
                    DestroyWindow(hwnd);
                }
            }
        }
    }
    pub fn show(&mut self, s: &Settings, label: &str, status: bool, duration: Option<u32>) {
        self.clear();
        unsafe {
            let feature = if status {
                "feature.showDesktopStatusOverlay"
            } else {
                "feature.showDesktopSwitchOverlay"
            };
            let appearance = Appearance::new(s, status);
            let mut monitors = Vec::<(RECT, u32)>::new();
            EnumDisplayMonitors(
                std::ptr::null_mut(),
                std::ptr::null(),
                Some(monitor),
                (&mut monitors as *mut Vec<(RECT, u32)>) as isize,
            );
            if !s.bool(&format!("{feature}.showOnAllMonitors")) {
                let mut info: MONITORINFO = std::mem::zeroed();
                info.cbSize = std::mem::size_of_val(&info) as u32;
                let m = MonitorFromPoint(POINT { x: 0, y: 0 }, MONITOR_DEFAULTTOPRIMARY);
                if GetMonitorInfoW(m, &mut info) != 0 {
                    monitors = vec![(info.rcWork, monitor_dpi(m))];
                }
            }
            let class = wide("WVDH.Overlay");
            let instance = GetModuleHandleW(std::ptr::null());
            let wc = WNDCLASSW {
                lpfnWndProc: Some(proc),
                hInstance: instance,
                lpszClassName: class.as_ptr(),
                ..std::mem::zeroed()
            };
            RegisterClassW(&wc);
            for (r, dpi) in monitors {
                let bounds = appearance.bounds(r, dpi);
                let (x, y, w, h) = (
                    bounds.left,
                    bounds.top,
                    bounds.right - bounds.left,
                    bounds.bottom - bounds.top,
                );
                let animate = s.bool(&format!("{feature}.animate"));
                let target = appearance.opacity;
                let state = Box::new(State {
                    label: label.into(),
                    fg: appearance.fg,
                    bg: appearance.bg,
                    font: super::font(
                        &appearance.font,
                        (appearance.font_size * dpi as f64 / 72.) as i32,
                    ),
                    border: appearance.border,
                    radius: scale(appearance.radius, dpi),
                    stroke: scale(1, dpi).max(1),
                    alpha: if animate { 0 } else { target },
                    target,
                });
                let ptr = Box::into_raw(state);
                let hwnd = CreateWindowExW(
                    WS_EX_TOOLWINDOW
                        | WS_EX_TOPMOST
                        | WS_EX_NOACTIVATE
                        | WS_EX_LAYERED
                        | WS_EX_TRANSPARENT,
                    class.as_ptr(),
                    wide(label).as_ptr(),
                    WS_POPUP,
                    x,
                    y,
                    w,
                    h,
                    std::ptr::null_mut(),
                    std::ptr::null_mut(),
                    instance,
                    ptr.cast(),
                );
                if hwnd.is_null() {
                    drop(Box::from_raw(ptr));
                    continue;
                }
                super::theme(hwnd, super::dark(s));
                // Layered windows need an explicit region: DWM rounding alone is
                // not reliable for transparent, click-through popup windows.
                let corner = DWMWCP_DONOTROUND;
                DwmSetWindowAttribute(
                    hwnd,
                    DWMWA_WINDOW_CORNER_PREFERENCE as u32,
                    (&corner as *const i32).cast(),
                    4,
                );
                if (*ptr).radius > 0 {
                    let region = CreateRoundRectRgn(
                        0,
                        0,
                        w + 1,
                        h + 1,
                        (*ptr).radius * 2,
                        (*ptr).radius * 2,
                    );
                    if SetWindowRgn(hwnd, region, 0) == 0 {
                        DeleteObject(region);
                    }
                }
                SetLayeredWindowAttributes(hwnd, 0, (*ptr).alpha, LWA_ALPHA);
                ShowWindow(hwnd, SW_SHOWNOACTIVATE);
                if animate {
                    SetTimer(hwnd, 1, 16, None);
                }
                if let Some(ms) = duration.filter(|ms| *ms > 0) {
                    SetTimer(hwnd, 2, ms, None);
                }
                self.windows.push(hwnd);
            }
        }
    }
}
impl Drop for Overlays {
    fn drop(&mut self) {
        self.clear();
    }
}
struct State {
    label: String,
    fg: u32,
    bg: u32,
    font: HFONT,
    alpha: u8,
    target: u8,
    border: bool,
    radius: i32,
    stroke: i32,
}
impl Drop for State {
    fn drop(&mut self) {
        unsafe {
            DeleteObject(self.font);
        }
    }
}
unsafe extern "system" fn monitor(m: HMONITOR, _: HDC, _: *mut RECT, data: isize) -> i32 {
    let mut info: MONITORINFO = std::mem::zeroed();
    info.cbSize = std::mem::size_of_val(&info) as u32;
    if GetMonitorInfoW(m, &mut info) != 0 {
        (*(data as *mut Vec<(RECT, u32)>)).push((info.rcWork, monitor_dpi(m)));
    }
    1
}
unsafe fn monitor_dpi(m: HMONITOR) -> u32 {
    let (mut x, mut y) = (96, 96);
    if GetDpiForMonitor(m, MDT_EFFECTIVE_DPI, &mut x, &mut y) < 0 {
        96
    } else {
        x.max(96)
    }
}
fn scale(value: i32, dpi: u32) -> i32 {
    ((value as i64 * dpi as i64 + 48) / 96) as i32
}
pub fn position(r: RECT, w: i32, h: i32, offset: i32, pos: &str) -> (i32, i32) {
    let x = if pos.ends_with("left") {
        r.left + offset
    } else if pos.ends_with("right") {
        r.right - w - offset
    } else {
        r.left + (r.right - r.left - w) / 2
    };
    let y = if pos.starts_with("top") {
        r.top + offset
    } else if pos.starts_with("bottom") {
        r.bottom - h - offset
    } else {
        r.top + (r.bottom - r.top - h) / 2
    };
    (x.clamp(r.left, r.right - w), y.clamp(r.top, r.bottom - h))
}
unsafe fn paint(dc: HDC, r: RECT, s: &State) {
    super::fill(dc, &r, s.bg);
    if s.border {
        let pen = CreatePen(PS_SOLID, s.stroke, s.fg);
        let old_pen = SelectObject(dc, pen);
        let old_brush = SelectObject(dc, GetStockObject(NULL_BRUSH));
        let inset = s.stroke / 2;
        if s.radius > 0 {
            RoundRect(
                dc,
                inset,
                inset,
                r.right - inset,
                r.bottom - inset,
                s.radius * 2,
                s.radius * 2,
            );
        } else {
            Rectangle(dc, inset, inset, r.right - inset, r.bottom - inset);
        }
        SelectObject(dc, old_brush);
        SelectObject(dc, old_pen);
        DeleteObject(pen);
    }
    super::text(
        dc,
        &s.label,
        r,
        s.fg,
        s.font,
        DT_CENTER | DT_VCENTER | DT_SINGLELINE | DT_END_ELLIPSIS,
    );
}

unsafe extern "system" fn proc(hwnd: HWND, msg: u32, w: usize, l: isize) -> isize {
    if msg == WM_NCCREATE {
        let cs = &*(l as *const CREATESTRUCTW);
        SetWindowLongPtrW(hwnd, GWLP_USERDATA, cs.lpCreateParams as _);
        return DefWindowProcW(hwnd, msg, w, l);
    }
    let ptr = GetWindowLongPtrW(hwnd, GWLP_USERDATA) as *mut State;
    if ptr.is_null() {
        return DefWindowProcW(hwnd, msg, w, l);
    }
    match msg {
        WM_NCHITTEST => HTTRANSPARENT as isize,
        WM_ERASEBKGND => 1,
        WM_PAINT => {
            let s = &*ptr;
            let mut ps = std::mem::zeroed();
            let dc = BeginPaint(hwnd, &mut ps);
            let mut r = std::mem::zeroed();
            GetClientRect(hwnd, &mut r);
            paint(dc, r, s);
            EndPaint(hwnd, &ps);
            0
        }
        WM_TIMER => {
            if w == 2 {
                KillTimer(hwnd, 2);
                ShowWindow(hwnd, SW_HIDE);
            } else {
                (*ptr).alpha = (*ptr).alpha.saturating_add(20).min((*ptr).target);
                let alpha = (*ptr).alpha;
                let target = (*ptr).target;
                SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA);
                if alpha == target {
                    KillTimer(hwnd, 1);
                }
            }
            0
        }
        WM_NCDESTROY => {
            SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
            drop(Box::from_raw(ptr));
            DefWindowProcW(hwnd, msg, w, l)
        }
        _ => DefWindowProcW(hwnd, msg, w, l),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn appearance_presets_offsets_and_preview_alpha_match_overlay_contract() {
        let mut settings = Settings {
            config: Default::default(),
            arguments: Default::default(),
            path: Default::default(),
        };
        settings.set("theme.overlay.width", "small");
        settings.set("theme.overlay.height", "small");
        settings.set("theme.overlay.offset", "24");
        settings.set("feature.showDesktopSwitchOverlay.position", "bottomright");
        let appearance = Appearance::new(&settings, false);
        let work = RECT {
            left: -1920,
            top: 0,
            right: 0,
            bottom: 1040,
        };
        let bounds = appearance.bounds(work, 144);
        assert_eq!(
            (bounds.left, bounds.top, bounds.right, bounds.bottom),
            (-516, 860, -36, 1004)
        );
        let pixels = preview_pixels(&appearance, "", 320, 96, 1.);
        assert_eq!(pixels[0] >> 24, 0);
        assert_eq!(pixels[48 * 320 + 160] >> 24, 153);
        settings.set("theme.overlay.roundedCorners", "false");
        settings.set("feature.showDesktopSwitchOverlay.translucent", "false");
        settings.set("theme.overlay.border", "true");
        let square = Appearance::new(&settings, false);
        let pixels = preview_pixels(&square, "", 320, 96, 1.);
        assert_eq!(pixels[0] >> 24, 255);
        assert_eq!(pixels[0] & 0xffffff, 0xffffff);
        assert_eq!(pixels[48 * 320 + 160] & 0xffffff, 0);
        // The permanent label resolves its own presets and styling independently.
        settings.set("theme.status.width", "large");
        settings.set("theme.status.fontSize", "large");
        let status = Appearance::new(&settings, true);
        assert_eq!(
            (status.width, status.height, status.font_size),
            (340, 40, 16.)
        );
        assert_eq!(status.opacity, 153);
    }
    #[test]
    fn positions_respect_monitor_origin_and_offset() {
        let r = RECT {
            left: -1920,
            top: -100,
            right: 0,
            bottom: 980,
        };
        assert_eq!(position(r, 250, 40, 10, "topleft"), (-1910, -90));
        assert_eq!(position(r, 250, 40, 10, "bottomright"), (-260, 930));
        assert_eq!(position(r, 250, 40, 0, "middlecenter"), (-1085, 420));
        assert_eq!(scale(100, 144), 150);
        assert_eq!(scale(40, 192), 80);
        assert_eq!(position(r, 250, 40, 5000, "bottomright"), (-1920, -100));
    }
}

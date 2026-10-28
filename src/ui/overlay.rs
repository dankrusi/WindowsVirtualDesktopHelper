use crate::{platform::wide, settings::Settings};
use windows_sys::Win32::{
    Foundation::*,
    Graphics::Gdi::*,
    System::LibraryLoader::GetModuleHandleW,
    UI::{HiDpi::*, WindowsAndMessaging::*},
};
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
            let section = if status {
                "theme.status"
            } else {
                "theme.overlay"
            };
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
            let theme = if super::dark(s) { "dark" } else { "light" };
            for (r, dpi) in monitors {
                let w = scale(s.int(&format!("{section}.width")), dpi).min(r.right - r.left);
                let h = scale(s.int(&format!("{section}.height")), dpi).min(r.bottom - r.top);
                let offset = if status {
                    scale(s.int("theme.status.offset"), dpi)
                } else {
                    0
                };
                let (x, y) = position(r, w, h, offset, s.get(&format!("{feature}.position")));
                let animate = s.bool(&format!("{feature}.animate"));
                let target = if s.bool(&format!("{feature}.translucent")) {
                    153
                } else {
                    255
                };
                let state = Box::new(State {
                    label: label.into(),
                    fg: super::color(s.get(&format!("{section}.overlayFG.{theme}")), 0xffffff),
                    bg: super::color(s.get(&format!("{section}.overlayBG.{theme}")), 0),
                    font: super::font(
                        s.get(&format!("{section}.font")),
                        (s.number(&format!("{section}.fontSize")) * dpi as f64 / 72.) as i32,
                    ),
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
            super::fill(dc, &r, s.bg);
            super::text(
                dc,
                &s.label,
                r,
                s.fg,
                s.font,
                DT_CENTER | DT_VCENTER | DT_SINGLELINE | DT_END_ELLIPSIS,
            );
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

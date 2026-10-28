use crate::{platform::desktop::Snapshot, platform::wide, settings::Settings};
use windows_sys::Win32::{
    Foundation::*,
    Graphics::Gdi::*,
    UI::{Shell::*, WindowsAndMessaging::*},
};
pub const MESSAGE: u32 = 0x8003;
pub struct Tray {
    hwnd: HWND,
    icons: Vec<(u32, HICON)>,
}
impl Tray {
    pub fn new(hwnd: HWND) -> Self {
        Self {
            hwnd,
            icons: Vec::new(),
        }
    }
    pub fn clear(&mut self) {
        for (id, icon) in self.icons.drain(..) {
            unsafe {
                let mut data: NOTIFYICONDATAW = std::mem::zeroed();
                data.cbSize = std::mem::size_of_val(&data) as u32;
                data.hWnd = self.hwnd;
                data.uID = id;
                Shell_NotifyIconW(NIM_DELETE, &data);
                DestroyIcon(icon);
            }
        }
    }
    pub fn update(&mut self, s: &Settings, snap: &Snapshot) {
        self.clear();
        let index = snap.current;
        let hide = s.bool("feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds");
        let mut icons = Vec::new();
        if s.bool("feature.showDesktopNumberInIconTray") {
            icons.push((1, (index + 1).to_string(), true));
        }
        if s.bool("feature.showDesktopNameInIconTray") {
            let name = snap.active().name.chars().next().unwrap_or('?').to_string();
            icons.push((2, name, true));
        }
        if s.bool("feature.showPrevNextIcons") {
            if !hide || index > 0 {
                icons.push((
                    3,
                    s.get("feature.showPrevNextIcons.prevChar").into(),
                    index > 0,
                ));
            }
            if !hide || index + 1 < snap.desktops.len() {
                icons.push((
                    4,
                    s.get("feature.showPrevNextIcons.nextChar").into(),
                    index + 1 < snap.desktops.len(),
                ));
            }
        }
        if icons.is_empty() {
            icons.push((5, "▦".into(), true));
        } // Keep Settings/Exit reachable.
        for (id, label, enabled) in icons {
            unsafe {
                let icon = icon(&label, s, enabled, id >= 3);
                let mut data: NOTIFYICONDATAW = std::mem::zeroed();
                data.cbSize = std::mem::size_of_val(&data) as u32;
                data.hWnd = self.hwnd;
                data.uID = id;
                data.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
                data.uCallbackMessage = MESSAGE;
                data.hIcon = icon;
                let tip = wide(&format!(
                    "{} · {} of {}",
                    snap.active().name,
                    index + 1,
                    snap.desktops.len()
                ));
                let mut tooltip = [0u16; 128];
                for (a, b) in tooltip.iter_mut().take(127).zip(tip) {
                    *a = b;
                }
                data.szTip = tooltip;
                if Shell_NotifyIconW(NIM_ADD, &data) == 0 {
                    crate::platform::log("Could not add notification icon");
                }
                self.icons.push((id, icon));
            }
        }
    }
}
impl Drop for Tray {
    fn drop(&mut self) {
        self.clear();
    }
}
unsafe fn icon(label: &str, s: &Settings, enabled: bool, symbol: bool) -> HICON {
    let size = GetSystemMetrics(SM_CXSMICON).max(16);
    let screen = GetDC(std::ptr::null_mut());
    let dc = CreateCompatibleDC(screen);
    let bitmap = CreateCompatibleBitmap(screen, size, size);
    let old = SelectObject(dc, bitmap);
    let theme = if super::dark(s) { "dark" } else { "light" };
    let bg = super::color(s.get(&format!("theme.icons.iconBG.{theme}")), 0);
    let mut fg = super::color(s.get(&format!("theme.icons.iconFG.{theme}")), 0xffffff);
    if !enabled {
        let opacity = s.number("theme.icons.disabledOpacity").clamp(0., 1.);
        let mut mixed = 0;
        for shift in [0, 8, 16] {
            let a = ((fg >> shift) & 255) as f64;
            let b = ((bg >> shift) & 255) as f64;
            mixed |= ((a * opacity + b * (1. - opacity)) as u32) << shift;
        }
        fg = mixed;
    }
    let r = RECT {
        left: 0,
        top: 0,
        right: size,
        bottom: size,
    };
    super::fill(dc, &r, bg);
    let f = super::font(
        s.get(if label.chars().any(|c| c as u32 > 0xffff) {
            "theme.icons.emojiFont"
        } else if symbol {
            "theme.icons.symbolsFont"
        } else {
            "theme.icons.font"
        }),
        size,
    );
    super::text(dc, label, r, fg, f, DT_CENTER | DT_VCENTER | DT_SINGLELINE);
    SelectObject(dc, old);
    let mask = CreateBitmap(size, size, 1, 1, std::ptr::null());
    let mask_dc = CreateCompatibleDC(screen);
    let old_mask = SelectObject(mask_dc, mask);
    PatBlt(mask_dc, 0, 0, size, size, BLACKNESS);
    SelectObject(mask_dc, old_mask);
    DeleteDC(mask_dc);
    let info = ICONINFO {
        fIcon: 1,
        xHotspot: 0,
        yHotspot: 0,
        hbmMask: mask,
        hbmColor: bitmap,
    };
    let icon = CreateIconIndirect(&info);
    DeleteObject(mask);
    DeleteObject(bitmap);
    DeleteObject(f);
    DeleteDC(dc);
    ReleaseDC(std::ptr::null_mut(), screen);
    icon
}

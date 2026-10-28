mod colors;
pub mod overlay;
pub mod settings;
pub mod tray;
use crate::platform::{registry, wide};
use crate::settings::Settings;
use windows_sys::Win32::{
    Foundation::*,
    Graphics::{Dwm::*, Gdi::*},
    UI::WindowsAndMessaging::*,
};
pub fn dark(s: &Settings) -> bool {
    match s.get("general.theme") {
        "dark" => true,
        "light" => false,
        _ => registry::dark(),
    }
}
pub fn colors(dark: bool) -> (u32, u32) {
    if dark {
        (0x202020, 0xf4f4f4)
    } else {
        (0xf8f8f8, 0x202020)
    }
}
pub unsafe fn theme(hwnd: HWND, dark: bool) {
    let mode = i32::from(dark);
    DwmSetWindowAttribute(
        hwnd,
        DWMWA_USE_IMMERSIVE_DARK_MODE as u32,
        (&mode as *const i32).cast(),
        4,
    );
    let corner = 2i32;
    DwmSetWindowAttribute(
        hwnd,
        DWMWA_WINDOW_CORNER_PREFERENCE as u32,
        (&corner as *const i32).cast(),
        4,
    );
}
pub fn color(s: &str, default: u32) -> u32 {
    let s = s.to_lowercase();
    if let Some((_, value)) = colors::COLORS.iter().find(|(name, _)| *name == s) {
        return *value;
    }
    let hex = s.strip_prefix('#').unwrap_or("").to_string();
    let hex = if hex.len() == 3 {
        hex.chars().flat_map(|c| [c, c]).collect()
    } else {
        hex
    };
    u32::from_str_radix(&hex, 16)
        .map(|rgb| ((rgb & 255) << 16) | (rgb & 0xff00) | ((rgb >> 16) & 255))
        .unwrap_or(default)
}
pub unsafe fn font(name: &str, size: i32) -> HFONT {
    let parts: Vec<_> = name.split(',').map(str::trim).collect();
    CreateFontW(
        -size,
        0,
        0,
        0,
        if parts.get(1).is_some_and(|s| s.eq_ignore_ascii_case("bold")) {
            700
        } else {
            400
        },
        u32::from(
            parts
                .get(1)
                .is_some_and(|s| s.eq_ignore_ascii_case("italic")),
        ),
        0,
        0,
        DEFAULT_CHARSET as u32,
        OUT_DEFAULT_PRECIS as u32,
        CLIP_DEFAULT_PRECIS as u32,
        CLEARTYPE_QUALITY as u32,
        DEFAULT_PITCH as u32,
        wide(parts[0]).as_ptr(),
    )
}
pub unsafe fn text(dc: HDC, s: &str, rect: RECT, color: u32, font: HFONT, flags: u32) {
    SetBkMode(dc, TRANSPARENT as i32);
    SetTextColor(dc, color);
    let old = SelectObject(dc, font);
    let mut rect = rect;
    let w = wide(s);
    DrawTextW(dc, w.as_ptr(), (w.len() - 1) as i32, &mut rect, flags);
    SelectObject(dc, old);
}
pub unsafe fn fill(dc: HDC, rect: &RECT, color: u32) {
    let b = CreateSolidBrush(color);
    FillRect(dc, rect, b);
    DeleteObject(b);
}
pub unsafe fn open_url(hwnd: HWND, url: &str) {
    windows_sys::Win32::UI::Shell::ShellExecuteW(
        hwnd,
        wide("open").as_ptr(),
        wide(url).as_ptr(),
        std::ptr::null(),
        std::ptr::null(),
        SW_SHOWNORMAL,
    );
}

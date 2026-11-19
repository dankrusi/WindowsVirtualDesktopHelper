mod colors;
mod config_editor;
pub mod overlay;
pub mod settings;
pub mod settings_process;
pub mod shortcut_capture;
pub mod tray;
mod tray_animation;
mod tray_preview;
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
unsafe fn create_font(name: &str, size: i32, quality: u8) -> HFONT {
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
        quality as u32,
        DEFAULT_PITCH as u32,
        wide(parts[0]).as_ptr(),
    )
}
/// GDI does not consistently link fonts while rasterizing into a DIB. Keep the
/// requested face when it covers the text, otherwise try installed Windows
/// script fonts. Checking glyph coverage also rejects unavailable font faces
/// that the font mapper silently substituted. No settings are rewritten.
pub unsafe fn font(name: &str, size: i32, label: &str, quality: u8) -> HFONT {
    let requested = create_font(name, size, quality);
    let dc = CreateCompatibleDC(std::ptr::null_mut());
    if dc.is_null() || requested.is_null() {
        if !dc.is_null() {
            DeleteDC(dc);
        }
        return requested;
    }
    // GetGlyphIndices operates on UTF-16 code units, not shaped supplementary
    // characters. Let Windows shape those rather than treating their surrogates
    // as missing glyphs. Combining marks are retained in the rendered label.
    let chars: Vec<_> = label
        .chars()
        .filter(|c| {
            !c.is_control()
                && *c as u32 <= 0xffff
                && !matches!(*c as u32, 0x200c..=0x200f | 0xfe00..=0xfe0f)
        })
        .map(|c| c as u16)
        .collect();
    let missing = |font: HFONT| {
        let previous = SelectObject(dc, font);
        let mut glyphs = vec![0; chars.len()];
        let result = GetGlyphIndicesW(
            dc,
            chars.as_ptr(),
            chars.len() as i32,
            glyphs.as_mut_ptr(),
            GGI_MARK_NONEXISTING_GLYPHS,
        );
        SelectObject(dc, previous);
        if result == GDI_ERROR as u32 {
            usize::MAX
        } else {
            glyphs.iter().filter(|g| **g == 0xffff).count()
        }
    };
    let mut best = requested;
    let mut missing_count = missing(best);
    if missing_count != 0 {
        let suffix = name.split_once(',').map_or("", |(_, style)| style);
        for fallback in [
            "Microsoft YaHei UI",
            "Microsoft JhengHei UI",
            "Yu Gothic UI",
            "Malgun Gothic",
            "Segoe UI",
            "Segoe UI Symbol",
        ] {
            let candidate = create_font(&format!("{fallback},{suffix}"), size, quality);
            if candidate.is_null() {
                continue;
            }
            let count = missing(candidate);
            if count < missing_count {
                DeleteObject(best);
                best = candidate;
                missing_count = count;
            } else {
                DeleteObject(candidate);
            }
            if missing_count == 0 {
                break;
            }
        }
    }
    DeleteDC(dc);
    best
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

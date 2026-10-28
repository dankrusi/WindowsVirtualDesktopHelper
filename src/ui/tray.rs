use crate::{platform::desktop::Snapshot, platform::wide, settings::Settings};
use windows_sys::core::GUID;
use windows_sys::Win32::{
    Foundation::*,
    Graphics::Gdi::*,
    UI::{HiDpi::*, Shell::*, WindowsAndMessaging::*},
};
pub const MESSAGE: u32 = 0x8003;
pub struct Tray {
    hwnd: HWND,
    icons: Vec<(u32, HICON)>,
    identity_scope: String,
}
impl Tray {
    pub fn new(hwnd: HWND) -> Self {
        Self {
            hwnd,
            icons: Vec::new(),
            identity_scope: std::env::current_exe()
                .expect("the running executable has a path")
                .to_string_lossy()
                .replace('/', "\\")
                .to_lowercase(),
        }
    }
    pub fn clear(&mut self) {
        for (id, icon) in self.icons.drain(..) {
            unsafe {
                let data = identity(self.hwnd, &self.identity_scope, id);
                Shell_NotifyIconW(NIM_DELETE, &data);
                DestroyIcon(icon);
            }
        }
    }
    pub fn update(&mut self, s: &Settings, snap: &Snapshot) {
        let mut previous = std::mem::take(&mut self.icons);
        let icons = icon_specs(s, snap);
        let metrics = metrics();
        for (id, label, enabled) in icons {
            unsafe {
                let icon = icon(&label, s, enabled, id >= 3, metrics);
                let mut data = identity(self.hwnd, &self.identity_scope, id);
                data.uFlags |= NIF_ICON | NIF_MESSAGE | NIF_TIP;
                data.uCallbackMessage = MESSAGE;
                data.hIcon = icon;
                data.szTip = tooltip(id, snap);
                let old = previous
                    .iter()
                    .position(|(old_id, _)| *old_id == id)
                    .map(|i| previous.remove(i).1);
                // Also replace an orphaned icon after an unclean shutdown.
                let updated = Shell_NotifyIconW(NIM_MODIFY, &data) != 0
                    || Shell_NotifyIconW(NIM_ADD, &data) != 0;
                if let Some(old) = old {
                    DestroyIcon(old);
                }
                if updated {
                    self.icons.push((id, icon));
                } else {
                    crate::platform::log(&format!(
                        "Could not add notification icon: {}",
                        icon_name(id)
                    ));
                    DestroyIcon(icon);
                }
            }
        }
        for (id, icon) in previous {
            unsafe {
                let data = identity(self.hwnd, &self.identity_scope, id);
                Shell_NotifyIconW(NIM_DELETE, &data);
                DestroyIcon(icon);
            }
        }
    }
    pub fn notify(&self, title: &str, message: &str) -> bool {
        let Some((id, _)) = self.icons.first() else {
            return false;
        };
        let mut data = identity(self.hwnd, &self.identity_scope, *id);
        data.uFlags |= NIF_INFO;
        data.szInfoTitle = shell_text(title);
        data.szInfo = shell_text(message);
        data.dwInfoFlags = NIIF_INFO;
        unsafe { Shell_NotifyIconW(NIM_MODIFY, &data) != 0 }
    }
}

/// Stable UUIDs per executable location and icon role. Windows binds a tray
/// GUID to its binary path; scoping avoids collisions between portable copies,
/// development builds and installed packages. Never change the namespace.
fn icon_guid(scope: &str, id: u32) -> GUID {
    const NAMESPACE: uuid::Uuid = uuid::Uuid::from_u128(0x8333b54dad424e31ba7c2145ab2df461);
    GUID::from_u128(uuid::Uuid::new_v5(&NAMESPACE, format!("{scope}\0{id}").as_bytes()).as_u128())
}
fn identity(hwnd: HWND, scope: &str, id: u32) -> NOTIFYICONDATAW {
    NOTIFYICONDATAW {
        cbSize: std::mem::size_of::<NOTIFYICONDATAW>() as u32,
        hWnd: hwnd,
        uID: id,
        uFlags: NIF_GUID,
        guidItem: icon_guid(scope, id),
        ..unsafe { std::mem::zeroed() }
    }
}
fn icon_name(id: u32) -> &'static str {
    match id {
        1 => "Desktop number",
        2 => "Desktop initial",
        3 => "Previous desktop",
        4 => "Next desktop",
        _ => "Desktop controls",
    }
}
fn tooltip(id: u32, snap: &Snapshot) -> [u16; 128] {
    shell_text(&format!(
        "{} · Virtual Desktop Helper\n{} · {} of {}",
        icon_name(id),
        snap.active().name,
        snap.current + 1,
        snap.desktops.len()
    ))
}
/// NUL-terminate Shell strings without cutting a UTF-16 surrogate pair.
fn shell_text<const N: usize>(text: &str) -> [u16; N] {
    let mut buffer = [0; N];
    let mut offset = 0;
    for ch in text.chars().take_while(|ch| *ch != '\0') {
        let mut units = [0; 2];
        let units = ch.encode_utf16(&mut units);
        if offset + units.len() >= N {
            break;
        }
        buffer[offset..offset + units.len()].copy_from_slice(units);
        offset += units.len();
    }
    buffer
}
impl Drop for Tray {
    fn drop(&mut self) {
        self.clear();
    }
}
/// Shared icon selection for the actual notification area and Settings preview.
pub fn icon_specs(s: &Settings, snap: &Snapshot) -> Vec<(u32, String, bool)> {
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
    icons
}
#[derive(Clone, Copy)]
pub struct Metrics {
    pub dpi: u32,
    pub size: i32,
    pub dark: bool,
}
pub fn metrics() -> Metrics {
    unsafe {
        let taskbar = FindWindowW(wide("Shell_TrayWnd").as_ptr(), std::ptr::null());
        let dpi = if taskbar.is_null() {
            96
        } else {
            GetDpiForWindow(taskbar).max(96)
        };
        Metrics {
            dpi,
            size: GetSystemMetricsForDpi(SM_CXSMICON, dpi).max(16),
            dark: crate::platform::registry::dark(),
        }
    }
}
/// Shared premultiplied BGRA artwork. Settings displays these exact icon pixels.
pub fn pixels(
    label: &str,
    s: &Settings,
    enabled: bool,
    symbol: bool,
    metrics: Metrics,
) -> Vec<u32> {
    let size = metrics.size;
    let theme = if metrics.dark { "dark" } else { "light" };
    let fg = super::color(s.get(&format!("theme.icons.iconFG.{theme}")), 0xffffff);
    let bg = if s.bool("theme.icons.transparentBackground") {
        None
    } else {
        Some(super::color(
            s.get(&format!("theme.icons.iconBG.{theme}")),
            0,
        ))
    };
    let font = s.get(if label.chars().any(|c| c as u32 > 0xffff) {
        "theme.icons.emojiFont"
    } else if symbol {
        "theme.icons.symbolsFont"
    } else {
        "theme.icons.font"
    });
    let opacity = if enabled {
        1.
    } else {
        s.number("theme.icons.disabledOpacity").clamp(0., 1.)
    };
    rasterize(label, size, font, fg, bg, opacity)
}
/// HICON and the preview share selection, DPI, colors, opacity and glyph layout.
unsafe fn icon(label: &str, s: &Settings, enabled: bool, symbol: bool, metrics: Metrics) -> HICON {
    let size = metrics.size;
    let pixels = pixels(label, s, enabled, symbol, metrics);
    let mut bits = std::ptr::null_mut();
    let info = bitmap_info(size);
    let bitmap = CreateDIBSection(
        std::ptr::null_mut(),
        &info,
        DIB_RGB_COLORS,
        &mut bits,
        std::ptr::null_mut(),
        0,
    );
    if bitmap.is_null() {
        return std::ptr::null_mut();
    }
    std::ptr::copy_nonoverlapping(pixels.as_ptr(), bits.cast::<u32>(), pixels.len());
    // A zero AND mask is correct when the 32-bit color bitmap carries alpha.
    let mask_data = vec![0u8; (size as usize).div_ceil(16) * 2 * size as usize];
    let mask = CreateBitmap(size, size, 1, 1, mask_data.as_ptr().cast());
    let icon = CreateIconIndirect(&ICONINFO {
        fIcon: 1,
        xHotspot: 0,
        yHotspot: 0,
        hbmMask: mask,
        hbmColor: bitmap,
    });
    DeleteObject(mask);
    DeleteObject(bitmap);
    icon
}
fn bitmap_info(size: i32) -> BITMAPINFO {
    BITMAPINFO {
        bmiHeader: BITMAPINFOHEADER {
            biSize: std::mem::size_of::<BITMAPINFOHEADER>() as u32,
            biWidth: size,
            biHeight: -size,
            biPlanes: 1,
            biBitCount: 32,
            biCompression: BI_RGB,
            ..unsafe { std::mem::zeroed() }
        },
        bmiColors: [RGBQUAD {
            rgbBlue: 0,
            rgbGreen: 0,
            rgbRed: 0,
            rgbReserved: 0,
        }],
    }
}
/// Render a grayscale coverage mask, fit its visible ink into the icon,
/// then downsample. ClearType RGB fringes and GDI's undefined alpha never enter HICON.
fn rasterize(
    label: &str,
    size: i32,
    font_name: &str,
    fg: u32,
    bg: Option<u32>,
    opacity: f64,
) -> Vec<u32> {
    unsafe {
        // Draw generously first: font em size is not the visible glyph size.
        // Small chevrons and initials must fit by their ink bounds, like digits.
        let high = size * 4 * 3;
        let info = bitmap_info(high);
        let mut bits = std::ptr::null_mut();
        let bitmap = CreateDIBSection(
            std::ptr::null_mut(),
            &info,
            DIB_RGB_COLORS,
            &mut bits,
            std::ptr::null_mut(),
            0,
        );
        let dc = CreateCompatibleDC(std::ptr::null_mut());
        if bitmap.is_null() || dc.is_null() {
            if !bitmap.is_null() {
                DeleteObject(bitmap);
            }
            if !dc.is_null() {
                DeleteDC(dc);
            }
            return vec![0; (size * size) as usize];
        }
        let old = SelectObject(dc, bitmap);
        std::ptr::write_bytes(bits, 0, (high * high * 4) as usize);
        let name = font_name.split(',').next().unwrap_or("Segoe UI");
        let mut height = (high as f64 * 0.94) as i32;
        let label = wide(label);
        let mut font;
        loop {
            font = CreateFontW(
                -height,
                0,
                0,
                0,
                if font_name.to_lowercase().contains("bold") {
                    700
                } else {
                    400
                },
                u32::from(font_name.to_lowercase().contains("italic")),
                0,
                0,
                DEFAULT_CHARSET as u32,
                OUT_DEFAULT_PRECIS as u32,
                CLIP_DEFAULT_PRECIS as u32,
                ANTIALIASED_QUALITY as u32,
                DEFAULT_PITCH as u32,
                wide(name).as_ptr(),
            );
            let previous = SelectObject(dc, font);
            let mut extent = SIZE { cx: 0, cy: 0 };
            GetTextExtentPoint32W(dc, label.as_ptr(), (label.len() - 1) as i32, &mut extent);
            SelectObject(dc, previous);
            if extent.cx <= high - 8 || height <= 8 {
                break;
            }
            DeleteObject(font);
            height -= 2;
        }
        let previous_font = SelectObject(dc, font);
        SetBkMode(dc, TRANSPARENT as i32);
        SetTextColor(dc, 0xffffff);
        let mut rect = RECT {
            left: 0,
            top: 0,
            right: high,
            bottom: high,
        };
        DrawTextW(
            dc,
            label.as_ptr(),
            (label.len() - 1) as i32,
            &mut rect,
            DT_CENTER | DT_VCENTER | DT_SINGLELINE | DT_NOPREFIX,
        );
        GdiFlush();
        let source = std::slice::from_raw_parts(bits.cast::<u32>(), (high * high) as usize);
        let (mut left, mut top, mut right, mut bottom) = (high, high, 0, 0);
        for y in 0..high {
            for x in 0..high {
                if source[(y * high + x) as usize] & 255 > 2 {
                    left = left.min(x);
                    right = right.max(x + 1);
                    top = top.min(y);
                    bottom = bottom.max(y + 1);
                }
            }
        }
        let target = size * 4;
        let padding = ((size as f64 * 0.08).round() as i32).max(1) * 4;
        let available = target - 2 * padding;
        let ink_width = (right - left).max(1);
        let ink_height = (bottom - top).max(1);
        let scale = available as f64 / ink_width.max(ink_height) as f64;
        let width = ((ink_width as f64 * scale).round() as i32).max(1);
        let height = ((ink_height as f64 * scale).round() as i32).max(1);
        let dx = (target - width) / 2;
        let dy = (target - height) / 2;
        let mut result = vec![0; (size * size) as usize];
        for y in 0..size {
            for x in 0..size {
                let mut sum = 0;
                for sy in 0..4 {
                    for sx in 0..4 {
                        let xx = x * 4 + sx - dx;
                        let yy = y * 4 + sy - dy;
                        if left < right
                            && top < bottom
                            && xx >= 0
                            && xx < width
                            && yy >= 0
                            && yy < height
                        {
                            let source_x =
                                left + ((xx as f64 + 0.5) * ink_width as f64 / width as f64) as i32;
                            let source_y = top
                                + ((yy as f64 + 0.5) * ink_height as f64 / height as f64) as i32;
                            sum += source[(source_y * high + source_x) as usize] & 255;
                        }
                    }
                }
                let coverage = ((sum as f64 / 16.) * opacity).round() as u32;
                let alpha = if bg.is_some() { 255 } else { coverage };
                let mut pixel = alpha << 24;
                for (shift, source_shift) in [(0, 16), (8, 8), (16, 0)] {
                    let value = ((fg >> source_shift) & 255) * coverage;
                    let value = value
                        + bg.map(|b| ((b >> source_shift) & 255) * (255 - coverage))
                            .unwrap_or(0);
                    pixel |= ((value + 127) / 255) << shift;
                }
                result[(y * size + x) as usize] = pixel;
            }
        }
        SelectObject(dc, previous_font);
        SelectObject(dc, old);
        DeleteObject(font);
        DeleteObject(bitmap);
        DeleteDC(dc);
        result
    }
}
#[cfg(test)]
mod tests {
    use super::*;
    fn guid_value(guid: GUID) -> u128 {
        ((guid.data1 as u128) << 96)
            | ((guid.data2 as u128) << 80)
            | ((guid.data3 as u128) << 64)
            | u64::from_be_bytes(guid.data4) as u128
    }
    #[test]
    fn tray_identity_survives_window_changes_and_is_unique_per_role_and_install() {
        let scope = r"c:\tools\helper.exe";
        // Freeze this UUID: changing it would discard Explorer's saved preferences.
        assert_eq!(
            guid_value(icon_guid(scope, 1)),
            0x991ccb07d8d55709acb09ba112a80ec5
        );
        let first = identity(1usize as HWND, scope, 1);
        let restart = identity(2usize as HWND, scope, 1);
        let first_guid = first.guidItem;
        let restarted_guid = restart.guidItem;
        assert_eq!(guid_value(first_guid), guid_value(restarted_guid));
        let first_window = first.hWnd;
        let restarted_window = restart.hWnd;
        assert_ne!(first_window, restarted_window);
        assert_eq!(first.uFlags & NIF_GUID, NIF_GUID);
        for id in 1..=5 {
            for other in 1..=5 {
                assert_eq!(
                    guid_value(icon_guid(scope, id)) == guid_value(icon_guid(scope, other)),
                    id == other
                );
            }
            assert_ne!(
                guid_value(icon_guid(scope, id)),
                guid_value(icon_guid(r"c:\other\helper.exe", id))
            );
        }
    }
    #[test]
    fn tooltip_names_stay_unique_with_long_unicode_desktop_names() {
        let snap = Snapshot {
            current: 0,
            desktops: vec![crate::platform::desktop::Desktop {
                id: 1,
                name: "\u{1f4bb}".repeat(200),
            }],
        };
        let tips: Vec<_> = (1..=5).map(|id| tooltip(id, &snap)).collect();
        for (index, tip) in tips.iter().enumerate() {
            let end = tip.iter().position(|ch| *ch == 0).unwrap();
            assert!(end <= 127);
            let text = String::from_utf16(&tip[..end]).expect("no split surrogate pair");
            assert!(text.starts_with(icon_name(index as u32 + 1)));
            assert_eq!(tips.iter().filter(|other| *other == tip).count(), 1);
        }
        assert_eq!(shell_text::<4>("ab\u{1f4bb}"), [97, 98, 0, 0]);
    }
    #[test]
    fn arrows_and_digits_fill_the_icon_by_visible_bounds() {
        for size in [16, 20, 24, 32, 40, 48] {
            for text in ["1", "W", "10", "99", "\u{2039}", "\u{203a}"] {
                let p = rasterize(text, size, "Segoe UI Symbol", 0xffffff, None, 1.);
                let (mut left, mut top, mut right, mut bottom) = (size, size, 0, 0);
                for y in 0..size {
                    for x in 0..size {
                        if p[(y * size + x) as usize] >> 24 > 32 {
                            left = left.min(x);
                            right = right.max(x + 1);
                            top = top.min(y);
                            bottom = bottom.max(y + 1);
                        }
                    }
                }
                assert!(
                    left >= 1 && top >= 1 && right < size && bottom < size,
                    "clipped {text} at {size}"
                );
                assert!(
                    (right - left).max(bottom - top) >= size * 3 / 4,
                    "undersized {text} at {size}"
                );
            }
        }
    }
    #[test]
    fn shared_selection_tracks_desktop_options_and_boundaries() {
        use crate::platform::desktop::Desktop;
        let mut s = Settings {
            config: Default::default(),
            arguments: Default::default(),
            path: Default::default(),
        };
        let mut snap = Snapshot {
            current: 0,
            desktops: vec![
                Desktop {
                    id: 1,
                    name: "Work".into(),
                },
                Desktop {
                    id: 2,
                    name: "Home".into(),
                },
            ],
        };
        s.set("feature.showDesktopNameInIconTray", "true");
        s.set("feature.showPrevNextIcons.prevChar", "<");
        assert_eq!(
            icon_specs(&s, &snap),
            vec![
                (1, "1".into(), true),
                (2, "W".into(), true),
                (3, "<".into(), false),
                (4, "\u{203a}".into(), true)
            ]
        );
        s.set(
            "feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds",
            "true",
        );
        assert!(!icon_specs(&s, &snap).iter().any(|(id, _, _)| *id == 3));
        snap.current = 1;
        let icons = icon_specs(&s, &snap);
        assert_eq!(icons[0].1, "2");
        assert_eq!(icons[1].1, "H");
        assert!(!icons.iter().any(|(id, _, _)| *id == 4));
        for key in [
            "feature.showDesktopNameInIconTray",
            "feature.showDesktopNumberInIconTray",
            "feature.showPrevNextIcons",
        ] {
            s.set(key, "false");
        }
        assert_eq!(icon_specs(&s, &snap), vec![(5, "\u{25a6}".into(), true)]);
    }
    #[test]
    fn transparent_icons_have_coverage_and_clear_corners_at_each_dpi() {
        for size in [16, 20, 24, 32, 40, 48] {
            for text in ["1", "10", "99", ">", "<"] {
                let pixels = rasterize(text, size, "Segoe UI", 0xffffff, None, 1.);
                assert_eq!(pixels.len(), (size * size) as usize);
                assert_eq!(pixels[0], 0);
                assert_eq!(pixels[pixels.len() - 1], 0);
                assert!(pixels.iter().any(|p| p >> 24 > 100));
                for p in pixels {
                    let alpha = p >> 24;
                    assert!(p & 255 <= alpha);
                    assert!((p >> 8) & 255 <= alpha);
                    assert!((p >> 16) & 255 <= alpha);
                }
            }
        }
    }
    #[test]
    fn disabled_opacity_changes_alpha() {
        let p = rasterize("2", 24, "Segoe UI", 0x00ff00, None, 0.5);
        assert!(p.iter().all(|p| p >> 24 <= 128));
        assert!(p.iter().any(|p| p >> 24 > 50));
    }
}

use crate::{platform::desktop::Snapshot, platform::wide, settings::Settings};
use unicode_segmentation::UnicodeSegmentation;
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
    pub fn update(&mut self, s: &Settings, snap: &Snapshot, connected: bool) -> bool {
        self.update_with(s, snap, connected, |operation, data| unsafe {
            let success = Shell_NotifyIconW(operation, data) != 0;
            if operation == NIM_ADD && !success {
                crate::platform::log(&format!(
                    "Could not add notification icon: {}",
                    icon_name(data.uID)
                ));
            }
            success
        })
    }
    fn update_with(
        &mut self,
        s: &Settings,
        snap: &Snapshot,
        connected: bool,
        mut notify: impl FnMut(NOTIFY_ICON_MESSAGE, &NOTIFYICONDATAW) -> bool,
    ) -> bool {
        let mut previous = std::mem::take(&mut self.icons);
        let icons = recovery_icon_specs(s, snap, connected);
        let metrics = metrics();
        let mut complete = true;
        for (id, label, enabled) in icons {
            unsafe {
                let icon = icon(&label, s, enabled, id >= 3, metrics);
                let mut data = identity(self.hwnd, &self.identity_scope, id);
                data.uFlags |= NIF_ICON | NIF_MESSAGE | NIF_TIP;
                data.uCallbackMessage = MESSAGE;
                data.hIcon = icon;
                data.szTip = if connected {
                    tooltip(id, snap)
                } else {
                    shell_text(&crate::i18n::format(
                        "tray-reconnecting",
                        &[("role", (icon_name(id)).to_string().into())],
                    ))
                };
                let old = previous
                    .iter()
                    .position(|(old_id, _)| *old_id == id)
                    .map(|i| previous.remove(i).1);
                // Also replace an orphaned icon after an unclean shutdown.
                if icon.is_null() {
                    complete = false;
                    if let Some(old) = old {
                        self.icons.push((id, old));
                    }
                    continue;
                }
                let updated = notify(NIM_MODIFY, &data) || notify(NIM_ADD, &data);
                if let Some(old) = old {
                    DestroyIcon(old);
                }
                if updated {
                    self.icons.push((id, icon));
                } else {
                    complete = false;
                    DestroyIcon(icon);
                }
            }
        }
        for (id, icon) in previous {
            unsafe {
                let data = identity(self.hwnd, &self.identity_scope, id);
                notify(NIM_DELETE, &data);
                DestroyIcon(icon);
            }
        }
        complete
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
        1 => crate::i18n::text("desktop-number"),
        2 => crate::i18n::text("desktop-initial"),
        3 => crate::i18n::text("previous-desktop"),
        4 => crate::i18n::text("next-desktop"),
        _ => crate::i18n::text("desktop-controls"),
    }
}
fn tooltip(id: u32, snap: &Snapshot) -> [u16; 128] {
    shell_text(&crate::i18n::format(
        "tray-tooltip",
        &[
            ("role", (icon_name(id)).to_string().into()),
            ("name", (snap.active().name).to_string().into()),
            ("number", (snap.current + 1).into()),
            ("total", (snap.desktops.len()).into()),
        ],
    ))
}
/// NUL-terminate Shell strings without cutting a visible Unicode character.
fn shell_text<const N: usize>(text: &str) -> [u16; N] {
    let mut buffer = [0; N];
    let mut offset = 0;
    for cluster in text.split('\0').next().unwrap_or("").graphemes(true) {
        let length = cluster.encode_utf16().count();
        if offset + length >= N {
            break;
        }
        for unit in cluster.encode_utf16() {
            buffer[offset] = unit;
            offset += 1;
        }
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
        let name = snap
            .active()
            .name
            .graphemes(true)
            .next()
            .unwrap_or("?")
            .to_string();
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
fn recovery_icon_specs(s: &Settings, snap: &Snapshot, connected: bool) -> Vec<(u32, String, bool)> {
    let mut icons = icon_specs(s, snap);
    if !connected {
        for (id, label, enabled) in &mut icons {
            if matches!(*id, 1 | 2) {
                *label = "?".into();
            }
            *enabled = *id == 5;
        }
    }
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
        let taskbar_dpi = GetDpiForWindow(taskbar);
        // Explorer's taskbar may be temporarily absent during a display change.
        let dpi = if taskbar_dpi == 0 {
            GetDpiForSystem().max(USER_DEFAULT_SCREEN_DPI)
        } else {
            taskbar_dpi
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
    let font = s.get(
        if label.chars().any(|c| matches!(c as u32, 0x1f000..=0x1faff)) {
            "theme.icons.emojiFont"
        } else if symbol {
            "theme.icons.symbolsFont"
        } else {
            "theme.icons.font"
        },
    );
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
        let text = label;
        let mut height = (high as f64 * 0.94) as i32;
        let label = wide(label);
        let mut font;
        loop {
            font = super::font(font_name, height, text, ANTIALIASED_QUALITY);
            let previous = SelectObject(dc, font);
            let mut extent = RECT {
                left: 0,
                top: 0,
                right: 0,
                bottom: 0,
            };
            DrawTextW(
                dc,
                label.as_ptr(),
                (label.len() - 1) as i32,
                &mut extent,
                DT_CALCRECT | DT_SINGLELINE | DT_NOPREFIX,
            );
            SelectObject(dc, previous);
            if extent.right <= high - 8 || height <= 8 {
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
        // Map each physical icon pixel directly to the high-resolution mask.
        // Preserve the aspect ratio and snap the outer ink origin to the pixel grid.
        let padding = (f64::from(size) * 0.08).round().max(1.);
        let ink_width = f64::from((right - left).max(0));
        let ink_height = f64::from((bottom - top).max(0));
        let source_step = ink_width.max(ink_height).max(1.) / (f64::from(size) - 2. * padding);
        let dx = ((f64::from(size) - ink_width / source_step) / 2.).round();
        let dy = ((f64::from(size) - ink_height / source_step) / 2.).round();
        let source_area = source_step * source_step;
        let mut result = vec![0; (size * size) as usize];
        for y in 0..size {
            for x in 0..size {
                let source_x = f64::from(left) + (f64::from(x) - dx) * source_step;
                let source_y = f64::from(top) + (f64::from(y) - dy) * source_step;
                let covered = covered_area(
                    source,
                    high,
                    source_x.max(f64::from(left))..(source_x + source_step).min(f64::from(right)),
                    source_y.max(f64::from(top))..(source_y + source_step).min(f64::from(bottom)),
                );
                let coverage = (covered / source_area * opacity).round() as u32;
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

/// Integrate grayscale coverage, weighting each source pixel by its overlap.
fn covered_area(
    source: &[u32],
    stride: i32,
    x: std::ops::Range<f64>,
    y: std::ops::Range<f64>,
) -> f64 {
    if x.is_empty() || y.is_empty() {
        return 0.;
    }
    let mut area = 0.;
    for row in y.start.floor() as i32..y.end.ceil() as i32 {
        let height = y.end.min(f64::from(row + 1)) - y.start.max(f64::from(row));
        for col in x.start.floor() as i32..x.end.ceil() as i32 {
            let width = x.end.min(f64::from(col + 1)) - x.start.max(f64::from(col));
            area += f64::from(source[(row * stride + col) as usize] & 255) * width * height;
        }
    }
    area
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn shell_truncation_preserves_whole_graphemes() {
        assert_eq!(shell_text::<3>("e\u{301}x"), [0x65, 0x301, 0]);
        assert_eq!(shell_text::<2>("e\u{301}x"), [0, 0]);
        assert_eq!(shell_text::<5>("A👩‍💻B"), [0x41, 0, 0, 0, 0]);
        assert_eq!(shell_text::<4>("工\0学"), [0x5de5, 0, 0, 0]);
    }
    #[test]
    fn initials_keep_combining_marks_cjk_and_emoji_sequences() {
        let mut settings = Settings {
            config: Default::default(),
            arguments: Default::default(),
            path: Default::default(),
        };
        settings.set("feature.showDesktopNameInIconTray", "true");
        for (name, expected) in [
            ("工作", "工"),
            ("e\u{301}quipe", "e\u{301}"),
            ("👩‍💻 Work", "👩‍💻"),
            ("𠀀桌面", "𠀀"),
        ] {
            let snapshot = Snapshot {
                current: 0,
                desktops: vec![crate::platform::desktop::Desktop {
                    id: 1,
                    name: name.into(),
                }],
            };
            let initial = icon_specs(&settings, &snapshot)
                .into_iter()
                .find(|(id, _, _)| *id == 2)
                .unwrap()
                .1;
            assert_eq!(initial, expected);
        }
    }
    #[test]
    fn cjk_rasterization_has_distinct_glyphs_at_taskbar_dpis() {
        for size in [16, 24, 32, 48] {
            let work = rasterize("工", size, "Segoe UI", 0xffffff, None, 1.);
            let study = rasterize("学", size, "Segoe UI", 0xffffff, None, 1.);
            let traditional = rasterize("學", size, "Segoe UI", 0xffffff, None, 1.);
            assert!(work.iter().any(|p| p >> 24 != 0));
            assert_ne!(
                work, study,
                "different characters must not produce the same missing-glyph box at {size}"
            );
            assert_ne!(study, traditional);
        }
    }
    #[test]
    fn tray_restores_partial_failures_and_shell_restarts_without_a_desktop_connection() {
        use std::collections::HashSet;
        let mut settings = Settings {
            config: Default::default(),
            arguments: Default::default(),
            path: Default::default(),
        };
        settings.set("feature.showDesktopNameInIconTray", "true");
        let snap = Snapshot {
            current: 0,
            desktops: vec![crate::platform::desktop::Desktop {
                id: 1,
                name: "Work".into(),
            }],
        };
        let mut tray = Tray::new(std::ptr::null_mut());
        let mut registered = HashSet::new();
        let mut operations = Vec::new();
        let mut fail_initial = true;
        // Inject shell outcomes; no icons are registered with the user's Explorer.
        let mut notify = |operation, data: &NOTIFYICONDATAW| {
            let id = data.uID;
            operations.push((operation, id));
            match operation {
                NIM_MODIFY => registered.contains(&id),
                NIM_ADD if id == 2 && fail_initial => {
                    fail_initial = false;
                    false
                }
                NIM_ADD => {
                    registered.insert(id);
                    true
                }
                NIM_DELETE => {
                    registered.remove(&id);
                    true
                }
                _ => unreachable!(),
            }
        };
        assert!(!tray.update_with(&settings, &snap, false, &mut notify));
        assert!(tray.update_with(&settings, &snap, false, &mut notify));
        assert_eq!(registered, HashSet::from([1, 2, 3, 4]));
        assert_eq!(
            operations.iter().filter(|(op, _)| *op == NIM_ADD).count(),
            5
        );
        registered.clear(); // Explorer lost every icon while COM remains down.
        let mut restored_tips = Vec::new();
        assert!(
            tray.update_with(&settings, &snap, false, |operation, data| {
                let id = data.uID;
                if operation == NIM_MODIFY {
                    return registered.contains(&id);
                }
                assert_eq!(operation, NIM_ADD);
                registered.insert(id);
                let tip = data.szTip;
                restored_tips.push(String::from_utf16_lossy(
                    &tip[..tip.iter().position(|ch| *ch == 0).unwrap()],
                ));
                true
            })
        );
        assert_eq!(registered.len(), 4);
        assert!(restored_tips
            .iter()
            .all(|tip| tip.contains("Reconnecting to Windows")));
        assert_eq!(restored_tips.iter().collect::<HashSet<_>>().len(), 4);
        let offline = recovery_icon_specs(&settings, &snap, false);
        assert_eq!(offline[0], (1, "?".into(), false));
        assert_eq!(offline[1], (2, "?".into(), false));
        assert!(offline.iter().all(|(_, _, enabled)| !enabled));
        let ids = tray.icons.iter().map(|(id, _)| *id).collect::<Vec<_>>();
        assert!(tray.update_with(&settings, &snap, true, |operation, data| {
            assert_eq!(operation, NIM_MODIFY);
            let id = data.uID;
            registered.contains(&id)
        }));
        assert_eq!(
            tray.icons.iter().map(|(id, _)| *id).collect::<Vec<_>>(),
            ids
        );
        // These HICONs were never sent to the shell; release them locally.
        for (_, icon) in tray.icons.drain(..) {
            unsafe {
                DestroyIcon(icon);
            }
        }
    }
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
    fn coverage_preserves_subpixel_diagonal_edges() {
        let diagonal = [255, 0, 0, 255];
        assert_eq!(covered_area(&diagonal, 2, 0.25..1.25, 0.25..1.25), 159.375);
        assert_eq!(covered_area(&diagonal, 2, 0.75..1.75, 0.75..1.75), 159.375);
        assert_eq!(covered_area(&diagonal, 2, 0.5..1.5, 0.5..1.5), 127.5);
    }
    #[test]
    fn disabled_opacity_changes_alpha() {
        let p = rasterize("2", 24, "Segoe UI", 0x00ff00, None, 0.5);
        assert!(p.iter().all(|p| p >> 24 <= 128));
        assert!(p.iter().any(|p| p >> 24 > 50));
    }
}

//! Reusable native setting rows. Each row binds a key to a toggle, choice or
//! text editor; adding settings does not require another window procedure.
use crate::{
    platform::wide,
    settings::{Settings, DEFAULTS},
};
use std::cell::RefCell;
use windows_sys::Win32::UI::Input::KeyboardAndMouse::EnableWindow;
use windows_sys::Win32::{
    Foundation::*,
    Graphics::Gdi::*,
    System::LibraryLoader::GetModuleHandleW,
    UI::{Controls::*, HiDpi::*, WindowsAndMessaging::*},
};
pub const APPLY: u32 = 0x8004;
const PAGES: [&str; 5] = ["General", "Overlays", "Hotkeys", "Appearance", "Advanced"];
struct Row {
    key: &'static str,
    label: &'static str,
    choices: &'static [&'static str],
}
const POS: &[&str] = &[
    "topleft",
    "topcenter",
    "topright",
    "middleleft",
    "middlecenter",
    "middleright",
    "bottomleft",
    "bottomcenter",
    "bottomright",
];
const GENERAL: &[Row] = &[
    Row {
        key: "general.startupWithWindows",
        label: "Start with Windows",
        choices: &[],
    },
    Row {
        key: "general.theme",
        label: "App theme",
        choices: &["auto", "light", "dark"],
    },
    Row {
        key: "feature.showDesktopNumberInIconTray",
        label: "Desktop number in tray",
        choices: &[],
    },
    Row {
        key: "feature.showDesktopNameInIconTray",
        label: "Desktop initial in tray",
        choices: &[],
    },
    Row {
        key: "feature.showPrevNextIcons",
        label: "Previous / next tray buttons",
        choices: &[],
    },
    Row {
        key: "feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds",
        label: "Hide arrows at first / last desktop",
        choices: &[],
    },
    Row {
        key: "feature.showDesktopNumberInIconTray.clickToOpenTaskView",
        label: "Click number to open Task View",
        choices: &[],
    },
    Row {
        key: "feature.restorePreviousWindowFocus",
        label: "Restore last focused window",
        choices: &[],
    },
    Row {
        key: "feature.showSplashScreen",
        label: "Show welcome overlay",
        choices: &[],
    },
];
const OVERLAYS: &[Row] = &[
    Row {
        key: "feature.showDesktopSwitchOverlay",
        label: "Show name when switching",
        choices: &[],
    },
    Row {
        key: "feature.showDesktopSwitchOverlay.duration",
        label: "Display time (ms; 0 = permanent)",
        choices: &[],
    },
    Row {
        key: "feature.showDesktopSwitchOverlay.position",
        label: "Switch overlay position",
        choices: POS,
    },
    Row {
        key: "feature.showDesktopSwitchOverlay.animate",
        label: "Animate switch overlay",
        choices: &[],
    },
    Row {
        key: "feature.showDesktopSwitchOverlay.translucent",
        label: "Translucent switch overlay",
        choices: &[],
    },
    Row {
        key: "feature.showDesktopSwitchOverlay.showOnAllMonitors",
        label: "Show on all monitors",
        choices: &[],
    },
    Row {
        key: "feature.showDesktopStatusOverlay",
        label: "Keep desktop name visible",
        choices: &[],
    },
    Row {
        key: "feature.showDesktopStatusOverlay.position",
        label: "Permanent overlay position",
        choices: POS,
    },
    Row {
        key: "feature.showDesktopStatusOverlay.showOnAllMonitors",
        label: "Permanent overlay on all monitors",
        choices: &[],
    },
];
const HOTKEYS: &[Row] = &[
    Row {
        key: "feature.useHotKeyToJumpToPreviousDesktop",
        label: "Jump to most recent desktop",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToJumpToPreviousDesktop.hotkey",
        label: "Most recent shortcut",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToSwitchDesktopForward",
        label: "Next desktop shortcut enabled",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToSwitchDesktopForward.hotkey",
        label: "Next desktop shortcut",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToSwitchDesktopBackward",
        label: "Previous desktop shortcut enabled",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToSwitchDesktopBackward.hotkey",
        label: "Previous desktop shortcut",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToJumpToDesktopNumber",
        label: "Jump to desktop 1–9",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToJumpToDesktopNumber.hotkey",
        label: "Number shortcut modifiers",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToJumpToDesktopNumber.useDigits",
        label: "Use keyboard digits",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToJumpToDesktopNumber.useNumpadDigits",
        label: "Use numpad digits",
        choices: &[],
    },
    Row {
        key: "feature.useHotKeyToJumpToDesktopNumber.useFunctionKeys",
        label: "Use function keys F1–F9",
        choices: &[],
    },
];
const APPEARANCE: &[Row] = &[
    Row {
        key: "theme.overlay.width",
        label: "Switch overlay width",
        choices: &[],
    },
    Row {
        key: "theme.overlay.height",
        label: "Switch overlay height",
        choices: &[],
    },
    Row {
        key: "theme.overlay.fontSize",
        label: "Switch text size (pt)",
        choices: &[],
    },
    Row {
        key: "theme.status.width",
        label: "Permanent overlay width",
        choices: &[],
    },
    Row {
        key: "theme.status.height",
        label: "Permanent overlay height",
        choices: &[],
    },
    Row {
        key: "theme.status.offset",
        label: "Permanent overlay edge offset",
        choices: &[],
    },
    Row {
        key: "theme.status.fontSize",
        label: "Permanent text size (pt)",
        choices: &[],
    },
    Row {
        key: "feature.showPrevNextIcons.prevChar",
        label: "Previous icon character",
        choices: &[],
    },
    Row {
        key: "feature.showPrevNextIcons.nextChar",
        label: "Next icon character",
        choices: &[],
    },
];
struct Control {
    hwnd: HWND,
    key: &'static str,
    kind: u8,
}
struct State {
    parent: HWND,
    draft: Settings,
    page: usize,
    controls: Vec<Control>,
    children: Vec<HWND>,
    font: HFONT,
    title_font: HFONT,
    brush: HBRUSH,
    dark: bool,
    scale: f64,
    loading: bool,
    preview: super::overlay::Overlays,
}
impl State {
    unsafe fn new(parent: HWND, s: &Settings) -> Self {
        let dark = super::dark(s);
        Self {
            parent,
            draft: s.clone(),
            page: 0,
            controls: Vec::new(),
            children: Vec::new(),
            font: super::font("Segoe UI", 16),
            title_font: super::font("Segoe UI Variable, Bold", 27),
            brush: CreateSolidBrush(super::colors(dark).0),
            dark,
            scale: 1.,
            loading: false,
            preview: super::overlay::Overlays::new(),
        }
    }
    fn px(&self, n: i32) -> i32 {
        (n as f64 * self.scale).round() as i32
    }
    unsafe fn child(
        &mut self,
        parent: HWND,
        class: &str,
        label: &str,
        style: u32,
        id: usize,
        r: (i32, i32, i32, i32),
    ) -> HWND {
        let style = if class == "BUTTON" && style & 0x0f != BS_AUTOCHECKBOX as u32 {
            (style & !0x0f) | BS_OWNERDRAW as u32
        } else {
            style
        };
        let h = CreateWindowExW(
            0,
            wide(class).as_ptr(),
            wide(label).as_ptr(),
            WS_CHILD | WS_VISIBLE | style,
            self.px(r.0),
            self.px(r.1),
            self.px(r.2),
            self.px(r.3),
            parent,
            id as HMENU,
            GetModuleHandleW(std::ptr::null()),
            std::ptr::null(),
        );
        SendMessageW(h, WM_SETFONT, self.font as usize, 1);
        // Disable visual-style painting for checkboxes so WM_CTLCOLORBTN can
        // supply readable text in both light and dark themes.
        if class == "BUTTON" {
            SetWindowTheme(h, wide("").as_ptr(), wide("").as_ptr());
        }
        self.children.push(h);
        h
    }
    unsafe fn collect(&mut self) -> Result<(), String> {
        if self.page == 4 {
            if let Some(c) = self.controls.first() {
                self.draft.config = crate::settings::parse(&window_text(c.hwnd))?;
            }
        } else {
            for c in &self.controls {
                if self.draft.arguments.contains_key(c.key) {
                    continue;
                }
                let value = match c.kind {
                    0 => (SendMessageW(c.hwnd, BM_GETCHECK, 0, 0) == BST_CHECKED as isize)
                        .to_string(),
                    _ => window_text(c.hwnd),
                };
                self.draft.set(c.key, value);
            }
        }
        Ok(())
    }
    unsafe fn rebuild(&mut self, hwnd: HWND) {
        self.loading = true;
        for h in self.children.drain(..) {
            DestroyWindow(h);
        }
        self.controls.clear();
        self.dark = super::dark(&self.draft);
        DeleteObject(self.brush);
        self.brush = CreateSolidBrush(super::colors(self.dark).0);
        super::theme(hwnd, self.dark);
        for (i, page) in PAGES.iter().enumerate() {
            self.child(
                hwnd,
                "BUTTON",
                page,
                WS_TABSTOP | BS_PUSHBUTTON as u32,
                10 + i,
                (22, 178 + i as i32 * 44, 150, 36),
            );
        }
        let rows = match self.page {
            0 => GENERAL,
            1 => OVERLAYS,
            2 => HOTKEYS,
            3 => APPEARANCE,
            _ => &[],
        };
        if self.page == 4 {
            self.child(
                hwnd,
                "STATIC",
                "All file settings · key: value · custom hotkeys: Alt + A = Desktop1",
                0,
                0,
                (202, 176, 650, 30),
            );
            let mut values: std::collections::BTreeMap<String, String> = DEFAULTS
                .iter()
                .map(|(k, v)| (k.to_string(), v.to_string()))
                .collect();
            values.extend(self.draft.config.clone());
            let value = values
                .iter()
                .map(|(k, v)| format!("{k}: {}\r\n", crate::settings::quote(v)))
                .collect::<String>();
            let h = self.child(
                hwnd,
                "EDIT",
                &value,
                WS_TABSTOP
                    | WS_BORDER
                    | WS_VSCROLL
                    | ES_MULTILINE as u32
                    | ES_AUTOVSCROLL as u32
                    | ES_WANTRETURN as u32,
                100,
                (202, 210, 650, 390),
            );
            SendMessageW(h, EM_SETLIMITTEXT, 1024 * 1024, 0);
            self.controls.push(Control {
                hwnd: h,
                key: "",
                kind: 1,
            });
        }
        for (i, row) in rows.iter().enumerate() {
            let y = 180 + i as i32 * 36;
            self.child(hwnd, "STATIC", row.label, 0, 0, (202, y + 5, 365, 28));
            let value = self.draft.get(row.key).to_string();
            let is_bool = DEFAULTS
                .iter()
                .any(|(k, v)| *k == row.key && (*v == "true" || *v == "false"));
            let (class, style, kind) = if is_bool {
                ("BUTTON", BS_AUTOCHECKBOX as u32, 0)
            } else if !row.choices.is_empty() {
                ("COMBOBOX", CBS_DROPDOWNLIST as u32 | WS_VSCROLL, 2)
            } else {
                ("EDIT", WS_BORDER | ES_AUTOHSCROLL as u32, 1)
            };
            let h = self.child(
                hwnd,
                class,
                if is_bool { "Enabled" } else { &value },
                style | WS_TABSTOP,
                100 + i,
                (575, y, 277, if kind == 2 { 300 } else { 28 }),
            );
            if is_bool {
                SendMessageW(
                    h,
                    BM_SETCHECK,
                    usize::from(value.eq_ignore_ascii_case("true")),
                    0,
                );
            }
            if kind == 2 {
                for choice in row.choices {
                    SendMessageW(h, CB_ADDSTRING, 0, wide(choice).as_ptr() as isize);
                }
                let selection = row.choices.iter().position(|c| *c == value).unwrap_or(0);
                SendMessageW(h, CB_SETCURSEL, selection, 0);
            }
            if self.draft.arguments.contains_key(row.key) {
                EnableWindow(h, 0);
            }
            if row.key == "general.startupWithWindows" && crate::platform::registry::packaged() {
                EnableWindow(h, 0);
            }
            self.controls.push(Control {
                hwnd: h,
                key: row.key,
                kind,
            });
        }
        self.child(
            hwnd,
            "STATIC",
            if self.page == 2 {
                "Use Ctrl, Alt, Shift or Win + a key. Custom bindings are in Advanced."
            } else if crate::platform::registry::packaged() {
                "Packaged startup is managed in Windows Settings → Apps → Startup."
            } else {
                "Changes are previewed here. Apply saves them; command-line overrides stay locked."
            },
            0,
            0,
            (202, 610, 650, 42),
        );
        if crate::platform::registry::packaged() {
            self.child(
                hwnd,
                "BUTTON",
                "Windows startup settings",
                WS_TABSTOP,
                33,
                (385, 660, 240, 36),
            );
        }
        self.child(
            hwnd,
            "BUTTON",
            "Preview overlay",
            WS_TABSTOP,
            30,
            (202, 660, 170, 36),
        );
        self.child(
            hwnd,
            "BUTTON",
            "Apply",
            WS_TABSTOP | BS_DEFPUSHBUTTON as u32,
            31,
            (648, 660, 96, 36),
        );
        self.child(hwnd, "BUTTON", "Close", WS_TABSTOP, 32, (756, 660, 96, 36));
        self.loading = false;
        InvalidateRect(hwnd, std::ptr::null(), 1);
    }
    unsafe fn paint(&self, hwnd: HWND) {
        let mut ps = std::mem::zeroed();
        let dc = BeginPaint(hwnd, &mut ps);
        let mut r = std::mem::zeroed();
        GetClientRect(hwnd, &mut r);
        let (bg, fg) = super::colors(self.dark);
        super::fill(dc, &r, bg);
        let rect = |x, y, w, h| RECT {
            left: self.px(x),
            top: self.px(y),
            right: self.px(x + w),
            bottom: self.px(y + h),
        };
        super::text(
            dc,
            "Virtual Desktop Helper",
            rect(24, 20, 510, 42),
            fg,
            self.title_font,
            DT_SINGLELINE,
        );
        super::text(
            dc,
            "A place for every task",
            rect(26, 65, 390, 30),
            fg,
            self.font,
            DT_SINGLELINE,
        );
        // Native vector preview stays crisp at every DPI; no bitmap/web runtime.
        for i in 0..3 {
            let x = 480 + i * 126;
            let r = rect(x, 22, 112, 82);
            super::fill(
                dc,
                &r,
                if i == 1 {
                    0xc66c27
                } else if self.dark {
                    0x3a3a3a
                } else {
                    0xe5e5e5
                },
            );
            super::text(
                dc,
                &format!("{}", i + 1),
                rect(x, 30, 112, 32),
                if i == 1 { 0xffffff } else { fg },
                self.font,
                DT_CENTER | DT_SINGLELINE,
            );
            if i == 1 {
                for (feature, section, status) in [
                    ("feature.showDesktopSwitchOverlay", "theme.overlay", false),
                    ("feature.showDesktopStatusOverlay", "theme.status", true),
                ] {
                    if !self.draft.bool(feature) {
                        continue;
                    }
                    let w = (self.draft.int(&format!("{section}.width")) * 112 / 1920)
                        .clamp(if status { 14 } else { 40 }, 108);
                    let h = (self.draft.int(&format!("{section}.height")) * 82 / 1080)
                        .clamp(if status { 5 } else { 20 }, 76);
                    let area = RECT {
                        left: x + 2,
                        top: 24,
                        right: x + 110,
                        bottom: 102,
                    };
                    let (px, py) = super::overlay::position(
                        area,
                        w,
                        h,
                        0,
                        self.draft.get(&format!("{feature}.position")),
                    );
                    let theme = if self.dark { "dark" } else { "light" };
                    let bg =
                        super::color(self.draft.get(&format!("{section}.overlayBG.{theme}")), 0);
                    let fg = super::color(
                        self.draft.get(&format!("{section}.overlayFG.{theme}")),
                        0xffffff,
                    );
                    super::fill(dc, &rect(px, py, w, h), bg);
                    if !status {
                        super::text(
                            dc,
                            "Work",
                            rect(px, py, w, h),
                            fg,
                            self.font,
                            DT_CENTER | DT_VCENTER | DT_SINGLELINE,
                        );
                    }
                }
            }
        }
        super::text(
            dc,
            "LIVE PREVIEW",
            rect(480, 114, 360, 25),
            fg,
            self.font,
            DT_CENTER | DT_SINGLELINE,
        );
        super::text(
            dc,
            PAGES[self.page],
            rect(202, 126, 270, 35),
            fg,
            self.title_font,
            DT_SINGLELINE,
        );
        EndPaint(hwnd, &ps);
    }
}
impl Drop for State {
    fn drop(&mut self) {
        unsafe {
            DeleteObject(self.font);
            DeleteObject(self.title_font);
            DeleteObject(self.brush);
        }
    }
}
pub unsafe fn open(parent: HWND, s: &Settings) -> HWND {
    let class = wide("WVDH.Settings");
    let wc = WNDCLASSW {
        lpfnWndProc: Some(proc),
        hInstance: GetModuleHandleW(std::ptr::null()),
        lpszClassName: class.as_ptr(),
        hCursor: LoadCursorW(std::ptr::null_mut(), IDC_ARROW),
        ..std::mem::zeroed()
    };
    RegisterClassW(&wc);
    let state = Box::into_raw(Box::new(RefCell::new(State::new(parent, s))));
    let hwnd = CreateWindowExW(
        WS_EX_CONTROLPARENT,
        class.as_ptr(),
        wide("Settings · Windows Virtual Desktop Helper").as_ptr(),
        WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        900,
        760,
        std::ptr::null_mut(),
        std::ptr::null_mut(),
        wc.hInstance,
        state.cast(),
    );
    if hwnd.is_null() {
        drop(Box::from_raw(state));
        return hwnd;
    }
    let mut state_ref = (*state).borrow_mut();
    state_ref.scale = GetDpiForWindow(hwnd) as f64 / 96.;
    let mut r = RECT {
        left: 0,
        top: 0,
        right: (880. * state_ref.scale) as i32,
        bottom: (720. * state_ref.scale) as i32,
    };
    AdjustWindowRectExForDpi(
        &mut r,
        WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
        0,
        WS_EX_CONTROLPARENT,
        GetDpiForWindow(hwnd),
    );
    SetWindowPos(
        hwnd,
        std::ptr::null_mut(),
        0,
        0,
        r.right - r.left,
        r.bottom - r.top,
        SWP_NOMOVE | SWP_NOZORDER,
    );
    resize_fonts(&mut state_ref);
    state_ref.rebuild(hwnd);
    drop(state_ref);
    ShowWindow(hwnd, SW_SHOW);
    SetForegroundWindow(hwnd);
    hwnd
}
unsafe fn resize_fonts(s: &mut State) {
    DeleteObject(s.font);
    DeleteObject(s.title_font);
    s.font = super::font("Segoe UI", s.px(16));
    s.title_font = super::font("Segoe UI Variable, Bold", s.px(27));
}
unsafe fn window_text(hwnd: HWND) -> String {
    let n = GetWindowTextLengthW(hwnd);
    let mut v = vec![0; n as usize + 1];
    let len = GetWindowTextW(hwnd, v.as_mut_ptr(), v.len() as i32);
    String::from_utf16_lossy(&v[..len as usize])
}
unsafe extern "system" fn proc(hwnd: HWND, msg: u32, w: usize, l: isize) -> isize {
    if msg == WM_NCCREATE {
        SetWindowLongPtrW(
            hwnd,
            GWLP_USERDATA,
            (*(l as *const CREATESTRUCTW)).lpCreateParams as _,
        );
        return DefWindowProcW(hwnd, msg, w, l);
    }
    let ptr = GetWindowLongPtrW(hwnd, GWLP_USERDATA) as *mut RefCell<State>;
    if ptr.is_null() {
        return DefWindowProcW(hwnd, msg, w, l);
    }
    if msg == WM_NCDESTROY {
        SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
        drop(Box::from_raw(ptr));
        return DefWindowProcW(hwnd, msg, w, l);
    }
    if msg == WM_CLOSE {
        if (*ptr).try_borrow_mut().is_err() {
            return 0;
        }
        DestroyWindow(hwnd);
        return 0;
    }
    let Ok(mut state_ref) = (*ptr).try_borrow_mut() else {
        return DefWindowProcW(hwnd, msg, w, l);
    };
    let s = &mut *state_ref;
    match msg {
        WM_DRAWITEM => {
            let item = &*(l as *const DRAWITEMSTRUCT);
            let selected =
                item.CtlID >= 10 && item.CtlID < 15 && item.CtlID as usize - 10 == s.page;
            let (bg, fg) = if selected || item.CtlID == 31 {
                (0xc66c27, 0xffffff)
            } else if s.dark {
                (0x383838, 0xf4f4f4)
            } else {
                (0xffffff, 0x202020)
            };
            let brush = CreateSolidBrush(bg);
            let pen = CreatePen(
                PS_SOLID,
                1,
                if item.itemState & ODS_FOCUS != 0 {
                    0xc66c27
                } else {
                    bg
                },
            );
            let oldb = SelectObject(item.hDC, brush);
            let oldp = SelectObject(item.hDC, pen);
            RoundRect(
                item.hDC,
                item.rcItem.left,
                item.rcItem.top,
                item.rcItem.right,
                item.rcItem.bottom,
                s.px(8),
                s.px(8),
            );
            SelectObject(item.hDC, oldb);
            SelectObject(item.hDC, oldp);
            DeleteObject(brush);
            DeleteObject(pen);
            super::text(
                item.hDC,
                &window_text(item.hwndItem),
                item.rcItem,
                fg,
                s.font,
                DT_CENTER | DT_VCENTER | DT_SINGLELINE,
            );
            if item.itemState & ODS_FOCUS != 0 {
                let mut r = item.rcItem;
                r.left += 4;
                r.top += 4;
                r.right -= 4;
                r.bottom -= 4;
                DrawFocusRect(item.hDC, &r);
            }
            1
        }
        WM_PAINT => {
            s.paint(hwnd);
            0
        }
        WM_ERASEBKGND => 1,
        WM_CTLCOLORSTATIC | WM_CTLCOLOREDIT | WM_CTLCOLORBTN | WM_CTLCOLORLISTBOX => {
            let (bg, fg) = super::colors(s.dark);
            SetTextColor(w as HDC, fg);
            SetBkColor(w as HDC, bg);
            s.brush as isize
        }
        WM_SETTINGCHANGE | WM_THEMECHANGED => {
            if !s.loading {
                let _ = s.collect();
                s.rebuild(hwnd);
            }
            0
        }
        WM_DPICHANGED => {
            s.scale = (w & 0xffff) as f64 / 96.;
            let r = &*(l as *const RECT);
            SetWindowPos(
                hwnd,
                std::ptr::null_mut(),
                r.left,
                r.top,
                r.right - r.left,
                r.bottom - r.top,
                SWP_NOZORDER,
            );
            let _ = s.collect();
            resize_fonts(s);
            s.rebuild(hwnd);
            0
        }
        WM_COMMAND => {
            if s.loading {
                return 0;
            }
            let id = w & 0xffff;
            let notification = (w >> 16) & 0xffff;
            if (10..15).contains(&id) {
                match s.collect() {
                    Ok(()) => {
                        s.page = id - 10;
                        s.rebuild(hwnd);
                    }
                    Err(e) => crate::platform::report(&e),
                }
            } else if id == 33 {
                super::open_url(hwnd, "ms-settings:startupapps");
            } else if id == 32 {
                PostMessageW(hwnd, WM_CLOSE, 0, 0);
            } else if id == 30 || id == 31 {
                let result = s
                    .collect()
                    .and_then(|_| s.draft.validate())
                    .and_then(|_| crate::hotkeys::bindings(&s.draft).map(|_| ()));
                match result {
                    Err(e) => crate::platform::report(&e),
                    Ok(()) => {
                        if id == 30 {
                            s.preview
                                .show(&s.draft, "Desktop 2 · Work", false, Some(2000));
                        } else {
                            let boxed = Box::into_raw(Box::new(s.draft.clone()));
                            if PostMessageW(s.parent, APPLY, 0, boxed as isize) == 0 {
                                drop(Box::from_raw(boxed));
                            }
                        }
                    }
                }
            } else if id >= 100
                && (notification == EN_CHANGE as usize
                    || notification == BN_CLICKED as usize
                    || notification == CBN_SELCHANGE as usize)
            {
                let _ = s.collect();
                if s.dark != super::dark(&s.draft) {
                    PostMessageW(hwnd, WM_THEMECHANGED, 0, 0);
                }
                InvalidateRect(hwnd, std::ptr::null(), 0);
            }
            0
        }
        WM_CLOSE => {
            DestroyWindow(hwnd);
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

use crate::{
    hotkeys::{self, Action, Hotkeys},
    platform::{
        self,
        desktop::{DesktopManager, History, Snapshot},
        events::Watcher,
        wide,
    },
    settings::Settings,
    ui,
};
use std::{
    cell::RefCell,
    collections::HashMap,
    sync::atomic::{AtomicIsize, Ordering},
};
use windows_sys::Win32::{
    Foundation::*,
    System::{Com::*, LibraryLoader::GetModuleHandleW, Threading::*},
    UI::{Accessibility::*, Controls::*, HiDpi::*, WindowsAndMessaging::*},
};
thread_local! { static PENDING: RefCell<Vec<(u32,usize,isize)>> = const { RefCell::new(Vec::new()) }; }
static HOST: AtomicIsize = AtomicIsize::new(0);
const FOCUS: u32 = 0x8002;
struct Apartment;
impl Drop for Apartment {
    fn drop(&mut self) {
        unsafe {
            CoUninitialize();
        }
    }
}
struct Hook(HWINEVENTHOOK);
impl Drop for Hook {
    fn drop(&mut self) {
        unsafe {
            UnhookWinEvent(self.0);
        }
    }
}
struct App {
    settings: Settings,
    manager: Option<DesktopManager>,
    snapshot: Snapshot,
    history: History,
    watcher: Option<Watcher>,
    hotkeys: Hotkeys,
    tray: ui::tray::Tray,
    status: ui::overlay::Overlays,
    overlay: ui::overlay::Overlays,
    settings_window: HWND,
    hwnd: HWND,
    taskbar: u32,
    focused: HashMap<u128, HWND>,
    retry: u32,
}
impl App {
    fn refresh(&mut self) -> Result<(), String> {
        let next = self
            .manager
            .as_ref()
            .ok_or("Desktop connection is unavailable")?
            .snapshot()?;
        let switched = next.active().id != self.snapshot.active().id;
        let changed = switched
            || next.desktops != self.snapshot.desktops
            || next.current != self.snapshot.current;
        self.history.record(&next);
        self.focused
            .retain(|id, _| next.desktops.iter().any(|d| d.id == *id));
        self.snapshot = next;
        if changed {
            self.update_ui();
            if switched {
                if self.settings.bool("feature.showDesktopSwitchOverlay") {
                    self.overlay.show(
                        &self.settings,
                        &self.snapshot.active().name,
                        false,
                        Some(
                            self.settings
                                .int("feature.showDesktopSwitchOverlay.duration")
                                as u32,
                        ),
                    );
                }
                if self.settings.bool("feature.restorePreviousWindowFocus") {
                    if let Some(h) = self.focused.get(&self.snapshot.active().id) {
                        unsafe {
                            if IsWindow(*h) != 0
                                && self.manager.as_ref().is_some_and(|m| {
                                    m.window_desktop(*h).ok() == Some(self.snapshot.active().id)
                                })
                            {
                                SetForegroundWindow(*h);
                            }
                        }
                    }
                }
            }
        }
        Ok(())
    }
    fn update_ui(&mut self) {
        if !self.settings.bool("feature.showDesktopSwitchOverlay") {
            self.overlay.clear();
        }
        self.tray.update(&self.settings, &self.snapshot);
        if self.settings.bool("feature.showDesktopStatusOverlay") {
            self.status
                .show(&self.settings, &self.snapshot.active().name, true, None);
        } else {
            self.status.clear();
        }
    }
    fn reconnect(&mut self) {
        self.watcher = None;
        self.manager = None;
        match DesktopManager::connect().and_then(|m| Watcher::start(self.hwnd).map(|w| (m, w))) {
            Ok((manager, watcher)) => {
                self.manager = Some(manager);
                self.watcher = Some(watcher);
                let _ = self.refresh();
                self.update_ui();
                self.retry = 0;
                unsafe {
                    KillTimer(self.hwnd, 7);
                }
            }
            Err(e) => {
                platform::log(&e);
                self.retry += 1;
                if self.retry <= 6 {
                    unsafe {
                        SetTimer(self.hwnd, 7, 500 * self.retry, None);
                    }
                } else {
                    unsafe {
                        KillTimer(self.hwnd, 7);
                    }
                    platform::report("Could not reconnect after Explorer restarted. Use Retry connection from the tray menu.");
                }
            }
        }
    }
    fn execute(&mut self, action: Action) {
        if let Err(e) = self.refresh() {
            platform::log(&e);
            self.reconnect();
        }
        let i = self.snapshot.current;
        let target = match action {
            Action::Desktop(n) => Some(n),
            Action::Forward => (i + 1 < self.snapshot.desktops.len()).then_some(i + 1),
            Action::Backward => i.checked_sub(1),
            Action::Previous => self.history.previous(&self.snapshot),
        };
        let Some(n) = target else {
            return;
        };
        let result = self
            .manager
            .as_ref()
            .ok_or("Desktop connection is unavailable".into())
            .and_then(|m| m.switch(n));
        if let Err(e) = result {
            platform::log(&e);
            let result = match action {
                Action::Forward => platform::input::chord(&[0x11, 0x5b], 0x27),
                Action::Backward => platform::input::chord(&[0x11, 0x5b], 0x25),
                _ => Err(e),
            };
            if let Err(e) = result {
                platform::report(&e);
            }
        } else {
            let _ = self.refresh();
        }
    }
    fn open_settings(&mut self) {
        unsafe {
            if !self.settings_window.is_null() && IsWindow(self.settings_window) != 0 {
                ShowWindow(self.settings_window, SW_RESTORE);
                SetForegroundWindow(self.settings_window);
            } else {
                self.settings_window = ui::settings::open(self.hwnd, &self.settings);
            }
        }
    }
    fn register_hotkeys(&mut self) {
        match hotkeys::bindings(&self.settings) {
            Ok(bindings) => {
                let errors = self.hotkeys.replace(bindings);
                if !errors.is_empty() {
                    platform::report(&format!(
                        "These shortcuts are already in use by Windows or another app:\n{}",
                        errors.join("\n")
                    ));
                }
            }
            Err(e) => platform::report(&e),
        }
    }
    fn apply(&mut self, s: Settings) -> Result<(), String> {
        s.validate()?;
        hotkeys::bindings(&s)?;
        if s.bool("general.startupWithWindows") != self.settings.bool("general.startupWithWindows")
        {
            platform::registry::startup(s.bool("general.startupWithWindows"))?;
        }
        s.save()?;
        self.settings = s;
        self.register_hotkeys();
        self.update_ui();
        Ok(())
    }
    unsafe fn menu(&mut self) {
        let menu = CreatePopupMenu();
        for (id, label) in [
            (1, "Settings…"),
            (2, "Previous desktop"),
            (3, "Next desktop"),
            (4, "Most recent desktop"),
            (5, "Retry connection"),
            (6, "Open configuration folder"),
            (7, "About"),
            (8, "Donate"),
            (9, "Exit"),
        ] {
            AppendMenuW(menu, MF_STRING, id, wide(label).as_ptr());
        }
        let mut pt = std::mem::zeroed();
        GetCursorPos(&mut pt);
        SetForegroundWindow(self.hwnd);
        let cmd = TrackPopupMenu(
            menu,
            TPM_RETURNCMD | TPM_RIGHTBUTTON,
            pt.x,
            pt.y,
            0,
            self.hwnd,
            std::ptr::null(),
        );
        DestroyMenu(menu);
        PostMessageW(self.hwnd, WM_NULL, 0, 0);
        match cmd {
            1 => self.open_settings(),
            2 => self.execute(Action::Backward),
            3 => self.execute(Action::Forward),
            4 => self.execute(Action::Previous),
            5 => self.reconnect(),
            6 => {
                if let Some(p) = self.settings.path.parent() {
                    ui::open_url(self.hwnd, &p.to_string_lossy());
                }
            }
            7 => {
                MessageBoxW(self.hwnd,wide(&format!("Windows Virtual Desktop Helper {}\nNative Rust / Win32\n{}\n\nDan Krusi and contributors\nGPL-3.0 · VirtualDesktopAPI: MIT",env!("CARGO_PKG_VERSION"),self.manager.as_ref().map(|m|m.profile()).unwrap_or("Disconnected"))).as_ptr(),wide("About").as_ptr(),MB_OK);
            }
            8 => ui::open_url(
                self.hwnd,
                "https://www.paypal.com/donate/?hosted_button_id=BG5FYMAHFG9V6",
            ),
            9 => {
                PostQuitMessage(0);
            }
            _ => {}
        }
    }
}
pub fn run() -> Result<(), String> {
    let settings = Settings::load()?;
    unsafe {
        // Read-only diagnostics are useful to packaging/compatibility checks, without
        // changing startup registration or opening application windows.
        let diagnostics = settings.bool("diagnostics");
        let hr = CoInitializeEx(std::ptr::null(), COINIT_APARTMENTTHREADED as u32);
        if hr < 0 {
            return Err(format!("COM initialization failed: {hr:X}"));
        }
        let _apartment = Apartment;
        let manager = DesktopManager::connect()?;
        let snapshot = manager.snapshot()?;
        if diagnostics {
            let text = format!(
                "{}\nCurrent: {}\nDesktops: {}\n",
                manager.profile(),
                snapshot.current + 1,
                snapshot.desktops.len()
            );
            println!("{text}");
            if !settings.get("diagnostics.output").is_empty() {
                std::fs::write(settings.get("diagnostics.output"), text)
                    .map_err(|e| e.to_string())?;
            }
            return Ok(());
        }
        let mutex = platform::Handle(CreateMutexW(
            std::ptr::null(),
            0,
            wide("Local\\WindowsVirtualDesktopHelper.Rust").as_ptr(),
        ));
        if mutex.0.is_null() {
            return Err(std::io::Error::last_os_error().to_string());
        }
        if settings.bool("debug.singleInstance") && GetLastError() == ERROR_ALREADY_EXISTS {
            return Err("Windows Virtual Desktop Helper is already running.".into());
        }
        SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
        let controls = INITCOMMONCONTROLSEX {
            dwSize: std::mem::size_of::<INITCOMMONCONTROLSEX>() as u32,
            dwICC: ICC_STANDARD_CLASSES,
        };
        InitCommonControlsEx(&controls);
        let instance = GetModuleHandleW(std::ptr::null());
        let class = wide("WVDH.Host");
        let wc = WNDCLASSW {
            lpfnWndProc: Some(proc),
            hInstance: instance,
            lpszClassName: class.as_ptr(),
            ..std::mem::zeroed()
        };
        RegisterClassW(&wc);
        // Hidden top-level window receives Explorer's broadcast TaskbarCreated.
        let hwnd = CreateWindowExW(
            WS_EX_TOOLWINDOW,
            class.as_ptr(),
            wide("Windows Virtual Desktop Helper").as_ptr(),
            WS_POPUP,
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
        let mut history = History::default();
        history.record(&snapshot);
        let watcher = Watcher::start(hwnd)?;
        let app = Box::new(RefCell::new(App {
            settings,
            manager: Some(manager),
            snapshot,
            history,
            watcher: Some(watcher),
            hotkeys: Hotkeys::new(hwnd),
            tray: ui::tray::Tray::new(hwnd),
            status: ui::overlay::Overlays::new(),
            overlay: ui::overlay::Overlays::new(),
            settings_window: std::ptr::null_mut(),
            hwnd,
            taskbar: RegisterWindowMessageW(wide("TaskbarCreated").as_ptr()),
            focused: HashMap::new(),
            retry: 0,
        }));
        SetWindowLongPtrW(hwnd, GWLP_USERDATA, (&*app as *const RefCell<App>) as _);
        HOST.store(hwnd as isize, Ordering::Release);
        let hook = Hook(SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND,
            EVENT_SYSTEM_FOREGROUND,
            std::ptr::null_mut(),
            Some(foreground),
            0,
            0,
            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS,
        ));
        let mut app_ref = app.borrow_mut();
        let app_state = &mut *app_ref;

        if let Err(e) = app_state.refresh() {
            platform::log(&e);
            app_state.reconnect();
        }
        app_state.register_hotkeys();
        app_state.update_ui();
        if app_state.settings.bool("feature.showSplashScreen")
            && app_state.settings.bool("feature.showDesktopSwitchOverlay")
        {
            app_state.overlay.show(
                &app_state.settings,
                app_state.settings.get("feature.showSplashScreen.text"),
                false,
                Some(app_state.settings.int("feature.showSplashScreen.duration") as u32),
            );
        }
        if app_state.settings.bool("settings") {
            app_state.open_settings();
        }
        drop(app_ref);
        let mut message: MSG = std::mem::zeroed();
        loop {
            let ret = GetMessageW(&mut message, std::ptr::null_mut(), 0, 0);
            if ret <= 0 {
                break;
            }
            let settings_window = app.borrow().settings_window;
            if !settings_window.is_null()
                && IsWindow(settings_window) != 0
                && IsDialogMessageW(settings_window, &message) != 0
            {
                continue;
            }
            TranslateMessage(&message);
            DispatchMessageW(&message);
        }
        HOST.store(0, Ordering::Release);
        drop(hook);
        let settings_window = app.borrow().settings_window;
        if IsWindow(settings_window) != 0 {
            DestroyWindow(settings_window);
        }
        SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
        drop(app);
        DestroyWindow(hwnd);
        Ok(())
    }
}
unsafe extern "system" fn foreground(
    _: HWINEVENTHOOK,
    _: u32,
    hwnd: HWND,
    _: i32,
    _: i32,
    _: u32,
    _: u32,
) {
    let host = HOST.load(Ordering::Acquire);
    if host != 0 {
        PostMessageW(host as HWND, FOCUS, hwnd as usize, 0);
    }
}
unsafe extern "system" fn proc(hwnd: HWND, msg: u32, w: usize, l: isize) -> isize {
    let ptr = GetWindowLongPtrW(hwnd, GWLP_USERDATA) as *mut RefCell<App>;
    if ptr.is_null() {
        return DefWindowProcW(hwnd, msg, w, l);
    }
    let Ok(mut app_ref) = (*ptr).try_borrow_mut() else {
        // Modal dialogs pump messages. Defer owned commands until the outer
        // dispatch releases its borrow; never alias mutable application state.
        if matches!(
            msg,
            0x8001..=0x8004 | WM_HOTKEY | WM_SETTINGCHANGE | WM_THEMECHANGED | WM_DISPLAYCHANGE
        ) || msg >= 0xc000
        {
            PENDING.with(|p| p.borrow_mut().push((msg, w, l)));
            return 0;
        }
        return DefWindowProcW(hwnd, msg, w, l);
    };
    let result = dispatch(&mut app_ref, hwnd, msg, w, l);
    drop(app_ref);
    let pending = PENDING.with(|p| std::mem::take(&mut *p.borrow_mut()));
    for (msg, w, l) in pending {
        PostMessageW(hwnd, msg, w, l);
    }
    result
}
unsafe fn dispatch(app: &mut App, hwnd: HWND, msg: u32, w: usize, l: isize) -> isize {
    if msg == app.taskbar {
        app.reconnect();
        return 0;
    }
    match msg {
        platform::events::DESKTOP_CHANGED => {
            if w != 0 {
                app.reconnect();
                return 0;
            }
            if let Err(e) = app.refresh() {
                platform::log(&e);
                app.reconnect();
            }
            0
        }
        FOCUS => {
            if app.settings.bool("feature.restorePreviousWindowFocus") {
                let h = w as HWND;
                if IsWindow(h) != 0
                    && IsWindowVisible(h) != 0
                    && (GetWindowLongW(h, GWL_EXSTYLE) as u32 & WS_EX_TOOLWINDOW) == 0
                {
                    let mut name = [0u16; 128];
                    GetClassNameW(h, name.as_mut_ptr(), 128);
                    let name = String::from_utf16_lossy(&name);
                    if !name.starts_with("Shell_")
                        && !name.starts_with("Progman")
                        && !name.starts_with("WorkerW")
                    {
                        if let Some(manager) = &app.manager {
                            if let Ok(id) = manager.window_desktop(h) {
                                app.focused.insert(id, h);
                            }
                        }
                    }
                }
            }
            0
        }
        WM_HOTKEY => {
            let action = app
                .hotkeys
                .registered
                .iter()
                .find(|(id, _)| *id == w as i32)
                .map(|(_, b)| b.action.clone());
            if let Some(action) = action {
                app.execute(action);
            }
            0
        }
        ui::settings::APPLY => {
            let settings = *Box::from_raw(l as *mut Settings);
            if let Err(e) = app.apply(settings) {
                platform::report(&e);
            }
            0
        }
        ui::tray::MESSAGE => {
            if l as u32 == WM_RBUTTONUP {
                app.menu();
            } else if l as u32 == WM_LBUTTONUP {
                match w {
                    3 => app.execute(Action::Backward),
                    4 => app.execute(Action::Forward),
                    1 | 2 => {
                        if app
                            .settings
                            .bool("feature.showDesktopNumberInIconTray.clickToOpenTaskView")
                        {
                            if let Err(e) = platform::input::chord(&[0x5b], 9) {
                                platform::report(&e);
                            }
                        } else {
                            app.open_settings();
                        }
                    }
                    _ => app.open_settings(),
                }
            }
            0
        }
        WM_SETTINGCHANGE | WM_THEMECHANGED | WM_DISPLAYCHANGE => {
            app.update_ui();
            let settings_window = app.settings_window;
            if IsWindow(settings_window) != 0 {
                PostMessageW(app.settings_window, WM_THEMECHANGED, 0, 0);
            }
            0
        }
        WM_TIMER => {
            if w == 7 {
                app.reconnect();
            }
            0
        }
        WM_QUERYENDSESSION => 1,
        WM_ENDSESSION => {
            if w != 0 {
                PostQuitMessage(0);
            }
            0
        }
        WM_CLOSE => {
            PostQuitMessage(0);
            0
        }
        _ => DefWindowProcW(hwnd, msg, w, l),
    }
}

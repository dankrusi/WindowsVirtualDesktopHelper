//! WinUI lives in an on-demand child process. The tray's COM apartment and idle
//! footprint do not depend on a XAML window or a second permanent message loop.
use crate::{platform::wide, settings::Settings};
use std::process::{Child, Command};
use windows_sys::Win32::System::DataExchange::COPYDATASTRUCT;
use windows_sys::Win32::{Foundation::*, UI::WindowsAndMessaging::*};
pub const SETTINGS_MESSAGE: usize = 0x57564448;
pub const RESET_MESSAGE: usize = 0x57564449;
pub const SHOW_ABOUT: u32 = WM_APP + 37;
pub struct SettingsProcess(Child);
impl SettingsProcess {
    pub fn open(parent: HWND, settings: &Settings, about: bool) -> Result<Self, String> {
        let mut command = Command::new(std::env::current_exe().map_err(|e| e.to_string())?);
        for (k, v) in &settings.arguments {
            if k != "ui-parent" && k != "settings-ui" && k != "ui-page" && k != "notify" {
                command.arg(format!("--{k}")).arg(v);
            }
        }
        command
            .arg("--settings-ui")
            .arg("--ui-parent")
            .arg((parent as usize).to_string());
        command
            .arg("--ui-page")
            .arg(if about { "about" } else { "general" });
        command.spawn().map(Self).map_err(|e| e.to_string())
    }
    pub fn activate(&mut self, about: bool) -> bool {
        if !matches!(self.0.try_wait(), Ok(None)) {
            return false;
        }
        unsafe {
            if about {
                let mut observer = std::ptr::null_mut();
                loop {
                    observer = FindWindowExW(
                        std::ptr::null_mut(),
                        observer,
                        wide("WVDH.SettingsPreviewObserver").as_ptr(),
                        std::ptr::null(),
                    );
                    if observer.is_null() {
                        break;
                    }
                    let mut pid = 0;
                    GetWindowThreadProcessId(observer, &mut pid);
                    if pid == self.0.id() {
                        PostMessageW(observer, SHOW_ABOUT, 0, 0);
                        break;
                    }
                }
            }
            let hwnd = window_for_process(self.0.id());
            if !hwnd.is_null() {
                ShowWindow(hwnd, SW_RESTORE);
                SetForegroundWindow(hwnd);
            }
        }
        true
    }
}
impl Drop for SettingsProcess {
    fn drop(&mut self) {
        if !matches!(self.0.try_wait(), Ok(None)) {
            return;
        }
        unsafe {
            let windows = windows_for_process(self.0.id());
            if windows.is_empty() && matches!(self.0.try_wait(), Ok(None)) {
                let _ = self.0.kill();
            }
            for hwnd in windows {
                // Let the editor's native close handler protect an unsaved draft.
                PostMessageW(hwnd, WM_CLOSE, 0, 0);
            }
        }
    }
}
unsafe fn window_for_process(pid: u32) -> HWND {
    let windows = windows_for_process(pid);
    windows
        .iter()
        .copied()
        .find(|hwnd| {
            let mut title = [0u16; 128];
            let len = GetWindowTextW(*hwnd, title.as_mut_ptr(), title.len() as i32);
            String::from_utf16_lossy(&title[..len as usize])
                == "Settings \u{b7} Virtual Desktop Helper"
        })
        .or_else(|| windows.first().copied())
        .unwrap_or(std::ptr::null_mut())
}
unsafe fn windows_for_process(pid: u32) -> Vec<HWND> {
    unsafe extern "system" fn find(hwnd: HWND, data: isize) -> i32 {
        let (pid, result) = &mut *(data as *mut (u32, Vec<HWND>));
        let mut owner = 0;
        GetWindowThreadProcessId(hwnd, &mut owner);
        if owner == *pid
            && IsWindowVisible(hwnd) != 0
            && GetWindow(hwnd, GW_OWNER).is_null()
            && GetWindowLongPtrW(hwnd, GWL_EXSTYLE) & WS_EX_TOOLWINDOW as isize == 0
        {
            result.push(hwnd);
        }
        1
    }
    let mut data = (pid, Vec::<HWND>::new());
    EnumWindows(Some(find), (&mut data as *mut (u32, Vec<HWND>)) as isize);
    data.1
}
pub fn publish(settings: &Settings, parent: HWND) -> Result<(), String> {
    settings.validate()?;
    crate::hotkeys::bindings(settings)?;
    if parent.is_null() {
        return settings.save();
    }
    let text = settings
        .config
        .iter()
        .map(|(k, v)| format!("{k}: {}\n", crate::settings::quote(v)))
        .collect::<String>();
    send(parent, SETTINGS_MESSAGE, &text)
}
pub fn reset(settings: &Settings, parent: HWND, keys: &[String]) -> Result<(), String> {
    settings.validate()?;
    crate::hotkeys::bindings(settings)?;
    if parent.is_null() {
        return settings.save();
    }
    send(parent, RESET_MESSAGE, &keys.join("\n"))
}
fn send(parent: HWND, message: usize, text: &str) -> Result<(), String> {
    let data = COPYDATASTRUCT {
        dwData: message,
        cbData: text.len() as u32,
        lpData: text.as_ptr() as *mut _,
    };
    let mut reply = 0;
    let sent = unsafe {
        SendMessageTimeoutW(
            parent,
            WM_COPYDATA,
            0,
            (&data as *const COPYDATASTRUCT) as isize,
            SMTO_ABORTIFHUNG | SMTO_BLOCK,
            5000,
            &mut reply,
        )
    };
    if sent == 0 {
        return Err("The desktop helper isn't responding. Your change was not saved. Reopen Settings and try again.".into());
    }
    match reply {1=>Ok(()),2=>Err("That shortcut is already in use. Choose a different combination; your previous settings are still active.".into()),_=>Err("Couldn't save this change. Your previous settings are still active. Check app.log in the configuration folder.".into())}
}
pub fn open_folder(settings: &Settings) {
    if let Some(path) = settings.path.parent() {
        let _ = std::fs::create_dir_all(path);
        unsafe {
            super::open_url(std::ptr::null_mut(), &path.to_string_lossy());
        }
    }
}
pub fn startup_settings() {
    unsafe {
        windows_sys::Win32::UI::Shell::ShellExecuteW(
            std::ptr::null_mut(),
            wide("open").as_ptr(),
            wide("ms-settings:startupapps").as_ptr(),
            std::ptr::null(),
            std::ptr::null(),
            SW_SHOWNORMAL,
        );
    }
}

// Apply the native caption theme as well as WinUI's content theme.
// AppWindow PreferredTheme alone does not update classic caption colors on all builds.
pub fn update_caption(settings: &Settings) {
    update_caption_for("Settings \u{b7} Virtual Desktop Helper", settings);
}
pub(super) fn window_named(title: &str) -> HWND {
    unsafe {
        let title = wide(title);
        let mut hwnd = std::ptr::null_mut();
        loop {
            hwnd = FindWindowExW(std::ptr::null_mut(), hwnd, std::ptr::null(), title.as_ptr());
            if hwnd.is_null() {
                return hwnd;
            }
            let mut pid = 0;
            GetWindowThreadProcessId(hwnd, &mut pid);
            if pid == windows_sys::Win32::System::Threading::GetCurrentProcessId() {
                return hwnd;
            }
        }
    }
}
pub(super) fn update_caption_for(title: &str, settings: &Settings) {
    unsafe {
        let hwnd = window_named(title);
        if !hwnd.is_null() {
            let dark = match settings.get("general.theme") {
                "dark" => true,
                "light" => false,
                _ => crate::platform::registry::apps_dark(),
            };
            super::theme(hwnd, dark);
        }
    }
}

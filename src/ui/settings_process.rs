//! WinUI lives in an on-demand child process. The tray's COM apartment and idle
//! footprint do not depend on a XAML window or a second permanent message loop.
use crate::{platform::wide, settings::Settings};
use std::process::{Child, Command};
use windows_sys::Win32::System::DataExchange::COPYDATASTRUCT;
use windows_sys::Win32::{Foundation::*, UI::WindowsAndMessaging::*};
pub const SETTINGS_MESSAGE: usize = 0x57564448;
pub struct SettingsProcess(Child);
impl SettingsProcess {
    pub fn open(parent: HWND, settings: &Settings) -> Result<Self, String> {
        let mut command = Command::new(std::env::current_exe().map_err(|e| e.to_string())?);
        for (k, v) in &settings.arguments {
            if k != "ui-parent" && k != "settings-ui" {
                command.arg(format!("--{k}")).arg(v);
            }
        }
        command
            .arg("--settings-ui")
            .arg("--ui-parent")
            .arg((parent as usize).to_string());
        command.spawn().map(Self).map_err(|e| e.to_string())
    }
    pub fn activate(&mut self) -> bool {
        if !matches!(self.0.try_wait(), Ok(None)) {
            return false;
        }
        unsafe {
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
            let hwnd = window_for_process(self.0.id());
            if !hwnd.is_null() {
                PostMessageW(hwnd, WM_CLOSE, 0, 0);
            } else if matches!(self.0.try_wait(), Ok(None)) {
                let _ = self.0.kill();
            }
        }
    }
}
unsafe fn window_for_process(pid: u32) -> HWND {
    unsafe extern "system" fn find(hwnd: HWND, data: isize) -> i32 {
        let (pid, result) = &mut *(data as *mut (u32, HWND));
        let mut owner = 0;
        GetWindowThreadProcessId(hwnd, &mut owner);
        if owner == *pid && IsWindowVisible(hwnd) != 0 && GetWindow(hwnd, GW_OWNER).is_null() {
            *result = hwnd;
            0
        } else {
            1
        }
    }
    let mut data = (pid, std::ptr::null_mut());
    EnumWindows(Some(find), (&mut data as *mut (u32, HWND)) as isize);
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
    let data = COPYDATASTRUCT {
        dwData: SETTINGS_MESSAGE,
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
    unsafe {
        let hwnd = FindWindowW(
            std::ptr::null(),
            wide("Settings \u{b7} Virtual Desktop Helper").as_ptr(),
        );
        let mut pid = 0;
        GetWindowThreadProcessId(hwnd, &mut pid);
        if !hwnd.is_null() && pid == windows_sys::Win32::System::Threading::GetCurrentProcessId() {
            let dark = match settings.get("general.theme") {
                "dark" => true,
                "light" => false,
                _ => crate::platform::registry::apps_dark(),
            };
            super::theme(hwnd, dark);
        }
    }
}

//! WinUI lives in an on-demand child process. The tray's COM apartment and idle
//! footprint do not depend on a XAML window or a second permanent message loop.
use crate::{platform::wide, settings::Settings};
use std::{
    cell::RefCell,
    collections::BTreeMap,
    process::{Child, Command},
};
use windows_sys::Win32::System::DataExchange::COPYDATASTRUCT;
use windows_sys::Win32::{
    Foundation::*,
    System::LibraryLoader::GetModuleHandleW,
    UI::{
        HiDpi::{GetDpiForWindow, GetSystemMetricsForDpi},
        WindowsAndMessaging::*,
    },
};
pub const SETTINGS_MESSAGE: usize = 0x57564448;
pub const RESET_MESSAGE: usize = 0x57564449;
pub const SHOW_ABOUT: u32 = WM_APP + 37;
// The application icon group in assets/app.rc.
const APP_ICON_ID: u16 = 101;
thread_local! {
    // Keep shared HICONs alive until the UI thread and its windows shut down.
    static CAPTION_ICONS: RefCell<BTreeMap<(i32, i32), CaptionIcon>> = const {
        RefCell::new(BTreeMap::new())
    };
}
struct CaptionIcon(HICON);
impl Drop for CaptionIcon {
    fn drop(&mut self) {
        if !self.0.is_null() {
            unsafe { DestroyIcon(self.0) };
        }
    }
}
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
    windows_for_process(pid)
        .into_iter()
        .find(|hwnd| GetPropW(*hwnd, wide("WVDH.WindowRole").as_ptr()) as usize == 1)
        .unwrap_or(std::ptr::null_mut())
}
unsafe fn windows_for_process(pid: u32) -> Vec<HWND> {
    unsafe extern "system" fn find(hwnd: HWND, data: isize) -> i32 {
        let (pid, result) = &mut *(data as *mut (u32, Vec<HWND>));
        let mut owner = 0;
        GetWindowThreadProcessId(hwnd, &mut owner);
        if owner == *pid
            && (GetWindowLongPtrW(hwnd, GWL_STYLE) as u32) & WS_CAPTION == WS_CAPTION
            && GetWindow(hwnd, GW_OWNER).is_null()
            // GetWindowLongPtrW returns isize on 64-bit and i32 on 32-bit Windows.
            && (GetWindowLongPtrW(hwnd, GWL_EXSTYLE) as u32) & WS_EX_TOOLWINDOW == 0
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
        return Err(crate::i18n::text(
            "the-desktop-helper-isn-t-responding-your-change-was-not-saved-reopen-set",
        )
        .into());
    }
    match reply {
        1 => Ok(()),
        2 => Err(crate::i18n::text(
            "that-shortcut-is-already-in-use-choose-a-different-combination-your-prev",
        )
        .into()),
        _ => Err(crate::i18n::text(
            "couldn-t-save-this-change-your-previous-settings-are-still-active-check",
        )
        .into()),
    }
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
    update_caption_for(1, settings);
}
/// Claim a native window once; translated titles are never used as identity.
/// Settings mounts before the editor, so only the newly mounted window is unclaimed.
pub(super) fn window_for_role(role: usize) -> HWND {
    unsafe {
        let property = wide("WVDH.WindowRole");
        let windows =
            windows_for_process(windows_sys::Win32::System::Threading::GetCurrentProcessId());
        if let Some(hwnd) = windows
            .iter()
            .find(|h| GetPropW(**h, property.as_ptr()) as usize == role)
        {
            return *hwnd;
        }
        if let Some(hwnd) = windows.iter().find(|h| {
            GetPropW(**h, property.as_ptr()).is_null()
                && GetWindowThreadProcessId(**h, std::ptr::null_mut())
                    == windows_sys::Win32::System::Threading::GetCurrentThreadId()
        }) {
            if SetPropW(*hwnd, property.as_ptr(), role as HANDLE) != 0 {
                return *hwnd;
            }
        }
        std::ptr::null_mut()
    }
}
pub(super) fn update_caption_for(role: usize, settings: &Settings) {
    unsafe {
        let hwnd = window_for_role(role);
        if !hwnd.is_null() {
            let dark = match settings.get("general.theme") {
                "dark" => true,
                "light" => false,
                _ => crate::platform::registry::apps_dark(),
            };
            let title = crate::i18n::text(if role == 1 {
                "settings-virtual-desktop-helper"
            } else {
                "configuration-virtual-desktop-helper"
            });
            SetWindowTextW(hwnd, wide(title).as_ptr());
            set_caption_icons(hwnd);
            super::theme(hwnd, dark);
        }
    }
}

unsafe fn set_caption_icons(hwnd: HWND) {
    let dpi = GetDpiForWindow(hwnd);
    if dpi == 0 {
        return;
    }
    for (kind, width, height) in [
        (ICON_SMALL, SM_CXSMICON, SM_CYSMICON),
        (ICON_BIG, SM_CXICON, SM_CYICON),
    ] {
        let width = GetSystemMetricsForDpi(width, dpi);
        let height = GetSystemMetricsForDpi(height, dpi);
        let icon = CAPTION_ICONS.with(|icons| {
            icons
                .borrow_mut()
                .entry((width, height))
                .or_insert_with(|| {
                    CaptionIcon(LoadImageW(
                        GetModuleHandleW(std::ptr::null()),
                        APP_ICON_ID as usize as *const u16,
                        IMAGE_ICON,
                        width,
                        height,
                        0,
                    ))
                })
                .0
        });
        let kind = kind as usize;
        if !icon.is_null() && SendMessageW(hwnd, WM_GETICON, kind, dpi as isize) != icon as isize {
            SendMessageW(hwnd, WM_SETICON, kind, icon as isize);
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn native_window_captions_keep_distinct_roles_and_share_the_application_icon() {
        // Hidden native windows exercise caption setup without opening desktop UI.
        unsafe {
            let create = || {
                CreateWindowExW(
                    0,
                    wide("STATIC").as_ptr(),
                    wide("Settings").as_ptr(),
                    WS_OVERLAPPEDWINDOW,
                    0,
                    0,
                    100,
                    100,
                    std::ptr::null_mut(),
                    std::ptr::null_mut(),
                    std::ptr::null_mut(),
                    std::ptr::null(),
                )
            };
            let settings = create();
            assert!(!settings.is_null());
            assert_eq!(window_for_role(1), settings);
            let editor = create();
            assert!(!editor.is_null());
            assert_eq!(window_for_role(2), editor);
            SetWindowTextW(settings, wide("设置").as_ptr());
            SetWindowTextW(editor, wide("设置").as_ptr());
            assert_eq!(window_for_role(1), settings);
            assert_eq!(window_for_role(2), editor);
            assert_eq!(
                window_for_process(windows_sys::Win32::System::Threading::GetCurrentProcessId()),
                settings
            );
            let config = Settings {
                config: Default::default(),
                arguments: Default::default(),
                path: Default::default(),
            };
            update_caption(&config);
            update_caption_for(2, &config);
            for kind in [ICON_SMALL, ICON_BIG] {
                let settings_icon = SendMessageW(settings, WM_GETICON, kind as usize, 0);
                assert_ne!(settings_icon, 0, "missing icon for window caption");
                assert_eq!(
                    SendMessageW(editor, WM_GETICON, kind as usize, 0),
                    settings_icon
                );
            }
            DestroyWindow(editor);
            DestroyWindow(settings);
        }
    }
}

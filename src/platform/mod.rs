pub mod desktop;
pub mod events;
pub mod input;
mod profiles;
pub mod recovery;
pub mod registry;
use windows_sys::Win32::{Foundation::*, UI::WindowsAndMessaging::*};
pub fn wide(s: &str) -> Vec<u16> {
    s.encode_utf16().chain(Some(0)).collect()
}
pub fn report(s: &str) {
    eprintln!("{s}");
    log(s);
    unsafe {
        MessageBoxW(
            std::ptr::null_mut(),
            wide(s).as_ptr(),
            wide("Windows Virtual Desktop Helper").as_ptr(),
            MB_OK | MB_ICONERROR,
        );
    }
}
pub fn log(s: &str) {
    use std::io::Write;
    if let Some(dir) = std::env::var_os("APPDATA") {
        let dir = std::path::PathBuf::from(dir).join("WindowsVirtualDesktopHelper");
        let _ = std::fs::create_dir_all(&dir);
        let path = dir.join("app.log");
        if std::fs::metadata(&path).is_ok_and(|m| m.len() > 1_000_000) {
            let _ = std::fs::rename(&path, dir.join("app.previous.log"));
        }
        if let Ok(mut f) = std::fs::OpenOptions::new()
            .create(true)
            .append(true)
            .open(path)
        {
            let _ = writeln!(f, "{} [{}] {s}", timestamp(), process_role());
        }
    }
}
/// Local time as `2026-11-18 14:05:09.123`, readable without converting FILETIME intervals.
fn timestamp() -> String {
    let mut t: SYSTEMTIME = unsafe { std::mem::zeroed() };
    unsafe { windows_sys::Win32::System::SystemInformation::GetLocalTime(&mut t) };
    format_time(&t)
}
fn format_time(t: &SYSTEMTIME) -> String {
    format!(
        "{:04}-{:02}-{:02} {:02}:{:02}:{:02}.{:03}",
        t.wYear, t.wMonth, t.wDay, t.wHour, t.wMinute, t.wSecond, t.wMilliseconds
    )
}
/// Settings runs in its own process and logs to the same file.
fn process_role() -> &'static str {
    if std::env::args().any(|a| a == "--settings-ui") {
        "settings"
    } else {
        "app"
    }
}
pub struct Handle(pub HANDLE);
impl Drop for Handle {
    fn drop(&mut self) {
        if !self.0.is_null() {
            unsafe {
                CloseHandle(self.0);
            }
        }
    }
}
#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn log_timestamps_are_readable_local_time() {
        let t = SYSTEMTIME {
            wYear: 2026,
            wMonth: 11,
            wDayOfWeek: 3,
            wDay: 8,
            wHour: 9,
            wMinute: 5,
            wSecond: 7,
            wMilliseconds: 42,
        };
        assert_eq!(format_time(&t), "2026-11-08 09:05:07.042");
    }
}

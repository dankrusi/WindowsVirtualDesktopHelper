pub mod desktop;
pub mod events;
pub mod input;
mod profiles;
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
            let _ = writeln!(f, "{:?} {s}", std::time::SystemTime::now());
        }
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

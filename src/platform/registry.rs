use super::wide;
use windows_sys::Win32::System::Registry::*;
pub const DESKTOPS: &str = r"Software\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops";
pub struct Key(pub HKEY);
impl Drop for Key {
    fn drop(&mut self) {
        unsafe {
            RegCloseKey(self.0);
        }
    }
}
pub fn open(root: HKEY, path: &str, access: u32) -> Result<Key, String> {
    let mut key = std::ptr::null_mut();
    let code = unsafe { RegOpenKeyExW(root, wide(path).as_ptr(), 0, access, &mut key) };
    if code != 0 {
        return Err(format!("Registry {path}: {code}"));
    }
    Ok(Key(key))
}
pub fn bytes(root: HKEY, path: &str, name: &str) -> Option<Vec<u8>> {
    let k = open(root, path, KEY_READ).ok()?;
    let mut len = 0;
    unsafe {
        if RegQueryValueExW(
            k.0,
            wide(name).as_ptr(),
            std::ptr::null(),
            std::ptr::null_mut(),
            std::ptr::null_mut(),
            &mut len,
        ) != 0
        {
            return None;
        }
        let mut data = vec![0; len as usize];
        if RegQueryValueExW(
            k.0,
            wide(name).as_ptr(),
            std::ptr::null(),
            std::ptr::null_mut(),
            data.as_mut_ptr(),
            &mut len,
        ) != 0
        {
            return None;
        }
        data.truncate(len as usize);
        Some(data)
    }
}
pub fn string(root: HKEY, path: &str, name: &str) -> Option<String> {
    let b = bytes(root, path, name)?;
    let v: Vec<_> = b
        .chunks_exact(2)
        .map(|c| u16::from_le_bytes([c[0], c[1]]))
        .take_while(|c| *c != 0)
        .collect();
    Some(String::from_utf16_lossy(&v))
}
pub fn dark() -> bool {
    bytes(
        HKEY_CURRENT_USER,
        r"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
        "SystemUsesLightTheme",
    )
    .is_some_and(|v| v.first() == Some(&0))
}
pub fn apps_dark() -> bool {
    bytes(
        HKEY_CURRENT_USER,
        r"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
        "AppsUseLightTheme",
    )
    .is_some_and(|v| v.first() == Some(&0))
}
pub fn startup(enabled: bool) -> Result<(), String> {
    // Packaged startup is managed by Windows, not a versioned WindowsApps path.
    if packaged() {
        return Ok(());
    }
    let k = open(
        HKEY_CURRENT_USER,
        r"Software\Microsoft\Windows\CurrentVersion\Run",
        KEY_SET_VALUE,
    )?;
    let name = wide("Windows Virtual Desktop Helper");
    let code = unsafe {
        if enabled {
            let exe = std::env::current_exe().map_err(|e| e.to_string())?;
            let value = wide(&format!("\"{}\"", exe.display()));
            RegSetValueExW(
                k.0,
                name.as_ptr(),
                0,
                REG_SZ,
                value.as_ptr().cast(),
                (value.len() * 2) as u32,
            )
        } else {
            RegDeleteValueW(k.0, name.as_ptr())
        }
    };
    if code != 0 && code != 2 {
        return Err(format!("Cannot update startup: {code}"));
    }
    Ok(())
}
pub fn packaged() -> bool {
    #[link(name = "kernel32")]
    extern "system" {
        fn GetCurrentPackageFullName(len: *mut u32, name: *mut u16) -> i32;
    }
    let mut len = 0;
    unsafe { GetCurrentPackageFullName(&mut len, std::ptr::null_mut()) == 122 }
}

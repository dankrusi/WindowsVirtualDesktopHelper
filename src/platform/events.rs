//! Blocking registry notifications; no interval-based desktop/theme polling.
use super::{registry, Handle};
use windows_sys::Win32::{
    Foundation::*,
    System::{Registry::*, RemoteDesktop::ProcessIdToSessionId, Threading::*},
    UI::WindowsAndMessaging::PostMessageW,
};
pub const DESKTOP_CHANGED: u32 = 0x8001;
pub struct Watcher {
    stop: Handle,
    thread: Option<std::thread::JoinHandle<()>>,
}
impl Watcher {
    pub fn start(hwnd: HWND) -> Result<Self, String> {
        let mut keys = vec![registry::open(
            HKEY_CURRENT_USER,
            registry::DESKTOPS,
            KEY_NOTIFY,
        )?];
        let mut session = 0;
        unsafe {
            ProcessIdToSessionId(GetCurrentProcessId(), &mut session);
        }
        let path = format!(
            r"Software\Microsoft\Windows\CurrentVersion\Explorer\SessionInfo\{session}\VirtualDesktops"
        );
        if let Ok(key) = registry::open(HKEY_CURRENT_USER, &path, KEY_NOTIFY) {
            keys.push(key);
        }
        let stop = Handle(unsafe { CreateEventW(std::ptr::null(), 1, 0, std::ptr::null()) });
        if stop.0.is_null() {
            return Err(std::io::Error::last_os_error().to_string());
        }
        let mut events = Vec::new();
        for key in &keys {
            let event = Handle(unsafe { CreateEventW(std::ptr::null(), 0, 0, std::ptr::null()) });
            if event.0.is_null() {
                return Err(std::io::Error::last_os_error().to_string());
            }
            let code = unsafe {
                RegNotifyChangeKeyValue(
                    key.0,
                    1,
                    REG_NOTIFY_CHANGE_NAME
                        | REG_NOTIFY_CHANGE_LAST_SET
                        | REG_NOTIFY_THREAD_AGNOSTIC,
                    event.0,
                    1,
                )
            };
            if code != 0 {
                return Err(format!("Desktop notifications unavailable: {code}"));
            }
            events.push(event);
        }
        let stop_value = stop.0 as usize;
        let hwnd = hwnd as usize;
        let raw: Vec<_> = keys
            .iter()
            .zip(&events)
            .map(|(k, e)| (k.0 as usize, e.0 as usize))
            .collect();
        let thread = std::thread::Builder::new()
            .name("desktop-events".into())
            .spawn(move || unsafe {
                let watches: Vec<_> = raw
                    .into_iter()
                    .map(|(k, e)| (registry::Key(k as HKEY), Handle(e as HANDLE)))
                    .collect();
                let handles: Vec<_> = std::iter::once(stop_value as HANDLE)
                    .chain(watches.iter().map(|(_, e)| e.0))
                    .collect();
                loop {
                    let result =
                        WaitForMultipleObjects(handles.len() as u32, handles.as_ptr(), 0, INFINITE);
                    if result == WAIT_OBJECT_0 || result == WAIT_FAILED {
                        break;
                    }
                    let index = (result - WAIT_OBJECT_0 - 1) as usize;
                    let Some((key, event)) = watches.get(index) else {
                        break;
                    };
                    let code = RegNotifyChangeKeyValue(
                        key.0,
                        1,
                        REG_NOTIFY_CHANGE_NAME
                            | REG_NOTIFY_CHANGE_LAST_SET
                            | REG_NOTIFY_THREAD_AGNOSTIC,
                        event.0,
                        1,
                    );
                    PostMessageW(hwnd as HWND, DESKTOP_CHANGED, usize::from(code != 0), 0);
                    if code != 0 {
                        break;
                    }
                }
            })
            .map_err(|e| e.to_string())?;
        // Ownership transferred to the worker only after successful creation.
        for k in keys {
            std::mem::forget(k);
        }
        for e in events {
            std::mem::forget(e);
        }
        Ok(Self {
            stop,
            thread: Some(thread),
        })
    }
}
impl Drop for Watcher {
    fn drop(&mut self) {
        unsafe {
            SetEvent(self.stop.0);
        }
        if let Some(t) = self.thread.take() {
            let _ = t.join();
        }
    }
}

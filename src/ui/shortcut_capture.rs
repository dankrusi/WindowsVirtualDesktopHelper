//! Keyboard hook exists only while the foreground Settings window records a shortcut.
use std::cell::RefCell;
use windows_sys::Win32::{
    Foundation::*,
    System::{LibraryLoader::GetModuleHandleW, Threading::GetCurrentProcessId},
    UI::{Input::KeyboardAndMouse::*, WindowsAndMessaging::*},
};
type Callback = Box<dyn FnOnce(Option<String>)>;
thread_local! {static CALLBACK: RefCell<Option<Callback>> = const {RefCell::new(None)};}
pub struct Capture(HHOOK);
impl Capture {
    pub fn start(callback: impl FnOnce(Option<String>) + 'static) -> Result<Self, String> {
        let hook = unsafe {
            SetWindowsHookExW(
                WH_KEYBOARD_LL,
                Some(keyboard),
                GetModuleHandleW(std::ptr::null()),
                0,
            )
        };
        if hook.is_null() {
            return Err("Couldn't start shortcut recording. Try again.".into());
        }
        CALLBACK.with(|slot| *slot.borrow_mut() = Some(Box::new(callback)));
        Ok(Self(hook))
    }
}
impl Drop for Capture {
    fn drop(&mut self) {
        unsafe {
            UnhookWindowsHookEx(self.0);
        }
        CALLBACK.with(|slot| {
            slot.borrow_mut().take();
        });
    }
}
unsafe extern "system" fn keyboard(code: i32, w: WPARAM, l: LPARAM) -> LRESULT {
    if code < 0 {
        return CallNextHookEx(std::ptr::null_mut(), code, w, l);
    }
    let mut pid = 0;
    GetWindowThreadProcessId(GetForegroundWindow(), &mut pid);
    if pid != GetCurrentProcessId() {
        finish(None);
        return CallNextHookEx(std::ptr::null_mut(), code, w, l);
    }
    if w as u32 == WM_KEYDOWN || w as u32 == WM_SYSKEYDOWN {
        let key = (*(l as *const KBDLLHOOKSTRUCT)).vkCode;
        if key == VK_ESCAPE as u32 {
            finish(None);
            return 1;
        }
        if let Some(name) = key_name(key) {
            let mut parts = Vec::new();
            for (vk, label) in [(VK_CONTROL, "Ctrl"), (VK_MENU, "Alt"), (VK_SHIFT, "Shift")] {
                if GetAsyncKeyState(vk as i32) < 0 {
                    parts.push(label.to_string());
                }
            }
            if GetAsyncKeyState(VK_LWIN as i32) < 0 || GetAsyncKeyState(VK_RWIN as i32) < 0 {
                parts.push("Win".into());
            }
            if !parts.is_empty() {
                parts.push(name);
                finish(Some(parts.join(" + ")));
            }
            return 1;
        }
    }
    CallNextHookEx(std::ptr::null_mut(), code, w, l)
}
fn finish(value: Option<String>) {
    let callback = CALLBACK.with(|slot| slot.borrow_mut().take());
    if let Some(callback) = callback {
        callback(value);
    }
}
fn key_name(key: u32) -> Option<String> {
    Some(match key {
        0x30..=0x39 | 0x41..=0x5a => (key as u8 as char).to_string(),
        0x60..=0x69 => format!("NumPad{}", key - 0x60),
        0x70..=0x87 => format!("F{}", key - 0x6f),
        8 => "Backspace".into(),
        9 => "Tab".into(),
        13 => "Enter".into(),
        19 => "Pause".into(),
        20 => "CapsLock".into(),
        32 => "Space".into(),
        33 => "PageUp".into(),
        34 => "PageDown".into(),
        35 => "End".into(),
        36 => "Home".into(),
        37 => "Left".into(),
        38 => "Up".into(),
        39 => "Right".into(),
        40 => "Down".into(),
        44 => "PrintScreen".into(),
        45 => "Insert".into(),
        46 => "Delete".into(),
        106 => "Multiply".into(),
        107 => "Add".into(),
        109 => "Subtract".into(),
        110 => "Decimal".into(),
        111 => "Divide".into(),
        186 => "Semicolon".into(),
        187 => "Plus".into(),
        188 => "Comma".into(),
        189 => "Minus".into(),
        190 => "Period".into(),
        191 => "Question".into(),
        192 => "Tilde".into(),
        219 => "OpenBrackets".into(),
        220 => "Pipe".into(),
        221 => "CloseBrackets".into(),
        222 => "Quotes".into(),
        223 => "Oem8".into(),
        226 => "Backslash".into(),
        _ => return None,
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn recorded_names_roundtrip_through_hotkey_parser() {
        for vk in 0..256 {
            if let Some(name) = key_name(vk) {
                let binding =
                    crate::hotkeys::parse(&format!("Ctrl + Alt + {name} = PreviousDesktop"))
                        .unwrap();
                assert_eq!(binding.key, vk);
            }
        }
    }
}

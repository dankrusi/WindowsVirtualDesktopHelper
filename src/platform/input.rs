use windows_sys::Win32::UI::Input::KeyboardAndMouse::*;
/// Preserve physically held modifiers, and release only keys inserted by us.
pub fn chord(modifiers: &[u16], key: u16) -> Result<(), String> {
    unsafe {
        let held: Vec<_> = modifiers
            .iter()
            .copied()
            .filter(|k| GetAsyncKeyState(*k as i32) >= 0)
            .collect();
        let event = |vk, up| INPUT {
            r#type: INPUT_KEYBOARD,
            Anonymous: INPUT_0 {
                ki: KEYBDINPUT {
                    wVk: vk,
                    wScan: 0,
                    dwFlags: if up { KEYEVENTF_KEYUP } else { 0 },
                    time: 0,
                    dwExtraInfo: 0,
                },
            },
        };
        let mut inputs = Vec::with_capacity(held.len() * 2 + 2);
        for k in &held {
            inputs.push(event(*k, false));
        }
        inputs.push(event(key, false));
        inputs.push(event(key, true));
        for k in held.iter().rev() {
            inputs.push(event(*k, true));
        }
        let n = SendInput(
            inputs.len() as u32,
            inputs.as_ptr(),
            std::mem::size_of::<INPUT>() as i32,
        );
        if n != inputs.len() as u32 {
            let cleanup: Vec<_> = held
                .iter()
                .rev()
                .copied()
                .chain([key])
                .map(|k| event(k, true))
                .collect();
            SendInput(
                cleanup.len() as u32,
                cleanup.as_ptr(),
                std::mem::size_of::<INPUT>() as i32,
            );
            return Err(
                "Windows blocked simulated input (possibly an elevated foreground application)."
                    .into(),
            );
        }
        Ok(())
    }
}

use crate::settings::Settings;
use windows_sys::Win32::{Foundation::HWND, UI::Input::KeyboardAndMouse::*};
#[derive(Clone, Debug, PartialEq, Eq)]
pub enum Action {
    Forward,
    Backward,
    Previous,
    Desktop(usize),
}
impl Action {
    pub fn parse(s: &str) -> Result<Self, String> {
        let s = s.trim().to_lowercase();
        match s.as_str() {
            "desktopforward" => Ok(Self::Forward),
            "desktopbackward" => Ok(Self::Backward),
            "previousdesktop" => Ok(Self::Previous),
            _ => s
                .strip_prefix("desktop")
                .and_then(|s| s.parse::<usize>().ok())
                .filter(|n| *n > 0)
                .map(|n| Self::Desktop(n - 1))
                .ok_or_else(|| format!("Unknown action: {s}")),
        }
    }
}
#[derive(Clone, Debug)]
pub struct Binding {
    pub modifiers: u32,
    pub key: u32,
    pub action: Action,
    pub text: String,
}
pub fn parse(s: &str) -> Result<Binding, String> {
    let (keys, action) = s.split_once('=').ok_or("Hotkey needs '= Action'")?;
    let mut modifiers = 0;
    let mut key = None;
    for word in keys.split('+').map(str::trim) {
        match word.to_lowercase().as_str() {
            "ctrl" | "control" => modifiers |= MOD_CONTROL,
            "alt" => modifiers |= MOD_ALT,
            "shift" => modifiers |= MOD_SHIFT,
            "win" | "windows" => modifiers |= MOD_WIN,
            _ => {
                if key.is_some() {
                    return Err(format!("Only one regular key is allowed: {s}"));
                }
                key = Some(key_code(word).ok_or_else(|| format!("Unknown key: {word}"))?);
            }
        }
    }
    if modifiers == 0 || key.is_none() {
        return Err(format!("A modifier and a regular key are required: {s}"));
    }
    Ok(Binding {
        modifiers,
        key: key.unwrap(),
        action: Action::parse(action)?,
        text: s.into(),
    })
}
pub fn bindings(s: &Settings) -> Result<Vec<Binding>, String> {
    let mut list = Vec::new();
    for (k, v) in s.merged() {
        if k.starts_with("hotkeys.") {
            list.push(v);
        }
    }
    for (feature, action) in [
        ("useHotKeyToJumpToPreviousDesktop", "PreviousDesktop"),
        ("useHotKeyToSwitchDesktopForward", "DesktopForward"),
        ("useHotKeyToSwitchDesktopBackward", "DesktopBackward"),
    ] {
        let k = format!("feature.{feature}");
        if s.bool(&k) {
            list.push(format!("{} = {action}", s.get(&format!("{k}.hotkey"))));
        }
    }
    let k = "feature.useHotKeyToJumpToDesktopNumber";
    if s.bool(k) {
        for n in 1..=9 {
            for (suffix, prefix) in [
                ("useDigits", "D"),
                ("useNumpadDigits", "NumPad"),
                ("useFunctionKeys", "F"),
            ] {
                if s.bool(&format!("{k}.{suffix}")) {
                    list.push(format!(
                        "{} + {prefix}{n} = Desktop{n}",
                        s.get(&format!("{k}.hotkey"))
                    ));
                }
            }
        }
    }
    let mut seen = std::collections::HashSet::new();
    let mut result = Vec::new();
    for text in list {
        let b = parse(&text)?;
        if !seen.insert((b.modifiers, b.key)) {
            return Err(format!("Duplicate shortcut: {text}"));
        }
        result.push(b);
    }
    Ok(result)
}
pub struct Hotkeys {
    hwnd: HWND,
    pub registered: Vec<(i32, Binding)>,
}
impl Hotkeys {
    pub fn new(hwnd: HWND) -> Self {
        Self {
            hwnd,
            registered: Vec::new(),
        }
    }
    pub fn replace(&mut self, list: Vec<Binding>) -> Vec<String> {
        self.clear();
        let mut errors = Vec::new();
        for (i, b) in list.into_iter().enumerate() {
            let id = i as i32 + 1;
            if unsafe { RegisterHotKey(self.hwnd, id, b.modifiers | MOD_NOREPEAT, b.key) } == 0 {
                errors.push(b.text);
            } else {
                self.registered.push((id, b));
            }
        }
        errors
    }
    fn clear(&mut self) {
        for (id, _) in self.registered.drain(..) {
            unsafe {
                UnregisterHotKey(self.hwnd, id);
            }
        }
    }
}
impl Drop for Hotkeys {
    fn drop(&mut self) {
        self.clear();
    }
}
fn key_code(s: &str) -> Option<u32> {
    let s = s.to_lowercase();
    if s.len() == 1 {
        let c = s.as_bytes()[0];
        if c.is_ascii_alphanumeric() {
            return Some(c.to_ascii_uppercase() as u32);
        }
    }
    if let Some(n) = s
        .strip_prefix('d')
        .and_then(|n| n.parse::<u32>().ok())
        .filter(|n| *n <= 9)
    {
        return Some(0x30 + n);
    }
    if let Some(n) = s
        .strip_prefix("numpad")
        .and_then(|n| n.parse::<u32>().ok())
        .filter(|n| *n <= 9)
    {
        return Some(0x60 + n);
    }
    if let Some(n) = s
        .strip_prefix('f')
        .and_then(|n| n.parse::<u32>().ok())
        .filter(|n| (1..=24).contains(n))
    {
        return Some(0x6f + n);
    }
    Some(match s.as_str() {
        "lbutton" => 1,
        "rbutton" => 2,
        "cancel" => 3,
        "mbutton" => 4,
        "xbutton1" => 5,
        "xbutton2" => 6,
        "back" | "backspace" => 8,
        "tab" => 9,
        "linefeed" => 10,
        "return" | "enter" => 13,
        "menu" => 18,
        "pause" => 19,
        "capital" | "capslock" => 20,
        "kanamode" | "hanguelmode" | "hangulmode" => 21,
        "junjamode" => 23,
        "finalmode" => 24,
        "hanjamode" | "kanjimode" => 25,
        "escape" => 27,
        "imeconvert" => 28,
        "imenonconvert" => 29,
        "imeaccept" | "imeaceept" | "imaccept" => 30,
        "imemodechange" => 31,
        "space" => 32,
        "prior" | "pageup" => 33,
        "next" | "pagedown" => 34,
        "end" => 35,
        "home" => 36,
        "left" => 37,
        "up" => 38,
        "right" => 39,
        "down" => 40,
        "select" => 41,
        "print" => 42,
        "execute" => 43,
        "snapshot" | "printscreen" => 44,
        "insert" => 45,
        "delete" => 46,
        "help" => 47,
        "apps" => 93,
        "sleep" => 95,
        "multiply" => 106,
        "add" => 107,
        "separator" => 108,
        "subtract" => 109,
        "decimal" => 110,
        "divide" => 111,
        "numlock" => 144,
        "scroll" => 145,
        "lmenu" => 164,
        "rmenu" => 165,
        "browserback" => 166,
        "browserforward" => 167,
        "browserrefresh" => 168,
        "browserstop" => 169,
        "browsersearch" => 170,
        "browserfavorites" => 171,
        "browserhome" => 172,
        "volumemute" => 173,
        "volumedown" => 174,
        "volumeup" => 175,
        "medianexttrack" => 176,
        "mediaprevioustrack" => 177,
        "mediastop" => 178,
        "mediaplaypause" => 179,
        "launchmail" => 180,
        "selectmedia" => 181,
        "launchapplication1" => 182,
        "launchapplication2" => 183,
        "semicolon" | "oemsemicolon" | "oem1" => 186,
        "plus" | "oemplus" => 187,
        "comma" | "oemcomma" => 188,
        "minus" | "oemminus" => 189,
        "period" | "oemperiod" => 190,
        "question" | "oemquestion" | "oem2" => 191,
        "tilde" | "backtick" | "oemtilde" | "oem3" => 192,
        "openbrackets" | "oemopenbrackets" | "oem4" => 219,
        "pipe" | "oempipe" | "oem5" => 220,
        "closebrackets" | "oemclosebrackets" | "oem6" => 221,
        "quotes" | "oemquotes" | "oem7" => 222,
        "oem8" => 223,
        "backslash" | "oembackslash" | "oem102" => 226,
        "processkey" => 229,
        "packet" => 231,
        "attn" => 246,
        "crsel" => 247,
        "exsel" => 248,
        "eraseeof" => 249,
        "play" => 250,
        "zoom" => 251,
        "noname" => 252,
        "pa1" => 253,
        "clear" | "oemclear" => 254,
        _ => return None,
    })
}
#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn aliases_and_actions() {
        assert_eq!(
            parse("Ctrl + Alt + Tilde = PreviousDesktop").unwrap().key,
            192
        );
        assert_eq!(
            parse("Win + F9 = Desktop9").unwrap().action,
            Action::Desktop(8)
        );
        assert!(parse("Alt + A + B = Desktop1").is_err());
        assert!(parse("A = Desktop1").is_err());
        assert!(parse("Alt + A = Desktop0").is_err());
    }
}

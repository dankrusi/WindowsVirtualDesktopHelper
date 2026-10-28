mod defaults;
pub use defaults::DEFAULTS;
use std::{
    collections::BTreeMap,
    fs,
    path::{Path, PathBuf},
};

#[derive(Clone, Debug, PartialEq)]
pub struct Settings {
    pub config: BTreeMap<String, String>,
    pub arguments: BTreeMap<String, String>,
    pub path: PathBuf,
}
impl Settings {
    pub fn load() -> Result<Self, String> {
        let dir = PathBuf::from(std::env::var_os("APPDATA").ok_or("APPDATA is unavailable")?)
            .join("WindowsVirtualDesktopHelper");
        let mut result = Self {
            config: BTreeMap::new(),
            arguments: BTreeMap::new(),
            path: dir.join("WindowsVirtualDesktopHelper.exe.config"),
        };
        if dir.exists() {
            let mut paths = fs::read_dir(&dir)
                .map_err(|e| e.to_string())?
                .filter_map(Result::ok)
                .map(|e| e.path())
                .filter(|p| {
                    p.extension()
                        .is_some_and(|e| e.eq_ignore_ascii_case("config"))
                })
                .collect::<Vec<_>>();
            paths.sort_by_key(|p| (p == &result.path, p.clone()));
            for path in paths {
                result.config.extend(parse(
                    &fs::read_to_string(&path).map_err(|e| format!("{}: {e}", path.display()))?,
                )?);
            }
        }
        result.arguments = arguments(std::env::args().skip(1))?;
        result.validate()?;
        Ok(result)
    }
    pub fn get(&self, key: &str) -> &str {
        self.arguments
            .get(key)
            .or_else(|| self.config.get(key))
            .map(String::as_str)
            .or_else(|| DEFAULTS.iter().find(|(k, _)| *k == key).map(|(_, v)| *v))
            .unwrap_or("")
    }
    pub fn bool(&self, key: &str) -> bool {
        self.get(key).eq_ignore_ascii_case("true")
    }
    pub fn int(&self, key: &str) -> i32 {
        self.get(key).parse().unwrap_or(0)
    }
    pub fn number(&self, key: &str) -> f64 {
        self.get(key).parse().unwrap_or(0.)
    }
    pub fn set(&mut self, key: &str, value: impl Into<String>) {
        self.config.insert(key.into(), value.into());
    }
    pub fn merged(&self) -> BTreeMap<String, String> {
        let mut map: BTreeMap<_, _> = DEFAULTS
            .iter()
            .map(|(k, v)| (k.to_string(), v.to_string()))
            .collect();
        map.extend(self.config.clone());
        map.extend(self.arguments.clone());
        map
    }
    pub fn validate(&self) -> Result<(), String> {
        for (key, default) in DEFAULTS {
            let v = self.get(key);
            let valid = if *default == "true" || *default == "false" {
                v.eq_ignore_ascii_case("true") || v.eq_ignore_ascii_case("false")
            } else if key.ends_with(".fontSize") {
                v.parse::<f64>().is_ok_and(|n| n.is_finite())
            } else if default.parse::<i32>().is_ok() {
                v.parse::<i32>().is_ok()
            } else if default.parse::<f64>().is_ok() {
                v.parse::<f64>().is_ok_and(|n| n.is_finite())
            } else {
                !v.contains('\0')
            };
            if !valid {
                return Err(format!("Invalid value for {key}: {v}"));
            }
            if (key.ends_with(".width") || key.ends_with(".height") || key.ends_with(".fontSize"))
                && !(1.0..=16384.0).contains(&self.number(key))
            {
                return Err(format!("{key} must be between 1 and 16384"));
            }
            if key.ends_with(".duration") && !(0..=i32::MAX).contains(&self.int(key)) {
                return Err(format!("{key} must be a nonnegative integer"));
            }
            if key.ends_with(".position")
                && ![
                    "topleft",
                    "topcenter",
                    "topright",
                    "middleleft",
                    "middlecenter",
                    "middleright",
                    "bottomleft",
                    "bottomcenter",
                    "bottomright",
                ]
                .contains(&v)
            {
                return Err(format!("Invalid overlay position: {v}"));
            }
        }
        if !["auto", "light", "dark"].contains(&self.get("general.theme")) {
            return Err("Theme must be auto, light or dark".into());
        }
        Ok(())
    }
    pub fn save(&self) -> Result<(), String> {
        let dir = self.path.parent().ok_or("Invalid settings path")?;
        fs::create_dir_all(dir).map_err(|e| e.to_string())?;
        // Write all file settings (including custom/unknown keys), never CLI overrides.
        let text = self
            .config
            .iter()
            .map(|(k, v)| format!("{k}: {}\n", quote(v)))
            .collect::<String>();
        atomic_write(&self.path, &text)
    }
}
fn atomic_write(path: &Path, text: &str) -> Result<(), String> {
    use windows_sys::Win32::Storage::FileSystem::{
        MoveFileExW, MOVEFILE_REPLACE_EXISTING, MOVEFILE_WRITE_THROUGH,
    };
    let tmp = path.with_extension(format!("config.{}.tmp", std::process::id()));
    fs::write(&tmp, text).map_err(|e| e.to_string())?;
    let ok = unsafe {
        MoveFileExW(
            crate::platform::wide(&tmp.to_string_lossy()).as_ptr(),
            crate::platform::wide(&path.to_string_lossy()).as_ptr(),
            MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH,
        )
    };
    if ok == 0 {
        let e = std::io::Error::last_os_error().to_string();
        let _ = fs::remove_file(tmp);
        return Err(e);
    }
    Ok(())
}
pub fn quote(s: &str) -> String {
    format!(
        "\"{}\"",
        s.replace('\\', "\\\\")
            .replace('"', "\\\"")
            .replace('\n', "\\n")
            .replace('\r', "\\r")
            .replace('\t', "\\t")
    )
}
pub fn unquote(s: &str) -> String {
    let Some(s) = s.strip_prefix('"').and_then(|s| s.strip_suffix('"')) else {
        return s.into();
    };
    let mut out = Vec::<u16>::new();
    let chars: Vec<_> = s.encode_utf16().collect();
    let mut i = 0;
    while i < chars.len() {
        if chars[i] == 92 && i + 1 < chars.len() {
            let next = chars[i + 1];
            let escaped = match next {
                110 => Some(10),
                114 => Some(13),
                116 => Some(9),
                34 => Some(34),
                92 => Some(92),
                _ => None,
            };
            if let Some(c) = escaped {
                out.push(c);
                i += 2;
                continue;
            }
            if next == 117 && i + 5 < chars.len() {
                if let Ok(c) =
                    u16::from_str_radix(&String::from_utf16_lossy(&chars[i + 2..i + 6]), 16)
                {
                    out.push(c);
                    i += 6;
                    continue;
                }
            }
        }
        out.push(chars[i]);
        i += 1;
    }
    String::from_utf16_lossy(&out)
}
pub fn parse(text: &str) -> Result<BTreeMap<String, String>, String> {
    let mut map = BTreeMap::new();
    for (i, line) in text.trim_start_matches('\u{feff}').lines().enumerate() {
        let line = line.trim();
        if line.is_empty() || line.starts_with('#') {
            continue;
        }
        let (k, v) = line
            .split_once([':', '='])
            .ok_or_else(|| format!("Settings line {} needs a colon", i + 1))?;
        if k.trim().is_empty() {
            return Err(format!("Settings line {} has an empty key", i + 1));
        }
        map.insert(k.trim().into(), unquote(v.trim()));
    }
    Ok(map)
}
fn arguments(args: impl Iterator<Item = String>) -> Result<BTreeMap<String, String>, String> {
    let mut args = args.peekable();
    let mut map = BTreeMap::new();
    while let Some(arg) = args.next() {
        let key = arg
            .strip_prefix("--")
            .ok_or_else(|| format!("Expected --setting, found {arg}"))?;
        let val = if args.peek().is_some_and(|a| !a.starts_with("--")) {
            args.next().unwrap()
        } else {
            "true".into()
        };
        map.insert(key.into(), unquote(&val));
    }
    Ok(map)
}
#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn legacy_strings_roundtrip() {
        for s in ["C:\\Users\\Dan", "Work: 🦀", "line\n\t\"", "\\u2039"] {
            assert_eq!(unquote(&quote(s)), s);
        }
        assert_eq!(unquote(r#""\uD83E\uDD80""#), "🦀");
    }
    #[test]
    fn comments_colons_and_equals() {
        let m = parse("# comment\nhotkeys.test = \"Alt + D1 = Desktop1\"\npath: \"C:\\Users\"\n")
            .unwrap();
        assert_eq!(m["hotkeys.test"], "Alt + D1 = Desktop1");
        assert_eq!(m["path"], "C:\\Users");
    }
    #[test]
    fn flags_and_negative_values() {
        let m = arguments(
            ["--feature.x", "--theme.status.offset", "-5"]
                .into_iter()
                .map(str::to_string),
        )
        .unwrap();
        assert_eq!(m["feature.x"], "true");
        assert_eq!(m["theme.status.offset"], "-5");
    }
    #[test]
    fn precedence() {
        let mut s = Settings {
            config: BTreeMap::new(),
            arguments: BTreeMap::new(),
            path: PathBuf::new(),
        };
        s.set("general.theme", "light");
        s.arguments.insert("general.theme".into(), "dark".into());
        assert_eq!(s.get("general.theme"), "dark");
        assert!(s.bool("feature.showDesktopNumberInIconTray"));
        assert!(s.validate().is_ok());
        assert_eq!(DEFAULTS.len(), 62);
    }
    #[test]
    fn saving_preserves_unknown_keys_and_never_cli_overrides() {
        let path = std::env::temp_dir().join(format!(
            "wvdh-test-{}-{}.config",
            std::process::id(),
            std::time::SystemTime::now()
                .duration_since(std::time::UNIX_EPOCH)
                .unwrap()
                .as_nanos()
        ));
        let mut s = Settings {
            config: BTreeMap::new(),
            arguments: BTreeMap::new(),
            path: path.clone(),
        };
        s.set("general.theme", "light");
        s.set("custom.path", r"C:\Users\Work: 1");
        s.arguments.insert("general.theme".into(), "dark".into());
        s.save().unwrap();
        let file = parse(&std::fs::read_to_string(&path).unwrap()).unwrap();
        assert_eq!(file["general.theme"], "light");
        assert_eq!(file["custom.path"], r"C:\Users\Work: 1");
        s.set("custom.path", "changed");
        s.save().unwrap();
        assert_eq!(
            parse(&std::fs::read_to_string(&path).unwrap()).unwrap()["custom.path"],
            "changed"
        );
        std::fs::remove_file(path).unwrap();
    }
    #[test]
    fn invalid_numeric_values_are_rejected() {
        let mut s = Settings {
            config: BTreeMap::new(),
            arguments: BTreeMap::new(),
            path: PathBuf::new(),
        };
        for v in ["1.5", "0", "-2", "999999999999"] {
            s.set("theme.overlay.width", v);
            assert!(s.validate().is_err(), "{v}");
        }
        s.set("theme.overlay.width", "900");
        s.set("theme.overlay.fontSize", "12.5");
        s.set("feature.showDesktopSwitchOverlay.duration", "0");
        assert!(s.validate().is_ok());
        s.set("feature.showDesktopSwitchOverlay.duration", "1.5");
        assert!(s.validate().is_err());
    }
}

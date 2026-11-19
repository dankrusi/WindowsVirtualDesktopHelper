//! Startup discovery shared by the tray and Settings processes.
use super::{Catalog, CHOICES, MESSAGE_IDS, SOURCES};
use fluent_bundle::{concurrent::FluentBundle, FluentResource};
use std::{
    collections::HashMap,
    path::{Path, PathBuf},
};

fn bundle(locale: &str, source: &str) -> Result<FluentBundle<FluentResource>, String> {
    let locale = locale.parse().map_err(|e| format!("Invalid locale: {e}"))?;
    let resource = FluentResource::try_new(source.trim_start_matches('\u{feff}').to_string())
        .map_err(|(_, errors)| format!("Invalid Fluent syntax: {errors:?}"))?;
    let mut bundle = FluentBundle::new_concurrent(vec![locale]);
    bundle.set_use_isolating(false);
    bundle
        .add_resource(resource)
        .map_err(|errors| format!("Duplicate translation identifiers: {errors:?}"))?;
    Ok(bundle)
}

fn catalog(locale: &str, name: &str, source: &str, external: bool) -> Result<Catalog, String> {
    // Validate the standalone file first, including duplicate identifiers.
    let mut translated = bundle(locale, source)?;
    let name = translated
        .get_message("language-name")
        .and_then(|m| m.value())
        .and_then(|p| {
            let mut errors = Vec::new();
            let value = translated
                .format_pattern(p, None, &mut errors)
                .trim()
                .to_string();
            (errors.is_empty() && !value.is_empty() && !value.chars().any(char::is_control))
                .then_some(value)
        })
        .unwrap_or_else(|| name.to_string());
    if external {
        if let Some((_, embedded)) = SOURCES.iter().find(|(id, _)| *id == locale) {
            translated = bundle(locale, embedded)?;
            translated.add_resource_overriding(
                FluentResource::try_new(source.trim_start_matches('\u{feff}').to_string())
                    .expect("validated resource"),
            );
        }
    }
    let mut plain = HashMap::new();
    for key in MESSAGE_IDS {
        if let Some(pattern) = translated.get_message(key).and_then(|m| m.value()) {
            let mut errors = Vec::new();
            let value = translated.format_pattern(pattern, None, &mut errors);
            if errors.is_empty() {
                plain.insert((*key).into(), value.into_owned());
            }
        }
    }
    Ok(Catalog {
        locale: locale.into(),
        name,
        external,
        bundle: translated,
        plain,
    })
}

pub(super) fn embedded() -> Vec<Catalog> {
    SOURCES
        .iter()
        .map(|(locale, source)| {
            let name = CHOICES
                .iter()
                .find(|(id, _)| id == locale)
                .map_or(*locale, |(_, name)| *name);
            catalog(locale, name, source, false).expect("validated embedded catalog")
        })
        .collect()
}

/// Later directories win. A malformed higher-priority file never replaces a
/// working lower-priority catalog. The filesystem is only read at startup.
pub(super) fn discover(directories: &[PathBuf]) -> (Vec<Catalog>, Vec<String>) {
    let mut catalogs = embedded();
    let mut diagnostics = Vec::new();
    for directory in directories {
        let entries = match std::fs::read_dir(directory) {
            Ok(entries) => entries,
            Err(e) if e.kind() == std::io::ErrorKind::NotFound => continue,
            Err(e) => {
                diagnostics.push(format!(
                    "Translation directory {}: {e}",
                    directory.display()
                ));
                continue;
            }
        };
        let mut paths: Vec<_> = entries
            .filter_map(Result::ok)
            .map(|e| e.path())
            .filter(|p| p.is_file() && p.extension().is_some_and(|e| e.eq_ignore_ascii_case("ftl")))
            .collect();
        paths.sort();
        for path in paths {
            match read_catalog(&path) {
                Ok(translated) => {
                    if let Some(index) = catalogs.iter().position(|c| c.locale == translated.locale)
                    {
                        catalogs[index] = translated;
                    } else {
                        catalogs.push(translated);
                    }
                }
                Err(error) => {
                    diagnostics.push(format!("Ignoring translation {}: {error}", path.display()))
                }
            }
        }
    }
    (catalogs, diagnostics)
}
fn read_catalog(path: &Path) -> Result<Catalog, String> {
    let stem = path
        .file_stem()
        .and_then(|s| s.to_str())
        .ok_or("Invalid locale filename")?;
    if stem.eq_ignore_ascii_case("auto") {
        return Err("auto is reserved for Windows language selection".into());
    }
    let locale: unic_langid::LanguageIdentifier = stem
        .replace('_', "-")
        .parse()
        .map_err(|e| format!("Invalid locale filename: {e}"))?;
    let locale = locale.to_string();
    let source = std::fs::read_to_string(path).map_err(|e| e.to_string())?;
    let name = CHOICES
        .iter()
        .find(|(id, _)| *id == locale)
        .map_or(locale.as_str(), |(_, name)| *name);
    catalog(&locale, name, &source, true)
}

pub(super) fn installed() -> Vec<Catalog> {
    // Unit tests never discover or depend on the user's personal files.
    if cfg!(test) {
        return embedded();
    }
    let mut directories = Vec::new();
    if let Ok(exe) = std::env::current_exe() {
        if let Some(directory) = exe.parent() {
            directories.push(directory.to_path_buf());
        }
    }
    if let Some(appdata) = std::env::var_os("APPDATA") {
        directories.push(PathBuf::from(appdata).join("WindowsVirtualDesktopHelper"));
    }
    let (catalogs, diagnostics) = discover(&directories);
    for diagnostic in diagnostics {
        crate::platform::log(&diagnostic);
    }
    catalogs
}

#[cfg(test)]
mod tests {
    use super::*;
    struct Fixture(PathBuf);
    impl Fixture {
        fn new() -> Self {
            let root = std::env::temp_dir().join(format!(
                "wvdh-languages-{}-{}",
                std::process::id(),
                std::time::SystemTime::now()
                    .duration_since(std::time::UNIX_EPOCH)
                    .unwrap()
                    .as_nanos()
            ));
            std::fs::create_dir_all(root.join("exe")).unwrap();
            std::fs::create_dir_all(root.join("config")).unwrap();
            Self(root)
        }
        fn dirs(&self) -> [PathBuf; 2] {
            [self.0.join("exe"), self.0.join("config")]
        }
    }
    impl Drop for Fixture {
        fn drop(&mut self) {
            let _ = std::fs::remove_dir_all(&self.0);
        }
    }
    #[test]
    fn external_esperanto_is_discovered_selectable_and_formats_with_fallback() {
        let fixture = Fixture::new();
        std::fs::write(
            fixture.0.join("config/eo.ftl"),
            include_str!("../../examples/translations/eo.ftl"),
        )
        .unwrap();
        let (catalogs, warnings) = discover(&fixture.dirs());
        assert!(warnings.is_empty(), "{warnings:?}");
        assert_eq!(
            super::super::resolve_in(&catalogs, &"eo".parse().unwrap()),
            Some("eo")
        );
        let eo = catalogs.iter().find(|c| c.locale == "eo").unwrap();
        assert_eq!(eo.plain["general"], "Ĝenerala");
        assert!(super::super::choices_from(&catalogs, "Auto")
            .contains(&("eo".into(), "Esperanto · eo.ftl".into())));
        let mut args = fluent_bundle::FluentArgs::new();
        args.set("number", 3);
        assert_eq!(
            super::super::format_bundles(
                [&eo.bundle, &catalogs[0].bundle],
                "desktop-number-label",
                &args
            ),
            "Labortablo 3"
        );
        assert_eq!(
            super::super::format_bundles([&eo.bundle, &catalogs[0].bundle], "width", &args),
            "Width"
        );
        std::fs::remove_file(fixture.0.join("config/eo.ftl")).unwrap();
        assert!(!discover(&fixture.dirs()).0.iter().any(|c| c.locale == "eo"));
        assert!(super::super::valid_language("eo"));
    }
    #[test]
    fn config_wins_and_invalid_files_cannot_replace_working_catalogs() {
        let fixture = Fixture::new();
        std::fs::write(fixture.0.join("exe/de.ftl"), "general = Exe German").unwrap();
        std::fs::write(
            fixture.0.join("config/de.ftl"),
            "\u{feff}general = Config German",
        )
        .unwrap();
        std::fs::write(
            fixture.0.join("exe/eo.FTL"),
            "language-name = Esperanto\ngeneral = Ĝenerala",
        )
        .unwrap();
        std::fs::write(fixture.0.join("config/eo.ftl"), "general = { invalid").unwrap();
        std::fs::write(
            fixture.0.join("config/fr.ftl"),
            "general = One\ngeneral = Two",
        )
        .unwrap();
        std::fs::write(fixture.0.join("config/not-a-locale.ftl"), "general = Bad").unwrap();
        std::fs::write(fixture.0.join("config/zh-TW.ftl"), "general = 自訂繁體").unwrap();
        let (catalogs, warnings) = discover(&fixture.dirs());
        assert_eq!(warnings.len(), 3);
        assert_eq!(
            super::super::resolve_available(&catalogs, "zh-TW"),
            Some("zh-TW")
        );
        assert_eq!(
            super::super::resolve_available(&catalogs, "zh-HK"),
            Some("zh-Hant")
        );
        assert_eq!(catalogs.iter().filter(|c| c.locale == "de").count(), 1);
        let de = catalogs.iter().find(|c| c.locale == "de").unwrap();
        assert_eq!(de.plain["general"], "Config German");
        assert_eq!(de.plain["width"], "Breite");
        assert_eq!(
            catalogs.iter().find(|c| c.locale == "eo").unwrap().plain["general"],
            "Ĝenerala"
        );
        assert_eq!(
            catalogs.iter().find(|c| c.locale == "fr").unwrap().plain["general"],
            "Général"
        );
    }
}

mod catalogs;
// Embedded and discovered Fluent catalogs shared by both processes.
use fluent_bundle::{concurrent::FluentBundle, FluentArgs, FluentResource, FluentValue};
use std::{
    collections::HashMap,
    sync::{
        atomic::{AtomicUsize, Ordering},
        LazyLock,
    },
};

include!(concat!(env!("OUT_DIR"), "/locales.rs"));

struct Catalog {
    locale: String,
    name: String,
    external: bool,
    bundle: FluentBundle<FluentResource>,
    plain: HashMap<String, String>,
}
static EMBEDDED: LazyLock<Vec<Catalog>> = LazyLock::new(catalogs::embedded);
static CATALOGS: LazyLock<Vec<Catalog>> = LazyLock::new(catalogs::installed);
static CURRENT: AtomicUsize = AtomicUsize::new(usize::MAX);

/// An explicit script takes precedence over region; never cross Chinese scripts.
pub fn resolve(locale: &str) -> Option<&'static str> {
    resolve_available(&CATALOGS, locale)
}
fn resolve_available<'a>(catalogs: &'a [Catalog], locale: &str) -> Option<&'a str> {
    let normalized = locale.replace('_', "-");
    let id: unic_langid::LanguageIdentifier = normalized.parse().ok()?;
    // Installed regional catalogs take precedence over script negotiation.
    if let Some(exact) = catalogs.iter().find(|c| {
        c.locale
            .parse::<unic_langid::LanguageIdentifier>()
            .ok()
            .as_ref()
            == Some(&id)
    }) {
        return Some(&exact.locale);
    }
    let requested = if id.language.as_str() == "zh" {
        match id.script.map(|s| s.to_string()).as_deref() {
            Some("Hant") => "zh-Hant",
            Some("Hans") => "zh-Hans",
            Some(_) => return None,
            None => match id.region.map(|r| r.to_string()).as_deref() {
                Some("TW" | "HK" | "MO") => "zh-Hant",
                _ => "zh-Hans",
            },
        }
        .parse()
        .ok()?
    } else {
        id
    };
    resolve_in(catalogs, &requested)
}
/// A previously selected external locale may be temporarily absent. Keep its
/// preference valid and fall back to English so Settings remains accessible.
pub fn valid_language(value: &str) -> bool {
    value.eq_ignore_ascii_case("auto")
        || value
            .replace('_', "-")
            .parse::<unic_langid::LanguageIdentifier>()
            .is_ok_and(|id| id.language.as_str() != "und")
}
fn resolve_in<'a>(
    catalogs: &'a [Catalog],
    requested: &unic_langid::LanguageIdentifier,
) -> Option<&'a str> {
    catalogs
        .iter()
        .find(|c| {
            c.locale
                .parse::<unic_langid::LanguageIdentifier>()
                .ok()
                .as_ref()
                == Some(requested)
        })
        .or_else(|| {
            catalogs.iter().find(|c| {
                let candidate: unic_langid::LanguageIdentifier =
                    c.locale.parse().expect("catalog locale");
                candidate.language == requested.language && candidate.script == requested.script
            })
        })
        .map(|c| c.locale.as_str())
}
fn windows_language() -> &'static str {
    use windows_sys::Win32::Globalization::{GetUserPreferredUILanguages, MUI_LANGUAGE_NAME};
    unsafe {
        let (mut count, mut size) = (0, 0);
        if GetUserPreferredUILanguages(
            MUI_LANGUAGE_NAME,
            &mut count,
            std::ptr::null_mut(),
            &mut size,
        ) != 0
            && size > 0
        {
            let mut buffer = vec![0u16; size as usize];
            if GetUserPreferredUILanguages(
                MUI_LANGUAGE_NAME,
                &mut count,
                buffer.as_mut_ptr(),
                &mut size,
            ) != 0
            {
                for value in buffer.split(|u| *u == 0).filter(|v| !v.is_empty()) {
                    if let Some(locale) = resolve(&String::from_utf16_lossy(value)) {
                        return locale;
                    }
                }
            }
        }
    }
    "en-US"
}
pub fn configure(value: &str) {
    let locale = if value.eq_ignore_ascii_case("auto") {
        windows_language()
    } else {
        resolve(value).unwrap_or("en-US")
    };
    CURRENT.store(
        CATALOGS
            .iter()
            .position(|catalog| catalog.locale == locale)
            .unwrap_or(0),
        Ordering::Relaxed,
    );
}
fn current() -> usize {
    let value = CURRENT.load(Ordering::Relaxed);
    if value == usize::MAX {
        // Deterministic tests; production initializes from Settings before opening UI.
        0
    } else {
        value
    }
}
pub fn choices() -> Vec<(String, String)> {
    choices_from(&CATALOGS, text("use-windows-setting"))
}
fn choices_from(catalogs: &[Catalog], automatic: &str) -> Vec<(String, String)> {
    let mut choices = vec![("auto".into(), automatic.into())];
    choices.extend(catalogs.iter().map(|c| {
        (
            c.locale.clone(),
            if c.external {
                format!("{} · {}.ftl", c.name, c.locale)
            } else {
                c.name.clone()
            },
        )
    }));
    choices
}
pub fn text(key: &str) -> &str {
    text_in(current(), key)
}
fn text_in(index: usize, key: &str) -> &str {
    CATALOGS[index]
        .plain
        .get(key)
        .or_else(|| {
            EMBEDDED
                .iter()
                .find(|c| c.locale == CATALOGS[index].locale)
                .and_then(|c| c.plain.get(key))
        })
        .or_else(|| EMBEDDED[0].plain.get(key))
        .map(String::as_str)
        .unwrap_or(key)
}
pub fn format(key: &str, values: &[(&str, FluentValue<'_>)]) -> String {
    format_in(current(), key, values)
}
fn format_in(index: usize, key: &str, values: &[(&str, FluentValue<'_>)]) -> String {
    let mut args = FluentArgs::new();
    for (name, value) in values {
        args.set(*name, value.clone());
    }
    let base = EMBEDDED
        .iter()
        .find(|c| c.locale == CATALOGS[index].locale)
        .unwrap_or(&EMBEDDED[0]);
    format_bundles(
        [&CATALOGS[index].bundle, &base.bundle, &EMBEDDED[0].bundle],
        key,
        &args,
    )
}
fn format_bundles<const N: usize>(
    bundles: [&FluentBundle<FluentResource>; N],
    key: &str,
    args: &FluentArgs,
) -> String {
    for bundle in bundles {
        if let Some(pattern) = bundle.get_message(key).and_then(|m| m.value()) {
            let mut errors = Vec::new();
            let value = bundle.format_pattern(pattern, Some(args), &mut errors);
            if errors.is_empty() {
                return value.into_owned();
            }
        }
    }
    key.into()
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn plural_rules_and_missing_or_broken_messages_fall_back() {
        let mut translated = FluentBundle::new_concurrent(vec!["de".parse().unwrap()]);
        translated.add_resource(FluentResource::try_new("items = { $count ->\n    [one] Ein Desktop\n   *[other] { $count } Desktops\n}\nbroken = { $missing }\n".into()).unwrap()).unwrap();
        let mut english = FluentBundle::new_concurrent(vec!["en-US".parse().unwrap()]);
        english
            .add_resource(
                FluentResource::try_new(
                    "missing = English fallback\nbroken = Safe fallback".into(),
                )
                .unwrap(),
            )
            .unwrap();
        translated.set_use_isolating(false);
        let mut args = FluentArgs::new();
        args.set("count", 1);
        assert_eq!(
            format_bundles([&translated, &english], "items", &args),
            "Ein Desktop"
        );
        args.set("count", 2);
        assert_eq!(
            format_bundles([&translated, &english], "items", &args),
            "2 Desktops"
        );
        assert_eq!(
            format_bundles([&translated, &english], "missing", &args),
            "English fallback"
        );
        assert_eq!(
            format_bundles([&translated, &english], "broken", &args),
            "Safe fallback"
        );
    }
    #[test]
    fn regional_and_script_resolution() {
        for (input, expected) in [
            ("de-CH", "de"),
            ("es-MX", "es"),
            ("fr-CA", "fr"),
            ("EN-gb", "en-US"),
            ("zh-CN", "zh-Hans"),
            ("zh-SG", "zh-Hans"),
            ("zh-TW", "zh-Hant"),
            ("zh-HK", "zh-Hant"),
            ("zh-Hans-TW", "zh-Hans"),
            ("zh-Hant-CN", "zh-Hant"),
        ] {
            assert_eq!(resolve(input), Some(expected), "{input}");
        }
        assert_eq!(resolve("invalid"), None);
    }
    #[test]
    fn every_catalog_formats_every_message() {
        for (i, (locale, _)) in SOURCES.iter().enumerate() {
            for (key, variables) in MESSAGE_VARIABLES {
                let args: Vec<(&str, FluentValue)> = variables
                    .iter()
                    .map(|v| (*v, "工作 & e\u{301} 👩‍💻".into()))
                    .collect();
                let mut fluent_args = FluentArgs::new();
                for (k, v) in &args {
                    fluent_args.set(*k, v.clone());
                }
                let message = CATALOGS[i]
                    .bundle
                    .get_message(key)
                    .expect("complete catalog");
                let mut errors = Vec::new();
                CATALOGS[i].bundle.format_pattern(
                    message.value().unwrap(),
                    Some(&fluent_args),
                    &mut errors,
                );
                assert!(errors.is_empty(), "{locale}/{key}: {errors:?}");
                assert_ne!(format_in(i, key, &args), *key);
            }
        }
    }
    #[test]
    fn missing_keys_are_safe_and_cjk_is_embedded() {
        assert_eq!(text_in(0, "unknown-key"), "unknown-key");
        let zh = SOURCES.iter().position(|s| s.0 == "zh-Hans").unwrap();
        assert_eq!(text_in(zh, "general"), "常规");
    }
}

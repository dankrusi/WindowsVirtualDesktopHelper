//! An unsaved document is independent of the live configuration and native textbox.
use crate::settings::{self, Settings, DEFAULTS};
use std::collections::{BTreeMap, BTreeSet};

pub(super) struct Document {
    pub text: String,
    saved_text: String,
    saved_values: BTreeMap<String, String>,
    base: Settings,
}
impl Document {
    pub fn new(settings: &Settings) -> Self {
        let values = file_values(settings);
        let text = values
            .iter()
            .map(|(key, value)| format!("{key}: {}\n", settings::quote(value)))
            .collect::<String>();
        Self {
            saved_text: text.clone(),
            text,
            saved_values: values,
            base: settings.clone(),
        }
    }
    pub fn edit(&mut self, text: String) {
        // Keep the exact native string for controlled TextBox reconciliation.
        self.text = text;
    }
    pub fn dirty(&self) -> bool {
        normalize_newlines(&self.text) != normalize_newlines(&self.saved_text)
    }
    pub fn candidate(&self, latest: &Settings) -> Result<Settings, String> {
        let edited = settings::parse(&normalize_newlines(&self.text))?;
        let base = file_values(&self.base);
        let current = file_values(latest);
        let mut candidate = latest.clone();
        // Only apply keys changed in this document. A Settings control may have
        // changed another key while the editor was open; preserve that change.
        let keys: BTreeSet<_> = self.saved_values.keys().chain(edited.keys()).collect();
        for key in keys {
            let value = edited.get(key);
            if value == self.saved_values.get(key) {
                continue;
            }
            let desired = value.map(String::as_str).or_else(|| {
                DEFAULTS
                    .iter()
                    .find(|(name, _)| *name == key)
                    .map(|(_, value)| *value)
            });
            if current.get(key) != base.get(key) && current.get(key).map(String::as_str) != desired
            {
                return Err(format!(
                    "{key} changed in Settings while this editor was open. Its current value is {}. Your draft has been kept; reopen the editor to start from the current settings.",
                    current.get(key).map(String::as_str).unwrap_or("(removed)")
                ));
            }
            match value {
                Some(value) => candidate.set(key, value.clone()),
                None => {
                    candidate.config.remove(key);
                }
            }
        }
        candidate.validate()?;
        crate::hotkeys::bindings(&candidate)?;
        Ok(candidate)
    }
    pub fn saved(&mut self, settings: &Settings) {
        self.saved_values =
            settings::parse(&normalize_newlines(&self.text)).expect("saved document was validated");
        self.saved_text = self.text.clone();
        self.base = settings.clone();
    }
}
fn normalize_newlines(text: &str) -> String {
    text.replace("\r\n", "\n").replace('\r', "\n")
}
fn file_values(settings: &Settings) -> BTreeMap<String, String> {
    let mut values: BTreeMap<_, _> = DEFAULTS
        .iter()
        .map(|(key, value)| (key.to_string(), value.to_string()))
        .collect();
    values.extend(settings.config.clone());
    values
}
#[cfg(test)]
mod tests {
    use super::*;
    fn settings() -> Settings {
        Settings {
            config: BTreeMap::new(),
            arguments: BTreeMap::new(),
            path: Default::default(),
        }
    }
    #[test]
    fn draft_is_explicit_and_newlines_do_not_make_it_dirty() {
        let original = settings();
        let mut document = Document::new(&original);
        document.edit(document.text.replace('\n', "\r\n"));
        assert!(!document.dirty());
        document.edit(
            document
                .text
                .replace("theme.overlay.width: \"480\"", "theme.overlay.width: 600"),
        );
        assert!(document.dirty());
        assert_eq!(original.int("theme.overlay.width"), 480);
        let candidate = document.candidate(&original).unwrap();
        assert_eq!(candidate.int("theme.overlay.width"), 600);
        let draft = document.text.clone();
        document.saved(&candidate);
        assert!(!document.dirty());
        assert_eq!(document.text, draft);
        document.edit(
            document
                .text
                .replace("theme.overlay.width: 600", "theme.overlay.width: 700"),
        );
        assert_eq!(
            document
                .candidate(&candidate)
                .unwrap()
                .int("theme.overlay.width"),
            700
        );
    }
    #[test]
    fn invalid_drafts_preserve_saved_state_and_can_be_corrected() {
        let original = settings();
        let mut document = Document::new(&original);
        let valid = document.text.clone();
        document.edit("no colon".into());
        assert!(document
            .candidate(&original)
            .unwrap_err()
            .contains("line 1"));
        assert!(document.dirty());
        document.edit(valid.replace("theme.overlay.width: \"480\"", "theme.overlay.width: -1"));
        assert!(document.candidate(&original).is_err());
        document.edit(valid);
        assert!(!document.dirty());
    }
    #[test]
    fn save_merges_unrelated_changes_and_detects_same_key_conflicts() {
        let mut original = settings();
        original.set("custom.unknown", "keep");
        original
            .arguments
            .insert("general.theme".into(), "dark".into());
        let mut document = Document::new(&original);
        assert!(document.text.contains("general.theme: \"auto\""));
        document.edit(
            document
                .text
                .replace("theme.overlay.width: \"480\"", "theme.overlay.width: 600"),
        );
        let mut latest = original.clone();
        latest.set("theme.status.width", "450");
        let candidate = document.candidate(&latest).unwrap();
        assert_eq!(candidate.int("theme.status.width"), 450);
        assert_eq!(candidate.int("theme.overlay.width"), 600);
        assert_eq!(candidate.get("custom.unknown"), "keep");
        assert_eq!(candidate.arguments, original.arguments);
        assert!(!candidate.config.contains_key("general.theme"));
        latest.set("theme.overlay.width", "700");
        assert!(document
            .candidate(&latest)
            .unwrap_err()
            .contains("changed in Settings"));
        latest.set("theme.overlay.width", "600");
        assert!(document.candidate(&latest).is_ok());
    }
    #[test]
    fn removed_keys_reset_defaults_and_can_be_added_after_saving() {
        let mut original = settings();
        original.set("theme.overlay.width", "700");
        original.set("custom.label", "Work: \u{1f980}");
        let mut document = Document::new(&original);
        document.edit(
            document
                .text
                .lines()
                .filter(|line| {
                    !line.starts_with("theme.overlay.width:") && !line.starts_with("custom.label:")
                })
                .map(|line| format!("{line}\n"))
                .collect(),
        );
        let candidate = document.candidate(&original).unwrap();
        assert_eq!(candidate.int("theme.overlay.width"), 480);
        assert!(!candidate.config.contains_key("custom.label"));
        document.saved(&candidate);
        document.edit(format!(
            "{}theme.overlay.width: 650\ncustom.label: \"Work: \u{1f980}\"\n",
            document.text
        ));
        let next = document.candidate(&candidate).unwrap();
        assert_eq!(next.int("theme.overlay.width"), 650);
        assert_eq!(next.get("custom.label"), "Work: \u{1f980}");
    }
}

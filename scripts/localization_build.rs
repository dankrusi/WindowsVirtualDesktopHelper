use fluent_syntax::{ast, parser};
use std::{
    collections::{BTreeMap, BTreeSet},
    fs,
    path::Path,
};

fn pattern_vars(pattern: &ast::Pattern<&str>, vars: &mut BTreeSet<String>) {
    for element in &pattern.elements {
        if let ast::PatternElement::Placeable { expression } = element {
            expression_vars(expression, vars);
        }
    }
}
fn expression_vars(expression: &ast::Expression<&str>, vars: &mut BTreeSet<String>) {
    match expression {
        ast::Expression::Inline(value) => inline_vars(value, vars),
        ast::Expression::Select { selector, variants } => {
            inline_vars(selector, vars);
            for variant in variants {
                pattern_vars(&variant.value, vars);
            }
        }
    }
}
fn inline_vars(value: &ast::InlineExpression<&str>, vars: &mut BTreeSet<String>) {
    match value {
        ast::InlineExpression::VariableReference { id } => {
            vars.insert(id.name.into());
        }
        ast::InlineExpression::Placeable { expression } => expression_vars(expression, vars),
        ast::InlineExpression::FunctionReference { arguments, .. }
        | ast::InlineExpression::TermReference {
            arguments: Some(arguments),
            ..
        } => {
            for value in &arguments.positional {
                inline_vars(value, vars);
            }
            for value in &arguments.named {
                inline_vars(&value.value, vars);
            }
        }
        _ => {}
    }
}
fn catalog(path: &Path) -> BTreeMap<String, BTreeSet<String>> {
    let source = fs::read_to_string(path).expect("UTF-8 translation file");
    let resource = parser::parse(source.as_str())
        .unwrap_or_else(|(_, errors)| panic!("{}: {errors:?}", path.display()));
    let mut messages = BTreeMap::new();
    let mut identifiers = BTreeSet::new();
    for entry in resource.body {
        match entry {
            ast::Entry::Message(message) => {
                assert!(
                    identifiers.insert(message.id.name.to_string()),
                    "{}: duplicate {}",
                    path.display(),
                    message.id.name
                );
                let mut vars = BTreeSet::new();
                let value = message.value.expect("messages must have a value");
                pattern_vars(&value, &mut vars);
                for attribute in message.attributes {
                    pattern_vars(&attribute.value, &mut vars);
                }
                messages.insert(message.id.name.into(), vars);
            }
            ast::Entry::Term(term) => {
                assert!(
                    identifiers.insert(format!("-{}", term.id.name)),
                    "duplicate term"
                );
            }
            _ => {}
        }
    }
    messages
}
fn validate_references(directory: &Path, messages: &BTreeMap<String, BTreeSet<String>>) {
    for entry in fs::read_dir(directory).unwrap() {
        let path = entry.unwrap().path();
        if path.is_dir() {
            validate_references(&path, messages);
        } else if path.extension().is_some_and(|e| e == "rs") {
            let source = fs::read_to_string(&path).unwrap();
            let source = source.split("#[cfg(test)]\nmod tests {").next().unwrap();
            for function in ["crate::i18n::text(", "crate::i18n::format("] {
                for part in source.split(function).skip(1) {
                    if let Some(literal) = part.trim_start().strip_prefix('"') {
                        let key = literal.split('"').next().unwrap();
                        assert!(
                            messages.contains_key(key),
                            "{} references missing translation {key}",
                            path.display()
                        );
                    }
                }
            }
        }
    }
}
pub fn generate() {
    println!("cargo:rerun-if-changed=locales");
    println!("cargo:rerun-if-changed=src");
    println!("cargo:rerun-if-changed=scripts/localization_build.rs");
    let base = catalog(Path::new("locales/en-US.ftl"));
    validate_references(Path::new("src"), &base);
    let mut paths: Vec<_> = fs::read_dir("locales")
        .unwrap()
        .map(|e| e.unwrap().path())
        .filter(|p| p.extension().is_some_and(|e| e == "ftl"))
        .collect();
    paths.sort_by_key(|p| (p.file_stem().unwrap() != "en-US", p.clone()));
    let language_list = fs::read_to_string("locales/languages.txt").unwrap();
    let mut names = BTreeMap::new();
    for line in language_list
        .lines()
        .map(str::trim)
        .filter(|l| !l.is_empty() && !l.starts_with('#'))
    {
        let (locale, name) = line.split_once('=').expect("locale = native name");
        assert!(
            names
                .insert(locale.trim().to_string(), name.trim().to_string())
                .is_none(),
            "duplicate locale"
        );
    }
    assert_eq!(
        paths.len(),
        names.len(),
        "each language needs a catalog and a native name"
    );
    let mut code = String::from("pub const SOURCES: &[(&str, &str)] = &[\n");
    for path in paths {
        let messages = catalog(&path);
        for (key, vars) in &messages {
            if key == "language-name" {
                assert!(vars.is_empty(), "language-name must not use variables");
                continue;
            }
            assert_eq!(
                base.get(key),
                Some(vars),
                "{}: unknown message or mismatched variables for {key}",
                path.display()
            );
        }
        println!(
            "cargo:warning=Translation {}: {}/{} messages",
            path.display(),
            messages
                .keys()
                .filter(|key| base.contains_key(*key))
                .count(),
            base.len()
        );
        // Missing translations fall back to English. Tests require shipped catalogs to be complete.
        let locale = path.file_stem().unwrap().to_str().unwrap();
        assert!(
            names.contains_key(locale),
            "missing native language name for {locale}"
        );
        code += &format!(
            "({locale:?}, include_str!({:?})),\n",
            fs::canonicalize(&path).unwrap()
        );
    }
    code += "];\npub const CHOICES: &[(&str, &str)] = &[(\"auto\", \"use-windows-setting\"),\n";
    for (locale, name) in names {
        code += &format!("({locale:?}, {name:?}),\n");
    }
    code += "];\npub const MESSAGE_IDS: &[&str] = &[\n";
    for key in base.keys() {
        code += &format!("{key:?},\n");
    }
    code += "];\n#[cfg(test)]\npub const MESSAGE_VARIABLES: &[(&str, &[&str])] = &[\n";
    for (key, vars) in &base {
        code += &format!("({key:?}, &{:?}),\n", vars.iter().collect::<Vec<_>>());
    }
    code += "];\n";
    fs::write(
        Path::new(&std::env::var("OUT_DIR").unwrap()).join("locales.rs"),
        code,
    )
    .unwrap();
}

# Translating Windows Virtual Desktop Helper

The app embeds its UTF-8 Fluent (`.ftl`) catalogs in the executable. Optional files beside the configuration or executable add or override languages without rebuilding. No translation service or network access is needed.

## Edit a translation

Edit the file for your language in `locales/` and submit a GitHub pull request. GitHub's file editor is sufficient for wording changes; no Rust edits are needed. English (`en-US.ftl`) is the reference. The initial translations are ready for native-speaker review, especially regional terminology and Chinese wording.

* Keep the identifier before `=` unchanged, even when the English wording changes.
* Translate the complete message. Named variables such as `{ $name }` can move anywhere in the sentence; preserve their names and braces.
* Keep continuation lines indented. Write Chinese and accented characters directly, without Unicode escapes.
* Keep product names, configuration keys, shortcut tokens, paths, and version numbers unchanged.
* Add `#` comments to explain context. Avoid concatenating fragments that impose English word order.
* Simplified (`zh-Hans`) and Traditional (`zh-Hant`) Chinese are separate translations. Traditional Chinese currently uses Taiwan terminology; do not mechanically convert characters to create a regional translation.

Example:

```fluent
desktop-number-label = Desktop { $number }
tray-preview-summary = Desktop { $number } of { $total } · { $name }
```

`$number`, `$total`, and `$line` are numeric values. Fluent can use numeric arguments in locale-aware plural selectors. Identifiers, names, paths and shortcuts are text arguments. See the [Fluent guide](https://projectfluent.org/fluent/guide/).

## Context and terminology

| Term | Meaning |
| --- | --- |
| Desktop | A Windows virtual desktop, not a physical screen |
| Previous / next | Adjacent desktops in Windows' desktop order |
| Most recent desktop | The previously used desktop, which may not be adjacent |
| Tray | The notification area near the taskbar clock |
| Desktop initial | The first complete visible character of a desktop's name |
| Switch notification | A temporary overlay after switching desktops |
| Status label | The permanently visible desktop name |
| Reset | Restore defaults; clear a launch override only for the current session |
| Configuration editor | A separate draft window with explicit Save and unsaved-change protection |

The `tray-tooltip` and `tray-reconnecting` messages appear in Windows' length-limited tray tooltips. Keep their role label first so icon roles stay recognizable when Windows truncates a long desktop name. User desktop names are inserted unchanged.

## Add a language

1. Copy `locales/en-US.ftl` to a new file named with a BCP 47 locale identifier, for example `it.ftl` or `pt-BR.ftl`.
2. Add `it = Italiano` (or the appropriate identifier and native name) to `locales/languages.txt`.
3. Translate every message and run `scripts/check.ps1`.

The build discovers catalogs and generates the language selector and embedded resources. Regional variants resolve to a matching supported language; Chinese resolution preserves Hans/Hant, with TW/HK/MO selecting Traditional when no script is specified. Right-to-left languages need additional WinUI layout-direction work and visual validation before shipping; the current six catalogs use left-to-right layouts.

## Validation and collaboration

```powershell
.\scripts\check.ps1
```

Every build validates Fluent syntax, duplicate message IDs, unknown IDs and variable names against English. Build output includes each catalog's translated/total count. Runtime lookup falls back to English for missing or invalid messages, while automated tests require the shipped catalogs to be complete and format successfully. CI runs these checks on pull requests. Include the language, pages reviewed, and any terminology questions in the pull request description.

## Try an external language without rebuilding

Custom catalogs work in debug and release builds. Put a UTF-8 file named after its BCP 47 locale, such as `eo.ftl`, directly in either directory:

* `%APPDATA%\WindowsVirtualDesktopHelper\` (beside the configuration file).
* The directory containing `WindowsVirtualDesktopHelper.exe`.

Optionally include a message such as `language-name = Esperanto` to supply its native display name. Otherwise the picker uses the bundled language name or the locale code. External choices show the filename after the name, for example `Esperanto · eo.ftl`.

Restart the entire app after adding, editing or removing a file. Then select it under **General → Language**, set `general.language: eo`, or launch with `--general.language eo`. Both the tray and Settings discover the same directories at startup. No separate path setting or environment variable is needed.

The configuration-folder file wins when both directories contain the same locale. A partial file overrides its bundled language if one exists, with English as the final fallback. The higher-priority file is selected as a whole; it is not merged with the lower-priority external file. Invalid filenames, duplicate identifiers and malformed Fluent files are skipped and logged in `app.log`. Broken individual messages fall back to bundled text. If a selected file is removed, the app remains accessible using a bundled language or English, while retaining your language preference.

The [Esperanto example](../examples/translations/eo.ftl) translates the main Settings page, navigation, tray menu and editor controls. It is intentionally incomplete so you can also verify English fallback. To try it:

```powershell
Copy-Item examples/translations/eo.ftl "$env:APPDATA\WindowsVirtualDesktopHelper\eo.ftl"
```

Restart the app and choose Esperanto. Delete that copied file and restart to remove the option. To include a completed translation in future builds, submit it under `locales/` and add its name to `locales/languages.txt` as described above.

For expanded, accented English that exposes cramped layouts:

```powershell
python scripts/pseudo-localize.py "$env:TEMP\WVDH-pseudo"
```

Copy the generated `en-US.ftl` beside a test executable or into an isolated test configuration folder, restart, and choose English. Remove it after testing. Check all pages, narrow windows, high DPI, editor dialogs, tooltips, and Chinese desktop names on the actual taskbar and overlays. Keep manual test configuration in an isolated APPDATA directory.

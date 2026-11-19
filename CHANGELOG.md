# Unreleased

## Features

## Changes
* Export English, Simplified Chinese, and Traditional Chinese dark overview illustrations, Store box art, and poster art from Figma as locale-named PNGs in the screenshot gallery.
* Add Simplified and Traditional Chinese Settings screenshots in light and dark themes, label screenshot filenames by locale, and support language selection in the screenshot script and gallery.
* Add editable Microsoft Store listing text for all six supported languages and document CSV imports that reuse existing screenshots and artwork.
* Document WinGet installation through the Microsoft Store below Scoop in the README, and mark the custom WinGet community package submission as abandoned in favor of the Store channel.

## Fixes

# v3.0.2

## Features
* Use the app in English, Simplified Chinese, Traditional Chinese, German, Spanish or French, with a language picker and automatic Windows language detection.
* Translate the app by editing readable language files; all translations are included in the executable.
* Add custom languages by placing a translation file beside the configuration or executable, then choosing it in Settings after restarting.
* Display Chinese desktop names and complete accented or emoji initials more reliably in tray icons and overlays.

## Changes
* Organize distribution files under `installers/microsoftstore`, `installers/scoop`, and `installers/winget`; update packaging scripts and declare all six embedded languages in the Store manifest.
* Generate x64, x86 and ARM64 Scoop and WinGet manifests from verified release ZIPs. Check SHA-256, executable architecture and required files before writing manifests; run offline regression checks in CI. Declare the Windows App Runtime dependency for WinGet and document it for Scoop.
* Include package-manager manifests in CI release artifacts and update current version examples and the MSIX template during releases. Document the separate upstream submission steps for Scoop metadata and portable WinGet packages.
* Add `general.language: auto` and the `--general.language` launch override, with regional locale matching and English fallback. Language changes refresh Settings, the configuration editor and tray without restarting.
* Extract 231 messages into UTF-8 Fluent catalogs. Build-time checks validate syntax, duplicate IDs and placeholder names; tests require complete shipped catalogs. Generate the language selector from `locales/languages.txt`.
* Discover external UTF-8 Fluent files in both debug and release builds. Configuration-folder files take precedence over executable-folder files; partial catalogs fall back to embedded translations. Include a partial Esperanto example and contributor instructions.
* Add translator documentation and an expanded pseudo-localization generator for manual layout checks.
* Add `release.ps1 --prepare-only` to prepare version metadata locally without committing, tagging, pushing or changing published download links. Show the full build version in the Settings sidebar.
* Identify native Settings/editor windows by stable role properties and IPC shortcut conflicts by typed errors, independently of translated text.
* Include third-party license notices for the new localization dependencies.
* Add Fluent, language identifier and Unicode grapheme dependencies; check GDI font coverage and try Windows script fonts when necessary.

## Fixes
* Preserve combining marks and joined emoji in desktop initials, and stop treating all supplementary Unicode characters as emoji.
* Render ampersands literally in desktop overlays and allow more room for translated controls and headings.

# v3.0.1

## Features
* The Tray icons page explains how to bring hidden tray icons onto the taskbar and put them in order, with a short animation showing how.
* The Keyboard shortcuts page notes that some key combinations are reserved by Windows and can't be used.
* The tray icon previews show Previous on the left and Next on the right, with the desktop number and initial in between.
* The desktop preview has a plain background, so the overlay stands out.

## Changes
* `app.log` lines start with a readable local timestamp (`2026-11-18 14:05:09.123`) and the process that wrote them (`[app]` or `[settings]`).
* The tray arrange animation is a separate WinUI component that redraws only itself, pauses while nothing moves, and shows a still image when Windows animations are turned off.
* Enable the `Win32_System_SystemInformation` feature of `windows-sys`.

## Fixes
* Fix unreadable timestamps such as `SystemTime { intervals: 134376789134260356 }` in `app.log`.

# v3.0.0

## Features
* Rebuilt from the ground up as a small native app that no longer needs .NET.
* New modern Settings window with light and dark themes, automatic saving, shortcut recording and live previews of the overlays.
* New configuration editor window with Save (Ctrl+S), input checks, hotkey conflict warnings and a reminder before closing with unsaved changes.
* More ways to customize the desktop name overlays.
* Crisp, transparent tray icons at any display scaling, and overlays that size correctly on every monitor.
* A more compact desktop-switch notification by default.
* Recovers automatically when Windows Explorer restarts or the tray icons disappear.
* Available for x64, x86 and ARM64 PCs, from the Microsoft Store or as a portable ZIP.
* Existing settings, hotkeys and command-line options keep working.
* Requires Windows 11. Windows 10 users should keep using v2.1.

## Changes
* Replace C#/.NET and MSI with native Rust/Win32 and MSIX packaging (single executable, statically linked CRT).
* Settings and the configuration editor use WinUI 3 through `windows-reactor` 0.100 in a separate `--settings-ui` process; requires Microsoft Windows App Runtime 2.4+ (`Microsoft.WindowsAppRuntime.2`).
* Minimum OS is Windows 11 build 22000: the Windows App Runtime bootstrap uses Windows 11 dynamic package dependency APIs.
* Preserve all 61 original configuration keys, CLI overrides, desktop actions and custom hotkeys.
* Replace desktop/focus/theme polling with registry notifications, foreground hooks and window messages.
* Use a 480 x 128 DIP switch notification by default; explicitly configured sizes are preserved.
* Render antialiased tray icons at taskbar DPI and update them in place; scale overlays per monitor.
* Use stable desktop IDs for desktop history and keep a consistent tray icon identity.
* Use the manifest startup task and Windows Startup Apps settings in MSIX builds; portable builds keep the per-user Run entry.
* Add Cargo checks, native PE/package validation and x64/x86/ARM64 CI builds; pin the Rust toolchain in `rust-toolchain.toml`.
* Add `scripts/release.ps1` to version, tag and publish releases, and `scripts/get-store-upload.ps1` to fetch the Store upload (one `.msixupload` bundling all architectures). `VERSION.md` holds the app version.
* Build scripts take Linux-style arguments (for example `--architecture x64 --store`).
* Add `scripts/screenshot-settings.ps1` to capture every Settings page in light and dark themes into `screenshots/`, using a throwaway `APPDATA`; windows are centered on the primary screen and captured over a solid backdrop. It also writes `screenshots/screenshots.md` listing every image in the folder (overview first, light/dark side by side); `--index-only` refreshes just that page.
* `--settings-ui` accepts any page for `--ui-page` (`general`, `tray`, `overlays`, `hotkeys`, `advanced`, `about`) and a `--ui-height` window height.

## Fixes
* Fix the x86 build (`GetWindowLongPtrW` returns `i32` on 32-bit Windows).
* Fix the CI check failing on newer Rust releases (clippy `chunks_exact_to_as_chunks`).

# v2.1

## Features
* Show the current desktop name in a permanent status overlay, with configurable position, font, transparency, animation, and monitor selection.
* Configure a hotkey to jump to the most recent desktop, with Ctrl/Alt/Shift/Win modifiers and Tab/backtick radio buttons. Repeated presses toggle between the two most recent desktops (#132).
* Choose any combination of Ctrl, Alt, Shift, and Win for numbered Jump to Desktop hotkeys (#192).
* Choose keyboard digits, numpad digits, and/or F1-F9 for Jump to Desktop hotkeys (#193). Keyboard and numpad digits remain enabled by default.

## Changes
* Support font styles in tray icon and overlay font configuration.
* Keep a single app instance by default; advanced configurations can override this with `debug.singleInstance`.
* Add a virtual desktop API implementation for Windows Server 2022 and improve fallback between API implementations.
* Make previous-window focus restoration opt-in through `feature.restorePreviousWindowFocus` (disabled by default).
* Restore the `+#` suffix on each numbered hotkey modifier label.
* Update the license to GNU GPL v3 or later.

## Fixes
* Prevent the hidden tray host window from appearing on the desktop or in Alt+Tab.
* Fix tray icon GDI handle leaks and event-handler leaks in desktop notification and permanent status overlays.
* Recover the virtual desktop API connection after Windows Explorer restarts.
* Preserve configuration values containing colons and backslashes, and consistently parse and save numbers across regional settings.
* Report hotkey registration conflicts while keeping other shortcuts working.
* Preserve saved hotkey modifiers while the feature is disabled and clear old hotkeys when no modifier or key group is selected.
* Remember the starting desktop and update history immediately after a jump so toggling works on the first switch and with rapid repeated presses.


# v2.0

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Custom hot keys for virtual desktop actions
* Autostart with Windows
* Simple and lightweight
* Fully configurable
* Free

## Fixes
* Improvements to last focused window detection and restoration

## Changes
* Setup and EXE are now code-signed!
* New powerful configuration system with lots of new configuration options via the config file or command line
* Support theming of icons (fonts, colors)
* Support theming of overlays (fonts, colors)
* Custom hotkeys can now be defined in the configuration file to run various actions

### Note: configuration from previous versions are not migrated - sorry!
 

# v1.17

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Jump to desktop using hot keys
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Fixes
* Support Windows 11 23H2 2921
* Minor typos

## Changes
* Show hide prev/next icon based on current index
* Automatically grab focus on last focused window when switching desktop



# v1.16

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Jump to desktop using hot keys
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Fixes
* Support Windows 11 22621

## Changes
* Support for Emojis on Windows 11 (Windows 10 it is not possible)


# v1.15

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Jump to desktop using hot keys
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Windows 11 Insider Canary initial support
* Show notification on all monitors

## Fixes
* Windows 11 use consistent notification tray icon labels so that icon arrangement always stays



# v1.14

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Fixes
* Windows 11 23H2 fix switch desktop with arrow icons


# v1.13

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Switch desktop with arrow icons fallback mechanism to use shortcut keys (CTRL+WIN+LEFT/RIGHT) if the API fails


# v1.12

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Support Windows Insider 11.226x1.2050


# v1.11

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Support for High DPI Monitors
* Icons are dynamically generated
* Support for additional icon which shows the first letter of the desktop name
* Improved detection for Windows 11 Insider Builds

## Fixes
* Bug where implementation loading with fallback actually didn't try to load the fallback, but kept trying to load the original suggested implementation
* Fix UI to support desktop name initial



# v1.10

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Improved light/dark theme detection
* Settings now saved per user (note: settings from previous versions are reset when updating to this version)



# v1.9

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Added new 500ms option for overlay duration
* Added show Task View on Desktop Number click feature



# v1.8

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Improved fallback attempts to other Windows APIs if initial guess doesn't work

## Fixes
* Support Windows 11 Insider builds 


# v1.7

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Notification display is click-through
* Fallback system which tries all virtual desktop API implementations if the best guess fails

## Fixes
* Prevent crashes on failed desktop information updates


# v1.6

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Allow custom positions for overlay (9 different positions)



# v1.5

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

## Changes
* Overlay settings (animate, translucency, duration, enabled)

## Fixes
* Improved rendering quality of overlay
* Overlays don't take focus



# v1.4

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Option to show prev/next desktop by clicking icons in notification area
* Option to autostart with Windows
* Simple and lightweight
* Free

## Fixes
* Support for Windows 11 22H2
* Support for light Windows theme
* Icons get removed on exit



# v1.3

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Option to show prev/next desktop by clicking icons in notification area
* Option to autostart with Windows
* Simple and lightweight
* Free

## Changes
* New error handling UI



# v1.2

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Simple and lightweight
* Free

## Changes
* Remove the 3rd-Party Installer, include standard Microsoft Setup instead



# v1.1

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Simple and lightweight
* Free

## Fixes
* Fix Windows11 detection



# v1.0

## Features

* Show desktop number in notification area
* Show desktop name when switching desktops
* Simple and lightweight
* Free

# Validation

Run `scripts/check.ps1` for formatting, Clippy and unit tests. The tests cover legacy strings/escaping, CLI parsing, setting precedence, hotkey parsing and stable desktop history.

Read-only current-machine COM smoke test:

```powershell
target\release\WindowsVirtualDesktopHelper.exe --diagnostics --diagnostics.output artifacts\diagnostics.txt
```

`--settings` opens the settings window on launch. Normal launch runs in the tray. For UI validation with an isolated configuration, set APPDATA only in the child process environment to a temporary directory; do not enable startup while testing.

Before publishing this Windows 11 WinUI build, check supported Windows 11 versions with differing desktop interfaces:

- Number/name and previous/next tray icons, boundary hiding, Task View, Settings and Exit.
- External desktop switch, create, remove, reorder and rename; rapid previous-desktop toggling.
- Digit, numpad, function and custom hotkeys; invalid syntax and occupied registrations.
- Temporary and permanent overlays, all nine positions, multi-monitor mode, duration zero, animation, translucency, fonts and colors.
- Foreground-window restoration, Explorer restart, display/DPI changes, light/dark themes, keyboard navigation and screen readers.
- Config read/save/reload, unknown keys, multiple config files and CLI overrides that must not persist.
- Portable startup enable/disable; packaged startup through Windows Settings.
- Signed MSIX clean install, upgrade, startup, uninstall and Microsoft Store certification.

Automated MakeAppx validation checks packaging structure; it does not install the package, trust a certificate, or establish Store certification. Development MSIX files are unsigned until explicitly signed using a trusted certificate whose subject matches the manifest Publisher.

## Initial Rust port results on 2026-10-28

- Windows 11 build 26200.9711: x64 and x86 COM diagnostic checks passed; nine desktops detected through the preserved 53F5CA0B interface.
- Nine unit tests, format check and Clippy with warnings denied passed.
- The opt-in `live_switch_and_registry_notification` integration test passed: switched to the adjacent desktop, received the registry event, verified the current desktop, and restored the original desktop in 0.07 seconds.
- Dark and light settings windows, native hotkey controls, overlay preview and Apply were inspected interactively. An isolated settings file confirmed that CLI theme/splash overrides were not persisted.
- x64 and x86 release builds, MakeAppx package creation, and native PE/manifest validation passed.
- ARM64 `cargo check` passed. Linking/package creation could not be validated locally because the ARM64 MSVC linker/toolset is not installed.
- No MSIX was installed, no certificate was trusted and no release was published. Other Windows builds, multiple physical monitors, Explorer restart, package upgrade/uninstall and Store certification remain release validation work.

To repeat the interactive switch test (it briefly changes desktops and restores the original):

```powershell
cargo test live_switch_and_registry_notification -- --ignored --nocapture
```

## WinUI redesign validation on 2026-10-28

- Replaced the initial GDI Settings window with actual WinUI 3 through Microsoft windows-reactor 0.100.0. Runtime 2.5.1.0 is installed on this Windows 11 machine.
- Formatting, Clippy with warnings denied, and fifteen unit tests pass. The desktop-switch integration test remains opt-in; the redesign did not change the COM implementation.
- New rasterization checks cover transparency, premultiplied alpha, disabled opacity, multi-digit glyphs, and sizes 16/20/24/32/40/48 pixels.
- Live checks with an isolated APPDATA confirm light/dark themes, shortcut recording, immediate saves through the parent tray process, CLI overrides shown disabled, and duplicate shortcut errors with the toggle restored to Off and the previous configuration retained.
- The 720-DIP compact layout and actual timed overlay preview were inspected. Shortcut recording names roundtrip through the existing hotkey parser.
- x64 and x86 MSIX builds validate the new Windows App Runtime framework dependency; ARM64 source checking passes. Signed installation, clean-machine runtime acquisition, other Windows versions, and physical mixed-DPI monitors still need release testing.

- Follow-up to `Screenshot 2026-10-28 191534.png`: verified that preview images show the current desktop and dimmed previous arrow. Added coverage for visible glyph size/padding at six DPIs, shared icon selection at first/last desktops, and alpha conversion into the WinUI preview bitmap.

## 2026-10-28 UI updates

- Formatting and Clippy with warnings denied; 17 tests pass on x64 and x86 (the opt-in desktop-switch integration test stays ignored). ARM64 source check passes.
- Preset tests cover all six dimension/text keys, numeric compatibility, mixed/custom sizes, CLI precedence, and invalid values. Overlay tests cover work-area positioning at 150% DPI, border pixels, rounded-corner alpha, opacity, and independent status appearance.
- Native WinUI checks use a separate APPDATA directory: dark and light themes, 720-DIP compact layout, right-aligned switches, group disabling, command-line override information, and the About page.
- Font edits save without a button. The multiline editor keeps all configuration lines, rejects invalid input without touching the saved file, and automatically saves valid presets and custom values.
- x64 and x86 release packages pass MakeAppx and native-payload validation. ARM64 packaging still requires the ARM64 MSVC linker toolchain.

## Tray identities, reset and enlarged preview

- 20 tests pass on x64 and x86; Clippy and formatting pass. Tests freeze a representative tray UUID, check distinct roles and executable locations, and check unique, valid UTF-16 tooltips even with very long emoji names.
- Read-only inspection of Windows 11 notification registrations confirms four distinct GUIDs for the test executable and the same four registrations after restarting that process. Final user placement remains controlled by Explorer.
- An isolated parent/Settings integration run verifies Reset removes a CLI theme override in the parent, restores the default, and enables the picker. Resetting a custom arrow pair changes both values together, updates tray pixels, removes the notice, and enables the dropdown.
- The desktop miniature enlarges labels, corner radii, and taskbar details by 2.25x while preserving configured positioning, colors and opacity; actual overlays and tray icons retain their configured sizes.

## Custom overlay sizing and automatic preview

- Formatting and Clippy with warnings denied pass; 21 tests pass and the live desktop-switch test remains opt-in. Preview routing coverage checks switching settings, unchanged values, unrelated status settings, editor-only state and CLI precedence.
- The x64 release package passes MakeAppx and native-payload validation. The updated portable app was relaunched with the existing configuration preserved; Windows confirmed displaying the completion notification.
- Interactive checks for Custom width/height, Reset, persistence and the 500 ms preview debounce are handed to the user at their request. Automated UI testing is no longer part of the workflow unless requested.

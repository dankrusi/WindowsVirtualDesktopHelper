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

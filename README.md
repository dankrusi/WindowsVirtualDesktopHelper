# Windows Virtual Desktop Helper

A small, native Windows app for virtual desktop tray indicators, overlays and global hotkeys.

Version 3 replaces the C#/.NET application with Rust, with WinUI 3 Settings and a lightweight Win32 tray process. It runs from a single executable without .NET or a browser engine. Settings requires Microsoft Windows App Runtime 2.4 or later (the `Microsoft.WindowsAppRuntime.2` framework). Existing v2 configuration keys and command-line flags are retained.

## Features

- Desktop number and name initial in the tray, with previous/next arrows and Task View access.
- Desktop-name switch notifications and a permanent, click-through status overlay, including multi-monitor placement.
- Custom shortcuts for numbered desktops, next/previous and the two most recently visited desktops.
- Digit, numpad and function-key shortcuts; optional restoration of the previous focused window.
- Fluent WinUI 3 settings cards, responsive navigation, light/dark themes, shortcut recording and actual overlay previews.
- Portable autostart or Windows-managed MSIX startup.
- Event-driven desktop/focus/theme updates, with automatic recovery after Explorer restarts.

The desktop integration uses undocumented Windows interfaces. Legacy Windows 10 desktop API profiles are retained, but this WinUI build targets Windows 11 (22000 or later). New Windows/Insider builds may require an updated desktop API profile. See [architecture](docs/architecture.md) and [validation](docs/validation.md).

## Build and run

Install Rust 1.95 or later with the MSVC toolchain, Visual Studio C++ Build Tools, and a Windows 10/11 SDK.

```powershell
cargo build --release --locked
.\target\release\WindowsVirtualDesktopHelper.exe
.\target\release\WindowsVirtualDesktopHelper.exe --settings
.\scripts\check.ps1
```

Right-click a tray icon for Settings, diagnostics/configuration access, About, Donate or Exit. Closing Settings keeps the app running. If all indicator features are disabled, a small app icon keeps the menu accessible.

Release binaries statically link the CRT. The two direct dependencies are Microsoft's `windows-sys` and `windows-reactor` (pinned to 0.100.0). The latter is Microsoft's early Rust binding layer over actual WinUI 3 controls; it loads only when Settings opens in a child process. Install the matching architecture of [Windows App Runtime](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads) for portable builds. Build output is in `target/`; distributable artifacts are in `dist/`.

## Installers

The new installer format is MSIX, opened by Windows App Installer:

```powershell
.\scripts\build-msix.ps1 -Architecture x64
# Also supports x86 and arm64.
```

This produces an unsigned development MSIX, portable ZIP, Store upload archive and SHA-256 checksums. Local MSIX installation requires signing with a trusted certificate whose subject matches the package Publisher. Building does not install or trust certificates. See [MSIX packaging and signing](docs/msix.md).

Published [v2.1 releases](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/tag/v2.1) and the historical Scoop manifest refer to the previous .NET application. A v3 build in this repository is not a published release.

## Configuration

Existing files remain in `%APPDATA%\WindowsVirtualDesktopHelper\*.config`. The main file is `WindowsVirtualDesktopHelper.exe.config`. All 61 original settings are preserved. Transparent tray backgrounds are enabled by the new `theme.icons.transparentBackground` setting; set it to `false` to use the legacy background colors. Additional files load alphabetically before the main file. Blank lines and `#` comment lines are ignored.

```text
feature.showDesktopStatusOverlay: true
hotkeys.work: "Ctrl + Alt + D1 = Desktop1"
hotkeys.previous: "Alt + Tilde = PreviousDesktop"
```

Command-line settings take priority and are not saved:

```powershell
WindowsVirtualDesktopHelper.exe --general.theme dark --theme.overlay.overlayBG.dark "red"
```

Use Settings → Advanced for the complete configuration, including arbitrary custom hotkeys. Toggles and pickers save immediately; text fields and the configuration editor have explicit Save buttons. Invalid changes and occupied shortcuts display inline errors and leave the previous configuration active. Command-line overrides appear disabled. Portable startup changes immediately when toggled; MSIX startup is managed through the Windows Startup Apps settings link.

See [settings](docs/Settings.md), [hotkeys](docs/Hotkeys.md), and [actions](docs/Actions.md). `--diagnostics --diagnostics.output path.txt` performs a read-only COM connectivity check.

## Contributing

Run `scripts/check.ps1` before submitting changes. CI runs format/lint/tests and builds MSIX packages for x64, x86 and ARM64. Follow the [manual compatibility checks](docs/validation.md) for Windows integration changes.

The original C# implementation remains available in Git history. Third-party notices are retained in `licenses/`. See [CHANGELOG.md](CHANGELOG.md), [LICENSE.md](LICENSE.md) and [PRIVACY.md](PRIVACY.md).

Thanks to Dan Krusi, MScholtes, Flaflo, zgdump, and all [contributors](https://github.com/dankrusi/WindowsVirtualDesktopHelper/graphs/contributors).

[Donate via PayPal](https://www.paypal.com/donate/?hosted_button_id=BG5FYMAHFG9V6)

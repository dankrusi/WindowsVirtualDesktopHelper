# Windows Virtual Desktop Helper

A small, native Windows app for virtual desktop tray indicators, overlays and global hotkeys.

Version 3 replaces the C#/.NET application with Rust and native Win32 UI. It runs as a single executable and does not require .NET or a browser runtime. Existing v2 configuration keys and command-line flags are retained.

## Features

- Desktop number and name initial in the tray, with previous/next arrows and Task View access.
- Desktop-name switch notifications and a permanent, click-through status overlay, including multi-monitor placement.
- Custom shortcuts for numbered desktops, next/previous and the two most recently visited desktops.
- Digit, numpad and function-key shortcuts; optional restoration of the previous focused window.
- Reusable native settings controls, light/dark themes, live vector preview and an actual overlay preview.
- Portable autostart or Windows-managed MSIX startup.
- Event-driven desktop/focus/theme updates, with automatic recovery after Explorer restarts.

The desktop integration uses undocumented Windows interfaces. Windows 10 and Windows 11 profiles are included; new Windows/Insider builds may require an updated profile. Windows 7/8 are unsupported. See [architecture](docs/architecture.md) and [validation](docs/validation.md).

## Build and run

Install Rust's MSVC toolchain, Visual Studio C++ Build Tools, and a Windows 10/11 SDK.

```powershell
cargo build --release --locked
.\target\release\WindowsVirtualDesktopHelper.exe
.\target\release\WindowsVirtualDesktopHelper.exe --settings
.\scripts\check.ps1
```

Right-click a tray icon for Settings, diagnostics/configuration access, About, Donate or Exit. Closing Settings keeps the app running. If all indicator features are disabled, a small app icon keeps the menu accessible.

Release binaries statically link the CRT. The only direct dependency is Microsoft's `windows-sys` (plus its `windows-link` support crate). Build output is in `target/`; distributable artifacts are in `dist/`.

## Installers

The new installer format is MSIX, opened by Windows App Installer:

```powershell
.\scripts\build-msix.ps1 -Architecture x64
# Also supports x86 and arm64.
```

This produces an unsigned development MSIX, portable ZIP, Store upload archive and SHA-256 checksums. Local MSIX installation requires signing with a trusted certificate whose subject matches the package Publisher. Building does not install or trust certificates. See [MSIX packaging and signing](docs/msix.md).

Published [v2.1 releases](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/tag/v2.1) and the historical Scoop manifest refer to the previous .NET application. A v3 build in this repository is not a published release.

## Configuration

Existing files remain in `%APPDATA%\WindowsVirtualDesktopHelper\*.config`. The main file is `WindowsVirtualDesktopHelper.exe.config`. All 61 original defaults are preserved. Additional files load alphabetically before the main file. Blank lines and `#` comment lines are ignored.

```text
feature.showDesktopStatusOverlay: true
hotkeys.work: "Ctrl + Alt + D1 = Desktop1"
hotkeys.previous: "Alt + Tilde = PreviousDesktop"
```

Command-line settings take priority and are not saved:

```powershell
WindowsVirtualDesktopHelper.exe --general.theme dark --theme.overlay.overlayBG.dark "red"
```

Use Settings → Advanced for the complete configuration, including arbitrary custom hotkeys. Changes take effect on Apply; Close discards unapplied changes. Hotkey registration conflicts are reported and other shortcuts continue working. Portable startup is changed when its setting is applied; MSIX startup is managed through the Windows Startup Apps settings link.

See [settings](docs/Settings.md), [hotkeys](docs/Hotkeys.md), and [actions](docs/Actions.md). `--diagnostics --diagnostics.output path.txt` performs a read-only COM connectivity check.

## Contributing

Run `scripts/check.ps1` before submitting changes. CI runs format/lint/tests and builds MSIX packages for x64, x86 and ARM64. Follow the [manual compatibility checks](docs/validation.md) for Windows integration changes.

The original C# implementation remains available in Git history. Third-party notices are retained in `licenses/`. See [CHANGELOG.md](CHANGELOG.md), [LICENSE.md](LICENSE.md) and [PRIVACY.md](PRIVACY.md).

Thanks to Dan Krusi, MScholtes, Flaflo, zgdump, and all [contributors](https://github.com/dankrusi/WindowsVirtualDesktopHelper/graphs/contributors).

[Donate via PayPal](https://www.paypal.com/donate/?hosted_button_id=BG5FYMAHFG9V6)

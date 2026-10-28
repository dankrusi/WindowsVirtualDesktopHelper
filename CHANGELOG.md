# 3.0.0 (unreleased)

- Replace C#/.NET and MSI with native Rust/Win32 and MSIX packaging.
- Preserve all 61 original configuration keys, CLI overrides, desktop actions and custom hotkeys.
- Replace desktop/focus/theme polling with registry notifications, foreground hooks and window messages.
- Replace the hand-drawn Settings window with actual WinUI 3 navigation and cards, automatic saving, inline errors, shortcut recording and overlay previews.
- Use a compact 480 x 128 DIP switch notification by default; preserve explicitly configured sizes.
- Render transparent, antialiased tray icons at taskbar DPI, update them in place, and scale overlays per monitor.
- Require Windows 11 and Microsoft Windows App Runtime 2.4+ for the modern WinUI build.
- Use stable desktop IDs for history; reconnect after Explorer restarts.
- Add Cargo checks, native PE/package validation and x64/x86/ARM64 CI builds.

# Unreleased

## Changes

* Add unsigned x64/x86 APPX packaging, Store upload archives, build/validation scripts, and a GitHub Actions pipeline.
* Apply and validate the reserved Store app name across package, tile, and startup-task display names.
* Use the manifest startup task and Windows startup settings in packaged builds.


# v2.1 - 2026-10-28

The v2.1 executable and MSI installer are unsigned. Windows may display an untrusted publisher warning.

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

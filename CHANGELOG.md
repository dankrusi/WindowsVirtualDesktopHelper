# v1.18 (not yet released)

## Features
* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Jump to desktop using hot keys
* Autostart with Windows
* Simple and lightweight
* Fully configurable
* Free

## Fixes
* Improvements to last focused window detection and restoration

## Changes
* New powerful configuration system with lots of new configuration options via the config file or command line
* Support theming of icons (fonts, colors)
* Support theming of overlays (fonts, colors)

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
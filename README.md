# Windows Virtual Desktop Helper

Simple and lightweight app to help with Virtual Desktops for Windows 10 and Windows 11.

![Screenshot](Images/WindowsVirtualDeskopHelper%20Screenshot.png)

[Download v1.17 Setup (.msi)](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/download/v1.17/WindowsVirtualDesktopHelper.Setup.v1.17.msi) | 
[Download v1.17 Executable (.zip)](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/download/v1.17/WindowsVirtualDesktopHelper.Executable.v1.17.zip)

Windows comes builtin with virtual desktops, however some important features are missing, such
as displaying which desktop you are on when switching. Windows Virtual Desktop Helper helps
fix some of these missing features.

Keywords: Virtual Desktop indicator, Virtual Desktop name, Virtual Desktop number

Note: Currently Windows Virtual Desktop Helper is not code-signed, and may be reported as untrusted by Windows
Defender or other anti-virus applications. Typically, after enough users download, install, and report
the software as okay/safe, this warning will go away.



## ⚡ Features

* Show desktop number in notification area
* Show desktop name when switching desktops
* Show prev/next desktop by clicking icons in notification area
* Show desktop initial in notification area
* Jump to desktop using hot keys
* Autostart with Windows
* Simple and lightweight
* Configurable
* Free

![Settings](Images/WindowsVirtualDeskopHelper%20Settings.png)



## 🚀 Installation

### Requirements

Windows Virtual Desktop Helper needs the Microsoft .NET Framework 4.7 or higher in order to run. Most likely you already have this installed, otherwise you can get it from [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet-framework)

### Setup

You can install Windows Virtual Desktop Helper to your system using the setup program.

[Download WindowsVirtualDesktopHelper Setup v1.17.msi](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/download/v1.17/WindowsVirtualDesktopHelper.Setup.v1.17.msi)

Note: Currently Windows Virtual Desktop Helper is not code-signed, and may be reported as malware by Windows
Defender or other anti-virus applications. Typically, after enough users download, install, and report
the software as okay/safe, this malware warning will go away. If you prefer to avoid some of these issues, use the Zip version of the executable instead.

### Executable

You can just run the executable file WindowsVirtualDesktopHelper.exe to use Windows Virtual Desktop Helper.

[Download WindowsVirtualDesktopHelper Executable v1.17.zip](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/download/v1.17/WindowsVirtualDesktopHelper.Executable.v1.17.zip)

### Scoop

A command-line installer for Windows

[https://scoop.sh/](https://scoop.sh/)

```scoop bucket add extras```

```scoop install windows-virtualdesktop-helper```



## 🎁 Donate

Do you like Windows Virtual Desktop Helper? 

[Donate via PayPal](https://www.paypal.com/donate/?hosted_button_id=BG5FYMAHFG9V6)



## 💻 Compatibility

Currently this app is compatible with Windows 10 and Windows 11. 
Microsoft is constantly changing their API for the Virtual Desktops, so it appears not all builds of Windows are fully working yet.
Please note that if you use bleeding-edge insider buildes, this app may not yet be compatible.

The following versions are not supported:

* Windows 7
* Windows 8
* Windows Server
  
If you know of a working version or non-working version, please report it.



## 🗺️ Roadmap

While the goal of this app is to remain simple and lightweight, there are some features that will still be added:

* Settings: more configuration options
* Console-Mode: allow a console-mode which can be used by scripts

Technical Roadmap:

- Refactor settings from .net settings loader to own system (the .net system is so bloated and crappy)
- Add support for setting settings via command line
- Split out settings UI and app UI
- =============
- Make features more modular

 

## 📜 Changelog

See [CHANGELOG.md](https://github.com/dankrusi/WindowsVirtualDesktopHelper/blob/main/CHANGELOG.md)



## 🤔 Frequently Asked Questions

### Help! This App doesn't work on my version of Windows 11

Microsoft is constantly changing the Windows APIs for managing virtual desktops - thus usually with each new version we have to write a new API wrapper to interface with Windows. This takes time, and sometimes is not easy to integrate with. Don't expect immediate support for un-released versions of Windows (like Insider).

### Where is the icon showing the screen number?

Windows automatically organizes the notification area icons, and places new ones in the icons menu under the ^ chevron. You can drag the screen number into the main icon area from there to have it always showing.

### Why are the prev/next screen icons in the wrong order?

Windows automatically organizes and orders the notification area icons. You can drag prev/next screen icons to organize them accordingly.

### Why is this app is being reported by Windows Defender as untrusted?

Currently Windows Virtual Desktop Helper is not code-signed, and thus may be reported as untrusted. With USD 70 donations per year, the app will be signed.

### Why is this app is being reported by Windows Defender as malware?

Currently Windows Virtual Desktop Helper is not code-signed, and due to its installation option for installing to autostart, may be flagged as malware. Typically, after enough users download, install, and report
the software as okay/safe, this malware warning will go away. With USD 70 donations per year, the app will be signed.

### Why is the app based on the older .NET 4.7?

The idea is to make the app as easy and lightweight to run as possible. Most systems have some version of .NET installed, thus we use a low version to cover as many users as possible. 



## ⚙️ Configuration

The most common and basic settings can be configured using the GUI Settings. 

For more advanced configurations and features you can use the config file or command line arguments (as of v1.18).

### Settings

|Config|Default|Description|
| --- | --- | --- |
| general.startupWithWindows | ``false`` | If true, the app will register itself with Windows to startup when Windows starts (via the registry). |
| general.theme | ``"auto"`` | Can be either auto, dark or light. If set to auto, the theme is derived from the current windows theme (dark or light). |
| theme.icons.disabledOpacity | ``"0.5"`` | Defines the opacity to use for icons which are disabled. |
| theme.icons.font | ``"Segoe UI"`` | Defines the font name to use for the icons (for regular numbers, characters). |
| theme.icons.emojiFont | ``"Segoe UI Symbol"`` | Defines the font name to use for emoji icons. |
| theme.icons.symbolsFont | ``"Segoe UI Symbol"`` | Defines the font name to use for symbol icons. |
| theme.icons.iconBG.dark | ``"black"`` |  |
| theme.icons.iconFG.dark | ``"white"`` |  |
| theme.icons.iconBG.light | ``"white"`` |  |
| theme.icons.iconFG.light | ``"black"`` |  |
| theme.overlay.width | ``900`` | With width in pixels of the overlay. |
| theme.overlay.height | ``430`` | With height in pixels of the overlay. |
| theme.overlay.font | ``"Segoe UI Light"`` | Defines the font name to use for the overlay. |
| theme.overlay.fontSize | ``30`` | Defines the font size to use for the overlay. |
| theme.overlay.overlayBG.dark | ``"black"`` |  |
| theme.overlay.overlayFG.dark | ``"white"`` |  |
| theme.overlay.overlayBG.light | ``"black"`` |  |
| theme.overlay.overlayFG.light | ``"white"`` |  |
| feature.showSplashScreen | ``true`` | If enabled, a splash screen is shown on startup of the app. Overlays must be enabled. |
| feature.showSplashScreen.duration | ``2000`` | Splash duration in milliseconds. |
| feature.showSplashScreen.text | ``"Virtual Desktop Helper"`` | The splash text to show. |
| feature.showPrevNextIcons | ``true`` | If enabled, a previous and next arrow will appear in the icons tray of Windows to allow easy switching between desktops. |
| feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds | ``false`` | If enabled, the prev/next icon will automatically hide if there is no prev/next desktop. |
| feature.showPrevNextIcons.nextChar | ``"\u203A"`` | Defines the character to use for next desktop icon (typically a unicode character like the chevron, for example \xE101 = skip forward (player style),  = next (arrow style), \xe26b = next (chevron style), \u02C3 = next (chevron style), \u203A = next (chevron style)) |
| feature.showPrevNextIcons.prevChar | ``"\u2039"`` | Defines the character to use for prev desktop icon (typically a unicode character like the chevron, for example \xE100 = skip back (player style), \xE112 = previous (arrow style), \xe26c = previous (chevron style), \u02C2 = previous (chevron style), \u2039 = previous (chevron style)) |
| feature.showDesktopSwitchOverlay | ``true`` |  |
| feature.showDesktopSwitchOverlay.duration | ``2000`` | Defines the duration in milliseconds for a switch overlay to show. If set to zero, then the overlay is shown indefinately. |
| feature.showDesktopSwitchOverlay.animate | ``true`` |  |
| feature.showDesktopSwitchOverlay.translucent | ``true`` |  |
| feature.showDesktopSwitchOverlay.showOnAllMonitors | ``true`` |  |
| feature.showDesktopSwitchOverlay.position | ``"middlecenter"`` |  |
| feature.useHotKeysToJumpToDesktop | ``false`` |  |
| feature.useHotKeysToJumpToDesktop.hotkey | ``"Alt+H"`` |  |
| feature.showDesktopNumberInIconTray | ``true`` |  |
| feature.showDesktopNumberInIconTray.clickToOpenTaskView | ``true`` |  |
| feature.showDesktopNameInIconTray | ``false`` |  |

### Config File

This is located in ``%appdata%\WindowsVirtualDesktopHelper`` (for example ``C:\Users\<USER>\AppData\Roaming\WindowsVirtualDesktopHelper``)
as a ``.config`` file, and can be edited with any text editor.

### Command Line Arguments

The app can be run with command line arguments to specificy any configuration setting. For example, one could
run the app with the following command line arguments:

```
WindowsVirtualDesktopHelper.exe --theme.overlay.overlayBG.dark "red" --feature.showPrevNextIcons.nextChar "]" --feature.showPrevNextIcons.prevChar "["
```

Command line arguments take precedence over the config file settings.



## 🔧 How it works

This program works by using some unofficial/undocumented Windows APIs which Windows uses internally to manage the desktop.
The unofficial nature of these APIs is very unfortunate, because it means that each time Windows 11 makes an update, we have
to reverse engineer the APIs and their undocumented COM CLSIDs - which is tedious and wastes a lot of time.

This is why the maintainers are reluctant to add too many other features, because its enough of a task to keep
up with all the Windows updates for the basic features.



## ⚒ Building & Contributing

Install Visual Studio 2022 or later with ".NET desktop development" feature set, and open the solution file WindowsVirtualDesktopHelper.sln. You can then build the project.

Note: The Setup project which creates the MSI installer will require the following extension to be installed: [Microsoft Visual Studio Installer Projects 2022](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2022InstallerProjects)

### Depencencies

This project doesn't have any dependencies or external libraries, and we expect it to stay this way. We want the app
to remain a single .exe file which can be run without any installation or dependencies.

### The Hunt for CLSIDs

TODO

### Your help is wanted!

TODO



## 🙏 Thanks

Many thanks for the original API work done by [MScholtes](https://github.com/MScholtes) and contributions by [Flaflo](https://github.com/Flaflo).

Thanks to the following contributors:
 - [y2nd66](https://github.com/y2nd66)
 - [SleepyBag](https://github.com/SleepyBag)
 - [yossdev](https://github.com/yossdev)
 - [MadoScientist97](https://github.com/MadoScientist97)
 - [...and more](https://github.com/dankrusi/WindowsVirtualDesktopHelper/pulls?q=is%3Apr+is%3Aclosed)


	 
## 🤩 You might also like...

### Windows Taskbar Helpers

A free utility to make the Windows 11 Taskbar just a little bit more useful by allowing you to pin quick launchers that always open a new folder, app or anything.

![Screenshot](https://raw.githubusercontent.com/dankrusi/WindowsTaskbarHelpers/main/Images/WindowsTaskbarHelpers_Screenshots_v1.png)(https://github.com/dankrusi/WindowsTaskbarHelpers)

[github.com/dankrusi/WindowsTaskbarHelpers](https://github.com/dankrusi/WindowsTaskbarHelpers)
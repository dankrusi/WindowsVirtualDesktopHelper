# Windows Virtual Desktop Helper

Back to [Home](https://github.com/dankrusi/WindowsVirtualDesktopHelper)

## Settings Documentation

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
| feature.useHotKeyToJumpToDesktopNumber | ``false`` |  |
| feature.useHotKeyToJumpToDesktopNumber.hotkey | ``"Alt"`` |  |
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

### Custom Hotkey Settings

See [Hotkeys Documentation](https://github.com/dankrusi/WindowsVirtualDesktopHelper/blob/main/Documentation/Hotkeys.md)
for more information on how to define custom hotkeys.
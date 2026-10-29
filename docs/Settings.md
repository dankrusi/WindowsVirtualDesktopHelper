# Windows Virtual Desktop Helper

Back to [Home](https://github.com/dankrusi/WindowsVirtualDesktopHelper)

## Settings Documentation

(Rust / WinUI 3 version)

### Size presets and overlay appearance

The core accepts `"small"`, `"medium"`, and `"large"` in configuration files and command-line overrides. Existing numeric dimensions and fractional font sizes remain supported. Both overlay Size pickers offer Small, Medium, Large and Custom. Custom reveals independent width and height sliders with live numeric values; Reset returns both dimensions to Medium. The Custom choice persists across restarts, including when its dimensions happen to match a preset. Text size remains independent; custom font sizes show an information box with Reset to re-enable the picker. Command-line overrides can also be reset for the current run; the original launch arguments apply again after a full restart.

| Key | Small | Medium | Large |
| --- | ---: | ---: | ---: |
| `theme.overlay.width` | 320 | 480 | 640 |
| `theme.overlay.height` | 96 | 128 | 176 |
| `theme.overlay.fontSize` | 16 | 20 | 28 |
| `theme.status.width` | 180 | 250 | 340 |
| `theme.status.height` | 32 | 40 | 56 |
| `theme.status.fontSize` | 10 | 12 | 16 |

Dimensions are device-independent pixels; text sizes are points. Each monitor's DPI determines the actual window size. Medium matches the existing defaults.

`theme.overlay.customSize` and `theme.status.customSize` (both default `false`) remember an explicit Custom selection. They do not change rendering: width and height remain authoritative. Numeric combinations outside the presets automatically show Custom. Sliders normally cover 1–1920 DIP wide and 1–1080 DIP high; larger existing values extend the range and are preserved.

```text
theme.overlay.width: "large"
theme.overlay.height: "large"
theme.overlay.fontSize: "medium"
theme.overlay.offset: 24
theme.overlay.border: true
theme.overlay.roundedCorners: true
```

Both `theme.overlay` and `theme.status` accept `offset` (default `0`), `border` (default `false`), and `roundedCorners` (default `true`, 8 DIP radius). Position and spacing use the monitor work area, keeping labels clear of the taskbar. Both `feature.showDesktopSwitchOverlay.translucent` and `feature.showDesktopStatusOverlay.translucent` use 60% opacity when enabled; disabling them makes the entire label opaque.

The previous/next symbol picker updates both existing `prevChar` and `nextChar` settings together. Custom symbol pairs remain supported through configuration. Tray icons are transparent by default; the legacy `theme.icons.transparentBackground: false` setting remains available in configuration only.

Settings controls save automatically. Changes to the switching-desktop overlay automatically preview on the desktop after 500 ms without further changes. The preview follows the configured display time; a display time of zero uses a temporary 2.2-second preview. Disabling the feature cancels pending previews and clears the current one. The Preview button is also available for both overlays. Command-line overrides are never written into the configuration file.

### Configuration editor

Advanced → Configuration editor → Open editor opens a separate, resizable WinUI window. Its multiline draft is independent of the live settings. Use **Save** or **Ctrl+S** to validate and apply the complete edit. Errors leave the draft and previous settings intact; closing an unsaved document offers Save, Discard or Keep editing. Repeatedly choosing Open editor activates the existing window.

Saving does not replace the textbox contents, preserving its native selection and undo history. The editor shows file values and defaults, excluding command-line overrides. Changes made through other Settings controls while the editor is open are retained; conflicting changes to the same key are reported rather than silently overwritten.

### Settings

|Config|Default|Description|
| --- | --- | --- |
| debug.singleInstance | ``true`` | If true, the app will prevent multiple instances of the app from starting.  Most users won't need to change this option. |
| general.startupWithWindows | ``false`` | If true, the app will register itself with Windows to startup when Windows starts (via the registry). |
| general.theme | ``"auto"`` | Can be either auto, dark or light. If set to auto, the theme is derived from the current windows theme (dark or light). |
| theme.icons.disabledOpacity | ``"0.5"`` | Defines the opacity to use for icons which are disabled. |
| theme.icons.font | ``"Segoe UI"`` | Defines the font name to use for the icons (for regular numbers, characters). If a specific style is to be used, then one can append 'Bold', 'Italic', 'Regular' after a comma and the font name - for example 'Arial, Bold'. |
| theme.icons.emojiFont | ``"Segoe UI Symbol"`` | Defines the font name to use for emoji icons. |
| theme.icons.symbolsFont | ``"Segoe UI Symbol"`` | Defines the font name to use for symbol icons. |
| theme.icons.iconBG.dark | ``"black"`` |  |
| theme.icons.iconFG.dark | ``"white"`` |  |
| theme.icons.iconBG.light | ``"white"`` |  |
| theme.icons.iconFG.light | ``"black"`` |  |
| theme.overlay.width | ``480`` | Width in device-independent pixels, or a size preset. |
| theme.overlay.height | ``128`` | Height in device-independent pixels, or a size preset. |
| theme.overlay.font | ``"Segoe UI Light"`` | Defines the font name to use for the switch overlay. |
| theme.overlay.fontSize | ``20`` | Text size in points, or a size preset. |
| theme.overlay.overlayBG.dark | ``"black"`` |  |
| theme.overlay.overlayFG.dark | ``"white"`` |  |
| theme.overlay.overlayBG.light | ``"black"`` |  |
| theme.overlay.overlayFG.light | ``"white"`` |  |
| theme.status.width | ``250`` | With width in pixels of the status overlay. |
| theme.status.height | ``40`` | With height in pixels of the status overlay. |
| theme.status.offset | ``0`` | Distance from the work-area edge in device-independent pixels. |
| theme.status.font | ``"Segoe UI Light"`` | Defines the font name to use for the status overlay. |
| theme.status.fontSize | ``12`` | Defines the font size to use for the status overlay. |
| theme.status.overlayBG.dark | ``"black"`` |  |
| theme.status.overlayFG.dark | ``"white"`` |  |
| theme.status.overlayBG.light | ``"black"`` |  |
| theme.status.overlayFG.light | ``"white"`` |  |
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
| feature.showDesktopStatusOverlay | ``false`` |  |
| feature.showDesktopStatusOverlay.animate | ``true`` |  |
| feature.showDesktopStatusOverlay.translucent | ``true`` |  |
| feature.showDesktopStatusOverlay.showOnAllMonitors | ``true`` |  |
| feature.showDesktopStatusOverlay.position | ``"topcenter"`` |  |
| feature.useHotKeyToJumpToDesktopNumber | ``false`` |  |
| feature.useHotKeyToJumpToDesktopNumber.useDigits | ``true`` | If enabled, the Jump To Desktop feature registers the Keyboard digits |
| feature.useHotKeyToJumpToDesktopNumber.useNumpadDigits | ``true`` | If enabled, the Jump To Desktop feature registers the Numpad digits |
| feature.useHotKeyToJumpToDesktopNumber.useFunctionKeys | ``false`` | If enabled, the Jump To Desktop feature registers the corresponding Function keys |
| feature.useHotKeyToJumpToDesktopNumber.hotkey | ``"Alt"`` |  |
| feature.useHotKeyToJumpToPreviousDesktop | ``false`` | Enable a hotkey to return to the most recently used desktop. Repeated presses toggle between the two most recent desktops. |
| feature.useHotKeyToJumpToPreviousDesktop.hotkey | ``"Alt + Tilde"`` | Shortcut for the most recent desktop. The Settings UI offers Tab or backtick (Tilde); custom keys can be set here. |
| feature.useHotKeyToSwitchDesktopForward | ``false`` |  |
| feature.useHotKeyToSwitchDesktopForward.hotkey | ``"Alt + Right"`` |  |
| feature.useHotKeyToSwitchDesktopBackward | ``false`` |  |
| feature.useHotKeyToSwitchDesktopBackward.hotkey | ``"Alt + Left"`` |  |
| feature.showDesktopNumberInIconTray | ``true`` |  |
| feature.showDesktopNumberInIconTray.clickToOpenTaskView | ``true`` |  |
| feature.showDesktopNameInIconTray | ``false`` |  |

### Config File

This is located in ``%appdata%\WindowsVirtualDesktopHelper`` (for example ``C:\Users\<USER>\AppData\Roaming\WindowsVirtualDesktopHelper``)
as a ``.config`` file, and can be edited with any text editor.

Note: configuration lines that start with ``#`` are comments and ignored by the configuration system.

### Command Line Arguments

The app can be run with command line arguments to specificy any configuration setting. For example, one could
run the app with the following command line arguments:

```
WindowsVirtualDesktopHelper.exe --theme.overlay.overlayBG.dark "red" --feature.showPrevNextIcons.nextChar "]" --feature.showPrevNextIcons.prevChar "["
```

Command line arguments take precedence over the config file settings.

### Custom Hotkey Settings

See [Hotkeys Documentation](https://github.com/dankrusi/WindowsVirtualDesktopHelper/blob/main/docs/Hotkeys.md)
for more information on how to define custom hotkeys.

## Tray transparency and sizing (v3)

`theme.icons.transparentBackground` defaults to `true`. Tray glyphs use premultiplied alpha and scale to the taskbar DPI; disabled arrows reduce opacity. Set this flag to `false` to restore the configured `theme.icons.iconBG.light` / `.dark` backgrounds. Tray colors follow the Windows taskbar theme independently of the Settings window theme.

The default switch notification is a compact 480 x 128 DIP banner with 20-point text; explicitly configured dimensions are preserved. Overlay width, height and edge offset are device-independent pixels; font sizes are points. Each monitor receives its own scaled overlay, constrained to its work area. Settings → Desktop overlays exposes the size controls and actual renderer previews.

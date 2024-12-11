# Windows Virtual Desktop Helper

Back to [Home](https://github.com/dankrusi/WindowsVirtualDesktopHelper)

## Hotkeys Documentation

(as of v2.0)

You can easily define custom hotkeys by configuring custom settings.

Any setting starting with ``hotkeys.`` is considered a hotkey setting.

For example, one can set a hotkey to jump to a specific desktop by setting the following setting:

```
hotkeys.myCustomHotkey1 = "Ctrl + Alt + Shift + D1 = Desktop1"
hotkeys.myCustomHotkey2 = "Ctrl + Alt + Shift + D2 = Desktop2"
```

or use a custom shortcut for prev/next desktop:

```
hotkeys.myCustomKey1: "Alt + W = DesktopForward"
hotkeys.myCustomKey2: "Alt + Q = DesktopBackward"
```

Hotkey settings always define an action:
- ``Desktop1``
- ``Desktop2``
- ``Desktop3...``
- ``DesktopForward``
- ``DesktopBackward``
- ``PreviousDesktop``

See [Actions Documentation](https://github.com/dankrusi/WindowsVirtualDesktopHelper/blob/main/Documentation/Actions.md)
for more information on actions.

Note: A hotkey MUST have at least one modifier key (Ctrl, Alt, Shift, Win) and one regular key (A-Z, 0-9, etc).

### Hotkeys

#### Modifiers

- ``Ctrl``
- ``Alt``
- ``Shift``
- ``Win``

#### Regular Keys

- ``LButton``
- ``RButton``
- ``Cancel``
- ``MButton``
- ``XButton1``
- ``XButton2``
- ``Back``
- ``Tab``
- ``LineFeed``
- ``Clear``
- ``Return``
- ``Enter``
- ``Menu``
- ``Pause``
- ``Capital``
- ``CapsLock``
- ``KanaMode``
- ``HanguelMode``
- ``HangulMode``
- ``JunjaMode``
- ``FinalMode``
- ``HanjaMode``
- ``KanjiMode``
- ``Escape``
- ``IMEConvert``
- ``IMENonconvert``
- ``IMAccept``
- ``IMEAceept``
- ``IMEModeChange``
- ``Space``
- ``Prior``
- ``PageUp``
- ``Next``
- ``PageDown``
- ``End``
- ``Home``
- ``Left``
- ``Up``
- ``Right``
- ``Down``
- ``Select``
- ``Print``
- ``Execute``
- ``Snapshot``
- ``PrintScreen``
- ``Insert``
- ``Delete``
- ``Help``
- ``D0``
- ``D1``
- ``D2``
- ``D3``
- ``D4``
- ``D5``
- ``D6``
- ``D7``
- ``D8``
- ``D9``
- ``A``
- ``B``
- ``C``
- ``D``
- ``E``
- ``F``
- ``G``
- ``H``
- ``I``
- ``J``
- ``K``
- ``L``
- ``M``
- ``N``
- ``O``
- ``P``
- ``Q``
- ``R``
- ``S``
- ``T``
- ``U``
- ``V``
- ``W``
- ``X``
- ``Y``
- ``Z``
- ``Apps``
- ``Sleep``
- ``NumPad0``
- ``NumPad1``
- ``NumPad2``
- ``NumPad3``
- ``NumPad4``
- ``NumPad5``
- ``NumPad6``
- ``NumPad7``
- ``NumPad8``
- ``NumPad9``
- ``Multiply``
- ``Add``
- ``Separator``
- ``Subtract``
- ``Decimal``
- ``Divide``
- ``F1``
- ``F2``
- ``F3``
- ``F4``
- ``F5``
- ``F6``
- ``F7``
- ``F8``
- ``F9``
- ``F10``
- ``F11``
- ``F12``
- ``F13``
- ``F14``
- ``F15``
- ``F16``
- ``F17``
- ``F18``
- ``F19``
- ``F20``
- ``F21``
- ``F22``
- ``F23``
- ``F24``
- ``NumLock``
- ``Scroll``
- ``LMenu``
- ``RMenu``
- ``BrowserBack``
- ``BrowserForward``
- ``BrowserRefresh``
- ``BrowserStop``
- ``BrowserSearch``
- ``BrowserFavorites``
- ``BrowserHome``
- ``VolumeMute``
- ``VolumeDown``
- ``VolumeUp``
- ``MediaNextTrack``
- ``MediaPreviousTrack``
- ``MediaStop``
- ``MediaPlayPause``
- ``LaunchMail``
- ``SelectMedia``
- ``LaunchApplication1``
- ``LaunchApplication2``
- ``Semicolon``
- ``Plus``
- ``Comma``
- ``Minus``
- ``Period``
- ``Question``
- ``Tilde``
- ``OpenBrackets``
- ``Pipe``
- ``CloseBrackets``
- ``Quotes``
- ``Backslash``
- ``ProcessKey``
- ``Packet``
- ``Attn``
- ``Crsel``
- ``Exsel``
- ``EraseEof``
- ``Play``
- ``Zoom``
- ``NoName``
- ``Pa1``
- ``Clear`` (as v2.1)

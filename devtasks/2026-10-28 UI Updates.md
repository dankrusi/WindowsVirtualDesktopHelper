Improve the Settings UI:

- All screens: remove hints at the bottom regarding saving automatically etc
- Never require a save button, that is bad UX
- Allow a common method that when a setting is overridden by config/params etc that is not supporte by the UI, that there is a common info box that states so, and states what the value is, and the ui is disabled (for example in dropdown options or so)
- When disabling a feature that is grouped in settings, make sure to disable all the other settings fields as well to symbolize that those settings are unused (ie when toggling off "when switching desktops", all other settings in that group should be disbaled)
- toggle switches should be right-aligned (seem to be offset a little to the left)

Under General:
- improve the desktop appearance live preview to actually better represent the current settings. make it slightly larger, position the things correctly as they would be in real, include corner radius, transparency etc. represent the taskbar on positioning of tray icons

Under Tray icons:
- improve preview to reflect the settings
- remove transparent background ui option
- make previous and next symbol into a single option Previous/next symbol with dropdown of variants

Desktop overlays:
- add option for border toggle (both switching and always visible)
- add option for rounded corners (toggle) (both switching and always visible)
- add translucent option for always visible
- refactor all size and text to the proper "when switching desktops" and "always visible desktop name", consolidating into settings like Size: "small, medium, large", Text Size: "small, medium, large", distance from screen: [slider].

About Dialog: needs a lot of improvement, look at original c# version

In general, if settings where consolidated into logical size texts etc, like desktop notification size, also support this in the core as string in the config (like theme.overlay.width: "small") as well.

feel free to make more improvements as needed according to the style of this guide.

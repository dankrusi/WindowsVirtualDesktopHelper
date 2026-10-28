# Desktop actions

Use actions on the right side of a custom hotkey setting:

```text
hotkeys.work: "Ctrl + Alt + D1 = Desktop1"
```

| Action | Behavior |
| --- | --- |
| `Desktop1`, `Desktop2`, … | Switch to that one-based desktop number. |
| `DesktopForward` | Switch to the next desktop; stop at the last desktop. |
| `DesktopBackward` | Switch to the previous desktop; stop at the first desktop. |
| `PreviousDesktop` | Toggle between the two most recently visited desktops. |

Action names are case-insensitive. Invalid actions and zero/negative desktop numbers are rejected. Previous-desktop history is based on stable desktop IDs and is pruned when desktops are removed.

# Scoop

The public Extras bucket installs the app with:

```powershell
scoop bucket add extras
scoop install windows-virtualdesktop-helper
```

`windows-virtualdesktop-helper.json.template` is the source for our manifest. The release script runs `scripts/update-installers.ps1`, verifies all released ZIPs and their checksums, and commits `windows-virtualdesktop-helper.json`. The local manifest supports x64, x86 and ARM64. Its notes explain the Windows App Runtime requirement for Settings.

To regenerate independently:

```powershell
.\scripts\update-installers.ps1 --version 3.0.3
```

To test our manifest on a disposable Windows machine with Scoop installed:

```powershell
scoop install .\installers\scoop\windows-virtualdesktop-helper.json
```

Check the Start menu shortcut, Settings, upgrade and uninstall. App settings are stored in `%APPDATA%\WindowsVirtualDesktopHelper`, outside Scoop's version directory.

The [public Extras manifest](https://github.com/ScoopInstaller/Extras/blob/master/bucket/windows-virtualdesktop-helper.json) is maintained separately. Its autoupdate already follows our historical x64 ZIP filename and checksum sidecar. Keep `WindowsVirtualDesktopHelper.Executable.v<version>.zip` stable. A pull request to Extras is needed to adopt our extra architectures, runtime note, GPL-3.0-or-later license and Windows 11 description; a local commit does not change those upstream fields.

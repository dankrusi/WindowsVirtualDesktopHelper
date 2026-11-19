# WinGet

The Microsoft Store channel is already available:

```powershell
winget install --id 9N4HNXPRTSDC --exact --source msstore
```

This folder also maintains portable-package manifests for `DanKrusi.WindowsVirtualDesktopHelper`. They use the same GitHub release ZIPs as Scoop, include x64, x86 and ARM64, require Windows 11, and declare the Windows App Runtime 2.4+ dependency.

## Generate and validate

`scripts/release.ps1` regenerates and commits both package-manager manifests after publishing the release. CI includes them in its release artifact. To regenerate independently from an existing release:

```powershell
.\scripts\update-installers.ps1 --version 3.0.2
winget validate --manifest installers\winget\manifests\d\DanKrusi\WindowsVirtualDesktopHelper\3.0.2
```

The generator downloads each ZIP and checksum, verifies its SHA-256 and executable architecture, and requires the root executable and license. It only writes manifests after all three archives pass. `--asset-directory` accepts already downloaded release assets; `--output-directory` allows isolated tests. Templates live in `templates/`; edit those rather than generated YAML.

For an interactive installation test in Windows Sandbox or a disposable machine, enable local manifests from an elevated terminal, then install:

```powershell
winget settings --enable LocalManifestFiles
winget install --manifest installers\winget\manifests\d\DanKrusi\WindowsVirtualDesktopHelper\3.0.2
```

Check Settings, tray icons, upgrading from an earlier version and uninstalling. Close the app before upgrades. Automated manifest validation does not replace this installation test.

## Publish to the community repository

Generating manifests does **not** publish the portable package to WinGet. Copy the generated `manifests/d/DanKrusi/WindowsVirtualDesktopHelper/<version>/` directory into a fork of [microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs), commit it, and open a pull request. Microsoft validates and reviews it. Repeat for subsequent releases; unlike Scoop's autoupdate, committing this folder alone does not update the community source.

Once the initial submission is accepted, users can install using the friendly moniker:

```powershell
winget install windows-virtualdesktop-helper --source winget
```

The formal package identifier remains `DanKrusi.WindowsVirtualDesktopHelper`, following WinGet's publisher/package naming convention. To select the friendly moniker explicitly, use `winget install --moniker windows-virtualdesktop-helper --exact --source winget`. Until the submission is accepted, use the Store command or a local manifest.

References: [manifest format](https://learn.microsoft.com/windows/package-manager/package/manifest), [submission process](https://learn.microsoft.com/windows/package-manager/package/repository).

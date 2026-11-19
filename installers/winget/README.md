# WinGet

WinGet uses the Microsoft Store version of Windows Virtual Desktop Helper:

```powershell
winget install --id 9N4HNXPRTSDC --exact --source msstore
```

Installation and updates are handled by the Microsoft Store. New versions become available through this command after Partner Center submission and Store certification. No separate community manifest or per-release WinGet submission is needed.

## Abandoned community package submission

The plan to submit a custom portable package (`DanKrusi.WindowsVirtualDesktopHelper`, with the moniker `windows-virtualdesktop-helper`) through a pull request to [microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs) was abandoned in favor of the Microsoft Store channel. No pull request was submitted.

The templates and generated portable manifests in this folder are retained for reference. The release tooling still generates them, but they are not published to the WinGet community repository. Use the Store command above; the proposed portable package name is not available there.

# MSIX packaging

Build a native MSIX and portable ZIP with the Windows SDK's MakeAppx tool:

```powershell
.\scripts\build-msix.ps1 -Architecture x64
.\scripts\build-msix.ps1 -Architecture arm64
.\scripts\build-msix.ps1 -Store -Architecture x64 -IdentityFile packaging\msix\StoreIdentity.json
```

Requirements: Rust MSVC toolchain, Visual Studio C++ Build Tools (including the target architecture tools), Windows SDK, PowerShell 5.1 or later. No Visual Studio Installer Projects extension or .NET development workload is needed. Cargo.toml is the version source; MSIX appends a fourth zero component.

Outputs in `dist/`:

- `.msix`: validated, unsigned package.
- `.msixupload`: archive containing the unsigned package for Partner Center (no crash symbols).
- `.zip`: portable executable and license notices.
- `.sha256`: checksums for each artifact.

Each build stages files in a fresh `dist/.work/<id>/` directory; the script never recursively deletes a caller-supplied output path. It preserves the existing Store identity files. `-Store` requires the reserved identity, publisher and display name from Partner Center, and rejects placeholder/development identities. It does not submit anything to the Store.

For local distribution, use a certificate already trusted by the target machine. Its subject must exactly match the manifest Publisher. Sign explicitly:

```powershell
.\scripts\sign-msix.ps1 -PackagePath dist\WindowsVirtualDesktopHelper_3.0.0.0_x64_development.msix -CertificateThumbprint YOUR_CERTIFICATE_THUMBPRINT
```

Signing verifies the signature and regenerates the MSIX checksum. Rebuild the upload archive separately if you need a signed archive; the generated Store upload intentionally contains the unsigned package. No certificate is created, imported or trusted by these scripts. The Microsoft Store signs packages distributed through the Store.

Double-click a trusted, signed MSIX to install with Windows App Installer. Packaged startup uses the manifest startup task; the app links to `ms-settings:startupapps`. Portable startup uses the existing per-user Run entry. The app needs no administrator privileges.

The package validator checks the manifest, logos, license notices, executable architecture, GUI subsystem, and absence of a CLR header. MakeAppx performs schema/block-map validation. Installation/upgrade/uninstall and Store certification remain separate release checks.

References: [MakeAppx](https://learn.microsoft.com/en-us/windows/msix/package/create-app-package-with-makeappx-tool), [MSIX signing](https://learn.microsoft.com/en-us/windows/msix/package/sign-app-package-using-signtool).

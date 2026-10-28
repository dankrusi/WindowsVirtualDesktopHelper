# APPX packaging

The APPX build wraps the existing .NET Framework 4.7.2 desktop application as a full-trust Windows desktop package. It produces unsigned packages for Microsoft Store ingestion; it does not submit or publish anything to the Store.

## Build prerequisites

* Windows with Windows PowerShell 5.1.
* Visual Studio 2022 or later, or Visual Studio Build Tools, with .NET desktop development and the .NET Framework 4.7.2 targeting pack.
* A Windows 10/11 SDK containing `MakeAppx.exe`.

No signing certificate, NuGet packages, or Visual Studio installer-project extension is required for APPX builds.

## Microsoft Store identity

`Packaging/Appx/StoreIdentity.json` is configured for Store product **9N4HNXPRTSDC**, with reserved app name **Virtual Desktop Helper**, package identity `16065DanKrusi.VirtualDesktopHelper`, and publisher `CN=87DD2A22-6F4A-4054-9D4F-CF33AF77D555`.

For a different Store listing, open Partner Center → the app → Product management → Product identity. Copy `Packaging/Appx/StoreIdentity.example.json` to `Packaging/Appx/StoreIdentity.json` and replace its values with:

* `Name`: Package/Identity/Name.
* `Publisher`: Package/Identity/Publisher, including the full `CN=...` value.
* `PublisherDisplayName`: Package/Properties/PublisherDisplayName.
* `DisplayName`: the exact app name reserved for this Store product, from the app name reservations page. This is a friendly name, not the package identity name or publisher display name.

These are public package identity values, not account credentials. Commit this JSON file to let CI build packages for the correct Store listing. Never put a password, certificate, or Partner Center API credential in it.

```powershell
.\Scripts\build-appx.ps1 -Store -Architecture x64
.\Scripts\build-appx.ps1 -Store -Architecture x86
```

Individual values can also be passed using `-IdentityName`, `-Publisher`, `-PublisherDisplayName`, and `-DisplayName`. `-IdentityFile` selects a different JSON file. Store builds fail before compilation if identity values or the reserved display name are missing or still contain example/development values.

Without `-Store`, the script builds a clearly named development package with a development identity. It does not automatically read the checked-in Store identity in this mode:

```powershell
.\Scripts\build-appx.ps1 -Architecture x64
```

A development package is not a package for the reserved Store listing. Do not upload it to Partner Center.

## Artifacts and versioning

By default, packages are written to `artifacts/appx/`:

* `WindowsVirtualDesktopHelper_2.1.0.0_x64_store.appx`: unsigned installable package payload for Store ingestion.
* `WindowsVirtualDesktopHelper_2.1.0.0_x64_store.appxupload`: upload archive containing that APPX.
* A `.sha256` checksum file for each artifact.

The x86 and development builds use matching names with their architecture/mode. No debug symbols are included in the package or upload archive, so Store crash-symbol uploads are not supplied by this pipeline.

The version comes from `Source/AssemblyInfo.cs`. An explicit `-Version` must match the assembly version. It must have four components, such as `2.1.0.0`; Store builds require the final component to be zero. Update the assembly version before submitting an update. Each invocation uses fresh intermediate and staging directories to prevent stale files from entering the package.

Useful build overrides:

```powershell
.\Scripts\build-appx.ps1 -Store -Architecture x64 `
    -OutputDirectory .\artifacts\appx\x64 `
    -WindowsSdkVersion 10.0.26100.0
```

`-MSBuildPath` can select a particular `MSBuild.exe`. The build otherwise discovers MSBuild and the newest installed SDK automatically. It always builds Release, explicitly disables code signing, and sets the `APPX_PACKAGE` compilation symbol.

## Manifest and application behavior

`Packaging/Appx/AppxManifest.xml` is the manifest template. The script sets identity, version, publisher display name, and architecture in a staged copy, and generates 44×44, 50×50, and 150×150 PNG assets from the existing repository logo. It applies the configured reserved app name to `Package/Properties/DisplayName`, the application's visual elements, and the startup task. Validation checks these names against the configuration; Partner Center checks whether the name is actually reserved for the product.

The package targets Windows Desktop from Windows 10 version 1809, and declares `runFullTrust` because the application uses desktop COM APIs, global hotkeys, and tray icons. The .NET Framework requirement is 4.7.2. Full-trust capability approval and application compatibility remain part of Store certification.

Packaged builds register an opt-in startup task, disabled by default. Their Settings dialog links to Windows Settings → Apps → Startup. This avoids the unpackaged build's registry-based startup registration, which is not appropriate for a packaged application. MSI and ZIP builds retain their existing startup checkbox.

## Validation and submission

`MakeAppx pack` runs its normal manifest and package validation; the script does not bypass validation with `/nv`. After packing, `Scripts/test-appx.ps1` checks the package identity, executable architecture and entry point, full-trust capability, opt-in startup task, logo sizes, unsigned state, every SHA-256 block-map hash, payload hashes, and exclusion of installer files, scripts, private keys, and debug symbols.

To inspect an existing artifact:

```powershell
.\Scripts\test-appx.ps1 `
    -PackagePath .\artifacts\appx\WindowsVirtualDesktopHelper_2.1.0.0_x64_store.appx `
    -ExpectedVersion 2.1.0.0 -ExpectedArchitecture x64
```

The scripts do not install packages, enable Developer Mode, add trusted certificates, or change system security settings. Packaging validation is not Store certification or a packaged runtime test. Test the actual Store-identity package with the Windows App Certification Kit and on supported Windows builds before submitting the `.appxupload` files in Partner Center. The Store signs APPX/MSIX packages during its publishing process; direct distribution outside the Store needs an appropriate trusted package signature.

See Microsoft's [package creation instructions](https://learn.microsoft.com/en-us/windows/uwp/packaging/create-app-package-with-makeappx-tool), [Store package requirements](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/package-version-numbering), and [startup task documentation](https://learn.microsoft.com/en-us/uwp/api/windows.applicationmodel.startuptask).

## GitHub Actions

`.github/workflows/appx.yml` builds and validates both architectures on Windows for relevant pushes and pull requests, new published releases, and manual runs. Each architecture is uploaded as a separate Actions artifact containing the APPX, APPXUPLOAD, and checksums, retained for 30 days.

* Once `StoreIdentity.json` is committed, normal CI builds use that Store identity.
* Before identity configuration, push/PR builds produce development packages.
* Published-release builds and manual runs with `store: true` require the Store identity and fail if it is missing.
* Manual runs with `store: false` use the development identity, even when the Store identity is configured.

The workflow has read-only repository permissions and does not upload packages to Partner Center or modify GitHub release assets. Download its Store-mode artifacts and submit them through Partner Center.

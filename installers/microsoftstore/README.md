# Microsoft Store

`AppxManifest.xml` is the MSIX template. `StoreIdentity.json` contains the reserved Partner Center identity; `StoreIdentity.example.json` documents its fields. These are package identifiers, not credentials.

Use `scripts/release.ps1` for Store releases. GitHub Actions builds x64, x86 and ARM64, then combines them into one `.msixupload` downloaded to `dist/store`. See [Releasing](../../docs/releasing.md) and [MSIX packaging](../../docs/msix.md).

The build script fills the version and architecture from the release metadata. The manifest declares all six embedded UI languages; custom translation files do not require rebuilding the package.

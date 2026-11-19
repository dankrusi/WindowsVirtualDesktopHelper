# Microsoft Store

`AppxManifest.xml` is the MSIX template. `StoreIdentity.json` contains the reserved Partner Center identity; `StoreIdentity.example.json` documents its fields. These are package identifiers, not credentials.

Use `scripts/release.ps1` for Store releases. GitHub Actions builds x64, x86 and ARM64, then combines them into one `.msixupload` downloaded to `dist/store`. See [Releasing](../../docs/releasing.md) and [MSIX packaging](../../docs/msix.md).

The build script fills the version and architecture from the release metadata. The manifest declares all six embedded UI languages; custom translation files do not require rebuilding the package.

## Store listing translations

`listings/*.json` contains editable Store descriptions, features, search terms, release notes, and license text for English, German, Spanish, French, Simplified Chinese, and Traditional Chinese. Keep the English description aligned with the project README, then update the translations. These files describe the Store listing; the app's UI translations live in `locales/`.

The Chinese listings use the reserved names `Virtual Desktop Helper（虚拟桌面助手）` and `Virtual Desktop Helper（虛擬桌面助手）`. Other languages use `Virtual Desktop Helper`.

Chinese Store artwork is available in `screenshots/`, with `zh-Hans` and `zh-Hant` filenames: dark overviews at 1920×1080, box art at 2160×2160, and poster art at 1440×2160. Editable source designs are in [Figma](https://www.figma.com/design/kuFbLioELdrt7EFzlUSxRh/Windows-Virtual-Desktop-Helper?node-id=37-191). Export the localized frames at their original size. When importing new PNGs into Partner Center, use **Import folder** with the CSV and referenced images together.

To update an existing Partner Center draft, export its listings and populate the language columns in that CSV. Map `Features` to `Feature1`–`Feature20`, `Keywords` to `SearchTerm1`–`SearchTerm7`, and `LicenseTerms` to `AdditionalLicenseTerms`. Other text fields use their JSON names. Preserve the exported `Field`, `ID`, and `Type` columns, and save as UTF-8. Copy the English image URLs, publisher/copyright information, and logo override settings to each language to reuse the existing screenshots and artwork.

Use **Import listings → Import .csv** in Partner Center and check for validation errors. A successful import saves the draft listings; submitting the release for certification is a separate step. See Microsoft's [import/export documentation](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/import-and-export-store-listings).

The Store CLI can read a Partner Center-created draft even when its update endpoint rejects it with `InvalidState`. In that situation, use CSV import to update the existing draft. Do not delete and recreate a submission just to work around the API error. Keep full API responses and generated import files under ignored `dist/`; API responses can include signed upload URLs.

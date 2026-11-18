# Releasing

One command makes a release:

```powershell
.\scripts\release.ps1                  # asks for the version
.\scripts\release.ps1 --version 3.0.1
```

It:

1. Checks that `main` is clean and up to date, then runs `scripts\check.ps1`.
2. Sets the version in `VERSION.md` and `Cargo.toml`/`Cargo.lock`, renames a top `# Unreleased` heading in `CHANGELOG.md` to `# v3.0.1` (a `# v3.0.1` heading is kept as is), and rewrites the README download links between the `<!-- release-* -->` marker comments.
3. Commits `Release v3.0.1`, tags `v3.0.1` and pushes both.
4. Waits for GitHub Actions (`.github/workflows/rust.yml`), which on a `v*` tag builds x64, x86 and ARM64 with the Partner Center identity (`packaging/msix/StoreIdentity.json`) and publishes a GitHub Release with the portable ZIPs and checksums (notes come from the CHANGELOG section).
5. Commits `installers/scoop/windows-virtualdesktop-helper.json` with the released ZIP's hash.
6. Downloads the Microsoft Store upload file to `dist\store\` and opens it in Explorer, along with the product in Partner Center (see below).

Options: `--no-push` (commit and tag locally only), `--no-wait` (don't wait for CI, update the Scoop manifest or download the Store file), `--skip-checks`, `--help`. Without `--version` it asks, suggesting the next patch version (or the current version if that was never tagged).

Write the CHANGELOG entries as you go (format in `AGENTS.md`: `## Features`, `## Changes`, `## Fixes`); the topmost section becomes the release notes.

## Microsoft Store

The release build produces one `WindowsVirtualDesktopHelper_<version>.0.msixupload` containing x64, x86 and ARM64. `release.ps1` downloads it at the end; to get it again later (within 90 days of the build):

```powershell
.\scripts\get-store-upload.ps1                  # version from VERSION.md
.\scripts\get-store-upload.ps1 --version 3.0.1
```

Then in [Partner Center](https://partner.microsoft.com/dashboard/products/9N4HNXPRTSDC/overview):

1. Click **Start update**.
2. Open **Packages** and drag in the `.msixupload`. Keep the existing 2.x packages: the Store gives each device the newest package it supports, so Windows 10 keeps getting 2.1 and Windows 11 gets the new version.
3. Click **Save**, then **Submit to the Store**. Certification usually takes 1-3 days.

The listing, screenshots and pricing carry over from the previous submission.

## Channels

| Channel | How it updates |
| --- | --- |
| GitHub Releases | Created by the workflow. |
| Microsoft Store | Upload the `.msixupload` in Partner Center (above). |
| winget | Store apps are served by winget automatically: `winget install 9N4HNXPRTSDC`. |
| Scoop | The [Extras manifest](https://github.com/ScoopInstaller/Extras/blob/master/bucket/windows-virtualdesktop-helper.json) autoupdates from new GitHub releases. It expects `WindowsVirtualDesktopHelper.Executable.v<version>.zip`, so keep that asset name. |

## Optional: automatic Store submission

The workflow can submit to the Store by itself with the [Microsoft Store Developer CLI](https://learn.microsoft.com/windows/apps/publish/msstore-dev-cli/overview). It is skipped unless these GitHub secrets exist: `PARTNER_CENTER_TENANT_ID`, `PARTNER_CENTER_CLIENT_ID`, `PARTNER_CENTER_CLIENT_SECRET`, `PARTNER_CENTER_SELLER_ID`.

Getting them requires a Microsoft Entra (work) account linked to Partner Center: create a tenant under **Account settings > Tenants**, sign in with its admin account, then create an application under **User management > Microsoft Entra applications** with the **Manager** role. Setting the repository variable `STORE_DRAFT` to `true` leaves automatic submissions as drafts.

## Cost

GitHub-hosted runners are free for public repositories. The only Store cost is the developer account you already have.

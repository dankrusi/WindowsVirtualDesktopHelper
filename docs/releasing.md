# Releasing

One command makes a release:

```powershell
.\scripts\release.ps1                  # asks for the version
.\scripts\release.ps1 --version 3.0.2
```

It:

1. Checks that `main` is clean and up to date, then runs `scripts\check.ps1`.
2. Sets the version in `VERSION.md` and `Cargo.toml`/`Cargo.lock`, renames a top `# Unreleased` heading in `CHANGELOG.md` to `# v3.0.2` (a `# v3.0.2` heading is kept as is), and rewrites the README download links between the `<!-- release-* -->` marker comments.
3. Commits `Release v3.0.2`, tags `v3.0.2` and pushes both.
4. Waits for GitHub Actions (`.github/workflows/rust.yml`), which on a `v*` tag builds x64, x86 and ARM64 with the Partner Center identity (`installers/microsoftstore/StoreIdentity.json`) and publishes a GitHub Release with the portable ZIPs and checksums (notes come from the CHANGELOG section).
5. Downloads and verifies all three released ZIPs (checksums and executable architectures), then commits the Scoop manifest and the portable WinGet manifests under `installers/`. CI also includes these manifests in the release artifact.
6. Downloads the Microsoft Store upload file to `dist\store\` and opens it in Explorer, along with the product in Partner Center (see below).

Options: `--no-push` (commit and tag locally only), `--no-wait` (don't wait for CI, update the package-manager manifests or download the Store file), `--skip-checks`, `--help`. Without `--version` it asks, suggesting the next patch version (or the current version if that was never tagged).

Write the CHANGELOG entries as you go (format in `AGENTS.md`: `## Features`, `## Changes`, `## Fixes`); the topmost section becomes the release notes.

## Prepare a local version bump

For a version bump without publishing:

```powershell
.\scripts\release.ps1 --version 3.0.2 --prepare-only
```

This runs the checks, updates VERSION.md, Cargo.toml/Cargo.lock, the MSIX template and current documentation examples, and renames the top Unreleased changelog section. It accepts a working tree with app changes. It does not pull, commit, tag, push, run CI or rewrite download links to an unpublished version. A later normal release uses the same prepared version and updates the links when publishing.

## Microsoft Store

The release build produces one `WindowsVirtualDesktopHelper_<version>.0.msixupload` containing x64, x86 and ARM64. `release.ps1` downloads it at the end; to get it again later (within 90 days of the build):

```powershell
.\scripts\get-store-upload.ps1                  # version from VERSION.md
.\scripts\get-store-upload.ps1 --version 3.0.2
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
| WinGet (Store) | `winget install --id 9N4HNXPRTSDC --exact --source msstore`. |
| WinGet (portable) | Generated under [installers/winget](../installers/winget/README.md). Each version needs a pull request to `microsoft/winget-pkgs`; generation alone does not publish it. |
| Scoop | The [Extras manifest](https://github.com/ScoopInstaller/Extras/blob/master/bucket/windows-virtualdesktop-helper.json) autoupdates from new GitHub releases. It expects `WindowsVirtualDesktopHelper.Executable.v<version>.zip`, so keep that asset name. See [Scoop maintenance](../installers/scoop/README.md) for local testing and upstream metadata updates. |

## Optional: automatic Store submission

The workflow can submit to the Store by itself with the [Microsoft Store Developer CLI](https://learn.microsoft.com/windows/apps/publish/msstore-dev-cli/overview). It is skipped unless these GitHub secrets exist: `PARTNER_CENTER_TENANT_ID`, `PARTNER_CENTER_CLIENT_ID`, `PARTNER_CENTER_CLIENT_SECRET`, `PARTNER_CENTER_SELLER_ID`.

Getting them requires a Microsoft Entra (work) account linked to Partner Center: create a tenant under **Account settings > Tenants**, sign in with its admin account, then create an application under **User management > Microsoft Entra applications** with the **Manager** role. Setting the repository variable `STORE_DRAFT` to `true` leaves automatic submissions as drafts.

## Cost

GitHub-hosted runners are free for public repositories. The only Store cost is the developer account you already have.

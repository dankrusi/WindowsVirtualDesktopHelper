# Working on this app

- After completing app changes, run the appropriate checks, rebuild, and relaunch the updated app with the user's existing configuration.
- Use an isolated `APPDATA` directory for UI tests; do not replace the user's configuration with test settings.
- Run automated checks yourself, then ask the user to perform interactive computer/UI testing. Relaunch the updated build and provide a short test checklist instead of automating the UI.
- If notifyme is installed and in the AGENTS instructions, send a a notification after successful completion and relaunch.

# Settings UI

- Place dependent settings inside their parent toggle’s card using the shared `toggle_group` and `SettingRow::Child` layout. Use compact, indented rows with the group’s divider and aligned controls.
- Keep child settings visible when unavailable. Disable the controls and mute their labels and descriptions using the same effective enabled state. Keep wrapped text and native control sizes so the rows work at narrow widths and high DPI.
- Apply the embedded app icon to every titled app window through the shared caption helper.

# Changelog

- Record every user-visible change in `CHANGELOG.md` under the topmost version heading (`# vX.Y.Z`). If that version is already released (it has a `vX.Y.Z` git tag), add a new `# Unreleased` heading above it; `scripts\release.ps1` renames it to the released version.
- Each version has three sections, in this order:
  - `## Features`: new or improved things users notice, written for users. Non-technical, no code, API or file names.
  - `## Changes`: technical and detailed changes (architecture, dependencies, packaging, build, configuration keys, behavior details).
  - `## Fixes`: bug fixes.
- Use `*` bullets, one change per bullet. The topmost section becomes the GitHub release notes.

# Releasing and Store builds

- To release a version or produce a Microsoft Store upload, commit the changes on `main` and run `.\scripts\release.ps1 --version X.Y.Z` (see `docs/releasing.md`). Don't bump the version by hand first: the script sets `VERSION.md`, `Cargo.toml`/`Cargo.lock`, the CHANGELOG heading and the README links itself.
- The script tags and pushes, and GitHub Actions builds x64, x86 and ARM64; it then downloads the single `.msixupload` with all three to `dist\store\`. Use that file for Partner Center.
- Don't build Store uploads locally with `build-msix.ps1 --store`: this machine has no Visual Studio ARM64 build tools, so a local upload would miss ARM64.
- The script waits for CI (often more than 10 minutes), so run it in the background and send a notification when it finishes.

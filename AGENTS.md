# Working on this app

- After completing app changes, run the appropriate checks, rebuild, and relaunch the updated app with the user's existing configuration.
- Use an isolated `APPDATA` directory for UI tests; do not replace the user's configuration with test settings.
- Run automated checks yourself, then ask the user to perform interactive computer/UI testing. Relaunch the updated build and provide a short test checklist instead of automating the UI.
- If notifyme is installed and in the AGENTS instructions, send a a notification after successful completion and relaunch.

# Changelog

- Record every user-visible change in `CHANGELOG.md` under the topmost version heading (`# vX.Y.Z`). If that version is already released (it has a `vX.Y.Z` git tag), add a new `# Unreleased` heading above it; `scripts\release.ps1` renames it to the released version.
- Each version has three sections, in this order:
  - `## Features`: new or improved things users notice, written for users. Non-technical, no code, API or file names.
  - `## Changes`: technical and detailed changes (architecture, dependencies, packaging, build, configuration keys, behavior details).
  - `## Fixes`: bug fixes.
- Use `*` bullets, one change per bullet. The topmost section becomes the GitHub release notes.

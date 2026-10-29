# Working on this app

- After completing app changes, run the appropriate checks, rebuild, and relaunch the updated app with the user's existing configuration.
- Use an isolated `APPDATA` directory for UI tests; do not replace the user's configuration with test settings.
- Run automated checks yourself, then ask the user to perform interactive computer/UI testing. Relaunch the updated build and provide a short test checklist instead of automating the UI.
- If notifyme is installed and in the AGENTS instructions, send a a notification after successful completion and relaunch.

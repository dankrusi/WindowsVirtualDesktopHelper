Refactor the entire application to Rust and native Windows 11 UI code.

The core features and flags should all remain the same, as well as the systems for VirtualDesktopAPI, WindowsHotKeyAPI, and WindowsInputAPI.

The UI should migrated to use more modern Windows 11 look and feel, respecting light/dark mode etc. Also make the UI easier to extend, by offering components that can be re-used for various settings (like hotkey settings, etc). Use SVGs illustrations where needed within the UI to allow for easier understanding which settings do what exactly, and consider a realtime preview of some sort.

The Rust code should use as few external libraries as possible, be extremely efficent and fast, and the core virtual desktop APIs should be efficient and optimized (unlike the C# version that uses polling etc).

For the setup/installer - use the new Windows installer system.

Also restructure the the folder/file layout to better match a Rust project as needed (capitalization, scripts folders, release folders, etc).


## Implementation ? 2026-10-28

Implemented as the Cargo project in `src/`, replacing the C# solution and MSI project. Existing settings and API profiles are preserved; desktop/focus updates are event-driven. The native settings UI has reusable rows, light/dark styling and vector/overlay previews. MSIX and portable release scripts are in `scripts/`.

See [architecture](../docs/architecture.md), [packaging](../docs/msix.md), and [validation results and remaining release checks](../docs/validation.md). x64/x86 packages are built and validated; ARM64 source checks pass but its linker is unavailable locally. Signing, installation and publication were not performed.

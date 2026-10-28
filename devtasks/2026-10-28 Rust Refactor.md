Refactor the entire application to Rust and native Windows 11 UI code.

The core features and flags should all remain the same, as well as the systems for VirtualDesktopAPI, WindowsHotKeyAPI, and WindowsInputAPI.

The UI should migrated to use more modern Windows 11 look and feel, respecting light/dark mode etc. Also make the UI easier to extend, by offering components that can be re-used for various settings (like hotkey settings, etc). Use SVGs illustrations where needed within the UI to allow for easier understanding which settings do what exactly, and consider a realtime preview of some sort.

The Rust code should use as few external libraries as possible, be extremely efficent and fast, and the core virtual desktop APIs should be efficient and optimized (unlike the C# version that uses polling etc).

For the setup/installer - use the new Windows installer system.

Also restructure the the folder/file layout to better match a Rust project as needed (capitalization, scripts folders, release folders, etc).

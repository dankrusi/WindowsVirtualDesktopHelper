# Bug & Issue Report — WindowsVirtualDesktopHelper

- **Date:** 2026-07-13
- **Reviewer:** Claude (Fable 5), full manual source review
- **Scope:** All C# sources under `Source/` (app core, settings system, forms, utils, hotkey API, all 9 VirtualDesktopAPI implementations), plus README/Documentation cross-checks. The vendored `WindowsInputAPI` (InputSimulator) library was only skimmed.
- **Baseline:** branch `main`, commit `d5b3542`

Severity legend: **High** = crashes, resource exhaustion, or feature-breaking for real users. **Medium** = incorrect behavior, data loss, or crash in edge configs. **Low** = latent bugs, code smells, robustness gaps.

## Summary

| ID | Severity | Area | Title |
|----|----------|------|-------|
| H1 | High | Icons | GDI `HICON` leak on every tray icon update |
| H2 | High | Overlay | Static event handler leak keeps every overlay form alive forever |
| H3 | High | Settings | Config parser truncates any value containing a colon |
| H4 | High | Settings | Number parsing/serialization is culture-dependent (breaks on non-English locales) |
| H5 | High | Settings | `Regex.Unescape` crashes on config strings containing backslashes; escape round-trip is broken |
| H6 | High | Hotkeys | A single hotkey registration conflict crashes the whole app |
| H7 | High | VD API | Stale COM reference after explorer.exe restart — app bricks until manual restart |
| M1 | Medium | Overlay | Documented `duration = 0` ("indefinite") actually throws on every switch |
| M2 | Medium | Threading | Unsynchronized collections shared across threads (3 instances) |
| M3 | Medium | Logging | In-memory log grows without bound |
| M4 | Medium | Settings | `SaveConfig` sort comparator can throw and is not transitive |
| M5 | Medium | Settings | Settings changes are lost unless the Settings window is closed |
| M6 | Medium | UI | Desktop-count monitor updates state but never refreshes the icons |
| M7 | Medium | Threading | Monitor threads are foreground threads → zombie-process exit paths |
| M8 | Medium | Settings | Type-strict getters crash the app on slightly-wrong config values |
| M9 | Medium | Docs | README/Hotkeys.md document `=` syntax the parser silently ignores |
| M10 | Medium | Tray UI | Prev/next icons react to right-clicks; rapid clicks are swallowed |
| M11 | Medium | VD API | Error sentinel `0` is indistinguishable from desktop 1; `-1` wraps to `uint.MaxValue` |
| M12 | Medium | Settings UI | Dependent-control enabled state never initialized on load |
| L1–L15 | Low | various | See section below |

---

## High severity

### H1. GDI `HICON` leak on every tray icon update
**File:** `Source/Util/Icons.cs:47` and `:124`

```csharp
return Icon.FromHandle(cachedBitmap.GetHicon());   // cache hit path
...
return Icon.FromHandle(bitmapScaledDown.GetHicon()); // miss path
```

`Bitmap.GetHicon()` allocates a native GDI icon handle. Per MSDN, `Icon.FromHandle` does **not** take ownership — the caller must call `DestroyIcon`, and disposing the `Icon` won't do it. Nothing ever destroys these handles, and the bitmap cache doesn't help because a **new HICON is minted on every call, including cache hits**.

Every desktop switch regenerates up to 4 icons (`UIUpdateIconForVDDisplayNumber`, `...DisplayName`, prev, next in `App.VDSwitchedSafe`). For a tray app that runs for weeks, this steadily consumes GDI objects; at the per-process limit (10,000) icon creation and rendering start failing and the app can crash. The previously assigned `NotifyIcon.Icon` instances are also never disposed.

Additional undisposed GDI objects in the same method: both `Graphics` instances, `Font` (recreated in the fit loop), `SolidBrush` ×2, `StringFormat`, and the 128×128 intermediate `bitmap` (only the scaled-down bitmap is cached).

**Fix:** cache the final `Icon` (not the bitmap), create it once via `GetHicon()` + `Icon.FromHandle(...).Clone()` + `DestroyIcon(handle)` (P/Invoke), and wrap all GDI objects in `using`. Alternatively keep a `Dictionary<string, Icon>` and return the same `Icon` instance for repeated keys.

### H2. Static event handler leak keeps every overlay form alive forever
**File:** `Source/Forms/SwitchNotificationForm.cs:76`

```csharp
WillShowNotificationFormEvent += this.OnWillShowNotificationForm;
```

The constructor subscribes to a **static** event and never unsubscribes — not on close, not on dispose. Every overlay form ever shown (one per monitor per desktop switch with `showOnAllMonitors` enabled) remains rooted by the static delegate for the lifetime of the process, along with its `Font`, label, and window resources. The invocation list also grows unboundedly, so `CloseAllNotifications` calls `Close()` on hundreds of already-disposed forms over time.

**Fix:** unsubscribe in an `FormClosed` handler or in `Dispose`:
```csharp
protected override void OnFormClosed(FormClosedEventArgs e) {
    WillShowNotificationFormEvent -= this.OnWillShowNotificationForm;
    base.OnFormClosed(e);
}
```

### H3. Config parser truncates any value containing a colon
**File:** `Source/App/Settings.cs:413-417`

```csharp
var parts = line.Split(':');
if(parts.Length >= 2) {
    var key = parts[0].Trim();
    var val = parts[1].Trim();   // <-- everything after the SECOND colon is dropped
```

A config line like `feature.showSplashScreen.text: "Desktop: Home"` loads as `"Desktop` (also un-terminated quote, so it isn't even recognized as a string). Any value containing `:` — labels, URLs, times — is silently corrupted.

**Fix:** split on the first colon only: `line.Split(new[] { ':' }, 2)`.

### H4. Number parsing/serialization is culture-dependent
**File:** `Source/App/Settings.cs` — `_parseValAsType` (`:390`), `GetDouble`/`GetFloat` (`:229-253`), `_serializeValAsType` (`:396-402`)

All numeric conversions use the current thread culture:

- On a German-locale system (`de-DE`), `float.TryParse("0.5")` succeeds and returns **5** (period = group separator). A user setting `theme.icons.disabledOpacity: 0.5` gets opacity 5.0 → `Color.FromArgb((int)(255*5), ...)` throws in `GenerateNotificationIcon` (H1 area) — startup crash.
- `_serializeValAsType` writes floats with `ToString()` → `"0,5"` on comma-decimal locales. That file then fails to parse (or parses wrongly) if the machine locale changes or the file is shared.
- Same for `GetInt`/`GetDouble` string paths.

**Fix:** use `CultureInfo.InvariantCulture` (and `NumberStyles.Float`) for every parse and `ToString` in the settings layer.

### H5. `Regex.Unescape` crashes on backslashes in config strings; escape round-trip broken
**File:** `Source/App/Settings.cs:354-377`, `Source/Main.cs:11-12`

`_unescapeString` runs `Regex.Unescape` on any quoted value. A user-entered value like `"C:\Users\dan"` throws `ArgumentException` (`\U` is not a valid escape). Consequences:

1. Via config file: `LoadConfig` throws → app shows the error form and won't start until the user hand-edits the file.
2. Via command line: `Settings.RegisterLaunchArgs(args)` runs **before** the global `try/catch` in `Main` (`Main.cs:11-14`), so a bad quoted argument kills the process with no error UI at all (WinExe → completely silent).

The round trip is also lossy: `_unescapeString` does `str.Trim('"')`, which strips *all* leading/trailing quote characters. A value that legitimately ends with an escaped quote (`"say \"hi\""`) has both trailing quotes trimmed, leaving a dangling `\` that then makes `Regex.Unescape` throw — i.e., `_escapeString` output cannot always be read back by `_unescapeString`.

**Fix:** write a small dedicated unescaper that handles exactly the escapes `_escapeString` produces (`\\`, `\n`, `\"`, `\uXXXX`) and strips exactly one quote from each end; move `LoadDefaults`/`RegisterLaunchArgs` inside the global handler.

### H6. A single hotkey registration conflict crashes the whole app
**File:** `Source/App/App.cs:547-549`, `Source/WindowsHotKeyAPI/KeyboardHook.cs:86-88`

```csharp
foreach(var hotkeyAction in _keyboardHooksHotKeysAndActions) {
    this._keyboardHooks.RegisterHotKey(hotkeyAction.Modifiers, hotkeyAction.Keys); // throws on failure
}
```

`KeyboardHook.RegisterHotKey` throws `InvalidOperationException` when the Win32 call fails — which happens whenever *any other application* already owns that hotkey, or when the user's config accidentally lists the same combination twice (custom `hotkeys.*` + a `feature.useHotKey...` generating the same combo). There is no try/catch around the loop:

- At startup (`App` constructor → `SetupHotKeys`) the exception propagates to `Main`'s global handler → the app shows the error form and refuses to run.
- From the Settings form (`checkBoxUseHotKeysToJumpToDesktop_CheckedChanged` → `SetupHotKeys`) it's an unhandled UI-thread exception.

`debug.singleInstance`'s own documentation ("expect errors registering hotkeys") shows multi-instance runs hit exactly this.

**Fix:** catch per-hotkey, log via `Util.Logging`, and continue registering the remaining hotkeys; optionally surface a balloon tip. Registration failure of one shortcut must never take down the app.

### H7. Stale COM reference after explorer.exe restart — app bricks until manual restart
**File:** all `Source/VirtualDesktopAPI/Implementation/*.cs`, e.g. `VirtualDesktopWin10.cs:225-231`

```csharp
static DesktopManager() {
    var shell = (IServiceProvider10)Activator.CreateInstance(...);
    VirtualDesktopManagerInternal = (IVirtualDesktopManagerInternal)shell.QueryService(...);
}
```

The immersive-shell COM object lives in explorer.exe. The wrapper is created once in a **static constructor** and cached forever. When Explorer crashes or is restarted (common: taskbar hangs, `taskkill /f /im explorer.exe`, Windows updates), every subsequent call fails with RPC-unavailable errors. From then on:

- `_MonitorVDSwitch`/`_MonitorVDDisplayCount` log an error every second, forever (feeding M3).
- `GetVDDisplayNumber(false)` returns the sentinel `0` (see M11), so the app briefly believes it switched to desktop 1 and shows an overlay reading "Unknown".
- Tray icons and switching stay dead until the user manually restarts the app.

**Fix:** on COM failure (`COMException`/`InvalidCastException`), re-run the `DesktopManager` initialization (make it a re-creatable instance instead of a static ctor), or re-run `Loader.LoadImplementationWithFallback` from `App` after N consecutive monitor failures.

---

## Medium severity

### M1. Documented `duration = 0` ("shown indefinitely") throws on every switch
**Files:** `Source/App/Settings.cs:65` (doc), `Source/Forms/SwitchNotificationForm.cs:117`

The default's documentation says: *"If set to zero, then the overlay is shown indefinately."* But `SwitchNotificationForm_Shown` executes `this.timerClose.Interval = this.DisplayTimeMS;` unconditionally, and `System.Windows.Forms.Timer.Interval` throws `ArgumentOutOfRangeException` for values < 1. With `feature.showDesktopSwitchOverlay.duration: 0`, every desktop switch (and the splash screen) raises an unhandled UI exception. The `if(this.DisplayTimeMS == 0) return;` guard in `timerClose_Tick` is never reached.

**Fix:** only assign/start `timerClose` when `DisplayTimeMS > 0`.

### M2. Unsynchronized collections shared across threads
**Files:** `Source/App/App.cs:52-55`, `Source/Util/Logging.cs:7-13`

Three plain collections are mutated and read concurrently with no locking:

1. `VDDToLastFocusedWin` (`Dictionary<int, IntPtr>`) — written by the `_monitorFocusedWindow` thread every 200 ms (`App.cs:329-343`), read/written on the UI thread by `_restorePrevWinFocus` and `SwitchToDesktop`. Concurrent write + read of `Dictionary` can throw or corrupt internal state.
2. `FGWindowHistory` (`List<string>`) — `Add`/`RemoveRange` every 20 ms on a worker thread (`App.cs:291-309`), while the UI thread calls `.Contains("Task View")` from tray click handlers (`AppForm.cs:124`, `:137`). `Contains` during a `RemoveRange` can throw `ArgumentOutOfRangeException` or read garbage.
3. `Logging._log` (`List<string>`) — appended from five worker threads plus the UI thread; enumerated via `string.Join` in `ErrorForm`/`AboutForm`. Enumeration during `Add` throws `InvalidOperationException` — ironically most likely exactly when the error form is trying to display.

**Fix:** `ConcurrentDictionary`, a lock around the list operations, or confine all access to the UI thread via `Invoke`.

### M3. In-memory log grows without bound
**File:** `Source/Util/Logging.cs:9-13`

`_log.Add(line)` with an acknowledged `//TODO: trim front...`. Normally slow growth, but combined with H7 the monitor loops add several entries **per second** forever — tens of MB per day plus a growing `Console.WriteLine` cost. Cap it (e.g., ring buffer of 2,000 lines).

### M4. `SaveConfig` sort comparator can throw and is not transitive
**File:** `Source/App/Settings.cs:149-161`

```csharp
var aParentKey = aKey.Substring(0, aKey.LastIndexOf('.'));
```

- The comparator operates on the **whole line** (`key: value`), not the key. A key with no dot anywhere in the line (any custom key such as `foo: true`) makes `LastIndexOf('.')` return −1 → `Substring(0, -1)` throws `ArgumentOutOfRangeException` → `List.Sort` wraps it in `InvalidOperationException` → closing the Settings window crashes (`SettingsForm_FormClosing` → `SaveConfig`).
- Because values can contain dots (`"0.5"`, quoted text), the "parent key" is often computed from inside the value, making the comparison non-transitive; `List.Sort` is allowed to throw *"IComparer.Compare() method returns inconsistent results"* on such comparators.

**Fix:** compare on the key portion only (`line.Split(new[]{':'},2)[0]`), and fall back to whole-key comparison when there is no dot.

### M5. Settings changes are lost unless the Settings window is closed
**Files:** `Source/Forms/SettingsForm.cs:60-89`, `Source/App/App.cs:726-729`

Checkbox/radio handlers write to the in-memory `_settingsConfig` only; `SaveConfig()` runs solely in `SettingsForm_FormClosing` and only for `CloseReason.UserClosing`. If the user changes settings and then exits the app from the tray menu (or Windows shuts down) while the Settings window is still open, everything except the registry-based autostart toggle is silently lost. **Fix:** also call `Settings.SaveConfig()` in `App.Exit()` (and/or save immediately on each change).

### M6. Desktop-count monitor updates state but never refreshes the icons
**File:** `Source/App/App.cs:147-161`

`_MonitorVDDisplayCount` polls `DisplayCount()` every 100 ms and updates `CurrentVDDisplayCount` — and then does nothing. When the user adds or removes a desktop (e.g., from Task View) without switching, the prev/next icons keep stale enabled/disabled opacity, and `feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds` doesn't hide/show them until the next switch or theme change. The thread's entire purpose appears to be driving that refresh — the call is missing:

```csharp
if(newCurrentVDDisplayCount != CurrentVDDisplayCount) {
    CurrentVDDisplayCount = newCurrentVDDisplayCount;
    // missing: invoke UIUpdateNextPrevIconVisibility on the UI thread
}
```

### M7. Monitor threads are foreground threads → zombie-process exit paths
**File:** `Source/App/App.cs:143-146`, `:163-168`, `:286-289`, `:312-315`, `:569-572`

All five monitor threads are created without `IsBackground = true` and loop `while(true)`. The app only dies today because `App.Exit()` calls `Environment.Exit(0)`. Any exit path that ends the message loop without `Environment.Exit` leaves an invisible process running forever — e.g., an unhandled UI exception after startup shows the WinForms `ThreadExceptionDialog`, the user clicks *Quit*, `Application.Run` returns, `Main` returns... and the five foreground threads keep the (now tray-icon-less) process alive. **Fix:** `thread.IsBackground = true;` on all monitor threads.

### M8. Type-strict getters crash the app on slightly-wrong config values
**File:** `Source/App/Settings.cs:203-253`

`_parseValAsType` eagerly types unquoted values (`1` → int, `true` → bool). The getters then throw for any mismatch: `feature.showPrevNextIcons: 1` → `GetBool` throws (`int` is neither `bool` nor `string`) → app fails to start with the error form. A hand-edited config should degrade gracefully. Also, the exception message has an interpolation typo — `$"... (value is ${ret})"` prints a literal `$`. **Fix:** coerce (`ToString` + `bool.TryParse`/int parse), fall back to the registered default on failure, and log a warning.

### M9. README/Hotkeys.md document `=` syntax the parser silently ignores
**Files:** `README.md:156-159`, `Documentation/Hotkeys.md:15-18`

The first documented example is:

```
hotkeys.myCustomHotkey1 = "Ctrl + Alt + Shift + D1 = Desktop1"
```

The config parser only accepts `key: value` (colon). This line contains no colon, so `parts.Length >= 2` fails and it is **silently skipped** — a user following the docs gets no hotkey and no error. (The second example on both pages correctly uses `:`.) Fix the docs, and consider logging a warning for non-comment lines that don't parse.

### M10. Prev/next tray icons react to right-clicks; rapid clicks are swallowed
**Files:** `Source/Forms/AppForm.cs:104-118`, `Source/Forms/AppForm.Designer.cs:93`, `:99`

`NotifyIcon.Click` is raised for **both** left and right mouse-ups (unlike `Control.Click`). `notifyIconPrev_Click`/`notifyIconNext_Click` don't inspect the button, and these icons have no context menu, so right-clicking them switches desktops — surprising, and it makes it impossible to ever attach a menu without also switching.

Additionally, `notifyIconPrev_DoubleClick`/`notifyIconNext_DoubleClick` exist in code (with TODOs) but are **not wired up in the designer** — and because a double-click raises Click once + DoubleClick once, a user rapidly clicking "next" loses every second click to the (dead) double-click slot.

**Fix:** switch to `MouseClick` with `e.Button == MouseButtons.Left`, and either wire DoubleClick to a second switch or implement the TODO (jump to first/last).

### M11. Error sentinel `0` is indistinguishable from desktop 1; `-1` wraps to `uint.MaxValue`
**Files:** `Source/App/App.cs:121-128`, `:169-187`; all implementations' `Current()`

- `GetVDDisplayNumber(false)` returns `0` on failure — the same value as "on desktop 1". Any transient COM failure in `_MonitorVDSwitch` therefore looks like a genuine switch to desktop 1: the app fires the overlay ("Unknown", since `GetVDDisplayName(false)` also failed), rewrites the icons, and pollutes `_desktopNumberHistory` (breaking `SwitchToPreviousDesktop`).
- In `Current()`, `GetDesktopIndex` returns `-1` when the desktop isn't found (e.g., it was removed mid-enumeration); `(uint)-1` = 4,294,967,295. In `UIUpdateIconForVDDisplayNumber` this then `number++` wraps around to 0 and the tray shows "0".

**Fix:** use a nullable return (`uint?`) or throw-and-skip in the monitor loop instead of returning a valid-looking index.

### M12. Dependent-control enabled state never initialized on Settings load
**File:** `Source/Forms/SettingsForm.cs:12-21`, `:56-57`

`LoadSettingsIntoUI` ends with:

```csharp
checkBoxShowOverlay_CheckedChanged(this, null);
checkBoxUseHotKeysToJumpToDesktop_CheckedChanged(this, null);
```

…but both handlers start with `if(IsLoading) return;`, and `IsLoading` is still `true` at that point. The intended sync (disabling the duration/position/animation controls when overlays are off, and the hotkey radios when hotkeys are off) never happens — with overlays disabled in config, the Settings window still shows all overlay controls enabled. **Fix:** extract the enable/disable logic into a method that doesn't check `IsLoading`, and call that.

---

## Low severity / latent issues

### L1. `OpenURL` corrupts URLs containing `&`
`Source/App/App.cs:721-724` — `url.Replace("&", "^&")` is cmd.exe caret-escaping, but the URL is passed directly to `ProcessStartInfo` with `UseShellExecute = true` (no cmd involved). Any future URL with a second query parameter would literally contain `^&`. The `//TODO: is this really needed?` is right: it isn't — remove it.

### L2. Autostart registry issues
`Source/App/App.cs:612-630` —
- `DisableStartupWithWindows`'s error message says "EnableStartupWithWindows" (copy-paste).
- The Run value is written as an **unquoted** `Application.ExecutablePath`; paths with spaces should be quoted (`"\"" + path + "\""`) — works today but is exactly the "unquoted service path" pattern security scanners flag, and it breaks if the exe path contains an ampersand-adjacent edge case.
- `OpenSubKey(..., true)` can return null (never on HKCU\...\Run in practice, but the NRE would be masked into a misleading message).

### L3. `throw e;` resets the stack trace
`Source/App/App.cs:75-81` — the try/catch around `LoadVDAPI`/`LoadVDDisplayInfo` does nothing but destroy the original stack trace. Use `throw;` or delete the wrapper.

### L4. `SetDouble` takes an `int`
`Source/App/Settings.cs:240` — `public static void SetDouble(string key, int value)`. Callers cannot actually store a double; the signature is a typo (`double value`).

### L5. `_serializeValAsType` doesn't handle `double`
`Source/App/Settings.cs:396-402` — checks `bool`/`int`/`float` but defaults such as `theme.icons.disabledOpacity` are registered as **double** (`0.5`), so they fall through to `_escapeString(val.ToString())` and are written as quoted, culture-formatted strings (`"0,5"` on comma locales — ties into H4).

### L6. Registry handles never disposed; theme key opened writable
`Source/Util/OS.cs:137-190` — every registry helper leaks its `RegistryKey` (no `using`); `IsSystemLightThemeModeEnabled` is called every second by the theme monitor and opens `Personalize` with `writable: true` (needless — and fails under restrictive ACLs). `(int)key.GetValue(...)` NREs/InvalidCastExceptions if the value is missing (older builds/Server SKUs) → with `general.theme: auto` the constructor path can kill startup.

### L7. `using static VirtualDesktopWin11_Insider` in the 23H2 implementations
`VirtualDesktopWin11_23H2.cs:7`, `VirtualDesktopWin11_23H2_2921.cs:7` — both files import all static members of the *Insider* implementation while declaring their own identically-named nested types (`DesktopManager`, `IVirtualDesktop`, ...). Today the local declarations shadow the import everywhere, so it's dead — but if a local member is ever renamed/removed, the compiler will silently bind to the Insider type **with different interface GUIDs**, producing baffling runtime failures. Given this project's whole maintenance burden is GUID churn, remove these imports.

### L8. COM object churn and inefficiency in the polling hot path
All `DesktopManager` wrappers — `Current()` runs every 100 ms and each call: creates an RCW for `GetCurrentDesktop()` (never released), enumerates all desktops creating one RCW each via `GetAt` (never released; only the `IObjectArray` is `ReleaseComObject`'d), and calls the COM `GetCount()` **inside the loop condition** (one cross-process call per iteration). `GetDesktopAtIndex` also keeps iterating after finding its index instead of breaking. Not a hard leak (GC finalizes RCWs) but constant cross-process COM pressure; cache the count and release the per-desktop objects.

### L9. COM objects used across apartments
The VDAPI wrappers are created on the STA UI thread (App constructor) but called every 100 ms from MTA worker threads *and* from UI event handlers. These immersive-shell interfaces tolerate it in practice, but it's technically incorrect COM usage and a standing suspect for machine-specific `InvalidCastException`/`RPC_E_WRONG_THREAD`-style bug reports.

### L10. Duplicate hotkeys execute their action multiple times
`Source/App/App.cs:554-563` — `_hotKeyPressed` runs **every** matching `HotKeyAction`. Once H6 is fixed (so duplicates no longer crash at registration), a combo listed twice (e.g., custom hotkey identical to a feature hotkey) will switch desktops twice per keypress. De-duplicate by (Modifiers, Keys) when building the list.

### L11. `Screen.AllScreens` index race
`Source/App/App.cs:204`, `Source/Forms/SwitchNotificationForm.cs:49` — the screen count is read in one place and indexed in another; unplugging a monitor between the two throws `IndexOutOfRangeException` inside the overlay constructor (on the UI thread).

### L12. `GetKeys()` returns `#`-prefixed pseudo-keys
`Source/App/Settings.cs:189-191`, `:281-287` — `_createMergedSettingsDictionary(true, true)` injects defaults under `"#" + key`; `GetKeys()` exposes those comment-keys to callers. The only current consumer (`SetupHotKeys`) survives because it filters on `StartsWith("hotkeys.")`, but any future `GetKeys()` consumer will trip over `#feature...` keys.

### L13. Dead/misleading code
- `App.cs:49` — `KeyboardHooksJumpToDesktop` field is never used.
- `AppForm.cs:120-131` — the **name** icon's click handler checks the **number** icon's setting (`feature.showDesktopNumberInIconTray.clickToOpenTaskView`); if intentional, the setting is misnamed.
- `Icons.cs:114` — `if (false && ...)` debug block; `bgBrush` created but never used.
- `ErrorForm.OpenIssueOnGithub` embeds the entire log in the URL query — can exceed browser/ShellExecute URL limits and silently fail for long logs.
- `Emoji.HasEmoji` recompiles a ~10 KB regex on every icon generation — cache a `static readonly Regex` with `RegexOptions.Compiled`.

### L14. All `*.config` files in the folder load in undefined order
`Source/App/Settings.cs:420-426` — `LoadConfig` loads **every** `.config` file in `%appdata%\WindowsVirtualDesktopHelper` in `Directory.GetFiles` order (NTFS-ordering, not guaranteed), with later files overwriting earlier keys. A stray backup ("`... - Copy.config`") silently changes effective settings. At minimum, sort the file list and log the merge order (the log does list used files, which helps).

### L15. Splash screen is gated on the overlay feature and inherits its crash
`Source/App/App.cs:704-715` — `ShowSplash` requires `feature.showDesktopSwitchOverlay` to also be true (the default's doc does say "Overlays must be enabled", so this is semi-intentional but surprising: disabling switch overlays silently disables the splash). It also constructs a `SwitchNotificationForm`, so it participates in M1's `duration = 0` crash via `feature.showSplashScreen.duration`.

---

## Suggested fix priority

1. **H1 + H2** — resource leaks that degrade every long-running install (this is a tray app; "long-running" is the normal case).
2. **H6** — most likely cause of hard-to-reproduce "app won't start" reports (any other app owning a configured hotkey).
3. **H7 + M11** — explorer restarts are routine; auto-recovery removes a whole class of support issues.
4. **H3/H4/H5/M4/M8** — settings-system robustness batch; these share code paths and are best fixed together with `InvariantCulture` + first-colon split + tolerant getters + safe unescape.
5. **M1, M5, M6, M7, M12** — quick, low-risk fixes.
6. Docs: **M9** (one-character fix in two files).

## Explicit non-findings

- The recent AppForm hidden-window fix (`SetVisibleCore` override, commit `82a2689`) looks correct: the handle is force-created for `Invoke`/tray usage and visibility is permanently suppressed; the startup wiring moved to `OnHandleCreated` with a `_startupDone` guard.
- `Loader.GetImplementationForOS` build/revision ranges and the fallback chain look consistent with the version history; `LoadImplementation` testing `Current()` twice (also done by `LoadImplementationWithFallback`) is redundant but harmless.
- The vtable declarations in the COM interfaces were spot-checked for ordering against usage (only early slots — `GetCount`, `GetCurrentDesktop`, `GetDesktops`, `GetAdjacentDesktop`, `SwitchDesktop` — are actually invoked); later unverified slots (`SwitchRemoteDesktop(..., Enum ...)`) are never called.
- The `Mutex` single-instance check in `Main.cs` is fine for its purpose (never released explicitly, but process exit abandons it safely).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using WindowsVirtualDesktopHelper.VirtualDesktopAPI;
using WindowsVirtualDesktopHelper.WindowsHotKeyAPI;
using System.Drawing.Text;
using System.Drawing;
using System.Globalization;

namespace WindowsVirtualDesktopHelper {
	class App {

		public IVirtualDesktopManager VDAPI = null;
		public string CurrentVDDisplayName = null;
		public uint CurrentVDDisplayNumber = 0;
		public int CurrentVDDisplayCount = 1;
		public SettingsForm SettingsForm;
		public AppForm AppForm;
		public string CurrentSystemThemeName = null;
		private Dictionary<int, IntPtr> VDDToLastFocusedWin = new Dictionary<int, IntPtr>();
		public List<string> FGWindowHistory = new List<string>(); // needed to detect if Task View was open
		public IntPtr LastForegroundhWnd = IntPtr.Zero;
		KeyboardHook KeyboardHooksJumpToDesktop = null;

		public static string DetectedVDImplementation = null;

		public static App Instance;

	

		public App() {
			// Set the app instance global
			App.Instance = this;

			// Settings
			{
				Util.Logging.WriteLine("Using config file(s):\r\n   "+string.Join("\r\n   ", Settings.GetUsedConfigFiles()));
				Util.Logging.WriteLine("Config: \r\n   "+Settings.GetSettingsAsString().Replace("\n", "\r\n   "));
			}

			// Test global error form:
			//throw new Exception("test exception!");

			// Load the implementation
			try {
				this.LoadVDAPI();
				this.LoadVDDisplayInfo();
				//throw new Exception("Test error", new Exception("Test inner error"));
			} catch (Exception e) {
				throw e;
			}

			// Load theme
			this.CurrentSystemThemeName = this.GetSystemThemeName();

			this.CurrentVDDisplayCount = this.GetVDDisplayCount();

			// Create the app form, which acts as our ui main thread (we need such a main thread form for many of the win api calls)
			this.AppForm = new AppForm();

			// Create settings form
			this.SettingsForm = new SettingsForm();

			// Hot keys
			this.SetupHotKeys();

		}


		public void LoadVDAPI() {
			App.DetectedVDImplementation = VirtualDesktopAPI.Loader.GetImplementationForOS();
			this.VDAPI = VirtualDesktopAPI.Loader.LoadImplementationWithFallback(App.DetectedVDImplementation);
		}

		public void LoadVDDisplayInfo() {
			try {
				this.CurrentVDDisplayNumber = this.GetVDDisplayNumber(true);
			} catch (Exception e) {
				throw new Exception("LoadVDDisplayInfo: could not get current display number: " + e.Message, e);
			}
			try {
				this.CurrentVDDisplayName = this.GetVDDisplayName(true);
			} catch (Exception e) {
				throw new Exception("LoadVDDisplayInfo: could not get current display name: " + e.Message, e);
			}
		}

		public uint GetVDDisplayNumber(bool throwException) {
			try {
				return this.VDAPI.Current();
			} catch (Exception e) {
				if (throwException) throw new Exception("GetVDDisplayNumber: could not get current display number: " + e.Message, e);
				else return 0;
			}
		}

		public int GetVDDisplayCount() {
			return this.VDAPI.DisplayCount();
		}

		public string GetVDDisplayName(bool throwException) {
			try {
				return this.VDAPI.CurrentDisplayName();
			} catch (Exception e) {
				if (throwException) throw new Exception("GetVDDisplayName: could not get current display number: " + e.Message, e);
				else return "Unknown";
			}
		}

		public void SetupHotKeys() {
			if(this.KeyboardHooksJumpToDesktop != null) {
				this.KeyboardHooksJumpToDesktop.Dispose();
				this.KeyboardHooksJumpToDesktop = null;
			}

			if (Settings.GetBool("feature.useHotKeysToJumpToDesktop")) {

				ModifierKeys _HotKeysToJumpToDesktop() {
					var hotKey = Settings.GetString("feature.useHotKeysToJumpToDesktop.hotKey");
					if(hotKey == "Alt") return WindowsHotKeyAPI.ModifierKeys.Alt;
					if(hotKey == "AltShift") return WindowsHotKeyAPI.ModifierKeys.Alt | WindowsHotKeyAPI.ModifierKeys.Shift;
					if(hotKey == "Ctrl") return WindowsHotKeyAPI.ModifierKeys.Control;
					if(hotKey == "CtrlAlt") return WindowsHotKeyAPI.ModifierKeys.Control | WindowsHotKeyAPI.ModifierKeys.Alt;
					throw new Exception("invalid modifier");
				}

				this.KeyboardHooksJumpToDesktop = new KeyboardHook();
				this.KeyboardHooksJumpToDesktop.KeyPressed += new EventHandler<KeyPressedEventArgs>(HotKeyPressed);
				ModifierKeys modifier = _HotKeysToJumpToDesktop();
				var keys = new List<Keys>() { 
					Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, 
					Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9
				};
				foreach (var key in keys) {
					this.KeyboardHooksJumpToDesktop.RegisterHotKey(modifier, key);
				}
			}
		}

		public void HotKeyPressed(object sender, KeyPressedEventArgs e) {
			int? desktopNumber = null;
			if (e.Key == Keys.D1 || e.Key == Keys.NumPad1) desktopNumber = 1;
			else if (e.Key == Keys.D2 || e.Key == Keys.NumPad2) desktopNumber = 2;
			else if (e.Key == Keys.D3 || e.Key == Keys.NumPad3) desktopNumber = 3;
			else if (e.Key == Keys.D4 || e.Key == Keys.NumPad4) desktopNumber = 4;
			else if (e.Key == Keys.D5 || e.Key == Keys.NumPad5) desktopNumber = 5;
			else if (e.Key == Keys.D6 || e.Key == Keys.NumPad6) desktopNumber = 6;
			else if (e.Key == Keys.D7 || e.Key == Keys.NumPad7) desktopNumber = 7;
			else if (e.Key == Keys.D8 || e.Key == Keys.NumPad8) desktopNumber = 8;
			else if (e.Key == Keys.D9 || e.Key == Keys.NumPad9) desktopNumber = 9;
			if (desktopNumber != null && App.Instance.VDAPI.GetVDCount() > desktopNumber.Value -1) {
				storeLastWinFocused(CurrentVDDisplayNumber);
				this.SwitchToDesktop(desktopNumber.Value - 1);
			}
		}

		public void Exit() {
			Application.Exit();
			System.Environment.Exit(0);
		}

		public void MonitorVDisplayCount() {
			var thread = new Thread(new ThreadStart(_MonitorVDDisplayCount));
			thread.Start();
		}
		private void _MonitorVDDisplayCount() {
			while (true) {
				try {
					var newCurrentVDDisplayCount = this.GetVDDisplayCount();
					if (newCurrentVDDisplayCount != CurrentVDDisplayCount) {
						CurrentVDDisplayCount = newCurrentVDDisplayCount;
						//Debug.WriteLine("Update Count: " + Thread.CurrentThread.ManagedThreadId);
					}
					System.Threading.Thread.Sleep(100);
				} catch (Exception e) {
					Util.Logging.WriteLine("App: Error: MonitorVDDisplayCount: " + e.Message);
					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		public void MonitorFGWindowName() {
			var thread = new Thread(new ThreadStart(_MonitorFGWindowName));
			thread.Start();
		}
		private void _MonitorFGWindowName() {
			while (true) {
				try {
					var fgWindowName = Util.OS.GetForegroundWindowName();
					FGWindowHistory.Add(fgWindowName);
					var maxHistory = 20;
					if (FGWindowHistory.Count > maxHistory) {
						FGWindowHistory.RemoveRange(0, FGWindowHistory.Count - maxHistory);
					}
					if (LastForegroundhWnd == IntPtr.Zero) {
						LastForegroundhWnd = Util.OS.GetFolderViewHandle();
					}
					System.Threading.Thread.Sleep(20);
				} catch (Exception e) {
					Util.Logging.WriteLine("App: Error: MonitorFGWindowName: " + e.Message);
					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		public void MonitorVDSwitch() {
			var thread = new Thread(new ThreadStart(_MonitorVDSwitch));
			//var thread2 = new Thread(new ThreadStart(delegate { UpdateSettingFormSafe(); }));
			thread.Start();
			//thread2.Start();
		}

		private void _MonitorVDSwitch() {
			while (true) {
				try {
					var newVDDisplayNumber = this.GetVDDisplayNumber(false);
					if (newVDDisplayNumber != this.CurrentVDDisplayNumber) {
						//Util.Logging.WriteLine("Switched to " + newVDDisplayName);
						//Debug.WriteLine("Switch VD: " + Thread.CurrentThread.ManagedThreadId);
						this.CurrentVDDisplayName = this.GetVDDisplayName(false);
						this.CurrentVDDisplayNumber = newVDDisplayNumber;
						VDSwitchedSafe();
					}
					else {
						storeLastWinFocused(newVDDisplayNumber);
					}
					System.Threading.Thread.Sleep(100);
				} catch (Exception e) {
					Util.Logging.WriteLine("App: Error: MonitorVDSwitch: " + e.Message);
					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		public void VDSwitchedSafe() {
			if (this.AppForm.InvokeRequired) {
				Action safeAction = delegate { VDSwitchedSafe(); };
				this.AppForm.Invoke(safeAction);
			} else {
				this.UIUpdateIconForVDDisplayNumber(this.CurrentSystemThemeName, this.CurrentVDDisplayNumber, this.CurrentVDDisplayName);
				this.UIUpdateIconForVDDisplayName(this.CurrentSystemThemeName, this.CurrentVDDisplayName);
				this.UIUpdateNextPrevIconVisibility(this.CurrentSystemThemeName);
				if (Settings.GetBool("feature.showDesktopSwitchOverlay")) {
					this.AppForm.Invoke((Action)(() => {
						SwitchNotificationForm.CloseAllNotifications(this.AppForm);
						if (Settings.GetBool("feature.showDesktopSwitchOverlay.showOnAllMonitors")) {
							for (var i = 0; i < Screen.AllScreens.Length; i++) {
								var form = new SwitchNotificationForm(i);
								form.LabelText = this.CurrentVDDisplayName;
								form.DisplayTimeMS = Settings.GetInt("feature.showDesktopSwitchOverlay.duration");
								form.Show();
							}
						} else {
							var form = new SwitchNotificationForm();
							form.LabelText = this.CurrentVDDisplayName;
							form.DisplayTimeMS = Settings.GetInt("feature.showDesktopSwitchOverlay.duration");
							form.Show();
						}
					}));
				}
			}
		}

		public void VDSwitched() {
			this.UIUpdateIconForVDDisplayNumber(this.CurrentSystemThemeName, this.CurrentVDDisplayNumber, this.CurrentVDDisplayName);
			this.UIUpdateIconForVDDisplayName(this.CurrentSystemThemeName, this.CurrentVDDisplayName);
			// this cause flicker when switch VD from windows Task View
			if (Settings.GetBool("feature.showDesktopSwitchOverlay")) {
				this.AppForm.Invoke((Action)(() => {
					SwitchNotificationForm.CloseAllNotifications(this.AppForm);
					if (Settings.GetBool("feature.showDesktopSwitchOverlay.showOnAllMonitors")) {
						for (var i = 0; i < Screen.AllScreens.Length; i++) {
							var form = new SwitchNotificationForm(i);
							form.LabelText = this.CurrentVDDisplayName;
							form.DisplayTimeMS = Settings.GetInt("feature.showDesktopSwitchOverlay.duration");
							form.Show();
						}
					} else {
						var form = new SwitchNotificationForm();
						form.LabelText = this.CurrentVDDisplayName;
						form.DisplayTimeMS = Settings.GetInt("feature.showDesktopSwitchOverlay.duration");
						form.Show();
					}
				}));
			}
		}

		public void MonitorSystemThemeSwitch() {
			var thread = new Thread(new ThreadStart(_MonitorSystemThemeSwitch));
			thread.Start();
		}

		private void _MonitorSystemThemeSwitch() {
			while (true) {
				try {
					var newSystemThemeName = this.GetSystemThemeName();
					if (newSystemThemeName != this.CurrentSystemThemeName) {
						this.CurrentSystemThemeName = newSystemThemeName;
						ThemeSwitched();
					}
					System.Threading.Thread.Sleep(1000);
				} catch (Exception e) {
					Util.Logging.WriteLine("App: Error: " + e.Message);
					System.Threading.Thread.Sleep(5000);
				}
			}
		}

		public void ThemeSwitched() {
			this.UIUpdateIcons();
		}

		public void ShowAbout() {
			this.AppForm.Invoke((Action)(() => {
				var form = new AboutForm();
				form.Show();
			}));
		}

		public void ShowSettings() {
			this.SettingsForm.Show();
		}

		public void ShowSplash() {
			if(Settings.GetBool("feature.showSplashScreen")) {
				if(Settings.GetBool("feature.showDesktopSwitchOverlay")) {
					this.AppForm.Invoke((Action)(() => {
						var form = new SwitchNotificationForm();
						form.DisplayTimeMS = Settings.GetInt("feature.showSplashScreen.duration");
						form.LabelText = Settings.GetString("feature.showSplashScreen.text");
						form.Show();
					}));
				}
			}
		}

		public void SwitchDesktopBackward() {
			// We try the virtual desktop implementation API, but fallback to shortcut keys if it fails...
			try {
				App.Instance.VDAPI.SwitchBackward();
			} catch(Exception e) {
				Util.OS.DesktopBackwardBySimulatingShortcutKey();
			}
		}

		public void SwitchDesktopForward() {
			// We try the virtual desktop implementation API, but fallback to shortcut keys if it fails...
			try {
				App.Instance.VDAPI.SwitchForward();
			} catch (Exception e) {
				Util.OS.DesktopForwardBySimulatingShortcutKey();
			}
		}

		public void SwitchToDesktop(int number) {
			// We try the virtual desktop implementation API, but fallback to shortcut keys if it fails...
			try {
				App.Instance.VDAPI.SwitchToDesktop(number);
			} catch (Exception e) {
				return;
			} 
			this.restorePrevWinFocus(number);
		}

		public void OpenURL(string url) {
			url = url.Replace("&", "^&"); //TODO: is this really needed?
			Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
		}

		public void EnableStartupWithWindows() {
			// https://stackoverflow.com/questions/674628/how-do-i-set-a-program-to-launch-at-startup
			try {
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
				key.SetValue(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title, Application.ExecutablePath);
			} catch (Exception e) {
				throw new Exception("EnableStartupWithWindows: could not set registry value: " + e.Message);
			}
		}

		public string GetSystemThemeName() {
			var themeSetting = Settings.GetString("general.theme");
			if(themeSetting == "auto") {
				return Util.OS.IsSystemLightThemeModeEnabled() == true ? "light" : "dark";
			} else if(themeSetting == "light") {
				return "light";
			} else if(themeSetting == "dark") {
				return "dark";
			} else {
				throw new Exception("invalid theme setting general.theme: "+ themeSetting);
			}
			
		}

		public void DisableStartupWithWindows() {
			// https://stackoverflow.com/questions/674628/how-do-i-set-a-program-to-launch-at-startup
			try {
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
				key.DeleteValue(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title, false);
			} catch (Exception e) {
				throw new Exception("EnableStartupWithWindows: could not delete registry value: " + e.Message);
			}
		}


		public void storeLastWinFocused(uint currVDDWinNo) {
			IntPtr hWnd = Util.OS.GetForegroundWindow();
			if (hWnd != IntPtr.Zero) {
				if (VDDToLastFocusedWin.ContainsKey((int)currVDDWinNo)) {
					VDDToLastFocusedWin[(int)currVDDWinNo] = hWnd;
				}
				else {
					VDDToLastFocusedWin.Add((int)currVDDWinNo, hWnd);
				}
			}	
		}

		public void restorePrevWinFocus(int prevVDDWinNo) {
			if (VDDToLastFocusedWin.ContainsKey(prevVDDWinNo)) {
				IntPtr lastWindowHandle = VDDToLastFocusedWin[prevVDDWinNo];
				if (Util.OS.IsWindow(lastWindowHandle)) {
					Util.OS.SetForegroundWindow(lastWindowHandle);
				}
			}
		}

		#region UI

		public void UIUpdate() {
			// Icons
			UIUpdateIcons();
		}

		public void UIUpdateIcons() {
			var theme = App.Instance.CurrentSystemThemeName;
			// Set current display icons
			UIUpdateIconForVDDisplayNumber(theme, App.Instance.CurrentVDDisplayNumber, App.Instance.CurrentVDDisplayName);
			UIUpdateIconForVDDisplayName(theme, App.Instance.CurrentVDDisplayName);
			UIUpdateNextPrevIconVisibility(theme);
			// Visibility by feature
			this.AppForm.notifyIconName.Visible = Settings.GetBool("feature.showDesktopNameInIconTray");
			this.AppForm.notifyIconNumber.Visible = Settings.GetBool("feature.showDesktopNumberInIconTray");
		}


		public void UIUpdateIconForVDDisplayNumber(string theme, uint number, string name) {
			number++;
			this.AppForm.notifyIconNumber.Icon = Util.Icons.GenerateNotificationIcon(number.ToString(), theme, this.AppForm.DeviceDpi, false, FontStyle.Bold);
		}

		public void UIUpdateIconForVDDisplayName(string theme, string name) {
			var nameToShow = name;
			if(nameToShow == null) nameToShow = "";
			if(nameToShow.Length > 1) nameToShow = new StringInfo(nameToShow).SubstringByTextElements(0, 1);
			this.AppForm.notifyIconName.Icon = Util.Icons.GenerateNotificationIcon(nameToShow, theme, this.AppForm.DeviceDpi, false);
		}

		public void UIUpdateNextPrevIconVisibility(string theme) {
			if(Settings.GetBool("feature.showPrevNextIcons")) {
				int count = App.Instance.CurrentVDDisplayCount - 1;
				var prevChar = Settings.GetString("feature.showPrevNextIcons.prevChar");
				var nextChar = Settings.GetString("feature.showPrevNextIcons.nextChar");
				var hasNextDesktop = count != 0 && App.Instance.CurrentVDDisplayNumber != count;
				var hasPrevDesktop = App.Instance.CurrentVDDisplayNumber != 0;
				// Update prev/next icons
				this.AppForm.notifyIconPrev.Icon = Util.Icons.GenerateNotificationIcon(prevChar, theme, this.AppForm.DeviceDpi, true, FontStyle.Regular, hasPrevDesktop ? 1.0f : Settings.GetDouble("theme.icons.disabledOpacity"));
				this.AppForm.notifyIconNext.Icon = Util.Icons.GenerateNotificationIcon(nextChar, theme, this.AppForm.DeviceDpi, true, FontStyle.Regular, hasNextDesktop ? 1.0f : Settings.GetDouble("theme.icons.disabledOpacity"));
				// Show or hide?
				if(Settings.GetBool("feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds")) {
					this.AppForm.notifyIconNext.Visible = hasNextDesktop;
					this.AppForm.notifyIconPrev.Visible = hasPrevDesktop;
				} else {
					this.AppForm.notifyIconNext.Visible = true;
					this.AppForm.notifyIconPrev.Visible = true;
				}
			} else {
				this.AppForm.notifyIconNext.Visible = false;
				this.AppForm.notifyIconPrev.Visible = false;
			}
		}

		#endregion

		#region Misc

		public void OpenEmailContact() {
			App.Instance.OpenURL("mailto:dan@dankrusi.com");
		}

		public void OpenAboutPage() {
			App.Instance.OpenURL("https://github.com/dankrusi/WindowsVirtualDesktopHelper");
		}

		public void OpenDonatePage() {
			App.Instance.OpenURL("https://www.paypal.com/donate/?hosted_button_id=BG5FYMAHFG9V6");
		}

		#endregion

	}
}

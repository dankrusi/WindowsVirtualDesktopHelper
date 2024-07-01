using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using WindowsVirtualDesktopHelper.VirtualDesktopAPI;
using WindowsVirtualDesktopHelper.WindowsHotKeyAPI;

namespace WindowsVirtualDesktopHelper {
	class App {

		public IVirtualDesktopManager VDAPI = null;
		public string CurrentVDDisplayName = null;
		public uint CurrentVDDisplayNumber = 0;
		public int CurrentVDDisplayCount = 1;
		public SettingsForm SettingsForm;
		public string CurrentSystemThemeName = null;
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
				Util.Logging.WriteLine("Using config file: "+AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.Save();
				Properties.Settings.Default.Reload();
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

			// Create the settings form, which acts as our ui main thread
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

			if (this.SettingsForm.UseHotKeysToJumpToDesktop()) {
				this.KeyboardHooksJumpToDesktop = new KeyboardHook();
				this.KeyboardHooksJumpToDesktop.KeyPressed += new EventHandler<KeyPressedEventArgs>(HotKeyPressed);
				ModifierKeys modifier = this.SettingsForm.HotKeysToJumpToDesktop();
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
			if(desktopNumber != null) this.SwitchToDesktop(desktopNumber.Value - 1);
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
					System.Threading.Thread.Sleep(100);
				} catch (Exception e) {
					Util.Logging.WriteLine("App: Error: MonitorVDSwitch: " + e.Message);
					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		public void VDSwitchedSafe() {
			if (this.SettingsForm.InvokeRequired) {
				Action safeAction = delegate { VDSwitchedSafe(); };
				this.SettingsForm.Invoke(safeAction);
			} else {
				this.SettingsForm.UpdateIconForVDDisplayNumber(this.CurrentSystemThemeName, this.CurrentVDDisplayNumber, this.CurrentVDDisplayName);
				this.SettingsForm.UpdateIconForVDDisplayName(this.CurrentSystemThemeName, this.CurrentVDDisplayName);
				this.SettingsForm.UpdateNextPrevIconVisibility();
				if (this.SettingsForm.ShowOverlay()) {
					this.SettingsForm.Invoke((Action)(() => {
						SwitchNotificationForm.CloseAllNotifications(this.SettingsForm);
						if (this.SettingsForm.OverlayShowOnAllMonitors()) {
							for (var i = 0; i < Screen.AllScreens.Length; i++) {
								var form = new SwitchNotificationForm(i);
								form.LabelText = this.CurrentVDDisplayName;
								form.DisplayTimeMS = this.SettingsForm.OverlayDurationMS();
								form.Show();
							}
						} else {
							var form = new SwitchNotificationForm();
							form.LabelText = this.CurrentVDDisplayName;
							form.DisplayTimeMS = this.SettingsForm.OverlayDurationMS();
							form.Show();
						}
					}));
				}
			}
		}

		public void VDSwitched() {
			this.SettingsForm.UpdateIconForVDDisplayNumber(this.CurrentSystemThemeName, this.CurrentVDDisplayNumber, this.CurrentVDDisplayName);
			this.SettingsForm.UpdateIconForVDDisplayName(this.CurrentSystemThemeName, this.CurrentVDDisplayName);
			// this cause flicker when switch VD from windows Task View
			if (this.SettingsForm.ShowOverlay()) {
				this.SettingsForm.Invoke((Action)(() => {
					SwitchNotificationForm.CloseAllNotifications(this.SettingsForm);
					if (this.SettingsForm.OverlayShowOnAllMonitors()) {
						for (var i = 0; i < Screen.AllScreens.Length; i++) {
							var form = new SwitchNotificationForm(i);
							form.LabelText = this.CurrentVDDisplayName;
							form.DisplayTimeMS = this.SettingsForm.OverlayDurationMS();
							form.Show();
						}
					} else {
						var form = new SwitchNotificationForm();
						form.LabelText = this.CurrentVDDisplayName;
						form.DisplayTimeMS = this.SettingsForm.OverlayDurationMS();
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
			this.SettingsForm.UpdateIconsForTheme(this.CurrentSystemThemeName);
		}

		public void ShowAbout() {
			this.SettingsForm.Invoke((Action)(() => {
				var form = new AboutForm();
				form.Show();
			}));
		}

		public void ShowSplash() {
			if (this.SettingsForm.ShowOverlay()) {
				this.SettingsForm.Invoke((Action)(() => {
					var form = new SwitchNotificationForm();
					form.DisplayTimeMS = 2000;
					form.LabelText = "Virtual Desktop Helper";
					form.Show();
				}));
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
				Util.OS.DesktopForwardBySimulatingShortcutKey();
			}
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
			return Util.OS.IsSystemLightThemeModeEnabled() == true ? "light" : "dark";
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

		public void OpenEmailContact() {
			App.Instance.OpenURL("mailto:dan@dankrusi.com");
		}

		public void OpenAboutPage() {
			App.Instance.OpenURL("https://github.com/dankrusi/WindowsVirtualDesktopHelper");
		}

		public void OpenDonatePage() {
			App.Instance.OpenURL("https://www.paypal.com/donate/?hosted_button_id=BG5FYMAHFG9V6");
		}

	}
}

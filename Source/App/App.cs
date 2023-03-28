using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using WindowsVirtualDesktopHelper.VirtualDesktopAPI;

namespace WindowsVirtualDesktopHelper {
    class App {

        public IVirtualDesktopManager VDAPI = null;
        public string CurrentVDDisplayName = null;
        public uint CurrentVDDisplayNumber = 0;
        public SettingsForm SettingsForm;
        public string CurrentSystemThemeName = null;
        public List<string> FGWindowHistory = new List<string>(); // needed to detect if Task View was open

        public static string DetectedVDImplementation = null;

        public static App Instance;

        public App() {
            // Set the app instance global
            App.Instance = this;

            // Settings
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Save();
            }

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

            // Create the settings form, which acts as our ui main thread
            this.SettingsForm = new SettingsForm();
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

        public string GetVDDisplayName(bool throwException) {
            try {
                return this.VDAPI.CurrentDisplayName();
            } catch (Exception e) {
                if (throwException) throw new Exception("GetVDDisplayName: could not get current display number: " + e.Message, e);
                else return "Unknown";
            }
        }

        

        public void Exit() {
            Application.Exit();
            System.Environment.Exit(0);
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
                    if(FGWindowHistory.Count > maxHistory) {
                        FGWindowHistory.RemoveRange(0, FGWindowHistory.Count - maxHistory);
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
            thread.Start();
        }

        private void _MonitorVDSwitch() {
            while (true) {
                try {
                    var newVDDisplayNumber = this.GetVDDisplayNumber(false);
                    if (newVDDisplayNumber != this.CurrentVDDisplayNumber) {
                        //Util.Logging.WriteLine("Switched to " + newVDDisplayName);
                        this.CurrentVDDisplayName = this.GetVDDisplayName(false);
                        this.CurrentVDDisplayNumber = newVDDisplayNumber;
                        VDSwitched();
                    }
                    System.Threading.Thread.Sleep(100);
                }catch(Exception e) {
                    Util.Logging.WriteLine("App: Error: MonitorVDSwitch: " + e.Message);
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        public void VDSwitched() {
            this.SettingsForm.UpdateIconForVDDisplayNumber(this.CurrentSystemThemeName, this.CurrentVDDisplayNumber, this.CurrentVDDisplayName);
            this.SettingsForm.UpdateIconForVDDisplayName(this.CurrentSystemThemeName, this.CurrentVDDisplayName);
            if (this.SettingsForm.ShowOverlay()) {
                this.SettingsForm.Invoke((Action)(() => {
                    var form = new SwitchNotificationForm();
                    form.LabelText = this.CurrentVDDisplayName;
                    form.DisplayTimeMS = this.SettingsForm.OverlayDurationMS();
                    form.Show();
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

        public void OpenURL(string url) {
            url = url.Replace("&", "^&"); //TODO: is this really needed?
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        public void EnableStartupWithWindows() {
            // https://stackoverflow.com/questions/674628/how-do-i-set-a-program-to-launch-at-startup
            try {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key.SetValue(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title, Application.ExecutablePath);
            } catch(Exception e) {
                throw new Exception("EnableStartupWithWindows: could not set registry value: "+e.Message);
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

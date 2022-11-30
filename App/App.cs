using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper {
    class App {

        public VirtualDesktopIndicator.Native.VirtualDesktop.IVirtualDesktopManager VDAPI = null;
        public string CurrentVDDisplayName = null;
        public uint CurrentVDDisplayNumber = 0;
        public SettingsForm SettingsForm;

        public static App Instance;

        public App() {
            // Set the app instance global
            App.Instance = this;

            // Load the implementation
            //const int win11MinBuild = 22000;
            //if (Environment.OSVersion.Version.Build >= win11MinBuild) { // note this method requires a app manifest declaring win11 support otherwise a 'lie' is returned
            try {
                this.LoadVDAPI();
                this.LoadVDDisplayInfo();
                //throw new Exception("Test error", new Exception("Test inner error"));
            } catch (Exception e) {
                throw e;
            }
            // Create the settings form, which acts as our ui main thread
            this.SettingsForm = new SettingsForm();
        }


        public void LoadVDAPI() {
            bool isWindows11 = true;
            try {
                isWindows11 = IsWindows11();
            } catch(Exception e) {
                throw new Exception("LoadVDAPI: could not determine Windows version: " + e.Message, e);
            }
            if (isWindows11) {
                Console.WriteLine("Detected Windows 11");
                try {
                    this.VDAPI = new VirtualDesktopIndicator.Native.VirtualDesktop.Implementation.VirtualDesktopWin11();
                } catch (Exception e) {
                    throw new Exception("LoadVDAPI: could not load VirtualDesktop API implementation VirtualDesktopWin11: " + e.Message, e);
                }
            } else {
                Console.WriteLine("Detected Windows 10");
                try {
                    this.VDAPI = new VirtualDesktopIndicator.Native.VirtualDesktop.Implementation.VirtualDesktopWin10();
                } catch (Exception e) {
                    throw new Exception("LoadVDAPI: could not load VirtualDesktop API implementation VirtualDesktopWin10: " + e.Message, e);
                }
            }
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

        public static bool IsWindows11() {
            // via https://stackoverflow.com/questions/69038560/detect-windows-11-with-net-framework-or-windows-api
            var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            var currentBuildStr = (string)reg.GetValue("CurrentBuild");
            var currentBuild = int.Parse(currentBuildStr);
            Console.WriteLine("Windows Build Version: "+currentBuildStr);
            return currentBuild >= 22000;
        }

        public void Exit() {
            Application.Exit();
            System.Environment.Exit(0);
        }

        public void MonitorVDSwitch() {
            var thread = new Thread(new ThreadStart(_MonitorVDSwitch));
            thread.Start();
        }

        private void _MonitorVDSwitch() {
            while (true) {
                var newVDDisplayNumber = this.GetVDDisplayNumber(false); 
                if (newVDDisplayNumber != this.CurrentVDDisplayNumber) {
                    //Console.WriteLine("Switched to " + newVDDisplayName);
                    this.CurrentVDDisplayName = this.GetVDDisplayName(false);
                    this.CurrentVDDisplayNumber = newVDDisplayNumber;
                    VDSwitched();
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        public void VDSwitched() {
            this.SettingsForm.UpdateIconForVDDisplayNumber(this.CurrentVDDisplayNumber);
            this.SettingsForm.Invoke((Action)(() =>
            {
                var form = new SwitchNotificationForm();
                form.LabelText = this.CurrentVDDisplayName;
                form.Show();
            }));
        }

        public void ShowAbout() {
            this.SettingsForm.Invoke((Action)(() => {
                var form = new AboutForm();
                form.Show();
            }));
        }

        public void ShowSplash() {
            this.SettingsForm.Invoke((Action)(() => {
                var form = new SwitchNotificationForm();
                form.DisplayTimeMS = 2000;
                form.LabelText = "Virtual Desktop Helper";
                form.Show();
            }));
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

        public bool IsDarkThemeMode() {
            // https://learn.microsoft.com/en-us/answers/questions/715081/how-to-detect-windows-dark-mode.html
            try {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", true);
                var ret = key.GetValue("SystemUsesLightTheme");
                var retNumber = (int)ret; // 1 == light
                if (retNumber == 1) return false;
                else return true;
            } catch (Exception e) {
                throw new Exception("IsDarkThemeMode: could not get dark/light theme setting: " + e.Message);
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

    }
}

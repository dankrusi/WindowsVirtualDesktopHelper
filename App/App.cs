using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            if (IsWindows11()) {
                this.VDAPI = new VirtualDesktopIndicator.Native.VirtualDesktop.Implementation.VirtualDesktopWin11();
            } else {
                this.VDAPI = new VirtualDesktopIndicator.Native.VirtualDesktop.Implementation.VirtualDesktopWin10();
            }
            this.CurrentVDDisplayName = this.VDAPI.CurrentDisplayName();
            this.CurrentVDDisplayNumber = this.VDAPI.Current();

            // Create the settings form, which acts as our ui main thread
            this.SettingsForm = new SettingsForm();


        }

        public static bool IsWindows11() {
            // via https://stackoverflow.com/questions/69038560/detect-windows-11-with-net-framework-or-windows-api
            var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            var currentBuildStr = (string)reg.GetValue("CurrentBuild");
            var currentBuild = int.Parse(currentBuildStr);

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
                var newVDDisplayNumber = this.VDAPI.Current(); 
                if (newVDDisplayNumber != this.CurrentVDDisplayNumber) {
                    //Console.WriteLine("Switched to " + newVDDisplayName);
                    this.CurrentVDDisplayName = this.VDAPI.CurrentDisplayName();
                    this.CurrentVDDisplayNumber = this.VDAPI.Current();
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
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

    }
}

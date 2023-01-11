using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsVirtualDesktopHelper.Util {
    public class OS {
        public static bool IsWindows11() {
            return GetWindowsBuildVersion() >= 22000;
        }

        public static void OpenTaskView() {
            System.Diagnostics.Process.Start("explorer.exe", "shell:::{3080F90E-D7AD-11D9-BD98-0000947B0257}");
        }

        public static int GetWindowsBuildVersion() {
            // via https://stackoverflow.com/questions/69038560/detect-windows-11-with-net-framework-or-windows-api
            var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var currentBuildStr = (string)reg.GetValue("CurrentBuild");
            var currentBuild = int.Parse(currentBuildStr);
            return currentBuild;
        }

        public static string GetWindowsProductName() {
            var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var retStr = (string)reg.GetValue("ProductName");
            return retStr;
        }

        public static string GetWindowsDisplayVersion() {
            var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var retStr = (string)reg.GetValue("DisplayVersion");
            return retStr;
        }

        public static int GetWindowsReleaseId() {
            var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var retStr = (string)reg.GetValue("ReleaseId");
            var retInt = int.Parse(retStr);
            return retInt;
        }
    }
}

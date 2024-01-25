﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI {
	public class Loader {

		public const string VirtualDesktopWin11_23H2_2921 = "VirtualDesktopWin11_23H2_2921";
		public const string VirtualDesktopWin11_23H2 = "VirtualDesktopWin11_23H2";
		public const string VirtualDesktopWin11_22H2 = "VirtualDesktopWin11_22H2";
		public const string VirtualDesktopWin11_21H2 = "VirtualDesktopWin11_21H2";
		public const string VirtualDesktopWin11_Insider = "VirtualDesktopWin11_Insider";
		public const string VirtualDesktopWin11_InsiderCanary = "VirtualDesktopWin11_InsiderCanary";
		public const string VirtualDesktopWin10 = "VirtualDesktopWin10";

		public static string GetImplementationForOS() {
			// We need to load the correct API for correct windows version...
			// See https://www.anoopcnair.com/windows-11-version-numbers-build-numbers-major/ for versions
			int currentBuild = 0;
			try {
				currentBuild = Util.OS.GetWindowsBuildVersion();
			} catch (Exception e) {
				throw new Exception("LoadVDAPI: could not determine Windows version: " + e.Message, e);
			}
			Util.Logging.WriteLine("GetImplementationForOS: Windows Build Version: " + currentBuild);
			if (currentBuild >= 25314) {
				Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 Insider Canary due to build >= 25314");
				return VirtualDesktopWin11_InsiderCanary;
			} else if (currentBuild >= 25158) {
				Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 Insider due to build >= 25158-5267");
				return VirtualDesktopWin11_Insider;
			} else if (currentBuild >= 23403) {
				// https://github.com/dankrusi/WindowsVirtualDesktopHelper/issues/35#issuecomment-1626892575
				Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 Insider due to build >= 23403 ");
				return VirtualDesktopWin11_Insider;
			} else if (currentBuild >= 22621) {
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
				int buildNumber = Int32.Parse(registryKey.GetValue("UBR").ToString());
				if ((buildNumber >= 2921 && buildNumber < 3007) || (buildNumber >= 3061)) {
					Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 23H2 due to build >= 22621.2921 & < 22621.3007");
					return VirtualDesktopWin11_23H2_2921;
				} else if (buildNumber >= 2050) {
					Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 23H2 due to build >= 22621.2050");
					return VirtualDesktopWin11_23H2;
				} else {
					Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 22H2 due to build >= 22621");
					return VirtualDesktopWin11_22H2;
				}
			} else if (currentBuild >= 22000) {
				Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 21H1 due to build >= 22000");
				return VirtualDesktopWin11_21H2;
			} else if (currentBuild >= 21996) {
				Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 due to build >= 21996 (beta version of Windows 11)");
				return VirtualDesktopWin11_21H2;
			} else {
				Util.Logging.WriteLine("GetImplementationForOS: Fallback to Windows 10 (fallback)");
				return VirtualDesktopWin10;
			}
		}

		public static IVirtualDesktopManager LoadImplementationWithFallback(string name) {
			var implementationsToTry = new List<string>();
			implementationsToTry.Add(name);
			if (!implementationsToTry.Contains(VirtualDesktopWin11_InsiderCanary)) implementationsToTry.Add(VirtualDesktopWin11_InsiderCanary);
			if (!implementationsToTry.Contains(VirtualDesktopWin11_Insider)) implementationsToTry.Add(VirtualDesktopWin11_Insider);
			if (!implementationsToTry.Contains(VirtualDesktopWin11_23H2_2921)) implementationsToTry.Add(VirtualDesktopWin11_23H2_2921);
			if (!implementationsToTry.Contains(VirtualDesktopWin11_23H2)) implementationsToTry.Add(VirtualDesktopWin11_23H2);
			if (!implementationsToTry.Contains(VirtualDesktopWin11_22H2)) implementationsToTry.Add(VirtualDesktopWin11_22H2);
			if (!implementationsToTry.Contains(VirtualDesktopWin11_21H2)) implementationsToTry.Add(VirtualDesktopWin11_21H2);
			if (!implementationsToTry.Contains(VirtualDesktopWin10)) implementationsToTry.Add(VirtualDesktopWin10);

			foreach (var implementationName in implementationsToTry) {
				Util.Logging.WriteLine("LoadImplementationWithFallback: trying to load implementation " + implementationName);
				try {
					var impl = LoadImplementation(implementationName);
					impl.Current(); // test for success
					Util.Logging.WriteLine("LoadImplementationWithFallback: success!");
					return impl;
				} catch (Exception e) {
					Util.Logging.WriteLine("LoadImplementationWithFallback: failed to load " + implementationName);
				}
			}
			throw new Exception("LoadImplementationWithFallback: no implementation loaded successfully, tried: " + string.Join(",", implementationsToTry));
		}

		public static IVirtualDesktopManager LoadImplementation(string name) {
			Util.Logging.WriteLine("LoadImplementation: Loading VDImplementation: " + name + "...");
			IVirtualDesktopManager impl = null;
			if (name == VirtualDesktopWin11_23H2_2921) {
				try {
					impl = new VirtualDesktopAPI.Implementation.VirtualDesktopWin11_23H2_2921();
					impl.Current();
				} catch (Exception e) {
					throw new Exception("LoadImplementation: could not load VirtualDesktop API implementation " + name + ": " + e.Message, e);
				}
			} else if (name == VirtualDesktopWin11_23H2) {                         
				try {
					impl = new VirtualDesktopAPI.Implementation.VirtualDesktopWin11_23H2();
					impl.Current();
				} catch (Exception e) {
					throw new Exception("LoadImplementation: could not load VirtualDesktop API implementation " + name + ": " + e.Message, e);
				}
			} else if (name == VirtualDesktopWin11_22H2) {
				try {
					impl = new VirtualDesktopAPI.Implementation.VirtualDesktopWin11_22H2();
					impl.Current();
				} catch (Exception e) {
					throw new Exception("LoadImplementation: could not load VirtualDesktop API implementation " + name + ": " + e.Message, e);
				}
			} else if (name == VirtualDesktopWin11_Insider) {
				try {
					impl = new VirtualDesktopAPI.Implementation.VirtualDesktopWin11_Insider();
					impl.Current();
				} catch (Exception e) {
					throw new Exception("LoadImplementation: could not load VirtualDesktop API implementation " + name + ": " + e.Message, e);
				}
			} else if (name == VirtualDesktopWin11_InsiderCanary) {
				try {
					impl = new VirtualDesktopAPI.Implementation.VirtualDesktopWin11_InsiderCanary();
					impl.Current();
				} catch (Exception e) {
					throw new Exception("LoadImplementation: could not load VirtualDesktop API implementation " + name + ": " + e.Message, e);
				}
			} else if (name == VirtualDesktopWin11_21H2) {
				try {
					impl = new VirtualDesktopAPI.Implementation.VirtualDesktopWin11_21H2();
					impl.Current();
				} catch (Exception e) {
					throw new Exception("LoadImplementation: could not load VirtualDesktop API implementation " + name + ": " + e.Message, e);
				}
			} else if (name == VirtualDesktopWin10) {
				try {
					impl = new VirtualDesktopAPI.Implementation.VirtualDesktopWin10();
					impl.Current();
				} catch (Exception e) {
					throw new Exception("LoadImplementation: could not load VirtualDesktop API implementation " + name + ": " + e.Message, e);
				}
			} else {
				throw new Exception("LoadImplementation: could not load VirtualDesktop API implementation " + name + ": " + "Unknown implementation");
			}
			return impl;
		}
	}
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI {
	public class Loader {

		public const string VirtualDesktopWin11_23H2_2921 = "VirtualDesktopWin11_23H2_2921";
		public const string VirtualDesktopWin11_23H2 = "VirtualDesktopWin11_23H2";
		public const string VirtualDesktopWin11_22H2 = "VirtualDesktopWin11_22H2";
		public const string VirtualDesktopWin11_21H2 = "VirtualDesktopWin11_21H2";
		public const string VirtualDesktopWin11_Insider = "VirtualDesktopWin11_Insider";
		public const string VirtualDesktopWin11_Insider22631 = "VirtualDesktopWin11_Insider22631";
		public const string VirtualDesktopWin11_Insider25314 = "VirtualDesktopWin11_Insider25314";
		public const string VirtualDesktopWin10 = "VirtualDesktopWin10";

		public static string GetImplementationForOS() {
			// We need to load the correct API for correct windows version...
			// See https://www.anoopcnair.com/windows-11-version-numbers-build-numbers-major/ for versions
			int currentBuild = 0;
			int currentBuildRevision = 0;
			try {
				currentBuild = Util.OS.GetWindowsBuildVersion();
				currentBuildRevision = Util.OS.GetWindowsBuildRevision();
			} catch (Exception e) {
				throw new Exception("LoadVDAPI: could not determine Windows version: " + e.Message, e);
			}
			Util.Logging.WriteLine("GetImplementationForOS: Windows Build Version: " + currentBuild+"."+ currentBuildRevision);
			if(currentBuild >= 25314) {
				Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 Insider Canary 25314 due to build >= 25314");
				return VirtualDesktopWin11_Insider25314;
			} else if (currentBuild >= 25158) {
				Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 Insider due to build >= 25158-5267");
				return VirtualDesktopWin11_Insider;
			} else if (currentBuild >= 23403) {
				// https://github.com/dankrusi/WindowsVirtualDesktopHelper/issues/35#issuecomment-1626892575
				Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 Insider due to build >= 23403 ");
				return VirtualDesktopWin11_Insider;
			} else if(currentBuild >= 22631) {
				Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 Insider Canary 22631 due to build >= 22631");
				return VirtualDesktopWin11_Insider22631;
			} else if (currentBuild >= 22621) {
				if (currentBuildRevision >= 2921 && currentBuildRevision < 3007 ) {
					Util.Logging.WriteLine("GetImplementationForOS: Detected Windows 11 23H2 due to build >= 22621.2921 & < 22621.3007");
					return VirtualDesktopWin11_23H2_2921;
				} else if (currentBuildRevision >= 2050) {
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
			if (!implementationsToTry.Contains(VirtualDesktopWin11_Insider25314)) implementationsToTry.Add(VirtualDesktopWin11_Insider25314);
			if (!implementationsToTry.Contains(VirtualDesktopWin11_Insider22631)) implementationsToTry.Add(VirtualDesktopWin11_Insider22631);
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
					Util.Logging.WriteLine("LoadImplementationWithFallback: failed to load " + implementationName+": "+e);
				}
			}
			throw new Exception("Oh no! It seems your version of Windows is not yet supported! This is most likely due to your Windows Version being a Insider/Canary build, or a having a new patch where the APIs have changed. You can report the issue, but please be patient as it's a lot of work to keep up with all the Windows versions! \r\nError: No implementation loaded successfully, tried: " + string.Join(", ", implementationsToTry)+ " (LoadImplementationWithFallback)");
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
			} else if(name == VirtualDesktopWin11_Insider22631) {
				try {
					impl = new VirtualDesktopAPI.Implementation.VirtualDesktopWin11_Insider22631();
					impl.Current();
				} catch(Exception e) {
					throw new Exception("LoadImplementation: could not load VirtualDesktop API implementation " + name + ": " + e.Message, e);
				}
			} else if(name == VirtualDesktopWin11_Insider25314) {
				try {
					impl = new VirtualDesktopAPI.Implementation.VirtualDesktopWin11_Insider25314();
					impl.Current();
				} catch(Exception e) {
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

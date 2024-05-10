// Original Implemation: windows-virtualdesktopindicator by zgdump (https://github.com/zgdump/windows-virtualdesktopindicator)
// Contributors: Dan Krusi (https://github.com/dankrusi), MScholtes (https://github.com/MScholtes), Flaflo (https://github.com/Flaflo)
// License: MIT License (https://github.com/zgdump/windows-virtualdesktopindicator/blob/main/LICENSE)

using System;
using System.Runtime.InteropServices;

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI.Implementation {





	internal class VirtualDesktopWin11_Insider : IVirtualDesktopManager {

		#region API

		public uint Current() {
			var currentDesktop = DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero);
			var currentDesktopIndex = DesktopManager.GetDesktopIndex(currentDesktop);

			return (uint)currentDesktopIndex;
		}

		public void SwitchForward() {
			var current = DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero);

			DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(current, 4, out var adjacent);
			if (adjacent == null) return;
			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(IntPtr.Zero, adjacent);
		}

		public void SwitchBackward() {
			var current = DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero);

			DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(current, 3, out var adjacent);
			if (adjacent == null) return;
			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(IntPtr.Zero, adjacent);
		}

		public string CurrentDisplayName() {
			return DesktopNameFromDesktop(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero));
		}

		public uint GetVDCount() {
			return (uint)DesktopManager.GetTotalVDCount();
		}

		public void SwitchToDesktop(int number) {
			var desktop = DesktopManager.GetDesktop(number);

			if (desktop == null) return;

			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(IntPtr.Zero, desktop);
		}

		#endregion

		private static string DesktopNameFromDesktop(IVirtualDesktop desktop) {
			var desktopName = Microsoft.Win32.Registry
				.GetValue(
					$"HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VirtualDesktops\\Desktops\\{{{desktop.GetId()}}}",
					"Name", null)?.ToString();

			if (string.IsNullOrEmpty(desktopName)) {
				desktopName = "Desktop " + (DesktopManager.GetDesktopIndex(desktop) + 1);
			}

			return desktopName;
		}

		#region COM API
		internal static class Guids {
			public static readonly Guid CLSID_ImmersiveShell = new Guid("C2F03A33-21F5-47FA-B4BB-156362A2F239");
			public static readonly Guid CLSID_VirtualDesktopManagerInternal = new Guid("C5E0CDCA-7B6E-41B2-9FC4-D93975CC467B");
			public static readonly Guid CLSID_VirtualDesktopManager = new Guid("AA509086-5CA9-4C25-8F95-589D3C07B48A");
			public static readonly Guid CLSID_VirtualDesktopPinnedApps = new Guid("B5A399E7-1C87-46B8-88E9-FC5747B171BD");
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct Size {
			public int X;
			public int Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct Rect {
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		internal enum APPLICATION_VIEW_CLOAK_TYPE : int {
			AVCT_NONE = 0,
			AVCT_DEFAULT = 1,
			AVCT_VIRTUAL_DESKTOP = 2
		}

		internal enum APPLICATION_VIEW_COMPATIBILITY_POLICY : int {
			AVCP_NONE = 0,
			AVCP_SMALL_SCREEN = 1,
			AVCP_TABLET_SMALL_SCREEN = 2,
			AVCP_VERY_SMALL_SCREEN = 3,
			AVCP_HIGH_SCALE_FACTOR = 4
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
		[Guid("372E1D3B-38D3-42E4-A15B-8AB2B178F513")]
		internal interface IApplicationView {
			int SetFocus();
			int SwitchTo();
			int TryInvokeBack(IntPtr /* IAsyncCallback* */ callback);
			int GetThumbnailWindow(out IntPtr hwnd);
			int GetMonitor(out IntPtr /* IImmersiveMonitor */ immersiveMonitor);
			int GetVisibility(out int visibility);
			int SetCloak(APPLICATION_VIEW_CLOAK_TYPE cloakType, int unknown);
			int GetPosition(ref Guid guid /* GUID for IApplicationViewPosition */, out IntPtr /* IApplicationViewPosition** */ position);
			int SetPosition(ref IntPtr /* IApplicationViewPosition* */ position);
			int InsertAfterWindow(IntPtr hwnd);
			int GetExtendedFramePosition(out Rect rect);
			int GetAppUserModelId([MarshalAs(UnmanagedType.LPWStr)] out string id);
			int SetAppUserModelId(string id);
			int IsEqualByAppUserModelId(string id, out int result);
			int GetViewState(out uint state);
			int SetViewState(uint state);
			int GetNeediness(out int neediness);
			int GetLastActivationTimestamp(out ulong timestamp);
			int SetLastActivationTimestamp(ulong timestamp);
			int GetVirtualDesktopId(out Guid guid);
			int SetVirtualDesktopId(ref Guid guid);
			int GetShowInSwitchers(out int flag);
			int SetShowInSwitchers(int flag);
			int GetScaleFactor(out int factor);
			int CanReceiveInput(out bool canReceiveInput);
			int GetCompatibilityPolicyType(out APPLICATION_VIEW_COMPATIBILITY_POLICY flags);
			int SetCompatibilityPolicyType(APPLICATION_VIEW_COMPATIBILITY_POLICY flags);
			int GetSizeConstraints(IntPtr /* IImmersiveMonitor* */ monitor, out Size size1, out Size size2);
			int GetSizeConstraintsForDpi(uint uint1, out Size size1, out Size size2);
			int SetSizeConstraintsForDpi(ref uint uint1, ref Size size1, ref Size size2);
			int OnMinSizePreferencesUpdated(IntPtr hwnd);
			int ApplyOperation(IntPtr /* IApplicationViewOperation* */ operation);
			int IsTray(out bool isTray);
			int IsInHighZOrderBand(out bool isInHighZOrderBand);
			int IsSplashScreenPresented(out bool isSplashScreenPresented);
			int Flash();
			int GetRootSwitchableOwner(out IApplicationView rootSwitchableOwner);
			int EnumerateOwnershipTree(out IObjectArray ownershipTree);
			int GetEnterpriseId([MarshalAs(UnmanagedType.LPWStr)] out string enterpriseId);
			int IsMirrored(out bool isMirrored);
			int Unknown1(out int unknown);
			int Unknown2(out int unknown);
			int Unknown3(out int unknown);
			int Unknown4(out int unknown);
			int Unknown5(out int unknown);
			int Unknown6(int unknown);
			int Unknown7();
			int Unknown8(out int unknown);
			int Unknown9(int unknown);
			int Unknown10(int unknownX, int unknownY);
			int Unknown11(int unknown);
			int Unknown12(out Size size1);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("1841C6D7-4F9D-42C0-AF41-8747538F10E5")]
		internal interface IApplicationViewCollection {
			int GetViews(out IObjectArray array);
			int GetViewsByZOrder(out IObjectArray array);
			int GetViewsByAppUserModelId(string id, out IObjectArray array);
			int GetViewForHwnd(IntPtr hwnd, out IApplicationView view);
			int GetViewForApplication(object application, out IApplicationView view);
			int GetViewForAppUserModelId(string id, out IApplicationView view);
			int GetViewInFocus(out IntPtr view);
			int Unknown1(out IntPtr view);
			void RefreshCollection();
			int RegisterForApplicationViewChanges(object listener, out int cookie);
			int UnregisterForApplicationViewChanges(int cookie);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("536D3495-B208-4CC9-AE26-DE8111275BF8")]
		internal interface IVirtualDesktop {
			bool IsViewVisible(IApplicationView view);
			Guid GetId();
			IntPtr Unknown1();
			[return: MarshalAs(UnmanagedType.HString)]
			string GetName();
			[return: MarshalAs(UnmanagedType.HString)]
			string GetWallpaperPath();
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("88846798-1611-4D18-946B-4A67BFF58C1B")]
		internal interface IVirtualDesktopManagerInternal {
			int GetCount(IntPtr hWndOrMon);
			void MoveViewToDesktop(IApplicationView view, IVirtualDesktop desktop);
			bool CanViewMoveDesktops(IApplicationView view);
			IVirtualDesktop GetCurrentDesktop(IntPtr hWndOrMon);
			IObjectArray GetAllCurrentDesktops();
			void GetDesktops(IntPtr hWndOrMon, out IObjectArray desktops);
			[PreserveSig]
			int GetAdjacentDesktop(IVirtualDesktop from, int direction, out IVirtualDesktop desktop);
			void SwitchDesktop(IntPtr hWndOrMon, IVirtualDesktop desktop);
			IVirtualDesktop CreateDesktop(IntPtr hWndOrMon);
			void MoveDesktop(IVirtualDesktop desktop, IntPtr hWndOrMon, int nIndex);
			void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
			IVirtualDesktop FindDesktop(ref Guid desktopid);
			void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop desktop, out IObjectArray unknown1, out IObjectArray unknown2);
			void SetDesktopName(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string name);
			void SetDesktopWallpaper(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string path);
			void UpdateWallpaperPathForAllDesktops([MarshalAs(UnmanagedType.HString)] string path);
			void CopyDesktopState(IApplicationView pView0, IApplicationView pView1);
			int GetDesktopIsPerMonitor();
			void SetDesktopIsPerMonitor(bool state);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B")]
		internal interface IVirtualDesktopManager {
			bool IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow);
			Guid GetWindowDesktopId(IntPtr topLevelWindow);
			void MoveWindowToDesktop(IntPtr topLevelWindow, ref Guid desktopId);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("4CE81583-1E4C-4632-A621-07A53543148F")]
		internal interface IVirtualDesktopPinnedApps {
			bool IsAppIdPinned(string appId);
			void PinAppID(string appId);
			void UnpinAppID(string appId);
			bool IsViewPinned(IApplicationView applicationView);
			void PinView(IApplicationView applicationView);
			void UnpinView(IApplicationView applicationView);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9")]
		internal interface IObjectArray {
			void GetCount(out int count);
			void GetAt(int index, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object obj);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
		internal interface IServiceProvider10 {
			[return: MarshalAs(UnmanagedType.IUnknown)]
			object QueryService(ref Guid service, ref Guid riid);
		}
		#endregion

		#region COM wrapper
		internal static class DesktopManager {
			static DesktopManager() {
				var shell = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell));
				VirtualDesktopManagerInternal = (IVirtualDesktopManagerInternal)shell.QueryService(Guids.CLSID_VirtualDesktopManagerInternal, typeof(IVirtualDesktopManagerInternal).GUID);
				VirtualDesktopManager = (IVirtualDesktopManager)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_VirtualDesktopManager));
				ApplicationViewCollection = (IApplicationViewCollection)shell.QueryService(typeof(IApplicationViewCollection).GUID, typeof(IApplicationViewCollection).GUID);
				VirtualDesktopPinnedApps = (IVirtualDesktopPinnedApps)shell.QueryService(Guids.CLSID_VirtualDesktopPinnedApps, typeof(IVirtualDesktopPinnedApps).GUID);
			}

			internal static IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
			internal static IVirtualDesktopManager VirtualDesktopManager;
			internal static IApplicationViewCollection ApplicationViewCollection;
			internal static IVirtualDesktopPinnedApps VirtualDesktopPinnedApps;

			internal static IVirtualDesktop GetDesktop(int index) {   // get desktop with index
				int count = VirtualDesktopManagerInternal.GetCount(IntPtr.Zero);
				if (index < 0 || index >= count) throw new ArgumentOutOfRangeException("index");
				IObjectArray desktops;
				VirtualDesktopManagerInternal.GetDesktops(IntPtr.Zero, out desktops);
				object objdesktop;
				desktops.GetAt(index, typeof(IVirtualDesktop).GUID, out objdesktop);
				Marshal.ReleaseComObject(desktops);
				return (IVirtualDesktop)objdesktop;
			}

			internal static int GetTotalVDCount() {
				return VirtualDesktopManagerInternal.GetCount(IntPtr.Zero);
			}

			internal static int GetDesktopIndex(IVirtualDesktop desktop) { // get index of desktop
				int index = -1;
				Guid IdSearch = desktop.GetId();
				IObjectArray desktops;
				VirtualDesktopManagerInternal.GetDesktops(IntPtr.Zero, out desktops);
				object objdesktop;
				for (int i = 0; i < VirtualDesktopManagerInternal.GetCount(IntPtr.Zero); i++) {
					desktops.GetAt(i, typeof(IVirtualDesktop).GUID, out objdesktop);
					if (IdSearch.CompareTo(((IVirtualDesktop)objdesktop).GetId()) == 0) {
						index = i;
						break;
					}
				}
				Marshal.ReleaseComObject(desktops);
				return index;
			}

		}
		#endregion

		#region public interface
		public class Desktop {
			// get process id to window handle
			[DllImport("user32.dll")]
			private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

			// get handle of active window
			[DllImport("user32.dll")]
			private static extern IntPtr GetForegroundWindow();

			private static readonly Guid AppOnAllDesktops = new Guid("BB64D5B7-4DE3-4AB2-A87C-DB7601AEA7DC");
			private static readonly Guid WindowOnAllDesktops = new Guid("C2DDEA68-66F2-4CF9-8264-1BFD00FBBBAC");

			private IVirtualDesktop ivd;
			private Desktop(IVirtualDesktop desktop) { this.ivd = desktop; }

			public override int GetHashCode() { // get hash
				return ivd.GetHashCode();
			}

			public override bool Equals(object obj) { // compare with object
				var desk = obj as Desktop;
				return desk != null && object.ReferenceEquals(this.ivd, desk.ivd);
			}

			public static int Count { // return the number of desktops
				get { return DesktopManager.VirtualDesktopManagerInternal.GetCount(IntPtr.Zero); }
			}

			public static Desktop Current { // returns current desktop
				get { return new Desktop(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero)); }
			}

			public static Desktop FromIndex(int index) { // return desktop object from index (-> index = 0..Count-1)
				return new Desktop(DesktopManager.GetDesktop(index));
			}

			public static Desktop FromWindow(IntPtr hWnd) { // return desktop object to desktop on which window <hWnd> is displayed
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				Guid id = DesktopManager.VirtualDesktopManager.GetWindowDesktopId(hWnd);
				if ((id.CompareTo(AppOnAllDesktops) == 0) || (id.CompareTo(WindowOnAllDesktops) == 0))
					return new Desktop(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero));
				else
					return new Desktop(DesktopManager.VirtualDesktopManagerInternal.FindDesktop(ref id));
			}

			public static int FromDesktop(Desktop desktop) { // return index of desktop object or -1 if not found
				return DesktopManager.GetDesktopIndex(desktop.ivd);
			}

			public static string DesktopNameFromDesktop(Desktop desktop) { // return name of desktop or "Desktop n" if it has no name

				// get desktop name
				string desktopName = null;
				try {
					desktopName = desktop.ivd.GetName();
				} catch { }

				// no name found, generate generic name
				if (string.IsNullOrEmpty(desktopName)) { // create name "Desktop n" (n = number starting with 1)
					desktopName = "Desktop " + (DesktopManager.GetDesktopIndex(desktop.ivd) + 1).ToString();
				}
				return desktopName;
			}

			public static string DesktopNameFromIndex(int index) { // return name of desktop from index (-> index = 0..Count-1) or "Desktop n" if it has no name

				// get desktop name
				string desktopName = null;
				try {
					desktopName = DesktopManager.GetDesktop(index).GetName();
				} catch { }

				// no name found, generate generic name
				if (string.IsNullOrEmpty(desktopName)) { // create name "Desktop n" (n = number starting with 1)
					desktopName = "Desktop " + (index + 1).ToString();
				}
				return desktopName;
			}

			public static bool HasDesktopNameFromIndex(int index) { // return true is desktop is named or false if it has no name

				// read desktop name in registry
				string desktopName = null;
				try {
					desktopName = DesktopManager.GetDesktop(index).GetName();
				} catch { }

				// name found?
				if (string.IsNullOrEmpty(desktopName))
					return false;
				else
					return true;
			}

			public static string DesktopWallpaperFromIndex(int index) { // return name of desktop wallpaper from index (-> index = 0..Count-1)

				// get desktop name
				string desktopwppath = "";
				try {
					desktopwppath = DesktopManager.GetDesktop(index).GetWallpaperPath();
				} catch { }

				return desktopwppath;
			}

			public static int SearchDesktop(string partialName) { // get index of desktop with partial name, return -1 if no desktop found
				int index = -1;

				for (int i = 0; i < DesktopManager.VirtualDesktopManagerInternal.GetCount(IntPtr.Zero); i++) { // loop through all virtual desktops and compare partial name to desktop name
					if (DesktopNameFromIndex(i).ToUpper().IndexOf(partialName.ToUpper()) >= 0) {
						index = i;
						break;
					}
				}

				return index;
			}

			public static Desktop Create() { // create a new desktop
				return new Desktop(DesktopManager.VirtualDesktopManagerInternal.CreateDesktop(IntPtr.Zero));
			}

			public void Remove(Desktop fallback = null) { // destroy desktop and switch to <fallback>
				IVirtualDesktop fallbackdesktop;
				if (fallback == null) { // if no fallback is given use desktop to the left except for desktop 0.
					Desktop dtToCheck = new Desktop(DesktopManager.GetDesktop(0));
					if (this.Equals(dtToCheck)) { // desktop 0: set fallback to second desktop (= "right" desktop)
						DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 4, out fallbackdesktop); // 4 = RightDirection
					} else { // set fallback to "left" desktop
						DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 3, out fallbackdesktop); // 3 = LeftDirection
					}
				} else
					// set fallback desktop
					fallbackdesktop = fallback.ivd;

				DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(ivd, fallbackdesktop);
			}

			public static void RemoveAll() { // remove all desktops but visible
				DesktopManager.VirtualDesktopManagerInternal.SetDesktopIsPerMonitor(true);
			}

			public void Move(int index) { // move current desktop to desktop in index (-> index = 0..Count-1)
				DesktopManager.VirtualDesktopManagerInternal.MoveDesktop(ivd, IntPtr.Zero, index);
			}

			public void SetName(string Name) { // set name for desktop, empty string removes name
				DesktopManager.VirtualDesktopManagerInternal.SetDesktopName(this.ivd, Name);
			}

			public void SetWallpaperPath(string Path) { // set path for wallpaper, empty string removes path
				if (string.IsNullOrEmpty(Path)) throw new ArgumentNullException();
				DesktopManager.VirtualDesktopManagerInternal.SetDesktopWallpaper(this.ivd, Path);
			}

			public static void SetAllWallpaperPaths(string Path) { // set wallpaper path for all desktops
				if (string.IsNullOrEmpty(Path)) throw new ArgumentNullException();
				DesktopManager.VirtualDesktopManagerInternal.UpdateWallpaperPathForAllDesktops(Path);
			}

			public bool IsVisible { // return true if this desktop is the current displayed one
				get { return object.ReferenceEquals(ivd, DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero)); }
			}

			public void MakeVisible() { // make this desktop visible
				DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(IntPtr.Zero, ivd);
			}

			public Desktop Left { // return desktop at the left of this one, null if none
				get {
					IVirtualDesktop desktop;
					int hr = DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 3, out desktop); // 3 = LeftDirection
					if (hr == 0)
						return new Desktop(desktop);
					else
						return null;
				}
			}

			public Desktop Right { // return desktop at the right of this one, null if none
				get {
					IVirtualDesktop desktop;
					int hr = DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 4, out desktop); // 4 = RightDirection
					if (hr == 0)
						return new Desktop(desktop);
					else
						return null;
				}
			}

			public void MoveWindow(IntPtr hWnd) { // move window to this desktop
				int processId;
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				GetWindowThreadProcessId(hWnd, out processId);

				if (System.Diagnostics.Process.GetCurrentProcess().Id == processId) { // window of process
					try // the easy way (if we are owner)
					{
						DesktopManager.VirtualDesktopManager.MoveWindowToDesktop(hWnd, ivd.GetId());
					} catch // window of process, but we are not the owner
					  {
						IApplicationView view;
						DesktopManager.ApplicationViewCollection.GetViewForHwnd(hWnd, out view);
						DesktopManager.VirtualDesktopManagerInternal.MoveViewToDesktop(view, ivd);
					}
				} else { // window of other process
					IApplicationView view;
					DesktopManager.ApplicationViewCollection.GetViewForHwnd(hWnd, out view);
					try {
						DesktopManager.VirtualDesktopManagerInternal.MoveViewToDesktop(view, ivd);
					} catch { // could not move active window, try main window (or whatever windows thinks is the main window)
						DesktopManager.ApplicationViewCollection.GetViewForHwnd(System.Diagnostics.Process.GetProcessById(processId).MainWindowHandle, out view);
						DesktopManager.VirtualDesktopManagerInternal.MoveViewToDesktop(view, ivd);
					}
				}
			}

			public void MoveActiveWindow() { // move active window to this desktop
				MoveWindow(GetForegroundWindow());
			}

			public bool HasWindow(IntPtr hWnd) { // return true if window is on this desktop
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				Guid id = DesktopManager.VirtualDesktopManager.GetWindowDesktopId(hWnd);
				if ((id.CompareTo(AppOnAllDesktops) == 0) || (id.CompareTo(WindowOnAllDesktops) == 0))
					return true;
				else
					return ivd.GetId() == id;
			}


		}
		#endregion
	}
}
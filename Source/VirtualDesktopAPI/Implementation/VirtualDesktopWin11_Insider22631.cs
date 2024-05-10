// Original Implemation: windows-virtualdesktopindicator by zgdump (https://github.com/zgdump/windows-virtualdesktopindicator)
// Contributors: Dan Krusi (https://github.com/dankrusi), MScholtes (https://github.com/MScholtes), Flaflo (https://github.com/Flaflo)
// License: MIT License (https://github.com/zgdump/windows-virtualdesktopindicator/blob/main/LICENSE)

// Version for Windows 11 Insider Canary Build 22631 and up
// https://github.com/slnz00/VirtualDesktopDumper/blob/master/dumps/Win11-23H2-22631.3085.txt
// https://blogs.windows.com/windows-insider/2023/11/16/releasing-windows-11-builds-22621-2787-and-22631-2787-to-the-release-preview-channel/

//
//
//
//
//
//
//
//
//
//
//
//
//
// DOES NOT WORK YET (but VirtualDesktopWin11_23H2_2921 seems to work on 22631.3085 !?!)
//
//
//
//
//
//
//
//
//
//
//
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI.Implementation {





	public class VirtualDesktopWin11_Insider22631 : IVirtualDesktopManager {

		#region API

		public uint Current() {
			var currentDesktop = DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop();
			var currentDesktopIndex = DesktopManager.GetDesktopIndex(currentDesktop);

			return (uint)currentDesktopIndex;
		}

		public void SwitchForward() {
			var current = DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop();

			DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(current, 4, out var adjacent);
			if (adjacent == null) return;
			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(adjacent);
		}

		public void SwitchBackward() {
			var current = DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop();

			DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(current, 3, out var adjacent);
			if (adjacent == null) return;
			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(adjacent);
		}

		public string CurrentDisplayName() {
			return DesktopNameFromDesktop(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());
		}

		public uint GetVDCount() {
			return (uint)DesktopManager.GetTotalVDCount();
		}

		public void SwitchToDesktop(int number) {
			var desktop = DesktopManager.GetDesktop(number);
			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(desktop);

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
			public static readonly Guid CLSID_VirtualDesktopManagerInternal = new Guid("53F5CA0B-158F-4124-900C-057158060B27");
			public static readonly Guid CLSID_VirtualDesktopManager = new Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B");
			public static readonly Guid CLSID_VirtualDesktopPinnedApps = new Guid("4CE81583-1E4C-4632-A621-07A53543148F");
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
		[Guid("3F07F4BE-B107-441A-AF0F-39D82529072C")]
		internal interface IVirtualDesktop {
			bool IsViewVisible(IApplicationView view);
			Guid GetId();
			[return: MarshalAs(UnmanagedType.HString)]
			string GetName();
			[return: MarshalAs(UnmanagedType.HString)]
			string GetWallpaperPath();
			bool IsRemote();
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("53F5CA0B-158F-4124-900C-057158060B27")]
		internal interface IVirtualDesktopManagerInternal {
			int GetCount();
			void MoveViewToDesktop(IApplicationView view, IVirtualDesktop desktop);
			bool CanViewMoveDesktops(IApplicationView view);
			IVirtualDesktop GetCurrentDesktop();
			void GetDesktops(out IObjectArray desktops);
			[PreserveSig]
			int GetAdjacentDesktop(IVirtualDesktop from, int direction, out IVirtualDesktop desktop);
			void SwitchDesktop(IVirtualDesktop desktop);
			void SwitchDesktopAndMoveForegroundView(IVirtualDesktop desktop);
			IVirtualDesktop CreateDesktop();
			void MoveDesktop(IVirtualDesktop desktop, int nIndex);
			void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
			IVirtualDesktop FindDesktop(ref Guid desktopid);
			void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop desktop, out IObjectArray unknown1, out IObjectArray unknown2);
			void SetDesktopName(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string name);
			void SetDesktopWallpaper(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string path);
			void UpdateWallpaperPathForAllDesktops([MarshalAs(UnmanagedType.HString)] string path);
			void CopyDesktopState(IApplicationView pView0, IApplicationView pView1);
			void CreateRemoteDesktop([MarshalAs(UnmanagedType.HString)] string path, out IVirtualDesktop desktop);
			void SwitchRemoteDesktop(IVirtualDesktop desktop);
			void SwitchDesktopWithAnimation(IVirtualDesktop desktop);
			void GetLastActiveDesktop(out IVirtualDesktop desktop);
			void WaitForAnimationToComplete();
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

			internal static IVirtualDesktop GetDesktop(int index) { // get desktop with index
				int count = VirtualDesktopManagerInternal.GetCount();
				if (index < 0 || index >= count) throw new ArgumentOutOfRangeException("index");
				IObjectArray desktops;
				VirtualDesktopManagerInternal.GetDesktops(out desktops);
				object objdesktop;
				desktops.GetAt(index, typeof(IVirtualDesktop).GUID, out objdesktop);
				Marshal.ReleaseComObject(desktops);
				return (IVirtualDesktop)objdesktop;
			}

			internal static int GetTotalVDCount() {
				return VirtualDesktopManagerInternal.GetCount();
			}

			internal static int GetDesktopIndex(IVirtualDesktop desktop) { // get index of desktop
				int index = -1;
				Guid IdSearch = desktop.GetId();
				IObjectArray desktops;
				VirtualDesktopManagerInternal.GetDesktops(out desktops);
				object objdesktop;
				for (int i = 0; i < VirtualDesktopManagerInternal.GetCount(); i++) {
					desktops.GetAt(i, typeof(IVirtualDesktop).GUID, out objdesktop);
					if (IdSearch.CompareTo(((IVirtualDesktop)objdesktop).GetId()) == 0) {
						index = i;
						break;
					}
				}
				Marshal.ReleaseComObject(desktops);
				return index;
			}

			public static IApplicationView GetApplicationView(IntPtr hWnd) { // get application view to window handle
				IApplicationView view;
				ApplicationViewCollection.GetViewForHwnd(hWnd, out view);
				return view;
			}

			public static string GetAppId(IntPtr hWnd) { // get Application ID to window handle
				string appId;
				GetApplicationView(hWnd).GetAppUserModelId(out appId);
				return appId;
			}
		}
		#endregion

		#region public interface
		public class WindowInformation { // stores window informations
			public string Title { get; set; }
			public int Handle { get; set; }
		}

		public class Desktop {
			// get process id to window handle
			[DllImport("user32.dll")]
			private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

			// get thread id of current process
			[DllImport("kernel32.dll")]
			static extern uint GetCurrentThreadId();

			// attach input to thread
			[DllImport("user32.dll")]
			static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

			// get handle of active window
			[DllImport("user32.dll")]
			private static extern IntPtr GetForegroundWindow();

			// try to set foreground window
			[DllImport("user32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)] static extern bool SetForegroundWindow(IntPtr hWnd);

			// send message to window
			[DllImport("user32.dll")]
			static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
			private const int SW_MINIMIZE = 6;

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
				get { return DesktopManager.VirtualDesktopManagerInternal.GetCount(); }
			}

			public static Desktop Current { // returns current desktop
				get { return new Desktop(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop()); }
			}

			public static Desktop FromIndex(int index) { // return desktop object from index (-> index = 0..Count-1)
				return new Desktop(DesktopManager.GetDesktop(index));
			}

			public static Desktop FromWindow(IntPtr hWnd) { // return desktop object to desktop on which window <hWnd> is displayed
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				Guid id = DesktopManager.VirtualDesktopManager.GetWindowDesktopId(hWnd);
				if ((id.CompareTo(AppOnAllDesktops) == 0) || (id.CompareTo(WindowOnAllDesktops) == 0))
					return new Desktop(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());
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

				for (int i = 0; i < DesktopManager.VirtualDesktopManagerInternal.GetCount(); i++) { // loop through all virtual desktops and compare partial name to desktop name
					if (DesktopNameFromIndex(i).ToUpper().IndexOf(partialName.ToUpper()) >= 0) {
						index = i;
						break;
					}
				}

				return index;
			}

			public static Desktop Create() { // create a new desktop
				return new Desktop(DesktopManager.VirtualDesktopManagerInternal.CreateDesktop());
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
				int desktopcount = DesktopManager.VirtualDesktopManagerInternal.GetCount();
				int desktopcurrent = DesktopManager.GetDesktopIndex(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());

				if (desktopcurrent < desktopcount - 1) { // remove all desktops "right" from current
					for (int i = desktopcount - 1; i > desktopcurrent; i--)
						DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(DesktopManager.GetDesktop(i), DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());
				}
				if (desktopcurrent > 0) { // remove all desktops "left" from current
					for (int i = 0; i < desktopcurrent; i++)
						DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(DesktopManager.GetDesktop(0), DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());
				}
			}

			public void Move(int index) { // move current desktop to desktop in index (-> index = 0..Count-1)
				DesktopManager.VirtualDesktopManagerInternal.MoveDesktop(ivd, index);
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
				get { return object.ReferenceEquals(ivd, DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop()); }
			}

			public void MakeVisible() { // make this desktop visible
				WindowInformation wi = FindWindow("Program Manager");

				// activate desktop to prevent flashing icons in taskbar
				int dummy;
				uint DesktopThreadId = GetWindowThreadProcessId(new IntPtr(wi.Handle), out dummy);
				uint ForegroundThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out dummy);
				uint CurrentThreadId = GetCurrentThreadId();

				if ((DesktopThreadId != 0) && (ForegroundThreadId != 0) && (ForegroundThreadId != CurrentThreadId)) {
					AttachThreadInput(DesktopThreadId, CurrentThreadId, true);
					AttachThreadInput(ForegroundThreadId, CurrentThreadId, true);
					SetForegroundWindow(new IntPtr(wi.Handle));
					AttachThreadInput(ForegroundThreadId, CurrentThreadId, false);
					AttachThreadInput(DesktopThreadId, CurrentThreadId, false);
				}

				DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(ivd);

				// direct desktop to give away focus
				ShowWindow(new IntPtr(wi.Handle), SW_MINIMIZE);
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

			public static bool IsWindowPinned(IntPtr hWnd) { // return true if window is pinned to all desktops
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				return DesktopManager.VirtualDesktopPinnedApps.IsViewPinned(DesktopManager.GetApplicationView(hWnd));
			}

			public static void PinWindow(IntPtr hWnd) { // pin window to all desktops
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				var view = DesktopManager.GetApplicationView(hWnd);
				if (!DesktopManager.VirtualDesktopPinnedApps.IsViewPinned(view)) { // pin only if not already pinned
					DesktopManager.VirtualDesktopPinnedApps.PinView(view);
				}
			}

			public static void UnpinWindow(IntPtr hWnd) { // unpin window from all desktops
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				var view = DesktopManager.GetApplicationView(hWnd);
				if (DesktopManager.VirtualDesktopPinnedApps.IsViewPinned(view)) { // unpin only if not already unpinned
					DesktopManager.VirtualDesktopPinnedApps.UnpinView(view);
				}
			}

			public static bool IsApplicationPinned(IntPtr hWnd) { // return true if application for window is pinned to all desktops
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				return DesktopManager.VirtualDesktopPinnedApps.IsAppIdPinned(DesktopManager.GetAppId(hWnd));
			}

			public static void PinApplication(IntPtr hWnd) { // pin application for window to all desktops
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				string appId = DesktopManager.GetAppId(hWnd);
				if (!DesktopManager.VirtualDesktopPinnedApps.IsAppIdPinned(appId)) { // pin only if not already pinned
					DesktopManager.VirtualDesktopPinnedApps.PinAppID(appId);
				}
			}

			public static void UnpinApplication(IntPtr hWnd) { // unpin application for window from all desktops
				if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
				var view = DesktopManager.GetApplicationView(hWnd);
				string appId = DesktopManager.GetAppId(hWnd);
				if (DesktopManager.VirtualDesktopPinnedApps.IsAppIdPinned(appId)) { // unpin only if pinned
					DesktopManager.VirtualDesktopPinnedApps.UnpinAppID(appId);
				}
			}

			// prepare callback function for window enumeration
			private delegate bool CallBackPtr(int hwnd, int lParam);
			private static CallBackPtr callBackPtr = Callback;
			// list of window informations
			private static List<WindowInformation> WindowInformationList = new List<WindowInformation>();

			// enumerate windows
			[DllImport("User32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			private static extern bool EnumWindows(CallBackPtr lpEnumFunc, IntPtr lParam);

			// get window title length
			[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			private static extern int GetWindowTextLength(IntPtr hWnd);

			// get window title
			[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

			// callback function for window enumeration
			private static bool Callback(int hWnd, int lparam) {
				int length = GetWindowTextLength((IntPtr)hWnd);
				if (length > 0) {
					StringBuilder sb = new StringBuilder(length + 1);
					if (GetWindowText((IntPtr)hWnd, sb, sb.Capacity) > 0) { WindowInformationList.Add(new WindowInformation { Handle = hWnd, Title = sb.ToString() }); }
				}
				return true;
			}

			// get list of all windows with title
			public static List<WindowInformation> GetWindows() {
				WindowInformationList = new List<WindowInformation>();
				EnumWindows(callBackPtr, IntPtr.Zero);
				return WindowInformationList;
			}

			// find first window with string in title
			public static WindowInformation FindWindow(string WindowTitle) {
				WindowInformationList = new List<WindowInformation>();
				EnumWindows(callBackPtr, IntPtr.Zero);
				WindowInformation result = WindowInformationList.Find(x => x.Title.IndexOf(WindowTitle, StringComparison.OrdinalIgnoreCase) >= 0);
				return result;
			}
		}
		#endregion
	}
}
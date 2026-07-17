using System;
using System.Runtime.InteropServices;

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI.Implementation {
	internal class VirtualDesktopServer2022 : IVirtualDesktopManager {
		private const string GUID_CLSID_ImmersiveShell = "C2F03A33-21F5-47FA-B4BB-156362A2F239";
		private const string GUID_CLSID_VirtualDesktopManagerInternal = "C5E0CDCA-7B6E-41B2-9FC4-D93975CC467B";
		private const string GUID_IApplicationView = "372E1D3B-38D3-42E4-A15B-8AB2B178F513";
		private const string GUID_IVirtualDesktop = "62fdf88b-11ca-4afb-8bd8-2296dfae49e2";
		private const string GUID_IVirtualDesktopManagerInternal = "094afe11-44f2-4ba0-976f-29a97e263ee0";
		private const string GUID_IObjectArray = "92CA9DCD-5622-4BBA-A805-5E9F541BD8C9";
		private const string GUID_IServiceProvider10 = "6D5140C1-7436-11CE-8034-00AA006009FA";

		#region API

		public uint Current() {
			var currentDesktop = DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero);
			var currentDesktopIndex = DesktopManager.GetDesktopIndex(currentDesktop);
			return (uint)currentDesktopIndex;
		}

		public void SwitchForward() {
			var current = DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero);

			var hr = DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(current, 4, out var adjacent);
			if (hr != 0 || adjacent == null)
				return;

			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(IntPtr.Zero, adjacent);
		}

		public void SwitchBackward() {
			var current = DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero);

			var hr = DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(current, 3, out var adjacent);
			if (hr != 0 || adjacent == null)
				return;

			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(IntPtr.Zero, adjacent);
		}

		public string CurrentDisplayName() {
			return DesktopNameFromDesktop(
				DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero)
			);
		}

		public uint GetVDCount() {
			return (uint)DesktopManager.GetTotalVDCount();
		}

		public void SwitchToDesktop(int number) {
			var desktop = DesktopManager.GetDesktopAtIndex(number);
			if (desktop == null)
				return;

			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(IntPtr.Zero, desktop);
		}

		public int DisplayCount() {
			return DesktopManager.VirtualDesktopManagerInternal.GetCount(IntPtr.Zero);
		}

		#endregion

		#region Implementation

		private static string DesktopNameFromDesktop(IVirtualDesktop desktop) {
			if (desktop == null)
				return "Desktop";

			string desktopName = null;

			try {
				desktopName = Microsoft.Win32.Registry.GetValue(
					$@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops\Desktops\{{{desktop.GetId()}}}",
					"Name",
					null
				)?.ToString();
			} catch {
			}

			if (string.IsNullOrEmpty(desktopName)) {
				desktopName = "Desktop " + (DesktopManager.GetDesktopIndex(desktop) + 1);
			}

			return desktopName;
		}

		internal static class Guids {
			public static readonly Guid CLSID_ImmersiveShell = new Guid(GUID_CLSID_ImmersiveShell);
			public static readonly Guid CLSID_VirtualDesktopManagerInternal = new Guid(GUID_CLSID_VirtualDesktopManagerInternal);
		}

		#endregion

		#region COM API

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
		[Guid(GUID_IApplicationView)]
		internal interface IApplicationView {
			int SetFocus();
			int SwitchTo();
			int TryInvokeBack(IntPtr callback);
			int GetThumbnailWindow(out IntPtr hwnd);
			int GetMonitor(out IntPtr immersiveMonitor);
			int GetVisibility(out int visibility);
			int SetCloak(APPLICATION_VIEW_CLOAK_TYPE cloakType, int unknown);
			int GetPosition(ref Guid guid, out IntPtr position);
			int SetPosition(ref IntPtr position);
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
			int GetSizeConstraints(IntPtr monitor, out Size size1, out Size size2);
			int GetSizeConstraintsForDpi(uint uint1, out Size size1, out Size size2);
			int SetSizeConstraintsForDpi(ref uint uint1, ref Size size1, ref Size size2);
			int OnMinSizePreferencesUpdated(IntPtr hwnd);
			int ApplyOperation(IntPtr operation);
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
		[Guid(GUID_IVirtualDesktop)]
		internal interface IVirtualDesktop {
			bool IsViewVisible(IApplicationView view);
			Guid GetId();
			IntPtr Unknown1();
			[return: MarshalAs(UnmanagedType.HString)]
			string GetName();
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid(GUID_IVirtualDesktopManagerInternal)]
		internal interface IVirtualDesktopManagerInternal {
			int GetCount(IntPtr hWndOrMon);
			void MoveViewToDesktop(IApplicationView view, IVirtualDesktop desktop);
			bool CanViewMoveDesktops(IApplicationView view);
			IVirtualDesktop GetCurrentDesktop(IntPtr hWndOrMon);
			void GetDesktops(IntPtr hWndOrMon, out IObjectArray desktops);
			[PreserveSig]
			int GetAdjacentDesktop(IVirtualDesktop from, int direction, out IVirtualDesktop desktop);
			void SwitchDesktop(IntPtr hWndOrMon, IVirtualDesktop desktop);
			IVirtualDesktop CreateDesktop(IntPtr hWndOrMon);
			void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
			IVirtualDesktop FindDesktop(ref Guid desktopid);
			void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop desktop, out IObjectArray unknown1, out IObjectArray unknown2);
			void SetDesktopName(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string name);
			void CopyDesktopState(IApplicationView pView0, IApplicationView pView1);
			int GetDesktopIsPerMonitor();
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid(GUID_IObjectArray)]
		internal interface IObjectArray {
			void GetCount(out int count);
			void GetAt(int index, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object obj);
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid(GUID_IServiceProvider10)]
		internal interface IServiceProvider10 {
			[return: MarshalAs(UnmanagedType.IUnknown)]
			object QueryService(ref Guid service, ref Guid riid);
		}

		#endregion

		#region COM wrapper

		internal static class DesktopManager {
			internal static readonly IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;

			static DesktopManager() {
				var shell = (IServiceProvider10)Activator.CreateInstance(
					Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell)
				);

				Guid service = Guids.CLSID_VirtualDesktopManagerInternal;
				Guid riid = typeof(IVirtualDesktopManagerInternal).GUID;

				VirtualDesktopManagerInternal =
					(IVirtualDesktopManagerInternal)shell.QueryService(ref service, ref riid);
			}

			internal static int GetDesktopIndex(IVirtualDesktop desktop) {
				if (desktop == null)
					return -1;

				var idSearch = desktop.GetId();
				VirtualDesktopManagerInternal.GetDesktops(IntPtr.Zero, out var desktops);

				try {
					var count = VirtualDesktopManagerInternal.GetCount(IntPtr.Zero);
					Guid iid = typeof(IVirtualDesktop).GUID;

					for (int i = 0; i < count; i++) {
						object objDesktop;
						desktops.GetAt(i, ref iid, out objDesktop);

						var vd = (IVirtualDesktop)objDesktop;
						if (idSearch.CompareTo(vd.GetId()) == 0)
							return i;
					}

					return -1;
				} finally {
					if (desktops != null && Marshal.IsComObject(desktops))
						Marshal.ReleaseComObject(desktops);
				}
			}

			internal static int GetTotalVDCount() {
				return VirtualDesktopManagerInternal.GetCount(IntPtr.Zero);
			}

			internal static IVirtualDesktop GetDesktopAtIndex(int index) {
				if (index < 0)
					return null;

				VirtualDesktopManagerInternal.GetDesktops(IntPtr.Zero, out var desktops);

				try {
					var count = VirtualDesktopManagerInternal.GetCount(IntPtr.Zero);
					if (index >= count)
						return null;

					Guid iid = typeof(IVirtualDesktop).GUID;
					object objDesktop;
					desktops.GetAt(index, ref iid, out objDesktop);

					return objDesktop as IVirtualDesktop;
				} finally {
					if (desktops != null && Marshal.IsComObject(desktops))
						Marshal.ReleaseComObject(desktops);
				}
			}
		}

		#endregion
	}
}

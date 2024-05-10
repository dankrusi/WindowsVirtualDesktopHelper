// Original Implemation: windows-virtualdesktopindicator by zgdump (https://github.com/zgdump/windows-virtualdesktopindicator)
// Contributors: Dan Krusi (https://github.com/dankrusi), MScholtes (https://github.com/MScholtes), Flaflo (https://github.com/Flaflo)
// License: MIT License (https://github.com/zgdump/windows-virtualdesktopindicator/blob/main/LICENSE)

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI {
	public interface IVirtualDesktopManager {
		uint Current();

		void SwitchForward();

		void SwitchBackward();

		void SwitchToDesktop(int number);

		string CurrentDisplayName();

		uint GetVDCount();
	}
}

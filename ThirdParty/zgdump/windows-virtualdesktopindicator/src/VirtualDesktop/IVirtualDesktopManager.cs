namespace VirtualDesktopIndicator.Native.VirtualDesktop
{
    internal interface IVirtualDesktopManager
    {
        uint Current();

        void SwitchForward();

        void SwitchBackward();

        string CurrentDisplayName();
    }
}

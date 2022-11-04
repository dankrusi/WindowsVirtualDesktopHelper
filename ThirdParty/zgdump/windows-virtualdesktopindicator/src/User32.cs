using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VirtualDesktopIndicator.Native {

    public static class User32 {
        #region Native

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc? lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        #endregion

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    }

    #region Structs

    [Flags]
    public enum WindowStylesEx : uint {
        /// <summary>Specifies a window that accepts drag-drop files.</summary>
        WS_EX_ACCEPTFILES = 0x00000010,

        /// <summary>Forces a top-level window onto the taskbar when the window is visible.</summary>
        WS_EX_APPWINDOW = 0x00040000,

        /// <summary>Specifies a window that has a border with a sunken edge.</summary>
        WS_EX_CLIENTEDGE = 0x00000200,

        /// <summary>
        /// Specifies a window that paints all descendants in bottom-to-top painting order using double-buffering.
        /// This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. This style is not supported in Windows 2000.
        /// </summary>
        /// <remarks>
        /// With WS_EX_COMPOSITED set, all descendants of a window get bottom-to-top painting order using double-buffering.
        /// Bottom-to-top painting order allows a descendent window to have translucency (alpha) and transparency (color-key) effects,
        /// but only if the descendent window also has the WS_EX_TRANSPARENT bit set.
        /// Double-buffering allows the window and its descendents to be painted without flicker.
        /// </remarks>
        WS_EX_COMPOSITED = 0x02000000,

        /// <summary>
        /// Specifies a window that includes a question mark in the title bar. When the user clicks the question mark,
        /// the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message.
        /// The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command.
        /// The Help application displays a pop-up window that typically contains help for the child window.
        /// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
        /// </summary>
        WS_EX_CONTEXTHELP = 0x00000400,

        /// <summary>
        /// Specifies a window which contains child windows that should take part in dialog box navigation.
        /// If this style is specified, the dialog manager recurses into children of this window when performing navigation operations
        /// such as handling the TAB key, an arrow key, or a keyboard mnemonic.
        /// </summary>
        WS_EX_CONTROLPARENT = 0x00010000,

        /// <summary>Specifies a window that has a double border.</summary>
        WS_EX_DLGMODALFRAME = 0x00000001,

        /// <summary>
        /// Specifies a window that is a layered window.
        /// This cannot be used for child windows or if the window has a class style of either CS_OWNDC or CS_CLASSDC.
        /// </summary>
        WS_EX_LAYERED = 0x00080000,

        /// <summary>
        /// Specifies a window with the horizontal origin on the right edge. Increasing horizontal values advance to the left.
        /// The shell language must support reading-order alignment for this to take effect.
        /// </summary>
        WS_EX_LAYOUTRTL = 0x00400000,

        /// <summary>Specifies a window that has generic left-aligned properties. This is the default.</summary>
        WS_EX_LEFT = 0x00000000,

        /// <summary>
        /// Specifies a window with the vertical scroll bar (if present) to the left of the client area.
        /// The shell language must support reading-order alignment for this to take effect.
        /// </summary>
        WS_EX_LEFTSCROLLBAR = 0x00004000,

        /// <summary>
        /// Specifies a window that displays text using left-to-right reading-order properties. This is the default.
        /// </summary>
        WS_EX_LTRREADING = 0x00000000,

        /// <summary>
        /// Specifies a multiple-document interface (MDI) child window.
        /// </summary>
        WS_EX_MDICHILD = 0x00000040,

        /// <summary>
        /// Specifies a top-level window created with this style does not become the foreground window when the user clicks it.
        /// The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
        /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
        /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
        /// </summary>
        WS_EX_NOACTIVATE = 0x08000000,

        /// <summary>
        /// Specifies a window which does not pass its window layout to its child windows.
        /// </summary>
        WS_EX_NOINHERITLAYOUT = 0x00100000,

        /// <summary>
        /// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
        /// </summary>
        WS_EX_NOPARENTNOTIFY = 0x00000004,

        /// <summary>
        /// The window does not render to a redirection surface.
        /// This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
        /// </summary>
        WS_EX_NOREDIRECTIONBITMAP = 0x00200000,

        /// <summary>Specifies an overlapped window.</summary>
        WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

        /// <summary>Specifies a palette window, which is a modeless dialog box that presents an array of commands.</summary>
        WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

        /// <summary>
        /// Specifies a window that has generic "right-aligned" properties. This depends on the window class.
        /// The shell language must support reading-order alignment for this to take effect.
        /// Using the WS_EX_RIGHT style has the same effect as using the SS_RIGHT (static), ES_RIGHT (edit), and BS_RIGHT/BS_RIGHTBUTTON (button) control styles.
        /// </summary>
        WS_EX_RIGHT = 0x00001000,

        /// <summary>Specifies a window with the vertical scroll bar (if present) to the right of the client area. This is the default.</summary>
        WS_EX_RIGHTSCROLLBAR = 0x00000000,

        /// <summary>
        /// Specifies a window that displays text using right-to-left reading-order properties.
        /// The shell language must support reading-order alignment for this to take effect.
        /// </summary>
        WS_EX_RTLREADING = 0x00002000,

        /// <summary>Specifies a window with a three-dimensional border style intended to be used for items that do not accept user input.</summary>
        WS_EX_STATICEDGE = 0x00020000,

        /// <summary>
        /// Specifies a window that is intended to be used as a floating toolbar.
        /// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
        /// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
        /// If a tool window has a system menu, its icon is not displayed on the title bar.
        /// However, you can display the system menu by right-clicking or by typing ALT+SPACE.
        /// </summary>
        WS_EX_TOOLWINDOW = 0x00000080,

        /// <summary>
        /// Specifies a window that should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.
        /// To add or remove this style, use the SetWindowPos function.
        /// </summary>
        WS_EX_TOPMOST = 0x00000008,

        /// <summary>
        /// Specifies a window that should not be painted until siblings beneath the window (that were created by the same thread) have been painted.
        /// The window appears transparent because the bits of underlying sibling windows have already been painted.
        /// To achieve transparency without these restrictions, use the SetWindowRgn function.
        /// </summary>
        WS_EX_TRANSPARENT = 0x00000020,

        /// <summary>Specifies a window that has a border with a raised edge.</summary>
        WS_EX_WINDOWEDGE = 0x00000100
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public enum MouseMessages {
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_MOUSEMOVE = 0x0200,
        WM_MOUSEWHEEL = 0x020A,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205
    }

    public enum HookType {
        WH_MSGFILTER = -1,
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    public enum SystemMetric {
        SM_CXSCREEN = 0,
        SM_CYSCREEN = 1,
        SM_CXVSCROLL = 2,
        SM_CYHSCROLL = 3,
        SM_CYCAPTION = 4,
        SM_CXBORDER = 5,
        SM_CYBORDER = 6,
        SM_CXDLGFRAME = 7,
        SM_CXFIXEDFRAME = 7,
        SM_CYDLGFRAME = 8,
        SM_CYFIXEDFRAME = 8,
        SM_CYVTHUMB = 9,
        SM_CXHTHUMB = 10,
        SM_CXICON = 11,
        SM_CYICON = 12,
        SM_CXCURSOR = 13,
        SM_CYCURSOR = 14,
        SM_CYMENU = 15,
        SM_CXFULLSCREEN = 16,
        SM_CYFULLSCREEN = 17,
        SM_CYKANJIWINDOW = 18,
        SM_MOUSEPRESENT = 19,
        SM_CYVSCROLL = 20,
        SM_CXHSCROLL = 21,
        SM_DEBUG = 22,
        SM_SWAPBUTTON = 23,
        SM_CXMIN = 28,
        SM_CYMIN = 29,
        SM_CXSIZE = 30,
        SM_CYSIZE = 31,
        SM_CXSIZEFRAME = 32,
        SM_CXFRAME = 32,
        SM_CYSIZEFRAME = 33,
        SM_CYFRAME = 33,
        SM_CXMINTRACK = 34,
        SM_CYMINTRACK = 35,
        SM_CXDOUBLECLK = 36,
        SM_CYDOUBLECLK = 37,
        SM_CXICONSPACING = 38,
        SM_CYICONSPACING = 39,
        SM_MENUDROPALIGNMENT = 40,
        SM_PENWINDOWS = 41,
        SM_DBCSENABLED = 42,
        SM_CMOUSEBUTTONS = 43,
        SM_SECURE = 44,
        SM_CXEDGE = 45,
        SM_CYEDGE = 46,
        SM_CXMINSPACING = 47,
        SM_CYMINSPACING = 48,
        SM_CXSMICON = 49,
        SM_CYSMICON = 50,
        SM_CYSMCAPTION = 51,
        SM_CXSMSIZE = 52,
        SM_CYSMSIZE = 53,
        SM_CXMENUSIZE = 54,
        SM_CYMENUSIZE = 55,
        SM_ARRANGE = 56,
        SM_CXMINIMIZED = 57,
        SM_CYMINIMIZED = 58,
        SM_CXMAXTRACK = 59,
        SM_CYMAXTRACK = 60,
        SM_CXMAXIMIZED = 61,
        SM_CYMAXIMIZED = 62,
        SM_NETWORK = 63,
        SM_CLEANBOOT = 67,
        SM_CXDRAG = 68,
        SM_CYDRAG = 69,
        SM_SHOWSOUNDS = 70,
        SM_CXMENUCHECK = 71,
        SM_CYMENUCHECK = 72,
        SM_SLOWMACHINE = 73,
        SM_MIDEASTENABLED = 74,
        SM_MOUSEWHEELPRESENT = 75,
        SM_XVIRTUALSCREEN = 76,
        SM_YVIRTUALSCREEN = 77,
        SM_CXVIRTUALSCREEN = 78,
        SM_CYVIRTUALSCREEN = 79,
        SM_CMONITORS = 80,
        SM_SAMEDISPLAYFORMAT = 81,
        SM_IMMENABLED = 82,
        SM_CXFOCUSBORDER = 83,
        SM_CYFOCUSBORDER = 84,
        SM_TABLETPC = 86,
        SM_MEDIACENTER = 87,
        SM_STARTER = 88,
        SM_SERVERR2 = 89,
        SM_MOUSEHORIZONTALWHEELPRESENT = 91,
        SM_CXPADDEDBORDER = 92,
        SM_DIGITIZER = 94,
        SM_MAXIMUMTOUCHES = 95,
        SM_REMOTESESSION = 0x1000,
        SM_SHUTTINGDOWN = 0x2000,
        SM_REMOTECONTROL = 0x2001,
        SM_CONVERTIBLESLATEMODE = 0x2003,
        SM_SYSTEMDOCKED = 0x2004,
    }

    #endregion
}
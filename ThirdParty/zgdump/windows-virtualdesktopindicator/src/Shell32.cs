using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VirtualDesktopIndicator.Native.Constants;

namespace VirtualDesktopIndicator.Native {  

public static class Shell32
{
    #region Native

    [DllImport("shell32.dll", SetLastError = true)]
    public static extern int Shell_NotifyIconGetRect([In] ref NOTIFYICONIDENTIFIER identifier, [Out] out RECT iconLocation);

    #endregion
    
    #region Wrapper

    private static readonly FieldInfo? WindowField = typeof(NotifyIcon).GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? IdField = typeof(NotifyIcon).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);

    private static IntPtr GetHandle(NotifyIcon icon) => (WindowField?.GetValue(icon) as NativeWindow)?.Handle ?? IntPtr.Zero;
    private static uint GetId(NotifyIcon icon) => (uint) (IdField?.GetValue(icon) ?? -1);
    
    public static RECT GetNotifyIconRect(NotifyIcon icon)
    {
        var rect = new RECT();

        var notifyIcon = new NOTIFYICONIDENTIFIER
        {
            cbSize = (uint) Marshal.SizeOf(typeof(NOTIFYICONIDENTIFIER)),
            hWnd = GetHandle(icon),
            uID = GetId(icon)
        };

        var result = Shell_NotifyIconGetRect(ref notifyIcon, out rect);

        if (result != WinError.S_OK)
        {
            throw new Win32Exception(result);
        }

        return rect;
    }

    public static void ShellExecuteClsid(Guid guid)
    {
        Process.Start(new ProcessStartInfo
        {
            // Important, because we want to execute it using shell to resolve the CLSID
            UseShellExecute = true,
            FileName = $"shell:::{{{guid}}}"
        });
    }
    #endregion
}

#region Structs

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}

[StructLayout(LayoutKind.Sequential)]
public struct NOTIFYICONIDENTIFIER
{
    public uint cbSize;
    public IntPtr hWnd;
    public uint uID;
    public Guid guidItem;
}

#endregion
}
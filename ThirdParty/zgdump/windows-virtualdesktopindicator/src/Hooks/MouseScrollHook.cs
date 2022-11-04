using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VirtualDesktopIndicator.Native.Hooks {
    /*
    internal class MouseScrollHook {
        public event EventHandler<MouseScrollEventArgs>? MouseScroll;

        private User32.HookProc? _lowLevelMouseDelegate;
        private IntPtr _handle = IntPtr.Zero;

        public void Register() {
            _lowLevelMouseDelegate = LowLevelMouseCallback;

            using var process = Process.GetCurrentProcess();
            using var module = process.MainModule;

            if (module != null) {
                _handle = User32.SetWindowsHookEx(HookType.WH_MOUSE_LL, _lowLevelMouseDelegate, module.BaseAddress, 0);
            }
        }

        public void Unregister() {
            _lowLevelMouseDelegate -= LowLevelMouseCallback;

            User32.UnhookWindowsHookEx(_handle);
        }

        private IntPtr LowLevelMouseCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && MouseMessages.WM_MOUSEWHEEL == (MouseMessages)wParam) {
                var structure = Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                if (structure != null) {
                    var hookStruct = (MSLLHOOKSTRUCT)structure;

                    MouseScroll?.Invoke(this, new() { Delta = (int)hookStruct.mouseData });
                }
            }

            return User32.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }

    internal class MouseScrollEventArgs : EventArgs {
        public int Delta { get; set; }
    } */
}
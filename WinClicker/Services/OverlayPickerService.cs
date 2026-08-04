using System;
using System.Runtime.InteropServices;

namespace WinClicker.Services
{
    public partial class OverlayPickerService : IOverlayPickerService
    {
        private IntPtr _hMouseHook = IntPtr.Zero;
        private IntPtr _hKeyboardHook = IntPtr.Zero;
        private HookProc? _mouseHookProc;
        private HookProc? _keyHookProc;

        private delegate IntPtr HookProc(int nCode, int wParam, IntPtr lParam);

        [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW", SetLastError = true)]
        private static partial IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UnhookWindowsHookEx(IntPtr hhk);

        [LibraryImport("user32.dll")]
        private static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, int wParam, IntPtr lParam);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetCursorPos(out POINT lpPoint);

        public struct POINT { public int X; public int Y; }

        public Action? OnCancel { get; set; }

        public void StartPicking(Action<int, int> onPick, Action<int, int> onMouseMove)
        {
            _mouseHookProc = (nCode, wParam, lParam) =>
            {
                if (nCode >= 0)
                {
                    if (wParam == 0x0200) // WM_MOUSEMOVE
                    {
                        GetCursorPos(out POINT pt);

                        onMouseMove?.Invoke(pt.X, pt.Y);
                    }
                    else if (wParam == 0x0201) // WM_LBUTTONDOWN
                    {
                        GetCursorPos(out POINT pt);

                        onPick?.Invoke(pt.X, pt.Y);

                        StopPicking();

                        return (IntPtr)1;
                    }
                }

                return CallNextHookEx(_hMouseHook, nCode, wParam, lParam);
            };

            _keyHookProc = (nCode, wParam, lParam) =>
            {
                // WM_KEYDOWN or WM_SYSKEYDOWN
                if (nCode >= 0 && (wParam == 0x0100 || wParam == 0x104))
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    if (vkCode == 0x1B) // VK_ESCAPE
                    {
                        OnCancel?.Invoke();

                        StopPicking();

                        return (IntPtr)1;
                    }
                    else if (vkCode == 0x0D) // VK_RETURN
                    {
                        GetCursorPos(out POINT pt);

                        onPick?.Invoke(pt.X, pt.Y);

                        StopPicking();

                        return (IntPtr)1;
                    }
                }

                return CallNextHookEx(_hKeyboardHook, nCode, wParam, lParam);
            };

            // 14 is WH_MOUSE_LL, which is a low‐level mouse hook
            _hMouseHook = SetWindowsHookEx(14, _mouseHookProc, IntPtr.Zero, 0);
            _hKeyboardHook = SetWindowsHookEx(13, _keyHookProc, IntPtr.Zero, 0);
        }

        public void StopPicking()
        {
            if (_hMouseHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hMouseHook);

                _hMouseHook = IntPtr.Zero;

                // Frees the delegate reference to allow garbage collection
                _mouseHookProc = null;
            }

            if (_hKeyboardHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hKeyboardHook);

                _hKeyboardHook = IntPtr.Zero;

                // Frees the delegate reference to allow garbage collection
                _keyHookProc = null;
            }
        }
    }
}

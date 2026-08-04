using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinClicker.Services
{
    public interface IMouseService
    {
        void MoveMouse(int x, int y);
        void PerformClick(string button, string type);
        (int X, int Y) GetCursorPosition();
    }

    public partial class MouseService : IMouseService
    {
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetCursorPos(int X, int Y);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetCursorPos(out POINT lpPoint);

        [LibraryImport("user32.dll")]
        private static partial uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray)][In] INPUT[] pInputs, int cbSize);

        private struct POINT { public int X; public int Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint Type;
            public MOUSEINPUT Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx, dy;
            public uint mouseData, dwFlags, time;
            public UIntPtr dwExtraInfo;
        }

        private const uint INPUT_MOUSE = 0;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

        public void MoveMouse(int x, int y) => SetCursorPos(x, y);

        public (int X, int Y) GetCursorPosition()
        {
            GetCursorPos(out POINT p);
            return (p.X, p.Y);
        }

        public void PerformClick(string button, string type)
        {
            uint down = button switch { "Right" => MOUSEEVENTF_RIGHTDOWN, "Middle" => MOUSEEVENTF_MIDDLEDOWN, _ => MOUSEEVENTF_LEFTDOWN };
            uint up = button switch { "Right" => MOUSEEVENTF_RIGHTUP, "Middle" => MOUSEEVENTF_MIDDLEUP, _ => MOUSEEVENTF_LEFTUP };

            int count = type == "Double" ? 2 : 1;

            for (int i = 0; i < count; i++)
            {
                SendMouseInput(down);
                SendMouseInput(up);

                if (type == "Double" && i == 0)
                {
                    Thread.Sleep(50);
                }
            }
        }

        private static void SendMouseInput(uint flags)
        {
            INPUT[] inputs = [
                new INPUT { Type = INPUT_MOUSE, Data = new MOUSEINPUT { dwFlags = flags } }
            ];
            SendInput(1, inputs, Marshal.SizeOf<INPUT>());
        }
    }
}

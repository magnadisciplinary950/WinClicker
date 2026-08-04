using System;
using System.Runtime.InteropServices;

namespace WinClicker.Services
{
    public static class HotkeyService
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        public static (uint Modifiers, uint Key) Parse(string input)
        {
            uint modifiers = 0;
            uint key = 0;
            string[] parts = input.Split('+');

            foreach (var part in parts)
            {
                string p = part.Trim();

                switch (p)
                {
                    case "Ctrl":
                        modifiers |= MOD_CONTROL;

                        break;
                    case "Shift":
                        modifiers |= MOD_SHIFT;

                        break;
                    case "Alt":
                        modifiers |= MOD_ALT;

                        break;
                    case "Win":
                        modifiers |= MOD_WIN;

                        break;
                    default:
                        key = ParseKey(p);

                        break;
                }
            }

            return (modifiers, key);
        }

        private static uint ParseKey(string key) => key switch
        {
            "F1" => 0x70,
            "F2" => 0x71,
            "F3" => 0x72,
            "F4" => 0x73,
            "F5" => 0x74,
            "F6" => 0x75,
            "F7" => 0x76,
            "F8" => 0x77,
            "F9" => 0x78,
            "F10" => 0x79,
            "F11" => 0x7A,
            "F12" => 0x7B,
            "F13" => 0x7C,
            "F14" => 0x7D,
            "F15" => 0x7E,
            "F16" => 0x7F,
            "F17" => 0x80,
            "F18" => 0x81,
            "F19" => 0x82,
            "F20" => 0x83,
            "F21" => 0x84,
            "F22" => 0x85,
            "F23" => 0x86,
            "F24" => 0x87,
            "Enter" => 0x0D,
            "Tab" => 0x09,
            "Backspace" => 0x08,
            "ArrowLeft" => 0x25,
            "ArrowUp" => 0x26,
            "ArrowRight" => 0x27,
            "ArrowDown" => 0x28,
            "0" => 0x30,
            "1" => 0x31,
            "2" => 0x32,
            "3" => 0x33,
            "4" => 0x34,
            "5" => 0x35,
            "6" => 0x36,
            "7" => 0x37,
            "8" => 0x38,
            "9" => 0x39,
            "A" => 0x41,
            "B" => 0x42,
            "C" => 0x43,
            "D" => 0x44,
            "E" => 0x45,
            "F" => 0x46,
            "G" => 0x47,
            "H" => 0x48,
            "I" => 0x49,
            "J" => 0x4A,
            "K" => 0x4B,
            "L" => 0x4C,
            "M" => 0x4D,
            "N" => 0x4E,
            "O" => 0x4F,
            "P" => 0x50,
            "Q" => 0x51,
            "R" => 0x52,
            "S" => 0x53,
            "T" => 0x54,
            "U" => 0x55,
            "V" => 0x56,
            "W" => 0x57,
            "X" => 0x58,
            "Y" => 0x59,
            "Z" => 0x5A,
            "Num0" => 0x60,
            "Num1" => 0x61,
            "Num2" => 0x62,
            "Num3" => 0x63,
            "Num4" => 0x64,
            "Num5" => 0x65,
            "Num6" => 0x66,
            "Num7" => 0x67,
            "Num8" => 0x68,
            "Num9" => 0x69,
            _ => 0x00
        };
    }
}

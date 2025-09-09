namespace ScreenAutomation.Input
{
    using System;
    using System.Runtime.InteropServices;
    using ScreenAutomation.Core;

    public sealed class Win32InputController : IInputController
    {
        [StructLayout(LayoutKind.Sequential)] struct INPUT { public int type; public InputUnion U; }
        [StructLayout(LayoutKind.Explicit)] struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }
        [StructLayout(LayoutKind.Sequential)] struct MOUSEINPUT
        {
            public int dx, dy; public int mouseData; public int dwFlags; public int time; public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)] struct KEYBDINPUT
        {
            public short wVk; public short wScan; public int dwFlags; public int time; public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)] static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        [DllImport("user32.dll")] static extern bool SetCursorPos(int X, int Y);

        const int INPUT_MOUSE = 0;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP   = 0x0004;

        public void Click(int x, int y)
        {
            SetCursorPos(x, y);
            var inputs = new INPUT[2];
            inputs[0].type = INPUT_MOUSE; inputs[0].U.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            inputs[1].type = INPUT_MOUSE; inputs[1].U.mi.dwFlags = MOUSEEVENTF_LEFTUP;
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        }
    }
}

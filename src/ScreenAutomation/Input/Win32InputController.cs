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
            if (!SetCursorPos(x, y))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "SetCursorPos failed");
                var inputs = new INPUT[2];
                inputs[0].type = INPUT_MOUSE; inputs[0].U.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
                inputs[1].type = INPUT_MOUSE; inputs[1].U.mi.dwFlags = MOUSEEVENTF_LEFTUP;
                
                uint sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
                if (sent != (uint)inputs.Length)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), $"SendInput sent {sent}/{inputs.Length}");
        }
    }
}

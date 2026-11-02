using System.Runtime.InteropServices;

namespace StandChillowCheat;

// keyboard input via SendInput with hardware scancodes (games read these, not WM_CHAR).
// x64 layout only: INPUT is 40 bytes, the payload union starts at offset 8.
public static class InputSender
{
    public const ushort ScanW = 0x11;
    public const ushort ScanA = 0x1E;
    public const ushort ScanS = 0x1F;
    public const ushort ScanD = 0x20;

    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_SCANCODE = 0x0008;

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    private struct INPUT
    {
        [FieldOffset(0)] public uint type;
        [FieldOffset(8)] public KEYBDINPUT ki;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint n, INPUT[] inputs, int size);

    public static void KeyDown(ushort scan) => Send(scan, KEYEVENTF_SCANCODE);

    public static void KeyUp(ushort scan) => Send(scan, KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP);

    // releases every movement key, safe to call at any time
    public static void ReleaseMovement()
    {
        KeyUp(ScanW);
        KeyUp(ScanA);
        KeyUp(ScanS);
        KeyUp(ScanD);
    }

    private static void Send(ushort scan, uint flags)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            ki = new KEYBDINPUT { wScan = scan, dwFlags = flags }
        };
        if (SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>()) != 1)
            throw new InvalidOperationException($"SendInput failed, err {Marshal.GetLastWin32Error()}");
    }
}

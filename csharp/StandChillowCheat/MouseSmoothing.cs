using System.Numerics;
using System.Runtime.InteropServices;

namespace StandChillowCheat;

// relative mouse movement via SendInput, eased toward the target angle.
public static class MouseSmoothing
{
    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx, dy;
        public uint mouseData, dwFlags, time;
        public IntPtr dwExtraInfo;
    }

    private const uint INPUT_MOUSE = 0;
    private const uint MOUSEEVENTF_MOVE = 0x0001;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint n, INPUT[] inputs, int size);

    public static async Task AimAsync(Vector3 targetWorld, Vector2 currentAngles, CancellationToken ct)
    {
        // FILL: convert targetWorld to yaw/pitch with the convention from off.ViewMatrix (Source/Unity row-major)
        Vector2 desired = Vector2.Zero;
        float smooth = 0.15f;

        for (int i = 0; i < 20 && !ct.IsCancellationRequested; i++)
        {
            Vector2 delta = (desired - currentAngles) * smooth;
            MoveRelative((int)delta.X, (int)-delta.Y);
            currentAngles += delta;
            await Task.Delay(8, ct);
        }
    }

    private static void MoveRelative(int dx, int dy)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            mi = new MOUSEINPUT { dx = dx, dy = dy, dwFlags = MOUSEEVENTF_MOVE }
        };
        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }
}

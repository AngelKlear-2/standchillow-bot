using System.Runtime.InteropServices;

namespace StandChillowCheat;

// external, read-only memory access. no writes, no injection.
public sealed class ProcessMemory : IDisposable
{
    private const uint PROCESS_VM_READ = 0x0010;
    private const uint PROCESS_QUERY_INFORMATION = 0x0400;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, nuint size, out nuint read);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr h);

    private IntPtr _handle;
    public int ProcessId { get; }
    public long ModuleBase { get; private set; }

    public ProcessMemory(int pid)
    {
        ProcessId = pid;
        _handle = OpenProcess(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION, false, pid);
        if (_handle == IntPtr.Zero)
            throw new InvalidOperationException($"OpenProcess failed for pid {pid}, err {Marshal.GetLastWin32Error()}");
    }

    public void SetModuleBase(long b) => ModuleBase = b;

    public T Read<T>(long address) where T : unmanaged
    {
        int size = Marshal.SizeOf<T>();
        byte[] buf = new byte[size];
        if (!ReadProcessMemory(_handle, (IntPtr)address, buf, (nuint)size, out nuint read) || (int)read != size)
            return default;
        return MemoryMarshal.Read<T>(buf);
    }

    public long ReadPtr(long address) => Read<long>(address);

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            CloseHandle(_handle);
            _handle = IntPtr.Zero;
        }
    }
}

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StandChillowCheat;

// external, read-only memory access. no writes, no injection.
public sealed class ProcessMemory : IDisposable
{
    private const uint PROCESS_VM_READ = 0x0010;
    private const uint PROCESS_QUERY_INFORMATION = 0x0400;

    private const uint MEM_COMMIT = 0x1000;
    private const uint PAGE_NOACCESS = 0x01;
    private const uint PAGE_READONLY = 0x02;
    private const uint PAGE_READWRITE = 0x04;
    private const uint PAGE_WRITECOPY = 0x08;
    private const uint PAGE_EXECUTE = 0x10;
    private const uint PAGE_EXECUTE_READ = 0x20;
    private const uint PAGE_EXECUTE_READWRITE = 0x40;
    private const uint PAGE_EXECUTE_WRITECOPY = 0x80;
    private const uint PAGE_GUARD = 0x100;
    private const uint READABLE = PAGE_READONLY | PAGE_READWRITE | PAGE_WRITECOPY | PAGE_EXECUTE
                                | PAGE_EXECUTE_READ | PAGE_EXECUTE_READWRITE | PAGE_EXECUTE_WRITECOPY;

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, nuint size, out nuint read);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint VirtualQueryEx(IntPtr h, IntPtr addr, out MEMORY_BASIC_INFORMATION mbi, nuint len);

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

    public void ResolveModuleBase(string moduleName)
    {
        using var proc = Process.GetProcessById(ProcessId);
        foreach (ProcessModule m in proc.Modules)
        {
            if (string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
            {
                ModuleBase = m.BaseAddress.ToInt64();
                return;
            }
        }
        throw new InvalidOperationException($"module {moduleName} not found in pid {ProcessId}");
    }

    public T Read<T>(long address) where T : unmanaged
    {
        int size = Marshal.SizeOf<T>();
        byte[] buf = new byte[size];
        if (!ReadProcessMemory(_handle, (IntPtr)address, buf, (nuint)size, out nuint read) || (int)read != size)
            return default;
        return MemoryMarshal.Read<T>(buf);
    }

    public long ReadPtr(long address) => Read<long>(address);

    // reads up to buf.Length bytes at address, returns the count actually read (0 on failure)
    public int ReadBytes(long address, byte[] buf, int length)
    {
        if (!ReadProcessMemory(_handle, (IntPtr)address, buf, (nuint)length, out nuint read))
            return 0;
        return (int)read;
    }

    // committed, readable, non-guard regions of the target process
    public IEnumerable<(long Start, long Size)> EnumerateReadableRegions()
    {
        long addr = 0;
        int mbiSize = Marshal.SizeOf<MEMORY_BASIC_INFORMATION>();
        while (true)
        {
            if (VirtualQueryEx(_handle, (IntPtr)addr, out var mbi, (nuint)mbiSize) == 0)
                yield break;

            long start = mbi.BaseAddress.ToInt64();
            long size = mbi.RegionSize.ToInt64();
            if (size <= 0)
                yield break;

            bool readable = mbi.State == MEM_COMMIT
                         && (mbi.Protect & PAGE_GUARD) == 0
                         && (mbi.Protect & PAGE_NOACCESS) == 0
                         && (mbi.Protect & READABLE) != 0;
            if (readable)
                yield return (start, size);

            long next = start + size;
            if (next <= addr)
                yield break;
            addr = next;
        }
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            CloseHandle(_handle);
            _handle = IntPtr.Zero;
        }
    }
}

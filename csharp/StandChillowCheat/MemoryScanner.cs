namespace StandChillowCheat;

// resolves the PlayerManager singleton without a static instance RVA.
// il2cpp object header: first 8 bytes = Il2CppClass* (klass). TypeInfo RVA holds that klass pointer.
// scan every readable region for 8-byte-aligned values equal to the PlayerManager klass,
// then validate the candidate by checking its cfog field points at a PlayerController.
public static class MemoryScanner
{
    private const int ChunkSize = 1 << 20;

    public static long FindPlayerManager(ProcessMemory mem)
    {
        long pmKlass = mem.ReadPtr(mem.ModuleBase + off.PlayerManagerTypeInfo);
        long pcKlass = mem.ReadPtr(mem.ModuleBase + off.PlayerControllerTypeInfo);
        if (pmKlass == 0 || pcKlass == 0)
            return 0;

        byte[] buf = new byte[ChunkSize + 8];
        byte[] klassBytes = BitConverter.GetBytes(pmKlass);

        foreach (var (start, size) in mem.EnumerateReadableRegions())
        {
            for (long chunkOff = 0; chunkOff < size; chunkOff += ChunkSize)
            {
                long readAt = start + chunkOff;
                int want = (int)Math.Min(ChunkSize + 8, size - chunkOff);
                int got = mem.ReadBytes(readAt, buf, want);

                for (int i = 0; i + 8 <= got; i += 8)
                {
                    if (BitConverter.ToInt64(buf, i) != pmKlass)
                        continue;

                    long candidate = readAt + i;
                    long localPlayer = mem.ReadPtr(candidate + off.PlayerManager_LocalPlayer);
                    if (localPlayer == 0)
                        continue;
                    if (mem.ReadPtr(localPlayer) == pcKlass)
                        return candidate;
                }
            }
        }
        return 0;
    }
}

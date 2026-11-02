namespace StandChillowCheat;

// finds the enemy PlayerController by heap-scanning for objects whose klass is PlayerController.
// accept candidate only if it is not the local player, has a team that is not None and differs from local,
// and has a valid MovementController pointer.
public static class EnemyScanner
{
    private const int ChunkSize = 1 << 20;

    public static long FindEnemyController(ProcessMemory mem, long localPc)
    {
        if (localPc == 0)
            return 0;

        long pcKlass = mem.ReadPtr(mem.ModuleBase + off.PlayerControllerTypeInfo);
        if (pcKlass == 0)
            return 0;

        byte localTeam = mem.Read<byte>(localPc + off.PlayerController_Team);

        // klass of MovementController, taken from the local player (known valid).
        // candidate's movement object must share it, this rejects stray klass-pointer matches.
        long localMc = mem.ReadPtr(localPc + off.PlayerController_Movement);
        if (localMc == 0)
            return 0;
        long mcKlass = mem.ReadPtr(localMc);
        if (mcKlass == 0)
            return 0;

        byte[] buf = new byte[ChunkSize + 8];

        foreach (var (start, size) in mem.EnumerateReadableRegions())
        {
            for (long chunkOff = 0; chunkOff < size; chunkOff += ChunkSize)
            {
                long readAt = start + chunkOff;
                int want = (int)Math.Min(ChunkSize + 8, size - chunkOff);
                int got = mem.ReadBytes(readAt, buf, want);

                for (int i = 0; i + 8 <= got; i += 8)
                {
                    if (BitConverter.ToInt64(buf, i) != pcKlass)
                        continue;

                    long candidate = readAt + i;
                    if (candidate == localPc)
                        continue;

                    byte team = mem.Read<byte>(candidate + off.PlayerController_Team);
                    if (team == 0 || team == localTeam)
                        continue;

                    long mc = mem.ReadPtr(candidate + off.PlayerController_Movement);
                    if (mc == 0 || mem.ReadPtr(mc) != mcKlass)
                        continue;

                    return candidate;
                }
            }
        }
        return 0;
    }
}

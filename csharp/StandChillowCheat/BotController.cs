using System.Numerics;

namespace StandChillowCheat;

// reads world state, picks head/chest 70/30, drives aim, triggers click.
// skeleton: reader + decision logic are wired, live reads need the offsets block filled.
public sealed class BotController
{
    private readonly ProcessMemory _mem;
    private readonly Random _rng = new();
    private volatile bool _stopped;

    public BotController(ProcessMemory mem) => _mem = mem;

    public object GetGameState()
    {
        var local = ReadLocalPosition();
        var enemy = ReadEnemyPosition();
        return new
        {
            ok = true,
            local = new { x = local.X, y = local.Y, z = local.Z },
            enemy = new { x = enemy.X, y = enemy.Y, z = enemy.Z },
            stopped = _stopped
        };
    }

    public object WalkRandomly()
    {
        _stopped = false;
        // FILL: send movement input (WASD via SendInput) once the input block is confirmed
        return new { ok = true, action = "walk_randomly" };
    }

    public async Task<object> EngageEnemyAsync()
    {
        if (_stopped) return new { ok = false, error = "stopped" };

        Vector3 target = PickBone(ReadEnemyBone(_rng.Next(100) < 70 ? off.BoneHead : off.BoneChest));
        await MouseSmoothing.AimAsync(target, ReadViewAngles(), ct: default);
        await TriggerShootAsync();
        return new { ok = true, action = "engage_enemy" };
    }

    // click when crosshair is within hitbox radius, 100-150ms delay before click
    public async Task TriggerShootAsync()
    {
        int delay = _rng.Next(100, 151);
        await Task.Delay(delay);
        // FILL: SendInput LMB down/up once hitbox test is wired
    }

    public object Stop()
    {
        _stopped = true;
        return new { ok = true, action = "stop_bot" };
    }

    private Vector3 ReadLocalPosition() => Vector3.Zero; // FILL: resolve local player via off.LocalPlayer
    private Vector3 ReadEnemyPosition() => Vector3.Zero; // FILL: walk off.EntityList, pick team != local
    private Vector3 ReadEnemyBone(int boneIndex) => Vector3.Zero; // FILL: off.PlayerBones + boneIndex
    private Vector3 PickBone(Vector3 bone) => bone;
    private Vector2 ReadViewAngles() => Vector2.Zero; // FILL: derive from off.ViewMatrix
}

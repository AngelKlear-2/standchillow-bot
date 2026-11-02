using System.Numerics;

namespace StandChillowCheat;

// reads world state, picks head/chest 70/30, drives aim, triggers click.
// skeleton: reader + decision logic are wired, live reads need the offsets block filled.
public sealed class BotController
{
    private readonly ProcessMemory _mem;
    private readonly Random _rng = new();
    private volatile bool _stopped;

    private long _playerManager;

    public BotController(ProcessMemory mem) => _mem = mem;

    // lazy: the PlayerManager object only exists once a match is loaded
    private long PlayerManagerPtr
    {
        get
        {
            if (_playerManager == 0)
                _playerManager = MemoryScanner.FindPlayerManager(_mem);
            return _playerManager;
        }
    }

    private long LocalPlayerPtr
    {
        get
        {
            long pm = PlayerManagerPtr;
            return pm == 0 ? 0 : _mem.ReadPtr(pm + off.PlayerManager_LocalPlayer);
        }
    }

    public object GetGameState()
    {
        var local = ReadLocalPosition();
        var enemy = ReadEnemyPosition();
        return new
        {
            ok = true,
            local = new { x = local.X, y = local.Y, z = local.Z },
            enemy = new { x = enemy.X, y = enemy.Y, z = enemy.Z },
            stopped = _stopped,
            debug = new
            {
                module_base = _mem.ModuleBase.ToString("X"),
                player_manager = PlayerManagerPtr.ToString("X"),
                local_player = LocalPlayerPtr.ToString("X"),
                mc_vec_CC = ReadMovementVec3(0xCC),
                mc_vec_D8 = ReadMovementVec3(0xD8),
                mc_vec_E4 = ReadMovementVec3(0xE4),
                mc_vec_F0 = ReadMovementVec3(0xF0)
            }
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

    // experiment: raw Vector3 read from PlayerController managed fields, to find a position-like value
    private object ReadLocalVec3(int offset)
    {
        long pc = LocalPlayerPtr;
        if (pc == 0) return null;
        Vector3 v = _mem.Read<Vector3>(pc + offset);
        return new { x = v.X, y = v.Y, z = v.Z };
    }

    // experiment: PlayerController.<cflz>k__BackingField (MovementController, 0xE8) -> Vector3 fields
    private object ReadMovementVec3(int offset)
    {
        long pc = LocalPlayerPtr;
        if (pc == 0) return null;
        long mc = _mem.ReadPtr(pc + 0xE8);
        if (mc == 0) return null;
        Vector3 v = _mem.Read<Vector3>(mc + offset);
        return new { x = v.X, y = v.Y, z = v.Z };
    }

    // local player world position: PlayerController -> MovementController -> Vector3 at 0xCC
    private Vector3 ReadLocalPosition()
    {
        long pc = LocalPlayerPtr;
        if (pc == 0) return Vector3.Zero;
        long mc = _mem.ReadPtr(pc + off.PlayerController_Movement);
        if (mc == 0) return Vector3.Zero;
        return _mem.Read<Vector3>(mc + off.MovementController_Position);
    }
    private long _enemy;

    // enemy world position, same MovementController path as the local player
    private Vector3 ReadEnemyPosition()
    {
        if (_enemy == 0)
            _enemy = EnemyScanner.FindEnemyController(_mem, LocalPlayerPtr);
        if (_enemy == 0)
            return Vector3.Zero;

        long mc = _mem.ReadPtr(_enemy + off.PlayerController_Movement);
        if (mc == 0)
        {
            _enemy = 0; // stale object, rescan next call
            return Vector3.Zero;
        }
        return _mem.Read<Vector3>(mc + off.MovementController_Position);
    }
    private Vector3 ReadEnemyBone(int boneIndex) => Vector3.Zero; // FILL: off.PlayerBones + boneIndex
    private Vector3 PickBone(Vector3 bone) => bone;
    private Vector2 ReadViewAngles() => Vector2.Zero; // FILL: derive from off.ViewMatrix
}

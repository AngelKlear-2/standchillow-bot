// Offsets block. Every value is FILL until the dump lands.
// Regen source: Il2CppDumper / Cpp2IL against GameAssembly.dll + global-metadata.dat.
// For anything that moves per patch, switch to a signature scan instead of a static offset.
namespace StandChillowCheat;

public static class off
{
    // module base of GameAssembly.dll is resolved at runtime, these are RVAs from it
    public const string ModuleName = "GameAssembly.dll";
    public const string ProcessName = "StandChillow";

    public const long LocalPlayer      = 0; // FILL: RVA of the static local-player pointer
    public const long EntityList       = 0; // FILL: RVA of the static entity container
    public const long ViewMatrix       = 0; // FILL: RVA of the camera view*proj matrix (row-major 4x4)

    public const int  PlayerPosition   = 0; // FILL: Transform -> position offset
    public const int  PlayerTeam       = 0; // FILL: team id field offset
    public const int  PlayerHealth     = 0; // FILL: health field offset
    public const int  PlayerBones      = 0; // FILL: animator/bone array offset
    public const int  BoneHead         = 0; // FILL: head bone index
    public const int  BoneChest        = 0; // FILL: chest bone index
}

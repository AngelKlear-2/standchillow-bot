// Offsets block.
// source: dump.cs from Il2CppDumper (GameAssembly_il2cppdump_20261102_0900).
// instance-field offsets below are taken from the dump and include the 0x10 il2cpp object header.
// static / RVA values stay FILL until script.json (TypeInfo RVAs) or a signature scan is provided.
namespace StandChillowCheat;

public static class off
{
    public const string ModuleName = "GameAssembly.dll";
    public const string ProcessName = "StandChillow";  // FILL: confirm exe name in task manager

    // ---- TypeInfo RVAs (script.json ScriptMetadata, RVA from GameAssembly base) ----
    public const long PlayerManagerTypeInfo    = 0x51E9610; // Chillow.StandChillow.Player.PlayerManager_TypeInfo
    public const long PlayerControllerTypeInfo = 0x51E8A30; // Chillow.StandChillow.Player.PlayerController_TypeInfo
    public const long PlayerMainCameraTypeInfo = 0x51E94F8; // Chillow.StandChillow.Player.PlayerMainCamera_TypeInfo

    // ---- still FILL: LazySingleton<PlayerManager> static instance has no resolved address in script.json ----
    public const long LocalPlayer = 0; // FILL: static cqac of LazySingleton<PlayerManager>; use heap scan on PlayerManager klass or a signature
    public const long ViewMatrix  = 0; // FILL: camera matrix, derive from PlayerMainCamera static cfnv (offset 0x0) -> Camera

    // ---- PlayerManager (class PlayerManager : LazySingleton<PlayerManager>) ----
    public const int PlayerManager_PlayersDict = 0x28; // Dictionary<int, PlayerController> cfod
    public const int PlayerManager_LocalPlayer = 0x40; // PlayerController <cfog>k__BackingField

    // ---- PlayerController ----
    public const int PlayerController_CharacterBiped = 0x30; // BipedMap _characterBiped
    public const int PlayerController_RootTransform  = 0xA0; // Transform cflq
    public const int PlayerController_Team           = 0x79; // Team <cfll>k__BackingField (byte: None=0, Tr=1, Ct=2, Spectator=3)

    // ---- BipedMap (MonoBehaviour, Transform fields) ----
    public const int BipedMap_Head   = 0x20;
    public const int BipedMap_Neck   = 0x28;
    public const int BipedMap_Spine  = 0x30;
    public const int BipedMap_Spine1 = 0x38;
    public const int BipedMap_Spine2 = 0x40;

    // ---- bone selectors (BipedMap.yy enum values) ----
    public const int BoneHead  = 0; // BipedMap.yy.Head
    public const int BoneChest = 3; // BipedMap.yy.Spine2 (upper chest)

    // ---- still unknown in dump, not located ----
    public const int PlayerHealth = 0; // FILL: no owning-class health field resolved in dump.cs yet
}

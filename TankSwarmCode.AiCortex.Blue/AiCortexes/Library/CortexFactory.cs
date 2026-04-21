using TankSwarmCode.SwarmTank;

namespace TankSwarmCode.AiCortex.Blue.AiCortexes.Library;

public static class CortexFactory
{
    public static IAiCortex For(string tankName, int slot = 0) => tankName switch
    {
        "BlueEcm"     => new BlueEcmCortex(),
        "BlueGuard"   => new BlueGuardCortex(),
        "BlueSharp"   => new BlueSharpCortex(),
        "BlueRush"    => new BlueRushCortex(),
        "BlueStrike"  => new BlueStrikeCortex(),
        "BlueTrooper" => new BlueTrooperCortex(slot),
        "BlueSurge"   => new BlueSurgeCortex(),
        "BlueRaider"   => new BlueRaiderCortex(),
        "BlueVanguard" => new BlueVanguardCortex(),
        "BlueLancer"   => new BlueLancerCortex(),
        "BlueScout"    => new BlueScoutCortex(),
        "BluePhoenix"  => new BluePhoenixCortex(),
        "Blue12"       => new BlueSlotCortex(12),
        "Blue13"       => new BlueSlotCortex(13),
        "Blue14"       => new BlueSlotCortex(14),
        "Blue15"       => new BlueSlotCortex(15),
        "Blue16"       => new BlueSlotCortex(16),
        "Blue17"       => new BlueSlotCortex(17),
        "Blue18"       => new BlueSlotCortex(18),
        "Blue19"       => new BlueSlotCortex(19),
        "Blue20"       => new BlueSlotCortex(20),
        "Blue21"       => new BlueSlotCortex(21),
        "Blue22"       => new BlueSlotCortex(22),
        "Blue23"       => new BlueSlotCortex(23),
        "Blue24"       => new BlueSlotCortex(24),
        "Blue25"       => new BlueSlotCortex(25),
        "Blue26"       => new BlueSlotCortex(26),
        "Blue27"       => new BlueSlotCortex(27),
        "Blue28"       => new BlueSlotCortex(28),
        _ => throw new ArgumentException($"Unknown tank type: {tankName}", nameof(tankName))
    };
}

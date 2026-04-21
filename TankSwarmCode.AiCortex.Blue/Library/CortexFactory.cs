using TankSwarmCode.SwarmTank;

namespace TankSwarmCode.AiCortex.Blue.Library;

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
        _ => throw new ArgumentException($"Unknown tank type: {tankName}", nameof(tankName))
    };
}

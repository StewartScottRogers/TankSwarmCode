using TankSwarmCode.AiCortex.Blue;
using TankSwarmCode.AiCortex.Red;
using TankSwarmCode.SwarmTank.Interfaces;

namespace TankSwarmCode.AiCortex;

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
        "RedGhost"    => new RedGhostCortex(),
        "RedBlade"    => new RedBladeCortex(),
        "RedArrow"    => new RedArrowCortex(),
        "RedHammer"   => new RedHammerCortex(),
        "RedTrooper"  => new RedTrooperCortex(slot),
        _ => throw new ArgumentException($"Unknown tank type: {tankName}", nameof(tankName))
    };
}

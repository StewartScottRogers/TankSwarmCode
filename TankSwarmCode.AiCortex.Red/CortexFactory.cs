using TankSwarmCode.SwarmTank.Interfaces;

namespace TankSwarmCode.AiCortex.Red;

public static class CortexFactory
{
    public static IAiCortex For(string tankName, int slot = 0) => tankName switch
    {
        "RedGhost"    => new RedGhostCortex(),
        "RedBlade"    => new RedBladeCortex(),
        "RedArrow"    => new RedArrowCortex(),
        "RedHammer"   => new RedHammerCortex(),
        "RedTrooper"  => new RedTrooperCortex(slot),
        _ => throw new ArgumentException($"Unknown tank type: {tankName}", nameof(tankName))
    };
}

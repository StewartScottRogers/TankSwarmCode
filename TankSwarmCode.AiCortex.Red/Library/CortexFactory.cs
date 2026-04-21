using TankSwarmCode.SwarmTank;

namespace TankSwarmCode.AiCortex.Red.Library;

public static class CortexFactory
{
    public static IAiCortex For(string tankName, int slot = 0) => tankName switch
    {
        "RedGhost"    => new RedGhostCortex(),
        "RedBlade"    => new RedBladeCortex(),
        "RedArrow"    => new RedArrowCortex(),
        "RedHammer"   => new RedHammerCortex(),
        "RedTrooper"  => new RedTrooperCortex(slot),
        "Red5"        => new RedTrooperCortex(5),
        "Red6"        => new RedTrooperCortex(6),
        "Red7"        => new RedTrooperCortex(7),
        "Red8"        => new RedTrooperCortex(8),
        "Red9"        => new RedTrooperCortex(9),
        "Red10"       => new RedTrooperCortex(10),
        "Red11"       => new RedTrooperCortex(11),
        _ => throw new ArgumentException($"Unknown tank type: {tankName}", nameof(tankName))
    };
}

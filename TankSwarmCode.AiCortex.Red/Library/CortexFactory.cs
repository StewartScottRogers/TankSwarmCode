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
        "Red12"       => new RedTrooperCortex(12),
        "Red13"       => new RedTrooperCortex(13),
        "Red14"       => new RedTrooperCortex(14),
        "Red15"       => new RedTrooperCortex(15),
        "Red16"       => new RedTrooperCortex(16),
        "Red17"       => new RedTrooperCortex(17),
        "Red18"       => new RedTrooperCortex(18),
        "Red19"       => new RedTrooperCortex(19),
        "Red20"       => new RedTrooperCortex(20),
        "Red21"       => new RedTrooperCortex(21),
        "Red22"       => new RedTrooperCortex(22),
        "Red23"       => new RedTrooperCortex(23),
        "Red24"       => new RedTrooperCortex(24),
        "Red25"       => new RedTrooperCortex(25),
        "Red26"       => new RedTrooperCortex(26),
        "Red27"       => new RedTrooperCortex(27),
        _ => throw new ArgumentException($"Unknown tank type: {tankName}", nameof(tankName))
    };
}

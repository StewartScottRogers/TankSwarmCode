namespace TankSwarmCode.SwarmTank.Interfaces.Enums;

/// <summary>Defines the tactical role a tank plays within its swarm.</summary>
public enum TankRole
{
    /// <summary>No assigned role; acts as a solo combatant.</summary>
    None,

    /// <summary>Moves fast with an active spinning radar; broadcasts enemy positions to the swarm.</summary>
    Scout,

    /// <summary>Focuses fire on swarm-designated targets.</summary>
    Attacker,

    /// <summary>Holds a position and provides suppressive cover fire.</summary>
    Defender,

    /// <summary>Maintains distance, assists low-energy swarm members.</summary>
    Support,

    /// <summary>
    /// Electronic warfare specialist. Carries no cannon; instead dedicates all
    /// capacity to jamming and spoofing enemy radar. An ECCM-capable variant may
    /// forgo spoofing in favour of protecting its swarm's own radar with burnthrough.
    /// </summary>
    EcmSpecialist
}

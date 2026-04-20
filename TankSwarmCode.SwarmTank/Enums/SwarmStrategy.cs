namespace TankSwarmCode.SwarmTank.Enums;

/// <summary>Collective tactical mode broadcast by the swarm leader each epoch.</summary>
public enum SwarmStrategy
{
    /// <summary>All tanks converge on the lowest-energy enemy and focus fire.</summary>
    Wolfpack,

    /// <summary>Tanks spread to equal angular intervals around the target and fire simultaneously.</summary>
    Encircle,

    /// <summary>Tanks split into two groups attacking from opposite bearings.</summary>
    Pincer,

    /// <summary>ECM specialist activates jamming; others rush to close range under cover.</summary>
    ECMScreen,

    /// <summary>All tanks retreat to the arena corner furthest from enemies to regroup.</summary>
    Fallback,

    /// <summary>Emergency survival mode: each tank flees individually, radio silence.</summary>
    Scatter
}

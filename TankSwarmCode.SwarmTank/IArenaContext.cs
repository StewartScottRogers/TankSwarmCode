using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank;

/// <summary>
/// Read-only view of the arena that a tank can query from within its AI methods.
/// Injected by the engine via <see cref="ISwarmTank.Initialize"/>.
/// </summary>
public interface IArenaContext
{
    /// <summary>Arena width in pixels.</summary>
    double ArenaWidth { get; }

    /// <summary>Arena height in pixels.</summary>
    double ArenaHeight { get; }

    /// <summary>Total number of tanks still alive across all swarms.</summary>
    int LivingTankCount { get; }

    /// <summary>Current simulation tick number (starts at 1).</summary>
    long TickNumber { get; }

    /// <summary>Returns the number of living tanks belonging to the given swarm.</summary>
    int GetSwarmSize(int swarmId);

    /// <summary>
    /// Returns a snapshot of all active bullets.
    /// Useful for evasion logic.
    /// </summary>
    IReadOnlyList<BulletState> GetActiveBullets();

    /// <summary>
    /// Returns the list of buildings placed in the arena for this round.
    /// Buildings block movement, bullets, and radar line-of-sight.
    /// Use this for navigation and firing-angle calculations.
    /// </summary>
    IReadOnlyList<BuildingDefinition> Buildings { get; }
}

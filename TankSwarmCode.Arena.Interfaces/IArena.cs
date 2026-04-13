using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.Arena.Interfaces;

/// <summary>
/// Represents the battle arena: manages tanks and bullets, drives the simulation.
/// </summary>
public interface IArena
{
    /// <summary>Arena width in pixels.</summary>
    double Width { get; }

    /// <summary>Arena height in pixels.</summary>
    double Height { get; }

    /// <summary>Whether the simulation is currently running.</summary>
    bool IsRunning { get; }

    /// <summary>True once <see cref="Start"/> has been called (and until <see cref="Reset"/>).</summary>
    bool HasStarted { get; }

    /// <summary>Current simulation tick number.</summary>
    long TickNumber { get; }

    /// <summary>All tanks registered in the arena (alive and dead).</summary>
    IReadOnlyList<ISwarmTank> Tanks { get; }

    /// <summary>All bullets currently in flight.</summary>
    IReadOnlyList<BulletState> Bullets { get; }

    // ── Control ──────────────────────────────────────────────────────────────

    /// <summary>Registers a tank before a round begins.</summary>
    void AddTank(ISwarmTank tank);

    /// <summary>Updates the arena dimensions at runtime. Living tanks receive a fresh OnStart() call so they can recalculate waypoints.</summary>
    void Resize(double width, double height);

    /// <summary>Places all tanks at random spawn positions and starts the simulation.</summary>
    void Start();

    /// <summary>Pauses the simulation without resetting state.</summary>
    void Stop();

    /// <summary>Stops the simulation and removes all tanks and bullets.</summary>
    void Reset();

    /// <summary>Advances the simulation by exactly one tick.</summary>
    void Tick();

    // ── Events ───────────────────────────────────────────────────────────────

    /// <summary>Raised after every tick completes.</summary>
    event EventHandler<TickEventArgs>? TickCompleted;

    /// <summary>Raised when only one swarm (or one solo tank) remains alive.</summary>
    event EventHandler<RoundEndedEventArgs>? RoundEnded;
}


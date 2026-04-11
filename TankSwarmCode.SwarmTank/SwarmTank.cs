using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank;

/// <summary>
/// Base class for all player-defined tanks.
/// Subclass this and override the <c>On*</c> methods to implement your AI.
/// </summary>
/// <example>
/// <code>
/// public class MyTank : SwarmTankBase
/// {
///     public override void OnTick(TickEventArgs e)
///     {
///         SetTurnRight(5);
///         SetAhead(100);
///     }
///
///     public override void OnScannedTank(ScannedTankEventArgs e)
///     {
///         SetFire(2.0);
///     }
/// }
/// </code>
/// </example>
public abstract class SwarmTankBase : ISwarmTank
{
    private TankCommand _pendingCommand = new();
    private readonly List<SwarmMessage> _pendingMessages = [];

    // ── Identity ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Display name shown in the arena.
    /// Override to provide a custom name; defaults to the class name.
    /// </summary>
    public virtual string Name => GetType().Name;

    public int SwarmId { get; set; }
    public TankRole Role { get; set; }

    // ── State ─────────────────────────────────────────────────────────────────

    public TankState State { get; private set; } = new();
    public IArenaContext Arena { get; private set; } = null!;

    // ── Action API ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void SetAhead(double distance) =>
        _pendingCommand = _pendingCommand with { MoveDistance = distance };

    /// <inheritdoc/>
    public void SetBack(double distance) =>
        _pendingCommand = _pendingCommand with { MoveDistance = -distance };

    /// <inheritdoc/>
    public void SetTurnRight(double degrees) =>
        _pendingCommand = _pendingCommand with { BodyTurnDegrees = degrees };

    /// <inheritdoc/>
    public void SetTurnLeft(double degrees) =>
        _pendingCommand = _pendingCommand with { BodyTurnDegrees = -degrees };

    /// <inheritdoc/>
    public void SetTurnGunRight(double degrees) =>
        _pendingCommand = _pendingCommand with { GunTurnDegrees = degrees };

    /// <inheritdoc/>
    public void SetTurnGunLeft(double degrees) =>
        _pendingCommand = _pendingCommand with { GunTurnDegrees = -degrees };

    /// <inheritdoc/>
    public void SetTurnRadarRight(double degrees) =>
        _pendingCommand = _pendingCommand with { RadarTurnDegrees = degrees };

    /// <inheritdoc/>
    public void SetTurnRadarLeft(double degrees) =>
        _pendingCommand = _pendingCommand with { RadarTurnDegrees = -degrees };

    /// <inheritdoc/>
    public void SetFire(double power) =>
        _pendingCommand = _pendingCommand with { FirePower = Math.Clamp(power, 0.1, 3.0) };

    // ── Swarm communication ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Broadcast(SwarmMessage message) => _pendingMessages.Add(message);

    // ── Lifecycle overrides ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public virtual void OnStart() { }

    /// <inheritdoc/>
    public virtual void OnTick(TickEventArgs e) { }

    /// <inheritdoc/>
    public virtual void OnScannedTank(ScannedTankEventArgs e) { }

    /// <inheritdoc/>
    public virtual void OnHitByBullet(HitByBulletEventArgs e) { }

    /// <inheritdoc/>
    public virtual void OnHitWall(HitWallEventArgs e) { }

    /// <inheritdoc/>
    public virtual void OnHitTank(HitTankEventArgs e) { }

    /// <inheritdoc/>
    public virtual void OnBulletHit(BulletHitEventArgs e) { }

    /// <inheritdoc/>
    public virtual void OnSwarmMessage(SwarmMessageEventArgs e) { }

    /// <inheritdoc/>
    public virtual void OnDeath() { }

    /// <inheritdoc/>
    public virtual void OnRoundEnded(RoundEndedEventArgs e) { }

    // ── Engine-internal ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Initialize(string name, int swarmId, TankRole role, IArenaContext arenaContext)
    {
        ArgumentNullException.ThrowIfNull(arenaContext);

        SwarmId = swarmId;
        Role = role;
        Arena = arenaContext;

        // Seed initial state; the engine will overwrite this before the first tick.
        State = new TankState
        {
            Name = name,
            SwarmId = swarmId,
            Role = role,
            Energy = ArenaConstants.TankStartEnergy,
            IsAlive = true
        };
    }

    /// <inheritdoc/>
    public void UpdateState(TankState newState) => State = newState;

    /// <inheritdoc/>
    public TankCommand FlushCommand()
    {
        TankCommand cmd = _pendingCommand with { BroadcastMessages = [.. _pendingMessages] };
        _pendingCommand = new TankCommand();
        _pendingMessages.Clear();
        return cmd;
    }

    /// <inheritdoc/>
    public void DeliverSwarmMessage(SwarmMessage message) =>
        OnSwarmMessage(new SwarmMessageEventArgs(message));
}

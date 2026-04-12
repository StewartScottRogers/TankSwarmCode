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

    // Shared radar picture: keyed by enemy tank name, merged from own scans and ally RadarShare messages.
    private readonly Dictionary<string, RadarContact> _radarMap = new(StringComparer.Ordinal);

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

    /// <summary>
    /// Read-only view of all known enemy contacts for this swarm.
    /// Updated automatically from direct radar scans and from
    /// <see cref="SwarmMessageType.RadarShare"/> messages received from allies.
    /// Contacts from the opposing swarm only — ally positions are never stored here.
    /// </summary>
    public IReadOnlyDictionary<string, RadarContact> RadarMap => _radarMap;

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

    /// <summary>
    /// Called when the radar sweeps over any tank.
    /// The base implementation records enemy contacts in <see cref="RadarMap"/> and
    /// broadcasts a <see cref="SwarmMessageType.RadarShare"/> to all allies automatically.
    /// Allied tank positions are never recorded or shared.
    /// Override to add firing or other reactions; call <c>base.OnScannedTank(e)</c> first
    /// to ensure the radar map stays current.
    /// </summary>
    public virtual void OnScannedTank(ScannedTankEventArgs e)
    {
        // Only track and share enemy contacts — never expose ally positions.
        if (e.Result.SwarmId == SwarmId)
            return;

        RadarContact contact = new()
        {
            Name = e.Result.Name,
            EnemySwarmId = e.Result.SwarmId,
            Position = e.Result.Position,
            Heading = e.Result.Heading,
            Velocity = e.Result.Velocity,
            Energy = e.Result.Energy,
            Timestamp = Arena.TickNumber,
            SpottedBy = Name
        };

        MergeContact(contact);

        // Auto-broadcast fresh radar data to every living ally; the engine
        // delivers only to same-swarm members, so enemies never receive this.
        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type = SwarmMessageType.RadarShare,
            RadarContact = contact,
            Timestamp = Arena.TickNumber
        });
    }

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

    // ── Radar helpers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the most recently observed enemy contact that is not older than
    /// <paramref name="staleAfterTicks"/> ticks, or <c>null</c> if no fresh contact exists.
    /// Considers both own scans and data shared by allies.
    /// </summary>
    protected RadarContact? GetFreshestEnemy(int staleAfterTicks = 30)
    {
        long threshold = Arena.TickNumber - staleAfterTicks;
        RadarContact? best = null;

        foreach (RadarContact c in _radarMap.Values)
        {
            if (c.Timestamp < threshold)
                continue;

            if (best is null || c.Timestamp > best.Timestamp)
                best = c;
        }

        return best;
    }

    /// Merges an incoming contact into the radar map, keeping only the freshest observation per enemy.
    private void MergeContact(RadarContact incoming)
    {
        if (!_radarMap.TryGetValue(incoming.Name, out RadarContact? existing)
            || incoming.Timestamp >= existing.Timestamp)
        {
            _radarMap[incoming.Name] = incoming;
        }
    }

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
    public void DeliverSwarmMessage(SwarmMessage message)
    {
        // Automatically merge radar contacts before user code sees the message.
        // This guarantees RadarMap is always current when OnSwarmMessage is called.
        if (message.Type == SwarmMessageType.RadarShare && message.RadarContact is { } contact)
            MergeContact(contact);

        OnSwarmMessage(new SwarmMessageEventArgs(message));
    }
}

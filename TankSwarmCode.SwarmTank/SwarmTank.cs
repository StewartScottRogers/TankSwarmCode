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

    // Shared building wall map: keyed by wall face identity, merged from own echoes and ally BuildingEchoShare messages.
    private readonly Dictionary<string, BuildingEcho> _buildingWallMap = new(StringComparer.Ordinal);

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
    /// Read-only view of all tank contacts observed by this tank's own radar,
    /// including both enemies and allies. Use <see cref="RadarContact.IsAlly"/>
    /// to filter by relationship.
    /// </summary>
    public IReadOnlyDictionary<string, RadarContact> RadarMap => _radarMap;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, BuildingEcho> BuildingWallMap => _buildingWallMap;

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

    /// <inheritdoc/>
    public void SetEcm(EcmMode mode) =>
        _pendingCommand = _pendingCommand with { EcmMode = mode };

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
    /// The base implementation records all contacts (enemies and allies) in
    /// <see cref="RadarMap"/>. Use <see cref="RadarContact.IsAlly"/> to
    /// distinguish them in your AI.
    /// Override to add firing or other reactions; call <c>base.OnScannedTank(e)</c>
    /// first to ensure the radar map stays current.
    /// </summary>
    public virtual void OnScannedTank(ScannedTankEventArgs e)
    {
        RadarContact contact = new()
        {
            Name = e.Result.Name,
            EnemySwarmId = e.Result.SwarmId,
            IsAlly = e.Result.SwarmId == SwarmId,
            Position = e.Result.Position,
            Heading = e.Result.Heading,
            Velocity = e.Result.Velocity,
            Energy = e.Result.Energy,
            Timestamp = Arena.TickNumber,
            SpottedBy = Name
        };

        MergeContact(contact);

        // Auto-broadcast enemy contacts so all swarm allies can update their RadarMap.
        if (!contact.IsAlly)
        {
            Broadcast(new SwarmMessage
            {
                SenderName = Name,
                Type = SwarmMessageType.RadarShare,
                TargetName = contact.Name,
                RadarContact = contact,
                Timestamp = Arena.TickNumber
            });
        }
    }

    /// <inheritdoc/>
    public virtual void OnScannedBuilding(ScannedBuildingEventArgs e)
    {
        MergeBuildingEcho(e.Echo);

        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type = SwarmMessageType.BuildingEchoShare,
            BuildingEcho = e.Echo,
            Timestamp = Arena.TickNumber
        });
    }

    /// <inheritdoc/>
    public virtual void OnPainted(PaintedEventArgs e)
    {
        // Auto-broadcast so allies know an enemy has revealed their position by painting us
        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type = SwarmMessageType.Painted,
            TargetName = e.PainterName,
            Position = e.PainterPosition,
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
    public virtual void OnSwarmMessage(SwarmMessageEventArgs e)
    {
        // Merge any incoming radar contact so the RadarMap stays current
        // without subclasses needing to handle this themselves.
        if (e.Message.Type == SwarmMessageType.RadarShare
            && e.Message.RadarContact is { } incoming)
        {
            MergeContact(incoming);
        }

        if (e.Message.Type == SwarmMessageType.BuildingEchoShare
            && e.Message.BuildingEcho is { } incomingEcho)
        {
            MergeBuildingEcho(incomingEcho);
        }
    }

    /// <inheritdoc/>
    public virtual void OnDeath() { }

    /// <inheritdoc/>
    public virtual void OnRoundEnded(RoundEndedEventArgs e) { }

    // ── Radar helpers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the most recently observed <em>enemy</em> contact (non-ally) that is not
    /// older than <paramref name="staleAfterTicks"/> ticks, or <c>null</c> if none exists.
    /// </summary>
    protected internal RadarContact? GetFreshestEnemy(int staleAfterTicks = 30)
        => GetFreshestContact(staleAfterTicks, includeAllies: false);

    /// <summary>
    /// Returns the most recently observed contact that is not older than
    /// <paramref name="staleAfterTicks"/> ticks, or <c>null</c> if none exists.
    /// </summary>
    /// <param name="staleAfterTicks">Contacts older than this many ticks are ignored.</param>
    /// <param name="includeAllies">
    /// When <c>true</c>, allied tanks are eligible targets (friendly fire).
    /// When <c>false</c> (default), only enemy contacts are returned.
    /// </param>
    protected RadarContact? GetFreshestContact(int staleAfterTicks = 30, bool includeAllies = false)
    {
        long threshold = Arena.TickNumber - staleAfterTicks;
        RadarContact? best = null;

        foreach (RadarContact c in _radarMap.Values)
        {
            if (c.Timestamp < threshold)
                continue;

            if (!includeAllies && c.IsAlly)
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

    /// Merges an echo into the building wall map, keyed by wall face endpoints.
    private void MergeBuildingEcho(BuildingEcho incoming)
    {
        string key = string.Join("|", incoming.Walls.Select(
            w => $"{w.Start.X:F0},{w.Start.Y:F0}-{w.End.X:F0},{w.End.Y:F0}"));
        if (!_buildingWallMap.TryGetValue(key, out BuildingEcho? existing)
            || incoming.Timestamp >= existing.Timestamp)
        {
            _buildingWallMap[key] = incoming;
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
        _buildingWallMap.Clear(); // buildings regenerate each round

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
        OnSwarmMessage(new SwarmMessageEventArgs(message));
    }

    /// <inheritdoc/>
    public void DeliverPaintedEvent(PaintedEventArgs e) => OnPainted(e);
}

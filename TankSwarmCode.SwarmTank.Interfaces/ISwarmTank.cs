using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank.Interfaces;

/// <summary>
/// Core contract for every tank in the arena.
/// Users implement AI by subclassing <c>SwarmTank</c> (which implements this interface)
/// and overriding the virtual <c>On*</c> event methods.
/// </summary>
public interface ISwarmTank
{
    // ── Identity ────────────────────────────────────────────────────────────

    /// <summary>Unique display name shown on the arena. Defaults to the class name.</summary>
    string Name { get; }

    /// <summary>
    /// Swarm identifier. Tanks sharing the same id are allies and receive each
    /// other's <see cref="Broadcast"/> messages. Use 0 for a solo tank.
    /// </summary>
    int SwarmId { get; }

    /// <summary>Current tactical role. Can be changed dynamically during a round.</summary>
    TankRole Role { get; set; }

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary>Latest immutable state snapshot, updated by the engine each tick.</summary>
    TankState State { get; }

    /// <summary>Read-only view of the arena, injected by the engine before the first tick.</summary>
    IArenaContext Arena { get; }

    /// <summary>
    /// Read-only map of all known enemy radar contacts accumulated by this tank.
    /// Merged automatically from direct scans and <see cref="SwarmMessageType.RadarShare"/>
    /// messages received from allies. Keyed by enemy tank name.
    /// </summary>
    IReadOnlyDictionary<string, RadarContact> RadarMap { get; }

    /// <summary>
    /// Read-only map of building wall echoes accumulated by this tank's radar and allies.
    /// Keyed by wall face identity (endpoint coordinates). Merged automatically from
    /// direct echoes and <see cref="SwarmMessageType.BuildingEchoShare"/> messages.
    /// </summary>
    IReadOnlyDictionary<string, BuildingEcho> BuildingWallMap { get; }

    // ── Action API (call inside OnTick) ─────────────────────────────────────

    /// <summary>Sets desired forward movement in pixels for this tick.</summary>
    void SetAhead(double distance);

    /// <summary>Sets desired reverse movement in pixels for this tick.</summary>
    void SetBack(double distance);

    /// <summary>Turns the tank body right by <paramref name="degrees"/> this tick.</summary>
    void SetTurnRight(double degrees);

    /// <summary>Turns the tank body left by <paramref name="degrees"/> this tick.</summary>
    void SetTurnLeft(double degrees);

    /// <summary>Turns the gun right by <paramref name="degrees"/> this tick (independent of body).</summary>
    void SetTurnGunRight(double degrees);

    /// <summary>Turns the gun left by <paramref name="degrees"/> this tick.</summary>
    void SetTurnGunLeft(double degrees);

    /// <summary>Turns the radar right by <paramref name="degrees"/> this tick (independent of gun).</summary>
    void SetTurnRadarRight(double degrees);

    /// <summary>Turns the radar left by <paramref name="degrees"/> this tick.</summary>
    void SetTurnRadarLeft(double degrees);

    /// <summary>
    /// Fires the gun with the given power this tick.
    /// <paramref name="power"/> is clamped to [0.1, 3.0].
    /// Requires sufficient energy.
    /// </summary>
    void SetFire(double power);

    /// <summary>
    /// Activates an ECM mode for this tick. The engine drains the appropriate
    /// energy cost; if energy falls to zero the tank dies.
    /// Call with <see cref="EcmMode.Off"/> (or simply do not call) to disable ECM.
    /// </summary>
    void SetEcm(EcmMode mode);

    // ── Swarm communication ─────────────────────────────────────────────────

    /// <summary>
    /// Queues a message to be delivered to every living ally in the same swarm
    /// at the end of this tick.
    /// </summary>
    void Broadcast(SwarmMessage message);

    // ── Lifecycle callbacks (override in subclass) ──────────────────────────

    /// <summary>Called once before the first tick of a round.</summary>
    void OnStart();

    /// <summary>Called every tick. Issue <c>Set*()</c> commands here.</summary>
    void OnTick(TickEventArgs e);

    /// <summary>Called when the radar sweeps over an enemy tank.</summary>
    void OnScannedTank(ScannedTankEventArgs e);

    /// <summary>
    /// Called when the radar sweep reflects off a building wall.
    /// The base implementation records the echo in <see cref="BuildingWallMap"/> and
    /// broadcasts it to swarm allies as a <see cref="SwarmMessageType.BuildingEchoShare"/> message.
    /// Override to react tactically; call <c>base.OnScannedBuilding(e)</c> to keep the map current.
    /// </summary>
    void OnScannedBuilding(ScannedBuildingEventArgs e);

    /// <summary>
    /// Called when an enemy radar beam sweeps over this tank.
    /// The base implementation automatically broadcasts a <see cref="SwarmMessageType.Painted"/>
    /// message so allies know the painter's position.
    /// </summary>
    void OnPainted(PaintedEventArgs e);

    /// <summary>Called when this tank is hit by a bullet.</summary>
    void OnHitByBullet(HitByBulletEventArgs e);

    /// <summary>Called when this tank hits a wall.</summary>
    void OnHitWall(HitWallEventArgs e);

    /// <summary>Called when this tank physically collides with another tank.</summary>
    void OnHitTank(HitTankEventArgs e);

    /// <summary>Called when a bullet fired by this tank hits an enemy.</summary>
    void OnBulletHit(BulletHitEventArgs e);

    /// <summary>Called when a swarm ally broadcasts a message to the swarm.</summary>
    void OnSwarmMessage(SwarmMessageEventArgs e);

    /// <summary>Called when this tank's energy reaches zero.</summary>
    void OnDeath();

    /// <summary>Called for every survivor when the round ends.</summary>
    void OnRoundEnded(RoundEndedEventArgs e);

    // ── Engine-internal methods (not for user code) ─────────────────────────

    /// <summary>Initialises identity and injects arena context. Called by the engine before the round starts.</summary>
    void Initialize(string name, int swarmId, TankRole role, IArenaContext arenaContext);

    /// <summary>Replaces the current state snapshot with the engine-computed one.</summary>
    void UpdateState(TankState newState);

    /// <summary>Returns the command set by the AI this tick and resets internal state for the next tick.</summary>
    TankCommand FlushCommand();

    /// <summary>Delivers a swarm message from an ally, triggering <see cref="OnSwarmMessage"/>.</summary>
    void DeliverSwarmMessage(SwarmMessage message);

    /// <summary>Delivers a painted event from the engine, triggering <see cref="OnPainted"/>. Engine-internal.</summary>
    void DeliverPaintedEvent(PaintedEventArgs e);
}

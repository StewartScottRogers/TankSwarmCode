using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank.Enums;

/// <summary>Well-known swarm message types used for coordinated behaviour.</summary>
public enum SwarmMessageType
{
    /// <summary>Sender has spotted an enemy; <c>Position</c> and <c>TargetName</c> carry the detail.</summary>
    EnemySpotted,

    /// <summary>Sender has locked onto a target and is engaging it.</summary>
    TargetLocked,

    /// <summary>Sender is low on energy and requests assistance.</summary>
    RequestBackup,

    /// <summary>Instructs swarm members to move to a shared formation position.</summary>
    FormationMove,

    /// <summary>Instructs swarm members to retreat.</summary>
    FallBack,

    /// <summary>Sender is changing its role; <c>CustomData</c> carries the new <see cref="TankRole"/> name.</summary>
    RoleChange,

    /// <summary>Application-defined message; interpret <c>CustomData</c> freely.</summary>
    Custom,

    /// <summary>
    /// <summary>
    /// Sender has detected enemy ECM activity (jamming or ghost contacts).
    /// Recipients should activate <see cref="EcmMode.Burnthrough"/> to protect their radar.
    /// <c>CustomData</c> carries a human-readable description of the threat.
    /// </summary>
    EcmAlert,

    /// Automatic radar sighting broadcast. The message's
    /// <see cref="SwarmMessage.RadarContact"/>
    /// carries the full contact snapshot.
    /// Emitted by the base class whenever the radar sweeps over an enemy;
    /// received contacts are merged into the recipient's <c>RadarMap</c>
    /// without any user code required.
    /// </summary>
    RadarShare,

    /// <summary>
    /// [PAINTED] — this tank was swept by an enemy radar beam.
    /// <c>TargetName</c> is the painter's tank name; <c>Position</c> is the painter's
    /// arena position at the moment of the scan.
    /// Emitted automatically by the base class on <see cref="ISwarmTank.OnPainted"/>.
    /// </summary>
    Painted,

    /// <summary>
    /// Leader's epoch decree. <c>TargetName</c> is the priority target.
    /// <c>CustomData</c> carries <c>{"strategy":"Wolfpack","targetName":"X","epoch":N}</c>.
    /// </summary>
    StrategyCommand,

    /// <summary>
    /// Coordinated fire order. <c>TargetName</c> is the target.
    /// <c>CustomData</c> carries <c>{"fireAtTick":N}</c>.
    /// Each recipient computes its own offset so all bullets arrive simultaneously.
    /// </summary>
    VolleyFire,

    /// <summary>
    /// Periodic energy status ping used for leadership determination.
    /// <c>CustomData</c> carries the sender's current energy as a decimal string.
    /// </summary>
    AllyPing,

    /// <summary>
    /// Automatic building echo broadcast. The message's
    /// <see cref="SwarmMessage.BuildingEcho"/>
    /// carries the wall-face data reflected by the radar.
    /// Emitted by the base class on <see cref="ISwarmTank.OnScannedBuilding"/>;
    /// received echoes are merged into the recipient's <c>BuildingWallMap</c>.
    /// </summary>
    BuildingEchoShare
}

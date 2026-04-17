namespace TankSwarmCode.SwarmTank.Interfaces.Enums;

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
    /// <see cref="TankSwarmCode.SwarmTank.Interfaces.Models.SwarmMessage.RadarContact"/>
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
    Painted
}

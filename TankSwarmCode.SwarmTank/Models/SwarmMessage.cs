using TankSwarmCode.SwarmTank.Enums;

namespace TankSwarmCode.SwarmTank.Models;

/// <summary>
/// A message broadcast by one swarm member to all allies in the same swarm.
/// </summary>
public record SwarmMessage
{
    /// <summary>Name of the sending tank.</summary>
    public string SenderName { get; init; } = string.Empty;

    public SwarmMessageType Type { get; init; }

    /// <summary>Optional: name of the enemy being referenced.</summary>
    public string? TargetName { get; init; }

    /// <summary>Optional: arena position relevant to this message.</summary>
    public Vector2D? Position { get; init; }

    /// <summary>Optional: arbitrary string payload (e.g. serialised JSON).</summary>
    public string? CustomData { get; init; }

    /// <summary>
    /// Optional: enemy radar contact carried by a
    /// <see cref="Enums.SwarmMessageType.RadarShare"/> message.
    /// The base class populates this automatically; never contains allied positions.
    /// </summary>
    public RadarContact? RadarContact { get; init; }

    /// <summary>
    /// Optional: building wall echo carried by a
    /// <see cref="Enums.SwarmMessageType.BuildingEchoShare"/> message.
    /// The base class populates this automatically from <see cref="ISwarmTank.OnScannedBuilding"/>.
    /// </summary>
    public BuildingEcho? BuildingEcho { get; init; }

    /// <summary>Tick number when this message was created.</summary>
    public long Timestamp { get; init; }
}

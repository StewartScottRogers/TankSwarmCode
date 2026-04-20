namespace TankSwarmCode.SwarmTank.Models;

/// <summary>
/// A tank sighting recorded by one swarm member's radar sweep.
/// Stored in each tank's <c>RadarMap</c> and shared automatically via
/// <see cref="Enums.SwarmMessageType.RadarShare"/> swarm messages.
/// Both enemy and allied contacts are recorded; use <see cref="IsAlly"/>
/// to distinguish them.
/// </summary>
public record RadarContact
{
    /// <summary>Display name of the scanned tank.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Swarm ID of the scanned tank.</summary>
    public int EnemySwarmId { get; init; }

    /// <summary>
    /// <c>true</c> when the scanned tank belongs to the same swarm as the observer.
    /// Friendly-fire is permitted by the engine; this flag lets AI authors choose
    /// whether to skip or target allied tanks.
    /// </summary>
    public bool IsAlly { get; init; }

    /// <summary>Last known arena position.</summary>
    public Vector2D Position { get; init; }

    /// <summary>Last known body heading in degrees (0 = north, clockwise).</summary>
    public double Heading { get; init; }

    /// <summary>Last known velocity in pixels/tick.</summary>
    public double Velocity { get; init; }

    /// <summary>Last known energy level.</summary>
    public double Energy { get; init; }

    /// <summary>Arena tick when this contact was observed.</summary>
    public long Timestamp { get; init; }

    /// <summary>Name of the swarm member that made this observation.</summary>
    public string SpottedBy { get; init; } = string.Empty;

    /// <summary>
    /// Velocity decomposed into a 2D arena vector.
    /// Derived from <see cref="Heading"/> and <see cref="Velocity"/>.
    /// Use for linear-prediction firing calculations.
    /// </summary>
    public Vector2D VelocityVector => new(
        Velocity * Math.Sin(Heading * Math.PI / 180.0),
        -Velocity * Math.Cos(Heading * Math.PI / 180.0));
}

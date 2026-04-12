namespace TankSwarmCode.SwarmTank.Interfaces.Models;

/// <summary>
/// An enemy sighting recorded by one swarm member's radar sweep.
/// Stored in each tank's <c>RadarMap</c> and shared automatically via
/// <see cref="Enums.SwarmMessageType.RadarShare"/> swarm messages.
/// Contacts from allies are merged so every member has the fullest
/// possible picture — enemies are never included in radar sharing.
/// </summary>
public record RadarContact
{
    /// <summary>Display name of the enemy tank.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Swarm ID of the enemy.</summary>
    public int EnemySwarmId { get; init; }

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

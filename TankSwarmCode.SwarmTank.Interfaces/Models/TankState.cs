using TankSwarmCode.SwarmTank.Interfaces.Enums;

namespace TankSwarmCode.SwarmTank.Interfaces.Models;

/// <summary>
/// Immutable snapshot of a tank's state at a given tick.
/// Shared with other tanks via radar scans and swarm messages.
/// </summary>
public record TankState
{
    /// <summary>Unique display name for this tank.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Swarm identifier; tanks with the same id are allies.</summary>
    public int SwarmId { get; init; }

    /// <summary>Current tactical role within the swarm.</summary>
    public TankRole Role { get; init; }

    /// <summary>Position in arena coordinates (pixels, origin = top-left).</summary>
    public Vector2D Position { get; init; }

    /// <summary>Body heading in degrees. 0 = north (up), clockwise.</summary>
    public double Heading { get; init; }

    /// <summary>Absolute gun heading in degrees.</summary>
    public double GunHeading { get; init; }

    /// <summary>Absolute radar heading in degrees.</summary>
    public double RadarHeading { get; init; }

    /// <summary>Current velocity in pixels/tick. Positive = forward, negative = reverse.</summary>
    public double Velocity { get; init; }

    /// <summary>Remaining energy. Tank dies when this reaches zero.</summary>
    public double Energy { get; init; }

    /// <summary>Whether the tank is still in the round.</summary>
    public bool IsAlive { get; init; }
}

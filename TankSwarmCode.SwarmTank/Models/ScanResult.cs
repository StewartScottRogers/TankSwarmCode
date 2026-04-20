namespace TankSwarmCode.SwarmTank.Interfaces.Models;

/// <summary>
/// Data returned when a tank's radar sweeps over an enemy.
/// Extends Robocode's scan result with the absolute arena position.
/// </summary>
public record ScanResult
{
    /// <summary>Display name of the scanned tank.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Swarm id of the scanned tank (0 = no swarm).</summary>
    public int SwarmId { get; init; }

    /// <summary>
    /// Bearing relative to the scanner's current heading, in degrees.
    /// Negative = left, positive = right. Range: (-180, 180].
    /// </summary>
    public double Bearing { get; init; }

    /// <summary>Distance from the scanner to the target, in pixels.</summary>
    public double Distance { get; init; }

    /// <summary>Absolute heading of the scanned tank (0 = north, clockwise).</summary>
    public double Heading { get; init; }

    /// <summary>Velocity of the scanned tank in pixels/tick.</summary>
    public double Velocity { get; init; }

    /// <summary>Remaining energy of the scanned tank.</summary>
    public double Energy { get; init; }

    /// <summary>
    /// Absolute arena position of the scanned tank.
    /// Enhancement over classic Robocode: lets allies share exact coordinates.
    /// </summary>
    public Vector2D Position { get; init; }
}

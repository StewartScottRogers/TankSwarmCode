namespace TankSwarmCode.SwarmTank.Interfaces.Models;

/// <summary>Immutable snapshot of an in-flight bullet.</summary>
public record BulletState
{
    public Guid Id { get; init; }

    /// <summary>Name of the tank that fired this bullet.</summary>
    public string OwnerName { get; init; } = string.Empty;

    public Vector2D Position { get; init; }

    /// <summary>Heading in degrees (0 = north, clockwise).</summary>
    public double Heading { get; init; }

    /// <summary>Fire power in the range [0.1, 3.0].</summary>
    public double Power { get; init; }

    /// <summary>Bullet speed in pixels/tick (Robocode formula: 20 - 3 * power).</summary>
    public double Speed => 20.0 - 3.0 * Power;

    /// <summary>Damage inflicted on a direct hit (Robocode formula).</summary>
    public double Damage => 4.0 * Power + (Power > 1.0 ? 2.0 * (Power - 1.0) : 0.0);

    /// <summary>Energy the firing tank regains on a hit (Robocode formula: 3 * power).</summary>
    public double EnergyReturn => 3.0 * Power;
}

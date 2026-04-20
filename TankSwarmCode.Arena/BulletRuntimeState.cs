using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.Arena;

/// <summary>Mutable, engine-internal state for a single in-flight bullet.</summary>
internal sealed class BulletRuntimeState
{
    internal BulletRuntimeState(string ownerId, double x, double y, double heading, double power)
    {
        Id = Guid.NewGuid();
        OwnerId = ownerId;
        X = x;
        Y = y;
        Heading = heading;
        Power = power;
        Active = true;
    }

    internal Guid Id { get; }
    internal string OwnerId { get; }
    internal double X { get; set; }
    internal double Y { get; set; }
    internal double Heading { get; }
    internal double Power { get; }
    internal double Speed => 20.0 - 3.0 * Power;
    internal bool Active { get; set; }

    // ── Ricochet state ────────────────────────────────────────────────────────
    /// <summary>True once the bullet has bounced off a tank that survived.</summary>
    internal bool IsDeflected { get; set; }
    /// <summary>Heading after the bounce (degrees, 0 = north, CW).</summary>
    internal double DeflectedHeading { get; set; }
    /// <summary>Current speed of the deflected bullet; decays each tick until zero.</summary>
    internal double DeflectedSpeed { get; set; }
    /// <summary>Name of the tank this bullet bounced off — excluded from further collisions.</summary>
    internal string? DeflectedFromTank { get; set; }

    internal BulletState ToBulletState() => new()
    {
        Id          = Id,
        OwnerName   = OwnerId,
        Position    = new Vector2D(X, Y),
        Heading     = IsDeflected ? DeflectedHeading : Heading,
        Power       = Power,
        IsDeflected = IsDeflected,
        CurrentSpeed = IsDeflected ? DeflectedSpeed : Speed
    };
}

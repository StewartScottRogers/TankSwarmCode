namespace TankSwarmCode.SwarmTank.Interfaces;

/// <summary>
/// Robocode-derived physics constants shared by the engine and tank AI.
/// </summary>
public static class ArenaConstants
{
    // ── Tank movement ────────────────────────────────────────────────────────
    public const double MaxVelocity = 8.0;
    public const double Acceleration = 1.0;
    public const double Deceleration = 2.0;

    /// <summary>Max body turn rate at zero velocity (degrees/tick).</summary>
    public const double MaxTurnRate = 10.0;

    /// <summary>Velocity penalty per degree of turn: turnRate = MaxTurnRate - VelocityTurnPenalty * |v|.</summary>
    public const double VelocityTurnPenalty = 0.75;

    // ── Gun ──────────────────────────────────────────────────────────────────
    public const double MaxGunTurnRate = 20.0;

    // ── Radar ────────────────────────────────────────────────────────────────
    public const double MaxRadarTurnRate = 45.0;

    // ── Tank body ────────────────────────────────────────────────────────────
    /// <summary>Half-size of the tank body used for collision detection (pixels).</summary>
    public const double TankHalfSize = 9.0;
    public const double TankStartEnergy = 100.0;
    public const double TankCollisionDamage = 0.6;

    // ── Bullets ──────────────────────────────────────────────────────────────
    public const double BulletMinPower = 0.1;
    public const double BulletMaxPower = 3.0;

    /// <summary>Bullet radius used for collision detection (pixels).</summary>
    public const double BulletRadius = 3.0;

    // ── Wall collision ───────────────────────────────────────────────────────
    /// <summary>Wall damage = max(|velocity| * WallDamageFactor - WallDamageThreshold, 0).</summary>
    public const double WallDamageFactor = 0.5;
    public const double WallDamageThreshold = 1.0;
}

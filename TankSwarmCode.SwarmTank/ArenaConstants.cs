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

    // ── ECM (Electronic Counter-Measures) ────────────────────────────────────

    /// <summary>Energy drained per tick while in <see cref="Enums.EcmMode.Jam"/> mode.</summary>
    public const double EcmJamCostPerTick = 0.5;

    /// <summary>Energy drained per tick while in <see cref="Enums.EcmMode.Spoof"/> mode.</summary>
    public const double EcmSpoofCostPerTick = 0.8;

    /// <summary>Energy drained per tick while in <see cref="Enums.EcmMode.Burnthrough"/> mode.</summary>
    public const double EcmBurnthroughCostPerTick = 0.3;

    /// <summary>
    /// Probability [0, 1] that a radar scan is completely dropped when the target is jamming
    /// and the scanner does NOT have burnthrough active.
    /// </summary>
    public const double EcmJamDropChance = 0.50;

    /// <summary>
    /// Probability [0, 1] that a radar scan returns corrupted data when the target is jamming
    /// (and the scan was not dropped). Scanner does NOT have burnthrough active.
    /// </summary>
    public const double EcmJamCorruptChance = 0.30;

    /// <summary>Jam drop chance when the scanner IS using burnthrough.</summary>
    public const double EcmBurnthroughDropChance = 0.08;

    /// <summary>Jam corrupt chance when the scanner IS using burnthrough.</summary>
    public const double EcmBurnthroughCorruptChance = 0.08;

    /// <summary>
    /// Probability [0, 1] that a burnthrough scanner recognises and discards a ghost echo
    /// rather than passing it to <c>OnScannedTank</c>.
    /// </summary>
    public const double EcmBurnthroughGhostFilterChance = 0.70;

    /// <summary>Maximum distance (pixels) that ghost echoes are projected from the spoofing tank.</summary>
    public const double EcmSpoofRadius = 130.0;

    /// <summary>Number of ghost contacts projected by each <see cref="Enums.EcmMode.Spoof"/> tank.</summary>
    public const int EcmSpoofGhostCount = 2;
}

namespace TankSwarmCode.SwarmTank.Telemetry;

public enum TankEventType
{
    FiredBullet,        // value = bullet power
    HitByBullet,        // otherName = shooter, value = damage received
    BulletHit,          // otherName = victim name, value = damage dealt
    HitTank,            // otherName = other tank name
    HitWall,
    ScannedTank,        // otherName = scanned tank name, value = distance
    Painted,            // otherName = painter tank name
    Died,
}

public record TankEventRecord
{
    public TankEventType Type { get; init; }
    public string? OtherName { get; init; }
    public double? Value { get; init; }
}

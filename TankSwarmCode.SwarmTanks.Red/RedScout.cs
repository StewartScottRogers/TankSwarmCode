using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

/// <summary>
/// Sweeps the entire arena with a full radar rotation, broadcasting every sighting to
/// Red allies. Moves in S-curves to stay hard to hit while gathering intelligence.
/// </summary>
public sealed class RedScout : SwarmTankBase
{
    public override string Name => "RedScout";

    // S-curve oscillation state
    private int _moveTick;
    private double _moveDir = 1.0;   // +1 or -1
    private const int OscillateEvery = 35;

    public RedScout()
    {
        SwarmId = 1;
        Role = TankRole.Scout;
    }

    public override void OnTick(TickEventArgs e)
    {
        // Full radar spin every tick so nothing escapes detection
        SetTurnRadarRight(45);

        // S-curve: oscillate turn direction every OscillateEvery ticks
        _moveTick++;
        if (_moveTick >= OscillateEvery)
        {
            _moveTick = 0;
            _moveDir = -_moveDir;
        }

        SetTurnRight(_moveDir * 8);
        SetAhead(150);
    }

    public override void OnScannedTank(ScannedTankEventArgs e)
    {
        // base records the contact in RadarMap and auto-broadcasts RadarShare to Red allies.
        base.OnScannedTank(e);

        // Only react to enemies (different swarm)
        if (e.Result.SwarmId == SwarmId)
            return;

        // Light harassing shot — scout's job is intel, not killing
        TurnGunToward(e.Result.Bearing);
        SetFire(0.5);

        // Broadcast exact position to all Red allies
        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type = SwarmMessageType.EnemySpotted,
            TargetName = e.Result.Name,
            Position = e.Result.Position,
            Timestamp = Arena.TickNumber
        });
    }

    public override void OnHitByBullet(HitByBulletEventArgs e)
    {
        // Perpendicular jink away from incoming fire
        SetTurnRight(RelativeBearing(State.Heading, State.Heading + e.Bullet.Heading + 90));
        SetAhead(80);
    }

    public override void OnHitWall(HitWallEventArgs e)
    {
        _moveDir = -_moveDir;
        SetBack(40);
        SetTurnRight(30);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// Turns the gun by the relative bearing so it faces the target.
    private void TurnGunToward(double scanBearing)
    {
        double gunTurn = RelativeBearing(State.GunHeading, State.Heading + scanBearing);
        if (gunTurn >= 0)
            SetTurnGunRight(gunTurn);
        else
            SetTurnGunLeft(-gunTurn);
    }

    private static double RelativeBearing(double fromHeading, double toAbsoluteBearing)
    {
        double rel = toAbsoluteBearing - fromHeading;
        while (rel > 180) rel -= 360;
        while (rel < -180) rel += 360;
        return rel;
    }
}

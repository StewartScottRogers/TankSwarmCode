using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

/// <summary>
/// Approaches the enemy from a fixed angular offset relative to the direct bearing,
/// creating a flanking pincer with its sibling Red flanker.
/// RedWolf uses +90° (right flank), RedFox uses -90° (left flank).
/// Fires 2.0 power when gun is aligned and within 300 px.
/// Leverages the swarm's shared <c>RadarMap</c> for target acquisition.
/// </summary>
public sealed class RedFlank : SwarmTankBase
{
    private readonly string _name;
    private readonly double _orbitDegrees;   // offset from direct bearing

    public override string Name => _name;

    private const int MySwarmId = 1;
    private const double FirePower = 2.0;
    private const double EngageRange = 300.0;
    private const double OrbitRange = 180.0;  // desired orbit distance from target
    private const int StaleAfterTicks = 25;

    public RedFlank(string name, double orbitDegrees)
    {
        _name = name;
        _orbitDegrees = orbitDegrees;
        SwarmId = MySwarmId;
        Role = TankRole.Attacker;
    }

    public override void OnTick(TickEventArgs e)
    {
        // RadarMap is kept fresh from both own scans and ally RadarShare messages.
        RadarContact? target = GetFreshestEnemy(StaleAfterTicks);

        if (target is not null)
        {
            FlankAndFire(target);
        }
        else
        {
            // Sweep radar while circling slowly
            SetTurnRadarRight(45);
            SetTurnRight(5);
            SetAhead(80);
        }
    }

    public override void OnScannedTank(ScannedTankEventArgs e)
    {
        // base records the contact in RadarMap and auto-broadcasts RadarShare to Red allies.
        base.OnScannedTank(e);
    }

    public override void OnHitWall(HitWallEventArgs e)
    {
        SetBack(30);
        SetTurnRight(60);
    }

    public override void OnHitTank(HitTankEventArgs e)
    {
        SetBack(20);
        SetTurnRight(45);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void FlankAndFire(RadarContact target)
    {
        // Direction from us to target
        double directBearing = State.Position.BearingTo(target.Position);

        // Approach point is OrbitRange px from target at the flank offset angle
        double approachAngleRad = (directBearing + _orbitDegrees) * Math.PI / 180.0;
        Vector2D approachPoint = new(
            target.Position.X + OrbitRange * Math.Sin(approachAngleRad),
            target.Position.Y - OrbitRange * Math.Cos(approachAngleRad));

        double approachBearing = State.Position.BearingTo(approachPoint);
        double distToApproach = State.Position.DistanceTo(approachPoint);

        // Steer body toward approach point
        double bodyTurn = RelativeBearing(State.Heading, approachBearing);
        if (bodyTurn >= 0)
            SetTurnRight(Math.Min(bodyTurn, 10));
        else
            SetTurnLeft(Math.Min(-bodyTurn, 10));

        if (distToApproach > 20)
            SetAhead(distToApproach);

        // Linear-predict gun aim using VelocityVector from the contact
        double bulletSpeed = 20.0 - 3.0 * FirePower;
        double distToTarget = State.Position.DistanceTo(target.Position);
        double travelTicks = distToTarget / bulletSpeed;

        Vector2D predictedPos = new(
            target.Position.X + target.VelocityVector.X * travelTicks,
            target.Position.Y + target.VelocityVector.Y * travelTicks);

        double gunBearing = State.Position.BearingTo(predictedPos);
        double gunTurn = RelativeBearing(State.GunHeading, gunBearing);

        if (gunTurn >= 0)
            SetTurnGunRight(Math.Min(gunTurn, 20));
        else
            SetTurnGunLeft(Math.Min(-gunTurn, 20));

        // Radar lock
        double radarTurn = RelativeBearing(State.RadarHeading, gunBearing);
        if (radarTurn >= 0)
            SetTurnRadarRight(Math.Min(radarTurn + 5, 45));
        else
            SetTurnRadarLeft(Math.Min(-radarTurn + 5, 45));

        // Fire when aligned and close enough
        if (Math.Abs(gunTurn) < 15 && distToTarget < EngageRange)
            SetFire(FirePower);
    }

    private static double RelativeBearing(double fromHeading, double toAbsoluteBearing)
    {
        double rel = toAbsoluteBearing - fromHeading;
        while (rel > 180) rel -= 360;
        while (rel < -180) rel += 360;
        return rel;
    }
}

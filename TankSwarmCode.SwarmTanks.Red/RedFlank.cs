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

    // Target state
    private Vector2D _targetPos;
    private Vector2D _targetVelocityVec;
    private string? _targetName;
    private long _lastTargetTick;
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
        bool hasTarget = _targetName is not null
            && (Arena.TickNumber - _lastTargetTick) <= StaleAfterTicks;

        if (hasTarget)
        {
            FlankAndFire();
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
        if (e.Result.SwarmId == SwarmId)
            return;

        _targetName = e.Result.Name;
        _targetPos = e.Result.Position;

        double rad = e.Result.Heading * Math.PI / 180.0;
        _targetVelocityVec = new Vector2D(
            e.Result.Velocity * Math.Sin(rad),
            -e.Result.Velocity * Math.Cos(rad));

        _lastTargetTick = Arena.TickNumber;
    }

    public override void OnSwarmMessage(SwarmMessageEventArgs e)
    {
        // Accept intel from scout when stale
        if (e.Message.Type != SwarmMessageType.EnemySpotted)
            return;

        bool ourTargetStale = _targetName is null
            || (Arena.TickNumber - _lastTargetTick) > StaleAfterTicks;

        if (ourTargetStale && e.Message.Position.HasValue)
        {
            _targetName = e.Message.TargetName;
            _targetPos = e.Message.Position.Value;
            _targetVelocityVec = default;
            _lastTargetTick = e.Message.Timestamp;
        }
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

    private void FlankAndFire()
    {
        // Direction from us to target
        double directBearing = State.Position.BearingTo(_targetPos);

        // Approach point is OrbitRange px from target at the flank offset angle
        double approachAngleRad = (directBearing + _orbitDegrees) * Math.PI / 180.0;
        Vector2D approachPoint = new(
            _targetPos.X + OrbitRange * Math.Sin(approachAngleRad),
            _targetPos.Y - OrbitRange * Math.Cos(approachAngleRad));

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

        // Linear-predict gun aim
        double bulletSpeed = 20.0 - 3.0 * FirePower;
        double distToTarget = State.Position.DistanceTo(_targetPos);
        double travelTicks = distToTarget / bulletSpeed;

        Vector2D predictedPos = new(
            _targetPos.X + _targetVelocityVec.X * travelTicks,
            _targetPos.Y + _targetVelocityVec.Y * travelTicks);

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

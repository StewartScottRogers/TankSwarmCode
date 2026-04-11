using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

/// <summary>
/// Closes on enemies using linear prediction firing at power 2.5.
/// Listens for EnemySpotted swarm messages to pick up targets found by the Scout.
/// Broadcasts RequestBackup and retreats when energy drops below 25.
/// </summary>
public sealed class RedAttacker : SwarmTankBase
{
    private readonly string _name;
    public override string Name => _name;

    private const int MySwarmId = 1;
    private const double FirePower = 2.5;

    // Target tracking
    private Vector2D _targetPos;
    private Vector2D _targetVelocityVec;
    private double _targetHeading;
    private double _targetVelocity;
    private string? _targetName;
    private long _lastTargetTick;
    private const int StaleAfterTicks = 20;

    // Retreat state
    private bool _retreating;
    private const double BackupEnergy = 25.0;
    private const double RecoverEnergy = 35.0;

    public RedAttacker(string name)
    {
        _name = name;
        SwarmId = MySwarmId;
        Role = TankRole.Attacker;
    }

    public override void OnTick(TickEventArgs e)
    {
        // Check retreat conditions
        if (!_retreating && State.Energy < BackupEnergy)
        {
            _retreating = true;
            Broadcast(new SwarmMessage
            {
                SenderName = Name,
                Type = SwarmMessageType.RequestBackup,
                Position = State.Position,
                Timestamp = Arena.TickNumber
            });
        }
        else if (_retreating && State.Energy > RecoverEnergy)
        {
            _retreating = false;
        }

        if (_retreating)
        {
            RetreatToCenter();
            return;
        }

        bool hasTarget = _targetName is not null
            && (Arena.TickNumber - _lastTargetTick) <= StaleAfterTicks;

        if (hasTarget)
        {
            PursueAndFire();
        }
        else
        {
            // No target: sweep radar and cruise forward
            SetTurnRadarRight(45);
            SetAhead(100);
        }
    }

    public override void OnScannedTank(ScannedTankEventArgs e)
    {
        if (e.Result.SwarmId == SwarmId)
            return;

        UpdateTarget(e.Result.Name, e.Result.Position,
            e.Result.Heading, e.Result.Velocity, e.Result.Bearing, e.Result.Distance);
    }

    public override void OnSwarmMessage(SwarmMessageEventArgs e)
    {
        // Accept scout intelligence when our own target is stale
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
        SetTurnRight(45);
    }

    public override void OnHitTank(HitTankEventArgs e)
    {
        SetBack(20);
        SetTurnRight(30);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void UpdateTarget(string targetName, Vector2D pos, double heading,
        double velocity, double bearing, double distance)
    {
        _targetName = targetName;

        double rad = heading * Math.PI / 180.0;
        _targetVelocityVec = new Vector2D(velocity * Math.Sin(rad), -velocity * Math.Cos(rad));
        _targetPos = pos;
        _targetHeading = heading;
        _targetVelocity = velocity;
        _lastTargetTick = Arena.TickNumber;
    }

    private void PursueAndFire()
    {
        // Linear prediction: estimate where target will be when bullet arrives
        double bulletSpeed = 20.0 - 3.0 * FirePower;
        double distance = State.Position.DistanceTo(_targetPos);
        double travelTicks = distance / bulletSpeed;

        Vector2D predictedPos = new(
            _targetPos.X + _targetVelocityVec.X * travelTicks,
            _targetPos.Y + _targetVelocityVec.Y * travelTicks);

        double absBearing = State.Position.BearingTo(predictedPos);
        double gunTurn = RelativeBearing(State.GunHeading, absBearing);
        double bodyTurn = RelativeBearing(State.Heading, absBearing);

        // Turn body toward target for approach
        if (Math.Abs(bodyTurn) > 5)
        {
            if (bodyTurn >= 0)
                SetTurnRight(Math.Min(bodyTurn, 10));
            else
                SetTurnLeft(Math.Min(-bodyTurn, 10));
        }

        // Close distance if too far; hold position if comfortably in range
        if (distance > 200)
            SetAhead(distance - 150);
        else if (distance < 100)
            SetBack(50);

        // Turn gun to predicted position
        if (gunTurn >= 0)
            SetTurnGunRight(Math.Min(gunTurn, 20));
        else
            SetTurnGunLeft(Math.Min(-gunTurn, 20));

        // Narrow radar lock on target
        double radarTurn = RelativeBearing(State.RadarHeading, absBearing);
        if (radarTurn >= 0)
            SetTurnRadarRight(Math.Min(radarTurn + 5, 45));
        else
            SetTurnRadarLeft(Math.Min(-radarTurn + 5, 45));

        // Fire when gun is roughly on target
        if (Math.Abs(gunTurn) < 10)
            SetFire(FirePower);
    }

    private void RetreatToCenter()
    {
        double cx = Arena.ArenaWidth / 2;
        double cy = Arena.ArenaHeight / 2;
        var center = new Vector2D(cx, cy);

        double bearing = RelativeBearing(State.Heading, State.Position.BearingTo(center));
        if (bearing >= 0)
            SetTurnRight(bearing);
        else
            SetTurnLeft(-bearing);

        SetAhead(100);

        // Keep radar spinning while retreating
        SetTurnRadarRight(45);
    }

    private static double RelativeBearing(double fromHeading, double toAbsoluteBearing)
    {
        double rel = toAbsoluteBearing - fromHeading;
        while (rel > 180) rel -= 360;
        while (rel < -180) rel += 360;
        return rel;
    }
}

using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Patrols a set of waypoints in its half of the arena (left or right).
/// Engages on contact with adaptive power and retreats to centre when low on energy.
/// Uses swarm messages to converge on Warden-spotted targets.
/// </summary>
public sealed class BluePatrol : SwarmTankBase
{
    private readonly string _name;
    private readonly int _patrolSide;   // 0 = left half, 1 = right half

    public override string Name => _name;

    private const int MySwarmId = 2;
    private const double Margin = 80.0;
    private const double WaypointRadius = 45.0;
    private const double BackupEnergy = 20.0;
    private const double RecoverEnergy = 30.0;

    private Vector2D[] _waypoints = [];
    private int _waypointIndex;
    private bool _retreating;

    // Target from swarm message or direct scan
    private Vector2D? _lastEnemyPos;
    private long _lastEnemyTick;
    private const int StaleAfterTicks = 30;

    public BluePatrol(string name, int patrolSide)
    {
        _name = name;
        _patrolSide = patrolSide;
        SwarmId = MySwarmId;
        Role = TankRole.Defender;
    }

    public override void OnStart()
    {
        double w = Arena.ArenaWidth;
        double h = Arena.ArenaHeight;
        double midX = w / 2;

        // Patrol the assigned half: two corners + midpoint on our side
        if (_patrolSide == 0)
        {
            // Left half
            _waypoints =
            [
                new Vector2D(Margin,        Margin),
                new Vector2D(midX - Margin, Margin),
                new Vector2D(midX - Margin, h - Margin),
                new Vector2D(Margin,        h - Margin)
            ];
        }
        else
        {
            // Right half
            _waypoints =
            [
                new Vector2D(midX + Margin, Margin),
                new Vector2D(w - Margin,    Margin),
                new Vector2D(w - Margin,    h - Margin),
                new Vector2D(midX + Margin, h - Margin)
            ];
        }
    }

    public override void OnTick(TickEventArgs e)
    {
        if (!_retreating && State.Energy < BackupEnergy)
            _retreating = true;
        else if (_retreating && State.Energy > RecoverEnergy)
            _retreating = false;

        if (_retreating)
        {
            RetreatToCenter();
            return;
        }

        bool hasEnemyIntel = _lastEnemyPos.HasValue
            && (Arena.TickNumber - _lastEnemyTick) <= StaleAfterTicks;

        if (hasEnemyIntel)
        {
            MoveToward(_lastEnemyPos!.Value, 150);
        }
        else
        {
            AdvanceWaypoint();
        }

        // Keep radar spinning
        SetTurnRadarRight(45);
    }

    public override void OnScannedTank(ScannedTankEventArgs e)
    {
        if (e.Result.SwarmId == SwarmId)
            return;

        _lastEnemyPos = e.Result.Position;
        _lastEnemyTick = Arena.TickNumber;

        double power = e.Result.Distance < 200 ? 2.5 : 1.5;
        double gunTurn = RelativeBearing(State.GunHeading, State.Heading + e.Result.Bearing);

        if (gunTurn >= 0) SetTurnGunRight(Math.Min(gunTurn, 20));
        else SetTurnGunLeft(Math.Min(-gunTurn, 20));

        if (Math.Abs(gunTurn) < 12)
            SetFire(power);
    }

    public override void OnSwarmMessage(SwarmMessageEventArgs e)
    {
        if (e.Message.Type != SwarmMessageType.EnemySpotted)
            return;

        bool stale = !_lastEnemyPos.HasValue
            || (Arena.TickNumber - _lastEnemyTick) > StaleAfterTicks;

        if (stale && e.Message.Position.HasValue)
        {
            _lastEnemyPos = e.Message.Position;
            _lastEnemyTick = e.Message.Timestamp;
        }
    }

    public override void OnHitWall(HitWallEventArgs e)
    {
        SetBack(30);
        SetTurnRight(45);
        _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void AdvanceWaypoint()
    {
        if (_waypoints.Length == 0)
            return;

        Vector2D target = _waypoints[_waypointIndex];
        double dist = State.Position.DistanceTo(target);

        if (dist < WaypointRadius)
        {
            _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
            return;
        }

        MoveToward(target, dist);
    }

    private void MoveToward(Vector2D target, double speed)
    {
        double bearing = RelativeBearing(State.Heading, State.Position.BearingTo(target));
        if (bearing >= 0) SetTurnRight(Math.Min(bearing, 10));
        else SetTurnLeft(Math.Min(-bearing, 10));
        SetAhead(speed);
    }

    private void RetreatToCenter()
    {
        var center = new Vector2D(Arena.ArenaWidth / 2, Arena.ArenaHeight / 2);
        MoveToward(center, 100);
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

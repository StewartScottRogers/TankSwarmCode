using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Blue team anchor. Holds the arena centre, rotating slowly to keep its radar
/// covering all approach vectors. Fires with power scaled to distance and
/// broadcasts every sighting to Blue allies.
/// </summary>
public sealed class BlueWarden : SwarmTankBase
{
    public override string Name => "BlueWarden";

    private const int MySwarmId = 2;
    private const double HoldRadius = 60.0;  // oscillate within this radius of centre

    // Centre coordinates — set on start once arena size is known
    private double _cx;
    private double _cy;

    // Oscillation
    private double _oscillateDir = 1.0;
    private int _oscillateTick;
    private const int OscillateEvery = 25;
    private const double OscillateDistance = 35.0;

    public BlueWarden()
    {
        SwarmId = MySwarmId;
        Role = TankRole.Defender;
    }

    public override void OnStart()
    {
        _cx = Arena.ArenaWidth / 2;
        _cy = Arena.ArenaHeight / 2;
    }

    public override void OnTick(TickEventArgs e)
    {
        double distToCenter = State.Position.DistanceTo(new Vector2D(_cx, _cy));

        if (distToCenter > HoldRadius)
        {
            // Navigate back to centre
            double bearing = RelativeBearing(State.Heading,
                State.Position.BearingTo(new Vector2D(_cx, _cy)));
            if (bearing >= 0) SetTurnRight(bearing); else SetTurnLeft(-bearing);
            SetAhead(distToCenter - HoldRadius / 2);
        }
        else
        {
            // Oscillate in place
            _oscillateTick++;
            if (_oscillateTick >= OscillateEvery)
            {
                _oscillateTick = 0;
                _oscillateDir = -_oscillateDir;
            }
            SetTurnRight(_oscillateDir * 5);
            SetAhead(_oscillateDir * OscillateDistance);
        }

        // Continuous radar sweep
        SetTurnRadarRight(45);
    }

    public override void OnScannedTank(ScannedTankEventArgs e)
    {
        // base records the contact in RadarMap and auto-broadcasts RadarShare to Blue allies.
        base.OnScannedTank(e);

        if (e.Result.SwarmId == SwarmId)
            return;

        // Adaptive fire power: close = high power, far = low power
        double power = e.Result.Distance switch
        {
            < 150 => 3.0,
            < 300 => 2.0,
            _     => 1.0
        };

        double gunTurn = RelativeBearing(State.GunHeading, State.Heading + e.Result.Bearing);
        if (gunTurn >= 0) SetTurnGunRight(Math.Min(gunTurn, 20));
        else SetTurnGunLeft(Math.Min(-gunTurn, 20));

        if (Math.Abs(gunTurn) < 10)
            SetFire(power);

        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type = SwarmMessageType.EnemySpotted,
            TargetName = e.Result.Name,
            Position = e.Result.Position,
            Timestamp = Arena.TickNumber
        });
    }

    public override void OnHitWall(HitWallEventArgs e)
    {
        SetBack(30);
        SetTurnRight(45);
    }

    private static double RelativeBearing(double fromHeading, double toAbsoluteBearing)
    {
        double rel = toAbsoluteBearing - fromHeading;
        while (rel > 180) rel -= 360;
        while (rel < -180) rel += 360;
        return rel;
    }
}

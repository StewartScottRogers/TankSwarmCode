using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Camps in an arena corner, fires at maximum power 3.0 with tight gun alignment,
/// and evades incoming bullets using dot-product inbound detection.
/// Relocates to the diagonally opposite corner if an enemy closes within 80 px.
/// Leverages the swarm's shared <c>RadarMap</c> for target acquisition.
/// </summary>
public sealed class BlueSniper : SwarmTankBase
{
    private readonly string _name;
    private readonly bool _topLeft;   // true = top-left corner, false = bottom-right

    public override string Name => _name;

    private const int MySwarmId = 2;
    private const double FirePower = 3.0;
    private const double CornerMargin = 90.0;
    private const double TooCloseRange = 80.0;
    private const double GunAlignThreshold = 5.0;

    private Vector2D _cornerA;
    private Vector2D _cornerB;
    private bool _useCornerA = true;   // toggles on relocation
    private bool _relocating;
    private const int StaleAfterTicks = 25;

    // Commander orders
    private string? _commandTarget;   // priority target name from TargetLocked

    public BlueSniper(string name, bool topLeft)
    {
        _name = name;
        _topLeft = topLeft;
        SwarmId = MySwarmId;
        Role = TankRole.Support;
    }

    public override void OnStart()
    {
        double w = Arena.ArenaWidth;
        double h = Arena.ArenaHeight;
        double m = CornerMargin;

        if (_topLeft)
        {
            _cornerA = new Vector2D(m, m);
            _cornerB = new Vector2D(w - m, h - m);
        }
        else
        {
            _cornerA = new Vector2D(w - m, m);
            _cornerB = new Vector2D(m, h - m);
        }
    }

    public override void OnTick(TickEventArgs e)
    {
        // Check bullet evasion every tick (highest priority)
        if (TryEvadeBullet())
            return;

        Vector2D myCorner = _useCornerA ? _cornerA : _cornerB;
        double distToCorner = State.Position.DistanceTo(myCorner);

        if (_relocating || distToCorner > 25)
        {
            MoveToCorner(myCorner);
            if (distToCorner < 20)
                _relocating = false;
        }
        else
        {
            // Settled in corner — minimal movement, focus on aiming
            SetAhead(0);
        }

        // Prefer the Commander's nominated target; fall back to freshest contact.
        RadarContact? target = GetCommandedOrFreshestEnemy();

        if (target is not null)
        {
            // Relocate if target is dangerously close
            if (State.Position.DistanceTo(target.Position) < TooCloseRange)
            {
                _useCornerA = !_useCornerA;
                _relocating = true;
            }

            AimAndFire(target);
        }
        else
        {
            // Slowly sweep radar
            SetTurnRadarRight(45);
        }
    }

    public override void OnScannedTank(ScannedTankEventArgs e)
    {
        // base records the contact in RadarMap and auto-broadcasts RadarShare to Blue allies.
        base.OnScannedTank(e);
    }

    public override void OnSwarmMessage(SwarmMessageEventArgs e)
    {
        base.OnSwarmMessage(e);

        // Snipers only honour target orders — they never abandon their corner for a rally.
        if (e.Message.Type == SwarmMessageType.TargetLocked)
            _commandTarget = e.Message.TargetName;
    }

    public override void OnHitWall(HitWallEventArgs e)
    {
        SetBack(20);
        SetTurnRight(30);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private RadarContact? GetCommandedOrFreshestEnemy()
    {
        if (_commandTarget is not null
            && RadarMap.TryGetValue(_commandTarget, out RadarContact? ordered)
            && !ordered.IsAlly
            && Arena.TickNumber - ordered.Timestamp <= StaleAfterTicks)
        {
            return ordered;
        }
        return GetFreshestEnemy(StaleAfterTicks);
    }

    /// Returns true and issues evasive commands if an inbound bullet is detected.
    private bool TryEvadeBullet()
    {
        foreach (BulletState bullet in Arena.GetActiveBullets())
        {
            // BulletState.OwnerName is available, but mapping a name back to a swarm ID
            // requires additional tracking; treat all inbound bullets as threats for now.
            Vector2D toTank = new(
                State.Position.X - bullet.Position.X,
                State.Position.Y - bullet.Position.Y);

            double bulletSpeed = bullet.Speed;
            double bulletHeadingRad = bullet.Heading * Math.PI / 180.0;
            Vector2D bulletDir = new(
                Math.Sin(bulletHeadingRad) * bulletSpeed,
                -Math.Cos(bulletHeadingRad) * bulletSpeed);

            // Dot product: positive means bullet is heading roughly toward us
            double dot = bulletDir.X * toTank.X + bulletDir.Y * toTank.Y;
            double distSq = toTank.X * toTank.X + toTank.Y * toTank.Y;

            bool inbound = dot > 0 && distSq < 200 * 200;
            if (!inbound)
                continue;

            // Evade perpendicular to bullet trajectory
            double perpBearing = bullet.Heading + 90;
            double bodyTurn = RelativeBearing(State.Heading, perpBearing);
            if (bodyTurn >= 0) SetTurnRight(bodyTurn); else SetTurnLeft(-bodyTurn);
            SetAhead(50);
            return true;
        }

        return false;
    }

    private void AimAndFire(RadarContact target)
    {
        double bulletSpeed = 20.0 - 3.0 * FirePower;
        double distToTarget = State.Position.DistanceTo(target.Position);
        double travelTicks = distToTarget / bulletSpeed;

        Vector2D predictedPos = new(
            target.Position.X + target.VelocityVector.X * travelTicks,
            target.Position.Y + target.VelocityVector.Y * travelTicks);

        double gunBearing = State.Position.BearingTo(predictedPos);
        double gunTurn = RelativeBearing(State.GunHeading, gunBearing);

        if (gunTurn >= 0) SetTurnGunRight(Math.Min(gunTurn, 20));
        else SetTurnGunLeft(Math.Min(-gunTurn, 20));

        // Radar narrows onto predicted target
        double radarTurn = RelativeBearing(State.RadarHeading, gunBearing);
        if (radarTurn >= 0) SetTurnRadarRight(Math.Min(radarTurn + 3, 45));
        else SetTurnRadarLeft(Math.Min(-radarTurn + 3, 45));

        // Only fire when gun is tightly aligned — sniper precision
        if (Math.Abs(gunTurn) < GunAlignThreshold)
            SetFire(FirePower);
    }

    private void MoveToCorner(Vector2D corner)
    {
        double bearing = RelativeBearing(State.Heading, State.Position.BearingTo(corner));
        if (bearing >= 0) SetTurnRight(Math.Min(bearing, 10));
        else SetTurnLeft(Math.Min(-bearing, 10));
        SetAhead(100);
    }

    private static double RelativeBearing(double fromHeading, double toAbsoluteBearing)
    {
        double rel = toAbsoluteBearing - fromHeading;
        while (rel > 180) rel -= 360;
        while (rel < -180) rel += 360;
        return rel;
    }
}

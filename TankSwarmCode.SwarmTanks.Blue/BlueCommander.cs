using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Blue swarm command-and-control tank.
/// Holds the arena centre, selects priority targets for the whole swarm to focus fire,
/// relays formation orders when allies call for backup, and falls back to defend
/// when its own energy runs low.
/// </summary>
public sealed class BlueCommander : SwarmTankBase
{
    public override string Name => "BlueCommand";

    private const int    MySwarmId              = 2;
    private const double HoldRadius             = 70.0;   // px — drift back to centre if further away
    private const double CloseRangeThreshold    = 150.0;  // px — use max power inside this range
    private const double NormalFirePower        = 2.0;
    private const double CloseFirePower         = 3.0;
    private const double FireAlignmentDeg       = 8.0;    // gun must be within this to fire
    private const double RetreatEnergyThreshold = 28.0;
    private const double RecoverEnergyThreshold = 55.0;
    private const int    CoordBroadcastInterval = 20;     // ticks between TargetLocked broadcasts
    private const int    StaleAfterTicks        = 30;

    private double _cx;
    private double _cy;
    private bool   _retreating;

    public BlueCommander()
    {
        SwarmId = MySwarmId;
        Role    = TankRole.Attacker;
    }

    public override void OnStart()
    {
        _cx        = Arena.ArenaWidth  / 2.0;
        _cy        = Arena.ArenaHeight / 2.0;
        _retreating = false;
    }

    public override void OnTick(TickEventArgs e)
    {
        // Always spin radar for full situational awareness
        SetTurnRadarRight(45);

        // Energy gate
        if (State.Energy < RetreatEnergyThreshold) _retreating = true;
        if (State.Energy > RecoverEnergyThreshold)  _retreating = false;

        if (_retreating)
        {
            MoveToCenter();
            return;
        }

        RadarContact? target = SelectPriorityTarget();

        if (target == null)
        {
            Patrol();
            return;
        }

        // Periodically tell the swarm what to shoot at
        if (e.TickNumber % CoordBroadcastInterval == 0)
            BroadcastPriorityTarget(target);

        AimAndFire(target);
        MaintainPosition();
    }

    public override void OnScannedTank(ScannedTankEventArgs e)
    {
        // Keeps RadarMap current and auto-broadcasts RadarShare to Blue allies
        base.OnScannedTank(e);
    }

    public override void OnSwarmMessage(SwarmMessageEventArgs e)
    {
        // Merges incoming RadarShare contacts into RadarMap
        base.OnSwarmMessage(e);

        SwarmMessage msg = e.Message;

        // When an ally calls for backup, redirect the whole swarm toward that position
        if (msg.Type == SwarmMessageType.RequestBackup && msg.Position is { } helpPos)
        {
            Broadcast(new SwarmMessage
            {
                SenderName = Name,
                Type       = SwarmMessageType.FormationMove,
                Position   = helpPos,
                Timestamp  = Arena.TickNumber
            });
        }
    }

    public override void OnHitByBullet(HitByBulletEventArgs e)
    {
        // Side-step perpendicular to the incoming round
        double dodge = e.Bearing > 0 ? -90 : 90;
        SetTurnRight(dodge);
        SetAhead(55);
    }

    public override void OnHitWall(HitWallEventArgs e)
    {
        SetBack(35);
        SetTurnRight(45);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Picks the enemy most worth focusing fire on: lowest energy wins,
    /// with distance as a tie-breaker.
    /// </summary>
    private RadarContact? SelectPriorityTarget()
    {
        RadarContact? best      = null;
        double        bestScore = double.MaxValue;

        foreach (RadarContact contact in RadarMap.Values)
        {
            if (contact.IsAlly) continue;
            if (Arena.TickNumber - contact.Timestamp > StaleAfterTicks) continue;

            double dist  = State.Position.DistanceTo(contact.Position);
            double score = contact.Energy + dist * 0.1;   // wounded enemies prioritised
            if (score < bestScore)
            {
                bestScore = score;
                best      = contact;
            }
        }

        return best;
    }

    /// <summary>Leads the target and fires when the gun is aligned.</summary>
    private void AimAndFire(RadarContact target)
    {
        double dist  = State.Position.DistanceTo(target.Position);
        double power = dist < CloseRangeThreshold ? CloseFirePower : NormalFirePower;

        double bulletSpeed  = 20.0 - 3.0 * power;
        double travelTicks  = dist / bulletSpeed;
        Vector2D predicted  = new(
            target.Position.X + target.VelocityVector.X * travelTicks,
            target.Position.Y + target.VelocityVector.Y * travelTicks);

        double targetBearing = State.Position.BearingTo(predicted);
        double gunRel        = RelativeBearing(State.GunHeading, targetBearing);

        if (gunRel >= 0) SetTurnGunRight(Math.Min(gunRel, 20));
        else             SetTurnGunLeft(Math.Min(-gunRel, 20));

        if (Math.Abs(gunRel) < FireAlignmentDeg)
            SetFire(power);
    }

    /// <summary>Broadcasts the chosen target to focus the whole swarm's fire.</summary>
    private void BroadcastPriorityTarget(RadarContact target)
    {
        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type       = SwarmMessageType.TargetLocked,
            TargetName = target.Name,
            Position   = target.Position,
            Timestamp  = Arena.TickNumber
        });
    }

    /// <summary>Nudges back toward centre if the commander has drifted too far.</summary>
    private void MaintainPosition()
    {
        var    center = new Vector2D(_cx, _cy);
        double dist   = State.Position.DistanceTo(center);
        if (dist > HoldRadius)
            MoveTowardPoint(center, dist - HoldRadius * 0.5);
    }

    /// <summary>Drives directly to the arena centre (used when retreating).</summary>
    private void MoveToCenter()
    {
        var center = new Vector2D(_cx, _cy);
        MoveTowardPoint(center, State.Position.DistanceTo(center));
        SetTurnRadarRight(45);
    }

    /// <summary>Slow turning patrol when no enemy contacts are fresh.</summary>
    private void Patrol()
    {
        SetTurnRight(8);
        SetAhead(25);
    }

    private void MoveTowardPoint(Vector2D target, double distance)
    {
        double rel = RelativeBearing(State.Heading, State.Position.BearingTo(target));
        if (rel >= 0) SetTurnRight(Math.Min(rel, 10));
        else          SetTurnLeft(Math.Min(-rel, 10));
        SetAhead(distance);
    }

    private static double RelativeBearing(double fromHeading, double toAbsoluteBearing)
    {
        double rel = toAbsoluteBearing - fromHeading;
        while (rel >  180) rel -= 360;
        while (rel < -180) rel += 360;
        return rel;
    }
}

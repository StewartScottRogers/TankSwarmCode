using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;
using TankSwarmCode.SwarmTank.Interfaces;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Blue ECCM (Electronic Counter-Counter-Measures) specialist.
/// This tank's primary mission is to protect the Blue swarm's radar picture by
/// running <see cref="EcmMode.Burnthrough"/>, suppressing enemy jamming and
/// filtering out ghost echoes projected by Red's ECM Jammer.
///
/// <para><b>Operating modes</b></para>
/// <list type="bullet">
///   <item><b>Burnthrough (default)</b> — penetrates enemy jamming; filters ~70 % of
///         ghost contacts, dramatically improving swarm radar quality.</item>
///   <item><b>Jam (offensive)</b> — when energy is high (&gt; 70) and an enemy ECM
///         specialist is detected, this tank can switch to Jam to disrupt the enemy's
///         spoofing pipeline — forcing them into Jam mode instead of Spoof.</item>
///   <item><b>Off</b> — falls back to no ECM when energy is critically low.</item>
/// </list>
///
/// <para>The operator carries a light cannon (max power 2.0) for self-defence and
/// opportunistic fire on confirmed targets.  It broadcasts
/// <see cref="SwarmMessageType.EcmAlert"/> when it detects ghost contacts, warning
/// allies to activate their own burnthrough.</para>
/// </summary>
public class BlueEcmOperator : SwarmTankBase
{
    public override string Name => "ECM-Operator";

    // ── Private state ─────────────────────────────────────────────────────────

    private EcmMode _mode = EcmMode.Burnthrough;
    private int _burnthroughCooldown;   // ticks of forced burnthrough after ECM detected
    private int _offensiveJamTicks;     // ticks remaining in offensive jam window
    private bool _enemyEcmDetected;
    private int _patrolPhase;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override void OnStart()
    {
        Role = TankRole.EcmSpecialist;
        _mode = EcmMode.Burnthrough;
        _burnthroughCooldown = 0;
        _offensiveJamTicks   = 0;
        _enemyEcmDetected    = false;
        _patrolPhase         = 0;
    }

    public override void OnTick(TickEventArgs e)
    {
        // ── ECM mode selection ────────────────────────────────────────────────
        if (State.Energy < 25)
        {
            _mode = EcmMode.Off;                // save energy when near death
        }
        else if (_offensiveJamTicks > 0)
        {
            _offensiveJamTicks--;
            _mode = EcmMode.Jam;                // offensive jam window
        }
        else if (_burnthroughCooldown > 0 || State.Energy >= 35)
        {
            if (_burnthroughCooldown > 0) _burnthroughCooldown--;
            _mode = EcmMode.Burnthrough;        // default: protect swarm radar
        }
        else
        {
            _mode = EcmMode.Off;
        }

        SetEcm(_mode);

        // ── Offensive jam trigger ─────────────────────────────────────────────
        // If we have high energy and have confirmed an enemy ECM specialist, switch
        // to Jam to disrupt their spoofing operations.
        if (_enemyEcmDetected && State.Energy > 70 && _offensiveJamTicks == 0)
        {
            _offensiveJamTicks = 20;
            Broadcast(new SwarmMessage
            {
                SenderName = Name,
                Type       = SwarmMessageType.EcmAlert,
                CustomData = "OFFENSIVE JAM: suppressing enemy ECM",
                Timestamp  = Arena.TickNumber
            });
        }

        // ── Combat ────────────────────────────────────────────────────────────
        RadarContact? target = GetFreshestEnemy(20);
        if (target is not null && !target.Name.StartsWith("Ghost-"))
        {
            double dx         = target.Position.X - State.Position.X;
            double dy         = target.Position.Y - State.Position.Y;
            double distance   = Math.Sqrt(dx * dx + dy * dy);
            double absBearing = State.Position.BearingTo(target.Position);
            double relGun     = NormaliseBearing(absBearing - State.GunHeading);

            SetTurnGunRight(Math.Clamp(relGun, -20, 20));

            // Light cannon — preserve energy for ECM but fire when gun is on target
            if (Math.Abs(relGun) < 6 && State.Energy > 40)
            {
                double power = distance < 180 ? 2.0 : 1.2;
                SetFire(power);
            }
        }

        // ── Movement: figure-8 patrol ─────────────────────────────────────────
        _patrolPhase++;
        double turnBias = Math.Sin(_patrolPhase * 0.045) * 8.0;
        SetTurnRight(turnBias);
        SetAhead(45);

        // ── Radar: constant spin for full-arena coverage ──────────────────────
        SetTurnRadarRight(45);
    }

    public override void OnScannedTank(ScannedTankEventArgs e)
    {
        base.OnScannedTank(e);

        // Detect ghost contacts injected by enemy Spoof tanks
        if (e.Result.Name.StartsWith("Ghost-", StringComparison.Ordinal))
        {
            _enemyEcmDetected    = true;
            _burnthroughCooldown = Math.Max(_burnthroughCooldown, 40);

            Broadcast(new SwarmMessage
            {
                SenderName = Name,
                Type       = SwarmMessageType.EcmAlert,
                CustomData = $"ECM GHOST DETECTED near {e.Result.Position.X:F0},{e.Result.Position.Y:F0} — activating burnthrough",
                Timestamp  = Arena.TickNumber
            });
            return; // don't track ghosts in the radar map as real contacts
        }

        // Detect jamming: a confirmed enemy with ActiveEcm = Jam is visible in their TankState
        // (Our burnthrough scan succeeds and returns accurate state including ECM mode)
    }

    public override void OnSwarmMessage(SwarmMessageEventArgs e)
    {
        base.OnSwarmMessage(e);

        if (e.Message.Type == SwarmMessageType.EcmAlert)
        {
            // Ally detected ECM — ramp up our burnthrough
            _burnthroughCooldown = Math.Max(_burnthroughCooldown, 35);
            _mode = EcmMode.Burnthrough;
        }
    }

    public override void OnHitByBullet(HitByBulletEventArgs e)
    {
        // When shot, switch to Jam briefly to make it harder to re-acquire
        _offensiveJamTicks = Math.Max(_offensiveJamTicks, 12);

        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type       = SwarmMessageType.RequestBackup,
            Timestamp  = Arena.TickNumber
        });
    }

    public override void OnHitWall(HitWallEventArgs e)
    {
        SetBack(25);
        SetTurnRight(15);
        _patrolPhase += 40; // skip phase to break out of wall-hugging loop
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static double NormaliseBearing(double b)
    {
        while (b >  180) b -= 360;
        while (b < -180) b += 360;
        return b;
    }
}

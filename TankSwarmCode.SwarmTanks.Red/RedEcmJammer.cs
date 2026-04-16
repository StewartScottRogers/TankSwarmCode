using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;
using TankSwarmCode.SwarmTank.Interfaces;

namespace TankSwarmCode.SwarmTanks.Red;

/// <summary>
/// Red ECM specialist. Carries no cannon. Instead it dominates the electromagnetic
/// spectrum with two mutually reinforcing modes:
/// <list type="bullet">
///   <item><b>Spoof (default)</b> — projects two ghost-tank echoes that drift around the
///         arena, polluting enemy RadarMaps with phantom contacts and wasting their fire.</item>
///   <item><b>Jam (threat response)</b> — floods the local EM spectrum with noise.
///         Enemy radar sweeps that catch this tank suffer a high drop/corrupt rate
///         until the threat passes.</item>
/// </list>
/// The jammer orbits the centre of the arena so its ghosts project into contested
/// space.  When it takes fire it switches to Jam mode for 25 ticks, then reverts.
/// It broadcasts <see cref="SwarmMessageType.EcmAlert"/> when threatened so allies
/// know to watch for counter-ECM activity.
/// </summary>
public class RedEcmJammer : SwarmTankBase
{
    public override string Name => "ECM-Jammer";

    // ── Private state ─────────────────────────────────────────────────────────

    private EcmMode _mode = EcmMode.Spoof;
    private int _jamCooldown;       // ticks remaining in threat-response Jam mode
    private int _orbitTick;         // drives the orbit motion pattern

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override void OnStart()
    {
        Role = TankRole.EcmSpecialist;
        _mode = EcmMode.Spoof;
        _jamCooldown = 0;
        _orbitTick = 0;
    }

    public override void OnTick(TickEventArgs e)
    {
        // ── ECM mode management ───────────────────────────────────────────────
        if (_jamCooldown > 0)
        {
            _jamCooldown--;
            _mode = EcmMode.Jam;
        }
        else
        {
            _mode = EcmMode.Spoof;
        }

        // No cannon — ECM specialists don't fire
        SetEcm(_mode);

        // ── Movement: orbit the arena centre ──────────────────────────────────
        // Keep the jammer roughly central so its ghosts spread into contested space.
        _orbitTick++;
        double cx = Arena.ArenaWidth  / 2.0;
        double cy = Arena.ArenaHeight / 2.0;
        double dx = cx - State.Position.X;
        double dy = cy - State.Position.Y;
        double distToCenter = Math.Sqrt(dx * dx + dy * dy);
        double idealOrbitRadius = Math.Min(Arena.ArenaWidth, Arena.ArenaHeight) * 0.22;

        if (distToCenter > idealOrbitRadius + 40)
        {
            // Approach centre orbit band
            double bearingToCenter = State.Position.BearingTo(new Vector2D(cx, cy));
            double relBearing = NormaliseBearing(bearingToCenter - State.Heading);
            SetTurnRight(Math.Clamp(relBearing, -10, 10));
            SetAhead(60);
        }
        else
        {
            // Orbit: strafe in a circle around centre
            SetTurnRight(8);
            SetAhead(40);
        }

        // ── Radar: constant full spin to keep contacts fresh ──────────────────
        SetTurnRadarRight(45);
    }

    public override void OnScannedTank(ScannedTankEventArgs e)
    {
        base.OnScannedTank(e);

        if (e.Result.SwarmId != SwarmId)
        {
            // Share enemy position with allies (even though we don't fire, intel matters)
            Broadcast(new SwarmMessage
            {
                SenderName = Name,
                Type       = SwarmMessageType.EnemySpotted,
                TargetName = e.Result.Name,
                Position   = e.Result.Position,
                Timestamp  = Arena.TickNumber
            });
        }
    }

    public override void OnHitByBullet(HitByBulletEventArgs e)
    {
        // Switch to Jam mode: make it hard for the shooter to re-acquire us
        _jamCooldown = 25;
        _mode = EcmMode.Jam;

        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type       = SwarmMessageType.EcmAlert,
            CustomData = $"JAMMER ENGAGED: taking fire bearing {e.Bearing:F0}",
            Timestamp  = Arena.TickNumber
        });
    }

    public override void OnHitWall(HitWallEventArgs e)
    {
        // Reverse direction to avoid hugging walls
        SetBack(30);
        SetTurnRight(20);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static double NormaliseBearing(double b)
    {
        while (b >  180) b -= 360;
        while (b < -180) b += 360;
        return b;
    }
}

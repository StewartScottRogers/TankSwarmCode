using TankSwarmCode.Arena.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.Arena;

/// <summary>
/// Robocode-inspired physics engine. Drives tank movement, radar, bullets,
/// collisions and swarm messaging each tick.
/// </summary>
public sealed class ArenaEngine : IArena
{
    // ── Runtime state ─────────────────────────────────────────────────────────

    internal readonly List<TankRuntimeState> RuntimeTanks = [];
    internal readonly List<BulletRuntimeState> RuntimeBullets = [];

    private readonly ArenaContext _context;
    private readonly Random _rng = new();

    // ── IArena ────────────────────────────────────────────────────────────────

    public double Width { get; private set; }
    public double Height { get; private set; }
    public bool IsRunning { get; private set; }
    public long TickNumber { get; private set; }

    /// <summary>True once <see cref="Start"/> has been called (and until <see cref="Reset"/>).</summary>
    public bool HasStarted { get; private set; }
    public int LivingTankCount => RuntimeTanks.Count(t => t.IsAlive);

    public IReadOnlyList<ISwarmTank> Tanks =>
        RuntimeTanks.Select(t => t.Tank).ToList();

    public IReadOnlyList<BulletState> Bullets =>
        RuntimeBullets.Where(b => b.Active).Select(b => b.ToBulletState()).ToList();

    public event EventHandler<TickEventArgs>? TickCompleted;
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;

    // ── Construction ──────────────────────────────────────────────────────────

    public ArenaEngine(double width = 800, double height = 600)
    {
        Width = width;
        Height = height;
        _context = new ArenaContext(this);
    }

    // ── Control ───────────────────────────────────────────────────────────────

    public void AddTank(ISwarmTank tank)
    {
        ArgumentNullException.ThrowIfNull(tank);
        RuntimeTanks.Add(new TankRuntimeState(tank));
    }

    public void Resize(double width, double height)
    {
        Width = Math.Max(100, width);
        Height = Math.Max(100, height);

        if (!IsRunning) return;

        // Re-notify all living tanks so they can recalculate dimension-dependent
        // state (waypoints, corners, patrol routes, etc.)
        foreach (TankRuntimeState rts in RuntimeTanks.Where(t => t.IsAlive))
            SafeCall(() => rts.Tank.OnStart());
    }

    public void Start()
    {
        if (RuntimeTanks.Count == 0)
            throw new InvalidOperationException("Add at least one tank before starting.");

        SpawnTanks();

        foreach (TankRuntimeState rts in RuntimeTanks)
        {
            rts.Tank.Initialize(rts.Tank.Name, rts.Tank.SwarmId, rts.Tank.Role, _context);
            rts.SyncToTank();
            try { rts.Tank.OnStart(); } catch { /* prevent bad AI from killing the engine */ }
        }

        HasStarted = true;
        IsRunning = true;
    }

    /// <summary>Resumes a paused simulation without re-spawning tanks.</summary>
    public void Resume()
    {
        if (!HasStarted)
            throw new InvalidOperationException("Call Start() before Resume().");

        IsRunning = true;
    }

    /// <summary>
    /// Advances simulation by exactly one tick regardless of running state.
    /// No-op if <see cref="Start"/> has never been called.
    /// </summary>
    public void StepOnce()
    {
        if (!HasStarted) return;

        bool wasRunning = IsRunning;
        IsRunning = true;
        Tick();

        // Restore pause only if we forced it on and CheckRoundEnd didn't already stop it.
        if (!wasRunning && IsRunning)
            IsRunning = false;
    }

    public void Stop() => IsRunning = false;

    public void Reset()
    {
        IsRunning = false;
        HasStarted = false;
        TickNumber = 0;
        RuntimeTanks.Clear();
        RuntimeBullets.Clear();
    }

    // ── Main tick ─────────────────────────────────────────────────────────────

    public void Tick()
    {
        if (!IsRunning) return;

        TickNumber++;
        var tickArgs = new TickEventArgs(TickNumber, LivingTankCount);

        // 1. Call OnTick for every living tank
        foreach (TankRuntimeState rts in RuntimeTanks.Where(t => t.IsAlive))
            SafeCall(() => rts.Tank.OnTick(tickArgs));

        // 2. Flush commands and apply movement + firing
        List<(TankRuntimeState Rts, TankCommand Cmd)> commands =
            RuntimeTanks
                .Where(t => t.IsAlive)
                .Select(t => (t, t.Tank.FlushCommand()))
                .ToList();

        foreach ((TankRuntimeState rts, TankCommand cmd) in commands)
            ApplyMovement(rts, cmd);

        foreach ((TankRuntimeState rts, TankCommand cmd) in commands)
            ApplyFiring(rts, cmd);

        // 3. Move bullets
        foreach (BulletRuntimeState b in RuntimeBullets.Where(b => b.Active))
            MoveBullet(b);

        // 4. Bullet-tank collisions
        CheckBulletTankCollisions();

        // 5. Tank-tank collisions
        CheckTankTankCollisions();

        // 6. Radar scans
        foreach ((TankRuntimeState rts, TankCommand cmd) in commands)
            ProcessRadarScan(rts);

        // 7. Deliver swarm messages
        foreach ((TankRuntimeState rts, TankCommand cmd) in commands)
            DeliverSwarmMessages(rts, cmd);

        // 8. Push updated state records back to tanks
        foreach (TankRuntimeState rts in RuntimeTanks)
        {
            rts.SyncToTank();
            rts.Tank.UpdateState(rts.ToTankState());
        }

        // 9. Remove spent bullets
        RuntimeBullets.RemoveAll(b => !b.Active);

        // 10. Check round-end condition
        CheckRoundEnd();

        TickCompleted?.Invoke(this, tickArgs);
    }

    // ── Physics helpers ───────────────────────────────────────────────────────

    private void ApplyMovement(TankRuntimeState rts, TankCommand cmd)
    {
        // --- Body turn (rate limited by current speed) ---
        double maxTurn = ArenaConstants.MaxTurnRate
                       - ArenaConstants.VelocityTurnPenalty * Math.Abs(rts.Velocity);
        double bodyTurn = Math.Clamp(cmd.BodyTurnDegrees, -maxTurn, maxTurn);
        rts.Heading = NormaliseAngle(rts.Heading + bodyTurn);

        // Gun and radar rotate with the body
        rts.GunHeading = NormaliseAngle(rts.GunHeading + bodyTurn);
        rts.RadarHeading = NormaliseAngle(rts.RadarHeading + bodyTurn);

        // --- Gun turn (independent, max 20°/tick) ---
        double gunTurn = Math.Clamp(cmd.GunTurnDegrees,
                                    -ArenaConstants.MaxGunTurnRate,
                                     ArenaConstants.MaxGunTurnRate);
        rts.GunHeading = NormaliseAngle(rts.GunHeading + gunTurn);

        // --- Radar turn (independent, max 45°/tick) ---
        rts.PrevRadarHeading = rts.RadarHeading;
        double radarTurn = Math.Clamp(cmd.RadarTurnDegrees,
                                      -ArenaConstants.MaxRadarTurnRate,
                                       ArenaConstants.MaxRadarTurnRate);
        rts.RadarHeading = NormaliseAngle(rts.RadarHeading + radarTurn);

        // --- Velocity (Robocode acceleration model) ---
        double desiredSpeed = Math.Sign(cmd.MoveDistance)
                            * Math.Min(Math.Abs(cmd.MoveDistance), ArenaConstants.MaxVelocity);
        if (desiredSpeed > rts.Velocity)
            rts.Velocity = Math.Min(rts.Velocity + ArenaConstants.Acceleration, desiredSpeed);
        else if (desiredSpeed < rts.Velocity)
            rts.Velocity = Math.Max(rts.Velocity - ArenaConstants.Deceleration, desiredSpeed);

        rts.Velocity = Math.Clamp(rts.Velocity,
                                  -ArenaConstants.MaxVelocity,
                                   ArenaConstants.MaxVelocity);

        // --- Position ---
        double headingRad = rts.Heading * (Math.PI / 180.0);
        double nx = rts.X + Math.Sin(headingRad) * rts.Velocity;
        double ny = rts.Y - Math.Cos(headingRad) * rts.Velocity; // screen Y flipped

        // --- Wall collision ---
        double half = ArenaConstants.TankHalfSize;
        bool hitWall = false;

        if (nx - half < 0) { nx = half; hitWall = true; }
        else if (nx + half > Width) { nx = Width - half; hitWall = true; }

        if (ny - half < 0) { ny = half; hitWall = true; }
        else if (ny + half > Height) { ny = Height - half; hitWall = true; }

        if (hitWall)
        {
            double wallDamage = Math.Max(Math.Abs(rts.Velocity) * ArenaConstants.WallDamageFactor
                                       - ArenaConstants.WallDamageThreshold, 0);
            rts.Velocity = 0;
            rts.Energy -= wallDamage;

            double wallBearing = RelativeBearing(rts.Heading, rts.ToTankState().Position.BearingTo(new Vector2D(nx, ny)));
            SafeCall(() => rts.Tank.OnHitWall(new HitWallEventArgs(wallBearing)));
            CheckDeath(rts);
        }

        rts.X = nx;
        rts.Y = ny;
    }

    private void ApplyFiring(TankRuntimeState rts, TankCommand cmd)
    {
        if (cmd.FirePower <= 0) return;
        double power = Math.Clamp(cmd.FirePower, ArenaConstants.BulletMinPower, ArenaConstants.BulletMaxPower);
        if (rts.Energy < power) return; // not enough energy

        rts.Energy -= power;

        RuntimeBullets.Add(new BulletRuntimeState(
            ownerId: rts.Tank.Name,
            x: rts.X,
            y: rts.Y,
            heading: rts.GunHeading,
            power: power));
    }

    private static void MoveBullet(BulletRuntimeState b)
    {
        double headingRad = b.Heading * (Math.PI / 180.0);
        b.X += Math.Sin(headingRad) * b.Speed;
        b.Y -= Math.Cos(headingRad) * b.Speed;
    }

    private void CheckBulletTankCollisions()
    {
        foreach (BulletRuntimeState b in RuntimeBullets.Where(b => b.Active))
        {
            foreach (TankRuntimeState rts in RuntimeTanks.Where(t => t.IsAlive))
            {
                if (rts.Tank.Name == b.OwnerId) continue; // own bullet

                double dx = b.X - rts.X;
                double dy = b.Y - rts.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist > ArenaConstants.TankHalfSize + ArenaConstants.BulletRadius)
                    continue;

                // Hit!
                b.Active = false;
                BulletState bs = b.ToBulletState();

                rts.Energy -= bs.Damage;
                double bearing = RelativeBearing(rts.Heading, new Vector2D(rts.X, rts.Y).BearingTo(new Vector2D(b.X, b.Y)));
                SafeCall(() => rts.Tank.OnHitByBullet(new HitByBulletEventArgs(bs, bearing)));

                // Notify shooter
                TankRuntimeState? shooter = RuntimeTanks.FirstOrDefault(t => t.Tank.Name == b.OwnerId);
                if (shooter is not null)
                {
                    shooter.Energy += bs.EnergyReturn;
                    SafeCall(() => shooter.Tank.OnBulletHit(new BulletHitEventArgs(bs, rts.ToTankState())));
                }

                CheckDeath(rts);
                break;
            }

            // Remove bullet if out of bounds
            if (b.X < 0 || b.X > Width || b.Y < 0 || b.Y > Height)
                b.Active = false;
        }
    }

    private void CheckTankTankCollisions()
    {
        for (int i = 0; i < RuntimeTanks.Count; i++)
        {
            if (!RuntimeTanks[i].IsAlive) continue;
            for (int j = i + 1; j < RuntimeTanks.Count; j++)
            {
                if (!RuntimeTanks[j].IsAlive) continue;

                TankRuntimeState a = RuntimeTanks[i];
                TankRuntimeState b = RuntimeTanks[j];
                double dx = a.X - b.X;
                double dy = a.Y - b.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist > ArenaConstants.TankHalfSize * 2) continue;

                a.Energy -= ArenaConstants.TankCollisionDamage;
                b.Energy -= ArenaConstants.TankCollisionDamage;
                a.Velocity = 0;
                b.Velocity = 0;

                double bearingAtoB = RelativeBearing(a.Heading, new Vector2D(a.X, a.Y).BearingTo(new Vector2D(b.X, b.Y)));
                double bearingBtoA = RelativeBearing(b.Heading, new Vector2D(b.X, b.Y).BearingTo(new Vector2D(a.X, a.Y)));

                SafeCall(() => a.Tank.OnHitTank(new HitTankEventArgs(b.ToTankState(), bearingAtoB)));
                SafeCall(() => b.Tank.OnHitTank(new HitTankEventArgs(a.ToTankState(), bearingBtoA)));

                CheckDeath(a);
                CheckDeath(b);
            }
        }
    }

    private void ProcessRadarScan(TankRuntimeState scanner)
    {
        // Radar sweeps the arc between PrevRadarHeading and RadarHeading
        double sweepStart = scanner.PrevRadarHeading;
        double sweepEnd = scanner.RadarHeading;

        foreach (TankRuntimeState target in RuntimeTanks.Where(t => t.IsAlive && t != scanner))
        {
            double bearing = new Vector2D(scanner.X, scanner.Y)
                                 .BearingTo(new Vector2D(target.X, target.Y));

            if (!AngleInSweep(bearing, sweepStart, sweepEnd)) continue;

            double distance = Math.Sqrt(Math.Pow(target.X - scanner.X, 2)
                                      + Math.Pow(target.Y - scanner.Y, 2));

            ScanResult result = new()
            {
                Name = target.Tank.Name,
                SwarmId = target.Tank.SwarmId,
                Bearing = RelativeBearing(scanner.Heading, bearing),
                Distance = distance,
                Heading = target.Heading,
                Velocity = target.Velocity,
                Energy = target.Energy,
                Position = new Vector2D(target.X, target.Y)
            };

            SafeCall(() => scanner.Tank.OnScannedTank(new ScannedTankEventArgs(result)));
        }
    }

    private void DeliverSwarmMessages(TankRuntimeState sender, TankCommand cmd)
    {
        if (cmd.BroadcastMessages.Count == 0) return;

        List<TankRuntimeState> allies = RuntimeTanks
            .Where(t => t.IsAlive && t != sender && t.Tank.SwarmId == sender.Tank.SwarmId)
            .ToList();

        foreach (SwarmMessage msg in cmd.BroadcastMessages)
            foreach (TankRuntimeState ally in allies)
                SafeCall(() => ally.Tank.DeliverSwarmMessage(msg));
    }

    private void CheckDeath(TankRuntimeState rts)
    {
        if (rts.Energy > 0 || !rts.IsAlive) return;
        rts.Energy = 0;
        rts.IsAlive = false;
        SafeCall(() => rts.Tank.OnDeath());
    }

    private void CheckRoundEnd()
    {
        IEnumerable<int> livingSwarms = RuntimeTanks
            .Where(t => t.IsAlive)
            .Select(t => t.Tank.SwarmId)
            .Distinct();

        if (livingSwarms.Count() > 1) return;

        // Round over — notify all survivors
        IsRunning = false;
        int? winnerId = livingSwarms.Cast<int?>().FirstOrDefault();

        foreach (TankRuntimeState rts in RuntimeTanks.Where(t => t.IsAlive))
        {
            bool won = rts.Tank.SwarmId == winnerId;
            SafeCall(() => rts.Tank.OnRoundEnded(new RoundEndedEventArgs(won, TickNumber)));
        }

        RoundEnded?.Invoke(this, new RoundEndedEventArgs(true, TickNumber));
    }

    // ── Spawning ──────────────────────────────────────────────────────────────

    private void SpawnTanks()
    {
        double margin = ArenaConstants.TankHalfSize * 4;
        foreach (TankRuntimeState rts in RuntimeTanks)
        {
            rts.X = _rng.NextDouble() * (Width - margin * 2) + margin;
            rts.Y = _rng.NextDouble() * (Height - margin * 2) + margin;
            rts.Heading = _rng.NextDouble() * 360;
            rts.GunHeading = rts.Heading;
            rts.RadarHeading = rts.Heading;
            rts.PrevRadarHeading = rts.Heading;
            rts.Velocity = 0;
            rts.Energy = ArenaConstants.TankStartEnergy;
            rts.IsAlive = true;
        }
    }

    // ── Geometry helpers ──────────────────────────────────────────────────────

    private static double NormaliseAngle(double degrees) => ((degrees % 360) + 360) % 360;

    private static double RelativeBearing(double tankHeading, double absoluteBearing)
    {
        double rel = absoluteBearing - tankHeading;
        while (rel > 180) rel -= 360;
        while (rel < -180) rel += 360;
        return rel;
    }

    /// <summary>
    /// Returns true if <paramref name="angle"/> falls within the directed radar sweep arc
    /// from <paramref name="sweepStart"/> to <paramref name="sweepEnd"/>.
    /// <para>
    /// Direction matters: a sweep from 350° to 10° is a clockwise 20° arc, not a
    /// counter-clockwise 340° arc. The signed delta <c>sweepEnd − sweepStart</c> (normalised
    /// to (−180, +180]) determines direction; the test then checks whether <paramref name="angle"/>
    /// lies within that signed span rather than the complementary sector.
    /// </para>
    /// </summary>
    private static bool AngleInSweep(double angle, double sweepStart, double sweepEnd)
    {
        angle      = NormaliseAngle(angle);
        sweepStart = NormaliseAngle(sweepStart);
        sweepEnd   = NormaliseAngle(sweepEnd);

        // Signed delta: positive = CW, negative = CCW. Clamped to (−180, +180].
        double delta = sweepEnd - sweepStart;
        while (delta >  180) delta -= 360;
        while (delta < -180) delta += 360;

        if (Math.Abs(delta) < 0.001) return false;

        // Angular distance from sweepStart to angle along the same direction as delta.
        double dist = angle - sweepStart;
        while (dist >  180) dist -= 360;
        while (dist < -180) dist += 360;

        // The angle is inside the swept sector when it is between 0 and delta (same sign).
        return delta >= 0 ? (dist >= 0 && dist <= delta) : (dist <= 0 && dist >= delta);
    }

    private static void SafeCall(Action action)
    {
        try { action(); }
        catch { /* Isolate bad AI code from crashing the engine */ }
    }
}

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
    private readonly List<BuildingDefinition> _buildings = [];

    private readonly ArenaContext _context;
    private readonly Random _rng = new();

    // Cached snapshots rebuilt once per tick — avoids per-access List allocations
    private IReadOnlyList<ISwarmTank> _tanksSnapshot = [];
    private IReadOnlyList<BulletState> _bulletsSnapshot = [];
    private int _livingTankCount;

    // ── IArena ────────────────────────────────────────────────────────────────

    public double Width { get; private set; }
    public double Height { get; private set; }
    public bool IsRunning { get; private set; }
    public long TickNumber { get; private set; }

    /// <summary>True once <see cref="Start"/> has been called (and until <see cref="Reset"/>).</summary>
    public bool HasStarted { get; private set; }
    public int LivingTankCount => _livingTankCount;

    public IReadOnlyList<ISwarmTank> Tanks => _tanksSnapshot;

    public IReadOnlyList<BulletState> Bullets => _bulletsSnapshot;

    public IReadOnlyList<BuildingDefinition> Buildings => _buildings;

    /// <summary>
    /// Ghost echo positions projected by all currently-spoofing tanks, rebuilt each tick.
    /// Each entry is (arena position, owner swarm id) for use by the renderer.
    /// </summary>
    public IReadOnlyList<(Vector2D Position, int OwnerSwarmId)> ActiveGhostEchoes { get; private set; } = [];

    /// <summary>
    /// Impact positions of non-lethal bullet ricochets this tick, for the renderer to draw
    /// a brief flash ring.  Replaced each tick.
    /// </summary>
    public IReadOnlyList<Vector2D> RicochetFlashes { get; private set; } = [];

    public event EventHandler<TickEventArgs>? TickCompleted;
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;

    /// <summary>
    /// Fired once per non-RadarShare broadcast, before delivery to recipients.
    /// Parameters: the message and the sender's SwarmId.
    /// Internal — only <see cref="ArenaUserControl"/> subscribes.
    /// </summary>
    public event Action<SwarmMessage, int>? SwarmMessageBroadcast;

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

        // Keep snapshot current so TankCount is accurate before the first Tick() runs.
        var snap = new ISwarmTank[RuntimeTanks.Count];
        for (int i = 0; i < RuntimeTanks.Count; i++) snap[i] = RuntimeTanks[i].Tank;
        _tanksSnapshot = snap;
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

        GenerateBuildings();
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
        _buildings.Clear();
    }

    // ── Main tick ─────────────────────────────────────────────────────────────

    public void Tick()
    {
        if (!IsRunning) return;

        TickNumber++;

        // Pre-compute living tanks once — avoids repeated Where() passes each phase.
        List<TankRuntimeState> livingTanks = [];
        for (int i = 0; i < RuntimeTanks.Count; i++)
            if (RuntimeTanks[i].IsAlive) livingTanks.Add(RuntimeTanks[i]);

        var tickArgs = new TickEventArgs(TickNumber, livingTanks.Count);

        // 1. Call OnTick for every living tank — parallel (each tank writes to its own command buffer only)
        Parallel.ForEach(livingTanks, rts => SafeCall(() => rts.Tank.OnTick(tickArgs)));

        // 2. Flush commands (single-threaded — ordering of FlushCommand matters)
        List<(TankRuntimeState Rts, TankCommand Cmd)> commands =
            livingTanks
                .Select(t => (t, t.Tank.FlushCommand()))
                .ToList();

        // 3. Apply movement (sequential — wall events call back into tank; cheap per-tank)
        foreach ((TankRuntimeState rts, TankCommand cmd) in commands)
            ApplyMovement(rts, cmd);

        // 4. Apply firing (sequential — writes to shared RuntimeBullets list)
        foreach ((TankRuntimeState rts, TankCommand cmd) in commands)
            ApplyFiring(rts, cmd);

        // 4.5. Apply ECM — drains energy, sets ActiveEcm, updates ghost positions (sequential)
        foreach ((TankRuntimeState rts, TankCommand cmd) in commands)
            ApplyEcm(rts, cmd);

        // 5. Move bullets — parallel (each bullet is completely independent)
        Parallel.ForEach(RuntimeBullets, b => { if (b.Active) MoveBullet(b); });

        // 6. Bullet-tank collisions (sequential — mutates b.Active and tank energy)
        var ricochetPositions = CheckBulletTankCollisions();

        // 7. Tank-tank collisions (sequential — mutates both tanks)
        CheckTankTankCollisions();

        // 8. Radar scans — parallel (each scanner writes only to its own RadarMap/command buffer)
        Parallel.ForEach(commands, pair => ProcessRadarScan(pair.Rts));

        // 9. Deliver swarm messages (sequential — writes into ally RadarMaps)
        foreach ((TankRuntimeState rts, TankCommand cmd) in commands)
            DeliverSwarmMessages(rts, cmd);

        // 10. Push updated state records back to tanks — parallel (per-tank, no cross-writes)
        Parallel.ForEach(RuntimeTanks, rts =>
        {
            rts.SyncToTank();
            rts.Tank.UpdateState(rts.ToTankState());
        });

        // 11. Remove spent bullets
        RuntimeBullets.RemoveAll(b => !b.Active);

        // 11.5. Rebuild ghost echo list for the renderer
        var ghosts = new List<(Vector2D, int)>();
        foreach (TankRuntimeState rts in RuntimeTanks)
        {
            if (!rts.IsAlive || rts.ActiveEcm != EcmMode.Spoof) continue;
            foreach ((double gx, double gy, double _, double _) in rts.GhostPositions)
                ghosts.Add((new Vector2D(gx, gy), rts.Tank.SwarmId));
        }
        ActiveGhostEchoes = ghosts;

        // 11.6. Publish ricochet flash positions gathered during this tick's collision pass
        RicochetFlashes = ricochetPositions;

        // 12. Rebuild cached snapshots once — avoids allocations in properties accessed by UI
        var tanksSnap = new ISwarmTank[RuntimeTanks.Count];
        for (int i = 0; i < RuntimeTanks.Count; i++) tanksSnap[i] = RuntimeTanks[i].Tank;
        _tanksSnapshot = tanksSnap;

        var bulletsSnap = new BulletState[RuntimeBullets.Count];
        for (int i = 0; i < RuntimeBullets.Count; i++) bulletsSnap[i] = RuntimeBullets[i].ToBulletState();
        _bulletsSnapshot = bulletsSnap;

        int alive = 0;
        for (int i = 0; i < RuntimeTanks.Count; i++)
            if (RuntimeTanks[i].IsAlive) alive++;
        _livingTankCount = alive;

        // 13. Check round-end condition
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

        // --- Building collision ---
        PushTankFromBuildings(rts);
    }

    /// <summary>
    /// Pushes <paramref name="rts"/> out of any building it currently overlaps.
    /// Uses the tank body's half-diagonal as the clearance radius so that square
    /// body corners never visually penetrate an building face.
    /// Safe to call after any position change (movement, tank-tank separation, etc.).
    /// </summary>
    private void PushTankFromBuildings(TankRuntimeState rts)
    {
        const double obsHalf = ArenaConstants.TankHalfSize * 1.415 + 0.5; // ≈ TankHalfSize*√2 + margin
        double half = ArenaConstants.TankHalfSize;

        foreach (BuildingDefinition obs in _buildings)
        {
            double cx = Math.Clamp(rts.X, obs.X, obs.X + obs.Width);
            double cy = Math.Clamp(rts.Y, obs.Y, obs.Y + obs.Height);
            double cdx = rts.X - cx;
            double cdy = rts.Y - cy;
            double distSq = cdx * cdx + cdy * cdy;

            if (distSq >= obsHalf * obsHalf) continue; // no overlap

            rts.Velocity = 0;

            if (distSq < 0.0001) // center is inside — push on minimum axis
            {
                double oL = rts.X - obs.X;
                double oR = obs.X + obs.Width  - rts.X;
                double oT = rts.Y - obs.Y;
                double oB = obs.Y + obs.Height - rts.Y;
                double minOv = Math.Min(Math.Min(oL, oR), Math.Min(oT, oB));
                if      (minOv == oL) rts.X = obs.X - obsHalf;
                else if (minOv == oR) rts.X = obs.X + obs.Width  + obsHalf;
                else if (minOv == oT) rts.Y = obs.Y - obsHalf;
                else                  rts.Y = obs.Y + obs.Height + obsHalf;
            }
            else
            {
                double dist = Math.Sqrt(distSq);
                rts.X += (cdx / dist) * (obsHalf - dist);
                rts.Y += (cdy / dist) * (obsHalf - dist);
            }

            rts.X = Math.Clamp(rts.X, half, Width  - half);
            rts.Y = Math.Clamp(rts.Y, half, Height - half);
        }
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
        if (b.IsDeflected)
        {
            b.DeflectedSpeed *= 0.72;          // friction: loses ~28 % speed per tick
            if (b.DeflectedSpeed < 0.4)
            {
                b.Active = false;              // close enough to stopped — dissipate
                return;
            }
            double dRad = b.DeflectedHeading * (Math.PI / 180.0);
            b.X += Math.Sin(dRad) * b.DeflectedSpeed;
            b.Y -= Math.Cos(dRad) * b.DeflectedSpeed;
        }
        else
        {
            double headingRad = b.Heading * (Math.PI / 180.0);
            b.X += Math.Sin(headingRad) * b.Speed;
            b.Y -= Math.Cos(headingRad) * b.Speed;
        }
    }

    private List<Vector2D> CheckBulletTankCollisions()
    {
        var ricochets = new List<Vector2D>();

        foreach (BulletRuntimeState b in RuntimeBullets.Where(b => b.Active))
        {
            // Deflected bullets skip all tank collisions — they are pure visual artefacts.
            if (b.IsDeflected) goto CheckBounds;

            foreach (TankRuntimeState rts in RuntimeTanks.Where(t => t.IsAlive))
            {
                if (rts.Tank.Name == b.OwnerId) continue; // own bullet

                double dx = b.X - rts.X;
                double dy = b.Y - rts.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist > ArenaConstants.TankHalfSize + ArenaConstants.BulletRadius)
                    continue;

                // Hit!
                BulletState bs = b.ToBulletState();
                bool willKill = (rts.Energy - bs.Damage) <= 0;

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

                if (willKill)
                {
                    // Lethal hit — bullet disappears immediately
                    b.Active = false;
                }
                else
                {
                    // Non-lethal hit — deflect the bullet off the tank surface.
                    // Bounce direction: reflect heading through the impact normal (approx.
                    // opposite of the bullet's incoming direction, with a small random spread).
                    double bounceBase = (b.Heading + 180.0) % 360.0;
                    double spread     = (_rng.NextDouble() - 0.5) * 50.0; // ±25°
                    b.IsDeflected        = true;
                    b.DeflectedHeading   = (bounceBase + spread + 360.0) % 360.0;
                    b.DeflectedSpeed     = b.Speed * 0.40;   // 40 % of original speed
                    b.DeflectedFromTank  = rts.Tank.Name;
                    ricochets.Add(new Vector2D(b.X, b.Y));
                }
                break;
            }

            CheckBounds:

            // Remove bullet if out of bounds
            if (b.X < 0 || b.X > Width || b.Y < 0 || b.Y > Height)
                b.Active = false;

            // Remove bullet if it hit an building
            if (b.Active)
            {
                foreach (BuildingDefinition obs in _buildings)
                {
                    if (b.X >= obs.X && b.X <= obs.X + obs.Width &&
                        b.Y >= obs.Y && b.Y <= obs.Y + obs.Height)
                    {
                        b.Active = false;
                        break;
                    }
                }
            }
        }
        return ricochets;
    }

    private void CheckTankTankCollisions()
    {
        // ── Live-vs-live collisions ───────────────────────────────────────────
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
                double minDist = ArenaConstants.TankHalfSize * 2;

                if (dist > minDist) continue;

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

                // Push the two tanks apart so they do not occupy the same arena space.
                // If they are exactly co-located choose an arbitrary separation axis.
                double sepDist = dist < 0.001 ? minDist : dist;
                double nx = dx / sepDist;
                double ny = dy / sepDist;
                double overlap = minDist - dist;
                double push = overlap / 2.0 + 0.5; // half each plus a tiny margin

                if (a.IsAlive)
                {
                    a.X = Math.Clamp(a.X + nx * push, ArenaConstants.TankHalfSize, Width - ArenaConstants.TankHalfSize);
                    a.Y = Math.Clamp(a.Y + ny * push, ArenaConstants.TankHalfSize, Height - ArenaConstants.TankHalfSize);
                    PushTankFromBuildings(a); // tank-tank push may have sent a into an building
                }

                if (b.IsAlive)
                {
                    b.X = Math.Clamp(b.X - nx * push, ArenaConstants.TankHalfSize, Width - ArenaConstants.TankHalfSize);
                    b.Y = Math.Clamp(b.Y - ny * push, ArenaConstants.TankHalfSize, Height - ArenaConstants.TankHalfSize);
                    PushTankFromBuildings(b);
                }
            }
        }

        // ── Live-vs-hulk collisions ───────────────────────────────────────────
        // Destroyed tanks leave an impassable burning hulk. Living tanks are pushed
        // away from hulks; hulks themselves never move.
        for (int i = 0; i < RuntimeTanks.Count; i++)
        {
            if (!RuntimeTanks[i].IsAlive) continue;

            TankRuntimeState live = RuntimeTanks[i];

            for (int j = 0; j < RuntimeTanks.Count; j++)
            {
                if (RuntimeTanks[j].IsAlive) continue; // skip living tanks

                TankRuntimeState hulk = RuntimeTanks[j];
                double dx = live.X - hulk.X;
                double dy = live.Y - hulk.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                double minDist = ArenaConstants.TankHalfSize * 2;

                if (dist > minDist) continue;

                live.Velocity = 0;

                // Push the living tank entirely out of the hulk — hulk does not move.
                double sepDist = dist < 0.001 ? minDist : dist;
                double nx = dx / sepDist;
                double ny = dy / sepDist;
                double overlap = minDist - dist;

                live.X = Math.Clamp(live.X + nx * (overlap + 0.5), ArenaConstants.TankHalfSize, Width - ArenaConstants.TankHalfSize);
                live.Y = Math.Clamp(live.Y + ny * (overlap + 0.5), ArenaConstants.TankHalfSize, Height - ArenaConstants.TankHalfSize);
                PushTankFromBuildings(live);
            }
        }
    }

    private void ProcessRadarScan(TankRuntimeState scanner)
    {
        // Radar sweeps the arc between PrevRadarHeading and RadarHeading
        double sweepStart = scanner.PrevRadarHeading;
        double sweepEnd = scanner.RadarHeading;
        bool scannerBurnthrough = scanner.ActiveEcm == EcmMode.Burnthrough;

        foreach (TankRuntimeState target in RuntimeTanks.Where(t => t.IsAlive && t != scanner))
        {
            double bearing = new Vector2D(scanner.X, scanner.Y)
                                 .BearingTo(new Vector2D(target.X, target.Y));

            if (!AngleInSweep(bearing, sweepStart, sweepEnd)) continue;

            // Check line-of-sight — buildings block radar
            bool losBlocked = false;
            foreach (BuildingDefinition obs in _buildings)
            {
                if (SegmentIntersectsRect(scanner.X, scanner.Y, target.X, target.Y,
                                          obs.X, obs.Y, obs.Width, obs.Height))
                {
                    losBlocked = true;
                    break;
                }
            }
            if (losBlocked) continue;

            double distance = Math.Sqrt(Math.Pow(target.X - scanner.X, 2)
                                      + Math.Pow(target.Y - scanner.Y, 2));

            ScanResult result = new()
            {
                Name     = target.Tank.Name,
                SwarmId  = target.Tank.SwarmId,
                Bearing  = RelativeBearing(scanner.Heading, bearing),
                Distance = distance,
                Heading  = target.Heading,
                Velocity = target.Velocity,
                Energy   = target.Energy,
                Position = new Vector2D(target.X, target.Y)
            };

            // ── ECM: Jam check ──────────────────────────────────────────────
            if (target.ActiveEcm == EcmMode.Jam)
            {
                double dropChance    = scannerBurnthrough ? ArenaConstants.EcmBurnthroughDropChance    : ArenaConstants.EcmJamDropChance;
                double corruptChance = scannerBurnthrough ? ArenaConstants.EcmBurnthroughCorruptChance : ArenaConstants.EcmJamCorruptChance;

                double roll = _rng.NextDouble();
                if (roll < dropChance)
                    continue; // Scan dropped — jammer not detected at all

                if (roll < dropChance + corruptChance)
                {
                    // Corrupted scan data — wrong position, heading, velocity, energy
                    result = result with
                    {
                        Position = new Vector2D(
                            Math.Clamp(result.Position.X + (_rng.NextDouble() - 0.5) * 160, 0, Width),
                            Math.Clamp(result.Position.Y + (_rng.NextDouble() - 0.5) * 160, 0, Height)),
                        Heading  = _rng.NextDouble() * 360,
                        Velocity = (_rng.NextDouble() * 2 - 1) * ArenaConstants.MaxVelocity,
                        Energy   = _rng.NextDouble() * ArenaConstants.TankStartEnergy
                    };
                }
                // else: scan succeeds normally despite jamming
            }

            SafeCall(() => scanner.Tank.OnScannedTank(new ScannedTankEventArgs(result)));
        }

        // ── ECM: Spoof ghost injection ──────────────────────────────────────
        // For each enemy tank running Spoof, check if any of its ghost positions
        // fall within this scanner's sweep arc and inject fake OnScannedTank events.
        foreach (TankRuntimeState spoofer in RuntimeTanks)
        {
            if (!spoofer.IsAlive) continue;
            if (spoofer == scanner) continue;
            if (spoofer.Tank.SwarmId == scanner.Tank.SwarmId) continue; // allies don't spoof allies
            if (spoofer.ActiveEcm != EcmMode.Spoof) continue;

            foreach ((double gx, double gy, double gh, double gv) in spoofer.GhostPositions)
            {
                double ghostBearing = new Vector2D(scanner.X, scanner.Y)
                                          .BearingTo(new Vector2D(gx, gy));
                if (!AngleInSweep(ghostBearing, sweepStart, sweepEnd)) continue;

                // Check line-of-sight for the ghost position
                bool ghostLosBlocked = false;
                foreach (BuildingDefinition obs in _buildings)
                {
                    if (SegmentIntersectsRect(scanner.X, scanner.Y, gx, gy,
                                              obs.X, obs.Y, obs.Width, obs.Height))
                    { ghostLosBlocked = true; break; }
                }
                if (ghostLosBlocked) continue;

                // Burnthrough has a good chance of recognising and discarding the ghost
                if (scannerBurnthrough && _rng.NextDouble() < ArenaConstants.EcmBurnthroughGhostFilterChance)
                    continue;

                double ghostDist = Math.Sqrt((gx - scanner.X) * (gx - scanner.X)
                                           + (gy - scanner.Y) * (gy - scanner.Y));

                // Clamp ghost name length for display
                string shortName = spoofer.Tank.Name.Length > 4
                    ? spoofer.Tank.Name[..4] : spoofer.Tank.Name;

                ScanResult ghostResult = new()
                {
                    Name     = $"Ghost-{shortName}",
                    SwarmId  = spoofer.Tank.SwarmId,
                    Bearing  = RelativeBearing(scanner.Heading, ghostBearing),
                    Distance = ghostDist,
                    Heading  = gh,
                    Velocity = gv,
                    Energy   = 45 + _rng.NextDouble() * 35,
                    Position = new Vector2D(gx, gy)
                };

                SafeCall(() => scanner.Tank.OnScannedTank(new ScannedTankEventArgs(ghostResult)));
            }
        }
    }

    private void DeliverSwarmMessages(TankRuntimeState sender, TankCommand cmd)
    {
        if (cmd.BroadcastMessages.Count == 0) return;

        List<TankRuntimeState> allies = RuntimeTanks
            .Where(t => t.IsAlive && t != sender && t.Tank.SwarmId == sender.Tank.SwarmId)
            .ToList();

        foreach (SwarmMessage msg in cmd.BroadcastMessages)
        {
            SwarmMessageBroadcast?.Invoke(msg, sender.Tank.SwarmId);

            foreach (TankRuntimeState ally in allies)
                SafeCall(() => ally.Tank.DeliverSwarmMessage(msg));
        }
    }

    // ── ECM helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Forces the named tank into a specific ECM mode regardless of what its AI requests.
    /// Pass <c>null</c> to restore AI control.
    /// </summary>
    public void SetEcmOverride(string tankName, EcmMode? mode)
    {
        var rts = RuntimeTanks.FirstOrDefault(
            r => string.Equals(r.Tank.Name, tankName, StringComparison.Ordinal));
        if (rts is not null) rts.EcmModeOverride = mode;
    }

    private void ApplyEcm(TankRuntimeState rts, TankCommand cmd)
    {
        // UI override takes priority over what the tank AI requested
        EcmMode effectiveMode = rts.EcmModeOverride ?? cmd.EcmMode;
        rts.ActiveEcm = effectiveMode;

        double cost = effectiveMode switch
        {
            EcmMode.Jam         => ArenaConstants.EcmJamCostPerTick,
            EcmMode.Spoof       => ArenaConstants.EcmSpoofCostPerTick,
            EcmMode.Burnthrough => ArenaConstants.EcmBurnthroughCostPerTick,
            _                   => 0.0
        };

        if (cost > 0)
        {
            rts.Energy -= cost;
            CheckDeath(rts);
        }

        if (effectiveMode == EcmMode.Spoof && rts.IsAlive)
            UpdateGhostPositions(rts);
        else
            rts.GhostPositions = [];
    }

    private void UpdateGhostPositions(TankRuntimeState rts)
    {
        int count = ArenaConstants.EcmSpoofGhostCount;

        if (rts.GhostPositions.Length != count)
        {
            // First time in Spoof mode this session — spawn ghosts at random offsets
            rts.GhostPositions = new (double X, double Y, double Heading, double Velocity)[count];
            for (int i = 0; i < count; i++)
            {
                double spawnAngle = _rng.NextDouble() * 360;
                double spawnDist  = 25 + _rng.NextDouble() * ArenaConstants.EcmSpoofRadius;
                double rad        = spawnAngle * Math.PI / 180;
                rts.GhostPositions[i] = (
                    Math.Clamp(rts.X + Math.Sin(rad) * spawnDist, 10, Width  - 10),
                    Math.Clamp(rts.Y - Math.Cos(rad) * spawnDist, 10, Height - 10),
                    _rng.NextDouble() * 360,
                    (_rng.NextDouble() * 2 - 1) * 4.0);
            }
            return;
        }

        // Each ghost drifts independently; 4 % chance per tick to change direction
        for (int i = 0; i < count; i++)
        {
            var (gx, gy, gh, gv) = rts.GhostPositions[i];

            if (_rng.NextDouble() < 0.04)
            {
                gh = _rng.NextDouble() * 360;
                gv = (_rng.NextDouble() * 2 - 1) * 5.0;
            }

            double rad = gh * Math.PI / 180;
            gx = Math.Clamp(gx + Math.Sin(rad) * gv, 10, Width  - 10);
            gy = Math.Clamp(gy - Math.Cos(rad) * gv, 10, Height - 10);

            rts.GhostPositions[i] = (gx, gy, gh, gv);
        }
    }

    private void CheckDeath(TankRuntimeState rts)
    {
        if (rts.Energy > 0 || !rts.IsAlive) return;
        rts.Energy = 0;
        rts.IsAlive = false;
        rts.DestroyedAtTick = TickNumber;
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

    // ── Spawning & building generation ────────────────────────────────────────

    /// <summary>
    /// Randomly places rectangular buildings in the arena for the upcoming round.
    /// Count scales with arena area; each building is kept away from the walls and
    /// from other buildings so tanks have room to navigate.
    /// </summary>
    private void GenerateBuildings()
    {
        _buildings.Clear();

        double arenaArea  = Width * Height;
        int    count      = Math.Clamp((int)(arenaArea / 45_000), 4, 14);
        double wallMargin = ArenaConstants.TankHalfSize * 6; // keep buildings clear of walls
        double buildingGap     = ArenaConstants.TankHalfSize * 2; // minimum gap between buildings

        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                double w = _rng.NextDouble() * 70 + 25; // 25–95 px wide
                double h = _rng.NextDouble() * 60 + 20; // 20–80 px tall
                double x = _rng.NextDouble() * (Width  - wallMargin * 2 - w) + wallMargin;
                double y = _rng.NextDouble() * (Height - wallMargin * 2 - h) + wallMargin;

                bool overlaps = _buildings.Any(obs =>
                    x < obs.X + obs.Width  + buildingGap &&
                    x + w > obs.X          - buildingGap &&
                    y < obs.Y + obs.Height + buildingGap &&
                    y + h > obs.Y          - buildingGap);

                if (!overlaps)
                {
                    _buildings.Add(new BuildingDefinition(x, y, w, h));
                    break;
                }
            }
        }
    }

    private void SpawnTanks()
    {
        double margin = ArenaConstants.TankHalfSize * 4;
        double minSep = ArenaConstants.TankHalfSize * 2 + 4; // minimum centre-to-centre gap

        foreach (TankRuntimeState rts in RuntimeTanks)
        {
            // Retry up to 200 times to find a non-overlapping spawn position.
            for (int attempt = 0; attempt < 200; attempt++)
            {
                double candidateX = _rng.NextDouble() * (Width - margin * 2) + margin;
                double candidateY = _rng.NextDouble() * (Height - margin * 2) + margin;

                bool tooClose = RuntimeTanks
                    .Where(other => other != rts && other.Energy > 0)
                    .Any(other =>
                    {
                        double dx = candidateX - other.X;
                        double dy = candidateY - other.Y;
                        return Math.Sqrt(dx * dx + dy * dy) < minSep;
                    });

                double spawnBuildingHalf = ArenaConstants.TankHalfSize * Math.Sqrt(2.0) + 4.0; // visual clearance at spawn
                bool insideBuilding = !tooClose && _buildings.Any(obs =>
                {
                    double closestX = Math.Clamp(candidateX, obs.X, obs.X + obs.Width);
                    double closestY = Math.Clamp(candidateY, obs.Y, obs.Y + obs.Height);
                    double dx = candidateX - closestX;
                    double dy = candidateY - closestY;
                    return Math.Sqrt(dx * dx + dy * dy) < spawnBuildingHalf;
                });

                if ((!tooClose && !insideBuilding) || attempt == 199)
                {
                    rts.X = candidateX;
                    rts.Y = candidateY;
                    break;
                }
            }

            rts.Heading = _rng.NextDouble() * 360;
            rts.GunHeading = rts.Heading;
            rts.RadarHeading = rts.Heading;
            rts.PrevRadarHeading = rts.Heading;
            rts.Velocity = 0;
            rts.Energy = ArenaConstants.TankStartEnergy;
            rts.IsAlive = true;
            rts.DestroyedAtTick = 0;
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

    /// <summary>
    /// Liang–Barsky segment-vs-AABB test. Returns true if the segment from
    /// (x1,y1) to (x2,y2) intersects or is contained by the rectangle.
    /// </summary>
    private static bool SegmentIntersectsRect(
        double x1, double y1, double x2, double y2,
        double rx, double ry, double rw, double rh)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        double t0 = 0.0, t1 = 1.0;

        if (!Clip(-dx, x1 - rx,       ref t0, ref t1)) return false;
        if (!Clip( dx, rx + rw - x1,  ref t0, ref t1)) return false;
        if (!Clip(-dy, y1 - ry,       ref t0, ref t1)) return false;
        if (!Clip( dy, ry + rh - y1,  ref t0, ref t1)) return false;
        return true;

        static bool Clip(double p, double q, ref double t0, ref double t1)
        {
            if (Math.Abs(p) < 1e-10) return q >= 0; // parallel — inside only if q ≥ 0
            double t = q / p;
            if (p < 0) t0 = Math.Max(t0, t);
            else       t1 = Math.Min(t1, t);
            return t0 <= t1;
        }
    }

    private static void SafeCall(Action action)
    {
        try { action(); }
        catch { /* Isolate bad AI code from crashing the engine */ }
    }
}

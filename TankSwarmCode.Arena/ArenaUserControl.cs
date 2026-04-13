using System.Drawing.Drawing2D;
using TankSwarmCode.Arena.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.Arena;

/// <summary>
/// WinForms control that hosts the <see cref="ArenaEngine"/> simulation and renders
/// the battle arena using GDI+. Drop onto a Form, call <see cref="AddTank"/> for each
/// participant, then call <see cref="Start"/>.
/// </summary>
public partial class ArenaUserControl : UserControl
{
    private ArenaEngine? _engine;
    private readonly System.Windows.Forms.Timer _gameTimer = new();
    private string _statusMessage = "Ready – call Start() to begin.";

    // Wall-clock interpolation — smooths pulse animation between discrete ticks
    private DateTime _lastTickTime = DateTime.UtcNow;
    private float _tickIntervalMs = 1000f / 3f; // kept in sync with _gameTimer.Interval

    // Pulse-gate — holds the simulation until the current round-trip animation completes
    private bool _waitingForPulse;
    private DateTime _pulseCompletionTime;

    // Rendering constants
    private const int TankBodySize = 18;
    private const int GunLength = 22;
    private const int RadarLength = 16;
    private const int EnergyBarWidth = 36;
    private const int EnergyBarHeight = 4;

    // Radar sweep trail — frames of heading history kept per tank
    private const int RadarTrailLength = 12;

    // Radar ping halo — sonar-pulse sizing; lifetime drives the expand+fade animation
    private const int RadarHaloBaseRadius = 18;   // px at full energy (at spawn time)
    private const int RadarHaloMinRadius = 8;     // px at near-dead energy (at spawn time)
    private const int ScanHaloLifetime = 10;      // ticks — 5 out + 5 back; ~1.7 s per phase at 3 TPS

    // Per-swarm colours (index = SwarmId % palette length)
    private static readonly Color[] SwarmColours =
    [
        Color.DodgerBlue,
        Color.OrangeRed,
        Color.LimeGreen,
        Color.Gold,
        Color.MediumOrchid,
        Color.DeepSkyBlue,
        Color.Coral,
        Color.Chartreuse
    ];

    // Per-tank radar heading history used to paint phosphor-decay sweep trails
    private readonly Dictionary<string, LinkedList<double>> _radarTrails =
        new(StringComparer.Ordinal);

    // One entry per physical radar hit; each lives for ScanHaloLifetime ticks then is removed.
    private readonly List<ScanEvent> _scanEvents = [];

    /// <summary>
    /// Immutable snapshot of a single radar-beam hit, captured the tick the radar
    /// physically swept over the enemy. Drives the expanding sonar-pulse halo animation.
    /// </summary>
    private readonly record struct ScanEvent(
        long TickFired,
        DateTime TickFiredWallTime,  // wall-clock birth time for smooth interpolation
        string SpotterName,
        Vector2D SpotterPosition,
        Vector2D Position,
        int EnemySwarmId,
        string EnemyName,
        Color SpotterColor,
        float EnergyFraction,
        Vector2D Velocity,
        float SweepSpanDeg,      // absolute angular width swept this tick
        float RadarHeadingDeg);  // radar heading at moment of contact (centre of swept arc)

    public ArenaUserControl()
    {
        InitializeComponent();

        SetStyle(ControlStyles.OptimizedDoubleBuffer
               | ControlStyles.AllPaintingInWmPaint
               | ControlStyles.UserPaint, true);

        _gameTimer.Tick += GameTimer_Tick;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Number of simulation ticks per second (default 20).</summary>
    [System.ComponentModel.DefaultValue(20)]
    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int TicksPerSecond
    {
        get => _gameTimer.Interval > 0 ? 1000 / _gameTimer.Interval : 20;
        set => _gameTimer.Interval = Math.Max(1, 1000 / Math.Max(1, value));
    }

    /// <summary>Underlying arena. Available after construction; populated by <see cref="AddTank"/>.</summary>
    public IArena? Arena => _engine;

    /// <summary>Registers a tank. Call before <see cref="Start"/>.</summary>
    public void AddTank(ISwarmTank tank)
    {
        _engine ??= CreateEngine();
        _engine.AddTank(tank);
        Invalidate();
    }

    /// <summary>Starts (or resumes) the simulation.</summary>
    public void Start()
    {
        _engine ??= CreateEngine();

        if (!_engine.IsRunning)
        {
            if (_engine.HasStarted)
                _engine.Resume();
            else
                _engine.Start();
        }

        _gameTimer.Interval = Math.Max(1, 1000 / Math.Max(1, TicksPerSecond));
        _gameTimer.Start();
        _statusMessage = string.Empty;
        Invalidate();
    }

    /// <summary>Pauses the simulation without resetting.</summary>
    public void Stop()
    {
        _gameTimer.Stop();
        _engine?.Stop();
        _waitingForPulse = false;
        _statusMessage = "Paused";
        Invalidate();
    }

    /// <summary>
    /// Pauses the timer (if running) and advances exactly one simulation tick.
    /// Initializes the engine on the first call if <see cref="Start"/> has not yet been called.
    /// </summary>
    public void SingleStep()
    {
        _gameTimer.Stop();
        _waitingForPulse = false;

        _engine ??= CreateEngine();

        if (!_engine.HasStarted)
            _engine.Start();

        _lastTickTime = DateTime.UtcNow;
        _engine.StepOnce();
        HarvestScanEvents();
        _statusMessage = $"Tick {_engine.TickNumber}";
        Invalidate();
    }

    /// <summary>Stops the simulation and clears all tanks.</summary>
    public void Reset()
    {
        _gameTimer.Stop();
        _engine?.Reset();
        _engine = null;
        _radarTrails.Clear();
        _scanEvents.Clear();
        _waitingForPulse = false;
        _statusMessage = "Ready – call Start() to begin.";
        Invalidate();
    }

    // ── Engine wiring ─────────────────────────────────────────────────────────

    private ArenaEngine CreateEngine()
    {
        var engine = new ArenaEngine(ClientSize.Width, ClientSize.Height);
        engine.TickCompleted += Engine_TickCompleted;
        engine.RoundEnded += Engine_RoundEnded;
        return engine;
    }

    private void Engine_TickCompleted(object? sender, TickEventArgs e)
    {
        // Redraw is driven by the timer; nothing extra needed here.
    }

    private void Engine_RoundEnded(object? sender, RoundEndedEventArgs e)
    {
        _gameTimer.Stop();
        _statusMessage = $"Round ended after {e.TotalTicks} ticks.";
        Invalidate();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        DateTime now = DateTime.UtcNow;
        _tickIntervalMs = _gameTimer.Interval;

        // If we are waiting for a radar pulse round-trip to complete, just repaint
        // so the animation keeps running — do not advance the simulation.
        if (_waitingForPulse)
        {
            if (now < _pulseCompletionTime)
            {
                Invalidate();
                return;
            }

            _waitingForPulse = false;
        }

        _lastTickTime = now;
        _engine?.Tick();
        int countBefore = _scanEvents.Count;
        HarvestScanEvents();

        // If this tick produced any new radar contacts, gate the next tick until
        // the full outbound + echo animation has played out.
        if (_scanEvents.Count > countBefore)
        {
            float lifetimeMs = ScanHaloLifetime * _tickIntervalMs;
            _pulseCompletionTime = _lastTickTime.AddMilliseconds(lifetimeMs);
            _waitingForPulse = true;
        }

        Invalidate();
    }

    /// <summary>
    /// Called once per engine tick (from <see cref="GameTimer_Tick"/>) to harvest physical
    /// radar hits into <see cref="_scanEvents"/>.  Running here — not inside OnPaint — ensures
    /// each contact fires at most one <see cref="ScanEvent"/> per tick regardless of how many
    /// repaints occur.  A deduplication guard prevents stacking when the same pair is still
    /// live from a previous tick.
    /// </summary>
    private void HarvestScanEvents()
    {
        if (_engine is null) return;

        long tick = _engine.TickNumber;

        var bySwarm = _engine.Tanks
            .GroupBy(t => t.SwarmId)
            .OrderBy(grp => grp.Key);

        foreach (var swarmGroup in bySwarm)
        {
            Color baseColor = SwarmColours[Math.Abs(swarmGroup.Key) % SwarmColours.Length];
            ISwarmTank[] swarmTanks = [.. swarmGroup.OrderBy(t => t.Name)];

            for (int i = 0; i < swarmTanks.Length; i++)
            {
                ISwarmTank tank = swarmTanks[i];
                if (!tank.State.IsAlive) continue;

                Color spotterColor = LightenColor(baseColor, i * 22);

                foreach (RadarContact contact in tank.RadarMap.Values)
                {
                    // Only fire when this tank's OWN radar physically swept the enemy this tick.
                    if (contact.Timestamp != tick) continue;
                    if (!string.Equals(contact.SpottedBy, tank.Name,
                            StringComparison.Ordinal)) continue;

                    // Dedup: skip if we already recorded this (spotter, enemy) pair this tick.
                    if (_scanEvents.Any(ev =>
                            ev.TickFired == tick &&
                            string.Equals(ev.SpotterName, tank.Name, StringComparison.Ordinal) &&
                            string.Equals(ev.EnemyName, contact.Name, StringComparison.Ordinal)))
                        continue;

                    float energyFraction = (float)Math.Clamp(
                        contact.Energy / ArenaConstants.TankStartEnergy, 0.1, 1.0);

                    double sweepDelta = tank.State.RadarHeading - tank.State.PrevRadarHeading;
                    while (sweepDelta >  180) sweepDelta -= 360;
                    while (sweepDelta < -180) sweepDelta += 360;
                    float sweepSpan = (float)Math.Max(Math.Abs(sweepDelta), 1.0);

                    _scanEvents.Add(new ScanEvent(
                        TickFired:          tick,
                        TickFiredWallTime:  _lastTickTime,
                        SpotterName:        tank.Name,
                        SpotterPosition: tank.State.Position,
                        Position:        contact.Position,
                        EnemySwarmId:    contact.EnemySwarmId,
                        EnemyName:       contact.Name,
                        SpotterColor:    spotterColor,
                        EnergyFraction:  energyFraction,
                        Velocity:        contact.VelocityVector,
                        SweepSpanDeg:    sweepSpan,
                        RadarHeadingDeg: (float)tank.State.RadarHeading));
                }
            }
        }
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        DrawBackground(g);

        if (_engine is null)
        {
            DrawCentredText(g, _statusMessage, Font, Brushes.Gray);
            return;
        }

        // Layer 1 – Radar reflections: beam from spotter to detected tank
        DrawAllRadarHalos(g);

        foreach (BulletState bullet in _engine.Bullets)
            DrawBullet(g, bullet);

        foreach (ISwarmTank tank in _engine.Tanks)
            DrawTank(g, tank.State);

        DrawHud(g);

        if (!string.IsNullOrEmpty(_statusMessage))
            DrawCentredText(g, _statusMessage, new Font(Font.FontFamily, 14, FontStyle.Bold), Brushes.White);
    }

    private void DrawBackground(Graphics g)
    {
        g.FillRectangle(Brushes.Black, ClientRectangle);
        using Pen borderPen = new(Color.FromArgb(60, 60, 60), 2);
        g.DrawRectangle(borderPen, 1, 1, ClientSize.Width - 2, ClientSize.Height - 2);
    }

    private void DrawTank(Graphics g, TankState tank)
    {
        if (!tank.IsAlive) return;

        float x = (float)tank.Position.X;
        float y = (float)tank.Position.Y;
        Color tankColor = SwarmColours[Math.Abs(tank.SwarmId) % SwarmColours.Length];

        GraphicsState saved = g.Save();
        g.TranslateTransform(x, y);

        // --- Body ---
        g.RotateTransform((float)tank.Heading);
        int half = TankBodySize / 2;
        using SolidBrush bodyBrush = new(tankColor);
        using Pen bodyPen = new(Color.White, 1);
        g.FillRectangle(bodyBrush, -half, -half, TankBodySize, TankBodySize);
        g.DrawRectangle(bodyPen, -half, -half, TankBodySize, TankBodySize);

        // Tread marks
        using Pen treadPen = new(Color.FromArgb(160, Color.Black), 3);
        g.DrawLine(treadPen, -half, -half + 3, -half, half - 3);
        g.DrawLine(treadPen,  half, -half + 3,  half, half - 3);

        g.RotateTransform(-(float)tank.Heading);

        // --- Gun ---
        float gunRad = (float)(tank.GunHeading * Math.PI / 180.0);
        float gunDx = (float)Math.Sin(gunRad) * GunLength;
        float gunDy = -(float)Math.Cos(gunRad) * GunLength;
        using Pen gunPen = new(Color.LightGray, 3);
        g.DrawLine(gunPen, 0, 0, gunDx, gunDy);

        // --- Radar sweep trail ---
        DrawRadarSweepTrail(g, tank, tankColor);

        g.Restore(saved);

        // --- Energy bar ---
        float barX = x - EnergyBarWidth / 2f;
        float barY = y - TankBodySize / 2f - 10;
        float energyFraction = (float)Math.Clamp(tank.Energy / ArenaConstants.TankStartEnergy, 0, 1);
        g.FillRectangle(Brushes.DarkRed, barX, barY, EnergyBarWidth, EnergyBarHeight);
        using SolidBrush energyBrush = new(Color.LawnGreen);
        g.FillRectangle(energyBrush, barX, barY, EnergyBarWidth * energyFraction, EnergyBarHeight);

        // --- Name label ---
        using Font nameFont = new(Font.FontFamily, 7);
        SizeF textSize = g.MeasureString(tank.Name, nameFont);
        g.DrawString(tank.Name, nameFont, Brushes.LightGray,
                     x - textSize.Width / 2, barY - textSize.Height - 1);
    }

    /// <summary>
    /// Draws a phosphor-decay radar sweep trail plus the exact scan-arc flash for this tick.
    /// <list type="bullet">
    ///   <item>Decay trail — fading <see cref="RadarTrailLength"/>-frame history in the tank's swarm colour.</item>
    ///   <item>Scan-arc flash — the precise arc swept from <see cref="TankState.PrevRadarHeading"/> to
    ///         <see cref="TankState.RadarHeading"/> this tick, drawn bright white over the trail.</item>
    ///   <item>Leading-edge beam — a crisp line at the current heading.</item>
    /// </list>
    /// Called from within the saved graphics transform (tank centre at origin, no rotation).
    /// </summary>
    private void DrawRadarSweepTrail(Graphics g, TankState tank, Color swarmColor)
    {
        // Maintain a per-tank circular buffer of radar headings in the renderer.
        if (!_radarTrails.TryGetValue(tank.Name, out LinkedList<double>? trail))
        {
            trail = new LinkedList<double>();
            _radarTrails[tank.Name] = trail;
        }

        trail.AddLast(tank.RadarHeading);
        while (trail.Count > RadarTrailLength)
            trail.RemoveFirst();

        // ── Phosphor-decay trail ──────────────────────────────────────────────
        // Oldest frame = near-invisible, newest = 100-alpha glow.
        // GDI+ FillPie: 0° = east (3 o'clock), clockwise.
        // Arena heading: 0° = north, clockwise → GDI+ start = heading − 90 − half-arc-width.
        double[] history = [.. trail];
        int count = history.Length;

        for (int i = 0; i < count; i++)
        {
            float ageFraction = (float)(i + 1) / count; // 0 = oldest, 1 = newest
            int alpha = (int)(100 * ageFraction);
            if (alpha < 5) continue;

            float gdiStart = (float)history[i] - 90f - 10f;
            using SolidBrush fadeBrush = new(Color.FromArgb(alpha, swarmColor));
            g.FillPie(fadeBrush,
                -RadarLength, -RadarLength,
                RadarLength * 2, RadarLength * 2,
                gdiStart, 20f);
        }

        // ── Exact scan-arc flash (this tick's true sweep zone) ────────────────
        // Compute the angular span the radar physically crossed this tick.
        double prev = tank.PrevRadarHeading;
        double curr = tank.RadarHeading;
        double delta = curr - prev;
        // Normalise delta to (−180, +180] for shortest-path calculation.
        while (delta > 180) delta -= 360;
        while (delta < -180) delta += 360;

        if (Math.Abs(delta) > 0.1)
        {
            // GDI+ start angle is the leading edge of the sweep (direction of travel).
            float flashStart = delta >= 0
                ? (float)prev - 90f          // sweeping clockwise: arc starts at prev
                : (float)curr - 90f;         // sweeping counter-clockwise: arc starts at curr
            float flashSpan = (float)Math.Abs(delta);

            // Outer bright layer: white-tinted swarm colour at full opacity.
            using SolidBrush flashBrush = new(Color.FromArgb(170, LightenColor(swarmColor, 120)));
            g.FillPie(flashBrush,
                -RadarLength, -RadarLength,
                RadarLength * 2, RadarLength * 2,
                flashStart, flashSpan);

            // Inner core: pure white highlight to mark the active scan zone.
            int innerR = (int)(RadarLength * 0.55f);
            using SolidBrush coreBrush = new(Color.FromArgb(80, Color.White));
            g.FillPie(coreBrush,
                -innerR, -innerR,
                innerR * 2, innerR * 2,
                flashStart, flashSpan);
        }

        // ── Leading-edge beam line at current heading ─────────────────────────
        float radarRad = (float)(tank.RadarHeading * Math.PI / 180.0);
        float rdx = (float)Math.Sin(radarRad) * RadarLength;
        float rdy = -(float)Math.Cos(radarRad) * RadarLength;
        using Pen radarPen = new(Color.FromArgb(220, swarmColor), 1.5f);
        g.DrawLine(radarPen, 0, 0, rdx, rdy);
    }

    private static void DrawBullet(Graphics g, BulletState bullet)
    {
        float bx = (float)bullet.Position.X;
        float by = (float)bullet.Position.Y;
        float radius = (float)(1.5 + bullet.Power);

        using SolidBrush bulletBrush = new(Color.Yellow);
        g.FillEllipse(bulletBrush, bx - radius, by - radius, radius * 2, radius * 2);

        // Glow
        using SolidBrush glowBrush = new(Color.FromArgb(60, Color.OrangeRed));
        float glow = radius * 2.5f;
        g.FillEllipse(glowBrush, bx - glow, by - glow, glow * 2, glow * 2);
    }

    private void DrawHud(Graphics g)
    {
        if (_engine is null) return;

        // Tick counter (top-left)
        string tickText = $"Tick: {_engine.TickNumber}";
        g.DrawString(tickText, Font, Brushes.DimGray, 6, 6);

        // Swarm scoreboard (top-right)
        var swarmGroups = _engine.Tanks
            .GroupBy(t => t.SwarmId)
            .OrderBy(grp => grp.Key)
            .ToList();

        float scoreX = ClientSize.Width - 120;
        float scoreY = 6;

        foreach (var grp in swarmGroups)
        {
            int alive = grp.Count(t => t.State.IsAlive);
            Color c = SwarmColours[Math.Abs(grp.Key) % SwarmColours.Length];
            string label = grp.Key == 0
                ? $"Solo: {alive} alive"
                : $"Swarm {grp.Key}: {alive} alive";

            using SolidBrush sb = new(c);
            g.DrawString(label, Font, sb, scoreX, scoreY);
            scoreY += Font.Height + 2;
        }

        // Radar legend (bottom-left)
        DrawRadarLegend(g);
    }

    private void DrawCentredText(Graphics g, string text, Font font, Brush brush)
    {
        SizeF size = g.MeasureString(text, font);
        float cx = (ClientSize.Width - size.Width) / 2f;
        float cy = (ClientSize.Height - size.Height) / 2f;
        g.DrawString(text, font, brush, cx, cy);
    }

    /// <summary>
    /// Renders all live radar-reflection events.
    /// Each event shows a beam line from the scanning tank to the detected enemy,
    /// a reflection burst at the enemy position, and a small pulse dot at the spotter.
    /// Events are pruned after <see cref="ScanHaloLifetime"/> ticks.
    /// </summary>
    private void DrawAllRadarHalos(Graphics g)
    {
        if (_engine is null) return;

        float lifetimeMs = ScanHaloLifetime * _tickIntervalMs;
        DateTime now = DateTime.UtcNow;

        _scanEvents.RemoveAll(e => (float)(now - e.TickFiredWallTime).TotalMilliseconds > lifetimeMs);

        // For each unique (spotter, enemy) pair only render the most recently fired event —
        // this ensures exactly one wavefront arc is visible per beam at any moment.
        var latestPerPair = _scanEvents
            .GroupBy(ev => (ev.SpotterName, ev.EnemyName))
            .Select(grp => grp.MaxBy(ev => ev.TickFired));

        foreach (ScanEvent ev in latestPerPair)
            DrawRadarReflection(g, ev, now);
    }

    // ── Radar reflection helpers ──────────────────────────────────────────────

    /// <summary>
    /// Renders a radar scan event as two strictly sequential expanding arc wavefronts.
    /// Only one wave is ever visible at a time:
    /// <list type="bullet">
    ///   <item>Phase 1 (first half of lifetime) — solid arc in spotter colour travels from the
    ///         spotter outward along the radar heading, fading to nothing as it reaches the enemy.</item>
    ///   <item>Dead gap — at the phase boundary both waves are fully transparent.</item>
    ///   <item>Phase 2 (second half of lifetime) — dashed arc in enemy colour travels from the
    ///         enemy back toward the spotter, fading to nothing as it arrives.</item>
    /// </list>
    /// </summary>
    private void DrawRadarReflection(Graphics g, ScanEvent ev, DateTime now)
    {
        float lifetimeMs  = ScanHaloLifetime * _tickIntervalMs;
        float elapsedMs   = (float)(now - ev.TickFiredWallTime).TotalMilliseconds;
        float ageFraction = Math.Clamp(elapsedMs / lifetimeMs, 0f, 1f);

        float sx = (float)ev.SpotterPosition.X;
        float sy = (float)ev.SpotterPosition.Y;
        float ex = (float)ev.Position.X;
        float ey = (float)ev.Position.Y;

        float dist = MathF.Sqrt((ex - sx) * (ex - sx) + (ey - sy) * (ey - sy));
        if (dist < 2f) return;

        Color spotterColor = ev.SpotterColor;

        // Outbound arc: centred on the radar heading at contact time (arena 0°=N → GDI+ subtract 90°).
        float outboundMid = ev.RadarHeadingDeg - 90f;
        float arcSpan     = ev.SweepSpanDeg;

        // ── Phase 1: outbound wave (ageFraction 0 → <0.5) ────────────────────
        // t goes 0→1 across the first half-lifetime.
        // Fade: full brightness at t=0, completely gone at t=1 so the phase boundary is clean.
        // A smooth-step curve keeps it visible during transit and drops sharply at arrival.
        if (ageFraction < 0.5f)
        {
            float t      = ageFraction * 2f;                          // 0→1
            float radius = dist * t;                                  // 0 → dist
            float fade   = 1f - t * t * t;                           // cubic: slow drop then steep at end
            if (radius > 1f && fade > 0.01f)
            {
                float penW = Math.Max(1f, 2.5f * (1f - t * 0.6f));
                using Pen arcPen = new(Color.FromArgb((int)(230 * fade), spotterColor), penW);
                g.DrawArc(arcPen,
                    sx - radius, sy - radius, radius * 2, radius * 2,
                    outboundMid - arcSpan / 2f, arcSpan);

                // Two edge lines from the spotter back to each end of the arc.
                float edgeAlpha = (int)(160 * fade);
                using Pen trailPen = new(Color.FromArgb((int)edgeAlpha, spotterColor), 0.8f);
                foreach (float edgeAngleDeg in new[] { outboundMid - arcSpan / 2f, outboundMid + arcSpan / 2f })
                {
                    float edgeRad = edgeAngleDeg * MathF.PI / 180f;
                    float edgeX   = sx + MathF.Cos(edgeRad) * radius;
                    float edgeY   = sy + MathF.Sin(edgeRad) * radius;
                    g.DrawLine(trailPen, sx, sy, edgeX, edgeY);
                }
            }
        }

        // ── Phase 2: echo wave (ageFraction 0.5 → 1.0) ───────────────────────
        // The echo originates at the sensed enemy's position and travels back to the spotter.
        // Arc centre interpolates from (ex,ey) toward (sx,sy) as t goes 0→1.
        // Arc span is deliberately narrow — a reflected ping, not a broad sweep.
        if (ageFraction >= 0.5f)
        {
            float t    = (ageFraction - 0.5f) * 2f;               // 0→1
            float fade = 1f - t * t * t;                           // cubic: bright start, clean end

            // Arc centre travels from enemy → spotter.
            float cx = ex + (sx - ex) * t;
            float cy = ey + (sy - ey) * t;

            // Radius keeps the wavefront at the moving centre (half the remaining distance).
            float remaining = dist * (1f - t);
            float radius    = remaining * 0.18f;                   // thin leading edge, not a full half-circle

            // Bearing from the current centre back toward the spotter (arc faces the direction of travel).
            float bearingToSpotter = MathF.Atan2(sy - cy, sx - cx) * 180f / MathF.PI;

            // Narrow fixed span — a tight reflected ping.
            const float echoSpan = 18f;

            if (radius > 1f && fade > 0.01f)
            {
                float penW = Math.Max(0.8f, 2f * (1f - t * 0.5f));
                using Pen arcPen = new(Color.FromArgb((int)(220 * fade), spotterColor), penW);
                arcPen.DashStyle = DashStyle.Dash;
                arcPen.DashPattern = [4f, 3f];
                g.DrawArc(arcPen,
                    cx - radius, cy - radius, radius * 2, radius * 2,
                    bearingToSpotter - echoSpan / 2f, echoSpan);

                // Single trailing line along the echo centre-axis toward the spotter.
                float edgeAlpha = (int)(130 * fade);
                using Pen trailPen = new(Color.FromArgb((int)edgeAlpha, spotterColor), 0.7f);
                trailPen.DashStyle = DashStyle.Dash;
                trailPen.DashPattern = [4f, 3f];
                float echoMidRad = bearingToSpotter * MathF.PI / 180f;
                float echoTipX   = cx + MathF.Cos(echoMidRad) * radius;
                float echoTipY   = cy + MathF.Sin(echoMidRad) * radius;
                g.DrawLine(trailPen, cx, cy, echoTipX, echoTipY);

                // Enemy name label: visible at the start of the echo, fades out quickly.
                if (t < 0.3f)
                {
                    float labelFade = (1f - t / 0.3f) * fade;
                    float baseR = RadarHaloMinRadius + (RadarHaloBaseRadius - RadarHaloMinRadius) * ev.EnergyFraction;
                    using Font contactFont = new(Font.FontFamily, 6f);
                    using SolidBrush labelBrush = new(Color.FromArgb((int)(200 * labelFade), Color.White));
                    g.DrawString(ev.EnemyName, contactFont, labelBrush, ex + baseR + 3f, ey - 4f);
                }
            }
        }
    }

    /// <summary>Lightens a colour by adding <paramref name="amount"/> to each RGB channel.</summary>
    private static Color LightenColor(Color c, int amount) =>
        Color.FromArgb(
            c.A,
            Math.Min(255, c.R + amount),
            Math.Min(255, c.G + amount),
            Math.Min(255, c.B + amount));

    /// <summary>
    /// Draws a compact legend in the bottom-left corner explaining the radar colour coding:
    /// <list type="bullet">
    ///   <item>One row per active swarm — swarm colour + label.</item>
    ///   <item>Static rows for outer ring, inner ring, velocity arrow, and fade key.</item>
    /// </list>
    /// </summary>
    private void DrawRadarLegend(Graphics g)
    {
        if (_engine is null) return;

        const int PadX = 8;
        const int PadY = 6;
        const int SwatchSize = 10;
        const int RowH = 15;
        const int ColW = 170;

        // Collect active swarms for the dynamic rows.
        var swarms = _engine.Tanks
            .GroupBy(t => t.SwarmId)
            .OrderBy(grp => grp.Key)
            .ToList();

        int dynamicRows = swarms.Count;
        int staticRows = 5; // outer ring, inner ring, scan flash, velocity arrow, fade
        int totalRows = dynamicRows + 1 + staticRows; // +1 separator row

        int panelW = ColW + PadX * 2;
        int panelH = totalRows * RowH + PadY * 2;
        int panelX = PadX;
        int panelY = ClientSize.Height - panelH - PadY;

        // Semi-transparent panel background.
        using SolidBrush panelBrush = new(Color.FromArgb(140, Color.Black));
        g.FillRectangle(panelBrush, panelX, panelY, panelW, panelH);
        using Pen panelPen = new(Color.FromArgb(80, Color.Gray), 1f);
        g.DrawRectangle(panelPen, panelX, panelY, panelW, panelH);

        using Font legendFont = new(Font.FontFamily, 6.5f);
        using Font headerFont = new(Font.FontFamily, 6.5f, FontStyle.Bold);

        float tx = panelX + PadX;
        float ty = panelY + PadY;

        // ── Swarm colour rows ─────────────────────────────────────────────────
        foreach (var grp in swarms)
        {
            Color sc = SwarmColours[Math.Abs(grp.Key) % SwarmColours.Length];
            int alive = grp.Count(t => t.State.IsAlive);
            string label = grp.Key == 0
                ? $"Solo  ({alive} alive)"
                : $"Swarm {grp.Key}  ({alive} alive)";

            using SolidBrush swatchBrush = new(sc);
            g.FillRectangle(swatchBrush, tx, ty + 1, SwatchSize, SwatchSize);
            using Pen swatchPen = new(Color.FromArgb(160, Color.White), 1f);
            g.DrawRectangle(swatchPen, tx, ty + 1, SwatchSize, SwatchSize);

            using SolidBrush textBrush = new(sc);
            g.DrawString(label, headerFont, textBrush, tx + SwatchSize + 4, ty);
            ty += RowH;
        }

        // Separator line.
        using Pen sepPen = new(Color.FromArgb(60, Color.Gray), 1f);
        g.DrawLine(sepPen, tx, ty + 4, tx + ColW, ty + 4);
        ty += RowH;

        // ── Static legend rows ────────────────────────────────────────────────
        void LegendRow(Color swatch, string text, bool outline = false)
        {
            using SolidBrush sb = new(swatch);
            if (outline)
            {
                using Pen ep = new(swatch, 1.5f);
                g.DrawEllipse(ep, tx, ty + 2, SwatchSize, SwatchSize - 2);
            }
            else
            {
                g.FillEllipse(sb, tx, ty + 2, SwatchSize, SwatchSize - 2);
            }
            using SolidBrush tb = new(Color.FromArgb(200, Color.LightGray));
            g.DrawString(text, legendFont, tb, tx + SwatchSize + 4, ty);
            ty += RowH;
        }

        LegendRow(Color.FromArgb(200, Color.DodgerBlue),  "Arcs — outbound wave (spotter colour)");
        LegendRow(Color.FromArgb(200, Color.OrangeRed),   "Dashed arcs — echo wave (enemy colour)", outline: true);
        LegendRow(Color.FromArgb(120, Color.LightGray),   "Fade — contact freshness");
        LegendRow(Color.FromArgb(200, Color.White),       "Label — detected tank name");
        LegendRow(Color.FromArgb(120, Color.LightGray),   "Arc width — detection range");
    }

    // ── Resize ────────────────────────────────────────────────────────────────

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (ClientSize.Width > 0 && ClientSize.Height > 0)
            _engine?.Resize(ClientSize.Width, ClientSize.Height);
        Invalidate();
    }
}

using System.Drawing.Drawing2D;
using TankSwarmCode.Arena;
using TankSwarmCode.Arena.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode;

/// <summary>
/// WinForms control that hosts the <see cref="ArenaEngine"/> simulation and renders
/// the battle arena using GDI+. Drop onto a Form, call <see cref="AddTank"/> for each
/// participant, then call <see cref="Start"/>.
/// </summary>
public partial class ArenaUserControl : UserControl
{
    private ArenaEngine? _engine;
    private readonly System.Windows.Forms.Timer _gameTimer = new();
    private string _statusMessage = "Add tanks via Red Swarm or Blue Swarm, then click Start.";

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

    // Explosion animation — plays once per destroyed tank at the tick the tank dies
    private const float ExplosionDurationMs = 2100f;  // total wall-clock ms for the full sequence
    private const float BurnStartMs        =  400f;  // flames ramp in from here
    private const float BurnRampMs         =  800f;  // ms to reach full flame intensity

    // Wall-clock birth time of the explosion for each destroyed tank (keyed by tank name).
    // Recorded at _lastTickTime the first tick we see IsAlive == false.
    private readonly Dictionary<string, DateTime> _explosionBirthTimes =
        new(StringComparer.Ordinal);

    // Radar radio throttle — keyed by "sender|target"; value is the last position logged.
    // A new log entry is only emitted when the contact has moved at least RadarLogMinMovePx.
    private readonly Dictionary<string, Vector2D> _radarLogLastPos =
        new(StringComparer.Ordinal);
    private const float RadarLogMinMovePx = 30f;

    // Tank names whose info panel is currently attached (right-click → Attach Info Panel).
    private readonly HashSet<string> _attachedPanels = new(StringComparer.Ordinal);

    // Screen bounds of each panel from the last paint pass — used for hover hit-testing.
    private readonly Dictionary<string, RectangleF> _panelBounds    = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RectangleF> _closeBtnBounds = new(StringComparer.Ordinal);
    private string? _hoveredPanelName;

    // Sensor-view: non-null while the user holds LMB on a tank.
    // Sensor-view: non-null while the user holds LMB on a tank, or when pinned via double-click.
    private ISwarmTank? _focusedTank;
    private ISwarmTank? _pinnedTank;

    // ── Render-toggle properties (set by ArenaConfigurationUserControl) ─────────
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowAntiAliasing     { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowRadarReflections { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowRadarSweepTrails { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowScanHalos        { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowBullets          { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowExplosions       { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowEnergyBars       { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowTankLabels       { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowHud              { get; set; } = true;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowInfoPanels       { get; set; } = true;

    // Info-panel layout constants
    private const int InfoPanelWidth   = 165;
    private const int InfoPanelRowH    =  13;
    private const int InfoPanelPadX    =   6;
    private const int InfoPanelPadY    =   5;
    private const int InfoPanelOffsetX =  26;   // px right of tank centre
    private const int InfoPanelOffsetY = -16;   // px above tank centre

    // Per-swarm colours (index = SwarmId % palette length)
    // Index 0 = solo/unknown, 1 = Red swarm, 2 = Blue swarm
    private static readonly Color[] SwarmColours =
    [
        Color.Silver,
        Color.OrangeRed,
        Color.DodgerBlue,
        Color.Gold,
        Color.MediumOrchid,
        Color.LimeGreen,
        Color.Coral,
        Color.Chartreuse
    ];

    // Per-tank radar heading history used to paint phosphor-decay sweep trails
    private readonly Dictionary<string, LinkedList<double>> _radarTrails =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Raised on the UI thread whenever a tank broadcasts a non-radar swarm message.
    /// The <see cref="RadioTransmissionEventArgs.FormattedLine"/> follows the strict
    /// [MSGTYPE]: content radio format.
    /// </summary>
    public event EventHandler<RadioTransmissionEventArgs>? RadioTransmission;

    /// <summary>
    /// Raised on the UI thread at the end of every simulation tick.
    /// </summary>
    public event EventHandler<TickEventArgs>? TickCompleted;

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
        float SweepSpanDeg,      // absolute angular width swept this tick (|PrevRadarHeading→RadarHeading|)
        float SweepMidDeg);      // midpoint of the swept arc in arena coords (NOT the final heading)

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

    /// <summary>
    /// How many simulation ticks are run per timer fire.
    /// Computed automatically from the active render toggles — disabling
    /// animation layers (radar reflections, scan halos) removes the pulse gate
    /// and allows batching multiple ticks per paint, multiplying effective speed.
    /// </summary>
    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int SimTicksPerFrame => ComputeSimTicksPerFrame();

    /// <summary>Effective simulation ticks per second, accounting for multi-tick batching.</summary>
    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int EffectiveTps => TicksPerSecond * ComputeSimTicksPerFrame();

    private int ComputeSimTicksPerFrame()
    {
        // Radar reflections and scan halos drive the pulse gate — while either is
        // on the timer must pause between ticks for the animation to complete,
        // so we can only run one tick per fire.
        if (ShowRadarReflections || ShowScanHalos)
            return 1;

        // No pulse gate needed: batch additional ticks per frame.
        // Each expensive layer that is off contributes extra capacity.
        int ticks = 5;                          // base boost without animations
        if (!ShowRadarSweepTrails) ticks += 3;
        if (!ShowExplosions)       ticks += 3;
        if (!ShowBullets)          ticks += 2;
        if (!ShowEnergyBars)       ticks += 1;
        if (!ShowTankLabels)       ticks += 1;
        return Math.Min(ticks, 40);
    }

    /// <summary>Underlying arena. Available after construction; populated by <see cref="AddTank"/>.</summary>
    public IArena? Arena => _engine;

    /// <summary>Number of tanks currently registered (0 when no tanks have been added).</summary>
    public int TankCount => _engine?.Tanks.Count ?? 0;

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
        // Guard: refuse to start with no tanks — show a friendly hint instead of crashing.
        if (TankCount == 0)
        {
            _statusMessage = "Add tanks first — use the Red Swarm or Blue Swarm menus.";
            Invalidate();
            return;
        }

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

        if (TankCount == 0)
        {
            _statusMessage = "Add tanks first — use the Red Swarm or Blue Swarm menus.";
            Invalidate();
            return;
        }

        _engine ??= CreateEngine();

        if (!_engine.HasStarted)
            _engine.Start();

        _lastTickTime = DateTime.UtcNow;
        _engine.StepOnce();
        HarvestExplosionBirthTimes();
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
        _explosionBirthTimes.Clear();
        _attachedPanels.Clear();
        _closeBtnBounds.Clear();
        _focusedTank = null;
        _pinnedTank  = null;
        _waitingForPulse = false;
        _statusMessage = "Add tanks via Red Swarm or Blue Swarm, then click Start.";
        Invalidate();
    }

    // ── Right-click context menu ──────────────────────────────────────────────

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (_engine is null) return;

        if (e.Button == MouseButtons.Right)
        {
            ISwarmTank? hit = HitTestTank(e.Location);
            if (hit is not null)
                ShowTankContextMenu(hit.Name, e.Location);
        }
        else if (e.Button == MouseButtons.Left)
        {
            // Check if the click landed on a panel close button first
            foreach (var (name, btnRect) in _closeBtnBounds)
            {
                if (btnRect.Contains(e.X, e.Y))
                {
                    _attachedPanels.Remove(name);
                    _closeBtnBounds.Remove(name);
                    _panelBounds.Remove(name);
                    Invalidate();
                    return;
                }
            }

            ISwarmTank? hit = HitTestTank(e.Location);
            if (hit is not null)
            {
                _focusedTank = hit;
                Invalidate();
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            _focusedTank = _pinnedTank; // release hold; keep pin if active
            Invalidate();
        }
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);
        if (_engine is null || e.Button != MouseButtons.Left) return;

        ISwarmTank? hit = HitTestTank(e.Location);
        if (hit is null) return;

        // Same tank → unpin; any other tank (or no pin) → pin this one
        _pinnedTank  = ReferenceEquals(_pinnedTank, hit) ? null : hit;
        _focusedTank = _pinnedTank;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_attachedPanels.Count == 0) return;

        string? newHover = null;
        foreach (var (name, bounds) in _panelBounds)
        {
            if (bounds.Contains(e.X, e.Y))
            {
                newHover = name;
                break;
            }
        }

        if (!string.Equals(newHover, _hoveredPanelName, StringComparison.Ordinal))
        {
            _hoveredPanelName = newHover;
            Invalidate();
        }
    }

    private ISwarmTank? HitTestTank(Point pt)
    {
        if (_engine is null) return null;
        // Slightly larger radius than the visual body for comfortable clicking
        const float hitRadius = (float)ArenaConstants.TankHalfSize + 5f;

        return _engine.Tanks
            .Where(t => t.State.IsAlive)
            .Cast<ISwarmTank?>()
            .FirstOrDefault(t =>
            {
                float dx = (float)t!.State.Position.X - pt.X;
                float dy = (float)t.State.Position.Y - pt.Y;
                return dx * dx + dy * dy <= hitRadius * hitRadius;
            });
    }

    /// <summary>
    /// Displays a context menu for the right-clicked tank.
    /// To add more actions, yield additional <see cref="ToolStripItem"/> objects from
    /// <see cref="BuildTankMenuItems"/>.
    /// </summary>
    private void ShowTankContextMenu(string tankName, Point location)
    {
        var menu = new ContextMenuStrip();

        // Non-clickable header shows the tank name
        menu.Items.Add(new ToolStripMenuItem(tankName) { Enabled = false });
        menu.Items.Add(new ToolStripSeparator());

        foreach (ToolStripItem item in BuildTankMenuItems(tankName))
            menu.Items.Add(item);

        menu.Show(this, location);
    }

    /// <summary>
    /// Returns the set of <see cref="ToolStripItem"/>s that populate the context menu
    /// for a given tank.  Add new items here to extend the menu.
    /// </summary>
    private IEnumerable<ToolStripItem> BuildTankMenuItems(string tankName)
    {
        // ── Info Panel ────────────────────────────────────────────────────────
        bool attached = _attachedPanels.Contains(tankName);
        var panelItem = new ToolStripMenuItem(attached ? "Detach Info Panel" : "Attach Info Panel");
        panelItem.Click += (_, _) => ToggleAttachedPanel(tankName);
        yield return panelItem;
    }

    private void ToggleAttachedPanel(string tankName)
    {
        if (!_attachedPanels.Remove(tankName))
            _attachedPanels.Add(tankName);
        Invalidate();
    }

    // ── Engine wiring ─────────────────────────────────────────────────────────

    private ArenaEngine CreateEngine()
    {
        var engine = new ArenaEngine(ClientSize.Width, ClientSize.Height);
        engine.TickCompleted += Engine_TickCompleted;
        engine.RoundEnded += Engine_RoundEnded;
        engine.SwarmMessageBroadcast += Engine_SwarmMessageBroadcast;
        return engine;
    }

    private void Engine_TickCompleted(object? sender, TickEventArgs e)
    {
        TickCompleted?.Invoke(this, e);
    }

    private void Engine_RoundEnded(object? sender, RoundEndedEventArgs e)
    {
        // Keep the render timer running so burning-hulk animations continue playing.
        // The engine simulation is already stopped; the timer just drives Invalidate calls.
        _statusMessage = $"Round ended after {e.TotalTicks} ticks.";
        Invalidate();
    }

    private void Engine_SwarmMessageBroadcast(SwarmMessage msg, int swarmId)
    {
        if (msg.Type == SwarmMessageType.RadarShare)
        {
            if (msg.RadarContact is not { } rc) return;
            string key = $"{msg.SenderName}|{rc.Name}";
            if (_radarLogLastPos.TryGetValue(key, out Vector2D last))
            {
                float dx = (float)(rc.Position.X - last.X);
                float dy = (float)(rc.Position.Y - last.Y);
                if (dx * dx + dy * dy < RadarLogMinMovePx * RadarLogMinMovePx) return;
            }
            _radarLogLastPos[key] = rc.Position;
        }

        string? line = FormatRadioMessage(msg);
        if (line is null) return;
        RadioTransmission?.Invoke(this, new RadioTransmissionEventArgs(line, swarmId));
    }

    /// <summary>
    /// Converts a <see cref="SwarmMessage"/> to the strict radio format
    /// <c>[MSGTYPE]: content</c>.  Returns <see langword="null"/> for message
    /// types that produce no meaningful radio traffic (e.g. RadarShare).
    /// </summary>
    private static string? FormatRadioMessage(SwarmMessage msg) => msg.Type switch
    {
        SwarmMessageType.EnemySpotted when msg.Position is { } pos
            => $"[TARGET]: {msg.SenderName} contacts {msg.TargetName ?? "unknown"} at {(int)pos.X} {(int)pos.Y}",

        SwarmMessageType.EnemySpotted
            => $"[TARGET]: {msg.SenderName} contacts {msg.TargetName ?? "unknown"}",

        SwarmMessageType.TargetLocked
            => $"[TARGET]: {msg.SenderName} engaging {msg.TargetName ?? "unknown"}",

        SwarmMessageType.RequestBackup
            => $"[ALERT]: {msg.SenderName} requests immediate backup",

        SwarmMessageType.FormationMove when msg.Position is { } pos
            => $"[MOVE]: {msg.SenderName} rally point {(int)pos.X} {(int)pos.Y}",

        SwarmMessageType.FormationMove
            => $"[MOVE]: {msg.SenderName} rally point designated",

        SwarmMessageType.FallBack
            => $"[MOVE]: {msg.SenderName} all units fall back",

        SwarmMessageType.RoleChange
            => $"[STATUS]: {msg.SenderName} assuming {msg.CustomData ?? "new"} role",

        SwarmMessageType.Custom when !string.IsNullOrWhiteSpace(msg.CustomData)
            => $"[STATUS]: {msg.SenderName} {msg.CustomData}",

        SwarmMessageType.RadarShare when msg.RadarContact is { } rc
            => $"[RADAR]: {msg.SenderName} >> {rc.Name} at ({(int)rc.Position.X}, {(int)rc.Position.Y})  hdg {(int)rc.Heading}°  spd {rc.Velocity:F1}  E:{rc.Energy:F0}",

        _ => null
    };

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        DateTime now = DateTime.UtcNow;
        _tickIntervalMs = _gameTimer.Interval;

        int ticksThisFrame = ComputeSimTicksPerFrame();
        bool needsPulseGate = ticksThisFrame == 1; // only gate when not batching

        // Pulse gate: hold simulation until radar-pulse animation completes.
        // Skipped entirely when batching multiple ticks (animations are off).
        if (needsPulseGate && _waitingForPulse)
        {
            if (now < _pulseCompletionTime)
            {
                Invalidate();
                return;
            }
        }
        _waitingForPulse = false; // clear gate (either expired or bypassed)

        bool harvestScans = ShowRadarReflections || ShowScanHalos;

        for (int i = 0; i < ticksThisFrame; i++)
        {
            _lastTickTime = DateTime.UtcNow;
            _engine?.Tick();
            HarvestExplosionBirthTimes();
            if (harvestScans)
                HarvestScanEvents();
        }

        Invalidate();
    }

    /// <summary>
    /// Records the wall-clock birth time of every newly destroyed tank so
    /// <see cref="DrawExplosionBlast"/> can drive a time-based explosion animation.
    /// Using <see cref="_lastTickTime"/> (set at the start of the tick) keeps the
    /// explosion onset tightly synchronised with the simulation pulse even during
    /// the radar pulse-gate pause.
    /// </summary>
    private void HarvestExplosionBirthTimes()
    {
        if (_engine is null) return;

        foreach (ISwarmTank tank in _engine.Tanks)
        {
            if (tank.State.IsAlive) continue;
            _explosionBirthTimes.TryAdd(tank.Name, _lastTickTime);
        }
    }

    /// <summary>
    /// Called once per engine tick (from <see cref="GameTimer_Tick"/>) to harvest physical
    /// radar hits into <see cref="_scanEvents"/>.
    /// each contact fires at most one <see cref="ScanEvent"/> per tick regardless of how many
    /// repaints occur.  A deduplication guard prevents stacking when the same pair is still
    /// live from a previous tick.
    /// <para>
    /// Scanner tanks are processed in deterministic order (by SwarmId then Name). Each scanner
    /// that produces at least one new contact is assigned its own animation slot staggered by
    /// one full <see cref="ScanHaloLifetime"/> so the outbound arc, halo, and echo of tank N
    /// complete before tank N+1 begins. The simulation pulse-gate is extended to cover all
    /// sequential slots.
    /// </para>
    /// </summary>
    private void HarvestScanEvents()
    {
        if (_engine is null) return;

        long tick = _engine.TickNumber;
        float lifetimeMs = ScanHaloLifetime * _tickIntervalMs;

        // Each scanner tank that contributes at least one new event gets its own sequential
        // animation slot. scannerGroupIndex drives both the TickFiredWallTime stagger and the
        // total pulse-gate duration set at the end.
        int scannerGroupIndex = 0;

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
                bool tankAddedEvent = false;

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

                    // True midpoint of [PrevRadarHeading, RadarHeading] along the swept direction.
                    // Using PrevRadarHeading + sweepDelta/2 (signed) handles both CW and CCW
                    // sweeps and the 0/360 wrap correctly.
                    float sweepMidDeg = (float)(((tank.State.PrevRadarHeading + sweepDelta / 2) % 360 + 360) % 360);

                    // All contacts from the same scanner share one slot so they appear together,
                    // but each scanner tank's slot is offset by one full lifetime from the previous.
                    _scanEvents.Add(new ScanEvent(
                        TickFired:          tick,
                        TickFiredWallTime:  _lastTickTime.AddMilliseconds(scannerGroupIndex * lifetimeMs),
                        SpotterName:        tank.Name,
                        SpotterPosition: tank.State.Position,
                        Position:        contact.Position,
                        EnemySwarmId:    contact.EnemySwarmId,
                        EnemyName:       contact.Name,
                        SpotterColor:    spotterColor,
                        EnergyFraction:  energyFraction,
                        Velocity:        contact.VelocityVector,
                        SweepSpanDeg:    sweepSpan,
                        SweepMidDeg:     sweepMidDeg));

                    tankAddedEvent = true;
                }

                // Only advance the slot when this scanner actually contributed new events.
                if (tankAddedEvent)
                    scannerGroupIndex++;
            }
        }

        // Gate the next simulation tick until the last sequential animation has fully played out.
        if (scannerGroupIndex > 0)
        {
            _pulseCompletionTime = _lastTickTime.AddMilliseconds(scannerGroupIndex * lifetimeMs);
            _waitingForPulse = true;
        }
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = ShowAntiAliasing ? SmoothingMode.AntiAlias : SmoothingMode.None;

        DrawBackground(g);

        if (_engine is null)
        {
            DrawCentredText(g, _statusMessage, Font, Brushes.Gray);
            return;
        }

        if (_focusedTank is not null)
        {
            DrawSensorView(g);
        }
        else
        {
            // Layer 1 – Radar reflections: beam from spotter to detected tank
            if (ShowRadarReflections) DrawAllRadarHalos(g);

            // Layer 2 – Charred hulk bodies. Suppress for the first 160 ms while the white
            // flash is bright enough to cover the hull; visible permanently thereafter.
            foreach (ISwarmTank tank in _engine.Tanks)
            {
                if (!tank.State.IsAlive)
                {
                    float hullAgeMs = _explosionBirthTimes.TryGetValue(tank.Name, out DateTime hbt)
                        ? (float)(DateTime.UtcNow - hbt).TotalMilliseconds
                        : float.MaxValue;
                    if (ShowExplosions && hullAgeMs > 160f)
                        DrawHulkBody(g, tank.State);
                }
            }

            // Layer 3 – Bullets (coloured by the firing tank's swarm)
            Dictionary<string, Color> bulletOwnerColors = _engine.Tanks
                .ToDictionary(
                    t => t.Name,
                    t => SwarmColours[Math.Abs(t.SwarmId) % SwarmColours.Length],
                    StringComparer.Ordinal);

            if (ShowBullets)
                foreach (BulletState bullet in _engine.Bullets)
                    DrawBullet(g, bullet, bulletOwnerColors);

            // Layer 4 – Living tanks
            foreach (ISwarmTank tank in _engine.Tanks)
            {
                if (tank.State.IsAlive)
                    DrawTank(g, tank.State);
            }

            // Layer 5 – Explosion blasts and burning flames (on top of everything for impact)
            foreach (ISwarmTank tank in _engine.Tanks)
            {
                if (!tank.State.IsAlive)
                {
                    float ageMs = _explosionBirthTimes.TryGetValue(tank.Name, out DateTime bt)
                        ? (float)(DateTime.UtcNow - bt).TotalMilliseconds
                        : float.MaxValue;

                    if (ShowExplosions && ageMs < ExplosionDurationMs)
                        DrawExplosionBlast(g, tank.State, ageMs);

                    if (ShowExplosions && ageMs > BurnStartMs)
                    {
                        float intensity = Math.Clamp((ageMs - BurnStartMs) / BurnRampMs, 0f, 1f);
                        DrawBurningFlame(g, tank.State, intensity);
                    }
                }
            }
        }

        if (ShowHud) DrawHud(g);

        // Layer 6 – Attached info panels (floats above all other content)
        if (ShowInfoPanels) DrawAttachedPanels(g);

        if (!string.IsNullOrEmpty(_statusMessage))
            DrawCentredText(g, _statusMessage, new Font(Font.FontFamily, 14, FontStyle.Bold), Brushes.White);
    }

    // ── Sensor view (LMB held on tank) ───────────────────────────────────────

    /// <summary>
    /// Replaces the normal rendering with a "what does this tank know?" view.
    /// Shows the focused tank fully, and every <see cref="RadarContact"/> in its
    /// <see cref="ISwarmTank.RadarMap"/> as a ghost at the last-known position.
    /// Contacts fade with staleness; allies are distinguished from enemies.
    /// Bullets and other tanks are hidden — the tank has no knowledge of them.
    /// </summary>
    private void DrawSensorView(Graphics g)
    {
        ISwarmTank focused = _focusedTank!;
        long currentTick = _engine!.TickNumber;

        // Ghost contacts behind the focused tank.
        // A contact is "direct" when the focused tank's own radar made the sighting;
        // otherwise it arrived via a swarm ally's RadarShare broadcast.
        foreach (RadarContact contact in focused.RadarMap.Values)
        {
            bool direct = string.Equals(contact.SpottedBy, focused.Name, StringComparison.Ordinal);
            DrawGhostContact(g, contact, currentTick, direct);
        }

        // Focused tank: full rendering + its own effects if dead
        if (focused.State.IsAlive)
        {
            DrawTank(g, focused.State);
        }
        else
        {
            float ageMs = _explosionBirthTimes.TryGetValue(focused.Name, out DateTime bt)
                ? (float)(DateTime.UtcNow - bt).TotalMilliseconds
                : float.MaxValue;
            if (ageMs > 160f)
                DrawHulkBody(g, focused.State);
            if (ageMs < ExplosionDurationMs)
                DrawExplosionBlast(g, focused.State, ageMs);
            if (ageMs > BurnStartMs)
            {
                float intensity = Math.Clamp((ageMs - BurnStartMs) / BurnRampMs, 0f, 1f);
                DrawBurningFlame(g, focused.State, intensity);
            }
        }
    }

    /// <summary>
    /// Draws a ghost silhouette of a radar contact at its last-known position.
    /// Ally contacts are rendered brighter and in their swarm colour.
    /// Enemy contacts use a dimmer ghost style.
    /// Both fade as the contact grows stale (based on tick age).
    /// When <paramref name="directRadarContact"/> is <see langword="true"/> the focused
    /// tank scanned this contact itself; a pulsing radar halo is drawn to mark it.
    /// Contacts known only through ally intelligence get no halo.
    /// </summary>
    private void DrawGhostContact(Graphics g, RadarContact contact, long currentTick,
                                  bool directRadarContact)
    {
        float cx = (float)contact.Position.X;
        float cy = (float)contact.Position.Y;

        // Fade linearly from fresh (1.0) to floor (0.15) over 50 ticks of staleness.
        long ticksAgo  = Math.Max(0, currentTick - contact.Timestamp);
        float freshness = Math.Max(0.15f, 1f - ticksAgo / 50f);

        Color contactColor = SwarmColours[Math.Abs(contact.EnemySwarmId) % SwarmColours.Length];
        int A(int baseAlpha) => (int)(baseAlpha * freshness);

        // ── Ghost body ────────────────────────────────────────────────────────
        GraphicsState saved = g.Save();
        g.TranslateTransform(cx, cy);
        g.RotateTransform((float)contact.Heading);

        int half = TankBodySize / 2;
        int bodyAlpha  = contact.IsAlly ? 110 : 70;
        int borderAlpha = contact.IsAlly ? 210 : 160;

        using SolidBrush bodyBrush  = new(Color.FromArgb(A(bodyAlpha),  contactColor));
        using Pen        bodyPen    = new(Color.FromArgb(A(borderAlpha), contactColor), 1.5f);
        g.FillRectangle(bodyBrush, -half, -half, TankBodySize, TankBodySize);
        g.DrawRectangle(bodyPen,   -half, -half, TankBodySize, TankBodySize);

        // Gun stub at last-known gun heading (relative to body)
        g.RotateTransform(-(float)contact.Heading);
        float gunRad = (float)(contact.Heading * Math.PI / 180.0);  // re-derive absolute
        float gdx = (float)Math.Sin(gunRad) * (GunLength * 0.7f);
        float gdy = -(float)Math.Cos(gunRad) * (GunLength * 0.7f);
        using Pen gunPen = new(Color.FromArgb(A(120), Color.LightGray), 2f);
        g.DrawLine(gunPen, 0, 0, gdx, gdy);

        g.Restore(saved);

        // ── Energy bar ────────────────────────────────────────────────────────
        float barX = cx - EnergyBarWidth / 2f;
        float barY = cy - TankBodySize / 2f - 10;
        float energyFraction = (float)Math.Clamp(contact.Energy / ArenaConstants.TankStartEnergy, 0, 1);
        using SolidBrush emptyBrush  = new(Color.FromArgb(A(80),  Color.DarkRed));
        using SolidBrush energyBrush = new(Color.FromArgb(A(150), Color.LawnGreen));
        g.FillRectangle(emptyBrush,  barX, barY, EnergyBarWidth, EnergyBarHeight);
        g.FillRectangle(energyBrush, barX, barY, EnergyBarWidth * energyFraction, EnergyBarHeight);

        // ── Name + staleness label ────────────────────────────────────────────
        using Font nameFont = new(Font.FontFamily, 7f);
        string label = ticksAgo == 0
            ? contact.Name
            : $"{contact.Name}  -{ticksAgo}t";
        SizeF textSize = g.MeasureString(label, nameFont);
        using SolidBrush textBrush = new(Color.FromArgb(A(190), contactColor));
        g.DrawString(label, nameFont, textBrush,
            cx - textSize.Width / 2f, barY - textSize.Height - 1);

        // ── Direct radar halo — own radar only, not relayed intelligence ──────
        if (directRadarContact)
            DrawDirectRadarHalo(g, cx, cy, contactColor, freshness);
    }

    /// <summary>
    /// Draws a pulsing sonar-ring halo around a contact that the focused tank
    /// scanned with its own radar (as opposed to receiving via swarm intelligence).
    /// The ring oscillates in radius and brightness to make it unmistakably "live".
    /// </summary>
    private static void DrawDirectRadarHalo(Graphics g, float cx, float cy,
                                            Color color, float freshness)
    {
        // Wall-clock pulse: full cycle every 1.4 s, independent of tick rate.
        float phase = (float)(DateTime.UtcNow.Ticks % (long)(TimeSpan.TicksPerSecond * 1.4))
                      / (float)(TimeSpan.TicksPerSecond * 1.4);
        float pulse = 0.5f + 0.5f * MathF.Sin(phase * MathF.PI * 2f);  // 0 → 1 → 0

        // Radius breathes between 16 and 24 px
        float ringR  = 16f + 8f * pulse;
        float glowR  = ringR + 5f;

        int ringAlpha = (int)(200 * freshness * (0.55f + 0.45f * pulse));
        int glowAlpha = (int)( 60 * freshness * (0.55f + 0.45f * pulse));

        // Inner crisp ring
        using Pen ringPen = new(Color.FromArgb(ringAlpha, LightenColor(color, 60)), 1.8f);
        g.DrawEllipse(ringPen, cx - ringR, cy - ringR, ringR * 2, ringR * 2);

        // Outer soft glow
        using Pen glowPen = new(Color.FromArgb(glowAlpha, color), 5f);
        g.DrawEllipse(glowPen, cx - glowR, cy - glowR, glowR * 2, glowR * 2);

        // 4-spoke tick marks at cardinal points of the ring
        float tickLen = 4f;
        using Pen tickPen = new(Color.FromArgb(ringAlpha, LightenColor(color, 80)), 1.2f);
        foreach (float angleDeg in new[] { 0f, 90f, 180f, 270f })
        {
            float rad = angleDeg * MathF.PI / 180f;
            float ix  = cx + MathF.Cos(rad) * ringR;
            float iy  = cy + MathF.Sin(rad) * ringR;
            float ox  = cx + MathF.Cos(rad) * (ringR + tickLen);
            float oy  = cy + MathF.Sin(rad) * (ringR + tickLen);
            g.DrawLine(tickPen, ix, iy, ox, oy);
        }
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

        // Drawn first so the glow ring sits behind the hull
        if (ShowScanHalos) DrawScanHalo(g, tank, x, y);

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
        if (ShowRadarSweepTrails) DrawRadarSweepTrail(g, tank, tankColor);

        g.Restore(saved);

        if (ShowEnergyBars)
        {
            // --- Energy bar ---
            float barX = x - EnergyBarWidth / 2f;
            float barY = y - TankBodySize / 2f - 10;
            float energyFraction = (float)Math.Clamp(tank.Energy / ArenaConstants.TankStartEnergy, 0, 1);
            g.FillRectangle(Brushes.DarkRed, barX, barY, EnergyBarWidth, EnergyBarHeight);
            using SolidBrush energyBrush = new(Color.LawnGreen);
            g.FillRectangle(energyBrush, barX, barY, EnergyBarWidth * energyFraction, EnergyBarHeight);

            if (ShowTankLabels)
            {
                // --- Name label ---
                using Font nameFont = new(Font.FontFamily, 7);
                SizeF textSize = g.MeasureString(tank.Name, nameFont);
                g.DrawString(tank.Name, nameFont, Brushes.LightGray,
                             x - textSize.Width / 2, barY - textSize.Height - 1);
            }
        }
        else if (ShowTankLabels)
        {
            // --- Name label (above hull when no energy bar) ---
            float labelY = y - TankBodySize / 2f - 12;
            using Font nameFont = new(Font.FontFamily, 7);
            SizeF textSize = g.MeasureString(tank.Name, nameFont);
            g.DrawString(tank.Name, nameFont, Brushes.LightGray,
                         x - textSize.Width / 2, labelY - textSize.Height);
        }
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
        // Arena heading: 0° = north, clockwise → GDI+ start = heading − 90.
        // Arc width per frame = actual sweep delta between this frame and the one before it,
        // so the painted sectors faithfully mirror what the engine swept each tick.
        double[] history = [.. trail];
        int count = history.Length;

        for (int i = 0; i < count; i++)
        {
            float ageFraction = (float)(i + 1) / count; // 0 = oldest, 1 = newest
            int alpha = (int)(100 * ageFraction);
            if (alpha < 5) continue;

            // Compute the signed delta between the previous recorded heading and this one.
            // For the oldest frame (i == 0) there is no predecessor — use a minimal 1° arc.
            float spanDeg;
            if (i == 0)
            {
                spanDeg = 1f;
            }
            else
            {
                double d = history[i] - history[i - 1];
                while (d >  180) d -= 360;
                while (d < -180) d += 360;
                spanDeg = Math.Max(1f, (float)Math.Abs(d));
            }

            // GDI+ arc starts at the trailing edge of the sweep (smaller angle for CW, larger for CCW).
            // We always paint CW in GDI+ with a positive span, so the start is heading - span.
            float gdiStart = (float)history[i] - 90f - spanDeg;
            using SolidBrush fadeBrush = new(Color.FromArgb(alpha, swarmColor));
            g.FillPie(fadeBrush,
                -RadarLength, -RadarLength,
                RadarLength * 2, RadarLength * 2,
                gdiStart, spanDeg);
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

    private static void DrawBullet(Graphics g, BulletState bullet, Dictionary<string, Color> ownerColors)
    {
        float bx = (float)bullet.Position.X;
        float by = (float)bullet.Position.Y;

        Color bulletColor = ownerColors.TryGetValue(bullet.OwnerName, out Color c)
            ? LightenColor(c, 80)
            : Color.Yellow;

        // Arrow dimensions scale with power
        float headLen  = (float)(5.0 + bullet.Power * 3.0);   // tip to base of arrowhead
        float headHalf = (float)(2.0 + bullet.Power * 1.2);   // half-width of arrowhead base
        float tailLen  = (float)(3.0 + bullet.Power * 2.0);   // shaft behind the arrowhead

        // heading: 0 = north (−Y), clockwise → convert to radians pointing up
        double rad = (bullet.Heading - 90.0) * Math.PI / 180.0;
        float dx = (float)Math.Cos(rad);   // unit vector in travel direction
        float dy = (float)Math.Sin(rad);
        float px = -dy;                     // perpendicular (left)
        float py =  dx;

        // Arrowhead triangle: tip, left base corner, right base corner
        PointF tip   = new(bx + dx * headLen,  by + dy * headLen);
        PointF baseL = new(bx + px * headHalf, by + py * headHalf);
        PointF baseR = new(bx - px * headHalf, by - py * headHalf);

        using SolidBrush arrowBrush = new(bulletColor);
        g.FillPolygon(arrowBrush, [tip, baseL, baseR]);

        // Tail shaft
        PointF tailEnd = new(bx - dx * tailLen, by - dy * tailLen);
        using Pen tailPen = new(Color.FromArgb(200, bulletColor), 1.2f);
        g.DrawLine(tailPen, bx, by, tailEnd.X, tailEnd.Y);

        // Glow behind the arrowhead
        using SolidBrush glowBrush = new(Color.FromArgb(60, bulletColor));
        float glow = headLen * 1.4f;
        g.FillEllipse(glowBrush, bx - glow * 0.5f, by - glow * 0.5f, glow, glow);
    }

    /// <summary>
    /// Draws the static charred wreck at the tank's death position.
    /// Called first so the hull sits below all effects in every render pass.
    /// </summary>
    private void DrawHulkBody(Graphics g, TankState tank)
    {
        float x = (float)tank.Position.X;
        float y = (float)tank.Position.Y;

        GraphicsState saved = g.Save();
        g.TranslateTransform(x, y);
        g.RotateTransform((float)tank.Heading);

        int half = TankBodySize / 2;

        // Scorched ground shadow
        using SolidBrush shadowBrush = new(Color.FromArgb(60, Color.DarkRed));
        g.FillEllipse(shadowBrush, -half - 4, -half - 4, (half + 4) * 2, (half + 4) * 2);

        // Charred tank body
        using SolidBrush hullBrush = new(Color.FromArgb(55, 45, 40));
        using Pen hullPen = new(Color.FromArgb(90, 70, 60), 1);
        g.FillRectangle(hullBrush, -half, -half, TankBodySize, TankBodySize);
        g.DrawRectangle(hullPen, -half, -half, TankBodySize, TankBodySize);

        // Tread remnants
        using Pen treadPen = new(Color.FromArgb(35, 30, 25), 2);
        g.DrawLine(treadPen, -half, -half + 3, -half, half - 3);
        g.DrawLine(treadPen,  half, -half + 3,  half, half - 3);

        // Broken gun stub
        g.RotateTransform(-(float)tank.Heading);
        g.RotateTransform((float)tank.GunHeading);
        using Pen gunStubPen = new(Color.FromArgb(70, 65, 60), 3);
        g.DrawLine(gunStubPen, 0, 0, 0, -(int)(GunLength * 0.55f));
        g.RotateTransform(-(float)tank.GunHeading);

        g.Restore(saved);
    }

    /// <summary>
    /// Draws animated burning flames and smoke above a charred hulk.
    /// <paramref name="intensity"/> is 0→1: 0 = just kindling, 1 = full steady burn.
    /// The transform is applied in world space (no saved-state rotation).
    /// </summary>
    private void DrawBurningFlame(Graphics g, TankState tank, float intensity)
    {
        float x = (float)tank.Position.X;
        float y = (float)tank.Position.Y;

        float phase = (float)(DateTime.UtcNow.Ticks % (TimeSpan.TicksPerSecond * 2))
                      / (float)(TimeSpan.TicksPerSecond * 2) * MathF.PI * 2f;

        GraphicsState saved = g.Save();
        g.TranslateTransform(x, y);

        // 3 flame tongues
        float[] flameOffX  = [-3f,  0f,  3f];
        float[] flamePhase = [ 0f, 0.7f, 1.4f];
        float[] flameMaxH  = [10f, 14f,  9f];

        for (int i = 0; i < 3; i++)
        {
            float fp    = phase + flamePhase[i];
            float h     = flameMaxH[i] * intensity * (0.6f + 0.4f * MathF.Sin(fp));
            float wobX  = flameOffX[i] + 2.5f * MathF.Sin(fp * 1.3f);
            float baseW = (4f + 2f * MathF.Sin(fp * 0.9f)) * intensity;

            using var flamePath = new GraphicsPath();
            flamePath.AddPolygon([
                new PointF(wobX - baseW, 0),
                new PointF(wobX + baseW, 0),
                new PointF(wobX, -h)
            ]);
            int alpha = (int)(200 * intensity * (0.7f + 0.3f * MathF.Sin(fp)));
            using SolidBrush flameBrush = new(Color.FromArgb(alpha, 220, 80, 0));
            g.FillPath(flameBrush, flamePath);

            float innerH = h * 0.55f;
            float innerW = baseW * 0.5f;
            using var corePath = new GraphicsPath();
            corePath.AddPolygon([
                new PointF(wobX - innerW, 0),
                new PointF(wobX + innerW, 0),
                new PointF(wobX, -innerH)
            ]);
            using SolidBrush coreBrush = new(Color.FromArgb(alpha, 255, 220, 50));
            g.FillPath(coreBrush, corePath);
        }

        // 2 smoke puffs drifting upward
        for (int i = 0; i < 2; i++)
        {
            float sp    = phase + i * 1.1f;
            float drift = 6f * MathF.Sin(sp * 0.7f);
            float rise  = -(14f + 7f * i + 4f * MathF.Sin(sp));
            float r     = 5f + 3f * MathF.Sin(sp * 0.5f);
            int smokeAlpha = (int)(70 * intensity * (0.5f + 0.5f * MathF.Sin(sp * 0.8f)));
            using SolidBrush smokeBrush = new(Color.FromArgb(smokeAlpha, 180, 170, 160));
            g.FillEllipse(smokeBrush, drift - r, rise - r, r * 2, r * 2);
        }

        g.Restore(saved);
    }

    /// <summary>
    /// Renders the one-shot explosion sequence that plays when a tank is first destroyed.
    /// All phases run in world-space coordinates (no graphics transform applied on entry).
    /// <list type="bullet">
    ///   <item>0–250 ms — white flash blast expanding outward.</item>
    ///   <item>0–700 ms — orange-red fireball with bright yellow core.</item>
    ///   <item>100–900 ms — thin shockwave ring racing outward.</item>
    ///   <item>150–1600 ms — 10 debris particles flying outward with slight gravity.</item>
    ///   <item>300–2100 ms — 5 smoke-cloud puffs rising and fading.</item>
    /// </list>
    /// </summary>
    private static void DrawExplosionBlast(Graphics g, TankState tank, float ageMs)
    {
        float x = (float)tank.Position.X;
        float y = (float)tank.Position.Y;

        // ── White flash (0-250 ms) ────────────────────────────────────────────
        if (ageMs < 250f)
        {
            float t    = ageMs / 250f;
            float fade = t < 0.18f ? t / 0.18f : 1f - (t - 0.18f) / 0.82f;
            fade = Math.Clamp(fade, 0f, 1f);
            float r    = 60f * t;
            using SolidBrush flashBrush = new(Color.FromArgb((int)(255 * fade), Color.White));
            g.FillEllipse(flashBrush, x - r, y - r, r * 2, r * 2);
        }

        // ── Fireball (0-700 ms) ───────────────────────────────────────────────
        if (ageMs < 700f)
        {
            float t    = ageMs / 700f;
            float fade = t < 0.14f ? t / 0.14f : 1f - (t - 0.14f) / 0.86f;
            fade = MathF.Pow(Math.Clamp(fade, 0f, 1f), 0.65f);
            float r    = 44f * MathF.Sqrt(t);

            using SolidBrush outerBrush = new(Color.FromArgb((int)(215 * fade), 230, 60, 10));
            g.FillEllipse(outerBrush, x - r, y - r, r * 2, r * 2);

            float ir = r * 0.52f;
            using SolidBrush innerBrush = new(Color.FromArgb((int)(240 * fade), 255, 185, 30));
            g.FillEllipse(innerBrush, x - ir, y - ir, ir * 2, ir * 2);
        }

        // ── Shockwave ring (100-900 ms) ───────────────────────────────────────
        if (ageMs >= 100f && ageMs < 900f)
        {
            float t    = (ageMs - 100f) / 800f;
            float r    = 10f + 72f * t;
            float fade = (1f - t) * (1f - t);
            int   a    = (int)(200 * fade);
            float penW = Math.Max(0.8f, 2.8f * (1f - t));
            using Pen ringPen = new(Color.FromArgb(a, 255, 200, 80), penW);
            g.DrawEllipse(ringPen, x - r, y - r, r * 2, r * 2);

            // Fainter trailing ring slightly behind
            if (t < 0.65f)
            {
                float r2 = 10f + 58f * t;
                int   a2 = (int)(90 * (1f - t / 0.65f));
                using Pen ring2 = new(Color.FromArgb(a2, 255, 140, 40), 1f);
                g.DrawEllipse(ring2, x - r2, y - r2, r2 * 2, r2 * 2);
            }
        }

        // ── Debris particles (150-1600 ms) ────────────────────────────────────
        if (ageMs >= 150f && ageMs < 1600f)
        {
            float t = (ageMs - 150f) / 1450f;
            // Stable per-tank random so particles stay on the same trajectories across frames.
            var rng = new Random(tank.Name.GetHashCode());

            for (int i = 0; i < 10; i++)
            {
                float angle = rng.NextSingle() * MathF.PI * 2f;
                float speed = 20f + rng.NextSingle() * 22f;
                float size  = 1.5f + rng.NextSingle() * 2.5f;

                float dist = speed * MathF.Sqrt(t);         // decelerate via sqrt curve
                float px   = x + MathF.Cos(angle) * dist;
                float py   = y + MathF.Sin(angle) * dist + 8f * t * t;  // gravity

                float fade = 1f - t;
                int   a    = (int)(220 * fade * fade);
                if (a < 5) continue;

                Color dc = t < 0.28f
                    ? Color.FromArgb(a, 255, (int)(140 * (1f - t * 3f)), 20)   // hot orange
                    : Color.FromArgb(a, 75, 65, 55);                           // dark char

                using SolidBrush db = new(dc);
                g.FillEllipse(db, px - size, py - size, size * 2, size * 2);
            }
        }

        // ── Rising smoke cloud (300-2100 ms) ─────────────────────────────────
        if (ageMs >= 300f)
        {
            float t    = Math.Clamp((ageMs - 300f) / 1800f, 0f, 1f);
            float fade = t < 0.35f ? t / 0.35f : 1f - (t - 0.35f) / 0.65f;
            fade = Math.Clamp(fade, 0f, 1f);

            var rng = new Random(tank.Name.GetHashCode() ^ 0x5A5A5A5A);

            for (int i = 0; i < 6; i++)
            {
                float angle = rng.NextSingle() * MathF.PI * 2f;
                float drift = (2f + rng.NextSingle() * 7f) * MathF.Sqrt(t);
                float cx    = x + MathF.Cos(angle) * drift;
                float cy    = y + MathF.Sin(angle) * drift - (12f + i * 5f) * t;
                float r     = (5f + rng.NextSingle() * 7f) + 16f * t;
                int   a     = (int)(75 * fade * (0.55f + 0.45f * rng.NextSingle()));
                using SolidBrush sb = new(Color.FromArgb(a, 105, 98, 92));
                g.FillEllipse(sb, cx - r, cy - r, r * 2, r * 2);
            }
        }
    }

    // ── Attached info panels ──────────────────────────────────────────────────

    private void DrawAttachedPanels(Graphics g)
    {
        if (_engine is null || _attachedPanels.Count == 0) return;

        _panelBounds.Clear();
        _closeBtnBounds.Clear();
        var byName = _engine.Tanks.ToDictionary(t => t.Name, StringComparer.Ordinal);

        foreach (string name in _attachedPanels)
        {
            if (!byName.TryGetValue(name, out ISwarmTank? tank)) continue;

            bool hovered = string.Equals(name, _hoveredPanelName, StringComparison.Ordinal);
            float opacity = hovered ? 1f : 0.4f;

            DrawTankPanel(g, tank.State, opacity, out RectangleF bounds, out RectangleF closeBtn);
            _panelBounds[name]    = bounds;
            _closeBtnBounds[name] = closeBtn;
        }
    }

    /// <summary>
    /// Renders a floating info panel anchored to the given tank's current position.
    /// The panel lists all key state fields and updates automatically each paint cycle.
    /// <paramref name="opacity"/> scales every alpha channel uniformly (1 = fully opaque,
    /// 0.4 = 60 % translucent default).
    /// </summary>
    private void DrawTankPanel(Graphics g, TankState tank, float opacity, out RectangleF panelBounds, out RectangleF closeBtnBounds)
    {
        // Inline helper: scales a base alpha by the panel opacity
        int A(int baseAlpha) => (int)(baseAlpha * opacity);

        Color swarmColor = SwarmColours[Math.Abs(tank.SwarmId) % SwarmColours.Length];
        float tx = (float)tank.Position.X;
        float ty = (float)tank.Position.Y;

        // Ordered rows displayed in the panel body (label, value)
        (string Label, string Value)[] rows =
        [
            ("Swarm",    tank.SwarmId.ToString()),
            ("Role",     tank.Role.ToString()),
            ("Energy",   $"{tank.Energy:F1}"),
            ("Pos",      $"{tank.Position.X:F0}, {tank.Position.Y:F0}"),
            ("Heading",  $"{tank.Heading:F1}°"),
            ("Gun",      $"{tank.GunHeading:F1}°"),
            ("Radar",    $"{tank.RadarHeading:F1}°"),
            ("Velocity", $"{tank.Velocity:F2} px/t"),
            ("Status",   tank.IsAlive ? "Alive" : "Dead"),
        ];

        // Header row + small gap + separator + data rows
        int panelHeight = InfoPanelPadY * 2 + InfoPanelRowH + 3 + rows.Length * InfoPanelRowH;

        // Position panel to the right of the tank, clamped inside the arena
        float px = Math.Clamp(tx + InfoPanelOffsetX, 2, ClientSize.Width  - InfoPanelWidth - 2);
        float py = Math.Clamp(ty + InfoPanelOffsetY, 2, ClientSize.Height - panelHeight    - 2);

        panelBounds = new RectangleF(px, py, InfoPanelWidth, panelHeight);

        // ── Background ───────────────────────────────────────────────────────
        using SolidBrush bgBrush = new(Color.FromArgb(A(210), 12, 14, 18));
        g.FillRectangle(bgBrush, px, py, InfoPanelWidth, panelHeight);

        // ── Border in swarm colour ────────────────────────────────────────────
        using Pen borderPen = new(Color.FromArgb(A(200), swarmColor), 1.5f);
        g.DrawRectangle(borderPen, px, py, InfoPanelWidth, panelHeight);

        // ── Dotted connector line to the tank centre ──────────────────────────
        using Pen connectorPen = new(Color.FromArgb(A(70), swarmColor), 1f)
        {
            DashStyle   = DashStyle.Dot,
            DashPattern = [1f, 3f]
        };
        // Attach the connector to the nearest horizontal edge of the panel
        float attachX = tx < px ? px : px + InfoPanelWidth;
        float attachY = py + panelHeight / 2f;
        g.DrawLine(connectorPen, tx, ty, attachX, attachY);

        // ── Header: tank name ─────────────────────────────────────────────────
        float fy = py + InfoPanelPadY;
        float fx = px + InfoPanelPadX;

        using Font headerFont = new(Font.FontFamily, 7.5f, FontStyle.Bold);
        using Font dataFont   = new(Font.FontFamily, 7f);
        using SolidBrush nameBrush  = new(Color.FromArgb(A(255), swarmColor));
        using SolidBrush labelBrush = new(Color.FromArgb(A(175), Color.LightGray));
        using SolidBrush valueBrush = new(Color.FromArgb(A(255), Color.White));

        g.DrawString(tank.Name, headerFont, nameBrush, fx, fy);

        // ── Close (×) button in upper-right corner ────────────────────────────
        const float BtnSize = 13f;
        const float BtnMargin = 3f;
        float bx = px + InfoPanelWidth - BtnSize - BtnMargin;
        float by_ = py + BtnMargin;
        closeBtnBounds = new RectangleF(bx, by_, BtnSize, BtnSize);

        using SolidBrush closeBg = new(Color.FromArgb(A(180), 80, 20, 20));
        g.FillRectangle(closeBg, closeBtnBounds);
        float xm = 3f;
        using Pen xPen = new(Color.FromArgb(A(230), Color.White), 1.5f);
        g.DrawLine(xPen, bx + xm, by_ + xm, bx + BtnSize - xm, by_ + BtnSize - xm);
        g.DrawLine(xPen, bx + BtnSize - xm, by_ + xm, bx + xm, by_ + BtnSize - xm);

        fy += InfoPanelRowH + 1;

        // Thin separator under the name
        using Pen sepPen = new(Color.FromArgb(A(55), swarmColor), 1f);
        g.DrawLine(sepPen, px + 3, fy, px + InfoPanelWidth - 3, fy);
        fy += 3;

        // ── Data rows (label left, value right-aligned) ───────────────────────
        using StringFormat rightAlign = new() { Alignment = StringAlignment.Far };
        foreach ((string label, string value) in rows)
        {
            g.DrawString(label, dataFont, labelBrush, fx, fy);
            g.DrawString(value, dataFont, valueBrush,
                new RectangleF(px, fy, InfoPanelWidth - InfoPanelPadX, InfoPanelRowH),
                rightAlign);
            fy += InfoPanelRowH;
        }
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

        // Radar legend (bottom-left) — hidden during sensor view (irrelevant context)
        if (_focusedTank is null)
            DrawRadarLegend(g);

        // Sensor-view mode banner — centred at bottom
        if (_focusedTank is not null)
        {
            string label = _pinnedTank is not null
                ? $"SENSOR VIEW  ·  {_focusedTank.Name}  ·  double-click to unpin"
                : $"SENSOR VIEW  ·  {_focusedTank.Name}  ·  hold LMB";
            using Font sensorFont = new(Font.FontFamily, 8.5f, FontStyle.Bold);
            SizeF sz = g.MeasureString(label, sensorFont);
            float lx = (ClientSize.Width  - sz.Width)  / 2f;
            float ly =  ClientSize.Height - sz.Height  - 8f;
            Color bannerColor = SwarmColours[Math.Abs(_focusedTank.SwarmId) % SwarmColours.Length];
            using SolidBrush bannerBrush = new(Color.FromArgb(200, bannerColor));
            g.DrawString(label, sensorFont, bannerBrush, lx, ly);
        }
    }

    private void DrawCentredText(Graphics g, string text, Font font, Brush brush)
    {
        SizeF size = g.MeasureString(text, font);
        float cx = (ClientSize.Width - size.Width) / 2f;
        float cy = (ClientSize.Height - size.Height) / 2f;
        g.DrawString(text, font, brush, cx, cy);
    }

    /// <summary>
    /// Renders a brilliant point-flash halo at the moment the outbound radar wavefront
    /// physically reaches the detected tank (<c>ageFraction == 0.5</c>).
    /// <para>
    /// The flash is built from five concentric radial-bloom layers — wide spotter-colour
    /// outer glow, a tighter white-shifted corona, a near-white inner fill, a crisp ring
    /// at the hull edge, and a pure white-hot core — topped by an 8-spoke starburst that
    /// sells the "brilliant flash of light" look.  The flash peaks instantly then snaps
    /// off with a quintic decay so it never lingers.
    /// </para>
    /// </summary>
    private void DrawScanHalo(Graphics g, TankState tank, float cx, float cy)
    {
        if (_scanEvents.Count == 0) return;

        ScanEvent? best = _scanEvents
            .Where(ev => string.Equals(ev.EnemyName, tank.Name, StringComparison.Ordinal))
            .Cast<ScanEvent?>()
            .MaxBy(ev => ev!.Value.TickFiredWallTime);

        if (best is null) return;

        ScanEvent hit     = best.Value;
        float lifetimeMs  = ScanHaloLifetime * _tickIntervalMs;
        float elapsedMs   = (float)(DateTime.UtcNow - hit.TickFiredWallTime).TotalMilliseconds;
        float ageFraction = Math.Clamp(elapsedMs / lifetimeMs, 0f, 1f);

        // Wave arrives at target when ageFraction == 0.5.
        // Flash peaks instantly at contact then snaps off via quintic decay.
        const float arrival = 0.5f;
        const float preWin  = 0.03f;  // 3 % of lifetime — near-instant ramp to full brightness
        const float postWin = 0.08f;  // 8 % of lifetime — fast snap-off after peak

        float fade;
        if (ageFraction < arrival - preWin || ageFraction > arrival + postWin)
            return;
        else if (ageFraction <= arrival)
            fade = (ageFraction - (arrival - preWin)) / preWin;  // linear 0→1 ramp-up
        else
        {
            float t = (ageFraction - arrival) / postWin;         // 0→1 post-contact
            fade = 1f - t * t * t * t * t;                       // quintic — near-instant collapse
        }

        fade = Math.Clamp(fade, 0f, 1f);
        if (fade < 0.02f) return;

        float hull = TankBodySize / 2f;

        // ── Layer 1: wide outer bloom in spotter colour ───────────────────────
        float bloomR = hull + 16f;
        using SolidBrush bloomBrush = new(Color.FromArgb((int)(110 * fade), hit.SpotterColor));
        g.FillEllipse(bloomBrush, cx - bloomR, cy - bloomR, bloomR * 2, bloomR * 2);

        // ── Layer 2: mid corona, white-shifted spotter colour ─────────────────
        float coronaR = hull + 9f;
        using SolidBrush coronaBrush = new(Color.FromArgb((int)(185 * fade), LightenColor(hit.SpotterColor, 100)));
        g.FillEllipse(coronaBrush, cx - coronaR, cy - coronaR, coronaR * 2, coronaR * 2);

        // ── Layer 3: tight inner fill, near-white ─────────────────────────────
        float innerFillR = hull + 4f;
        using SolidBrush innerBrush = new(Color.FromArgb((int)(220 * fade), LightenColor(hit.SpotterColor, 180)));
        g.FillEllipse(innerBrush, cx - innerFillR, cy - innerFillR, innerFillR * 2, innerFillR * 2);

        // ── Crisp ring at hull edge ───────────────────────────────────────────
        float ringR = hull + 3f;
        using Pen ringPen = new(Color.FromArgb((int)(255 * fade), LightenColor(hit.SpotterColor, 150)), 2.5f);
        g.DrawEllipse(ringPen, cx - ringR, cy - ringR, ringR * 2, ringR * 2);

        // ── Layer 4: white-hot core ───────────────────────────────────────────
        float coreR = hull * 0.85f;
        using SolidBrush coreBrush = new(Color.FromArgb((int)(255 * fade), Color.White));
        g.FillEllipse(coreBrush, cx - coreR, cy - coreR, coreR * 2, coreR * 2);

        // ── 8-spoke starburst — sells the brilliant flash look ────────────────
        float rayLen = hull + 14f * fade;
        using Pen rayPen = new(Color.FromArgb((int)(245 * fade), Color.White), 1.5f);

        for (int i = 0; i < 4; i++)
        {
            float angle = i * MathF.PI / 4f;  // 0°, 45°, 90°, 135° — paired → 8 spokes
            float dx = MathF.Cos(angle) * rayLen;
            float dy = MathF.Sin(angle) * rayLen;
            g.DrawLine(rayPen, cx - dx, cy - dy, cx + dx, cy + dy);
        }
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
    ///   <item>Phase 1 (first half of lifetime) — solid arc in spotter colour expands from the
    ///         spotter outward. The arc spans exactly the angular sector swept by the engine
    ///         (<c>[PrevRadarHeading, RadarHeading]</c>), centred on the sweep midpoint, so
    ///         every detected enemy lies within the arc when the wavefront reaches it.</item>
    ///   <item>Phase 2 (second half of lifetime) — dashed echo arc travels from the detected
    ///         enemy back toward the spotter.</item>
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

        // Outbound arc: centred on the midpoint of [PrevRadarHeading, RadarHeading] so the
        // visual sector exactly matches the angular region the engine checked.
        // Arena 0°=N clockwise → GDI+ (0°=E clockwise) requires subtracting 90°.
        float outboundMid = ev.SweepMidDeg - 90f;
        float arcSpan     = ev.SweepSpanDeg;

        // ── Phase 1: outbound wave (ageFraction 0 → <0.5) ────────────────────
        // t goes 0→1 across the first half-lifetime.
        // Fade GROWS from 0 to 1 so the arc is invisible at the spotter and reaches full
        // brightness exactly when the wavefront arrives at the enemy position (t=1).
        // This ensures the target is visibly "painted" before the echo begins.
        if (ageFraction < 0.5f)
        {
            float t      = ageFraction * 2f;                          // 0→1
            float radius = dist * t;                                  // 0 → dist
            float fade   = t;                                         // linear: dim at origin, full brightness at contact
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

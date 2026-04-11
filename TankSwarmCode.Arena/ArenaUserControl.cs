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

    // Rendering constants
    private const int TankBodySize = 18;
    private const int GunLength = 22;
    private const int RadarLength = 16;
    private const int EnergyBarWidth = 36;
    private const int EnergyBarHeight = 4;

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
            _engine.Start();

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
        _statusMessage = "Paused";
        Invalidate();
    }

    /// <summary>Stops the simulation and clears all tanks.</summary>
    public void Reset()
    {
        _gameTimer.Stop();
        _engine?.Reset();
        _engine = null;
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
        _engine?.Tick();
        Invalidate(); // triggers OnPaint
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

        // --- Radar arc ---
        float radarRad = (float)(tank.RadarHeading * Math.PI / 180.0);
        float rdx = (float)Math.Sin(radarRad) * RadarLength;
        float rdy = -(float)Math.Cos(radarRad) * RadarLength;
        using Pen radarPen = new(Color.FromArgb(120, Color.Cyan), 1);
        g.DrawLine(radarPen, 0, 0, rdx, rdy);
        // Fan showing 20° radar sweep
        using SolidBrush radarBrush = new(Color.FromArgb(25, Color.Cyan));
        float sweepStart = (float)tank.RadarHeading - 10f;
        g.FillPie(radarBrush, -RadarLength, -RadarLength,
                  RadarLength * 2, RadarLength * 2,
                  sweepStart - 90, 20);

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
    }

    private void DrawCentredText(Graphics g, string text, Font font, Brush brush)
    {
        SizeF size = g.MeasureString(text, font);
        float cx = (ClientSize.Width - size.Width) / 2f;
        float cy = (ClientSize.Height - size.Height) / 2f;
        g.DrawString(text, font, brush, cx, cy);
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

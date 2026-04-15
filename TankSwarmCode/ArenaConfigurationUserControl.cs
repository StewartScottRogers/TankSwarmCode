namespace TankSwarmCode;

/// <summary>
/// Side-panel that exposes render-toggle checkboxes for <see cref="ArenaUserControl"/>.
/// Call <see cref="SetArena"/> once after both controls are initialised.
/// </summary>
public partial class ArenaConfigurationUserControl : UserControl
{
    private ArenaUserControl? _arena;

    public ArenaConfigurationUserControl()
    {
        InitializeComponent();
        BuildControls();
    }

    /// <summary>Connects this panel to the arena control whose rendering it configures.</summary>
    public void SetArena(ArenaUserControl arena) => _arena = arena;

    // ── UI construction ───────────────────────────────────────────────────────

    private void BuildControls()
    {
        BackColor = Color.FromArgb(18, 18, 22);
        ForeColor = Color.Silver;
        Padding   = new Padding(0);

        // ── Header ────────────────────────────────────────────────────────────
        var header = new Label
        {
            Text      = "  ◼ RENDER OPTIONS",
            Dock      = DockStyle.Top,
            Height    = 22,
            Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = Color.Goldenrod,
            BackColor = Color.FromArgb(28, 28, 28),
            TextAlign = ContentAlignment.MiddleLeft,
        };

        // ── Checkbox list ─────────────────────────────────────────────────────
        var flow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            AutoScroll    = true,
            BackColor     = Color.FromArgb(18, 18, 22),
            Padding       = new Padding(8, 6, 4, 4),
        };

        // (label, getter, setter) for each toggle
        (string Label, Func<ArenaUserControl, bool> Get, Action<ArenaUserControl, bool> Set)[] toggles =
        [
            ("Anti-aliasing",       a => a.ShowAntiAliasing,     (a, v) => a.ShowAntiAliasing     = v),
            ("Radar reflections",   a => a.ShowRadarReflections,  (a, v) => a.ShowRadarReflections  = v),
            ("Radar sweep trails",  a => a.ShowRadarSweepTrails,  (a, v) => a.ShowRadarSweepTrails  = v),
            ("Scan halos",          a => a.ShowScanHalos,         (a, v) => a.ShowScanHalos         = v),
            ("Bullets",             a => a.ShowBullets,           (a, v) => a.ShowBullets           = v),
            ("Explosions & flames", a => a.ShowExplosions,        (a, v) => a.ShowExplosions        = v),
            ("Energy bars",         a => a.ShowEnergyBars,        (a, v) => a.ShowEnergyBars        = v),
            ("Tank labels",         a => a.ShowTankLabels,        (a, v) => a.ShowTankLabels        = v),
            ("HUD",                 a => a.ShowHud,               (a, v) => a.ShowHud               = v),
            ("Info panels",         a => a.ShowInfoPanels,        (a, v) => a.ShowInfoPanels        = v),
        ];

        foreach (var (label, get, set) in toggles)
        {
            var cb = new CheckBox
            {
                Text      = label,
                Checked   = true,   // default matches ArenaUserControl defaults
                AutoSize  = false,
                Width     = Width - 20,
                Height    = 22,
                ForeColor = Color.Silver,
                BackColor = Color.Transparent,
                Font      = new Font("Segoe UI", 8.5f),
                Margin    = new Padding(0, 2, 0, 2),
            };

            // Capture for closure
            var capturedGet = get;
            var capturedSet = set;

            cb.CheckedChanged += (_, _) =>
            {
                if (_arena is null) return;
                capturedSet(_arena, cb.Checked);
                _arena.Invalidate();
            };

            // Sync checkbox state when arena is connected after construction
            flow.Controls.Add(cb);
        }

        // ── Separator ─────────────────────────────────────────────────────────
        var sep = new Label
        {
            Text      = string.Empty,
            Dock      = DockStyle.Top,
            Height    = 1,
            BackColor = Color.FromArgb(45, 45, 45),
            Margin    = new Padding(0),
        };

        Controls.Add(flow);
        Controls.Add(sep);
        Controls.Add(header);
    }
}

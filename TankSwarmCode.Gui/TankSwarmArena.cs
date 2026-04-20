using TankSwarmCode.Arena;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTanks.Blue;
using TankSwarmCode.SwarmTanks.Red;

namespace TankSwarmCode.Gui;

public partial class TankSwarmArena : Form
{
    private bool _blueStrikeAdded;
    private bool _blueSharpAdded;
    private bool _blueRushAdded;
    private bool _blueGuardAdded;
    private bool _blueEcmAdded;
    private bool _redHammerAdded;
    private bool _redBladeAdded;
    private bool _redArrowAdded;
    private bool _redGhostAdded;
    private bool _roundEnded;
    private int  _lastNvN;

    // Per-swarm colours for radio log text, indexed by SwarmId (SwarmId 0 = solo/unknown)
    private static readonly Color[] _radioSwarmColours =
    [
        Color.Silver,       // 0 – solo / unknown
        Color.OrangeRed,    // 1 – Red swarm
        Color.DodgerBlue,   // 2 – Blue swarm
        Color.LimeGreen,    // 3
        Color.Gold,         // 4
        Color.MediumOrchid, // 5
        Color.DeepSkyBlue,  // 6
        Color.Coral,        // 7
        Color.Chartreuse,   // 8
    ];

    private const int RadioLogMaxLines = 300;
    private const int RadioLogTrimLines = 50;

    public TankSwarmArena()
    {
        InitializeComponent();
        _arenaConfigUserControl.SetArena(arenaUserControl1);
        arenaUserControl1.TicksPerSecond = 10;  // Normal speed by default
        arenaUserControl1.RadioTransmission += ArenaUserControl_RadioTransmission;
        arenaUserControl1.TickCompleted    += (_, _) => UpdateStatusStrip();
        arenaUserControl1.RoundEnded       += (_, _) => { _roundEnded = true; UpdateMenuState(); UpdateStatusStrip(); };
        BuildNvNSubMenu();
        BuildEcmMenuItems();
        SyncSpeedMenuChecks(10);
        UpdateStatusStrip();
    }

    // ── Radio comms log ───────────────────────────────────────────────────────

    private void ArenaUserControl_RadioTransmission(object? sender, RadioTransmissionEventArgs e)
    {
        Color color = e.SwarmId >= 0 && e.SwarmId < _radioSwarmColours.Length
            ? _radioSwarmColours[e.SwarmId]
            : Color.Silver;

        // Trim when the log grows too long (measured by line count)
        if (_radioLog.Lines.Length >= RadioLogMaxLines)
        {
            int charsToRemove = 0;
            string[] lines = _radioLog.Lines;
            for (int i = 0; i < RadioLogTrimLines && i < lines.Length; i++)
                charsToRemove += lines[i].Length + 1; // +1 for newline

            _radioLog.ReadOnly = false;
            _radioLog.Select(0, Math.Min(charsToRemove, _radioLog.TextLength));
            _radioLog.SelectedText = string.Empty;
            _radioLog.ReadOnly = true;
        }

        _radioLog.SelectionStart = _radioLog.TextLength;
        _radioLog.SelectionLength = 0;
        _radioLog.SelectionColor = color;
        _radioLog.AppendText(e.FormattedLine + "\n");
        _radioLog.ScrollToCaret();
    }

    private void ClearRadioLog()
    {
        _radioLog.Clear();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // Apply splitter constraints here, after the form is fully laid out and the
        // SplitContainer has its real width.  Setting these in InitializeComponent
        // causes EndInit() to validate against the control's tiny default size and
        // throw InvalidOperationException.
        _splitContainer.Panel1MinSize = 180;
        _splitContainer.Panel2MinSize = 400;
        _splitContainer.SplitterDistance = 527;
        UpdateMenuState();
    }

    // ── ECM menu items (added programmatically) ───────────────────────────────

    private ToolStripMenuItem? _menuItemAddRedEcmJammer;
    private ToolStripMenuItem? _menuItemAddBlueEcmOperator;

    /// <summary>Injects ECM-specialist menu entries into the Red and Blue swarm menus.</summary>
    private void BuildEcmMenuItems()
    {
        // Red ECM Jammer (goes after existing Red entries, before the separator/clear)
        _menuItemAddRedEcmJammer = new ToolStripMenuItem("Add ECM Jammer (no cannon, Spoof/Jam)");
        _menuItemAddRedEcmJammer.Click += (_, _) => MenuItemAddRedEcmJammer_Click();

        // Insert before the last separator in the Red Swarm menu
        var redMenu = _menuItemRedSwarm.DropDownItems;
        int redClearIdx = redMenu.IndexOf(_menuItemClearAllTanks);
        if (redClearIdx > 0)
            redMenu.Insert(redClearIdx - 1, _menuItemAddRedEcmJammer);
        else
            redMenu.Add(_menuItemAddRedEcmJammer);

        // Blue ECM Operator
        _menuItemAddBlueEcmOperator = new ToolStripMenuItem("Add ECM Operator (ECCM + light cannon)");
        _menuItemAddBlueEcmOperator.Click += (_, _) => MenuItemAddBlueEcmOperator_Click();

        var blueMenu = _menuItemBlueSwarm.DropDownItems;
        int blueClearIdx = blueMenu.IndexOf(_menuItemClearAllTanks2);
        if (blueClearIdx > 0)
            blueMenu.Insert(blueClearIdx - 1, _menuItemAddBlueEcmOperator);
        else
            blueMenu.Add(_menuItemAddBlueEcmOperator);
    }

    private void MenuItemAddRedEcmJammer_Click()
    {
        EnsureResetAfterRound();
        if (_redGhostAdded) return;
        arenaUserControl1.AddTank(new RedGhost());
        _redGhostAdded = true;
        UpdateMenuState();
    }

    private void MenuItemAddBlueEcmOperator_Click()
    {
        EnsureResetAfterRound();
        if (_blueEcmAdded) return;
        arenaUserControl1.AddTank(new BlueEcm());
        _blueEcmAdded = true;
        UpdateMenuState();
    }

    // ── Red Swarm handlers ────────────────────────────────────────────────────

    private void MenuItemBuildDefaultRedSwarm_Click(object? sender, EventArgs e)
    {
        EnsureResetAfterRound();
        arenaUserControl1.AddTank(new RedHammer());
        arenaUserControl1.AddTank(new RedBlade());
        arenaUserControl1.AddTank(new RedArrow());
        arenaUserControl1.AddTank(new RedGhost());
        _redHammerAdded = true;
        _redBladeAdded  = true;
        _redArrowAdded  = true;
        _redGhostAdded  = true;
        UpdateMenuState();
    }

    private void MenuItemAddRedScout_Click(object? sender, EventArgs e)
    {
        EnsureResetAfterRound();
        if (_redArrowAdded) return;
        arenaUserControl1.AddTank(new RedArrow());
        _redArrowAdded = true;
        UpdateMenuState();
    }

    private void MenuItemAddRedAttacker_Click(object? sender, EventArgs e)
    {
        EnsureResetAfterRound();
        if (_redHammerAdded) return;
        arenaUserControl1.AddTank(new RedHammer());
        _redHammerAdded = true;
        UpdateMenuState();
    }

    private void MenuItemAddRedFlanker_Click(object? sender, EventArgs e)
    {
        EnsureResetAfterRound();
        if (_redBladeAdded) return;
        arenaUserControl1.AddTank(new RedBlade());
        _redBladeAdded = true;
        UpdateMenuState();
    }

    // ── Blue Swarm handlers ───────────────────────────────────────────────────

    private void MenuItemBuildDefaultBlueSwarm_Click(object? sender, EventArgs e)
    {
        EnsureResetAfterRound();
        arenaUserControl1.AddTank(new BlueStrike());
        arenaUserControl1.AddTank(new BlueSharp());
        arenaUserControl1.AddTank(new BlueRush());
        arenaUserControl1.AddTank(new BlueGuard());
        arenaUserControl1.AddTank(new BlueEcm());
        _blueStrikeAdded = true;
        _blueSharpAdded  = true;
        _blueRushAdded   = true;
        _blueGuardAdded  = true;
        _blueEcmAdded    = true;
        UpdateMenuState();
    }

    private void MenuItemAddBlueWarden_Click(object? sender, EventArgs e)
    {
        EnsureResetAfterRound();
        if (_blueGuardAdded) return;
        arenaUserControl1.AddTank(new BlueGuard());
        _blueGuardAdded = true;
        UpdateMenuState();
    }

    private void MenuItemAddBluePatrol_Click(object? sender, EventArgs e)
    {
        EnsureResetAfterRound();
        if (_blueRushAdded) return;
        arenaUserControl1.AddTank(new BlueRush());
        _blueRushAdded = true;
        UpdateMenuState();
    }

    private void MenuItemAddBlueSniper_Click(object? sender, EventArgs e)
    {
        EnsureResetAfterRound();
        if (_blueSharpAdded) return;
        arenaUserControl1.AddTank(new BlueSharp());
        _blueSharpAdded = true;
        UpdateMenuState();
    }

    private void MenuItemAddBlueCommander_Click(object? sender, EventArgs e)
    {
        EnsureResetAfterRound();
        if (_blueStrikeAdded) return;
        arenaUserControl1.AddTank(new BlueStrike());
        _blueStrikeAdded = true;
        UpdateMenuState();
    }

    // ── Shared clear handler ──────────────────────────────────────────────────

    /// <summary>
    /// If the last round has ended, silently resets the arena before the user
    /// adds the first tank of the next game, clearing out all dead tanks.
    /// </summary>
    private void EnsureResetAfterRound()
    {
        if (!_roundEnded) return;
        arenaUserControl1.Reset();
        ResetTankFlags();
        _roundEnded = false;
        ClearRadioLog();
    }

    private void MenuItemClearAllTanks_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.Reset();
        ResetTankFlags();
        _roundEnded = false;
        ClearRadioLog();
        UpdateMenuState();
    }

    private void ResetTankFlags()
    {
        _blueStrikeAdded = false;
        _blueSharpAdded  = false;
        _blueRushAdded   = false;
        _blueGuardAdded  = false;
        _blueEcmAdded    = false;
        _redHammerAdded  = false;
        _redBladeAdded   = false;
        _redArrowAdded   = false;
        _redGhostAdded   = false;
    }

    // ── War handlers ──────────────────────────────────────────────────────────

    private void MenuItemStart_Click(object? sender, EventArgs e)
    {
        if (_roundEnded && _lastNvN > 0)
        {
            ConfigureNvN(_lastNvN);
            return;
        }
        arenaUserControl1.Start();
        UpdateMenuState();
        UpdateStatusStrip();
    }

    private void MenuItemStop_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.Stop();
        UpdateMenuState();
        UpdateStatusStrip();
    }

    private void MenuItemSingleStep_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.SingleStep();
        UpdateMenuState();
        UpdateStatusStrip();
    }

    private void MenuItemResetArena_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.Reset();
        ResetTankFlags();
        _roundEnded = false;
        ClearRadioLog();
        UpdateMenuState();
        UpdateStatusStrip();
    }

    // ── Player vs Player handlers ─────────────────────────────────────────────

    /// <summary>Appends items 3 vs 3 through 12 vs 12 directly to the Player vs Player menu.</summary>
    private void BuildNvNSubMenu()
    {
        for (int n = 3; n <= 12; n++)
        {
            int captured = n;
            var item = new ToolStripMenuItem($"&{captured} vs {captured}")
            {
                Name = $"_menuItemPvP{captured}v{captured}"
            };
            item.Click += (_, _) => ConfigureNvN(captured);
            _menuItemPlayerVsPlayer.DropDownItems.Add(item);
        }
    }

    /// <summary>Clears the arena and builds a balanced N-per-side Red vs Blue configuration.</summary>
    private void ConfigureNvN(int n)
    {
        _lastNvN = n;
        arenaUserControl1.Reset();
        ResetTankFlags();
        _roundEnded = false;
        ClearRadioLog();

        BuildRedTeam(n);
        BuildBlueTeam(n);

        arenaUserControl1.Start();
        UpdateMenuState();
    }

    // Red tanks ordered by formation slot (slot 0 = highest authority)
    private static readonly Func<ISwarmTank>[] RedTankFactories =
    [
        () => new RedHammer(),
        () => new RedBlade(),
        () => new RedArrow(),
        () => new RedGhost(),
    ];

    // Blue tanks ordered by formation slot
    private static readonly Func<ISwarmTank>[] BlueTankFactories =
    [
        () => new BlueStrike(),
        () => new BlueSharp(),
        () => new BlueRush(),
        () => new BlueGuard(),
        () => new BlueEcm(),
    ];

    private void BuildRedTeam(int n)
    {
        for (int i = 0; i < n; i++)
            arenaUserControl1.AddTank(i < RedTankFactories.Length
                ? RedTankFactories[i]()
                : new RedTrooper(i));
    }

    private void BuildBlueTeam(int n)
    {
        for (int i = 0; i < n; i++)
            arenaUserControl1.AddTank(i < BlueTankFactories.Length
                ? BlueTankFactories[i]()
                : new BlueTrooper(i));
    }

    /// <summary>Configures a 1 vs 1 match.</summary>
    private void MenuItemPvP1v1_Click(object? sender, EventArgs e) => ConfigureNvN(1);

    /// <summary>Configures a 2 vs 2 match.</summary>
    private void MenuItemPvP2v2_Click(object? sender, EventArgs e) => ConfigureNvN(2);

    // ── Speed handler ────────────────────────────────────────────────────────────

    private void MenuItemSpeed_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem clicked) return;

        int tps = clicked switch
        {
            _ when ReferenceEquals(clicked, _menuItemSpeedSlow)     => 3,
            _ when ReferenceEquals(clicked, _menuItemSpeedNormal)   => 10,
            _ when ReferenceEquals(clicked, _menuItemSpeedFast)     => 20,
            _ when ReferenceEquals(clicked, _menuItemSpeedVeryFast) => 30,
            _                                                        => 10
        };

        arenaUserControl1.TicksPerSecond = tps;
        _speedTrackBar.Value  = Math.Clamp(tps, _speedTrackBar.Minimum, _speedTrackBar.Maximum);
        _lblSpeedValue.Text   = $"{tps} TPS";
        SyncSpeedMenuChecks(tps);
    }

    /// <summary>Handles the toolbar slider being dragged.</summary>
    private void SpeedTrackBar_Scroll(object? sender, EventArgs e)
    {
        int tps = _speedTrackBar.Value;
        arenaUserControl1.TicksPerSecond = tps;
        _lblSpeedValue.Text = $"{tps} TPS";
        SyncSpeedMenuChecks(tps);
    }

    private void SyncSpeedMenuChecks(int tps)
    {
        _menuItemSpeedSlow.Checked     = tps == 3;
        _menuItemSpeedNormal.Checked   = tps == 10;
        _menuItemSpeedFast.Checked     = tps == 20;
        _menuItemSpeedVeryFast.Checked = tps == 30;
    }

    // ── Status strip ─────────────────────────────────────────────────────────

    private void UpdateStatusStrip()
    {
        var arena = arenaUserControl1.Arena;

        if (arena is null || !arena.HasStarted)
        {
            _statusLabelState.Text   = "Ready";
            _statusLabelTick.Text    = "Tick: —";
            _statusLabelRed.Text     = "— / —";
            _statusLabelBlue.Text    = "— / —";
            _statusLabelBullets.Text = "—";
            return;
        }

        _statusLabelState.Text = arena.IsRunning ? "Running" : "Stopped";
        _statusLabelState.ForeColor = arena.IsRunning ? Color.Green : SystemColors.MenuText;

        int eTps = arenaUserControl1.EffectiveTps;
        int mult = arenaUserControl1.SimTicksPerFrame;
        _statusLabelTick.Text = mult > 1
            ? $"Tick: {arena.TickNumber}  ({eTps} TPS ×{mult})"
            : $"Tick: {arena.TickNumber}  ({eTps} TPS)";

        var tanks = arena.Tanks;
        int redTotal  = tanks.Count(t => t.SwarmId == 1);
        int redAlive  = tanks.Count(t => t.SwarmId == 1 && t.State.IsAlive);
        int blueTotal = tanks.Count(t => t.SwarmId == 2);
        int blueAlive = tanks.Count(t => t.SwarmId == 2 && t.State.IsAlive);

        _statusLabelRed.Text  = $"{redAlive} / {redTotal}";
        _statusLabelBlue.Text = $"{blueAlive} / {blueTotal}";

        _statusLabelBullets.Text = arena.Bullets.Count.ToString();
    }

    // ── Menu state ────────────────────────────────────────────────────────────

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        ScreenWakeLock.Allow();
    }

    /// <summary>
    /// Enables or disables menu items to reflect the current simulation state.
    /// </summary>
    private void UpdateMenuState()
    {
        bool running = arenaUserControl1.Arena?.IsRunning ?? false;

        if (running)
            ScreenWakeLock.Prevent();
        else
            ScreenWakeLock.Allow();
        bool hasStarted = arenaUserControl1.Arena?.HasStarted ?? false;

        // Build commands are available before the game starts, or once a round has ended.
        // When _roundEnded, EnsureResetAfterRound() will silently reset before the first add.
        bool canBuild = !hasStarted || _roundEnded;
        _menuItemBuildDefaultRedSwarm.Enabled = canBuild;
        _menuItemAddRedScout.Enabled          = canBuild;
        _menuItemAddRedAttacker.Enabled       = canBuild;
        _menuItemAddRedFlanker.Enabled        = canBuild;
        _menuItemClearAllTanks.Enabled        = canBuild;
        _menuItemBuildDefaultBlueSwarm.Enabled = canBuild;
        _menuItemAddBlueWarden.Enabled         = canBuild;
        _menuItemAddBluePatrol.Enabled         = canBuild;
        _menuItemAddBlueSniper.Enabled         = canBuild;
        _menuItemAddBlueCommander.Enabled      = canBuild && !_blueStrikeAdded;
        _menuItemClearAllTanks2.Enabled        = canBuild;

        if (_menuItemAddRedEcmJammer    is not null) _menuItemAddRedEcmJammer.Enabled    = canBuild && !_redGhostAdded;
        if (_menuItemAddBlueEcmOperator is not null) _menuItemAddBlueEcmOperator.Enabled = canBuild && !_blueEcmAdded;
        // ConfigureNvN always resets first, so it's safe to offer NvN any time the sim isn't ticking.
        _menuItemPlayerVsPlayer.Enabled        = !running;

        bool hasTanks = arenaUserControl1.TankCount > 0;

        bool canStart = !running && ((!_roundEnded && hasTanks) || (_roundEnded && _lastNvN > 0));
        _menuItemStart.Enabled      = canStart;
        _menuItemStop.Enabled       = running;
        _menuItemSingleStep.Enabled = !running && hasTanks && !_roundEnded;
        // Reset is always available
        _menuItemResetArena.Enabled = true;

        // ── Toolbar mirrors War menu ──────────────────────────────────────────
        _btnStartWar.Enabled   = canStart;
        _btnStopWar.Enabled    = running;
        _btnSingleStep.Enabled = !running && hasTanks && !_roundEnded;
    }
}


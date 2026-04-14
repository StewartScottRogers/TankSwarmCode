using TankSwarmCode.SwarmTanks.Blue;
using TankSwarmCode.SwarmTanks.Red;

namespace TankSwarmCode;

public partial class TankSwarmArena : Form
{
    private int _redAttackerCount;
    private int _redFlankerCount;
    private int _bluePatrolCount;
    private int _blueSniperCount;

    public TankSwarmArena()
    {
        InitializeComponent();
        arenaUserControl1.TicksPerSecond = 10;  // Normal speed by default
        BuildNvNSubMenu();
        SyncSpeedMenuChecks(10);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UpdateMenuState();
    }

    // ── Red Swarm handlers ────────────────────────────────────────────────────

    private void MenuItemBuildDefaultRedSwarm_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.AddTank(new RedScout());
        arenaUserControl1.AddTank(new RedAttacker("RedAlpha"));
        arenaUserControl1.AddTank(new RedAttacker("RedBravo"));
        arenaUserControl1.AddTank(new RedFlank("RedWolf",  90));
        arenaUserControl1.AddTank(new RedFlank("RedFox",  -90));
        _redAttackerCount = 2;
        _redFlankerCount  = 2;
        UpdateMenuState();
    }

    private void MenuItemAddRedScout_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.AddTank(new RedScout());
        UpdateMenuState();
    }

    private void MenuItemAddRedAttacker_Click(object? sender, EventArgs e)
    {
        _redAttackerCount++;
        arenaUserControl1.AddTank(new RedAttacker($"Red{_redAttackerCount}"));
        UpdateMenuState();
    }

    private void MenuItemAddRedFlanker_Click(object? sender, EventArgs e)
    {
        _redFlankerCount++;
        int angle = (_redFlankerCount % 2 == 1) ? 90 : -90;
        arenaUserControl1.AddTank(new RedFlank($"RedFlanker{_redFlankerCount}", angle));
        UpdateMenuState();
    }

    // ── Blue Swarm handlers ───────────────────────────────────────────────────

    private void MenuItemBuildDefaultBlueSwarm_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.AddTank(new BlueWarden());
        arenaUserControl1.AddTank(new BluePatrol("BlueEast", 0));
        arenaUserControl1.AddTank(new BluePatrol("BlueWest", 1));
        arenaUserControl1.AddTank(new BlueSniper("BlueEagle", topLeft: true));
        arenaUserControl1.AddTank(new BlueSniper("BlueHawk",  topLeft: false));
        _bluePatrolCount = 2;
        UpdateMenuState();
    }

    private void MenuItemAddBlueWarden_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.AddTank(new BlueWarden());
        UpdateMenuState();
    }

    private void MenuItemAddBluePatrol_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.AddTank(new BluePatrol($"BluePatrol{_bluePatrolCount}", _bluePatrolCount % 2));
        _bluePatrolCount++;
        UpdateMenuState();
    }

    private void MenuItemAddBlueSniper_Click(object? sender, EventArgs e)
    {
        _blueSniperCount++;
        arenaUserControl1.AddTank(new BlueSniper($"BlueSniper{_blueSniperCount}", topLeft: _blueSniperCount % 2 == 1));
        UpdateMenuState();
    }

    // ── Shared clear handler ──────────────────────────────────────────────────

    private void MenuItemClearAllTanks_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.Reset();
        _redAttackerCount = 0;
        _redFlankerCount  = 0;
        _bluePatrolCount  = 0;
        _blueSniperCount  = 0;
        UpdateMenuState();
    }

    // ── War handlers ──────────────────────────────────────────────────────────

    private void MenuItemStart_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.Start();
        UpdateMenuState();
    }

    private void MenuItemStop_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.Stop();
        UpdateMenuState();
    }

    private void MenuItemSingleStep_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.SingleStep();
        UpdateMenuState();
    }

    private void MenuItemResetArena_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.Reset();
        _redAttackerCount = 0;
        _redFlankerCount  = 0;
        _bluePatrolCount  = 0;
        _blueSniperCount  = 0;
        UpdateMenuState();
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
        arenaUserControl1.Reset();
        _redAttackerCount = 0;
        _redFlankerCount  = 0;
        _bluePatrolCount  = 0;
        _blueSniperCount  = 0;

        BuildRedTeam(n);
        BuildBlueTeam(n);

        arenaUserControl1.Start();
        UpdateMenuState();
    }

    /// <summary>
    /// Adds n Red tanks:
    ///   n=1 → 1 Attacker
    ///   n=2 → 1 Attacker + 1 Flanker
    ///   n≥3 → 1 Scout + ceil((n-1)/2) Attackers + floor((n-1)/2) Flankers
    /// </summary>
    private void BuildRedTeam(int n)
    {
        bool hasScout  = n >= 3;
        int  combat    = n - (hasScout ? 1 : 0);
        int  attackers = (combat + 1) / 2;
        int  flankers  = combat / 2;

        if (hasScout)
            arenaUserControl1.AddTank(new RedScout());

        for (int i = 0; i < attackers; i++)
        {
            _redAttackerCount++;
            arenaUserControl1.AddTank(new RedAttacker($"Red{_redAttackerCount}"));
        }

        for (int i = 0; i < flankers; i++)
        {
            _redFlankerCount++;
            int angle = (_redFlankerCount % 2 == 1) ? 90 : -90;
            arenaUserControl1.AddTank(new RedFlank($"RedFlanker{_redFlankerCount}", angle));
        }
    }

    /// <summary>
    /// Adds n Blue tanks:
    ///   n=1 → 1 Patrol
    ///   n=2 → 1 Patrol + 1 Sniper
    ///   n≥3 → 1 Warden + ceil((n-1)/2) Patrols + floor((n-1)/2) Snipers
    /// </summary>
    private void BuildBlueTeam(int n)
    {
        bool hasWarden = n >= 3;
        int  combat    = n - (hasWarden ? 1 : 0);
        int  patrols   = (combat + 1) / 2;
        int  snipers   = combat / 2;

        if (hasWarden)
            arenaUserControl1.AddTank(new BlueWarden());

        for (int i = 0; i < patrols; i++)
        {
            arenaUserControl1.AddTank(new BluePatrol($"BluePatrol{_bluePatrolCount}", _bluePatrolCount % 2));
            _bluePatrolCount++;
        }

        for (int i = 1; i <= snipers; i++)
        {
            arenaUserControl1.AddTank(new BlueSniper($"BlueSniper{i}", topLeft: i % 2 == 1));
        }
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

    // ── Menu state ────────────────────────────────────────────────────────────

    /// <summary>
    /// Enables or disables menu items to reflect the current simulation state.
    /// </summary>
    private void UpdateMenuState()
    {
        bool running = arenaUserControl1.Arena?.IsRunning ?? false;
        bool hasStarted = arenaUserControl1.Arena?.HasStarted ?? false;

        // Build commands are only available when the war has not started
        bool canBuild = !hasStarted;
        _menuItemBuildDefaultRedSwarm.Enabled = canBuild;
        _menuItemAddRedScout.Enabled          = canBuild;
        _menuItemAddRedAttacker.Enabled       = canBuild;
        _menuItemAddRedFlanker.Enabled        = canBuild;
        _menuItemClearAllTanks.Enabled        = canBuild;
        _menuItemBuildDefaultBlueSwarm.Enabled = canBuild;
        _menuItemAddBlueWarden.Enabled         = canBuild;
        _menuItemAddBluePatrol.Enabled         = canBuild;
        _menuItemAddBlueSniper.Enabled         = canBuild;
        _menuItemClearAllTanks2.Enabled        = canBuild;
        _menuItemPlayerVsPlayer.Enabled        = canBuild;

        _menuItemStart.Enabled      = !running;
        _menuItemStop.Enabled       = running;
        _menuItemSingleStep.Enabled = !running;
        // Reset is always available
        _menuItemResetArena.Enabled = true;

        // ── Toolbar mirrors War menu ──────────────────────────────────────────
        _btnStartWar.Enabled   = !running;
        _btnStopWar.Enabled    = running;
        _btnSingleStep.Enabled = !running;
    }
}


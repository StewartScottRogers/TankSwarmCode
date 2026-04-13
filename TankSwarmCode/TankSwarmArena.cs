using TankSwarmCode.SwarmTanks.Blue;
using TankSwarmCode.SwarmTanks.Red;

namespace TankSwarmCode;

public partial class TankSwarmArena : Form
{
    private int _redAttackerCount;
    private int _redFlankerCount;
    private int _bluePatrolCount;

    public TankSwarmArena()
    {
        InitializeComponent();
        arenaUserControl1.TicksPerSecond = 3;
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
        bool topLeft = (arenaUserControl1.Arena?.Tanks.Count(t => t.SwarmId == 2) ?? 0) % 2 == 0;
        arenaUserControl1.AddTank(new BlueSniper($"BlueSniper{_bluePatrolCount}", topLeft));
        UpdateMenuState();
    }

    // ── Shared clear handler ──────────────────────────────────────────────────

    private void MenuItemClearAllTanks_Click(object? sender, EventArgs e)
    {
        arenaUserControl1.Reset();
        _redAttackerCount = 0;
        _redFlankerCount  = 0;
        _bluePatrolCount  = 0;
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
        UpdateMenuState();
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


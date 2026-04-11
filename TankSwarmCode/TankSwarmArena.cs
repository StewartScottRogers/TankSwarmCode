using TankSwarmCode.SwarmTanks.Blue;
using TankSwarmCode.SwarmTanks.Red;

namespace TankSwarmCode;

public partial class TankSwarmArena : Form
{
    public TankSwarmArena()
    {
        InitializeComponent();
        arenaUserControl1.TicksPerSecond = 20;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // ── Red Swarm (SwarmId = 1) ──────────────────────────────────────────
        // Scout: sweeps radar, S-curves, broadcasts enemy positions
        arenaUserControl1.AddTank(new RedScout());

        // Two attackers: linear prediction + RequestBackup on low energy
        arenaUserControl1.AddTank(new RedAttacker("RedAlpha"));
        arenaUserControl1.AddTank(new RedAttacker("RedBravo"));

        // Two flankers: pincer from left (+90°) and right (-90°)
        arenaUserControl1.AddTank(new RedFlank("RedWolf",  90));
        arenaUserControl1.AddTank(new RedFlank("RedFox",  -90));

        // ── Blue Swarm (SwarmId = 2) ─────────────────────────────────────────
        // Warden: holds arena centre, adaptive fire, broadcasts sightings
        arenaUserControl1.AddTank(new BlueWarden());

        // Two patrols: cover left and right halves, converge on intel
        arenaUserControl1.AddTank(new BluePatrol("BlueEast", 0));
        arenaUserControl1.AddTank(new BluePatrol("BlueWest", 1));

        // Two snipers: opposite corners, bullet evasion, 3.0-power precision shots
        arenaUserControl1.AddTank(new BlueSniper("BlueEagle", topLeft: true));
        arenaUserControl1.AddTank(new BlueSniper("BlueHawk",  topLeft: false));

        arenaUserControl1.Start();
    }
}

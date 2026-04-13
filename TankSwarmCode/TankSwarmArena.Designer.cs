namespace TankSwarmCode
{
    partial class TankSwarmArena
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _mainMenuStrip = new MenuStrip();

            // ── Red Swarm menu items ──────────────────────────────────────────
            _menuItemRedSwarm = new ToolStripMenuItem();
            _menuItemBuildDefaultRedSwarm = new ToolStripMenuItem();
            _separatorRed1 = new ToolStripSeparator();
            _menuItemAddRedScout = new ToolStripMenuItem();
            _menuItemAddRedAttacker = new ToolStripMenuItem();
            _menuItemAddRedFlanker = new ToolStripMenuItem();
            _separatorRed2 = new ToolStripSeparator();
            _menuItemClearAllTanks = new ToolStripMenuItem();

            // ── Blue Swarm menu items ─────────────────────────────────────────
            _menuItemBlueSwarm = new ToolStripMenuItem();
            _menuItemBuildDefaultBlueSwarm = new ToolStripMenuItem();
            _separatorBlue1 = new ToolStripSeparator();
            _menuItemAddBlueWarden = new ToolStripMenuItem();
            _menuItemAddBluePatrol = new ToolStripMenuItem();
            _menuItemAddBlueSniper = new ToolStripMenuItem();
            _separatorBlue2 = new ToolStripSeparator();
            _menuItemClearAllTanks2 = new ToolStripMenuItem();

            // ── War menu items ────────────────────────────────────────────────
            _menuItemWar = new ToolStripMenuItem();
            _menuItemStart = new ToolStripMenuItem();
            _menuItemStop = new ToolStripMenuItem();
            _separatorWar1 = new ToolStripSeparator();
            _menuItemSingleStep = new ToolStripMenuItem();
            _separatorWar2 = new ToolStripSeparator();
            _menuItemResetArena = new ToolStripMenuItem();
            _separatorWar3 = new ToolStripSeparator();
            _menuItemSpeed = new ToolStripMenuItem();
            _menuItemSpeedSlow = new ToolStripMenuItem();
            _menuItemSpeedNormal = new ToolStripMenuItem();
            _menuItemSpeedFast = new ToolStripMenuItem();
            _menuItemSpeedVeryFast = new ToolStripMenuItem();
            _separatorWar4 = new ToolStripSeparator();

            // ── Player vs Player sub-menu items ───────────────────────────────
            _menuItemPlayerVsPlayer = new ToolStripMenuItem();
            _menuItemPvP1v1 = new ToolStripMenuItem();
            _menuItemPvP2v2 = new ToolStripMenuItem();

            arenaUserControl1 = new TankSwarmCode.Arena.ArenaUserControl();

            // ── Toolbar controls ──────────────────────────────────────────────
            _toolStrip = new ToolStrip();
            _btnStartWar = new ToolStripButton();
            _btnStopWar = new ToolStripButton();
            _separatorToolbar1 = new ToolStripSeparator();
            _btnSingleStep = new ToolStripButton();
            _separatorToolbar2 = new ToolStripSeparator();
            _lblSpeed = new ToolStripLabel();
            _speedTrackBar = new TrackBar();
            _speedTrackBarHost = new ToolStripControlHost(_speedTrackBar);
            _lblSpeedValue = new ToolStripLabel();

            _mainMenuStrip.SuspendLayout();
            _toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_speedTrackBar).BeginInit();
            SuspendLayout();

            // ── _mainMenuStrip ────────────────────────────────────────────────
            _mainMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                _menuItemRedSwarm,
                _menuItemBlueSwarm,
                _menuItemWar
            });
            _mainMenuStrip.Location = new Point(0, 0);
            _mainMenuStrip.Name = "_mainMenuStrip";
            _mainMenuStrip.Size = new Size(1079, 24);
            _mainMenuStrip.TabIndex = 1;
            _mainMenuStrip.Text = "menuStrip";

            // ── Red Swarm top-level ───────────────────────────────────────────
            _menuItemRedSwarm.DropDownItems.AddRange(new ToolStripItem[]
            {
                _menuItemBuildDefaultRedSwarm,
                _separatorRed1,
                _menuItemAddRedScout,
                _menuItemAddRedAttacker,
                _menuItemAddRedFlanker,
                _separatorRed2,
                _menuItemClearAllTanks
            });
            _menuItemRedSwarm.Name = "_menuItemRedSwarm";
            _menuItemRedSwarm.Text = "&Red Swarm";

            _menuItemBuildDefaultRedSwarm.Name = "_menuItemBuildDefaultRedSwarm";
            _menuItemBuildDefaultRedSwarm.Text = "Build &Default Red Swarm";
            _menuItemBuildDefaultRedSwarm.Click += MenuItemBuildDefaultRedSwarm_Click;

            _menuItemAddRedScout.Name = "_menuItemAddRedScout";
            _menuItemAddRedScout.Text = "Add Red &Scout";
            _menuItemAddRedScout.Click += MenuItemAddRedScout_Click;

            _menuItemAddRedAttacker.Name = "_menuItemAddRedAttacker";
            _menuItemAddRedAttacker.Text = "Add Red &Attacker";
            _menuItemAddRedAttacker.Click += MenuItemAddRedAttacker_Click;

            _menuItemAddRedFlanker.Name = "_menuItemAddRedFlanker";
            _menuItemAddRedFlanker.Text = "Add Red &Flanker";
            _menuItemAddRedFlanker.Click += MenuItemAddRedFlanker_Click;

            _menuItemClearAllTanks.Name = "_menuItemClearAllTanks";
            _menuItemClearAllTanks.Text = "&Clear All Tanks";
            _menuItemClearAllTanks.Click += MenuItemClearAllTanks_Click;

            // ── Blue Swarm top-level ──────────────────────────────────────────
            _menuItemBlueSwarm.DropDownItems.AddRange(new ToolStripItem[]
            {
                _menuItemBuildDefaultBlueSwarm,
                _separatorBlue1,
                _menuItemAddBlueWarden,
                _menuItemAddBluePatrol,
                _menuItemAddBlueSniper,
                _separatorBlue2,
                _menuItemClearAllTanks2
            });
            _menuItemBlueSwarm.Name = "_menuItemBlueSwarm";
            _menuItemBlueSwarm.Text = "&Blue Swarm";

            _menuItemBuildDefaultBlueSwarm.Name = "_menuItemBuildDefaultBlueSwarm";
            _menuItemBuildDefaultBlueSwarm.Text = "Build &Default Blue Swarm";
            _menuItemBuildDefaultBlueSwarm.Click += MenuItemBuildDefaultBlueSwarm_Click;

            _menuItemAddBlueWarden.Name = "_menuItemAddBlueWarden";
            _menuItemAddBlueWarden.Text = "Add Blue &Warden";
            _menuItemAddBlueWarden.Click += MenuItemAddBlueWarden_Click;

            _menuItemAddBluePatrol.Name = "_menuItemAddBluePatrol";
            _menuItemAddBluePatrol.Text = "Add Blue &Patrol";
            _menuItemAddBluePatrol.Click += MenuItemAddBluePatrol_Click;

            _menuItemAddBlueSniper.Name = "_menuItemAddBlueSniper";
            _menuItemAddBlueSniper.Text = "Add Blue &Sniper";
            _menuItemAddBlueSniper.Click += MenuItemAddBlueSniper_Click;

            _menuItemClearAllTanks2.Name = "_menuItemClearAllTanks2";
            _menuItemClearAllTanks2.Text = "&Clear All Tanks";
            _menuItemClearAllTanks2.Click += MenuItemClearAllTanks_Click;

            // ── War top-level ─────────────────────────────────────────────────
            _menuItemWar.DropDownItems.AddRange(new ToolStripItem[]
            {
                _menuItemStart,
                _menuItemStop,
                _separatorWar1,
                _menuItemSingleStep,
                _separatorWar2,
                _menuItemResetArena,
                _separatorWar3,
                _menuItemSpeed,
                _separatorWar4,
                _menuItemPlayerVsPlayer
            });
            _menuItemWar.Name = "_menuItemWar";
            _menuItemWar.Text = "&War";

            _menuItemStart.Name = "_menuItemStart";
            _menuItemStart.ShortcutKeys = Keys.F5;
            _menuItemStart.Text = "&Start";
            _menuItemStart.Click += MenuItemStart_Click;

            _menuItemStop.Name = "_menuItemStop";
            _menuItemStop.ShortcutKeys = Keys.F6;
            _menuItemStop.Text = "S&top";
            _menuItemStop.Enabled = false;
            _menuItemStop.Click += MenuItemStop_Click;

            _menuItemSingleStep.Name = "_menuItemSingleStep";
            _menuItemSingleStep.ShortcutKeys = Keys.F10;
            _menuItemSingleStep.Text = "S&ingle Step";
            _menuItemSingleStep.Click += MenuItemSingleStep_Click;

            _menuItemResetArena.Name = "_menuItemResetArena";
            _menuItemResetArena.Text = "&Reset Arena";
            _menuItemResetArena.Click += MenuItemResetArena_Click;

            // ── Player vs Player sub-menu ─────────────────────────────────────
            // 3 vs 3 … 12 vs 12 are added dynamically in BuildNvNSubMenu()
            _menuItemPlayerVsPlayer.DropDownItems.AddRange(new ToolStripItem[]
            {
                _menuItemPvP1v1,
                _menuItemPvP2v2
            });
            _menuItemPlayerVsPlayer.Name = "_menuItemPlayerVsPlayer";
            _menuItemPlayerVsPlayer.Text = "&Player vs Player";

            _menuItemPvP1v1.Name = "_menuItemPvP1v1";
            _menuItemPvP1v1.Text = "&1 vs 1";
            _menuItemPvP1v1.Click += MenuItemPvP1v1_Click;

            _menuItemPvP2v2.Name = "_menuItemPvP2v2";
            _menuItemPvP2v2.Text = "&2 vs 2";
            _menuItemPvP2v2.Click += MenuItemPvP2v2_Click;

            // ── Speed sub-menu ────────────────────────────────────────────────
            _menuItemSpeed.DropDownItems.AddRange(new ToolStripItem[]
            {
                _menuItemSpeedSlow,
                _menuItemSpeedNormal,
                _menuItemSpeedFast,
                _menuItemSpeedVeryFast
            });
            _menuItemSpeed.Name = "_menuItemSpeed";
            _menuItemSpeed.Text = "S&peed";

            _menuItemSpeedSlow.Name = "_menuItemSpeedSlow";
            _menuItemSpeedSlow.Text = "&Slow (3 TPS)";
            _menuItemSpeedSlow.Click += MenuItemSpeed_Click;

            _menuItemSpeedNormal.Checked = true;
            _menuItemSpeedNormal.Name = "_menuItemSpeedNormal";
            _menuItemSpeedNormal.Text = "&Normal (10 TPS)";
            _menuItemSpeedNormal.Click += MenuItemSpeed_Click;

            _menuItemSpeedFast.Name = "_menuItemSpeedFast";
            _menuItemSpeedFast.Text = "&Fast (20 TPS)";
            _menuItemSpeedFast.Click += MenuItemSpeed_Click;

            _menuItemSpeedVeryFast.Name = "_menuItemSpeedVeryFast";
            _menuItemSpeedVeryFast.Text = "&Very Fast (30 TPS)";
            _menuItemSpeedVeryFast.Click += MenuItemSpeed_Click;

            // ── _toolStrip
            _toolStrip.Items.AddRange(new ToolStripItem[]
            {
                _btnStartWar,
                _btnStopWar,
                _separatorToolbar1,
                _btnSingleStep,
                _separatorToolbar2,
                _lblSpeed,
                _speedTrackBarHost,
                _lblSpeedValue
            });
            _toolStrip.Location = new Point(0, 24);
            _toolStrip.Name = "_toolStrip";
            _toolStrip.Size = new Size(1079, 25);
            _toolStrip.TabIndex = 2;
            _toolStrip.Text = "toolStrip";

            _lblSpeed.Name = "_lblSpeed";
            _lblSpeed.Text = "Speed:";

            _speedTrackBar.AutoSize = false;
            _speedTrackBar.Maximum = 30;
            _speedTrackBar.Minimum = 1;
            _speedTrackBar.Name = "_speedTrackBar";
            _speedTrackBar.Size = new Size(150, 22);
            _speedTrackBar.SmallChange = 1;
            _speedTrackBar.LargeChange = 5;
            _speedTrackBar.TickFrequency = 5;
            _speedTrackBar.Value = 10;
            _speedTrackBar.Scroll += SpeedTrackBar_Scroll;

            _speedTrackBarHost.AutoSize = false;
            _speedTrackBarHost.Name = "_speedTrackBarHost";
            _speedTrackBarHost.Size = new Size(154, 22);
            _speedTrackBarHost.ToolTipText = "Drag to adjust simulation speed (ticks per second)";

            _lblSpeedValue.Name = "_lblSpeedValue";
            _lblSpeedValue.Text = "10 TPS";

            _btnStartWar.Name = "_btnStartWar";
            _btnStartWar.Text = "\u25B6  Start War";
            _btnStartWar.ToolTipText = "Start simulation (F5)";
            _btnStartWar.Click += MenuItemStart_Click;

            _btnStopWar.Name = "_btnStopWar";
            _btnStopWar.Text = "\u23F9  Stop";
            _btnStopWar.ToolTipText = "Stop simulation (F6)";
            _btnStopWar.Enabled = false;
            _btnStopWar.Click += MenuItemStop_Click;

            _btnSingleStep.Name = "_btnSingleStep";
            _btnSingleStep.Text = "\u2192\u258F  Step";
            _btnSingleStep.ToolTipText = "Advance one tick (F10)";
            _btnSingleStep.Click += MenuItemSingleStep_Click;

            // ── arenaUserControl1 ─────────────────────────────────────────────
            arenaUserControl1.Dock = DockStyle.Fill;
            arenaUserControl1.Location = new Point(0, 49);
            arenaUserControl1.Name = "arenaUserControl1";
            arenaUserControl1.Size = new Size(1079, 660);
            arenaUserControl1.TabIndex = 0;

            // ── TankSwarmArena ────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(arenaUserControl1);
            Controls.Add(_toolStrip);
            Controls.Add(_mainMenuStrip);
            MainMenuStrip = _mainMenuStrip;
            MinimizeBox = false;
            Name = "TankSwarmArena";
            ShowIcon = false;
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tank Swarm Arena";
            ClientSize = new Size(1079, 734);
            ((System.ComponentModel.ISupportInitialize)_speedTrackBar).EndInit();
            _toolStrip.ResumeLayout(false);
            _toolStrip.PerformLayout();
            _mainMenuStrip.ResumeLayout(false);
            _mainMenuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Arena.ArenaUserControl arenaUserControl1;
        private MenuStrip _mainMenuStrip;

        // Red Swarm menu
        private ToolStripMenuItem _menuItemRedSwarm;
        private ToolStripMenuItem _menuItemBuildDefaultRedSwarm;
        private ToolStripSeparator _separatorRed1;
        private ToolStripMenuItem _menuItemAddRedScout;
        private ToolStripMenuItem _menuItemAddRedAttacker;
        private ToolStripMenuItem _menuItemAddRedFlanker;
        private ToolStripSeparator _separatorRed2;
        private ToolStripMenuItem _menuItemClearAllTanks;

        // Blue Swarm menu
        private ToolStripMenuItem _menuItemBlueSwarm;
        private ToolStripMenuItem _menuItemBuildDefaultBlueSwarm;
        private ToolStripSeparator _separatorBlue1;
        private ToolStripMenuItem _menuItemAddBlueWarden;
        private ToolStripMenuItem _menuItemAddBluePatrol;
        private ToolStripMenuItem _menuItemAddBlueSniper;
        private ToolStripSeparator _separatorBlue2;
        private ToolStripMenuItem _menuItemClearAllTanks2;

        // War menu
        private ToolStripMenuItem _menuItemWar;
        private ToolStripMenuItem _menuItemStart;
        private ToolStripMenuItem _menuItemStop;
        private ToolStripSeparator _separatorWar1;
        private ToolStripMenuItem _menuItemSingleStep;
        private ToolStripSeparator _separatorWar2;
        private ToolStripMenuItem _menuItemResetArena;
        private ToolStripSeparator _separatorWar3;
        private ToolStripSeparator _separatorWar4;
        private ToolStripMenuItem _menuItemSpeed;
        private ToolStripMenuItem _menuItemSpeedSlow;
        private ToolStripMenuItem _menuItemSpeedNormal;
        private ToolStripMenuItem _menuItemSpeedFast;
        private ToolStripMenuItem _menuItemSpeedVeryFast;

        // Player vs Player sub-menu
        private ToolStripMenuItem _menuItemPlayerVsPlayer;
        private ToolStripMenuItem _menuItemPvP1v1;
        private ToolStripMenuItem _menuItemPvP2v2;

        // Toolbar
        private ToolStrip _toolStrip;
        private ToolStripButton _btnStartWar;
        private ToolStripButton _btnStopWar;
        private ToolStripSeparator _separatorToolbar1;
        private ToolStripButton _btnSingleStep;
        private ToolStripSeparator _separatorToolbar2;
        private ToolStripLabel _lblSpeed;
        private TrackBar _speedTrackBar;
        private ToolStripControlHost _speedTrackBarHost;
        private ToolStripLabel _lblSpeedValue;
    }
}

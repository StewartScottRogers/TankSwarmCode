namespace TankSwarmCode.Gui
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
            _statusStrip = new StatusStrip();
            _statusLabelState = new ToolStripStatusLabel();
            _statusLabelTick = new ToolStripStatusLabel();
            _statusSepRed = new ToolStripStatusLabel();
            _statusLabelRed = new ToolStripStatusLabel();
            _statusSepBlue = new ToolStripStatusLabel();
            _statusLabelBlue = new ToolStripStatusLabel();
            _statusSepBullets = new ToolStripStatusLabel();
            _statusLabelBullets = new ToolStripStatusLabel();
            _mainMenuStrip = new MenuStrip();
            _menuItemRedSwarm = new ToolStripMenuItem();
            _menuItemBuildDefaultRedSwarm = new ToolStripMenuItem();
            _separatorRed1 = new ToolStripSeparator();
            _menuItemAddRedScout = new ToolStripMenuItem();
            _menuItemAddRedAttacker = new ToolStripMenuItem();
            _menuItemAddRedFlanker = new ToolStripMenuItem();
            _separatorRed2 = new ToolStripSeparator();
            _menuItemClearAllTanks = new ToolStripMenuItem();
            _menuItemBlueSwarm = new ToolStripMenuItem();
            _menuItemBuildDefaultBlueSwarm = new ToolStripMenuItem();
            _separatorBlue1 = new ToolStripSeparator();
            _menuItemAddBlueWarden = new ToolStripMenuItem();
            _menuItemAddBluePatrol = new ToolStripMenuItem();
            _menuItemAddBlueSniper = new ToolStripMenuItem();
            _menuItemAddBlueCommander = new ToolStripMenuItem();
            _separatorBlue2 = new ToolStripSeparator();
            _menuItemClearAllTanks2 = new ToolStripMenuItem();
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
            _menuItemPlayerVsPlayer = new ToolStripMenuItem();
            _menuItemPvP1v1 = new ToolStripMenuItem();
            _menuItemPvP2v2 = new ToolStripMenuItem();
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
            _splitContainer = new SplitContainer();
            _radioCommsPanel = new Panel();
            _radioLog = new RichTextBox();
            _radioCommsHeader = new Label();
            arenaUserControl1 = new ArenaUserControl();
            _arenaConfigUserControl = new ArenaConfigurationUserControl();
            _statusStrip.SuspendLayout();
            _mainMenuStrip.SuspendLayout();
            _toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_speedTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_splitContainer).BeginInit();
            _splitContainer.Panel1.SuspendLayout();
            _splitContainer.Panel2.SuspendLayout();
            _splitContainer.SuspendLayout();
            _radioCommsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _statusStrip
            // 
            _statusStrip.BackColor = SystemColors.MenuBar;
            _statusStrip.ForeColor = SystemColors.MenuText;
            _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabelState, _statusLabelTick, _statusSepRed, _statusLabelRed, _statusSepBlue, _statusLabelBlue, _statusSepBullets, _statusLabelBullets });
            _statusStrip.Location = new Point(0, 758);
            _statusStrip.Name = "_statusStrip";
            _statusStrip.Size = new Size(1687, 24);
            _statusStrip.TabIndex = 4;
            // 
            // _statusLabelState
            // 
            _statusLabelState.ForeColor = SystemColors.MenuText;
            _statusLabelState.Name = "_statusLabelState";
            _statusLabelState.Size = new Size(39, 19);
            _statusLabelState.Text = "Ready";
            // 
            // _statusLabelTick
            // 
            _statusLabelTick.BorderSides = ToolStripStatusLabelBorderSides.Left;
            _statusLabelTick.ForeColor = SystemColors.MenuText;
            _statusLabelTick.Name = "_statusLabelTick";
            _statusLabelTick.Size = new Size(45, 19);
            _statusLabelTick.Text = "Tick: 0";
            // 
            // _statusSepRed
            // 
            _statusSepRed.BorderSides = ToolStripStatusLabelBorderSides.Left;
            _statusSepRed.ForeColor = Color.OrangeRed;
            _statusSepRed.Name = "_statusSepRed";
            _statusSepRed.Size = new Size(34, 19);
            _statusSepRed.Text = "Red:";
            // 
            // _statusLabelRed
            // 
            _statusLabelRed.ForeColor = Color.OrangeRed;
            _statusLabelRed.Name = "_statusLabelRed";
            _statusLabelRed.Size = new Size(30, 19);
            _statusLabelRed.Text = "0 / 0";
            // 
            // _statusSepBlue
            // 
            _statusSepBlue.BorderSides = ToolStripStatusLabelBorderSides.Left;
            _statusSepBlue.ForeColor = Color.RoyalBlue;
            _statusSepBlue.Name = "_statusSepBlue";
            _statusSepBlue.Size = new Size(37, 19);
            _statusSepBlue.Text = "Blue:";
            // 
            // _statusLabelBlue
            // 
            _statusLabelBlue.ForeColor = Color.RoyalBlue;
            _statusLabelBlue.Name = "_statusLabelBlue";
            _statusLabelBlue.Size = new Size(30, 19);
            _statusLabelBlue.Text = "0 / 0";
            // 
            // _statusSepBullets
            // 
            _statusSepBullets.BorderSides = ToolStripStatusLabelBorderSides.Left;
            _statusSepBullets.ForeColor = SystemColors.MenuText;
            _statusSepBullets.Name = "_statusSepBullets";
            _statusSepBullets.Size = new Size(49, 19);
            _statusSepBullets.Text = "Bullets:";
            //
            // _statusLabelBullets
            //
            _statusLabelBullets.ForeColor = SystemColors.MenuText;
            _statusLabelBullets.Name = "_statusLabelBullets";
            _statusLabelBullets.Size = new Size(13, 19);
            _statusLabelBullets.Text = "0";
            // 
            // _mainMenuStrip
            // 
            _mainMenuStrip.Items.AddRange(new ToolStripItem[] { _menuItemRedSwarm, _menuItemBlueSwarm, _menuItemWar });
            _mainMenuStrip.Location = new Point(0, 0);
            _mainMenuStrip.Name = "_mainMenuStrip";
            _mainMenuStrip.Size = new Size(1687, 24);
            _mainMenuStrip.TabIndex = 1;
            _mainMenuStrip.Text = "menuStrip";
            // 
            // _menuItemRedSwarm
            // 
            _menuItemRedSwarm.DropDownItems.AddRange(new ToolStripItem[] { _menuItemBuildDefaultRedSwarm, _separatorRed1, _menuItemAddRedScout, _menuItemAddRedAttacker, _menuItemAddRedFlanker, _separatorRed2, _menuItemClearAllTanks });
            _menuItemRedSwarm.Name = "_menuItemRedSwarm";
            _menuItemRedSwarm.Size = new Size(78, 20);
            _menuItemRedSwarm.Text = "&Red Swarm";
            // 
            // _menuItemBuildDefaultRedSwarm
            // 
            _menuItemBuildDefaultRedSwarm.Name = "_menuItemBuildDefaultRedSwarm";
            _menuItemBuildDefaultRedSwarm.Size = new Size(204, 22);
            _menuItemBuildDefaultRedSwarm.Text = "Build &Default Red Swarm";
            _menuItemBuildDefaultRedSwarm.Click += MenuItemBuildDefaultRedSwarm_Click;
            // 
            // _separatorRed1
            // 
            _separatorRed1.Name = "_separatorRed1";
            _separatorRed1.Size = new Size(201, 6);
            // 
            // _menuItemAddRedScout
            // 
            _menuItemAddRedScout.Name = "_menuItemAddRedScout";
            _menuItemAddRedScout.Size = new Size(204, 22);
            _menuItemAddRedScout.Text = "Add Red &Scout";
            _menuItemAddRedScout.Click += MenuItemAddRedScout_Click;
            // 
            // _menuItemAddRedAttacker
            // 
            _menuItemAddRedAttacker.Name = "_menuItemAddRedAttacker";
            _menuItemAddRedAttacker.Size = new Size(204, 22);
            _menuItemAddRedAttacker.Text = "Add Red &Attacker";
            _menuItemAddRedAttacker.Click += MenuItemAddRedAttacker_Click;
            // 
            // _menuItemAddRedFlanker
            // 
            _menuItemAddRedFlanker.Name = "_menuItemAddRedFlanker";
            _menuItemAddRedFlanker.Size = new Size(204, 22);
            _menuItemAddRedFlanker.Text = "Add Red &Flanker";
            _menuItemAddRedFlanker.Click += MenuItemAddRedFlanker_Click;
            // 
            // _separatorRed2
            // 
            _separatorRed2.Name = "_separatorRed2";
            _separatorRed2.Size = new Size(201, 6);
            // 
            // _menuItemClearAllTanks
            // 
            _menuItemClearAllTanks.Name = "_menuItemClearAllTanks";
            _menuItemClearAllTanks.Size = new Size(204, 22);
            _menuItemClearAllTanks.Text = "&Clear All Tanks";
            _menuItemClearAllTanks.Click += MenuItemClearAllTanks_Click;
            // 
            // _menuItemBlueSwarm
            // 
            _menuItemBlueSwarm.DropDownItems.AddRange(new ToolStripItem[] { _menuItemBuildDefaultBlueSwarm, _separatorBlue1, _menuItemAddBlueWarden, _menuItemAddBluePatrol, _menuItemAddBlueSniper, _menuItemAddBlueCommander, _separatorBlue2, _menuItemClearAllTanks2 });
            _menuItemBlueSwarm.Name = "_menuItemBlueSwarm";
            _menuItemBlueSwarm.Size = new Size(81, 20);
            _menuItemBlueSwarm.Text = "&Blue Swarm";
            // 
            // _menuItemBuildDefaultBlueSwarm
            // 
            _menuItemBuildDefaultBlueSwarm.Name = "_menuItemBuildDefaultBlueSwarm";
            _menuItemBuildDefaultBlueSwarm.Size = new Size(207, 22);
            _menuItemBuildDefaultBlueSwarm.Text = "Build &Default Blue Swarm";
            _menuItemBuildDefaultBlueSwarm.Click += MenuItemBuildDefaultBlueSwarm_Click;
            // 
            // _separatorBlue1
            // 
            _separatorBlue1.Name = "_separatorBlue1";
            _separatorBlue1.Size = new Size(204, 6);
            // 
            // _menuItemAddBlueWarden
            // 
            _menuItemAddBlueWarden.Name = "_menuItemAddBlueWarden";
            _menuItemAddBlueWarden.Size = new Size(207, 22);
            _menuItemAddBlueWarden.Text = "Add Blue &Warden";
            _menuItemAddBlueWarden.Click += MenuItemAddBlueWarden_Click;
            // 
            // _menuItemAddBluePatrol
            // 
            _menuItemAddBluePatrol.Name = "_menuItemAddBluePatrol";
            _menuItemAddBluePatrol.Size = new Size(207, 22);
            _menuItemAddBluePatrol.Text = "Add Blue &Patrol";
            _menuItemAddBluePatrol.Click += MenuItemAddBluePatrol_Click;
            // 
            // _menuItemAddBlueSniper
            // 
            _menuItemAddBlueSniper.Name = "_menuItemAddBlueSniper";
            _menuItemAddBlueSniper.Size = new Size(207, 22);
            _menuItemAddBlueSniper.Text = "Add Blue &Sniper";
            _menuItemAddBlueSniper.Click += MenuItemAddBlueSniper_Click;
            // 
            // _menuItemAddBlueCommander
            // 
            _menuItemAddBlueCommander.Name = "_menuItemAddBlueCommander";
            _menuItemAddBlueCommander.Size = new Size(207, 22);
            _menuItemAddBlueCommander.Text = "Add Blue &Commander";
            _menuItemAddBlueCommander.Click += MenuItemAddBlueCommander_Click;
            // 
            // _separatorBlue2
            // 
            _separatorBlue2.Name = "_separatorBlue2";
            _separatorBlue2.Size = new Size(204, 6);
            // 
            // _menuItemClearAllTanks2
            // 
            _menuItemClearAllTanks2.Name = "_menuItemClearAllTanks2";
            _menuItemClearAllTanks2.Size = new Size(207, 22);
            _menuItemClearAllTanks2.Text = "&Clear All Tanks";
            _menuItemClearAllTanks2.Click += MenuItemClearAllTanks_Click;
            // 
            // _menuItemWar
            // 
            _menuItemWar.DropDownItems.AddRange(new ToolStripItem[] { _menuItemStart, _menuItemStop, _separatorWar1, _menuItemSingleStep, _separatorWar2, _menuItemResetArena, _separatorWar3, _menuItemSpeed, _separatorWar4, _menuItemPlayerVsPlayer });
            _menuItemWar.Name = "_menuItemWar";
            _menuItemWar.Size = new Size(40, 20);
            _menuItemWar.Text = "&War";
            // 
            // _menuItemStart
            // 
            _menuItemStart.Name = "_menuItemStart";
            _menuItemStart.ShortcutKeys = Keys.F5;
            _menuItemStart.Size = new Size(157, 22);
            _menuItemStart.Text = "&Start";
            _menuItemStart.Click += MenuItemStart_Click;
            // 
            // _menuItemStop
            // 
            _menuItemStop.Enabled = false;
            _menuItemStop.Name = "_menuItemStop";
            _menuItemStop.ShortcutKeys = Keys.F6;
            _menuItemStop.Size = new Size(157, 22);
            _menuItemStop.Text = "S&top";
            _menuItemStop.Click += MenuItemStop_Click;
            // 
            // _separatorWar1
            // 
            _separatorWar1.Name = "_separatorWar1";
            _separatorWar1.Size = new Size(154, 6);
            // 
            // _menuItemSingleStep
            // 
            _menuItemSingleStep.Name = "_menuItemSingleStep";
            _menuItemSingleStep.ShortcutKeys = Keys.F10;
            _menuItemSingleStep.Size = new Size(157, 22);
            _menuItemSingleStep.Text = "S&ingle Step";
            _menuItemSingleStep.Click += MenuItemSingleStep_Click;
            // 
            // _separatorWar2
            // 
            _separatorWar2.Name = "_separatorWar2";
            _separatorWar2.Size = new Size(154, 6);
            // 
            // _menuItemResetArena
            // 
            _menuItemResetArena.Name = "_menuItemResetArena";
            _menuItemResetArena.Size = new Size(157, 22);
            _menuItemResetArena.Text = "&Reset Arena";
            _menuItemResetArena.Click += MenuItemResetArena_Click;
            // 
            // _separatorWar3
            // 
            _separatorWar3.Name = "_separatorWar3";
            _separatorWar3.Size = new Size(154, 6);
            // 
            // _menuItemSpeed
            // 
            _menuItemSpeed.DropDownItems.AddRange(new ToolStripItem[] { _menuItemSpeedSlow, _menuItemSpeedNormal, _menuItemSpeedFast, _menuItemSpeedVeryFast });
            _menuItemSpeed.Name = "_menuItemSpeed";
            _menuItemSpeed.Size = new Size(157, 22);
            _menuItemSpeed.Text = "S&peed";
            // 
            // _menuItemSpeedSlow
            // 
            _menuItemSpeedSlow.Name = "_menuItemSpeedSlow";
            _menuItemSpeedSlow.Size = new Size(166, 22);
            _menuItemSpeedSlow.Text = "&Slow (3 TPS)";
            _menuItemSpeedSlow.Click += MenuItemSpeed_Click;
            // 
            // _menuItemSpeedNormal
            // 
            _menuItemSpeedNormal.Checked = true;
            _menuItemSpeedNormal.CheckState = CheckState.Checked;
            _menuItemSpeedNormal.Name = "_menuItemSpeedNormal";
            _menuItemSpeedNormal.Size = new Size(166, 22);
            _menuItemSpeedNormal.Text = "&Normal (10 TPS)";
            _menuItemSpeedNormal.Click += MenuItemSpeed_Click;
            // 
            // _menuItemSpeedFast
            // 
            _menuItemSpeedFast.Name = "_menuItemSpeedFast";
            _menuItemSpeedFast.Size = new Size(166, 22);
            _menuItemSpeedFast.Text = "&Fast (20 TPS)";
            _menuItemSpeedFast.Click += MenuItemSpeed_Click;
            // 
            // _menuItemSpeedVeryFast
            // 
            _menuItemSpeedVeryFast.Name = "_menuItemSpeedVeryFast";
            _menuItemSpeedVeryFast.Size = new Size(166, 22);
            _menuItemSpeedVeryFast.Text = "&Very Fast (30 TPS)";
            _menuItemSpeedVeryFast.Click += MenuItemSpeed_Click;
            // 
            // _separatorWar4
            // 
            _separatorWar4.Name = "_separatorWar4";
            _separatorWar4.Size = new Size(154, 6);
            // 
            // _menuItemPlayerVsPlayer
            // 
            _menuItemPlayerVsPlayer.DropDownItems.AddRange(new ToolStripItem[] { _menuItemPvP1v1, _menuItemPvP2v2 });
            _menuItemPlayerVsPlayer.Name = "_menuItemPlayerVsPlayer";
            _menuItemPlayerVsPlayer.Size = new Size(157, 22);
            _menuItemPlayerVsPlayer.Text = "&Player vs Player";
            // 
            // _menuItemPvP1v1
            // 
            _menuItemPvP1v1.Name = "_menuItemPvP1v1";
            _menuItemPvP1v1.Size = new Size(103, 22);
            _menuItemPvP1v1.Text = "&1 vs 1";
            _menuItemPvP1v1.Click += MenuItemPvP1v1_Click;
            // 
            // _menuItemPvP2v2
            // 
            _menuItemPvP2v2.Name = "_menuItemPvP2v2";
            _menuItemPvP2v2.Size = new Size(103, 22);
            _menuItemPvP2v2.Text = "&2 vs 2";
            _menuItemPvP2v2.Click += MenuItemPvP2v2_Click;
            // 
            // _toolStrip
            // 
            _toolStrip.Items.AddRange(new ToolStripItem[] { _btnStartWar, _btnStopWar, _separatorToolbar1, _btnSingleStep, _separatorToolbar2, _lblSpeed, _speedTrackBarHost, _lblSpeedValue });
            _toolStrip.Location = new Point(0, 24);
            _toolStrip.Name = "_toolStrip";
            _toolStrip.Size = new Size(1687, 25);
            _toolStrip.TabIndex = 2;
            _toolStrip.Text = "toolStrip";
            // 
            // _btnStartWar
            // 
            _btnStartWar.Name = "_btnStartWar";
            _btnStartWar.Size = new Size(75, 22);
            _btnStartWar.Text = "▶  Start War";
            _btnStartWar.ToolTipText = "Start simulation (F5)";
            _btnStartWar.Click += MenuItemStart_Click;
            // 
            // _btnStopWar
            // 
            _btnStopWar.Enabled = false;
            _btnStopWar.Name = "_btnStopWar";
            _btnStopWar.Size = new Size(53, 22);
            _btnStopWar.Text = "⏹  Stop";
            _btnStopWar.ToolTipText = "Stop simulation (F6)";
            _btnStopWar.Click += MenuItemStop_Click;
            // 
            // _separatorToolbar1
            // 
            _separatorToolbar1.Name = "_separatorToolbar1";
            _separatorToolbar1.Size = new Size(6, 25);
            // 
            // _btnSingleStep
            // 
            _btnSingleStep.Name = "_btnSingleStep";
            _btnSingleStep.Size = new Size(51, 22);
            _btnSingleStep.Text = "→▏  Step";
            _btnSingleStep.ToolTipText = "Advance one tick (F10)";
            _btnSingleStep.Click += MenuItemSingleStep_Click;
            // 
            // _separatorToolbar2
            // 
            _separatorToolbar2.Name = "_separatorToolbar2";
            _separatorToolbar2.Size = new Size(6, 25);
            // 
            // _lblSpeed
            // 
            _lblSpeed.Name = "_lblSpeed";
            _lblSpeed.Size = new Size(42, 22);
            _lblSpeed.Text = "Speed:";
            //
            // _speedTrackBar
            //
            _speedTrackBar.AccessibleName = "_speedTrackBar";
            _speedTrackBar.AutoSize = false;
            _speedTrackBar.Location = new Point(242, 1);
            _speedTrackBar.Maximum = 30;
            _speedTrackBar.Minimum = 1;
            _speedTrackBar.Name = "_speedTrackBar";
            _speedTrackBar.Size = new Size(154, 22);
            _speedTrackBar.TabIndex = 0;
            _speedTrackBar.TickFrequency = 5;
            _speedTrackBar.Value = 10;
            _speedTrackBar.Scroll += SpeedTrackBar_Scroll;
            //
            // _speedTrackBarHost
            //
            _speedTrackBarHost.AutoSize = false;
            _speedTrackBarHost.Name = "_speedTrackBarHost";
            _speedTrackBarHost.Size = new Size(154, 22);
            // 
            // _lblSpeedValue
            // 
            _lblSpeedValue.Name = "_lblSpeedValue";
            _lblSpeedValue.Size = new Size(42, 22);
            _lblSpeedValue.Text = "10 TPS";
            // 
            // _splitContainer
            // 
            _splitContainer.BorderStyle = BorderStyle.Fixed3D;
            _splitContainer.Dock = DockStyle.Fill;
            _splitContainer.FixedPanel = FixedPanel.Panel1;
            _splitContainer.Location = new Point(0, 49);
            _splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            _splitContainer.Panel1.Controls.Add(_radioCommsPanel);
            // 
            // _splitContainer.Panel2
            // 
            _splitContainer.Panel2.Controls.Add(arenaUserControl1);
            _splitContainer.Panel2.Controls.Add(_arenaConfigUserControl);
            _splitContainer.Size = new Size(1687, 709);
            _splitContainer.SplitterWidth = 5;
            _splitContainer.TabIndex = 3;
            // 
            // _radioCommsPanel
            // 
            _radioCommsPanel.BackColor = Color.FromArgb(12, 12, 12);
            _radioCommsPanel.Controls.Add(_radioLog);
            _radioCommsPanel.Controls.Add(_radioCommsHeader);
            _radioCommsPanel.Dock = DockStyle.Fill;
            _radioCommsPanel.Location = new Point(0, 0);
            _radioCommsPanel.Name = "_radioCommsPanel";
            _radioCommsPanel.Size = new Size(46, 705);
            _radioCommsPanel.TabIndex = 0;
            // 
            // _radioLog
            // 
            _radioLog.BackColor = Color.FromArgb(12, 12, 12);
            _radioLog.BorderStyle = BorderStyle.None;
            _radioLog.Dock = DockStyle.Fill;
            _radioLog.Font = new Font("Consolas", 8.25F);
            _radioLog.ForeColor = Color.DimGray;
            _radioLog.Location = new Point(0, 18);
            _radioLog.Name = "_radioLog";
            _radioLog.ReadOnly = true;
            _radioLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            _radioLog.Size = new Size(46, 687);
            _radioLog.TabIndex = 0;
            _radioLog.TabStop = false;
            _radioLog.Text = "";
            // 
            // _radioCommsHeader
            // 
            _radioCommsHeader.BackColor = Color.FromArgb(28, 28, 28);
            _radioCommsHeader.Dock = DockStyle.Top;
            _radioCommsHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _radioCommsHeader.ForeColor = Color.Goldenrod;
            _radioCommsHeader.Location = new Point(0, 0);
            _radioCommsHeader.Name = "_radioCommsHeader";
            _radioCommsHeader.Size = new Size(46, 18);
            _radioCommsHeader.TabIndex = 1;
            _radioCommsHeader.Text = "  ◼ RADIO COMMS";
            _radioCommsHeader.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // arenaUserControl1
            // 
            arenaUserControl1.Dock = DockStyle.Fill;
            arenaUserControl1.Location = new Point(0, 0);
            arenaUserControl1.Name = "arenaUserControl1";
            arenaUserControl1.Size = new Size(1408, 705);
            arenaUserControl1.TabIndex = 0;
            // 
            // _arenaConfigUserControl
            // 
            _arenaConfigUserControl.BackColor = SystemColors.MenuBar;
            _arenaConfigUserControl.Dock = DockStyle.Right;
            _arenaConfigUserControl.Location = new Point(1408, 0);
            _arenaConfigUserControl.Name = "_arenaConfigUserControl";
            _arenaConfigUserControl.Size = new Size(220, 705);
            _arenaConfigUserControl.TabIndex = 1;
            // 
            // TankSwarmArena
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1687, 782);
            Controls.Add(_splitContainer);
            Controls.Add(_statusStrip);
            Controls.Add(_toolStrip);
            Controls.Add(_mainMenuStrip);
            MainMenuStrip = _mainMenuStrip;
            MinimizeBox = false;
            MinimumSize = new Size(700, 500);
            Name = "TankSwarmArena";
            ShowIcon = false;
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tank Swarm Arena";
            _statusStrip.ResumeLayout(false);
            _statusStrip.PerformLayout();
            _mainMenuStrip.ResumeLayout(false);
            _mainMenuStrip.PerformLayout();
            _toolStrip.ResumeLayout(false);
            _toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_speedTrackBar).EndInit();
            _splitContainer.Panel1.ResumeLayout(false);
            _splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_splitContainer).EndInit();
            _splitContainer.ResumeLayout(false);
            _radioCommsPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ArenaUserControl arenaUserControl1;
        private ArenaConfigurationUserControl _arenaConfigUserControl;
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
        private ToolStripMenuItem _menuItemAddBlueCommander;
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
        private ToolStripLabel _lblSpeedValue;

        // Split container + radio comms panel
        private SplitContainer _splitContainer;
        private Panel _radioCommsPanel;
        private Label _radioCommsHeader;
        private RichTextBox _radioLog;

        // Status strip
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabelState;
        private ToolStripStatusLabel _statusLabelTick;
        private ToolStripStatusLabel _statusSepRed;
        private ToolStripStatusLabel _statusLabelRed;
        private ToolStripStatusLabel _statusSepBlue;
        private ToolStripStatusLabel _statusLabelBlue;
        private ToolStripStatusLabel _statusSepBullets;
        private ToolStripStatusLabel _statusLabelBullets;
        private ToolStripControlHost _speedTrackBarHost;
    }
}

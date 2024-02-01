
namespace WindowsVirtualDesktopHelper {
    partial class SettingsForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.notifyIconNumber = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDonate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIconPrev = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyIconNext = new System.Windows.Forms.NotifyIcon(this.components);
            this.checkBoxShowPrevNextIcons = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.radioButtonUseHotKeysToJumpToDesktopAlt = new System.Windows.Forms.RadioButton();
            this.radioButtonUseHotKeysToJumpToDesktopAltShift = new System.Windows.Forms.RadioButton();
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt = new System.Windows.Forms.RadioButton();
            this.radioButtonUseHotKeysToJumpToDesktopCtrl = new System.Windows.Forms.RadioButton();
            this.checkBoxUseHotKeysToJumpToDesktop = new System.Windows.Forms.CheckBox();
            this.checkBoxOverlayShowOnAllMonitors = new System.Windows.Forms.CheckBox();
            this.checkBoxShowDesktopNameInitial = new System.Windows.Forms.CheckBox();
            this.checkBoxClickDesktopNumberTaskView = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.radioButtonOverlayMicroDuration = new System.Windows.Forms.RadioButton();
            this.radioButtonOverlayLongDuration = new System.Windows.Forms.RadioButton();
            this.radioButtonOverlayMediumDuration = new System.Windows.Forms.RadioButton();
            this.radioButtonOverlayShortDuration = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButtonPositionBottomRight = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionBottomCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionBottomLeft = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionMiddleRight = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionMiddleCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionMiddleLeft = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionTopRight = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionTopCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionTopLeft = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxOverlayTranslucent = new System.Windows.Forms.CheckBox();
            this.checkBoxOverlayAnimate = new System.Windows.Forms.CheckBox();
            this.checkBoxShowOverlay = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxStartupWithWindows = new System.Windows.Forms.CheckBox();
            this.notifyIconName = new System.Windows.Forms.NotifyIcon(this.components);
            this.checkBoxUseHotKeyToJumpToMostRecentDesktop = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIconNumber
            // 
            this.notifyIconNumber.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIconNumber.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconNumber.Icon")));
            this.notifyIconNumber.Text = "Desktop Number";
            this.notifyIconNumber.Visible = true;
            this.notifyIconNumber.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIconNumber_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAbout,
            this.toolStripMenuItemDonate,
            this.toolStripMenuItemSettings,
            this.toolStripMenuItemExit});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(149, 132);
            this.contextMenuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuStrip1_ItemClicked);
            // 
            // toolStripMenuItemAbout
            // 
            this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            this.toolStripMenuItemAbout.Size = new System.Drawing.Size(148, 32);
            this.toolStripMenuItemAbout.Tag = "about";
            this.toolStripMenuItemAbout.Text = "About";
            // 
            // toolStripMenuItemDonate
            // 
            this.toolStripMenuItemDonate.Name = "toolStripMenuItemDonate";
            this.toolStripMenuItemDonate.Size = new System.Drawing.Size(148, 32);
            this.toolStripMenuItemDonate.Tag = "donate";
            this.toolStripMenuItemDonate.Text = "Donate";
            // 
            // toolStripMenuItemSettings
            // 
            this.toolStripMenuItemSettings.Name = "toolStripMenuItemSettings";
            this.toolStripMenuItemSettings.Size = new System.Drawing.Size(148, 32);
            this.toolStripMenuItemSettings.Tag = "settings";
            this.toolStripMenuItemSettings.Text = "Settings";
            // 
            // toolStripMenuItemExit
            // 
            this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            this.toolStripMenuItemExit.Size = new System.Drawing.Size(148, 32);
            this.toolStripMenuItemExit.Tag = "exit";
            this.toolStripMenuItemExit.Text = "Exit";
            // 
            // notifyIconPrev
            // 
            this.notifyIconPrev.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconPrev.Icon")));
            this.notifyIconPrev.Text = "Previous Desktop";
            this.notifyIconPrev.Visible = true;
            this.notifyIconPrev.Click += new System.EventHandler(this.notifyIconPrev_Click);
            // 
            // notifyIconNext
            // 
            this.notifyIconNext.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconNext.Icon")));
            this.notifyIconNext.Text = "Next Desktop";
            this.notifyIconNext.Visible = true;
            this.notifyIconNext.Click += new System.EventHandler(this.notifyIconNext_Click);
            // 
            // checkBoxShowPrevNextIcons
            // 
            this.checkBoxShowPrevNextIcons.AutoSize = true;
            this.checkBoxShowPrevNextIcons.Location = new System.Drawing.Point(23, 29);
            this.checkBoxShowPrevNextIcons.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxShowPrevNextIcons.Name = "checkBoxShowPrevNextIcons";
            this.checkBoxShowPrevNextIcons.Size = new System.Drawing.Size(332, 24);
            this.checkBoxShowPrevNextIcons.TabIndex = 1;
            this.checkBoxShowPrevNextIcons.Text = "Show Previous / Next Desktop in Icon Tray";
            this.checkBoxShowPrevNextIcons.UseVisualStyleBackColor = true;
            this.checkBoxShowPrevNextIcons.CheckedChanged += new System.EventHandler(this.checkBoxShowPrevNextIcons_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxUseHotKeyToJumpToMostRecentDesktop);
            this.groupBox1.Controls.Add(this.panel3);
            this.groupBox1.Controls.Add(this.checkBoxUseHotKeysToJumpToDesktop);
            this.groupBox1.Controls.Add(this.checkBoxOverlayShowOnAllMonitors);
            this.groupBox1.Controls.Add(this.checkBoxShowDesktopNameInitial);
            this.groupBox1.Controls.Add(this.checkBoxClickDesktopNumberTaskView);
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkBoxOverlayTranslucent);
            this.groupBox1.Controls.Add(this.checkBoxOverlayAnimate);
            this.groupBox1.Controls.Add(this.checkBoxShowOverlay);
            this.groupBox1.Controls.Add(this.checkBoxShowPrevNextIcons);
            this.groupBox1.Location = new System.Drawing.Point(18, 3);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.groupBox1.Size = new System.Drawing.Size(619, 501);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Features";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.radioButtonUseHotKeysToJumpToDesktopAlt);
            this.panel3.Controls.Add(this.radioButtonUseHotKeysToJumpToDesktopAltShift);
            this.panel3.Controls.Add(this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt);
            this.panel3.Controls.Add(this.radioButtonUseHotKeysToJumpToDesktopCtrl);
            this.panel3.Location = new System.Drawing.Point(60, 424);
            this.panel3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(540, 38);
            this.panel3.TabIndex = 20;
            // 
            // radioButtonUseHotKeysToJumpToDesktopAlt
            // 
            this.radioButtonUseHotKeysToJumpToDesktopAlt.AutoSize = true;
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Location = new System.Drawing.Point(7, 8);
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Name = "radioButtonUseHotKeysToJumpToDesktopAlt";
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Size = new System.Drawing.Size(71, 24);
            this.radioButtonUseHotKeysToJumpToDesktopAlt.TabIndex = 9;
            this.radioButtonUseHotKeysToJumpToDesktopAlt.TabStop = true;
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Text = "Alt+#";
            this.radioButtonUseHotKeysToJumpToDesktopAlt.UseVisualStyleBackColor = true;
            this.radioButtonUseHotKeysToJumpToDesktopAlt.CheckedChanged += new System.EventHandler(this.radioButtonUseHotKeysToJumpToDesktopAlt_CheckedChanged);
            // 
            // radioButtonUseHotKeysToJumpToDesktopAltShift
            // 
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.AutoSize = true;
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Location = new System.Drawing.Point(281, 8);
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Name = "radioButtonUseHotKeysToJumpToDesktopAltShift";
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Size = new System.Drawing.Size(113, 24);
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.TabIndex = 8;
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.TabStop = true;
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Text = "Alt+Shift+#";
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.UseVisualStyleBackColor = true;
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.CheckedChanged += new System.EventHandler(this.radioButtonUseHotKeysToJumpToDesktopAltShift_CheckedChanged);
            // 
            // radioButtonUseHotKeysToJumpToDesktopCtrlAlt
            // 
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.AutoSize = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Location = new System.Drawing.Point(169, 8);
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Name = "radioButtonUseHotKeysToJumpToDesktopCtrlAlt";
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Size = new System.Drawing.Size(104, 24);
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.TabIndex = 7;
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.TabStop = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Text = "Ctrl+Alt+#";
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.UseVisualStyleBackColor = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.CheckedChanged += new System.EventHandler(this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt_CheckedChanged);
            // 
            // radioButtonUseHotKeysToJumpToDesktopCtrl
            // 
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.AutoSize = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Location = new System.Drawing.Point(85, 8);
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Name = "radioButtonUseHotKeysToJumpToDesktopCtrl";
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Size = new System.Drawing.Size(76, 24);
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.TabIndex = 6;
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.TabStop = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Text = "Ctrl+#";
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.UseVisualStyleBackColor = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.CheckedChanged += new System.EventHandler(this.radioButtonUseHotKeysToJumpToDesktopCtrl_CheckedChanged);
            // 
            // checkBoxUseHotKeysToJumpToDesktop
            // 
            this.checkBoxUseHotKeysToJumpToDesktop.AutoSize = true;
            this.checkBoxUseHotKeysToJumpToDesktop.Location = new System.Drawing.Point(23, 402);
            this.checkBoxUseHotKeysToJumpToDesktop.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxUseHotKeysToJumpToDesktop.Name = "checkBoxUseHotKeysToJumpToDesktop";
            this.checkBoxUseHotKeysToJumpToDesktop.Size = new System.Drawing.Size(275, 24);
            this.checkBoxUseHotKeysToJumpToDesktop.TabIndex = 23;
            this.checkBoxUseHotKeysToJumpToDesktop.Text = "Use Hot Keys to Jump to Desktop";
            this.checkBoxUseHotKeysToJumpToDesktop.UseVisualStyleBackColor = true;
            this.checkBoxUseHotKeysToJumpToDesktop.CheckedChanged += new System.EventHandler(this.checkBoxUseHotKeysToJumpToDesktop_CheckedChanged);
            // 
            // checkBoxOverlayShowOnAllMonitors
            // 
            this.checkBoxOverlayShowOnAllMonitors.AutoSize = true;
            this.checkBoxOverlayShowOnAllMonitors.Location = new System.Drawing.Point(64, 204);
            this.checkBoxOverlayShowOnAllMonitors.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxOverlayShowOnAllMonitors.Name = "checkBoxOverlayShowOnAllMonitors";
            this.checkBoxOverlayShowOnAllMonitors.Size = new System.Drawing.Size(181, 24);
            this.checkBoxOverlayShowOnAllMonitors.TabIndex = 22;
            this.checkBoxOverlayShowOnAllMonitors.Text = "Show on all Monitors";
            this.checkBoxOverlayShowOnAllMonitors.UseVisualStyleBackColor = true;
            this.checkBoxOverlayShowOnAllMonitors.CheckedChanged += new System.EventHandler(this.checkBoxOverlayShowOnAllMonitors_CheckedChanged);
            // 
            // checkBoxShowDesktopNameInitial
            // 
            this.checkBoxShowDesktopNameInitial.AutoSize = true;
            this.checkBoxShowDesktopNameInitial.Location = new System.Drawing.Point(23, 368);
            this.checkBoxShowDesktopNameInitial.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxShowDesktopNameInitial.Name = "checkBoxShowDesktopNameInitial";
            this.checkBoxShowDesktopNameInitial.Size = new System.Drawing.Size(311, 24);
            this.checkBoxShowDesktopNameInitial.TabIndex = 21;
            this.checkBoxShowDesktopNameInitial.Text = "Show Desktop Name Initial in Icon Tray";
            this.checkBoxShowDesktopNameInitial.UseVisualStyleBackColor = true;
            this.checkBoxShowDesktopNameInitial.CheckedChanged += new System.EventHandler(this.checkBoxShowDesktopNameInitial_CheckedChanged);
            // 
            // checkBoxClickDesktopNumberTaskView
            // 
            this.checkBoxClickDesktopNumberTaskView.AutoSize = true;
            this.checkBoxClickDesktopNumberTaskView.Location = new System.Drawing.Point(23, 333);
            this.checkBoxClickDesktopNumberTaskView.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxClickDesktopNumberTaskView.Name = "checkBoxClickDesktopNumberTaskView";
            this.checkBoxClickDesktopNumberTaskView.Size = new System.Drawing.Size(422, 24);
            this.checkBoxClickDesktopNumberTaskView.TabIndex = 20;
            this.checkBoxClickDesktopNumberTaskView.Text = "Clicking Desktop Number in Icon Tray opens Task View";
            this.checkBoxClickDesktopNumberTaskView.UseVisualStyleBackColor = true;
            this.checkBoxClickDesktopNumberTaskView.CheckedChanged += new System.EventHandler(this.checkBoxClickDesktopNumberTaskView_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.radioButtonOverlayMicroDuration);
            this.panel2.Controls.Add(this.radioButtonOverlayLongDuration);
            this.panel2.Controls.Add(this.radioButtonOverlayMediumDuration);
            this.panel2.Controls.Add(this.radioButtonOverlayShortDuration);
            this.panel2.Location = new System.Drawing.Point(60, 95);
            this.panel2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(540, 38);
            this.panel2.TabIndex = 19;
            // 
            // radioButtonOverlayMicroDuration
            // 
            this.radioButtonOverlayMicroDuration.AutoSize = true;
            this.radioButtonOverlayMicroDuration.Location = new System.Drawing.Point(7, 8);
            this.radioButtonOverlayMicroDuration.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonOverlayMicroDuration.Name = "radioButtonOverlayMicroDuration";
            this.radioButtonOverlayMicroDuration.Size = new System.Drawing.Size(82, 24);
            this.radioButtonOverlayMicroDuration.TabIndex = 9;
            this.radioButtonOverlayMicroDuration.TabStop = true;
            this.radioButtonOverlayMicroDuration.Text = "500ms";
            this.radioButtonOverlayMicroDuration.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverlayLongDuration
            // 
            this.radioButtonOverlayLongDuration.AutoSize = true;
            this.radioButtonOverlayLongDuration.Location = new System.Drawing.Point(304, 8);
            this.radioButtonOverlayLongDuration.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonOverlayLongDuration.Name = "radioButtonOverlayLongDuration";
            this.radioButtonOverlayLongDuration.Size = new System.Drawing.Size(91, 24);
            this.radioButtonOverlayLongDuration.TabIndex = 8;
            this.radioButtonOverlayLongDuration.TabStop = true;
            this.radioButtonOverlayLongDuration.Text = "3000ms";
            this.radioButtonOverlayLongDuration.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverlayMediumDuration
            // 
            this.radioButtonOverlayMediumDuration.AutoSize = true;
            this.radioButtonOverlayMediumDuration.Location = new System.Drawing.Point(205, 8);
            this.radioButtonOverlayMediumDuration.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonOverlayMediumDuration.Name = "radioButtonOverlayMediumDuration";
            this.radioButtonOverlayMediumDuration.Size = new System.Drawing.Size(91, 24);
            this.radioButtonOverlayMediumDuration.TabIndex = 7;
            this.radioButtonOverlayMediumDuration.TabStop = true;
            this.radioButtonOverlayMediumDuration.Text = "2000ms";
            this.radioButtonOverlayMediumDuration.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverlayShortDuration
            // 
            this.radioButtonOverlayShortDuration.AutoSize = true;
            this.radioButtonOverlayShortDuration.Location = new System.Drawing.Point(104, 8);
            this.radioButtonOverlayShortDuration.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonOverlayShortDuration.Name = "radioButtonOverlayShortDuration";
            this.radioButtonOverlayShortDuration.Size = new System.Drawing.Size(91, 24);
            this.radioButtonOverlayShortDuration.TabIndex = 6;
            this.radioButtonOverlayShortDuration.TabStop = true;
            this.radioButtonOverlayShortDuration.Text = "1000ms";
            this.radioButtonOverlayShortDuration.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonPositionBottomRight);
            this.panel1.Controls.Add(this.radioButtonPositionBottomCenter);
            this.panel1.Controls.Add(this.radioButtonPositionBottomLeft);
            this.panel1.Controls.Add(this.radioButtonPositionMiddleRight);
            this.panel1.Controls.Add(this.radioButtonPositionMiddleCenter);
            this.panel1.Controls.Add(this.radioButtonPositionMiddleLeft);
            this.panel1.Controls.Add(this.radioButtonPositionTopRight);
            this.panel1.Controls.Add(this.radioButtonPositionTopCenter);
            this.panel1.Controls.Add(this.radioButtonPositionTopLeft);
            this.panel1.Location = new System.Drawing.Point(131, 233);
            this.panel1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(93, 91);
            this.panel1.TabIndex = 18;
            // 
            // radioButtonPositionBottomRight
            // 
            this.radioButtonPositionBottomRight.AutoSize = true;
            this.radioButtonPositionBottomRight.Location = new System.Drawing.Point(65, 63);
            this.radioButtonPositionBottomRight.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonPositionBottomRight.Name = "radioButtonPositionBottomRight";
            this.radioButtonPositionBottomRight.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionBottomRight.TabIndex = 26;
            this.radioButtonPositionBottomRight.TabStop = true;
            this.radioButtonPositionBottomRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionBottomCenter
            // 
            this.radioButtonPositionBottomCenter.AutoSize = true;
            this.radioButtonPositionBottomCenter.Location = new System.Drawing.Point(34, 63);
            this.radioButtonPositionBottomCenter.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonPositionBottomCenter.Name = "radioButtonPositionBottomCenter";
            this.radioButtonPositionBottomCenter.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionBottomCenter.TabIndex = 25;
            this.radioButtonPositionBottomCenter.TabStop = true;
            this.radioButtonPositionBottomCenter.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionBottomLeft
            // 
            this.radioButtonPositionBottomLeft.AutoSize = true;
            this.radioButtonPositionBottomLeft.Location = new System.Drawing.Point(5, 63);
            this.radioButtonPositionBottomLeft.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonPositionBottomLeft.Name = "radioButtonPositionBottomLeft";
            this.radioButtonPositionBottomLeft.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionBottomLeft.TabIndex = 24;
            this.radioButtonPositionBottomLeft.TabStop = true;
            this.radioButtonPositionBottomLeft.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionMiddleRight
            // 
            this.radioButtonPositionMiddleRight.AutoSize = true;
            this.radioButtonPositionMiddleRight.Location = new System.Drawing.Point(65, 34);
            this.radioButtonPositionMiddleRight.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonPositionMiddleRight.Name = "radioButtonPositionMiddleRight";
            this.radioButtonPositionMiddleRight.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionMiddleRight.TabIndex = 23;
            this.radioButtonPositionMiddleRight.TabStop = true;
            this.radioButtonPositionMiddleRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionMiddleCenter
            // 
            this.radioButtonPositionMiddleCenter.AutoSize = true;
            this.radioButtonPositionMiddleCenter.Location = new System.Drawing.Point(34, 34);
            this.radioButtonPositionMiddleCenter.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonPositionMiddleCenter.Name = "radioButtonPositionMiddleCenter";
            this.radioButtonPositionMiddleCenter.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionMiddleCenter.TabIndex = 22;
            this.radioButtonPositionMiddleCenter.TabStop = true;
            this.radioButtonPositionMiddleCenter.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionMiddleLeft
            // 
            this.radioButtonPositionMiddleLeft.AutoSize = true;
            this.radioButtonPositionMiddleLeft.Location = new System.Drawing.Point(5, 34);
            this.radioButtonPositionMiddleLeft.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonPositionMiddleLeft.Name = "radioButtonPositionMiddleLeft";
            this.radioButtonPositionMiddleLeft.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionMiddleLeft.TabIndex = 21;
            this.radioButtonPositionMiddleLeft.TabStop = true;
            this.radioButtonPositionMiddleLeft.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionTopRight
            // 
            this.radioButtonPositionTopRight.AutoSize = true;
            this.radioButtonPositionTopRight.Location = new System.Drawing.Point(65, 5);
            this.radioButtonPositionTopRight.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonPositionTopRight.Name = "radioButtonPositionTopRight";
            this.radioButtonPositionTopRight.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionTopRight.TabIndex = 20;
            this.radioButtonPositionTopRight.TabStop = true;
            this.radioButtonPositionTopRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionTopCenter
            // 
            this.radioButtonPositionTopCenter.AutoSize = true;
            this.radioButtonPositionTopCenter.Location = new System.Drawing.Point(34, 5);
            this.radioButtonPositionTopCenter.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonPositionTopCenter.Name = "radioButtonPositionTopCenter";
            this.radioButtonPositionTopCenter.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionTopCenter.TabIndex = 19;
            this.radioButtonPositionTopCenter.TabStop = true;
            this.radioButtonPositionTopCenter.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionTopLeft
            // 
            this.radioButtonPositionTopLeft.AutoSize = true;
            this.radioButtonPositionTopLeft.Location = new System.Drawing.Point(5, 5);
            this.radioButtonPositionTopLeft.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.radioButtonPositionTopLeft.Name = "radioButtonPositionTopLeft";
            this.radioButtonPositionTopLeft.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionTopLeft.TabIndex = 18;
            this.radioButtonPositionTopLeft.TabStop = true;
            this.radioButtonPositionTopLeft.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(60, 236);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 20);
            this.label1.TabIndex = 9;
            this.label1.Text = "Position:";
            // 
            // checkBoxOverlayTranslucent
            // 
            this.checkBoxOverlayTranslucent.AutoSize = true;
            this.checkBoxOverlayTranslucent.Location = new System.Drawing.Point(65, 171);
            this.checkBoxOverlayTranslucent.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxOverlayTranslucent.Name = "checkBoxOverlayTranslucent";
            this.checkBoxOverlayTranslucent.Size = new System.Drawing.Size(118, 24);
            this.checkBoxOverlayTranslucent.TabIndex = 7;
            this.checkBoxOverlayTranslucent.Text = "Translucent";
            this.checkBoxOverlayTranslucent.UseVisualStyleBackColor = true;
            // 
            // checkBoxOverlayAnimate
            // 
            this.checkBoxOverlayAnimate.AutoSize = true;
            this.checkBoxOverlayAnimate.Location = new System.Drawing.Point(65, 135);
            this.checkBoxOverlayAnimate.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxOverlayAnimate.Name = "checkBoxOverlayAnimate";
            this.checkBoxOverlayAnimate.Size = new System.Drawing.Size(142, 24);
            this.checkBoxOverlayAnimate.TabIndex = 6;
            this.checkBoxOverlayAnimate.Text = "Animate In/Out";
            this.checkBoxOverlayAnimate.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowOverlay
            // 
            this.checkBoxShowOverlay.AutoSize = true;
            this.checkBoxShowOverlay.Location = new System.Drawing.Point(23, 65);
            this.checkBoxShowOverlay.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxShowOverlay.Name = "checkBoxShowOverlay";
            this.checkBoxShowOverlay.Size = new System.Drawing.Size(306, 24);
            this.checkBoxShowOverlay.TabIndex = 2;
            this.checkBoxShowOverlay.Text = "Show Overlay when switching Desktop";
            this.checkBoxShowOverlay.UseVisualStyleBackColor = true;
            this.checkBoxShowOverlay.CheckedChanged += new System.EventHandler(this.checkBoxShowOverlay_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxStartupWithWindows);
            this.groupBox2.Location = new System.Drawing.Point(18, 506);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.groupBox2.Size = new System.Drawing.Size(619, 74);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Settings";
            // 
            // checkBoxStartupWithWindows
            // 
            this.checkBoxStartupWithWindows.AutoSize = true;
            this.checkBoxStartupWithWindows.Location = new System.Drawing.Point(23, 29);
            this.checkBoxStartupWithWindows.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.checkBoxStartupWithWindows.Name = "checkBoxStartupWithWindows";
            this.checkBoxStartupWithWindows.Size = new System.Drawing.Size(188, 24);
            this.checkBoxStartupWithWindows.TabIndex = 1;
            this.checkBoxStartupWithWindows.Text = "Startup with Windows";
            this.checkBoxStartupWithWindows.UseVisualStyleBackColor = true;
            this.checkBoxStartupWithWindows.CheckedChanged += new System.EventHandler(this.checkBoxStartupWithWindows_CheckedChanged);
            // 
            // notifyIconName
            // 
            this.notifyIconName.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIconName.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconName.Icon")));
            this.notifyIconName.Text = "Desktop Name";
            this.notifyIconName.Visible = true;
            this.notifyIconName.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIconName_MouseClick);
            // 
            // checkBoxUseHotKeyToJumpToMostRecentDesktop
            // 
            this.checkBoxUseHotKeyToJumpToMostRecentDesktop.AutoSize = true;
            this.checkBoxUseHotKeyToJumpToMostRecentDesktop.Location = new System.Drawing.Point(23, 467);
            this.checkBoxUseHotKeyToJumpToMostRecentDesktop.Margin = new System.Windows.Forms.Padding(5);
            this.checkBoxUseHotKeyToJumpToMostRecentDesktop.Name = "checkBoxUseHotKeyToJumpToMostRecentDesktop";
            this.checkBoxUseHotKeyToJumpToMostRecentDesktop.Size = new System.Drawing.Size(458, 24);
            this.checkBoxUseHotKeyToJumpToMostRecentDesktop.TabIndex = 24;
            this.checkBoxUseHotKeyToJumpToMostRecentDesktop.Text = "Use Hot Key (Modifier + 0) to Jump to Most Recent Desktop";
            this.checkBoxUseHotKeyToJumpToMostRecentDesktop.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 594);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingsForm_FormClosed);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIconNumber;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAbout;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
        private System.Windows.Forms.NotifyIcon notifyIconPrev;
        private System.Windows.Forms.NotifyIcon notifyIconNext;
        private System.Windows.Forms.CheckBox checkBoxShowPrevNextIcons;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSettings;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxStartupWithWindows;
        private System.Windows.Forms.CheckBox checkBoxShowOverlay;
        private System.Windows.Forms.CheckBox checkBoxOverlayTranslucent;
        private System.Windows.Forms.CheckBox checkBoxOverlayAnimate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton radioButtonOverlayLongDuration;
        private System.Windows.Forms.RadioButton radioButtonOverlayMediumDuration;
        private System.Windows.Forms.RadioButton radioButtonOverlayShortDuration;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton radioButtonPositionBottomRight;
        private System.Windows.Forms.RadioButton radioButtonPositionBottomCenter;
        private System.Windows.Forms.RadioButton radioButtonPositionBottomLeft;
        private System.Windows.Forms.RadioButton radioButtonPositionMiddleRight;
        private System.Windows.Forms.RadioButton radioButtonPositionMiddleCenter;
        private System.Windows.Forms.RadioButton radioButtonPositionMiddleLeft;
        private System.Windows.Forms.RadioButton radioButtonPositionTopRight;
        private System.Windows.Forms.RadioButton radioButtonPositionTopCenter;
        private System.Windows.Forms.RadioButton radioButtonPositionTopLeft;
        private System.Windows.Forms.RadioButton radioButtonOverlayMicroDuration;
        private System.Windows.Forms.CheckBox checkBoxClickDesktopNumberTaskView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDonate;
        private System.Windows.Forms.NotifyIcon notifyIconName;
		private System.Windows.Forms.CheckBox checkBoxShowDesktopNameInitial;
		private System.Windows.Forms.CheckBox checkBoxOverlayShowOnAllMonitors;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.RadioButton radioButtonUseHotKeysToJumpToDesktopAlt;
		private System.Windows.Forms.RadioButton radioButtonUseHotKeysToJumpToDesktopAltShift;
		private System.Windows.Forms.RadioButton radioButtonUseHotKeysToJumpToDesktopCtrlAlt;
		private System.Windows.Forms.RadioButton radioButtonUseHotKeysToJumpToDesktopCtrl;
		private System.Windows.Forms.CheckBox checkBoxUseHotKeysToJumpToDesktop;
		private System.Windows.Forms.CheckBox checkBoxUseHotKeyToJumpToMostRecentDesktop;
	}
}
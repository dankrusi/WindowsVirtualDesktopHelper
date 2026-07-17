
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
            this.checkBoxStatusOverlayShowOnAllMonitors = new System.Windows.Forms.CheckBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.radioButtonStatusOverlayPositionBottomRight = new System.Windows.Forms.RadioButton();
            this.radioButtonStatusOverlayPositionBottomCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonStatusOverlayPositionBottomLeft = new System.Windows.Forms.RadioButton();
            this.radioButtonStatusOverlayPositionMiddleRight = new System.Windows.Forms.RadioButton();
            this.radioButtonStatusOverlayPositionMiddleCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonStatusOverlayPositionMiddleLeft = new System.Windows.Forms.RadioButton();
            this.radioButtonStatusOverlayPositionTopRight = new System.Windows.Forms.RadioButton();
            this.radioButtonStatusOverlayPositionTopCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonStatusOverlayPositionTopLeft = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxStatusOverlayTranslucent = new System.Windows.Forms.CheckBox();
            this.checkBoxStatusOverlayAnimate = new System.Windows.Forms.CheckBox();
            this.checkBoxShowStatusOverlay = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxShowPrevNextIcons
            // 
            this.checkBoxShowPrevNextIcons.AutoSize = true;
            this.checkBoxShowPrevNextIcons.Location = new System.Drawing.Point(28, 35);
            this.checkBoxShowPrevNextIcons.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxShowPrevNextIcons.Name = "checkBoxShowPrevNextIcons";
            this.checkBoxShowPrevNextIcons.Size = new System.Drawing.Size(409, 29);
            this.checkBoxShowPrevNextIcons.TabIndex = 1;
            this.checkBoxShowPrevNextIcons.Text = "Show Previous / Next Desktop in Icon Tray";
            this.checkBoxShowPrevNextIcons.UseVisualStyleBackColor = true;
            this.checkBoxShowPrevNextIcons.CheckedChanged += new System.EventHandler(this.checkBoxShowPrevNextIcons_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxStatusOverlayShowOnAllMonitors);
            this.groupBox1.Controls.Add(this.panel4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.checkBoxStatusOverlayTranslucent);
            this.groupBox1.Controls.Add(this.checkBoxStatusOverlayAnimate);
            this.groupBox1.Controls.Add(this.checkBoxShowStatusOverlay);
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
            this.groupBox1.Location = new System.Drawing.Point(22, 22);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox1.Size = new System.Drawing.Size(757, 634);
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
            this.panel3.Location = new System.Drawing.Point(73, 567);
            this.panel3.Margin = new System.Windows.Forms.Padding(6);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(660, 46);
            this.panel3.TabIndex = 20;
            // 
            // radioButtonUseHotKeysToJumpToDesktopAlt
            // 
            this.radioButtonUseHotKeysToJumpToDesktopAlt.AutoSize = true;
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Location = new System.Drawing.Point(9, 9);
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Name = "radioButtonUseHotKeysToJumpToDesktopAlt";
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Size = new System.Drawing.Size(83, 29);
            this.radioButtonUseHotKeysToJumpToDesktopAlt.TabIndex = 9;
            this.radioButtonUseHotKeysToJumpToDesktopAlt.TabStop = true;
            this.radioButtonUseHotKeysToJumpToDesktopAlt.Text = "Alt+#";
            this.radioButtonUseHotKeysToJumpToDesktopAlt.UseVisualStyleBackColor = true;
            this.radioButtonUseHotKeysToJumpToDesktopAlt.CheckedChanged += new System.EventHandler(this.radioButtonUseHotKeysToJumpToDesktopAlt_CheckedChanged);
            // 
            // radioButtonUseHotKeysToJumpToDesktopAltShift
            // 
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.AutoSize = true;
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Location = new System.Drawing.Point(343, 9);
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Name = "radioButtonUseHotKeysToJumpToDesktopAltShift";
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Size = new System.Drawing.Size(134, 29);
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.TabIndex = 8;
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.TabStop = true;
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.Text = "Alt+Shift+#";
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.UseVisualStyleBackColor = true;
            this.radioButtonUseHotKeysToJumpToDesktopAltShift.CheckedChanged += new System.EventHandler(this.radioButtonUseHotKeysToJumpToDesktopAltShift_CheckedChanged);
            // 
            // radioButtonUseHotKeysToJumpToDesktopCtrlAlt
            // 
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.AutoSize = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Location = new System.Drawing.Point(205, 9);
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Name = "radioButtonUseHotKeysToJumpToDesktopCtrlAlt";
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Size = new System.Drawing.Size(125, 29);
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.TabIndex = 7;
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.TabStop = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Text = "Ctrl+Alt+#";
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.UseVisualStyleBackColor = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.CheckedChanged += new System.EventHandler(this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt_CheckedChanged);
            // 
            // radioButtonUseHotKeysToJumpToDesktopCtrl
            // 
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.AutoSize = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Location = new System.Drawing.Point(104, 9);
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Name = "radioButtonUseHotKeysToJumpToDesktopCtrl";
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Size = new System.Drawing.Size(90, 29);
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.TabIndex = 6;
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.TabStop = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.Text = "Ctrl+#";
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.UseVisualStyleBackColor = true;
            this.radioButtonUseHotKeysToJumpToDesktopCtrl.CheckedChanged += new System.EventHandler(this.radioButtonUseHotKeysToJumpToDesktopCtrl_CheckedChanged);
            // 
            // checkBoxUseHotKeysToJumpToDesktop
            // 
            this.checkBoxUseHotKeysToJumpToDesktop.AutoSize = true;
            this.checkBoxUseHotKeysToJumpToDesktop.Location = new System.Drawing.Point(28, 539);
            this.checkBoxUseHotKeysToJumpToDesktop.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxUseHotKeysToJumpToDesktop.Name = "checkBoxUseHotKeysToJumpToDesktop";
            this.checkBoxUseHotKeysToJumpToDesktop.Size = new System.Drawing.Size(331, 29);
            this.checkBoxUseHotKeysToJumpToDesktop.TabIndex = 23;
            this.checkBoxUseHotKeysToJumpToDesktop.Text = "Use Hot Keys to Jump to Desktop";
            this.checkBoxUseHotKeysToJumpToDesktop.UseVisualStyleBackColor = true;
            this.checkBoxUseHotKeysToJumpToDesktop.CheckedChanged += new System.EventHandler(this.checkBoxUseHotKeysToJumpToDesktop_CheckedChanged);
            // 
            // checkBoxOverlayShowOnAllMonitors
            // 
            this.checkBoxOverlayShowOnAllMonitors.AutoSize = true;
            this.checkBoxOverlayShowOnAllMonitors.Location = new System.Drawing.Point(79, 246);
            this.checkBoxOverlayShowOnAllMonitors.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxOverlayShowOnAllMonitors.Name = "checkBoxOverlayShowOnAllMonitors";
            this.checkBoxOverlayShowOnAllMonitors.Size = new System.Drawing.Size(219, 29);
            this.checkBoxOverlayShowOnAllMonitors.TabIndex = 22;
            this.checkBoxOverlayShowOnAllMonitors.Text = "Show on all Monitors";
            this.checkBoxOverlayShowOnAllMonitors.UseVisualStyleBackColor = true;
            this.checkBoxOverlayShowOnAllMonitors.CheckedChanged += new System.EventHandler(this.checkBoxOverlayShowOnAllMonitors_CheckedChanged);
            // 
            // checkBoxShowDesktopNameInitial
            // 
            this.checkBoxShowDesktopNameInitial.AutoSize = true;
            this.checkBoxShowDesktopNameInitial.Location = new System.Drawing.Point(28, 498);
            this.checkBoxShowDesktopNameInitial.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxShowDesktopNameInitial.Name = "checkBoxShowDesktopNameInitial";
            this.checkBoxShowDesktopNameInitial.Size = new System.Drawing.Size(378, 29);
            this.checkBoxShowDesktopNameInitial.TabIndex = 21;
            this.checkBoxShowDesktopNameInitial.Text = "Show Desktop Name Initial in Icon Tray";
            this.checkBoxShowDesktopNameInitial.UseVisualStyleBackColor = true;
            this.checkBoxShowDesktopNameInitial.CheckedChanged += new System.EventHandler(this.checkBoxShowDesktopNameInitial_CheckedChanged);
            // 
            // checkBoxClickDesktopNumberTaskView
            // 
            this.checkBoxClickDesktopNumberTaskView.AutoSize = true;
            this.checkBoxClickDesktopNumberTaskView.Location = new System.Drawing.Point(28, 458);
            this.checkBoxClickDesktopNumberTaskView.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxClickDesktopNumberTaskView.Name = "checkBoxClickDesktopNumberTaskView";
            this.checkBoxClickDesktopNumberTaskView.Size = new System.Drawing.Size(521, 29);
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
            this.panel2.Location = new System.Drawing.Point(73, 114);
            this.panel2.Margin = new System.Windows.Forms.Padding(6);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(660, 46);
            this.panel2.TabIndex = 19;
            // 
            // radioButtonOverlayMicroDuration
            // 
            this.radioButtonOverlayMicroDuration.AutoSize = true;
            this.radioButtonOverlayMicroDuration.Location = new System.Drawing.Point(9, 9);
            this.radioButtonOverlayMicroDuration.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonOverlayMicroDuration.Name = "radioButtonOverlayMicroDuration";
            this.radioButtonOverlayMicroDuration.Size = new System.Drawing.Size(96, 29);
            this.radioButtonOverlayMicroDuration.TabIndex = 9;
            this.radioButtonOverlayMicroDuration.TabStop = true;
            this.radioButtonOverlayMicroDuration.Text = "500ms";
            this.radioButtonOverlayMicroDuration.UseVisualStyleBackColor = true;
            this.radioButtonOverlayMicroDuration.CheckedChanged += new System.EventHandler(this.radioButtonOverlayMicroDuration_CheckedChanged);
            // 
            // radioButtonOverlayLongDuration
            // 
            this.radioButtonOverlayLongDuration.AutoSize = true;
            this.radioButtonOverlayLongDuration.Location = new System.Drawing.Point(372, 9);
            this.radioButtonOverlayLongDuration.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonOverlayLongDuration.Name = "radioButtonOverlayLongDuration";
            this.radioButtonOverlayLongDuration.Size = new System.Drawing.Size(107, 29);
            this.radioButtonOverlayLongDuration.TabIndex = 8;
            this.radioButtonOverlayLongDuration.TabStop = true;
            this.radioButtonOverlayLongDuration.Text = "3000ms";
            this.radioButtonOverlayLongDuration.UseVisualStyleBackColor = true;
            this.radioButtonOverlayLongDuration.CheckedChanged += new System.EventHandler(this.radioButtonOverlayLongDuration_CheckedChanged);
            // 
            // radioButtonOverlayMediumDuration
            // 
            this.radioButtonOverlayMediumDuration.AutoSize = true;
            this.radioButtonOverlayMediumDuration.Location = new System.Drawing.Point(251, 9);
            this.radioButtonOverlayMediumDuration.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonOverlayMediumDuration.Name = "radioButtonOverlayMediumDuration";
            this.radioButtonOverlayMediumDuration.Size = new System.Drawing.Size(107, 29);
            this.radioButtonOverlayMediumDuration.TabIndex = 7;
            this.radioButtonOverlayMediumDuration.TabStop = true;
            this.radioButtonOverlayMediumDuration.Text = "2000ms";
            this.radioButtonOverlayMediumDuration.UseVisualStyleBackColor = true;
            this.radioButtonOverlayMediumDuration.CheckedChanged += new System.EventHandler(this.radioButtonOverlayMediumDuration_CheckedChanged);
            // 
            // radioButtonOverlayShortDuration
            // 
            this.radioButtonOverlayShortDuration.AutoSize = true;
            this.radioButtonOverlayShortDuration.Location = new System.Drawing.Point(126, 9);
            this.radioButtonOverlayShortDuration.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonOverlayShortDuration.Name = "radioButtonOverlayShortDuration";
            this.radioButtonOverlayShortDuration.Size = new System.Drawing.Size(107, 29);
            this.radioButtonOverlayShortDuration.TabIndex = 6;
            this.radioButtonOverlayShortDuration.TabStop = true;
            this.radioButtonOverlayShortDuration.Text = "1000ms";
            this.radioButtonOverlayShortDuration.UseVisualStyleBackColor = true;
            this.radioButtonOverlayShortDuration.CheckedChanged += new System.EventHandler(this.radioButtonOverlayShortDuration_CheckedChanged);
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
            this.panel1.Location = new System.Drawing.Point(438, 164);
            this.panel1.Margin = new System.Windows.Forms.Padding(6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(114, 109);
            this.panel1.TabIndex = 18;
            // 
            // radioButtonPositionBottomRight
            // 
            this.radioButtonPositionBottomRight.AutoSize = true;
            this.radioButtonPositionBottomRight.Location = new System.Drawing.Point(79, 76);
            this.radioButtonPositionBottomRight.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonPositionBottomRight.Name = "radioButtonPositionBottomRight";
            this.radioButtonPositionBottomRight.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionBottomRight.TabIndex = 26;
            this.radioButtonPositionBottomRight.TabStop = true;
            this.radioButtonPositionBottomRight.UseVisualStyleBackColor = true;
            this.radioButtonPositionBottomRight.CheckedChanged += new System.EventHandler(this.radioButtonPositionBottomRight_CheckedChanged);
            // 
            // radioButtonPositionBottomCenter
            // 
            this.radioButtonPositionBottomCenter.AutoSize = true;
            this.radioButtonPositionBottomCenter.Location = new System.Drawing.Point(42, 76);
            this.radioButtonPositionBottomCenter.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonPositionBottomCenter.Name = "radioButtonPositionBottomCenter";
            this.radioButtonPositionBottomCenter.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionBottomCenter.TabIndex = 25;
            this.radioButtonPositionBottomCenter.TabStop = true;
            this.radioButtonPositionBottomCenter.UseVisualStyleBackColor = true;
            this.radioButtonPositionBottomCenter.CheckedChanged += new System.EventHandler(this.radioButtonPositionBottomCenter_CheckedChanged);
            // 
            // radioButtonPositionBottomLeft
            // 
            this.radioButtonPositionBottomLeft.AutoSize = true;
            this.radioButtonPositionBottomLeft.Location = new System.Drawing.Point(6, 76);
            this.radioButtonPositionBottomLeft.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonPositionBottomLeft.Name = "radioButtonPositionBottomLeft";
            this.radioButtonPositionBottomLeft.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionBottomLeft.TabIndex = 24;
            this.radioButtonPositionBottomLeft.TabStop = true;
            this.radioButtonPositionBottomLeft.UseVisualStyleBackColor = true;
            this.radioButtonPositionBottomLeft.CheckedChanged += new System.EventHandler(this.radioButtonPositionBottomLeft_CheckedChanged);
            // 
            // radioButtonPositionMiddleRight
            // 
            this.radioButtonPositionMiddleRight.AutoSize = true;
            this.radioButtonPositionMiddleRight.Location = new System.Drawing.Point(79, 41);
            this.radioButtonPositionMiddleRight.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonPositionMiddleRight.Name = "radioButtonPositionMiddleRight";
            this.radioButtonPositionMiddleRight.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionMiddleRight.TabIndex = 23;
            this.radioButtonPositionMiddleRight.TabStop = true;
            this.radioButtonPositionMiddleRight.UseVisualStyleBackColor = true;
            this.radioButtonPositionMiddleRight.CheckedChanged += new System.EventHandler(this.radioButtonPositionMiddleRight_CheckedChanged);
            // 
            // radioButtonPositionMiddleCenter
            // 
            this.radioButtonPositionMiddleCenter.AutoSize = true;
            this.radioButtonPositionMiddleCenter.Location = new System.Drawing.Point(42, 41);
            this.radioButtonPositionMiddleCenter.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonPositionMiddleCenter.Name = "radioButtonPositionMiddleCenter";
            this.radioButtonPositionMiddleCenter.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionMiddleCenter.TabIndex = 22;
            this.radioButtonPositionMiddleCenter.TabStop = true;
            this.radioButtonPositionMiddleCenter.UseVisualStyleBackColor = true;
            this.radioButtonPositionMiddleCenter.CheckedChanged += new System.EventHandler(this.radioButtonPositionMiddleCenter_CheckedChanged);
            // 
            // radioButtonPositionMiddleLeft
            // 
            this.radioButtonPositionMiddleLeft.AutoSize = true;
            this.radioButtonPositionMiddleLeft.Location = new System.Drawing.Point(6, 41);
            this.radioButtonPositionMiddleLeft.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonPositionMiddleLeft.Name = "radioButtonPositionMiddleLeft";
            this.radioButtonPositionMiddleLeft.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionMiddleLeft.TabIndex = 21;
            this.radioButtonPositionMiddleLeft.TabStop = true;
            this.radioButtonPositionMiddleLeft.UseVisualStyleBackColor = true;
            this.radioButtonPositionMiddleLeft.CheckedChanged += new System.EventHandler(this.radioButtonPositionMiddleLeft_CheckedChanged);
            // 
            // radioButtonPositionTopRight
            // 
            this.radioButtonPositionTopRight.AutoSize = true;
            this.radioButtonPositionTopRight.Location = new System.Drawing.Point(79, 6);
            this.radioButtonPositionTopRight.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonPositionTopRight.Name = "radioButtonPositionTopRight";
            this.radioButtonPositionTopRight.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionTopRight.TabIndex = 20;
            this.radioButtonPositionTopRight.TabStop = true;
            this.radioButtonPositionTopRight.UseVisualStyleBackColor = true;
            this.radioButtonPositionTopRight.CheckedChanged += new System.EventHandler(this.radioButtonPositionTopRight_CheckedChanged);
            // 
            // radioButtonPositionTopCenter
            // 
            this.radioButtonPositionTopCenter.AutoSize = true;
            this.radioButtonPositionTopCenter.Location = new System.Drawing.Point(42, 6);
            this.radioButtonPositionTopCenter.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonPositionTopCenter.Name = "radioButtonPositionTopCenter";
            this.radioButtonPositionTopCenter.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionTopCenter.TabIndex = 19;
            this.radioButtonPositionTopCenter.TabStop = true;
            this.radioButtonPositionTopCenter.UseVisualStyleBackColor = true;
            this.radioButtonPositionTopCenter.CheckedChanged += new System.EventHandler(this.radioButtonPositionTopCenter_CheckedChanged);
            // 
            // radioButtonPositionTopLeft
            // 
            this.radioButtonPositionTopLeft.AutoSize = true;
            this.radioButtonPositionTopLeft.Location = new System.Drawing.Point(6, 6);
            this.radioButtonPositionTopLeft.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonPositionTopLeft.Name = "radioButtonPositionTopLeft";
            this.radioButtonPositionTopLeft.Size = new System.Drawing.Size(21, 20);
            this.radioButtonPositionTopLeft.TabIndex = 18;
            this.radioButtonPositionTopLeft.TabStop = true;
            this.radioButtonPositionTopLeft.UseVisualStyleBackColor = true;
            this.radioButtonPositionTopLeft.CheckedChanged += new System.EventHandler(this.radioButtonPositionTopLeft_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(351, 165);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 25);
            this.label1.TabIndex = 9;
            this.label1.Text = "Position:";
            // 
            // checkBoxOverlayTranslucent
            // 
            this.checkBoxOverlayTranslucent.AutoSize = true;
            this.checkBoxOverlayTranslucent.Location = new System.Drawing.Point(79, 205);
            this.checkBoxOverlayTranslucent.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxOverlayTranslucent.Name = "checkBoxOverlayTranslucent";
            this.checkBoxOverlayTranslucent.Size = new System.Drawing.Size(141, 29);
            this.checkBoxOverlayTranslucent.TabIndex = 7;
            this.checkBoxOverlayTranslucent.Text = "Translucent";
            this.checkBoxOverlayTranslucent.UseVisualStyleBackColor = true;
            this.checkBoxOverlayTranslucent.CheckedChanged += new System.EventHandler(this.checkBoxOverlayTranslucent_CheckedChanged);
            // 
            // checkBoxOverlayAnimate
            // 
            this.checkBoxOverlayAnimate.AutoSize = true;
            this.checkBoxOverlayAnimate.Location = new System.Drawing.Point(79, 162);
            this.checkBoxOverlayAnimate.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxOverlayAnimate.Name = "checkBoxOverlayAnimate";
            this.checkBoxOverlayAnimate.Size = new System.Drawing.Size(169, 29);
            this.checkBoxOverlayAnimate.TabIndex = 6;
            this.checkBoxOverlayAnimate.Text = "Animate In/Out";
            this.checkBoxOverlayAnimate.UseVisualStyleBackColor = true;
            this.checkBoxOverlayAnimate.CheckedChanged += new System.EventHandler(this.checkBoxOverlayAnimate_CheckedChanged);
            // 
            // checkBoxShowOverlay
            // 
            this.checkBoxShowOverlay.AutoSize = true;
            this.checkBoxShowOverlay.Location = new System.Drawing.Point(28, 78);
            this.checkBoxShowOverlay.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxShowOverlay.Name = "checkBoxShowOverlay";
            this.checkBoxShowOverlay.Size = new System.Drawing.Size(375, 29);
            this.checkBoxShowOverlay.TabIndex = 2;
            this.checkBoxShowOverlay.Text = "Show Overlay when switching Desktop";
            this.checkBoxShowOverlay.UseVisualStyleBackColor = true;
            this.checkBoxShowOverlay.CheckedChanged += new System.EventHandler(this.checkBoxShowOverlay_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxStartupWithWindows);
            this.groupBox2.Location = new System.Drawing.Point(22, 668);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox2.Size = new System.Drawing.Size(757, 84);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Settings";
            // 
            // checkBoxStartupWithWindows
            // 
            this.checkBoxStartupWithWindows.AutoSize = true;
            this.checkBoxStartupWithWindows.Location = new System.Drawing.Point(28, 35);
            this.checkBoxStartupWithWindows.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxStartupWithWindows.Name = "checkBoxStartupWithWindows";
            this.checkBoxStartupWithWindows.Size = new System.Drawing.Size(226, 29);
            this.checkBoxStartupWithWindows.TabIndex = 1;
            this.checkBoxStartupWithWindows.Text = "Startup with Windows";
            this.checkBoxStartupWithWindows.UseVisualStyleBackColor = true;
            this.checkBoxStartupWithWindows.CheckedChanged += new System.EventHandler(this.checkBoxStartupWithWindows_CheckedChanged);
            // 
            // checkBoxStatusOverlayShowOnAllMonitors
            // 
            this.checkBoxStatusOverlayShowOnAllMonitors.AutoSize = true;
            this.checkBoxStatusOverlayShowOnAllMonitors.Location = new System.Drawing.Point(79, 412);
            this.checkBoxStatusOverlayShowOnAllMonitors.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxStatusOverlayShowOnAllMonitors.Name = "checkBoxStatusOverlayShowOnAllMonitors";
            this.checkBoxStatusOverlayShowOnAllMonitors.Size = new System.Drawing.Size(219, 29);
            this.checkBoxStatusOverlayShowOnAllMonitors.TabIndex = 29;
            this.checkBoxStatusOverlayShowOnAllMonitors.Text = "Show on all Monitors";
            this.checkBoxStatusOverlayShowOnAllMonitors.UseVisualStyleBackColor = true;
            this.checkBoxStatusOverlayShowOnAllMonitors.CheckedChanged += new System.EventHandler(this.checkBoxStatusOverlayShowOnAllMonitors_CheckedChanged);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.radioButtonStatusOverlayPositionBottomRight);
            this.panel4.Controls.Add(this.radioButtonStatusOverlayPositionBottomCenter);
            this.panel4.Controls.Add(this.radioButtonStatusOverlayPositionBottomLeft);
            this.panel4.Controls.Add(this.radioButtonStatusOverlayPositionMiddleRight);
            this.panel4.Controls.Add(this.radioButtonStatusOverlayPositionMiddleCenter);
            this.panel4.Controls.Add(this.radioButtonStatusOverlayPositionMiddleLeft);
            this.panel4.Controls.Add(this.radioButtonStatusOverlayPositionTopRight);
            this.panel4.Controls.Add(this.radioButtonStatusOverlayPositionTopCenter);
            this.panel4.Controls.Add(this.radioButtonStatusOverlayPositionTopLeft);
            this.panel4.Location = new System.Drawing.Point(438, 330);
            this.panel4.Margin = new System.Windows.Forms.Padding(6);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(114, 109);
            this.panel4.TabIndex = 28;
            // 
            // radioButtonStatusOverlayPositionBottomRight
            // 
            this.radioButtonStatusOverlayPositionBottomRight.AutoSize = true;
            this.radioButtonStatusOverlayPositionBottomRight.Location = new System.Drawing.Point(79, 76);
            this.radioButtonStatusOverlayPositionBottomRight.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonStatusOverlayPositionBottomRight.Name = "radioButtonStatusOverlayPositionBottomRight";
            this.radioButtonStatusOverlayPositionBottomRight.Size = new System.Drawing.Size(21, 20);
            this.radioButtonStatusOverlayPositionBottomRight.TabIndex = 26;
            this.radioButtonStatusOverlayPositionBottomRight.TabStop = true;
            this.radioButtonStatusOverlayPositionBottomRight.UseVisualStyleBackColor = true;
            this.radioButtonStatusOverlayPositionBottomRight.CheckedChanged += new System.EventHandler(this.radioButtonStatusOverlayPositionBottomRight_CheckedChanged);
            // 
            // radioButtonStatusOverlayPositionBottomCenter
            // 
            this.radioButtonStatusOverlayPositionBottomCenter.AutoSize = true;
            this.radioButtonStatusOverlayPositionBottomCenter.Location = new System.Drawing.Point(42, 76);
            this.radioButtonStatusOverlayPositionBottomCenter.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonStatusOverlayPositionBottomCenter.Name = "radioButtonStatusOverlayPositionBottomCenter";
            this.radioButtonStatusOverlayPositionBottomCenter.Size = new System.Drawing.Size(21, 20);
            this.radioButtonStatusOverlayPositionBottomCenter.TabIndex = 25;
            this.radioButtonStatusOverlayPositionBottomCenter.TabStop = true;
            this.radioButtonStatusOverlayPositionBottomCenter.UseVisualStyleBackColor = true;
            this.radioButtonStatusOverlayPositionBottomCenter.CheckedChanged += new System.EventHandler(this.radioButtonStatusOverlayPositionBottomCenter_CheckedChanged);
            // 
            // radioButtonStatusOverlayPositionBottomLeft
            // 
            this.radioButtonStatusOverlayPositionBottomLeft.AutoSize = true;
            this.radioButtonStatusOverlayPositionBottomLeft.Location = new System.Drawing.Point(6, 76);
            this.radioButtonStatusOverlayPositionBottomLeft.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonStatusOverlayPositionBottomLeft.Name = "radioButtonStatusOverlayPositionBottomLeft";
            this.radioButtonStatusOverlayPositionBottomLeft.Size = new System.Drawing.Size(21, 20);
            this.radioButtonStatusOverlayPositionBottomLeft.TabIndex = 24;
            this.radioButtonStatusOverlayPositionBottomLeft.TabStop = true;
            this.radioButtonStatusOverlayPositionBottomLeft.UseVisualStyleBackColor = true;
            this.radioButtonStatusOverlayPositionBottomLeft.CheckedChanged += new System.EventHandler(this.radioButtonStatusOverlayPositionBottomLeft_CheckedChanged);
            // 
            // radioButtonStatusOverlayPositionMiddleRight
            // 
            this.radioButtonStatusOverlayPositionMiddleRight.AutoSize = true;
            this.radioButtonStatusOverlayPositionMiddleRight.Location = new System.Drawing.Point(79, 41);
            this.radioButtonStatusOverlayPositionMiddleRight.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonStatusOverlayPositionMiddleRight.Name = "radioButtonStatusOverlayPositionMiddleRight";
            this.radioButtonStatusOverlayPositionMiddleRight.Size = new System.Drawing.Size(21, 20);
            this.radioButtonStatusOverlayPositionMiddleRight.TabIndex = 23;
            this.radioButtonStatusOverlayPositionMiddleRight.TabStop = true;
            this.radioButtonStatusOverlayPositionMiddleRight.UseVisualStyleBackColor = true;
            this.radioButtonStatusOverlayPositionMiddleRight.CheckedChanged += new System.EventHandler(this.radioButtonStatusOverlayPositionMiddleRight_CheckedChanged);
            // 
            // radioButtonStatusOverlayPositionMiddleCenter
            // 
            this.radioButtonStatusOverlayPositionMiddleCenter.AutoSize = true;
            this.radioButtonStatusOverlayPositionMiddleCenter.Location = new System.Drawing.Point(42, 41);
            this.radioButtonStatusOverlayPositionMiddleCenter.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonStatusOverlayPositionMiddleCenter.Name = "radioButtonStatusOverlayPositionMiddleCenter";
            this.radioButtonStatusOverlayPositionMiddleCenter.Size = new System.Drawing.Size(21, 20);
            this.radioButtonStatusOverlayPositionMiddleCenter.TabIndex = 22;
            this.radioButtonStatusOverlayPositionMiddleCenter.TabStop = true;
            this.radioButtonStatusOverlayPositionMiddleCenter.UseVisualStyleBackColor = true;
            this.radioButtonStatusOverlayPositionMiddleCenter.CheckedChanged += new System.EventHandler(this.radioButtonStatusOverlayPositionMiddleCenter_CheckedChanged);
            // 
            // radioButtonStatusOverlayPositionMiddleLeft
            // 
            this.radioButtonStatusOverlayPositionMiddleLeft.AutoSize = true;
            this.radioButtonStatusOverlayPositionMiddleLeft.Location = new System.Drawing.Point(6, 41);
            this.radioButtonStatusOverlayPositionMiddleLeft.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonStatusOverlayPositionMiddleLeft.Name = "radioButtonStatusOverlayPositionMiddleLeft";
            this.radioButtonStatusOverlayPositionMiddleLeft.Size = new System.Drawing.Size(21, 20);
            this.radioButtonStatusOverlayPositionMiddleLeft.TabIndex = 21;
            this.radioButtonStatusOverlayPositionMiddleLeft.TabStop = true;
            this.radioButtonStatusOverlayPositionMiddleLeft.UseVisualStyleBackColor = true;
            this.radioButtonStatusOverlayPositionMiddleLeft.CheckedChanged += new System.EventHandler(this.radioButtonStatusOverlayPositionMiddleLeft_CheckedChanged);
            // 
            // radioButtonStatusOverlayPositionTopRight
            // 
            this.radioButtonStatusOverlayPositionTopRight.AutoSize = true;
            this.radioButtonStatusOverlayPositionTopRight.Location = new System.Drawing.Point(79, 6);
            this.radioButtonStatusOverlayPositionTopRight.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonStatusOverlayPositionTopRight.Name = "radioButtonStatusOverlayPositionTopRight";
            this.radioButtonStatusOverlayPositionTopRight.Size = new System.Drawing.Size(21, 20);
            this.radioButtonStatusOverlayPositionTopRight.TabIndex = 20;
            this.radioButtonStatusOverlayPositionTopRight.TabStop = true;
            this.radioButtonStatusOverlayPositionTopRight.UseVisualStyleBackColor = true;
            this.radioButtonStatusOverlayPositionTopRight.CheckedChanged += new System.EventHandler(this.radioButtonStatusOverlayPositionTopRight_CheckedChanged);
            // 
            // radioButtonStatusOverlayPositionTopCenter
            // 
            this.radioButtonStatusOverlayPositionTopCenter.AutoSize = true;
            this.radioButtonStatusOverlayPositionTopCenter.Location = new System.Drawing.Point(42, 6);
            this.radioButtonStatusOverlayPositionTopCenter.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonStatusOverlayPositionTopCenter.Name = "radioButtonStatusOverlayPositionTopCenter";
            this.radioButtonStatusOverlayPositionTopCenter.Size = new System.Drawing.Size(21, 20);
            this.radioButtonStatusOverlayPositionTopCenter.TabIndex = 19;
            this.radioButtonStatusOverlayPositionTopCenter.TabStop = true;
            this.radioButtonStatusOverlayPositionTopCenter.UseVisualStyleBackColor = true;
            this.radioButtonStatusOverlayPositionTopCenter.CheckedChanged += new System.EventHandler(this.radioButtonStatusOverlayPositionTopCenter_CheckedChanged);
            // 
            // radioButtonStatusOverlayPositionTopLeft
            // 
            this.radioButtonStatusOverlayPositionTopLeft.AutoSize = true;
            this.radioButtonStatusOverlayPositionTopLeft.Location = new System.Drawing.Point(6, 6);
            this.radioButtonStatusOverlayPositionTopLeft.Margin = new System.Windows.Forms.Padding(6);
            this.radioButtonStatusOverlayPositionTopLeft.Name = "radioButtonStatusOverlayPositionTopLeft";
            this.radioButtonStatusOverlayPositionTopLeft.Size = new System.Drawing.Size(21, 20);
            this.radioButtonStatusOverlayPositionTopLeft.TabIndex = 18;
            this.radioButtonStatusOverlayPositionTopLeft.TabStop = true;
            this.radioButtonStatusOverlayPositionTopLeft.UseVisualStyleBackColor = true;
            this.radioButtonStatusOverlayPositionTopLeft.CheckedChanged += new System.EventHandler(this.radioButtonStatusOverlayPositionTopLeft_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(351, 331);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 25);
            this.label2.TabIndex = 27;
            this.label2.Text = "Position:";
            // 
            // checkBoxStatusOverlayTranslucent
            // 
            this.checkBoxStatusOverlayTranslucent.AutoSize = true;
            this.checkBoxStatusOverlayTranslucent.Location = new System.Drawing.Point(79, 371);
            this.checkBoxStatusOverlayTranslucent.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxStatusOverlayTranslucent.Name = "checkBoxStatusOverlayTranslucent";
            this.checkBoxStatusOverlayTranslucent.Size = new System.Drawing.Size(141, 29);
            this.checkBoxStatusOverlayTranslucent.TabIndex = 26;
            this.checkBoxStatusOverlayTranslucent.Text = "Translucent";
            this.checkBoxStatusOverlayTranslucent.UseVisualStyleBackColor = true;
            this.checkBoxStatusOverlayTranslucent.CheckedChanged += new System.EventHandler(this.checkBoxStatusOverlayTranslucent_CheckedChanged);
            // 
            // checkBoxStatusOverlayAnimate
            // 
            this.checkBoxStatusOverlayAnimate.AutoSize = true;
            this.checkBoxStatusOverlayAnimate.Location = new System.Drawing.Point(79, 328);
            this.checkBoxStatusOverlayAnimate.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxStatusOverlayAnimate.Name = "checkBoxStatusOverlayAnimate";
            this.checkBoxStatusOverlayAnimate.Size = new System.Drawing.Size(110, 29);
            this.checkBoxStatusOverlayAnimate.TabIndex = 25;
            this.checkBoxStatusOverlayAnimate.Text = "Animate";
            this.checkBoxStatusOverlayAnimate.UseVisualStyleBackColor = true;
            this.checkBoxStatusOverlayAnimate.CheckedChanged += new System.EventHandler(this.checkBoxStatusOverlayAnimate_CheckedChanged);
            // 
            // checkBoxShowStatusOverlay
            // 
            this.checkBoxShowStatusOverlay.AutoSize = true;
            this.checkBoxShowStatusOverlay.Location = new System.Drawing.Point(28, 287);
            this.checkBoxShowStatusOverlay.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxShowStatusOverlay.Name = "checkBoxShowStatusOverlay";
            this.checkBoxShowStatusOverlay.Size = new System.Drawing.Size(261, 29);
            this.checkBoxShowStatusOverlay.TabIndex = 24;
            this.checkBoxShowStatusOverlay.Text = "Show Permanent Overlay";
            this.checkBoxShowStatusOverlay.UseVisualStyleBackColor = true;
            this.checkBoxShowStatusOverlay.CheckedChanged += new System.EventHandler(this.checkBoxShowStatusOverlay_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(805, 775);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingsForm_FormClosed);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
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
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.CheckBox checkBoxShowPrevNextIcons;
        private System.Windows.Forms.GroupBox groupBox1;
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
		private System.Windows.Forms.CheckBox checkBoxShowDesktopNameInitial;
		private System.Windows.Forms.CheckBox checkBoxOverlayShowOnAllMonitors;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.RadioButton radioButtonUseHotKeysToJumpToDesktopAlt;
		private System.Windows.Forms.RadioButton radioButtonUseHotKeysToJumpToDesktopAltShift;
		private System.Windows.Forms.RadioButton radioButtonUseHotKeysToJumpToDesktopCtrlAlt;
		private System.Windows.Forms.RadioButton radioButtonUseHotKeysToJumpToDesktopCtrl;
		private System.Windows.Forms.CheckBox checkBoxUseHotKeysToJumpToDesktop;
		private System.Windows.Forms.CheckBox checkBoxStatusOverlayShowOnAllMonitors;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.RadioButton radioButtonStatusOverlayPositionBottomRight;
		private System.Windows.Forms.RadioButton radioButtonStatusOverlayPositionBottomCenter;
		private System.Windows.Forms.RadioButton radioButtonStatusOverlayPositionBottomLeft;
		private System.Windows.Forms.RadioButton radioButtonStatusOverlayPositionMiddleRight;
		private System.Windows.Forms.RadioButton radioButtonStatusOverlayPositionMiddleCenter;
		private System.Windows.Forms.RadioButton radioButtonStatusOverlayPositionMiddleLeft;
		private System.Windows.Forms.RadioButton radioButtonStatusOverlayPositionTopRight;
		private System.Windows.Forms.RadioButton radioButtonStatusOverlayPositionTopCenter;
		private System.Windows.Forms.RadioButton radioButtonStatusOverlayPositionTopLeft;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox checkBoxStatusOverlayTranslucent;
		private System.Windows.Forms.CheckBox checkBoxStatusOverlayAnimate;
		private System.Windows.Forms.CheckBox checkBoxShowStatusOverlay;
		private System.Windows.Forms.RadioButton radioButtonPositionBottomRight;
	}
}

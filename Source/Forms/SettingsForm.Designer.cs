
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
            this.toolStripMenuItemSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIconPrev = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyIconNext = new System.Windows.Forms.NotifyIcon(this.components);
            this.checkBoxShowPrevNextIcons = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxOverlayTranslucent = new System.Windows.Forms.CheckBox();
            this.checkBoxOverlayAnimate = new System.Windows.Forms.CheckBox();
            this.radioButtonOverlayLongDuration = new System.Windows.Forms.RadioButton();
            this.radioButtonOverlayMediumDuration = new System.Windows.Forms.RadioButton();
            this.radioButtonOverlayShortDuration = new System.Windows.Forms.RadioButton();
            this.checkBoxShowOverlay = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxStartupWithWindows = new System.Windows.Forms.CheckBox();
            this.radioButtonPositionTopLeft = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonPositionTopCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionTopRight = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionMiddleRight = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionMiddleCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionMiddleLeft = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionBottomRight = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionBottomCenter = new System.Windows.Forms.RadioButton();
            this.radioButtonPositionBottomLeft = new System.Windows.Forms.RadioButton();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIconNumber
            // 
            this.notifyIconNumber.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIconNumber.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconNumber.Icon")));
            this.notifyIconNumber.Text = "Windows Virtual Desktop Helper";
            this.notifyIconNumber.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAbout,
            this.toolStripMenuItemSettings,
            this.toolStripMenuItemExit});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(117, 70);
            this.contextMenuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuStrip1_ItemClicked);
            // 
            // toolStripMenuItemAbout
            // 
            this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            this.toolStripMenuItemAbout.Size = new System.Drawing.Size(116, 22);
            this.toolStripMenuItemAbout.Tag = "about";
            this.toolStripMenuItemAbout.Text = "About";
            // 
            // toolStripMenuItemSettings
            // 
            this.toolStripMenuItemSettings.Name = "toolStripMenuItemSettings";
            this.toolStripMenuItemSettings.Size = new System.Drawing.Size(116, 22);
            this.toolStripMenuItemSettings.Tag = "settings";
            this.toolStripMenuItemSettings.Text = "Settings";
            // 
            // toolStripMenuItemExit
            // 
            this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            this.toolStripMenuItemExit.Size = new System.Drawing.Size(116, 22);
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
            this.checkBoxShowPrevNextIcons.Location = new System.Drawing.Point(15, 19);
            this.checkBoxShowPrevNextIcons.Name = "checkBoxShowPrevNextIcons";
            this.checkBoxShowPrevNextIcons.Size = new System.Drawing.Size(232, 17);
            this.checkBoxShowPrevNextIcons.TabIndex = 1;
            this.checkBoxShowPrevNextIcons.Text = "Show Previous / Next Desktop in Icon Tray";
            this.checkBoxShowPrevNextIcons.UseVisualStyleBackColor = true;
            this.checkBoxShowPrevNextIcons.CheckedChanged += new System.EventHandler(this.checkBoxShowPrevNextIcons_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonPositionBottomRight);
            this.groupBox1.Controls.Add(this.radioButtonPositionBottomCenter);
            this.groupBox1.Controls.Add(this.radioButtonPositionBottomLeft);
            this.groupBox1.Controls.Add(this.radioButtonPositionMiddleRight);
            this.groupBox1.Controls.Add(this.radioButtonPositionMiddleCenter);
            this.groupBox1.Controls.Add(this.radioButtonPositionMiddleLeft);
            this.groupBox1.Controls.Add(this.radioButtonPositionTopRight);
            this.groupBox1.Controls.Add(this.radioButtonPositionTopCenter);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.radioButtonPositionTopLeft);
            this.groupBox1.Controls.Add(this.checkBoxOverlayTranslucent);
            this.groupBox1.Controls.Add(this.checkBoxOverlayAnimate);
            this.groupBox1.Controls.Add(this.radioButtonOverlayLongDuration);
            this.groupBox1.Controls.Add(this.radioButtonOverlayMediumDuration);
            this.groupBox1.Controls.Add(this.radioButtonOverlayShortDuration);
            this.groupBox1.Controls.Add(this.checkBoxShowOverlay);
            this.groupBox1.Controls.Add(this.checkBoxShowPrevNextIcons);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(392, 205);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Features";
            // 
            // checkBoxOverlayTranslucent
            // 
            this.checkBoxOverlayTranslucent.AutoSize = true;
            this.checkBoxOverlayTranslucent.Location = new System.Drawing.Point(43, 111);
            this.checkBoxOverlayTranslucent.Name = "checkBoxOverlayTranslucent";
            this.checkBoxOverlayTranslucent.Size = new System.Drawing.Size(82, 17);
            this.checkBoxOverlayTranslucent.TabIndex = 7;
            this.checkBoxOverlayTranslucent.Text = "Translucent";
            this.checkBoxOverlayTranslucent.UseVisualStyleBackColor = true;
            // 
            // checkBoxOverlayAnimate
            // 
            this.checkBoxOverlayAnimate.AutoSize = true;
            this.checkBoxOverlayAnimate.Location = new System.Drawing.Point(43, 88);
            this.checkBoxOverlayAnimate.Name = "checkBoxOverlayAnimate";
            this.checkBoxOverlayAnimate.Size = new System.Drawing.Size(98, 17);
            this.checkBoxOverlayAnimate.TabIndex = 6;
            this.checkBoxOverlayAnimate.Text = "Animate In/Out";
            this.checkBoxOverlayAnimate.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverlayLongDuration
            // 
            this.radioButtonOverlayLongDuration.AutoSize = true;
            this.radioButtonOverlayLongDuration.Location = new System.Drawing.Point(253, 65);
            this.radioButtonOverlayLongDuration.Name = "radioButtonOverlayLongDuration";
            this.radioButtonOverlayLongDuration.Size = new System.Drawing.Size(92, 17);
            this.radioButtonOverlayLongDuration.TabIndex = 5;
            this.radioButtonOverlayLongDuration.TabStop = true;
            this.radioButtonOverlayLongDuration.Text = "Long Duration";
            this.radioButtonOverlayLongDuration.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverlayMediumDuration
            // 
            this.radioButtonOverlayMediumDuration.AutoSize = true;
            this.radioButtonOverlayMediumDuration.Location = new System.Drawing.Point(142, 65);
            this.radioButtonOverlayMediumDuration.Name = "radioButtonOverlayMediumDuration";
            this.radioButtonOverlayMediumDuration.Size = new System.Drawing.Size(105, 17);
            this.radioButtonOverlayMediumDuration.TabIndex = 4;
            this.radioButtonOverlayMediumDuration.TabStop = true;
            this.radioButtonOverlayMediumDuration.Text = "Medium Duration";
            this.radioButtonOverlayMediumDuration.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverlayShortDuration
            // 
            this.radioButtonOverlayShortDuration.AutoSize = true;
            this.radioButtonOverlayShortDuration.Location = new System.Drawing.Point(43, 65);
            this.radioButtonOverlayShortDuration.Name = "radioButtonOverlayShortDuration";
            this.radioButtonOverlayShortDuration.Size = new System.Drawing.Size(93, 17);
            this.radioButtonOverlayShortDuration.TabIndex = 3;
            this.radioButtonOverlayShortDuration.TabStop = true;
            this.radioButtonOverlayShortDuration.Text = "Short Duration";
            this.radioButtonOverlayShortDuration.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowOverlay
            // 
            this.checkBoxShowOverlay.AutoSize = true;
            this.checkBoxShowOverlay.Location = new System.Drawing.Point(15, 42);
            this.checkBoxShowOverlay.Name = "checkBoxShowOverlay";
            this.checkBoxShowOverlay.Size = new System.Drawing.Size(211, 17);
            this.checkBoxShowOverlay.TabIndex = 2;
            this.checkBoxShowOverlay.Text = "Show Overlay when switching Desktop";
            this.checkBoxShowOverlay.UseVisualStyleBackColor = true;
            this.checkBoxShowOverlay.CheckedChanged += new System.EventHandler(this.checkBoxShowOverlay_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxStartupWithWindows);
            this.groupBox2.Location = new System.Drawing.Point(12, 223);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(392, 82);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Settings";
            // 
            // checkBoxStartupWithWindows
            // 
            this.checkBoxStartupWithWindows.AutoSize = true;
            this.checkBoxStartupWithWindows.Location = new System.Drawing.Point(15, 19);
            this.checkBoxStartupWithWindows.Name = "checkBoxStartupWithWindows";
            this.checkBoxStartupWithWindows.Size = new System.Drawing.Size(129, 17);
            this.checkBoxStartupWithWindows.TabIndex = 1;
            this.checkBoxStartupWithWindows.Text = "Startup with Windows";
            this.checkBoxStartupWithWindows.UseVisualStyleBackColor = true;
            this.checkBoxStartupWithWindows.CheckedChanged += new System.EventHandler(this.checkBoxStartupWithWindows_CheckedChanged);
            // 
            // radioButtonPositionTopLeft
            // 
            this.radioButtonPositionTopLeft.AutoSize = true;
            this.radioButtonPositionTopLeft.Location = new System.Drawing.Point(90, 135);
            this.radioButtonPositionTopLeft.Name = "radioButtonPositionTopLeft";
            this.radioButtonPositionTopLeft.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionTopLeft.TabIndex = 8;
            this.radioButtonPositionTopLeft.TabStop = true;
            this.radioButtonPositionTopLeft.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 135);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Position:";
            // 
            // radioButtonPositionTopCenter
            // 
            this.radioButtonPositionTopCenter.AutoSize = true;
            this.radioButtonPositionTopCenter.Location = new System.Drawing.Point(110, 135);
            this.radioButtonPositionTopCenter.Name = "radioButtonPositionTopCenter";
            this.radioButtonPositionTopCenter.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionTopCenter.TabIndex = 10;
            this.radioButtonPositionTopCenter.TabStop = true;
            this.radioButtonPositionTopCenter.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionTopRight
            // 
            this.radioButtonPositionTopRight.AutoSize = true;
            this.radioButtonPositionTopRight.Location = new System.Drawing.Point(130, 135);
            this.radioButtonPositionTopRight.Name = "radioButtonPositionTopRight";
            this.radioButtonPositionTopRight.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionTopRight.TabIndex = 11;
            this.radioButtonPositionTopRight.TabStop = true;
            this.radioButtonPositionTopRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionMiddleRight
            // 
            this.radioButtonPositionMiddleRight.AutoSize = true;
            this.radioButtonPositionMiddleRight.Location = new System.Drawing.Point(130, 154);
            this.radioButtonPositionMiddleRight.Name = "radioButtonPositionMiddleRight";
            this.radioButtonPositionMiddleRight.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionMiddleRight.TabIndex = 14;
            this.radioButtonPositionMiddleRight.TabStop = true;
            this.radioButtonPositionMiddleRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionMiddleCenter
            // 
            this.radioButtonPositionMiddleCenter.AutoSize = true;
            this.radioButtonPositionMiddleCenter.Location = new System.Drawing.Point(110, 154);
            this.radioButtonPositionMiddleCenter.Name = "radioButtonPositionMiddleCenter";
            this.radioButtonPositionMiddleCenter.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionMiddleCenter.TabIndex = 13;
            this.radioButtonPositionMiddleCenter.TabStop = true;
            this.radioButtonPositionMiddleCenter.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionMiddleLeft
            // 
            this.radioButtonPositionMiddleLeft.AutoSize = true;
            this.radioButtonPositionMiddleLeft.Location = new System.Drawing.Point(90, 154);
            this.radioButtonPositionMiddleLeft.Name = "radioButtonPositionMiddleLeft";
            this.radioButtonPositionMiddleLeft.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionMiddleLeft.TabIndex = 12;
            this.radioButtonPositionMiddleLeft.TabStop = true;
            this.radioButtonPositionMiddleLeft.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionBottomRight
            // 
            this.radioButtonPositionBottomRight.AutoSize = true;
            this.radioButtonPositionBottomRight.Location = new System.Drawing.Point(130, 173);
            this.radioButtonPositionBottomRight.Name = "radioButtonPositionBottomRight";
            this.radioButtonPositionBottomRight.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionBottomRight.TabIndex = 17;
            this.radioButtonPositionBottomRight.TabStop = true;
            this.radioButtonPositionBottomRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionBottomCenter
            // 
            this.radioButtonPositionBottomCenter.AutoSize = true;
            this.radioButtonPositionBottomCenter.Location = new System.Drawing.Point(110, 173);
            this.radioButtonPositionBottomCenter.Name = "radioButtonPositionBottomCenter";
            this.radioButtonPositionBottomCenter.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionBottomCenter.TabIndex = 16;
            this.radioButtonPositionBottomCenter.TabStop = true;
            this.radioButtonPositionBottomCenter.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionBottomLeft
            // 
            this.radioButtonPositionBottomLeft.AutoSize = true;
            this.radioButtonPositionBottomLeft.Location = new System.Drawing.Point(90, 173);
            this.radioButtonPositionBottomLeft.Name = "radioButtonPositionBottomLeft";
            this.radioButtonPositionBottomLeft.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionBottomLeft.TabIndex = 15;
            this.radioButtonPositionBottomLeft.TabStop = true;
            this.radioButtonPositionBottomLeft.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 317);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
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
        private System.Windows.Forms.RadioButton radioButtonOverlayLongDuration;
        private System.Windows.Forms.RadioButton radioButtonOverlayMediumDuration;
        private System.Windows.Forms.RadioButton radioButtonOverlayShortDuration;
        private System.Windows.Forms.CheckBox checkBoxShowOverlay;
        private System.Windows.Forms.CheckBox checkBoxOverlayTranslucent;
        private System.Windows.Forms.CheckBox checkBoxOverlayAnimate;
        private System.Windows.Forms.RadioButton radioButtonPositionBottomRight;
        private System.Windows.Forms.RadioButton radioButtonPositionBottomCenter;
        private System.Windows.Forms.RadioButton radioButtonPositionBottomLeft;
        private System.Windows.Forms.RadioButton radioButtonPositionMiddleRight;
        private System.Windows.Forms.RadioButton radioButtonPositionMiddleCenter;
        private System.Windows.Forms.RadioButton radioButtonPositionMiddleLeft;
        private System.Windows.Forms.RadioButton radioButtonPositionTopRight;
        private System.Windows.Forms.RadioButton radioButtonPositionTopCenter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButtonPositionTopLeft;
    }
}
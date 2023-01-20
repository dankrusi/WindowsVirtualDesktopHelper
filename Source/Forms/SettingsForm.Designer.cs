
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
            this.toolStripMenuItemDonate = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIconNumber
            // 
            this.notifyIconNumber.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIconNumber.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconNumber.Icon")));
            this.notifyIconNumber.Text = "Windows Virtual Desktop Helper";
            this.notifyIconNumber.Visible = true;
            this.notifyIconNumber.Click += new System.EventHandler(this.notifyIconNumber_Click);
            this.notifyIconNumber.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIconNumber_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAbout,
            this.toolStripMenuItemDonate,
            this.toolStripMenuItemSettings,
            this.toolStripMenuItemExit});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(117, 92);
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
            this.groupBox1.Controls.Add(this.checkBoxClickDesktopNumberTaskView);
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkBoxOverlayTranslucent);
            this.groupBox1.Controls.Add(this.checkBoxOverlayAnimate);
            this.groupBox1.Controls.Add(this.checkBoxShowOverlay);
            this.groupBox1.Controls.Add(this.checkBoxShowPrevNextIcons);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(413, 239);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Features";
            // 
            // checkBoxClickDesktopNumberTaskView
            // 
            this.checkBoxClickDesktopNumberTaskView.AutoSize = true;
            this.checkBoxClickDesktopNumberTaskView.Location = new System.Drawing.Point(15, 198);
            this.checkBoxClickDesktopNumberTaskView.Name = "checkBoxClickDesktopNumberTaskView";
            this.checkBoxClickDesktopNumberTaskView.Size = new System.Drawing.Size(290, 17);
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
            this.panel2.Location = new System.Drawing.Point(40, 62);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(360, 25);
            this.panel2.TabIndex = 19;
            // 
            // radioButtonOverlayMicroDuration
            // 
            this.radioButtonOverlayMicroDuration.AutoSize = true;
            this.radioButtonOverlayMicroDuration.Location = new System.Drawing.Point(5, 5);
            this.radioButtonOverlayMicroDuration.Name = "radioButtonOverlayMicroDuration";
            this.radioButtonOverlayMicroDuration.Size = new System.Drawing.Size(56, 17);
            this.radioButtonOverlayMicroDuration.TabIndex = 9;
            this.radioButtonOverlayMicroDuration.TabStop = true;
            this.radioButtonOverlayMicroDuration.Text = "500ms";
            this.radioButtonOverlayMicroDuration.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverlayLongDuration
            // 
            this.radioButtonOverlayLongDuration.AutoSize = true;
            this.radioButtonOverlayLongDuration.Location = new System.Drawing.Point(203, 5);
            this.radioButtonOverlayLongDuration.Name = "radioButtonOverlayLongDuration";
            this.radioButtonOverlayLongDuration.Size = new System.Drawing.Size(62, 17);
            this.radioButtonOverlayLongDuration.TabIndex = 8;
            this.radioButtonOverlayLongDuration.TabStop = true;
            this.radioButtonOverlayLongDuration.Text = "3000ms";
            this.radioButtonOverlayLongDuration.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverlayMediumDuration
            // 
            this.radioButtonOverlayMediumDuration.AutoSize = true;
            this.radioButtonOverlayMediumDuration.Location = new System.Drawing.Point(137, 5);
            this.radioButtonOverlayMediumDuration.Name = "radioButtonOverlayMediumDuration";
            this.radioButtonOverlayMediumDuration.Size = new System.Drawing.Size(62, 17);
            this.radioButtonOverlayMediumDuration.TabIndex = 7;
            this.radioButtonOverlayMediumDuration.TabStop = true;
            this.radioButtonOverlayMediumDuration.Text = "2000ms";
            this.radioButtonOverlayMediumDuration.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverlayShortDuration
            // 
            this.radioButtonOverlayShortDuration.AutoSize = true;
            this.radioButtonOverlayShortDuration.Location = new System.Drawing.Point(69, 5);
            this.radioButtonOverlayShortDuration.Name = "radioButtonOverlayShortDuration";
            this.radioButtonOverlayShortDuration.Size = new System.Drawing.Size(62, 17);
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
            this.panel1.Location = new System.Drawing.Point(87, 133);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(62, 59);
            this.panel1.TabIndex = 18;
            // 
            // radioButtonPositionBottomRight
            // 
            this.radioButtonPositionBottomRight.AutoSize = true;
            this.radioButtonPositionBottomRight.Location = new System.Drawing.Point(43, 41);
            this.radioButtonPositionBottomRight.Name = "radioButtonPositionBottomRight";
            this.radioButtonPositionBottomRight.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionBottomRight.TabIndex = 26;
            this.radioButtonPositionBottomRight.TabStop = true;
            this.radioButtonPositionBottomRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionBottomCenter
            // 
            this.radioButtonPositionBottomCenter.AutoSize = true;
            this.radioButtonPositionBottomCenter.Location = new System.Drawing.Point(23, 41);
            this.radioButtonPositionBottomCenter.Name = "radioButtonPositionBottomCenter";
            this.radioButtonPositionBottomCenter.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionBottomCenter.TabIndex = 25;
            this.radioButtonPositionBottomCenter.TabStop = true;
            this.radioButtonPositionBottomCenter.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionBottomLeft
            // 
            this.radioButtonPositionBottomLeft.AutoSize = true;
            this.radioButtonPositionBottomLeft.Location = new System.Drawing.Point(3, 41);
            this.radioButtonPositionBottomLeft.Name = "radioButtonPositionBottomLeft";
            this.radioButtonPositionBottomLeft.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionBottomLeft.TabIndex = 24;
            this.radioButtonPositionBottomLeft.TabStop = true;
            this.radioButtonPositionBottomLeft.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionMiddleRight
            // 
            this.radioButtonPositionMiddleRight.AutoSize = true;
            this.radioButtonPositionMiddleRight.Location = new System.Drawing.Point(43, 22);
            this.radioButtonPositionMiddleRight.Name = "radioButtonPositionMiddleRight";
            this.radioButtonPositionMiddleRight.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionMiddleRight.TabIndex = 23;
            this.radioButtonPositionMiddleRight.TabStop = true;
            this.radioButtonPositionMiddleRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionMiddleCenter
            // 
            this.radioButtonPositionMiddleCenter.AutoSize = true;
            this.radioButtonPositionMiddleCenter.Location = new System.Drawing.Point(23, 22);
            this.radioButtonPositionMiddleCenter.Name = "radioButtonPositionMiddleCenter";
            this.radioButtonPositionMiddleCenter.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionMiddleCenter.TabIndex = 22;
            this.radioButtonPositionMiddleCenter.TabStop = true;
            this.radioButtonPositionMiddleCenter.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionMiddleLeft
            // 
            this.radioButtonPositionMiddleLeft.AutoSize = true;
            this.radioButtonPositionMiddleLeft.Location = new System.Drawing.Point(3, 22);
            this.radioButtonPositionMiddleLeft.Name = "radioButtonPositionMiddleLeft";
            this.radioButtonPositionMiddleLeft.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionMiddleLeft.TabIndex = 21;
            this.radioButtonPositionMiddleLeft.TabStop = true;
            this.radioButtonPositionMiddleLeft.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionTopRight
            // 
            this.radioButtonPositionTopRight.AutoSize = true;
            this.radioButtonPositionTopRight.Location = new System.Drawing.Point(43, 3);
            this.radioButtonPositionTopRight.Name = "radioButtonPositionTopRight";
            this.radioButtonPositionTopRight.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionTopRight.TabIndex = 20;
            this.radioButtonPositionTopRight.TabStop = true;
            this.radioButtonPositionTopRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionTopCenter
            // 
            this.radioButtonPositionTopCenter.AutoSize = true;
            this.radioButtonPositionTopCenter.Location = new System.Drawing.Point(23, 3);
            this.radioButtonPositionTopCenter.Name = "radioButtonPositionTopCenter";
            this.radioButtonPositionTopCenter.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionTopCenter.TabIndex = 19;
            this.radioButtonPositionTopCenter.TabStop = true;
            this.radioButtonPositionTopCenter.UseVisualStyleBackColor = true;
            // 
            // radioButtonPositionTopLeft
            // 
            this.radioButtonPositionTopLeft.AutoSize = true;
            this.radioButtonPositionTopLeft.Location = new System.Drawing.Point(3, 3);
            this.radioButtonPositionTopLeft.Name = "radioButtonPositionTopLeft";
            this.radioButtonPositionTopLeft.Size = new System.Drawing.Size(14, 13);
            this.radioButtonPositionTopLeft.TabIndex = 18;
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
            this.groupBox2.Location = new System.Drawing.Point(12, 257);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(413, 48);
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
            // toolStripMenuItemDonate
            // 
            this.toolStripMenuItemDonate.Name = "toolStripMenuItemDonate";
            this.toolStripMenuItemDonate.Size = new System.Drawing.Size(116, 22);
            this.toolStripMenuItemDonate.Tag = "donate";
            this.toolStripMenuItemDonate.Text = "Donate";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 317);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
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
    }
}
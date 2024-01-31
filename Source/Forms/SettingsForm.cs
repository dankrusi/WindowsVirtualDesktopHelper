using System;
using System.Globalization;
using System.Windows.Forms;
using WindowsVirtualDesktopHelper.WindowsHotKeyAPI;

namespace WindowsVirtualDesktopHelper {
	public partial class SettingsForm : Form {

		private bool IsLoading;

		public SettingsForm() {
			IsLoading = true;

			// Init UI
			InitializeComponent();
			LoadSettingsIntoUI();

			// Theme
			UpdateIconsForTheme(App.Instance.CurrentSystemThemeName);

			// Apply some settings
			if (checkBoxShowPrevNextIcons.Checked) {
				notifyIconPrev.Visible = true;
				notifyIconNext.Visible = true;
			} else {
				notifyIconPrev.Visible = false;
				notifyIconNext.Visible = false;
			}
			if (checkBoxShowDesktopNameInitial.Checked) {
				notifyIconName.Visible = true;
			} else {
				notifyIconName.Visible = false;
			}

			//TODO: how to sync the startup setting - best would be to see if the reg key is actually there

			IsLoading = false;
		}

		public void UpdateIconsForTheme(string theme) {
			// Prev/next
			// Get prev char
			// \xE100 = skip back (player style)
			// \xE112 = previous (arrow style)
			// \xe26c = previous (chevron style)
			// \u02C2 = previous (chevron style)
			// \u2039 = previous (chevron style)
			var prevChar = "\u2039";
			// Get next char
			// \xE101 = skip forward (player style)
			// \xE111 = next (arrow style)
			// \xe26b = next (chevron style)
			// \u02C3 = next (chevron style)
			// \u203A = next (chevron style)
			var nextChar = "\u203A";
			// Set prev/next icons
			notifyIconPrev.Icon = Util.Icons.GenerateNotificationIcon(prevChar, theme, this.DeviceDpi, true);
			notifyIconNext.Icon = Util.Icons.GenerateNotificationIcon(nextChar, theme, this.DeviceDpi, true);
			// Set current display icons
			UpdateIconForVDDisplayNumber(theme, App.Instance.CurrentVDDisplayNumber, App.Instance.CurrentVDDisplayName);
			UpdateIconForVDDisplayName(theme, App.Instance.CurrentVDDisplayName);
		}
		public ModifierKeys HotKeysToJumpToDesktop() {
			if (this.radioButtonUseHotKeysToJumpToDesktopAlt.Checked) return WindowsHotKeyAPI.ModifierKeys.Alt;
			if (this.radioButtonUseHotKeysToJumpToDesktopAltShift.Checked) return WindowsHotKeyAPI.ModifierKeys.Alt | WindowsHotKeyAPI.ModifierKeys.Shift;
			if (this.radioButtonUseHotKeysToJumpToDesktopCtrl.Checked) return WindowsHotKeyAPI.ModifierKeys.Control;
			if (this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Checked) return WindowsHotKeyAPI.ModifierKeys.Control | WindowsHotKeyAPI.ModifierKeys.Alt;
			throw new Exception("invalid modifier");
		}
		public bool UseHotKeysToJumpToDesktop() {
			return this.checkBoxUseHotKeysToJumpToDesktop.Checked;
		}
		public bool ShowOverlay() {
			return this.checkBoxShowOverlay.Checked;
		}
		public bool OverlayAnimate() {
			return this.checkBoxOverlayAnimate.Checked;
		}
		public bool OverlayTranslucent() {
			return this.checkBoxOverlayTranslucent.Checked;
		}
		public bool OverlayShowOnAllMonitors() {
			return this.checkBoxOverlayShowOnAllMonitors.Checked;
		}
		public int OverlayDurationMS() {
			if (this.radioButtonOverlayLongDuration.Checked == true) return 3000;
			else if (this.radioButtonOverlayMediumDuration.Checked == true) return 2000;
			else if (this.radioButtonOverlayShortDuration.Checked == true) return 1000;
			else if (this.radioButtonOverlayMicroDuration.Checked == true) return 500;
			else return 2000;
		}
		public string OverlayPosition() {
			if (this.radioButtonPositionTopLeft.Checked) return "topleft";
			else if (this.radioButtonPositionTopCenter.Checked) return "topcenter";
			else if (this.radioButtonPositionTopRight.Checked) return "topright";
			else if (this.radioButtonPositionMiddleLeft.Checked) return "middleleft";
			else if (this.radioButtonPositionMiddleCenter.Checked) return "middlecenter";
			else if (this.radioButtonPositionMiddleRight.Checked) return "middleright";
			else if (this.radioButtonPositionBottomLeft.Checked) return "bottomleft";
			else if (this.radioButtonPositionBottomCenter.Checked) return "bottomcenter";
			else if (this.radioButtonPositionBottomRight.Checked) return "bottomright";
			else return "middlecenter";
		}

		private void LoadSettingsIntoUI() {
			this.checkBoxShowPrevNextIcons.Checked = Properties.Settings.Default.ShowPrevNextIcons;
			this.checkBoxShowDesktopNameInitial.Checked = Properties.Settings.Default.ShowDesktopNameInitial;
			this.checkBoxStartupWithWindows.Checked = Properties.Settings.Default.StartupWithWindows;
			this.checkBoxShowOverlay.Checked = Properties.Settings.Default.ShowOverlay;
			this.checkBoxOverlayAnimate.Checked = Properties.Settings.Default.OverlayAnimate;
			this.checkBoxOverlayTranslucent.Checked = Properties.Settings.Default.OverlayTranslucent;
			this.checkBoxOverlayShowOnAllMonitors.Checked = Properties.Settings.Default.OverlayShowOnAllMonitors;
			this.checkBoxClickDesktopNumberTaskView.Checked = Properties.Settings.Default.ClickDesktopNumberOpensTaskView;
			this.checkBoxUseHotKeysToJumpToDesktop.Checked = Properties.Settings.Default.UseHotKeysToJumpToDesktop;
			this.radioButtonOverlayLongDuration.Checked = Properties.Settings.Default.OverlayDuration == "long";
			this.radioButtonOverlayMediumDuration.Checked = Properties.Settings.Default.OverlayDuration == "medium";
			this.radioButtonOverlayShortDuration.Checked = Properties.Settings.Default.OverlayDuration == "short";
			this.radioButtonOverlayMicroDuration.Checked = Properties.Settings.Default.OverlayDuration == "micro";
			this.radioButtonPositionTopLeft.Checked = Properties.Settings.Default.OverlayPosition == "topleft";
			this.radioButtonPositionTopCenter.Checked = Properties.Settings.Default.OverlayPosition == "topcenter";
			this.radioButtonPositionTopRight.Checked = Properties.Settings.Default.OverlayPosition == "topright";
			this.radioButtonPositionMiddleLeft.Checked = Properties.Settings.Default.OverlayPosition == "middleleft";
			this.radioButtonPositionMiddleCenter.Checked = Properties.Settings.Default.OverlayPosition == "middlecenter";
			this.radioButtonPositionMiddleRight.Checked = Properties.Settings.Default.OverlayPosition == "middleright";
			this.radioButtonPositionBottomLeft.Checked = Properties.Settings.Default.OverlayPosition == "bottomleft";
			this.radioButtonPositionBottomCenter.Checked = Properties.Settings.Default.OverlayPosition == "bottomcenter";
			this.radioButtonPositionBottomRight.Checked = Properties.Settings.Default.OverlayPosition == "bottomright";
			this.radioButtonUseHotKeysToJumpToDesktopAlt.Checked = Properties.Settings.Default.HotKeysToJumpToDesktop == "Alt";
			this.radioButtonUseHotKeysToJumpToDesktopAltShift.Checked = Properties.Settings.Default.HotKeysToJumpToDesktop == "AltShift";
			this.radioButtonUseHotKeysToJumpToDesktopCtrl.Checked = Properties.Settings.Default.HotKeysToJumpToDesktop == "Ctrl";
			this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Checked = Properties.Settings.Default.HotKeysToJumpToDesktop == "CtrlAlt";
			checkBoxShowOverlay_CheckedChanged(this, null);
			checkBoxUseHotKeysToJumpToDesktop_CheckedChanged(this, null);
		}
		private void SaveSettingsFromUI() {
			Properties.Settings.Default.ShowPrevNextIcons = this.checkBoxShowPrevNextIcons.Checked;
			Properties.Settings.Default.ShowDesktopNameInitial = this.checkBoxShowDesktopNameInitial.Checked;
			Properties.Settings.Default.StartupWithWindows = this.checkBoxStartupWithWindows.Checked;
			Properties.Settings.Default.ShowOverlay = this.checkBoxShowOverlay.Checked;
			Properties.Settings.Default.OverlayAnimate = this.checkBoxOverlayAnimate.Checked;
			Properties.Settings.Default.OverlayTranslucent = this.checkBoxOverlayTranslucent.Checked;
			Properties.Settings.Default.OverlayShowOnAllMonitors = this.checkBoxOverlayShowOnAllMonitors.Checked;
			Properties.Settings.Default.ClickDesktopNumberOpensTaskView = this.checkBoxClickDesktopNumberTaskView.Checked;
			Properties.Settings.Default.UseHotKeysToJumpToDesktop = this.checkBoxUseHotKeysToJumpToDesktop.Checked;
			if (this.radioButtonOverlayLongDuration.Checked) Properties.Settings.Default.OverlayDuration = "long";
			if (this.radioButtonOverlayMediumDuration.Checked) Properties.Settings.Default.OverlayDuration = "medium";
			if (this.radioButtonOverlayShortDuration.Checked) Properties.Settings.Default.OverlayDuration = "short";
			if (this.radioButtonOverlayMicroDuration.Checked) Properties.Settings.Default.OverlayDuration = "micro";
			if (this.radioButtonPositionTopLeft.Checked) Properties.Settings.Default.OverlayPosition = "topleft";
			if (this.radioButtonPositionTopCenter.Checked) Properties.Settings.Default.OverlayPosition = "topcenter";
			if (this.radioButtonPositionTopRight.Checked) Properties.Settings.Default.OverlayPosition = "topright";
			if (this.radioButtonPositionMiddleLeft.Checked) Properties.Settings.Default.OverlayPosition = "middleleft";
			if (this.radioButtonPositionMiddleCenter.Checked) Properties.Settings.Default.OverlayPosition = "middlecenter";
			if (this.radioButtonPositionMiddleRight.Checked) Properties.Settings.Default.OverlayPosition = "middleright";
			if (this.radioButtonPositionBottomLeft.Checked) Properties.Settings.Default.OverlayPosition = "bottomleft";
			if (this.radioButtonPositionBottomCenter.Checked) Properties.Settings.Default.OverlayPosition = "bottomcenter";
			if (this.radioButtonPositionBottomRight.Checked) Properties.Settings.Default.OverlayPosition = "bottomright";
			if (this.radioButtonUseHotKeysToJumpToDesktopAlt.Checked) Properties.Settings.Default.HotKeysToJumpToDesktop = "Alt";
			if (this.radioButtonUseHotKeysToJumpToDesktopAltShift.Checked) Properties.Settings.Default.HotKeysToJumpToDesktop = "AltShift";
			if (this.radioButtonUseHotKeysToJumpToDesktopCtrl.Checked) Properties.Settings.Default.HotKeysToJumpToDesktop = "Ctrl";
			if (this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Checked) Properties.Settings.Default.HotKeysToJumpToDesktop = "CtrlAlt";
			// Save user settings
			// https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-write-user-settings-at-run-time-with-csharp?view=netframeworkdesktop-4.8
			Properties.Settings.Default.Save();
			Properties.Settings.Default.Reload();
		}

		public void UpdateIconForVDDisplayNumber(string theme, uint number, string name) {
			number++;
			this.notifyIconNumber.Icon = Util.Icons.GenerateNotificationIcon(number.ToString(), theme, this.DeviceDpi, false);
			//this.notifyIconNumber.Text = name;
		}

		public void UpdateIconForVDDisplayName(string theme, string name) {
			{
				var nameToShow = name;
				if (nameToShow == null) nameToShow = "";
				if (nameToShow.Length > 1) nameToShow = new StringInfo(nameToShow).SubstringByTextElements(0, 1);

				this.notifyIconName.Icon = Util.Icons.GenerateNotificationIcon(nameToShow, theme, this.DeviceDpi, false);
			}
		}

		private void SettingsForm_Load(object sender, EventArgs e) {
			App.Instance.ShowSplash();
			App.Instance.MonitorVDSwitch();
			App.Instance.MonitorFGWindowName();
			App.Instance.MonitorSystemThemeSwitch();
		}

		private void SettingsForm_Shown(object sender, EventArgs e) {
			UpdateIconForVDDisplayNumber(App.Instance.CurrentSystemThemeName, App.Instance.CurrentVDDisplayNumber, App.Instance.CurrentVDDisplayName);
			UpdateIconForVDDisplayName(App.Instance.CurrentSystemThemeName, App.Instance.CurrentVDDisplayName);
			this.notifyIconNumber.Visible = true;
			this.Hide();
		}

		private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
			if (e.ClickedItem.Tag.ToString() == "exit") App.Instance.Exit();
			else if (e.ClickedItem.Tag.ToString() == "settings") this.Show();
			else if (e.ClickedItem.Tag.ToString() == "about") App.Instance.ShowAbout();
			else if (e.ClickedItem.Tag.ToString() == "donate") App.Instance.OpenDonatePage();
		}

		private void notifyIconPrev_Click(object sender, EventArgs e) {
			App.Instance.SwitchDesktopBackward();
		}

		private void notifyIconNext_Click(object sender, EventArgs e) {
			App.Instance.SwitchDesktopForward();
		}

		private void checkBoxShowPrevNextIcons_CheckedChanged(object sender, EventArgs e) {
			if (IsLoading) return;

			if (checkBoxShowPrevNextIcons.Checked) {
				notifyIconPrev.Visible = true;
				notifyIconNext.Visible = true;
			} else {
				notifyIconPrev.Visible = false;
				notifyIconNext.Visible = false;
			}
		}

		private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e) {

		}

		private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason == CloseReason.UserClosing) {
				e.Cancel = true;
				SaveSettingsFromUI();
				Hide();
			} else if (e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing) {
				// Remove all notif icons
				notifyIconName.Visible = false;
				notifyIconNumber.Visible = false;
				notifyIconPrev.Visible = false;
				notifyIconNext.Visible = false;
			}
		}

		private void checkBoxStartupWithWindows_CheckedChanged(object sender, EventArgs e) {
			if (IsLoading) return;

			if (checkBoxStartupWithWindows.Checked) {
				App.Instance.EnableStartupWithWindows();
			} else {
				App.Instance.DisableStartupWithWindows();
			}
		}

		private void checkBoxShowOverlay_CheckedChanged(object sender, EventArgs e) {
			radioButtonOverlayMicroDuration.Enabled = checkBoxShowOverlay.Checked;
			radioButtonOverlayShortDuration.Enabled = checkBoxShowOverlay.Checked;
			radioButtonOverlayMediumDuration.Enabled = checkBoxShowOverlay.Checked;
			radioButtonOverlayLongDuration.Enabled = checkBoxShowOverlay.Checked;
			checkBoxOverlayAnimate.Enabled = checkBoxShowOverlay.Checked;
			checkBoxOverlayTranslucent.Enabled = checkBoxShowOverlay.Checked;
			checkBoxOverlayShowOnAllMonitors.Enabled = checkBoxShowOverlay.Checked;
			radioButtonPositionTopLeft.Enabled = checkBoxShowOverlay.Checked;
			radioButtonPositionTopCenter.Enabled = checkBoxShowOverlay.Checked;
			radioButtonPositionTopRight.Enabled = checkBoxShowOverlay.Checked;
			radioButtonPositionMiddleLeft.Enabled = checkBoxShowOverlay.Checked;
			radioButtonPositionMiddleCenter.Enabled = checkBoxShowOverlay.Checked;
			radioButtonPositionMiddleRight.Enabled = checkBoxShowOverlay.Checked;
			radioButtonPositionBottomLeft.Enabled = checkBoxShowOverlay.Checked;
			radioButtonPositionBottomCenter.Enabled = checkBoxShowOverlay.Checked;
			radioButtonPositionBottomRight.Enabled = checkBoxShowOverlay.Checked;

		}

		private void notifyIconPrev_DoubleClick(object sender, EventArgs e) {
			//TODO: got to first desktop
		}

		private void notifyIconNext_DoubleClick(object sender, EventArgs e) {
			//TODO: go to last desktop
		}

		private void checkBoxClickDesktopNumberTaskView_CheckedChanged(object sender, EventArgs e) {

		}

		private void notifyIconName_MouseClick(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left && this.checkBoxClickDesktopNumberTaskView.Checked) {
				// Already open?
				if (App.Instance.FGWindowHistory.Contains("Task View")) {
					// Do nothing
				} else {
					Util.OS.OpenTaskView();
				}
			}
		}

		private void notifyIconNumber_MouseClick(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left && this.checkBoxClickDesktopNumberTaskView.Checked) {
				// Already open?
				if (App.Instance.FGWindowHistory.Contains("Task View")) {
					// Do nothing
				} else {
					Util.OS.OpenTaskView();
				}
			}
		}

		private void checkBoxShowDesktopNameInitial_CheckedChanged(object sender, EventArgs e) {
			if (IsLoading) return;

			if (checkBoxShowDesktopNameInitial.Checked) {
				notifyIconName.Visible = true;
			} else {
				notifyIconName.Visible = false;
			}
		}

		private void checkBoxOverlayShowOnAllMonitors_CheckedChanged(object sender, EventArgs e) {
			if (IsLoading) return;
		}

		private void checkBoxUseHotKeysToJumpToDesktop_CheckedChanged(object sender, EventArgs e) {
			radioButtonUseHotKeysToJumpToDesktopAlt.Enabled = checkBoxUseHotKeysToJumpToDesktop.Checked;
			radioButtonUseHotKeysToJumpToDesktopAltShift.Enabled = checkBoxUseHotKeysToJumpToDesktop.Checked;
			radioButtonUseHotKeysToJumpToDesktopCtrl.Enabled = checkBoxUseHotKeysToJumpToDesktop.Checked;
			radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Enabled = checkBoxUseHotKeysToJumpToDesktop.Checked;

			if(!IsLoading) App.Instance.SetupHotKeys();
		}

		private void radioButtonUseHotKeysToJumpToDesktopAlt_CheckedChanged(object sender, EventArgs e) {
			if (!IsLoading) App.Instance.SetupHotKeys();
		}

		private void radioButtonUseHotKeysToJumpToDesktopCtrl_CheckedChanged(object sender, EventArgs e) {
			if (!IsLoading) App.Instance.SetupHotKeys();
		}

		private void radioButtonUseHotKeysToJumpToDesktopCtrlAlt_CheckedChanged(object sender, EventArgs e) {
			if (!IsLoading) App.Instance.SetupHotKeys();
		}

		private void radioButtonUseHotKeysToJumpToDesktopAltShift_CheckedChanged(object sender, EventArgs e) {
			if (!IsLoading) App.Instance.SetupHotKeys();
		}
	}
}

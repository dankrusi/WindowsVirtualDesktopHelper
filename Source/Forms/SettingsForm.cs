using System;
using System.Drawing;
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
			// Set current display icons
			UpdateIconForVDDisplayNumber(theme, App.Instance.CurrentVDDisplayNumber, App.Instance.CurrentVDDisplayName);
			UpdateIconForVDDisplayName(theme, App.Instance.CurrentVDDisplayName);
			UpdateNextPrevIconVisibility(theme);
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
			this.checkBoxShowPrevNextIcons.Checked = Settings.GetBool("feature.showPrevNextIcons");
			this.checkBoxShowDesktopNameInitial.Checked = Settings.GetBool("feature.showDesktopNameInIconTray");
			this.checkBoxStartupWithWindows.Checked = Settings.GetBool("general.startupWithWindows");
			this.checkBoxShowOverlay.Checked = Settings.GetBool("feature.showDesktopSwitchOverlay");
			this.checkBoxOverlayAnimate.Checked = Settings.GetBool("feature.showDesktopSwitchOverlay.animate");
			this.checkBoxOverlayTranslucent.Checked = Settings.GetBool("feature.showDesktopSwitchOverlay.translucent");
			this.checkBoxOverlayShowOnAllMonitors.Checked = Settings.GetBool("feature.showDesktopSwitchOverlay.showOnAllMonitors");
			this.checkBoxClickDesktopNumberTaskView.Checked = Settings.GetBool("feature.showDesktopNumberInIconTray.clickToOpenTaskView");
			this.checkBoxUseHotKeysToJumpToDesktop.Checked = Settings.GetBool("feature.useHotKeysToJumpToDesktop");
			this.radioButtonOverlayLongDuration.Checked = Settings.GetInt("feature.showDesktopSwitchOverlay.duration") == 3000;
			this.radioButtonOverlayMediumDuration.Checked = Settings.GetInt("feature.showDesktopSwitchOverlay.duration") == 2000;
			this.radioButtonOverlayShortDuration.Checked = Settings.GetInt("feature.showDesktopSwitchOverlay.duration") == 1000;
			this.radioButtonOverlayMicroDuration.Checked = Settings.GetInt("feature.showDesktopSwitchOverlay.duration") == 500;
			this.radioButtonPositionTopLeft.Checked = Settings.GetString("feature.showDesktopSwitchOverlay.position") == "topleft";
			this.radioButtonPositionTopCenter.Checked = Settings.GetString("feature.showDesktopSwitchOverlay.position") == "topcenter";
			this.radioButtonPositionTopRight.Checked = Settings.GetString("feature.showDesktopSwitchOverlay.position") == "topright";
			this.radioButtonPositionMiddleLeft.Checked = Settings.GetString("feature.showDesktopSwitchOverlay.position") == "middleleft";
			this.radioButtonPositionMiddleCenter.Checked = Settings.GetString("feature.showDesktopSwitchOverlay.position") == "middlecenter";
			this.radioButtonPositionMiddleRight.Checked = Settings.GetString("feature.showDesktopSwitchOverlay.position") == "middleright";
			this.radioButtonPositionBottomLeft.Checked = Settings.GetString("feature.showDesktopSwitchOverlay.position") == "bottomleft";
			this.radioButtonPositionBottomCenter.Checked = Settings.GetString("feature.showDesktopSwitchOverlay.position") == "bottomcenter";
			this.radioButtonPositionBottomRight.Checked = Settings.GetString("feature.showDesktopSwitchOverlay.position") == "bottomright";
			this.radioButtonUseHotKeysToJumpToDesktopAlt.Checked = Settings.GetString("feature.useHotKeysToJumpToDesktop.hotkey") == "Alt";
			this.radioButtonUseHotKeysToJumpToDesktopAltShift.Checked = Settings.GetString("feature.useHotKeysToJumpToDesktop.hotkey") == "AltShift";
			this.radioButtonUseHotKeysToJumpToDesktopCtrl.Checked = Settings.GetString("feature.useHotKeysToJumpToDesktop.hotkey") == "Ctrl";
			this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Checked = Settings.GetString("feature.useHotKeysToJumpToDesktop.hotkey") == "CtrlAlt";

			checkBoxShowOverlay_CheckedChanged(this, null);
			checkBoxUseHotKeysToJumpToDesktop_CheckedChanged(this, null);
		}
		private void SaveSettingsFromUI() {
			Settings.SetBool("feature.showPrevNextIcons", this.checkBoxShowPrevNextIcons.Checked);
			Settings.SetBool("feature.showDesktopNameInIconTray", this.checkBoxShowDesktopNameInitial.Checked);
			Settings.SetBool("general.startupWithWindows", this.checkBoxStartupWithWindows.Checked);
			Settings.SetBool("feature.showDesktopSwitchOverlay", this.checkBoxShowOverlay.Checked);
			Settings.SetBool("feature.showDesktopSwitchOverlay.animate", this.checkBoxOverlayAnimate.Checked);
			Settings.SetBool("feature.showDesktopSwitchOverlay.translucent", this.checkBoxOverlayTranslucent.Checked);
			Settings.SetBool("feature.showDesktopSwitchOverlay.showOnAllMonitors", this.checkBoxOverlayShowOnAllMonitors.Checked);
			Settings.SetBool("feature.showDesktopNumberInIconTray.clickToOpenTaskView", this.checkBoxClickDesktopNumberTaskView.Checked);
			Settings.SetBool("feature.useHotKeysToJumpToDesktop", this.checkBoxUseHotKeysToJumpToDesktop.Checked);
			if(this.radioButtonOverlayLongDuration.Checked) Settings.SetInt("feature.showDesktopSwitchOverlay.duration", 3000);
			if(this.radioButtonOverlayMediumDuration.Checked) Settings.SetInt("feature.showDesktopSwitchOverlay.duration", 2000);
			if(this.radioButtonOverlayShortDuration.Checked) Settings.SetInt("feature.showDesktopSwitchOverlay.duration", 1000);
			if(this.radioButtonOverlayMicroDuration.Checked) Settings.SetInt("feature.showDesktopSwitchOverlay.duration", 500);
			if(this.radioButtonPositionTopLeft.Checked) Settings.SetString("feature.showDesktopSwitchOverlay.position", "topleft");
			if(this.radioButtonPositionTopCenter.Checked) Settings.SetString("feature.showDesktopSwitchOverlay.position", "topcenter");
			if(this.radioButtonPositionTopRight.Checked) Settings.SetString("feature.showDesktopSwitchOverlay.position", "topright");
			if(this.radioButtonPositionMiddleLeft.Checked) Settings.SetString("feature.showDesktopSwitchOverlay.position", "middleleft");
			if(this.radioButtonPositionMiddleCenter.Checked) Settings.SetString("feature.showDesktopSwitchOverlay.position", "middlecenter");
			if(this.radioButtonPositionMiddleRight.Checked) Settings.SetString("feature.showDesktopSwitchOverlay.position", "middleright");
			if(this.radioButtonPositionBottomLeft.Checked) Settings.SetString("feature.showDesktopSwitchOverlay.position", "bottomleft");
			if(this.radioButtonPositionBottomCenter.Checked) Settings.SetString("feature.showDesktopSwitchOverlay.position", "bottomcenter");
			if(this.radioButtonPositionBottomRight.Checked) Settings.SetString("feature.showDesktopSwitchOverlay.position", "bottomright");
			if(this.radioButtonUseHotKeysToJumpToDesktopAlt.Checked) Settings.SetString("feature.useHotKeysToJumpToDesktop.hotkey", "Alt");
			if(this.radioButtonUseHotKeysToJumpToDesktopAltShift.Checked) Settings.SetString("feature.useHotKeysToJumpToDesktop.hotkey", "AltShift");
			if(this.radioButtonUseHotKeysToJumpToDesktopCtrl.Checked) Settings.SetString("feature.useHotKeysToJumpToDesktop.hotkey", "Ctrl");
			if(this.radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Checked) Settings.SetString("feature.useHotKeysToJumpToDesktop.hotkey", "CtrlAlt");
			// Save user settings
			Settings.SaveConfig();
		}

		public void UpdateIconForVDDisplayNumber(string theme, uint number, string name) {
			number++;
			this.notifyIconNumber.Icon = Util.Icons.GenerateNotificationIcon(number.ToString(), theme, this.DeviceDpi, false, FontStyle.Bold);
		}

		public void UpdateIconForVDDisplayName(string theme, string name) {
			{
				var nameToShow = name;
				if (nameToShow == null) nameToShow = "";
				if (nameToShow.Length > 1) nameToShow = new StringInfo(nameToShow).SubstringByTextElements(0, 1);

				this.notifyIconName.Icon = Util.Icons.GenerateNotificationIcon(nameToShow, theme, this.DeviceDpi, false);
			}
		}

		public void UpdateNextPrevIconVisibility(string theme) {
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
			int count = App.Instance.CurrentVDDisplayCount - 1;
			if (checkBoxShowPrevNextIcons.Checked) {
				var hasNextDesktop = count != 0 && App.Instance.CurrentVDDisplayNumber != count;
				var hasPrevDesktop = App.Instance.CurrentVDDisplayNumber != 0;
				// Update prev/next icons
				notifyIconPrev.Icon = Util.Icons.GenerateNotificationIcon(prevChar, theme, this.DeviceDpi, true, FontStyle.Regular, hasPrevDesktop ? 1.0f : 0.5f);
				notifyIconNext.Icon = Util.Icons.GenerateNotificationIcon(nextChar, theme, this.DeviceDpi, true, FontStyle.Regular, hasNextDesktop ? 1.0f : 0.5f);
				// Show or hide?
				if(Settings.GetBool("feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds")) {
					notifyIconNext.Visible = hasNextDesktop;
					notifyIconPrev.Visible = hasPrevDesktop;
				} else {
					notifyIconNext.Visible = true;
					notifyIconPrev.Visible = true;
				}
			}
		}

		private void SettingsForm_Load(object sender, EventArgs e) {
			App.Instance.ShowSplash();
			App.Instance.MonitorVDSwitch();
			App.Instance.MonitorFGWindowName();
			App.Instance.MonitorSystemThemeSwitch();
			App.Instance.MonitorVDisplayCount();
		}

		private void SettingsForm_Shown(object sender, EventArgs e) {
			UpdateIconForVDDisplayNumber(App.Instance.CurrentSystemThemeName, App.Instance.CurrentVDDisplayNumber, App.Instance.CurrentVDDisplayName);
			UpdateIconForVDDisplayName(App.Instance.CurrentSystemThemeName, App.Instance.CurrentVDDisplayName);
			UpdateNextPrevIconVisibility(App.Instance.CurrentSystemThemeName);
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
				UpdateNextPrevIconVisibility(App.Instance.CurrentSystemThemeName);
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

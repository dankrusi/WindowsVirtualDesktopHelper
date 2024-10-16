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

			// Make sure the form is invisible on load
			this.Visible = false;
			this.ShowInTaskbar = false;

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

			IsLoading = false;
		}

		public void UpdateIconsForTheme(string theme) {
			// Set current display icons
			UpdateIconForVDDisplayNumber(theme, App.Instance.CurrentVDDisplayNumber, App.Instance.CurrentVDDisplayName);
			UpdateIconForVDDisplayName(theme, App.Instance.CurrentVDDisplayName);
			UpdateNextPrevIconVisibility(theme);
		}
		

		private void LoadSettingsIntoUI() {

			//TODO: how to sync the startup setting - best would be to see if the reg key is actually there

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
			// Save user settings to storage
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

		#region Form Events

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

		private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e) {

		}

		private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
			if(e.CloseReason == CloseReason.UserClosing) {
				e.Cancel = true;
				SaveSettingsFromUI();
				Hide();
			} else if(e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing) {
				// Remove all notif icons
				notifyIconName.Visible = false;
				notifyIconNumber.Visible = false;
				notifyIconPrev.Visible = false;
				notifyIconNext.Visible = false;
			}
		}

		#endregion

		#region Menu and Icon Tray Events

		private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
			if(e.ClickedItem.Tag.ToString() == "exit") App.Instance.Exit();
			else if(e.ClickedItem.Tag.ToString() == "settings") App.Instance.ShowSettings();
			else if(e.ClickedItem.Tag.ToString() == "about") App.Instance.ShowAbout();
			else if(e.ClickedItem.Tag.ToString() == "donate") App.Instance.OpenDonatePage();
		}

		private void notifyIconPrev_Click(object sender, EventArgs e) {
			App.Instance.SwitchDesktopBackward();
		}

		private void notifyIconNext_Click(object sender, EventArgs e) {
			App.Instance.SwitchDesktopForward();
		}

		private void notifyIconPrev_DoubleClick(object sender, EventArgs e) {
			//TODO: got to first desktop
		}

		private void notifyIconNext_DoubleClick(object sender, EventArgs e) {
			//TODO: go to last desktop
		}

		private void notifyIconName_MouseClick(object sender, MouseEventArgs e) {
			if(e.Button == MouseButtons.Left && this.checkBoxClickDesktopNumberTaskView.Checked) {
				// Already open?
				if(App.Instance.FGWindowHistory.Contains("Task View")) {
					// Do nothing
				} else {
					Util.OS.OpenTaskView();
				}
			}
		}

		private void notifyIconNumber_MouseClick(object sender, MouseEventArgs e) {
			if(e.Button == MouseButtons.Left && this.checkBoxClickDesktopNumberTaskView.Checked) {
				// Already open?
				if(App.Instance.FGWindowHistory.Contains("Task View")) {
					// Do nothing
				} else {
					Util.OS.OpenTaskView();
				}
			}
		}

		#endregion





		#region Settings UI Events


		private void checkBoxStartupWithWindows_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			Settings.SetBool("general.startupWithWindows", this.checkBoxStartupWithWindows.Checked);

			if(checkBoxStartupWithWindows.Checked) {
				App.Instance.EnableStartupWithWindows();
			} else {
				App.Instance.DisableStartupWithWindows();
			}
		}

		private void checkBoxShowPrevNextIcons_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			Settings.SetBool("feature.showPrevNextIcons", this.checkBoxShowPrevNextIcons.Checked);

			if(checkBoxShowPrevNextIcons.Checked) {
				UpdateNextPrevIconVisibility(App.Instance.CurrentSystemThemeName);
			} else {
				notifyIconPrev.Visible = false;
				notifyIconNext.Visible = false;
			}
		}

		private void checkBoxShowDesktopNameInitial_CheckedChanged(object sender, EventArgs e) {
			if (IsLoading) return;
			Settings.SetBool("feature.showDesktopNameInIconTray", this.checkBoxShowDesktopNameInitial.Checked);

			if (checkBoxShowDesktopNameInitial.Checked) {
				notifyIconName.Visible = true;
			} else {
				notifyIconName.Visible = false;
			}
		}

		private void checkBoxClickDesktopNumberTaskView_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			Settings.SetBool("feature.showDesktopNumberInIconTray.clickToOpenTaskView", this.checkBoxClickDesktopNumberTaskView.Checked);
		}

		private void checkBoxOverlayShowOnAllMonitors_CheckedChanged(object sender, EventArgs e) {
			if (IsLoading) return;
			Settings.SetBool("feature.showDesktopSwitchOverlay.showOnAllMonitors", this.checkBoxOverlayShowOnAllMonitors.Checked);
		}

		private void checkBoxShowOverlay_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			Settings.SetBool("feature.showDesktopSwitchOverlay", this.checkBoxShowOverlay.Checked);

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

		private void checkBoxUseHotKeysToJumpToDesktop_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			Settings.SetBool("feature.useHotKeysToJumpToDesktop", this.checkBoxUseHotKeysToJumpToDesktop.Checked);

			radioButtonUseHotKeysToJumpToDesktopAlt.Enabled = checkBoxUseHotKeysToJumpToDesktop.Checked;
			radioButtonUseHotKeysToJumpToDesktopAltShift.Enabled = checkBoxUseHotKeysToJumpToDesktop.Checked;
			radioButtonUseHotKeysToJumpToDesktopCtrl.Enabled = checkBoxUseHotKeysToJumpToDesktop.Checked;
			radioButtonUseHotKeysToJumpToDesktopCtrlAlt.Enabled = checkBoxUseHotKeysToJumpToDesktop.Checked;

			if(!IsLoading) App.Instance.SetupHotKeys();
		}

		private void radioButtonUseHotKeysToJumpToDesktopAlt_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.useHotKeysToJumpToDesktop.hotkey", "Alt");
				App.Instance.SetupHotKeys();
			}
		}

		private void radioButtonUseHotKeysToJumpToDesktopCtrl_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.useHotKeysToJumpToDesktop.hotkey", "Ctrl");
				App.Instance.SetupHotKeys();
			}
		}

		private void radioButtonUseHotKeysToJumpToDesktopCtrlAlt_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.useHotKeysToJumpToDesktop.hotkey", "CtrlAlt");
				App.Instance.SetupHotKeys();
			}
		}

		private void radioButtonUseHotKeysToJumpToDesktopAltShift_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.useHotKeysToJumpToDesktop.hotkey", "AltShift");
				App.Instance.SetupHotKeys();
			}
		}

		private void radioButtonOverlayLongDuration_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetInt("feature.showDesktopSwitchOverlay.duration", 3000);
			}
		}

		private void radioButtonOverlayMediumDuration_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetInt("feature.showDesktopSwitchOverlay.duration", 2000);
			}
		}

		private void radioButtonOverlayShortDuration_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetInt("feature.showDesktopSwitchOverlay.duration", 1000);
			}
		}

		private void radioButtonOverlayMicroDuration_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetInt("feature.showDesktopSwitchOverlay.duration", 500);
			}
		}

		private void checkBoxOverlayAnimate_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			Settings.SetBool("feature.showDesktopSwitchOverlay.animate", this.checkBoxOverlayAnimate.Checked);
		}

		private void checkBoxOverlayTranslucent_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			Settings.SetBool("feature.showDesktopSwitchOverlay.translucent", this.checkBoxOverlayTranslucent.Checked);
		}

		private void radioButtonPositionTopLeft_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.showDesktopSwitchOverlay.position", "topleft");
			}
		}

		private void radioButtonPositionTopCenter_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.showDesktopSwitchOverlay.position", "topcenter");
			}
		}

		private void radioButtonPositionTopRight_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.showDesktopSwitchOverlay.position", "topright");
			}
		}

		private void radioButtonPositionMiddleLeft_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.showDesktopSwitchOverlay.position", "middleleft");
			}
		}

		private void radioButtonPositionMiddleCenter_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.showDesktopSwitchOverlay.position", "middlecenter");
			}
		}

		private void radioButtonPositionMiddleRight_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.showDesktopSwitchOverlay.position", "middleright");
			}
		}

		private void radioButtonPositionBottomLeft_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.showDesktopSwitchOverlay.position", "bottomleft");
			}
		}

		private void radioButtonPositionBottomCenter_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.showDesktopSwitchOverlay.position", "bottomcenter");
			}
		}

		private void radioButtonPositionBottomRight_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			if((sender as RadioButton).Checked == true) {
				Settings.SetString("feature.showDesktopSwitchOverlay.position", "bottomright");
			}
		}

		#endregion
	}
}

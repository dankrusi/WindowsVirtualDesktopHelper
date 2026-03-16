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

			IsLoading = false;
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
			this.checkBoxUseHotKeysToJumpToDesktop.Checked = Settings.GetBool("feature.useHotKeyToJumpToDesktopNumber");
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

			if (this.checkBoxUseHotKeysToJumpToDesktop.Checked) {
				string keyModifierSetting = Settings.GetString("feature.useHotKeyToJumpToDesktopNumber.hotkey").Replace(" + ", ", ");
				if (KeyModifiers.TryParse(keyModifierSetting, true, out KeyModifiers keyModifiers)) {

					this.checkBoxUseHotKeysToJumpToDesktopCtrl.Checked = keyModifiers.HasFlag(KeyModifiers.Ctrl);
					this.checkBoxUseHotKeysToJumpToDesktopAlt.Checked = keyModifiers.HasFlag(KeyModifiers.Alt);
					this.checkBoxUseHotKeysToJumpToDesktopShift.Checked = keyModifiers.HasFlag(KeyModifiers.Shift);
					this.checkBoxUseHotKeysToJumpToDesktopWin.Checked = keyModifiers.HasFlag(KeyModifiers.Win);
				}
			}

			EnableDisableModifierCheckboxes(checkBoxUseHotKeysToJumpToDesktop.Checked);
			ValidateHotkeys();
		}

		private void SaveSettingsFromUI() {
			// Save user settings to storage
			Settings.SaveConfig();
		}

		#region Form Events

		private void SettingsForm_Load(object sender, EventArgs e) {
			
		}

		private void SettingsForm_Shown(object sender, EventArgs e) {
			
		}

		private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e) {
			
		}

		private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
			if(e.CloseReason == CloseReason.UserClosing) {
				e.Cancel = true;

				var useHotkeys = this.checkBoxUseHotKeysToJumpToDesktop.Checked && ValidateHotkeys();
				Settings.SetBool("feature.useHotKeyToJumpToDesktopNumber", useHotkeys);
				SaveSettingsFromUI();
				if (useHotkeys || !this.checkBoxUseHotKeysToJumpToDesktop.Checked) App.Instance.SetupHotKeys();

				Hide();
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

			App.Instance.UIUpdate();
		}

		private void checkBoxShowDesktopNameInitial_CheckedChanged(object sender, EventArgs e) {
			if (IsLoading) return;
			Settings.SetBool("feature.showDesktopNameInIconTray", this.checkBoxShowDesktopNameInitial.Checked);

			App.Instance.UIUpdate();
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

		private void EnableDisableModifierCheckboxes(bool enable) {
			checkBoxUseHotKeysToJumpToDesktopCtrl.Enabled = enable;
			checkBoxUseHotKeysToJumpToDesktopAlt.Enabled = enable;
			checkBoxUseHotKeysToJumpToDesktopShift.Enabled = enable;
			checkBoxUseHotKeysToJumpToDesktopWin.Enabled = enable;
		}

		private bool ValidateHotkeys() {
			if (checkBoxUseHotKeysToJumpToDesktop.Checked) {
				KeyModifiers keyModifiers = KeyModifiers.None;

				if (checkBoxUseHotKeysToJumpToDesktopCtrl.Checked) keyModifiers |= KeyModifiers.Ctrl;
				if (checkBoxUseHotKeysToJumpToDesktopAlt.Checked) keyModifiers |= KeyModifiers.Alt;
				if (checkBoxUseHotKeysToJumpToDesktopShift.Checked) keyModifiers |= KeyModifiers.Shift;
				if (checkBoxUseHotKeysToJumpToDesktopWin.Checked) keyModifiers |= KeyModifiers.Win;

				if (keyModifiers == KeyModifiers.None) {
					labelKeyModifiersAreRequired.Text = "REQUIRED";
					labelKeyModifiersAreRequired.Visible = true;
				} else if (keyModifiers == KeyModifiers.Shift || keyModifiers == KeyModifiers.Win) {
					labelKeyModifiersAreRequired.Text = "NOT RECOMMENDED";
					labelKeyModifiersAreRequired.Visible = true;
				} else labelKeyModifiersAreRequired.Visible = false;
				
				return keyModifiers != KeyModifiers.None;
			}
			
			labelKeyModifiersAreRequired.Visible = false;
			return true;
		}

		private KeyModifiers GetHotkeys() {
			if (!checkBoxUseHotKeysToJumpToDesktop.Checked) return KeyModifiers.None;

			KeyModifiers keyModifiers = KeyModifiers.None;
			if (checkBoxUseHotKeysToJumpToDesktopCtrl.Checked) keyModifiers |= KeyModifiers.Ctrl;
			if (checkBoxUseHotKeysToJumpToDesktopAlt.Checked) keyModifiers |= KeyModifiers.Alt;
			if (checkBoxUseHotKeysToJumpToDesktopShift.Checked) keyModifiers |= KeyModifiers.Shift;
			if (checkBoxUseHotKeysToJumpToDesktopWin.Checked) keyModifiers |= KeyModifiers.Win;
			return keyModifiers;
		}

		private void checkBoxUseHotKeysToJumpToDesktop_CheckedChanged(object sender, EventArgs e) {
			if(IsLoading) return;
			Settings.SetBool("feature.useHotKeyToJumpToDesktopNumber", this.checkBoxUseHotKeysToJumpToDesktop.Checked);
			EnableDisableModifierCheckboxes(checkBoxUseHotKeysToJumpToDesktop.Checked);

			ValidateHotkeys();
		}

		private void checkBoxUseHotKeysToJumpToDesktopModifier_CheckedChanged(object sender, EventArgs e) {
			if (IsLoading) return;

			if (ValidateHotkeys()) {
				KeyModifiers keyModifiers = GetHotkeys();
				Settings.SetString("feature.useHotKeyToJumpToDesktopNumber.hotkey", keyModifiers.ToString().Replace(", ", " + "));
			} else {
				Settings.SetBool("feature.useHotKeyToJumpToDesktopNumber", false);
				Settings.SetString("feature.useHotKeyToJumpToDesktopNumber.hotkey", "None");
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

		// Define a new KeyModifiers - don't use ModifierKeys in KeyboardHook.cs because:
		// 1. It conflicts with the WinForms ModifierKeys property
		// 2. This enum defines the keys in a different order that are IMHO more natural, and are used in this order when writing out to the config file
		[Flags]
		enum KeyModifiers {
			None = 0,
			Ctrl = 1,
			Alt = 2,
			Shift = 4,
			Win = 8
		}
	}
}

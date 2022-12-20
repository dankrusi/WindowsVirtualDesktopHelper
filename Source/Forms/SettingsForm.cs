using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper {
    public partial class SettingsForm : Form {

        private bool IsLoading;

        public SettingsForm() {
            IsLoading = true;

            // Init UI
            InitializeComponent();
            LoadSettingsIntoUI();

            if(!App.Instance.IsSystemLightThemeModeEnabled()) {
                // Dark mode - icons should be white
                notifyIconPrev.Icon = Resources.Icons.chevron_left_256_white;
                notifyIconNext.Icon = Resources.Icons.chevron_right_256_white;
            } else {
                // Light mode - icons should be black
                notifyIconPrev.Icon = Resources.Icons.chevron_left_256_black;
                notifyIconNext.Icon = Resources.Icons.chevron_right_256_black;
            }

            // Apply some settings
            if (checkBoxShowPrevNextIcons.Checked) {
                notifyIconPrev.Visible = true;
                notifyIconNext.Visible = true;
            } else {
                notifyIconPrev.Visible = false;
                notifyIconNext.Visible = false;
            }

            //TODO: how to sync the startup setting - best would be to see if the reg key is actually there

            IsLoading = false;
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
        public int OverlayDurationMS() {
            if (this.radioButtonOverlayLongDuration.Checked == true) return 3000;
            else if (this.radioButtonOverlayMediumDuration.Checked == true) return 2000;
            else if (this.radioButtonOverlayShortDuration.Checked == true) return 1000;
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
            this.checkBoxShowPrevNextIcons.Checked = GetBool("ShowPrevNextIcons", false);
            this.checkBoxStartupWithWindows.Checked = GetBool("StartupWithWindows", false);
            this.checkBoxShowOverlay.Checked = GetBool("ShowOverlay", true);
            this.checkBoxOverlayAnimate.Checked = GetBool("OverlayAnimate", true);
            this.checkBoxOverlayTranslucent.Checked = GetBool("OverlayTranslucent", true);
            this.radioButtonOverlayLongDuration.Checked = GetString("OverlayDuration","medium") == "long";
            this.radioButtonOverlayMediumDuration.Checked = GetString("OverlayDuration", "medium") == "medium";
            this.radioButtonOverlayShortDuration.Checked = GetString("OverlayDuration", "medium") == "short";
            this.radioButtonPositionTopLeft.Checked = GetString("OverlayPosition", "middlecenter") == "topleft";
            this.radioButtonPositionTopCenter.Checked = GetString("OverlayPosition", "middlecenter") == "topcenter";
            this.radioButtonPositionTopRight.Checked = GetString("OverlayPosition", "middlecenter") == "topright";
            this.radioButtonPositionMiddleLeft.Checked = GetString("OverlayPosition", "middlecenter") == "middleleft";
            this.radioButtonPositionMiddleCenter.Checked = GetString("OverlayPosition", "middlecenter") == "middlecenter";
            this.radioButtonPositionMiddleRight.Checked = GetString("OverlayPosition", "middlecenter") == "middleright";
            this.radioButtonPositionBottomLeft.Checked = GetString("OverlayPosition", "middlecenter") == "bottomleft";
            this.radioButtonPositionBottomCenter.Checked = GetString("OverlayPosition", "middlecenter") == "bottomcenter";
            this.radioButtonPositionBottomRight.Checked = GetString("OverlayPosition", "middlecenter") == "bottomright";
            checkBoxShowOverlay_CheckedChanged(this, null);
        }
        private void SaveSettingsFromUI() {
            SetBool("ShowPrevNextIcons", this.checkBoxShowPrevNextIcons.Checked);
            SetBool("StartupWithWindows", this.checkBoxStartupWithWindows.Checked);
            SetBool("ShowOverlay", this.checkBoxShowOverlay.Checked);
            SetBool("OverlayAnimate", this.checkBoxOverlayAnimate.Checked);
            SetBool("OverlayTranslucent", this.checkBoxOverlayTranslucent.Checked);
            if (this.radioButtonOverlayLongDuration.Checked) SetString("OverlayDuration", "long");
            if (this.radioButtonOverlayMediumDuration.Checked) SetString("OverlayDuration", "medium");
            if (this.radioButtonOverlayShortDuration.Checked) SetString("OverlayDuration", "short");
            if (this.radioButtonPositionTopLeft.Checked) SetString("OverlayPosition", "topleft");
            if (this.radioButtonPositionTopCenter.Checked) SetString("OverlayPosition", "topcenter");
            if (this.radioButtonPositionTopRight.Checked) SetString("OverlayPosition", "topright");
            if (this.radioButtonPositionMiddleLeft.Checked) SetString("OverlayPosition", "middleleft");
            if (this.radioButtonPositionMiddleCenter.Checked) SetString("OverlayPosition", "middlecenter");
            if (this.radioButtonPositionMiddleRight.Checked) SetString("OverlayPosition", "middleright");
            if (this.radioButtonPositionBottomLeft.Checked) SetString("OverlayPosition", "bottomleft");
            if (this.radioButtonPositionBottomCenter.Checked) SetString("OverlayPosition", "bottomcenter");
            if (this.radioButtonPositionBottomRight.Checked) SetString("OverlayPosition", "bottomright");
        }

        static string GetString(string key, string defaultValue) {
            try {
                var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                var settings = config.AppSettings.Settings;
                if (settings[key] == null || settings[key].Value == null) return defaultValue;
                return settings[key].Value;
            } catch (Exception e) {
                Util.Logging.WriteLine("Error reading app settings: " + e.Message);
                return defaultValue;
            }
        }

        static void SetString(string key, string value) {
            try {
                var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                var settings = config.AppSettings.Settings;
                if (settings[key] == null) {
                    settings.Add(key, value);
                } else {
                    settings[key].Value = value;
                }
                config.Save(ConfigurationSaveMode.Modified);
            } catch (Exception e) {
                Util.Logging.WriteLine("Error saving app settings: " + e.Message);
            }
        }

        static bool GetBool(string key, bool defaultValue) {
            try {
                return bool.Parse(GetString(key,defaultValue.ToString().ToLower()));
            } catch (Exception e) {
                Util.Logging.WriteLine("Error reading app settings: "+e.Message);
                return defaultValue;
            }
        }

        static void SetBool(string key, bool value) {
            SetString(key, value.ToString().ToLower());
        }

        public void UpdateIconForVDDisplayNumber(uint number) {
            number++;
            if (!App.Instance.IsSystemLightThemeModeEnabled()) {
                // White icon
                if (number == 1) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_1_256_white;
                } else if (number == 2) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_2_256_white;
                } else if (number == 3) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_3_256_white;
                } else if (number == 4) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_4_256_white;
                } else if (number == 5) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_5_256_white;
                } else if (number == 6) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_6_256_white;
                } else if (number == 7) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_7_256_white;
                } else if (number == 8) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_8_256_white;
                } else if (number == 9) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_9_256_white;
                } else {
                    this.notifyIconNumber.Icon = Resources.Icons.number_plus_256_white;
                }
            } else {
                // Black icon
                if (number == 1) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_1_256_black;
                } else if (number == 2) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_2_256_black;
                } else if (number == 3) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_3_256_black;
                } else if (number == 4) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_4_256_black;
                } else if (number == 5) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_5_256_black;
                } else if (number == 6) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_6_256_black;
                } else if (number == 7) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_7_256_black;
                } else if (number == 8) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_8_256_black;
                } else if (number == 9) {
                    this.notifyIconNumber.Icon = Resources.Icons.number_9_256_black;
                } else {
                    this.notifyIconNumber.Icon = Resources.Icons.number_plus_256_black;
                }
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e) {
            App.Instance.ShowSplash();
            App.Instance.MonitorVDSwitch();
        }

        private void SettingsForm_Shown(object sender, EventArgs e) {
            UpdateIconForVDDisplayNumber(App.Instance.CurrentVDDisplayNumber);
            this.notifyIconNumber.Visible = true;
            this.Hide();
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if (e.ClickedItem.Tag.ToString() == "exit") App.Instance.Exit();
            else if (e.ClickedItem.Tag.ToString() == "settings") this.Show();
            else if (e.ClickedItem.Tag.ToString() == "about") App.Instance.ShowAbout();
        }

        private void notifyIconPrev_Click(object sender, EventArgs e) {
            App.Instance.VDAPI.SwitchBackward();
        }

        private void notifyIconNext_Click(object sender, EventArgs e) {
            App.Instance.VDAPI.SwitchForward();
        }

        private void checkBoxShowPrevNextIcons_CheckedChanged(object sender, EventArgs e) {
            if (IsLoading) return;
            
            if(checkBoxShowPrevNextIcons.Checked) {
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
            radioButtonOverlayShortDuration.Enabled = checkBoxShowOverlay.Checked;
            radioButtonOverlayMediumDuration.Enabled = checkBoxShowOverlay.Checked;
            radioButtonOverlayLongDuration.Enabled = checkBoxShowOverlay.Checked;
            checkBoxOverlayAnimate.Enabled = checkBoxShowOverlay.Checked;
            checkBoxOverlayTranslucent.Enabled = checkBoxShowOverlay.Checked;
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
    }
}

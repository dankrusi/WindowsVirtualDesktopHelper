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

        private void LoadSettingsIntoUI() {
            this.checkBoxShowPrevNextIcons.CheckState = GetBool("ShowPrevNextIcons", false) ? CheckState.Checked: CheckState.Unchecked;
            this.checkBoxStartupWithWindows.CheckState = GetBool("StartupWithWindows", false) ? CheckState.Checked : CheckState.Unchecked;

            
        }
        private void SaveSettingsFromUI() {
            SetBool("ShowPrevNextIcons", this.checkBoxShowPrevNextIcons.Checked);
            SetBool("StartupWithWindows", this.checkBoxStartupWithWindows.Checked);
        }

        static bool GetBool(string key, bool defaultValue) {
            try {
                var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                var settings = config.AppSettings.Settings;
                if (settings[key] == null || settings[key].Value == null) return defaultValue;
                return bool.Parse(settings[key].Value);
            } catch (Exception e) {
                Console.WriteLine("Error reading app settings: "+e.Message);
                return defaultValue;
            }
        }

        static void SetBool(string key, bool value) {
            try {
                var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                var settings = config.AppSettings.Settings;
                if(settings[key] == null) {
                    settings.Add(key, value.ToString().ToLower());
                } else {
                    settings[key].Value = value.ToString().ToLower();
                }
                config.Save(ConfigurationSaveMode.Modified);
            } catch (Exception e) {
                Console.WriteLine("Error saving app settings: " + e.Message);
            }
        }

        public void UpdateIconForVDDisplayNumber(uint number) {
            number++;
            if(number == 1) {
                this.notifyIconNumber.Icon = Icons.circled_1_256;
            } else if (number == 2) {
                this.notifyIconNumber.Icon = Icons.circled_2_256;
            } else if (number == 3) {
                this.notifyIconNumber.Icon = Icons.circled_3_256;
            } else if (number == 4) {
                this.notifyIconNumber.Icon = Icons.circled_4_256;
            } else if (number == 5) {
                this.notifyIconNumber.Icon = Icons.circled_5_256;
            } else if (number == 6) {
                this.notifyIconNumber.Icon = Icons.circled_6_256;
            } else if (number == 7) {
                this.notifyIconNumber.Icon = Icons.circled_7_256;
            } else if (number == 8) {
                this.notifyIconNumber.Icon = Icons.circled_8_256;
            } else if (number == 9) {
                this.notifyIconNumber.Icon = Icons.circled_9_256;
            } else {
                this.notifyIconNumber.Icon = Icons.circled_plus_256;
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
            SaveSettingsFromUI();
        }

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e) {
            
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                Hide();
            }
        }

        private void checkBoxStartupWithWindows_CheckedChanged(object sender, EventArgs e) {
            if (IsLoading) return;

            if (checkBoxStartupWithWindows.Checked) {
                App.Instance.EnableStartupWithWindows();
            } else {
                App.Instance.DisableStartupWithWindows();
            }
            SaveSettingsFromUI();
        }
    }
}

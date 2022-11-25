using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper {
    public partial class SettingsForm : Form {



        public SettingsForm() {
            InitializeComponent();
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
                Hide();
            }
        }
    }
}

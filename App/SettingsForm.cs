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
                this.notifyIcon1.Icon = Icons.circled_1_256;
            } else if (number == 2) {
                this.notifyIcon1.Icon = Icons.circled_2_256;
            } else if (number == 3) {
                this.notifyIcon1.Icon = Icons.circled_3_256;
            } else if (number == 4) {
                this.notifyIcon1.Icon = Icons.circled_4_256;
            } else if (number == 5) {
                this.notifyIcon1.Icon = Icons.circled_5_256;
            } else if (number == 6) {
                this.notifyIcon1.Icon = Icons.circled_6_256;
            } else if (number == 7) {
                this.notifyIcon1.Icon = Icons.circled_7_256;
            } else if (number == 8) {
                this.notifyIcon1.Icon = Icons.circled_8_256;
            } else if (number == 9) {
                this.notifyIcon1.Icon = Icons.circled_9_256;
            } else {
                this.notifyIcon1.Icon = Icons.circled_plus_256;
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e) {
            App.Instance.ShowSplash();
            App.Instance.MonitorVDSwitch();
        }

        private void SettingsForm_Shown(object sender, EventArgs e) {
            UpdateIconForVDDisplayNumber(App.Instance.CurrentVDDisplayNumber);
            this.notifyIcon1.Visible = true;
            this.Hide();
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if (e.ClickedItem.Tag.ToString() == "exit") App.Instance.Exit();
            else if (e.ClickedItem.Tag.ToString() == "about") App.Instance.ShowAbout();
        }
    }
}

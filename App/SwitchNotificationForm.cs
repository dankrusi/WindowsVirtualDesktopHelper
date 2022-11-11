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
    public partial class SwitchNotificationForm : Form {

        public string LabelText;
        public int DisplayTimeMS = 1000;

        public SwitchNotificationForm() {
            InitializeComponent();
        }

        private void SwitchNotificationForm_Shown(object sender, EventArgs e) {
            this.timer1.Interval = this.DisplayTimeMS;
            this.timer1.Start();
        }

        private void SwitchNotificationForm_Load(object sender, EventArgs e) {
            this.label1.Text = LabelText;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            this.Close();
        }
    }
}

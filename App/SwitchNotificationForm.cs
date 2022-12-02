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
        public int DisplayTimeMS = 2000;

        public SwitchNotificationForm() {
            InitializeComponent();
        }

        // Preven from from being focusable
        // Via https://stackoverflow.com/questions/2423234/make-a-form-not-focusable-in-c-sharp
        private const int WS_EX_NOACTIVATE = 0x08000000;
        protected override CreateParams CreateParams {
            get {
                var createParams = base.CreateParams;

                createParams.ExStyle |= WS_EX_NOACTIVATE;
                return createParams;
            }
        }

        private void SwitchNotificationForm_Shown(object sender, EventArgs e) {
            this.timerClose.Interval = this.DisplayTimeMS;
            this.timerClose.Start();
        }

        private void SwitchNotificationForm_Load(object sender, EventArgs e) {
            this.label1.Text = LabelText;
        }

        private void timerClose_Tick(object sender, EventArgs e) {
            this.Close();
        }
    }
}

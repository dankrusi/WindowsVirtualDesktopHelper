using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper {
    class Program {

        [STAThread]
        public static void Main(string[] args) {
            var app = new App();
            Application.EnableVisualStyles();
            Application.Run(app.SettingsForm);
        }
    }
}

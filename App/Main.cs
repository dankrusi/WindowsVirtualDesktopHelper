using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper {
    class Program {

        [STAThread]
        public static void Main(string[] args) {
            try {
                var app = new App();
                Application.EnableVisualStyles();
                Application.Run(app.SettingsForm);
            }catch(Exception e) {
                // Global error handler
                Console.Error.WriteLine(e);
                var form = new ErrorForm();
                form.UpdateUIForError(e);
                //form.Show();
                Application.Run(form);
            }
        }
    }
}

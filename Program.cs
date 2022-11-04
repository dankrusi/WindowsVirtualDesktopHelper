using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper {
    class Program {

        [STAThread]
        public static void Main(string[] args) {
            Console.WriteLine("asdf");
            Application.EnableVisualStyles();
            Application.Run(new Form()); // or whatever
        }
    }
}

using System;
using System.Threading;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper {
	class Program {

		[STAThread]
		public static void Main(string[] args) {
			// Settings
			Settings.LoadDefaults();
			Settings.RegisterLaunchArgs(args);
			// Catch all exceptions in global exception handler
			try {
				// Load config
				Settings.LoadConfig();
				// Start app
				using (Mutex mutex = new Mutex(false, "Mutex/WindowsVirtualDesktopHelper")) {
					if (Settings.GetBool("debug.singleInstance") && !mutex.WaitOne(0)) {
						MessageBox.Show("WindowsVirtualDesktopHelper is already running.", "Error", MessageBoxButtons.OK);
						return;
					}

					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);
					var app = new App();
					Application.Run(app.AppForm);
				}
			} catch (Exception e) {
				// Global error handler
				Console.Error.WriteLine(e);
				var form = new ErrorForm();
				form.UpdateUIForError(e);
				Application.Run(form);
			}
		}
	}
}

using System;
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
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				var app = new App();
				Application.Run(app.SettingsForm);
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

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using WindowsVirtualDesktopHelper.WindowsHotKeyAPI;

namespace WindowsVirtualDesktopHelper {
	public partial class AppForm : Form {


		public AppForm() {
			// Init UI
			InitializeComponent();
		}

		

		#region Form Events

		private void AppForm_Load(object sender, EventArgs e) {
			App.Instance.ShowSplash();
			App.Instance.MonitorVDSwitch();
			App.Instance.MonitorFGWindowName();
			App.Instance.MonitorSystemThemeSwitch();
			App.Instance.MonitorVDisplayCount();

			App.Instance.UIUpdate();
		}

		private void AppForm_Shown(object sender, EventArgs e) {
			
		}

		private void AppForm_FormClosed(object sender, FormClosedEventArgs e) {

		}

		private void AppForm_FormClosing(object sender, FormClosingEventArgs e) {
			if(e.CloseReason == CloseReason.UserClosing) {
				e.Cancel = true;
				Hide();
			} else if(e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing) {
				// Remove all notif icons
				notifyIconName.Visible = false;
				notifyIconNumber.Visible = false;
				notifyIconPrev.Visible = false;
				notifyIconNext.Visible = false;
			}
		}

		#endregion

		#region Menu and Icon Tray Events

		private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
			if(e.ClickedItem.Tag.ToString() == "exit") App.Instance.Exit();
			else if(e.ClickedItem.Tag.ToString() == "settings") App.Instance.ShowSettings();
			else if(e.ClickedItem.Tag.ToString() == "about") App.Instance.ShowAbout();
			else if(e.ClickedItem.Tag.ToString() == "donate") App.Instance.OpenDonatePage();
		}

		private void notifyIconPrev_Click(object sender, EventArgs e) {
			App.Instance.SwitchDesktopBackward();
		}

		private void notifyIconNext_Click(object sender, EventArgs e) {
			App.Instance.SwitchDesktopForward();
		}

		private void notifyIconPrev_DoubleClick(object sender, EventArgs e) {
			//TODO: got to first desktop
		}

		private void notifyIconNext_DoubleClick(object sender, EventArgs e) {
			//TODO: go to last desktop
		}

		private void notifyIconName_MouseClick(object sender, MouseEventArgs e) {
			if(Settings.GetBool("feature.showDesktopNumberInIconTray.clickToOpenTaskView")) {
				if(e.Button == MouseButtons.Left) {
					// Already open?
					if(App.Instance.FGWindowHistory.Contains("Task View")) {
						// Do nothing
					} else {
						Util.OS.OpenTaskView();
					}
				}
			}
		}

		private void notifyIconNumber_MouseClick(object sender, MouseEventArgs e) {
			if (Settings.GetBool("feature.showDesktopNumberInIconTray.clickToOpenTaskView")) {
				if(e.Button == MouseButtons.Left) {
					// Already open?
					if(App.Instance.FGWindowHistory.Contains("Task View")) {
						// Do nothing
					} else {
						Util.OS.OpenTaskView();
					}
				}
			}
		}

		#endregion




	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper {
    public partial class ErrorForm : Form {
        public ErrorForm() {
            InitializeComponent();
        }

        public void UpdateUIForError(Exception e) {
            this.labelError.Text = e.Message;
            this.textBoxDetails.Text = e.Message;
            if(e.StackTrace != null) this.textBoxDetails.Text += "\r\n" + e.StackTrace.ToString();
            if (e.InnerException != null) this.textBoxDetails.Text += "\r\n\r\n" + e.InnerException.Message;
            if (e.InnerException != null && e.InnerException.StackTrace != null) this.textBoxDetails.Text += "\r\n" + e.InnerException.StackTrace.ToString();
            this.textBoxDetails.Text += "\r\n\r\n" + "Windows Build: "+GetWinBuildVersion();
        }

        private string GetWinBuildVersion() {
            try {
                var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                var currentBuildStr = (string)reg.GetValue("CurrentBuild");
                return currentBuildStr;
            }catch(Exception e) {
                return "Error getting build version: "+e.Message;
            }
        }

        private void buttonExit_Click(object sender, EventArgs e) {
            Application.Exit();
            System.Environment.Exit(1);
        }

        private void buttonOpenIssue_Click(object sender, EventArgs e) {
            var url = "https://github.com/dankrusi/WindowsVirtualDesktopHelper/issues/new";
            url += $"?title={Uri.EscapeDataString("Error: "+ this.labelError.Text)}";
            url += $"&body={Uri.EscapeDataString("\n\n"+ this.textBoxDetails.Text)}";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}

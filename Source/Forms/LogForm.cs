using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper.Forms {
	public partial class LogForm : Form {
		public LogForm() {
			InitializeComponent();
		}

		public void SetLogText(string text) {
			this.textBoxLog.Text = text.Replace("\n", "\r\n");
		}

	}
}

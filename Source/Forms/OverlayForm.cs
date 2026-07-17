using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper {
	public partial class OverlayForm : Form {

		public string LabelText;
		public string Position = "middlecenter";
		public int? ScreenNumber = null;
		public bool FadeIn = false;
		public bool Translucent = true;

		private int animationDirection = +1;
		private int animationOpacityCurrent = 0;
		private int animationOpacityTarget = 100;
		private int animationOpacityStep = 5;

		public static event EventHandler WillShowNotificationFormEvent;

		public OverlayForm(int? screenNumber = null) {
			InitializeComponent();

			this.ScreenNumber = screenNumber;
			this.FadeIn = Settings.GetBool("feature.showDesktopStatusOverlay.animate");
			this.Translucent = Settings.GetBool("feature.showDesktopStatusOverlay.translucent");
			this.Position = Settings.GetString("feature.showDesktopStatusOverlay.position");

			// Theme
			var theme = App.Instance.CurrentSystemThemeName;
			var fgColor = ColorTranslator.FromHtml(Settings.GetString("theme.status.overlayFG." + theme));
			var bgColor = ColorTranslator.FromHtml(Settings.GetString("theme.status.overlayBG." + theme));
			this.BackColor = bgColor;
			this.label1.ForeColor = fgColor;
			
			// Size
			this.Width = Settings.GetInt("theme.status.width");
			this.Height = Settings.GetInt("theme.status.height");

			// Font
			var font = new Font(Settings.GetString("theme.status.font"), Settings.GetFloat("theme.status.fontSize"), FontStyle.Regular);
			this.label1.Font = font;

			// Set position
			var positionOffset = Settings.GetInt("theme.status.offset");
			this.StartPosition = FormStartPosition.Manual;
			var screen = Screen.FromControl(this); // get main screen
			if(this.ScreenNumber != null) screen = Screen.AllScreens[this.ScreenNumber.Value]; 
			var screenW = screen.WorkingArea.Width;
			var screenH = screen.WorkingArea.Height;
			var screenX = screen.WorkingArea.X;
			var screenY = screen.WorkingArea.Y;
			if (this.Position == "topleft") {
				this.Location = new Point(screenX+positionOffset, screenY + positionOffset);
			} else if (this.Position == "topcenter") {
				this.Location = new Point(screenX + screenW / 2 - this.Width / 2, screenY + positionOffset);
			} else if (this.Position == "topright") {
				this.Location = new Point(screenX + screenW - this.Width - positionOffset, screenY + positionOffset);
			} else if (this.Position == "middleleft") {
				this.Location = new Point(screenX + positionOffset, screenY + screenH / 2 - this.Height / 2);
			} else if (this.Position == "middlecenter") {
				this.Location = new Point(screenX + screenW / 2 - this.Width / 2, screenY + screenH / 2 - this.Height / 2);
			} else if (this.Position == "middleright") {
				this.Location = new Point(screenX + screenW - this.Width - positionOffset, screenY + screenH / 2 - this.Height / 2);
			} else if (this.Position == "bottomleft") {
				this.Location = new Point(screenX + positionOffset, screenY + screenH - this.Height - positionOffset);
			} else if (this.Position == "bottomcenter") {
				this.Location = new Point(screenX + screenW / 2 - this.Width / 2, screenY + screenH - this.Height - positionOffset);
			} else if (this.Position == "bottomright") {
				this.Location = new Point(screenX + screenW - this.Width - positionOffset, screenY + screenH - this.Height - positionOffset);
			}

			
			// Register for signal to close self
			WillShowNotificationFormEvent += this.OnWillShowNotificationForm;

			// Set initial opacity
			if (this.FadeIn == true) {
				this.Opacity = 0;
			} else {
				if (this.Translucent) this.Opacity = 0.6;
				else this.Opacity = 1.0;
			}
		}

		public static void CloseAllNotifications(object sender) {
			// Send signal to close all others
			OverlayForm.WillShowNotificationFormEvent?.Invoke(sender, EventArgs.Empty);
		}

		protected virtual void OnWillShowNotificationForm(object sender, EventArgs e) {
			if (sender == this) return; // make sure we dont close ourselves

			// If another SwitchNotification is being shown, then close ourselves
			this.Close();
		}

		protected override void OnFormClosed(FormClosedEventArgs e) {
			// Unsubscribe from the static event, otherwise every overlay form ever shown stays
			// rooted by the event handler list and can never be garbage collected
			WillShowNotificationFormEvent -= this.OnWillShowNotificationForm;
			base.OnFormClosed(e);
		}

		// Preven from from being focusable
		// via https://stackoverflow.com/questions/2423234/make-a-form-not-focusable-in-c-sharp
		// via https://stackoverflow.com/questions/2798245/click-through-in-c-sharp-form
		private const int WS_EX_NOACTIVATE = 0x08000000;
		private const int WS_EX_LAYERED = 0x80000;
		private const int WS_EX_TRANSPARENT = 0x20;
		protected override CreateParams CreateParams {
			get {
				var createParams = base.CreateParams;

				createParams.ExStyle |= WS_EX_NOACTIVATE;
				createParams.ExStyle |= WS_EX_LAYERED;
				createParams.ExStyle |= WS_EX_TRANSPARENT;
				return createParams;
			}
		}

		private void OverlayForm_Shown(object sender, EventArgs e) {
			
			if (this.FadeIn) {
				this.animationOpacityCurrent = 0;
				this.animationOpacityTarget = 100;
				if (this.Translucent) this.animationOpacityTarget = 60;
				else this.animationOpacityTarget = 100;
				this.timerAnimate.Start();
			} else {
				
			}
		}

		private void OverlayForm_Load(object sender, EventArgs e) {
			this.label1.Text = LabelText;
		}


		private void timerAnimate_Tick(object sender, EventArgs e) {
			this.animationOpacityCurrent += (this.animationOpacityStep * this.animationDirection);
			if (
				(this.animationDirection == +1 && this.animationOpacityCurrent >= this.animationOpacityTarget)
				||
				(this.animationDirection == -1 && this.animationOpacityCurrent <= this.animationOpacityTarget)
				) {
				this.animationOpacityCurrent = this.animationOpacityTarget;
				this.timerAnimate.Enabled = false;
				if (this.animationDirection == +1) {
					
				} else {
					this.Close();
				}
			}
			this.Opacity = this.animationOpacityCurrent / 100.0;
		}
	}
}

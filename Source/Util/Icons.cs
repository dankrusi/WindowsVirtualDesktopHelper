using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper.Util {

	class Icons {

		// Icons are cached as ready-to-use Icon objects: the cache is bounded by the number of
		// distinct texts/sizes/themes/styles, and re-using the same Icon instance avoids creating
		// a new GDI icon handle on every tray icon update (which would leak, see DestroyIcon below)
		private static ConcurrentDictionary<string, Icon> _cache = new ConcurrentDictionary<string, Icon>();

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool DestroyIcon(IntPtr hIcon);

		public static Icon GenerateNotificationIcon(string text, string theme, int dpi, bool drawAsSymbol, double opacity = 1.0) {
			// Init
			var size = 16;
			if (dpi > 96) size = 64;
			var renderSize = 128; // GDI has really weak text drawing on transparent, so to get best results we render large then downscale...
			var textToRender = text;
			if (textToRender == null) textToRender = ""; // sanity
			var textToRenderInfo = new StringInfo(textToRender);
			if (textToRenderInfo.LengthInTextElements > 2) textToRender = new StringInfo(textToRender).SubstringByTextElements(0, 2);
			var textToRenderSizeRatio = 1.0f;
			if (textToRenderInfo.LengthInTextElements == 1) textToRenderSizeRatio = 0.9f;
			if (textToRenderInfo.LengthInTextElements == 2) textToRenderSizeRatio = 0.5f;
			if (textToRenderInfo.LengthInTextElements == 2) {
				textToRenderSizeRatio = 0.75f;
				//textStyle = FontStyle.Regular;
			}
			var automaticFontSizeFitTolerance = 0.2f;
			var offsetY = 0.0f;
			var fontFamily = Settings.GetFontName("theme.icons.font");
			var fontStyle = Settings.GetFontStyle("theme.icons.font");
			if(Util.Emoji.HasEmoji(textToRender)) {
				fontFamily = Settings.GetFontName("theme.icons.emojiFont");
				fontStyle = Settings.GetFontStyle("theme.icons.emojiFont");
			}
			if (drawAsSymbol) {
				fontFamily = Settings.GetFontName("theme.icons.symbolsFont");
				fontStyle = Settings.GetFontStyle("theme.icons.symbolsFont");
				textToRenderSizeRatio = 1.8f;
				if (dpi > 96) textToRenderSizeRatio = 1.0f;
				automaticFontSizeFitTolerance = 2.0f;
				offsetY = -0.4f;
			}
			var textSize = renderSize * textToRenderSizeRatio;

			// Cache hit?
			var cacheKey = textToRender + "_" + textSize + "_" + size + "_" + theme + "_" + fontStyle + "_" + opacity;
			Icon cachedIcon;
			if (_cache.TryGetValue(cacheKey, out cachedIcon)) {
				return cachedIcon;
			}

			// Theme
			var fgColor = ColorTranslator.FromHtml(Settings.GetString("theme.icons.iconFG." + theme));
			if (opacity != 1.0) fgColor = Color.FromArgb((int)(255.0f * opacity), fgColor);

			Icon icon;
			using (var bitmap = new Bitmap(renderSize, renderSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
			using (var bitmapScaledDown = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
				// Draw icon
				using (var g = Graphics.FromImage(bitmap))
				using (var fgBrush = new SolidBrush(fgColor))
				using (var format = new StringFormat()) {
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

					g.Clear(Color.Transparent);

					format.Alignment = StringAlignment.Center;
					format.LineAlignment = StringAlignment.Center;

					var rect = new Rectangle(
						-renderSize / 2, // x
						(int)(renderSize * offsetY), // y
						renderSize * 2, // w (note: we oversize the drawing area so that we never clip text to new lines...
						renderSize + (int)(-1 * renderSize * offsetY)); // h

					var font = new Font(fontFamily, textSize, fontStyle);
					try {
						// Automatic text fit
						var fontScaleDownFactor = 1.0f;
						var fontScaleDownAttempts = 0;
						while (fontScaleDownAttempts < 10 && fontScaleDownFactor > 0) {
							fontScaleDownAttempts++;
							fontScaleDownFactor -= 0.1f;
							var measure = g.MeasureString(textToRender, font);
							if (measure.Width > renderSize * (1.0f + automaticFontSizeFitTolerance)) {
								font.Dispose();
								font = new Font(fontFamily, textSize * fontScaleDownFactor, fontStyle);
							} else {
								break;
							}
						}
						g.DrawString(textToRender, font, fgBrush, rect, format);
						g.Flush();
					} finally {
						font.Dispose();
					}
				}

				// Scale down
				using (var g = Graphics.FromImage(bitmapScaledDown)) {
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					g.DrawImage(bitmap, 0, 0, size, size);
					g.Flush();
				}

				// Create the icon: GetHicon() allocates a native GDI icon handle which Icon.FromHandle
				// does not take ownership of - we clone to an Icon which owns its own handle and then
				// destroy the original handle, otherwise every icon update leaks a GDI handle until
				// the process hits the GDI object limit
				var hIcon = bitmapScaledDown.GetHicon();
				try {
					using (var tempIcon = Icon.FromHandle(hIcon)) {
						icon = (Icon)tempIcon.Clone();
					}
				} finally {
					DestroyIcon(hIcon);
				}
			}

			// Register in cache
			_cache[cacheKey] = icon;

			return icon;
		}

	}
}

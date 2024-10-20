﻿using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace WindowsVirtualDesktopHelper.Util {

    class Icons {

        private static ConcurrentDictionary<string, Bitmap> _cache = new ConcurrentDictionary<string,Bitmap>();

		public static Icon GenerateNotificationIcon(string text, string theme, int dpi, bool drawAsSymbol, FontStyle textStyle = FontStyle.Regular, double opacity = 1.0) {
			// Init
			var size = 16;
			if (dpi > 96) size = 64;
			var renderSize = 128; // GDI has really weak text drawing on transparent, so to get best results we render large then downscale...
			//var textStyle = FontStyle.Bold;
			var textToRender = text;
			if (textToRender == null) textToRender = ""; // sanity
			var textToRenderInfo = new StringInfo(textToRender);
			if (textToRenderInfo.LengthInTextElements > 2) textToRender = new StringInfo(textToRender).SubstringByTextElements(0, 2);
			var textToRenderSizeRatio = 1.0f;
			if (textToRenderInfo.LengthInTextElements == 1) textToRenderSizeRatio = 0.9f;
			if (textToRenderInfo.LengthInTextElements == 2) textToRenderSizeRatio = 0.5f;
			if (textToRenderInfo.LengthInTextElements == 2) {
				textToRenderSizeRatio = 0.75f;
				textStyle = FontStyle.Regular;
			}
			var automaticFontSizeFitTolerance = 0.2f;
			var offsetY = 0.0f;
			var fontFamily = Settings.GetString("theme.icons.font");
			if (Util.Emoji.HasEmoji(textToRender)) fontFamily = Settings.GetString("theme.icons.emojiFont");
			if (drawAsSymbol) {
				fontFamily = Settings.GetString("theme.icons.symbolsFont");
				textToRenderSizeRatio = 1.8f;
				if (dpi > 96) textToRenderSizeRatio = 1.0f;
				automaticFontSizeFitTolerance = 2.0f;
				offsetY = -0.4f;
			}
			var textSize = renderSize * textToRenderSizeRatio;

			// Cache hit?
			var cacheKey = textToRender + "_" + textSize + "_" + theme + "_" + textStyle + "_" + opacity;
			if (_cache.ContainsKey(cacheKey)) {
				var cachedBitmap = _cache[cacheKey];
				return Icon.FromHandle(cachedBitmap.GetHicon());
			}

			// Theme
            var fgColor = ColorTranslator.FromHtml(Settings.GetString("theme.icons.iconFG."+theme));
			var bgColor = ColorTranslator.FromHtml(Settings.GetString("theme.icons.iconBG." + theme));
			var fgBrush = new SolidBrush(fgColor);
			var bgBrush = new SolidBrush(bgColor);

            // Init drawing
            Bitmap bitmap = new Bitmap(renderSize, renderSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            // Draw icon
            {
                Graphics g = Graphics.FromImage(bitmap);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                g.Clear(Color.Transparent);


                var font = new Font(fontFamily, textSize, textStyle);

                var format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                var rect = new Rectangle(
					-renderSize/2, // x
					(int)(renderSize*offsetY), // y
					renderSize*2, // w (note: we oversize the drawing area so that we never clip text to new lines...
					renderSize+ (int)(-1*renderSize * offsetY)); // h

				// Automatic text fit
				var fontScaleDownFactor = 1.0f;
				var fontScaleDownAttempts = 0;
                while(fontScaleDownAttempts < 10 && fontScaleDownFactor > 0) {
					fontScaleDownAttempts++;
					fontScaleDownFactor -= 0.1f;
					var measure = g.MeasureString(textToRender, font);
                    if (measure.Width > renderSize*(1.0f+ automaticFontSizeFitTolerance)) {
                        //var scaleDownRatio = (float)((renderSize * 1.2) / measure.Width);
                        font = new Font(fontFamily, textSize * fontScaleDownFactor, FontStyle.Bold);
                    } else {
						break;
					}
                }
				if(opacity != 1.0) {
					fgColor = Color.FromArgb((int)(255.0f * opacity), fgColor);
					fgBrush = new SolidBrush(fgColor);
				}
				g.DrawString(textToRender, font, fgBrush, rect, format);
                g.Flush();
            }
            
            var bitmapScaledDown = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            {
                Graphics g = Graphics.FromImage(bitmapScaledDown);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, 0, 0, size, size);
                g.Flush();
            }

			// Register in cache
			_cache[cacheKey] = bitmapScaledDown;

			// Debug display
			if (false && textToRender == "11"){
                var form = new System.Windows.Forms.Form();
                form.Width = size;
                form.Height = size+30;
                form.BackColor = bgColor;
                form.BackgroundImage = bitmap;
                form.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
                form.Show();
            }

            return Icon.FromHandle(bitmapScaledDown.GetHicon());

            
        }

        
    }
}

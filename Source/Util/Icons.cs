using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsVirtualDesktopHelper.Util {
    class Icons {

        //private static Font _font = new Font("Segoe UI", 12.0f, FontStyle.Bold);

        private static ConcurrentDictionary<string, Bitmap> _cache = new ConcurrentDictionary<string,Bitmap>();

        public static Icon GenerateNotificationIcon(string text, string theme, int dpi, bool useSymbolsFont) {


            // Init
            var size = 16;
            if (dpi > 96) size = 64;
            var renderSize = 128; // GDI has really weak text drawing on transparent, so to get best results we render large then downscale...
            var textStyle = FontStyle.Bold;
            var textToRender = text;
            if (textToRender == null) textToRender = ""; // sanity
            if (textToRender.Length > 2) textToRender = textToRender.Substring(0, 2);
            var textToRenderSizeRatio = 1.0f;
            if (textToRender.Length == 1) textToRenderSizeRatio = 0.9f;
            if (textToRender.Length == 2) textToRenderSizeRatio = 0.5f;
            if (textToRender.Length == 2) {
                textToRenderSizeRatio = 0.75f;
                textStyle = FontStyle.Regular;
            }
            var automaticFontSizeFitTolerance = 0.2f;
            var offsetY = 0.0f;
            var fontFamily = "Segoe UI";
            if (useSymbolsFont) {
                fontFamily = "Segoe UI Symbol";
                textToRenderSizeRatio = 1.8f;
                if (dpi > 96) textToRenderSizeRatio = 1.0f;
                automaticFontSizeFitTolerance = 2.0f;
                offsetY = -0.4f;
            }
            var textSize = renderSize * textToRenderSizeRatio;

            // Cache hit?
            var cacheKey = textToRender + "_" + textSize + "_" + theme;
            if(_cache.ContainsKey(cacheKey)) {
                var cachedBitmap = _cache[cacheKey];
                return Icon.FromHandle(cachedBitmap.GetHicon());
            }

            // Theme
            var fgBrush = Brushes.White;
            var fgColor = Color.White;
            var bgBrush = Brushes.Black;
            var bgColor = Color.Black;
            if(theme == "dark") {
                fgBrush = Brushes.White;
                fgColor = Color.White;
                bgBrush = Brushes.Black;
                bgColor = Color.Black;
            } else {
                fgBrush = Brushes.Black;
                fgColor = Color.Black;
                bgBrush = Brushes.White;
                bgColor = Color.White;
            }

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

                var rect = new Rectangle(0, (int)(renderSize*offsetY), renderSize, renderSize+ (int)(-1*renderSize * offsetY));

                // Automatic text fit
                if(true){
                    var measure = g.MeasureString(textToRender, font);
                    if (measure.Width > renderSize*(1.0f+ automaticFontSizeFitTolerance)) {
                        var scaleDownRatio = (float)((renderSize * 1.2) / measure.Width);
                        
                        font = new Font(fontFamily, textSize * scaleDownRatio, FontStyle.Bold);
                    }
                }
                //g.Clear(Color.Pink);
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
            if (false){
                var form = new System.Windows.Forms.Form();
                form.Width = size;
                form.Height = size+30;
                form.BackColor = bgColor;
                form.BackgroundImage = bitmapScaledDown;
                form.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
                form.Show();
            }

            return Icon.FromHandle(bitmapScaledDown.GetHicon());

            
        }

        
    }
}

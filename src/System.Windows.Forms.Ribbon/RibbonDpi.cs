using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace System.Windows.Forms
{
    /// <summary>
    /// Scales the ribbon's layout metrics, which are written for 96 DPI, to the DPI of the display.
    /// </summary>
    /// <remarks>
    /// The ribbon is laid out once, for the system DPI (the application is system DPI aware), so a single
    /// process-wide factor is enough. In a process that is not DPI aware Windows reports 96 DPI and the factor is 1.
    /// </remarks>
    public static class RibbonDpi
    {
        private static float? _factor;

        /// <summary>
        /// Gets the ratio of the display's DPI to 96 DPI, e.g. 1.75 at 168 DPI.
        /// </summary>
        public static float Factor
        {
            get
            {
                if (_factor == null)
                {
                    try
                    {
                        using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                        {
                            _factor = Math.Max(1f, g.DpiX / 96f);
                        }
                    }
                    catch
                    {
                        _factor = 1f;
                    }
                }
                return _factor.Value;
            }
        }

        public static int Scale(int value) => (int)Math.Round(value * Factor);

        public static Size Scale(Size size) => new Size(Scale(size.Width), Scale(size.Height));

        public static Padding Scale(Padding padding) =>
            new Padding(Scale(padding.Left), Scale(padding.Top), Scale(padding.Right), Scale(padding.Bottom));

        /// <summary>
        /// Gets the size to draw an image designed for 96 DPI at.
        /// </summary>
        public static Size ScaleImage(Image image)
        {
            if (image == null)
                return Size.Empty;
            object marker;
            if (_prescaledImages.TryGetValue(image, out marker))
                return image.Size;
            return Scale(image.Size);
        }

        /// <summary>
        /// Marks an image as already drawn for the display's DPI, so <see cref="ScaleImage"/> does not enlarge it again.
        /// </summary>
        public static void MarkScaled(Image image)
        {
            if (image != null && !_prescaledImages.TryGetValue(image, out _))
                _prescaledImages.Add(image, null);
        }

        private static readonly ConditionalWeakTable<Image, object> _prescaledImages = new ConditionalWeakTable<Image, object>();

        /// <summary>
        /// Draws an image designed for 96 DPI into the given (already scaled) bounds. An enlarged image is resampled
        /// once with a high quality filter and cached, rather than being stretched on every paint.
        /// </summary>
        public static void DrawImage(Graphics g, Image image, Rectangle bounds)
        {
            if (image == null || bounds.Width <= 0 || bounds.Height <= 0)
                return;

            if (image.Width == bounds.Width && image.Height == bounds.Height)
            {
                g.DrawImage(image, bounds);
                return;
            }

            Bitmap scaled;
            if (!_scaledImages.TryGetValue(image, out scaled) || scaled.Size != bounds.Size)
            {
                scaled = Resample(image, bounds.Size);
                _scaledImages.Remove(image);
                _scaledImages.Add(image, scaled);
            }
            g.DrawImage(scaled, bounds);
        }

        private static readonly ConditionalWeakTable<Image, Bitmap> _scaledImages = new ConditionalWeakTable<Image, Bitmap>();

        private static readonly ConditionalWeakTable<Image, Image> _disabledImages = new ConditionalWeakTable<Image, Image>();

        /// <summary>
        /// Gets the greyed out version of an image, made once with <paramref name="create"/> and kept, so that it is
        /// not made again (and its enlarged copy resampled again) every time a disabled item is painted.
        /// </summary>
        public static Image GetDisabledImage(Image image, Func<Image, Image> create)
        {
            if (image == null)
                return null;
            return _disabledImages.GetValue(image, x => create(x));
        }

        /// <summary>
        /// Resizes an image with a high quality filter.
        /// </summary>
        public static Bitmap Resample(Image image, Size size)
        {
            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(result))
            using (var attributes = new ImageAttributes())
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                // Stops the edge pixels blending with transparent black outside the image.
                attributes.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(image, new Rectangle(Point.Empty, size), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return result;
        }
    }
}

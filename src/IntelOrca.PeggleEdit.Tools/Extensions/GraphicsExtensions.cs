using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace IntelOrca.PeggleEdit.Tools.Extensions
{
    public static class GraphicsExtensions
    {
        // Clamps (mirrors) sampling at the edges of an image, so that a scaled image does not fade into, or take colour
        // from, what is outside it
        private static readonly ImageAttributes ClampedEdges = CreateClampedEdges();

        private static ImageAttributes CreateClampedEdges()
        {
            var attributes = new ImageAttributes();
            attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
            return attributes;
        }

        /// <summary>
        /// Draws an image with its edges on whole device pixels, so that an object at a fraction of a pixel (e.g. while
        /// it moves) is not resampled, which blurs it and cuts off or bleeds its edges. Where the image is scaled (e.g. on
        /// a high DPI display), its edges are sampled clamped.
        /// </summary>
        public static void DrawImageSnapped(this Graphics g, Image image, RectangleF dst)
        {
            DrawImageSnapped(g, image, dst, new Rectangle(0, 0, image.Width, image.Height));
        }

        /// <inheritdoc cref="DrawImageSnapped(Graphics, Image, RectangleF)"/>
        public static void DrawImageSnapped(this Graphics g, Image image, float x, float y, float width, float height)
        {
            DrawImageSnapped(g, image, new RectangleF(x, y, width, height));
        }

        /// <inheritdoc cref="DrawImageSnapped(Graphics, Image, RectangleF)"/>
        public static void DrawImageSnapped(this Graphics g, Image image, RectangleF dst, Rectangle src)
        {
            // Zoomed (e.g. for a high DPI display), the image is scaled once to the size it is shown at and then copied
            // pixel for pixel, as scaling it every time it is drawn is slow. Zoomed so far that the scaled image would
            // be very large, only the part of it that is drawn is scaled, each time.
            float[] m;
            using (var transform = g.Transform)
                m = transform.Elements;
            if (m[1] == 0 && m[2] == 0 && m[0] > 0 && m[3] > 0 && (m[0] != 1 || m[3] != 1))
            {
                var left = (int)RoundToPixel((dst.Left * m[0]) + m[4]);
                var top = (int)RoundToPixel((dst.Top * m[3]) + m[5]);
                var width = (int)Math.Round(dst.Width * m[0]);
                var height = (int)Math.Round(dst.Height * m[3]);
                if (width <= 0 || height <= 0)
                    return;
                if ((long)width * height > MaxScaledPixels)
                {
                    DrawVisiblePartScaled(g, image, new Rectangle(left, top, width, height), src);
                    return;
                }
                var scaled = GetScaledImage(image, src, new Size(width, height));
                var state = g.Save();
                g.ResetTransform();
                g.DrawImageUnscaled(scaled, left, top);
                g.Restore(state);
                return;
            }

            dst = SnapToDevicePixels(g, dst);
            if (dst.Width == src.Width && dst.Height == src.Height && m[0] == 1 && m[3] == 1 && m[1] == 0 && m[2] == 0)
            {
                // Not scaled: copied pixel for pixel, without the slower drawing with image attributes
                g.DrawImage(image, new Rectangle((int)RoundToPixel(dst.X), (int)RoundToPixel(dst.Y), src.Width, src.Height), src, GraphicsUnit.Pixel);
                return;
            }
            var points = new[]
            {
                new PointF(dst.Left, dst.Top),
                new PointF(dst.Right, dst.Top),
                new PointF(dst.Left, dst.Bottom),
            };
            g.DrawImage(image, points, src, GraphicsUnit.Pixel, ClampedEdges);
        }

        /// <summary>
        /// Draws the part of an image that is visible, scaled to <paramref name="device"/> (in device pixels), as the
        /// scaled images that are kept are made.
        /// </summary>
        private static void DrawVisiblePartScaled(Graphics g, Image image, Rectangle device, Rectangle src)
        {
            var state = g.Save();
            try
            {
                g.ResetTransform();
                var visible = Rectangle.Intersect(device, Rectangle.Ceiling(g.VisibleClipBounds));
                if (visible.Width <= 0 || visible.Height <= 0)
                    return;
                // The part of the image under the visible pixels, with two of its pixels around it so that the edges are
                // sampled as when the whole image is drawn
                float sx = (float)src.Width / device.Width, sy = (float)src.Height / device.Height;
                visible.Inflate((int)Math.Ceiling(2 / sx) + 1, (int)Math.Ceiling(2 / sy) + 1);
                visible.Intersect(device);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.DrawImage(image, visible,
                    src.X + ((visible.X - device.X) * sx), src.Y + ((visible.Y - device.Y) * sy),
                    visible.Width * sx, visible.Height * sy, GraphicsUnit.Pixel, ClampedEdges);
            }
            finally
            {
                g.Restore(state);
            }
        }

        // The images scaled for drawing zoomed, by image, then by the part of it and the size it is scaled to
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Image, Dictionary<(Rectangle, Size), Bitmap>> _scaled =
            new System.Runtime.CompilerServices.ConditionalWeakTable<Image, Dictionary<(Rectangle, Size), Bitmap>>();

        // Each image's scaled images, so that they can all be let go of when there are too many
        private static readonly List<WeakReference<Dictionary<(Rectangle, Size), Bitmap>>> _allScaled =
            new List<WeakReference<Dictionary<(Rectangle, Size), Bitmap>>>();

        // The pixels in the scaled images kept, of all images
        private static long _scaledPixels;

        private static readonly object _scaledLock = new object();

        // The most sizes kept of one image (e.g. a peg drawn at many sizes), after which they are made again
        private const int MaxScaledSizes = 64;

        // The most pixels an image is scaled to and kept (e.g. 2800 x 2100, a background zoomed 350%), so that zooming
        // far in does not use a lot of memory
        private const long MaxScaledPixels = 8 * 1024 * 1024;

        // The most pixels kept in scaled images of all images (about 100 MB), after which they are all made again
        private const long MaxAllScaledPixels = 24 * 1024 * 1024;

        private static Bitmap GetScaledImage(Image image, Rectangle src, Size size)
        {
            lock (_scaledLock)
            {
                if (!_scaled.TryGetValue(image, out var sizes))
                {
                    sizes = new Dictionary<(Rectangle, Size), Bitmap>();
                    _scaled.Add(image, sizes);
                    _allScaled.RemoveAll(r => !r.TryGetTarget(out _));
                    _allScaled.Add(new WeakReference<Dictionary<(Rectangle, Size), Bitmap>>(sizes));
                }
                if (sizes.TryGetValue((src, size), out var result))
                    return result;

                long pixels = (long)size.Width * size.Height;
                if (sizes.Count >= MaxScaledSizes)
                    Clear(sizes);
                if (_scaledPixels + pixels > MaxAllScaledPixels)
                {
                    foreach (var reference in _allScaled)
                    {
                        if (reference.TryGetTarget(out var other))
                            Clear(other);
                    }
                    // Images no longer used may have had scaled images that are not counted any more
                    _scaledPixels = 0;
                }

                result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(result))
                {
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.DrawImage(image, new Rectangle(Point.Empty, size), src.X, src.Y, src.Width, src.Height, GraphicsUnit.Pixel, ClampedEdges);
                }
                sizes[(src, size)] = result;
                _scaledPixels += pixels;
                return result;
            }
        }

        private static void Clear(Dictionary<(Rectangle, Size), Bitmap> sizes)
        {
            foreach (var bitmap in sizes.Values)
            {
                _scaledPixels -= (long)bitmap.Width * bitmap.Height;
                bitmap.Dispose();
            }
            sizes.Clear();
            _scaledPixels = Math.Max(0, _scaledPixels);
        }

        /// <summary>
        /// Moves a rectangle so that its edges fall on whole device pixels, keeping its size in device pixels. A rotated
        /// or skewed transform is left alone.
        /// </summary>
        private static RectangleF SnapToDevicePixels(Graphics g, RectangleF rect)
        {
            float[] m;
            using (var transform = g.Transform)
                m = transform.Elements;
            float sx = m[0], sy = m[3], ox = m[4], oy = m[5];
            if (m[1] != 0 || m[2] != 0 || sx <= 0 || sy <= 0)
                return rect;

            var left = (float)RoundToPixel((rect.Left * sx) + ox);
            var top = (float)RoundToPixel((rect.Top * sy) + oy);
            var width = (float)Math.Round(rect.Width * sx);
            var height = (float)Math.Round(rect.Height * sy);
            return new RectangleF((left - ox) / sx, (top - oy) / sy, width / sx, height / sy);
        }

        /// <summary>
        /// Rounds a position on the device to the nearest pixel, a half always up. Math.Round rounds a half to the even
        /// pixel, which is not the same for a part of the editor drawn on its own (into a buffer moved by a whole number
        /// of pixels), so an object exactly between two pixels moved by a pixel each time it was drawn that way.
        /// </summary>
        public static double RoundToPixel(double value) => Math.Floor(value + 0.5);

        public static void DrawRectangle(this Graphics g, Pen pen, RectangleF rect)
        {
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static void DrawSquare(this Graphics g, Pen outline, Brush fill, PointF pos, int size)
        {
            var halfSize = size / 2;
            g.FillRectangle(fill, pos.X - halfSize, pos.Y - halfSize, size, size);
            g.DrawRectangle(outline, pos.X - halfSize, pos.Y - halfSize, size, size);
        }

        public static void DrawCircle(this Graphics g, Pen outline, Brush fill, PointF pos, int size)
        {
            var halfSize = size / 2;
            g.FillEllipse(fill, pos.X - halfSize, pos.Y - halfSize, size, size);
            g.DrawEllipse(outline, pos.X - halfSize, pos.Y - halfSize, size, size);
        }

        public static void DrawImageWithColour(this Graphics g, Image image, RectangleF dstF, Color color)
        {
            g.DrawImageSnapped(GetTintedImage(image, color), dstF);
        }

        public static void DrawImageWithColour(this Graphics g, Image image, Rectangle dst, Color color)
        {
            var tinted = GetTintedImage(image, color);
            g.DrawImage(tinted, dst, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
        }

        [ThreadStatic]
        private static Dictionary<(Image, int), Bitmap> _tinted;

        private static Bitmap GetTintedImage(Image image, Color color)
        {
            var cache = _tinted ?? (_tinted = new Dictionary<(Image, int), Bitmap>());
            var key = (image, color.ToArgb());
            if (!cache.TryGetValue(key, out var result))
            {
                result = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(result))
                {
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, GetImageAttributes(color));
                }
                cache.Add(key, result);
            }
            return result;
        }

        // The attributes made for each colour, kept as making them for every image drawn was slow (and they were not
        // disposed). One set for each thread, as GDI+ objects can not be used by two threads at once.
        [ThreadStatic]
        private static Dictionary<int, ImageAttributes> _imageAttributes;

        private static ImageAttributes GetImageAttributes(Color color)
        {
            var cache = _imageAttributes ?? (_imageAttributes = new Dictionary<int, ImageAttributes>());
            if (!cache.TryGetValue(color.ToArgb(), out var result))
            {
                result = CreateImageAttributes(color);
                cache.Add(color.ToArgb(), result);
            }
            return result;
        }

        private static ImageAttributes CreateImageAttributes(Color color)
        {
            var r = color.R / 255.0f;
            var g = color.G / 255.0f;
            var b = color.B / 255.0f;
            var a = color.A / 255.0f;

            var matrixItems = new float[][] {
                new float[] { r, 0, 0, 0, 0 },
                new float[] { 0, g, 0, 0, 0 },
                new float[] { 0, 0, b, 0, 0 },
                new float[] { 0, 0, 0, a, 0 },
                new float[] { 0, 0, 0, 0, 1  } };
            var colorMatrix = new ColorMatrix(matrixItems);
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorMatrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);
            return imageAttributes;
        }
    }
}

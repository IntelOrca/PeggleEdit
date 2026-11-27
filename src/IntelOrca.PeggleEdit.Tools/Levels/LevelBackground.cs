using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// Where the game shows a level's background, and making one from an image.
    /// </summary>
    /// <remarks>
    /// The game never scales a background. One of 800 x 600 is the whole screen. One of 646 x 543, the size of the
    /// backgrounds in Peggle, is drawn at (77, 52), which just covers the opening in the frame. Any other size is centred
    /// on the screen, so one smaller than 644 x 582 leaves black showing around it. Found by trying each size in Peggle
    /// and Peggle Nights.
    /// </remarks>
    public static class LevelBackground
    {
        /// <summary>
        /// The game's screen, which a background of this size fills.
        /// </summary>
        public static readonly Size ScreenSize = new Size(800, 600);

        /// <summary>
        /// Where the game draws a background the size of those in Peggle.
        /// </summary>
        public static readonly Rectangle PeggleBounds = new Rectangle(77, 52, 646, 543);

        /// <summary>
        /// The part of the screen that the frame does not cover, which the background shows through.
        /// </summary>
        public static readonly Rectangle FrameOpening = new Rectangle(79, 52, 643, 539);

        /// <summary>
        /// The JPEG quality used when none is given.
        /// </summary>
        public const int DefaultJpegQuality = 90;

        /// <summary>
        /// Gets where on the screen the game draws a background of the given size.
        /// </summary>
        public static Point GetLocation(Size size)
        {
            if (size == PeggleBounds.Size)
                return PeggleBounds.Location;
            return new Point((ScreenSize.Width - size.Width) / 2, (ScreenSize.Height - size.Height) / 2);
        }

        /// <summary>
        /// Gets where on the screen the game draws a background of the given size.
        /// </summary>
        public static Rectangle GetBounds(Size size) => new Rectangle(GetLocation(size), size);

        /// <summary>
        /// Gets whether an image at the given place on the screen covers the opening in the frame, so that no black shows
        /// around it in the game.
        /// </summary>
        public static bool CoversFrameOpening(RectangleF bounds)
        {
            // Allows for the rounding of a scaled image's edges
            const float tolerance = 0.01f;
            return bounds.Left <= FrameOpening.Left + tolerance && bounds.Top <= FrameOpening.Top + tolerance &&
                bounds.Right >= FrameOpening.Right - tolerance && bounds.Bottom >= FrameOpening.Bottom - tolerance;
        }

        /// <summary>
        /// Gets where an image of the given size goes to fill an area: scaled to cover it, keeping its shape, and centred
        /// on it, so that what is left over is cut off equally from each side.
        /// </summary>
        public static RectangleF Fill(Size image, Rectangle area)
        {
            var scale = Math.Max((float)area.Width / image.Width, (float)area.Height / image.Height);
            var width = image.Width * scale;
            var height = image.Height * scale;
            return new RectangleF(area.X + ((area.Width - width) / 2), area.Y + ((area.Height - height) / 2), width, height);
        }

        /// <summary>
        /// Gets where an image of the given size goes to fit in an area: scaled to show all of it, keeping its shape, and
        /// centred on it, so that what it does not cover is left equally at each side.
        /// </summary>
        public static RectangleF Contain(Size image, Rectangle area)
        {
            var scale = Math.Min((float)area.Width / image.Width, (float)area.Height / image.Height);
            var width = image.Width * scale;
            var height = image.Height * scale;
            return new RectangleF(area.X + ((area.Width - width) / 2), area.Y + ((area.Height - height) / 2), width, height);
        }

        /// <summary>
        /// Draws an image, scaled to the given place, onto a new bitmap of the given size. What the image does not cover
        /// is black, as the game shows it.
        /// </summary>
        public static Bitmap Draw(Image image, Size size, RectangleF bounds)
        {
            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
            try
            {
                using (var g = Graphics.FromImage(result))
                using (var attributes = new ImageAttributes())
                {
                    g.Clear(Color.Black);
                    if (IsUnscaled(image.Size, bounds))
                    {
                        // Copied pixel for pixel, as scaling it would blur it however little it moved
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                        g.DrawImage(image, Rectangle.Round(bounds), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                        return result;
                    }
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    // Stops the edges fading where the scaling reads past the image
                    attributes.SetWrapMode(WrapMode.TileFlipXY);
                    var points = new[]
                    {
                        bounds.Location,
                        new PointF(bounds.Right, bounds.Top),
                        new PointF(bounds.Left, bounds.Bottom),
                    };
                    g.DrawImage(image, points, new RectangleF(0, 0, image.Width, image.Height), GraphicsUnit.Pixel, attributes);
                }
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets whether an image at the given place is at its own size, on whole pixels, so that each of its pixels is one
        /// of the screen's.
        /// </summary>
        public static bool IsUnscaled(Size image, RectangleF bounds)
        {
            const float tolerance = 0.001f;
            return Math.Abs(bounds.Width - image.Width) < tolerance && Math.Abs(bounds.Height - image.Height) < tolerance &&
                Math.Abs(bounds.X - (float)Math.Round(bounds.X)) < tolerance &&
                Math.Abs(bounds.Y - (float)Math.Round(bounds.Y)) < tolerance;
        }

        /// <summary>
        /// Saves an image as a PNG, or as a JPEG of the given quality (1 to 100). Any transparency is shown over black, as
        /// the game shows it.
        /// </summary>
        public static byte[] Encode(Image image, bool jpeg, int quality = DefaultJpegQuality)
        {
            using (var opaque = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb))
            using (var ms = new MemoryStream())
            {
                using (var g = Graphics.FromImage(opaque))
                {
                    g.Clear(Color.Black);
                    g.CompositingMode = CompositingMode.SourceOver;
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                }
                if (jpeg)
                {
                    var codec = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
                    using (var parameters = new EncoderParameters(1))
                    {
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)Math.Max(1, Math.Min(100, quality)));
                        opaque.Save(ms, codec, parameters);
                    }
                }
                else
                {
                    opaque.Save(ms, ImageFormat.Png);
                }
                return ms.ToArray();
            }
        }
    }
}

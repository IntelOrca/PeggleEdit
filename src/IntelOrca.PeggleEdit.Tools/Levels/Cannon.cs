// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2026. http://tedtycoon.co.uk
//
// PeggleEdit is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeggleEdit is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeggleEdit. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// The ball launcher at the top of the board, as Peggle Nights draws it: after the draw sort objects and before
    /// the Foreground2 ones.
    /// </summary>
    public static class Cannon
    {
        /// <summary>
        /// Where the cannon turns, in level coordinates: screen (400, 70), measured against the game to the pixel.
        /// </summary>
        public static readonly PointF Pivot = new PointF(325, 30);

        /// <summary>
        /// The angle the cannon points at the start of a level: straight down. Angles are in radians,
        /// anticlockwise from pointing right, as the game's.
        /// </summary>
        public const float RestAngle = (float)(-Math.PI / 2);

        // The middle of the image is this far from the pivot, along where it points
        const float ImageDistance = 72;

        // The image is the skin's seams (82x14) with the shooter (63x64) over them, 10 across and 3 down
        const int ImageWidth = 82;
        const int ImageHeight = 64;

        // The gun's collision is a triangle along where it points: its tip 90 from the pivot, its base 60 from the
        // pivot and 20 wide
        const float CollisionBase = 60;
        const float CollisionTip = 90;
        const float CollisionHalfWidth = 10;

        /// <summary>
        /// The area the cannon can be drawn in at any angle, in level coordinates.
        /// </summary>
        public static RectangleF Bounds
        {
            get
            {
                float r = ImageDistance + (float)Math.Sqrt((ImageWidth * ImageWidth / 4.0) + (ImageHeight * ImageHeight / 4.0)) + 2;
                return new RectangleF(Pivot.X - r, Pivot.Y - r, r * 2, r * 2);
            }
        }

        // The cannon can not point within this of straight up
        const double UpwardsLimit = 83 * Math.PI / 180;

        /// <summary>
        /// Gets the angle the cannon points to aim at a point, as the game turns it towards the mouse, or
        /// straight down with no point. Like the game, it does not point upwards: see <see cref="LimitAngle"/>.
        /// </summary>
        public static float GetAngle(PointF? target)
        {
            if (!(target is PointF t))
                return RestAngle;
            float dx = t.X - Pivot.X;
            float dy = Pivot.Y - t.Y;
            if (dx == 0 && dy == 0)
                return RestAngle;
            return LimitAngle((float)Math.Atan2(dy, dx));
        }

        /// <summary>
        /// Limits an angle to where the cannon can point, as the game does whenever it sets the cannon's angle: an
        /// angle within 83 degrees of straight up, i.e. between 7 and 173 degrees, is moved to the
        /// nearer of them, or to 7 degrees (pointing right) when it is straight up. So the cannon points at most 97
        /// degrees either side of straight down. The angle returned is between -pi and pi.
        /// </summary>
        public static float LimitAngle(float angle)
        {
            double a = angle % (2 * Math.PI);
            if (a < 0)
                a += 2 * Math.PI;
            const double up = Math.PI / 2;
            if (a > up - UpwardsLimit && a < up + UpwardsLimit)
            {
                // Compared with the middle as a float, as the game does, so that straight up points right
                a = (float)a <= (float)up ? up - UpwardsLimit : up + UpwardsLimit;
            }
            return (float)(a > Math.PI ? a - (2 * Math.PI) : a);
        }

        /// <summary>
        /// Draws the cannon pointing at an angle. <paramref name="g"/> is in level coordinates.
        /// </summary>
        public static void Draw(Graphics g, float angle)
        {
            var image = GetImage();
            if (image == null)
                return;

            var backup = g.Save();
            g.TranslateTransform(Pivot.X, Pivot.Y);
            // The image hangs down from the pivot when the angle is straight down; GDI+ turns clockwise
            g.RotateTransform(-(float)((angle - RestAngle) * 180 / Math.PI));
            g.TranslateTransform(0, ImageDistance);
            g.DrawImage(image, -ImageWidth / 2.0f, -ImageHeight / 2.0f, ImageWidth, ImageHeight);
            g.Restore(backup);
        }

        /// <summary>
        /// Gets the triangle the ball collides with at the end of the cannon, in level coordinates.
        /// </summary>
        public static PointF[] GetCollisionShape(float angle)
        {
            float cos = (float)Math.Cos(angle), sin = (float)Math.Sin(angle);
            PointF At(float along, float across) => new PointF(
                Pivot.X + (along * cos) + (across * sin),
                Pivot.Y - (along * sin) + (across * cos));
            return new[]
            {
                At(CollisionTip, 0),
                At(CollisionBase, -CollisionHalfWidth),
                At(CollisionBase, CollisionHalfWidth),
            };
        }

        private static Bitmap _image;
        private static bool _imageLoaded;

        /// <summary>
        /// The cannon's image, made from the game's files (embedded, see <see cref="GameFiles"/>) as the game makes it.
        /// </summary>
        private static Bitmap GetImage()
        {
            if (!_imageLoaded)
            {
                _imageLoaded = true;
                try
                {
                    _image = MakeImage();
                }
                catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is ExternalException)
                {
                }
            }
            return _image;
        }

        private static Bitmap MakeImage()
        {
            var shooter = LevelPack.GetEmbeddedImage(@"images\ip\shooter")?.Image;
            if (shooter == null)
                return null;

            var result = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                using (var seams = GetSeams())
                {
                    if (seams != null)
                        g.DrawImage(seams, new Rectangle(0, 0, ImageWidth, 14), new Rectangle(0, 0, ImageWidth, 14), GraphicsUnit.Pixel);
                }
                // The first of the shooter's three frames: the cannon at rest
                int frameHeight = shooter.Height / 3;
                g.DrawImage(shooter, new Rectangle(10, 3, shooter.Width, frameHeight), new Rectangle(0, 0, shooter.Width, frameHeight), GraphicsUnit.Pixel);
            }
            return result;
        }

        /// <summary>
        /// Gets the seams of the first skin, the blue of the interface: the seams image has a skin in each 14 high row,
        /// and its alpha image is one row, used for each (an alpha grid).
        /// </summary>
        private static Bitmap GetSeams()
        {
            var colourData = GameFiles.Get(@"images\ip\seams.gif");
            var alphaData = GameFiles.Get(@"images\ip\seams_.gif");
            if (colourData == null || alphaData == null)
                return null;

            using (var colourStream = new MemoryStream(colourData))
            using (var alphaStream = new MemoryStream(alphaData))
            using (var colour = new Bitmap(colourStream))
            using (var alpha = new Bitmap(alphaStream))
            {
                var result = new Bitmap(ImageWidth, 14, PixelFormat.Format32bppArgb);
                for (int y = 0; y < result.Height && y < alpha.Height && y < colour.Height; y++)
                {
                    for (int x = 0; x < result.Width && x < alpha.Width && x < colour.Width; x++)
                    {
                        var c = colour.GetPixel(x, y);
                        result.SetPixel(x, y, Color.FromArgb(alpha.GetPixel(x, y).R, c.R, c.G, c.B));
                    }
                }
                return result;
            }
        }
    }
}

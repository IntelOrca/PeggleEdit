using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using IntelOrca.PeggleEdit.Tools.Properties;

namespace IntelOrca.PeggleEdit.Tools
{
    internal static class BrickImage
    {
        private static bool _initialised;
        private static Bitmap[] _brickTextures;
        private static Bitmap[] _brickTexturesFlip;
        private static Dictionary<BrickData, Bitmap> _cachedBitmaps = new Dictionary<BrickData, Bitmap>();

        private static void Initialise()
        {
            if (_initialised)
                return;

            _brickTextures = new Bitmap[8];
            _brickTexturesFlip = new Bitmap[8];
            for (var i = 0; i < _brickTextures.Length; i++)
            {
                _brickTexturesFlip[i] = SliceBitmap(Resources.brick, new Rectangle(0, i * 20, 32, 20), false);
                _brickTextures[i] = SliceBitmap(Resources.brick, new Rectangle(0, i * 20, 32, 20), true);
            }

            _initialised = true;
        }

        private static Bitmap SliceBitmap(Bitmap src, Rectangle srcRect, bool flipY)
        {
            var result = new Bitmap(srcRect.Width, srcRect.Height);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(src, 0, 0, srcRect, GraphicsUnit.Pixel);
            }
            if (flipY)
                result.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return result;
        }

        // Where the game draws bricks compared to where PeggleEdit would, measured against screenshots of 725 bricks
        // in Peggle Nights (hardware rendering). Straight bricks: x along the brick, y across it. Curved bricks: x away
        // from the centre of the curve, y along it. The screen offset is after rotating.
        internal static PointF StraightOffset = new PointF(-0.75f, 0.44f);
        internal static PointF CurvedOffset = new PointF(0.23f, -0.63f);
        internal static PointF ScreenOffset = new PointF(-0.7f, -0.7f);

        /// <summary>
        /// Draws a textured brick centred on <paramref name="location"/> with the current transform of
        /// <paramref name="g"/>, which includes the brick's rotation. <paramref name="drawn"/> keeps how the brick was
        /// last drawn, so that a brick drawn the same way again (as each one that is not moving is, every frame of the
        /// preview) is copied pixel for pixel rather than drawn rotated and scaled again, which is slow.
        /// </summary>
        public static void Draw(Graphics g, in BrickData brick, PointF location, ref DrawnBrick drawn)
        {
            var image = GetBrickImage(brick);
            float x = location.X - (image.Width / 2);
            float y = location.Y - (image.Height / 2);

            float[] m;
            using (var transform = g.Transform)
                m = transform.Elements;
            // Where the image goes on the device, with the screen offset after the transform
            float dx = m[4] + ScreenOffset.X;
            float dy = m[5] + ScreenOffset.Y;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var (cx, cy) in new[] { (x, y), (x + image.Width, y), (x, y + image.Height), (x + image.Width, y + image.Height) })
            {
                float px = (cx * m[0]) + (cy * m[2]) + dx;
                float py = (cx * m[1]) + (cy * m[3]) + dy;
                minX = Math.Min(minX, px);
                minY = Math.Min(minY, py);
                maxX = Math.Max(maxX, px);
                maxY = Math.Max(maxY, py);
            }

            // Drawn as it is when it is too large (or the transform not usable) to keep
            bool keepable = !float.IsNaN(minX) && !float.IsNaN(minY) && maxX - minX < 4096 && maxY - minY < 4096 &&
                Math.Abs(minX) < 1e6 && Math.Abs(minY) < 1e6;
            int left = keepable ? (int)Math.Floor(minX) - 1 : 0;
            int top = keepable ? (int)Math.Floor(minY) - 1 : 0;

            // The same image, transform and position of the image within a device pixel draw the same pixels
            float originX = (x * m[0]) + (y * m[2]) + dx;
            float originY = (x * m[1]) + (y * m[3]) + dy;
            var key = new DrawnBrick.DrawKey(image, m, originX - left, originY - top, g.InterpolationMode, g.PixelOffsetMode, g.CompositingQuality);
            if (keepable && drawn != null && drawn.Key.Equals(key))
            {
                var size = new Size((int)Math.Ceiling(maxX) + 2 - left, (int)Math.Ceiling(maxY) + 2 - top);
                if (drawn.Image == null || drawn.Image.Size != size)
                {
                    // Drawn the same way twice running, so it is not moving: kept drawn
                    drawn.Image?.Dispose();
                    var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
                    using (var bg = Graphics.FromImage(result))
                    using (var transform = new Matrix(m[0], m[1], m[2], m[3], dx - left, dy - top))
                    {
                        bg.InterpolationMode = g.InterpolationMode;
                        bg.PixelOffsetMode = g.PixelOffsetMode;
                        bg.CompositingQuality = g.CompositingQuality;
                        bg.Transform = transform;
                        bg.DrawImage(image, x, y);
                    }
                    drawn.Image = result;
                }
                var state = g.Save();
                g.ResetTransform();
                g.DrawImage(drawn.Image, new Rectangle(left, top, size.Width, size.Height), 0, 0, size.Width, size.Height, GraphicsUnit.Pixel);
                g.Restore(state);
                return;
            }

            // Drawn for the first time, or moved: drawn as it is
            if (drawn == null)
                drawn = new DrawnBrick();
            drawn.Image?.Dispose();
            drawn.Image = null;
            drawn.Key = key;
            var backup = g.Transform;
            g.TranslateTransform(ScreenOffset.X, ScreenOffset.Y, MatrixOrder.Append);
            g.DrawImage(image, x, y);
            g.Transform = backup;
            backup.Dispose();
        }

        /// <summary>
        /// How a brick was last drawn, and the brick drawn that way once it has been drawn the same way twice.
        /// </summary>
        internal sealed class DrawnBrick
        {
            public DrawKey Key;
            public Bitmap Image;

            internal readonly struct DrawKey : IEquatable<DrawKey>
            {
                private readonly Bitmap _image;
                private readonly float _m11, _m12, _m21, _m22, _x, _y;
                private readonly InterpolationMode _interpolation;
                private readonly PixelOffsetMode _pixelOffset;
                private readonly CompositingQuality _compositing;

                public DrawKey(Bitmap image, float[] m, float x, float y, InterpolationMode interpolation,
                    PixelOffsetMode pixelOffset, CompositingQuality compositing)
                {
                    _image = image;
                    _m11 = m[0];
                    _m12 = m[1];
                    _m21 = m[2];
                    _m22 = m[3];
                    _x = x;
                    _y = y;
                    _interpolation = interpolation;
                    _pixelOffset = pixelOffset;
                    _compositing = compositing;
                }

                public bool Equals(DrawKey other) =>
                    _image != null && _image == other._image &&
                    _m11 == other._m11 && _m12 == other._m12 && _m21 == other._m21 && _m22 == other._m22 &&
                    _x == other._x && _y == other._y &&
                    _interpolation == other._interpolation && _pixelOffset == other._pixelOffset &&
                    _compositing == other._compositing;

                public override bool Equals(object obj) => obj is DrawKey other && Equals(other);

                public override int GetHashCode() => _x.GetHashCode() ^ (_y.GetHashCode() * 31);
            }
        }

        private const int MaxCachedBitmaps = 256;

        public static Bitmap GetBrickImage(in BrickData brick)
        {
            Initialise();
            if (!_cachedBitmaps.TryGetValue(brick, out var result))
            {
                // Every size and angle of a brick is kept (e.g. each step while one is drawn out), so start again
                // when there are many
                if (_cachedBitmaps.Count >= MaxCachedBitmaps)
                {
                    foreach (var bitmap in _cachedBitmaps.Values)
                        bitmap.Dispose();
                    _cachedBitmaps.Clear();
                }
                result = CreateBrickBitmap(brick);
                _cachedBitmaps.Add(brick, result);
            }
            return result;
        }

        public static Bitmap GetBrickTexture(in BrickData brick)
        {
            Initialise();

            var index = 0;
            if (brick.CanBeOrange)
                index++;
            if (brick.QuickDisappear)
                index += 4;
            return brick.TextureFlip ? _brickTexturesFlip[index] : _brickTextures[index];
        }

        private static Bitmap CreateBrickBitmap(in BrickData brickData)
        {
            CalculateBrickVertices(brickData, out var vertices, out var texCoords);

            // Large enough for the brick, centred (so each side is even), drawn at twice the size and then scaled down
            var extentX = 0.0f;
            var extentY = 0.0f;
            foreach (var v in vertices)
            {
                extentX = Math.Max(extentX, Math.Abs(v.X));
                extentY = Math.Max(extentY, Math.Abs(v.Y));
            }
            // A brick with no size (or one that is not a number) is still drawn, as nothing
            if (!(extentX < 1024))
                extentX = float.IsNaN(extentX) ? 0 : 1024;
            if (!(extentY < 1024))
                extentY = float.IsNaN(extentY) ? 0 : 1024;
            var size = new Size(
                Math.Min(2048, 2 * (int)Math.Ceiling(extentX + 2)),
                Math.Min(2048, 2 * (int)Math.Ceiling(extentY + 2)));
            const int Scale = 2;
            var largeSize = new Size(size.Width * Scale, size.Height * Scale);

            var texture = GetTexturePixels(GetBrickTexture(brickData));
            var pixels = new int[largeSize.Width * largeSize.Height];
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new PointF((largeSize.Width / 2) + (vertices[i].X * Scale), (largeSize.Height / 2) + (vertices[i].Y * Scale));
            var numTriangles = vertices.Length / 3;
            for (int i = 0; i < numTriangles; i++)
            {
                FillTriangle(texture, pixels, largeSize,
                    new ReadOnlySpan<PointF>(vertices, i * 3, 3),
                    new ReadOnlySpan<PointF>(texCoords, i * 3, 3));
            }

            using (var bitmapLarge = new Bitmap(largeSize.Width, largeSize.Height, PixelFormat.Format32bppArgb))
            {
                var data = bitmapLarge.LockBits(new Rectangle(Point.Empty, largeSize), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < largeSize.Height; y++)
                        Marshal.Copy(pixels, y * largeSize.Width, data.Scan0 + (y * data.Stride), largeSize.Width);
                }
                finally
                {
                    bitmapLarge.UnlockBits(data);
                }

                var bitmapSmall = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(bitmapSmall))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.DrawImage(bitmapLarge, 0, 0, size.Width, size.Height);
                }
                return bitmapSmall;
            }
        }

        /// <summary>
        /// A texture's pixels (ARGB, not premultiplied) with its size.
        /// </summary>
        private readonly struct TexturePixels
        {
            public readonly int[] Pixels;
            public readonly int Width;
            public readonly int Height;

            public TexturePixels(int[] pixels, int width, int height)
            {
                Pixels = pixels;
                Width = width;
                Height = height;
            }
        }

        private static readonly Dictionary<Bitmap, TexturePixels> _texturePixels = new Dictionary<Bitmap, TexturePixels>();

        private static TexturePixels GetTexturePixels(Bitmap texture)
        {
            if (!_texturePixels.TryGetValue(texture, out var result))
            {
                var pixels = new int[texture.Width * texture.Height];
                var data = texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < texture.Height; y++)
                        Marshal.Copy(data.Scan0 + (y * data.Stride), pixels, y * texture.Width, texture.Width);
                }
                finally
                {
                    texture.UnlockBits(data);
                }
                result = new TexturePixels(pixels, texture.Width, texture.Height);
                _texturePixels.Add(texture, result);
            }
            return result;
        }

        private static int GetTexture(in TexturePixels texture, float u, float v)
        {
            int width = texture.Width;
            int height = texture.Height;

            // Not a number (from a triangle with no area) is the edge
            if (!(u >= 0)) u = 0;
            if (!(v >= 0)) v = 0;
            if (u > 1) u = 1;
            if (v > 1) v = 1;

            // The edge texels are centred on the edges of the brick (not inside it as in Direct3D), as in the game
            var x = u * (width - 1);
            var y = v * (height - 1);

            // Get the integer and fractional parts of the texture coordinates
            var x0 = (int)Math.Floor(x);
            var y0 = (int)Math.Floor(y);
            var x1 = Math.Min(x0 + 1, width - 1);
            var y1 = Math.Min(y0 + 1, height - 1);
            var dx = x - x0;
            var dy = y - y0;

            // Get the texel colors of the four surrounding texels
            var c00 = texture.Pixels[(y0 * width) + x0];
            var c10 = texture.Pixels[(y0 * width) + x1];
            var c01 = texture.Pixels[(y1 * width) + x0];
            var c11 = texture.Pixels[(y1 * width) + x1];

            // Perform linear interpolation to get the final texel color
            int Channel(int shift) => (int)Math.Round(
                ((1 - dx) * (1 - dy) * ((c00 >> shift) & 0xFF)) + (dx * (1 - dy) * ((c10 >> shift) & 0xFF)) +
                ((1 - dx) * dy * ((c01 >> shift) & 0xFF)) + (dx * dy * ((c11 >> shift) & 0xFF))) << shift;
            return Channel(24) | Channel(16) | Channel(8) | Channel(0);
        }

        private static void FillTriangle(in TexturePixels texture, int[] pixels, Size size, ReadOnlySpan<PointF> vertices,
            ReadOnlySpan<PointF> texCoords)
        {
            // A triangle with no area (e.g. of a brick with no length) covers nothing
            var area = ((vertices[1].Y - vertices[2].Y) * (vertices[0].X - vertices[2].X)) +
                       ((vertices[2].X - vertices[1].X) * (vertices[0].Y - vertices[2].Y));
            if (!(area != 0) || float.IsInfinity(area))
                return;
            int minX = Math.Max(0, (int)Math.Min(vertices[0].X, Math.Min(vertices[1].X, vertices[2].X)));
            int maxX = Math.Min(size.Width - 1, (int)Math.Max(vertices[0].X, Math.Max(vertices[1].X, vertices[2].X)));
            int minY = Math.Max(0, (int)Math.Min(vertices[0].Y, Math.Min(vertices[1].Y, vertices[2].Y)));
            int maxY = Math.Min(size.Height - 1, (int)Math.Max(vertices[0].Y, Math.Max(vertices[1].Y, vertices[2].Y)));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    // Check if the pixel is inside the triangle
                    // Sample at the centre of the pixel
                    if (PointInTriangle(new PointF(x + 0.5f, y + 0.5f), vertices[0], vertices[1], vertices[2]))
                    {
                        var uv = InterpolateUV(x + 0.5f, y + 0.5f, vertices, texCoords);
                        pixels[(y * size.Width) + x] = GetTexture(texture, uv.X, uv.Y);
                    }
                }
            }
        }

        private static PointF InterpolateUV(float x, float y, ReadOnlySpan<PointF> vertices, ReadOnlySpan<PointF> texCoords)
        {
            // Calculate the barycentric coordinates of the point with respect to the triangle
            var alpha = ((vertices[1].Y - vertices[2].Y) * (x - vertices[2].X) + (vertices[2].X - vertices[1].X) * (y - vertices[2].Y)) /
                          ((vertices[1].Y - vertices[2].Y) * (vertices[0].X - vertices[2].X) + (vertices[2].X - vertices[1].X) * (vertices[0].Y - vertices[2].Y));
            var beta = ((vertices[2].Y - vertices[0].Y) * (x - vertices[2].X) + (vertices[0].X - vertices[2].X) * (y - vertices[2].Y)) /
                         ((vertices[1].Y - vertices[2].Y) * (vertices[0].X - vertices[2].X) + (vertices[2].X - vertices[1].X) * (vertices[0].Y - vertices[2].Y));
            var gamma = 1 - alpha - beta;

            // Interpolate the texture coordinates using the barycentric coordinates
            var interpolatedU = alpha * texCoords[0].X + beta * texCoords[1].X + gamma * texCoords[2].X;
            var interpolatedV = alpha * texCoords[0].Y + beta * texCoords[1].Y + gamma * texCoords[2].Y;

            return new PointF(interpolatedU, interpolatedV);
        }

        private static bool PointInTriangle(PointF p, PointF p0, PointF p1, PointF p2)
        {
            var s = (p0.X - p2.X) * (p.Y - p2.Y) - (p0.Y - p2.Y) * (p.X - p2.X);
            var t = (p1.X - p0.X) * (p.Y - p0.Y) - (p1.Y - p0.Y) * (p.X - p0.X);

            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;

            var d = (p2.X - p1.X) * (p.Y - p1.Y) - (p2.Y - p1.Y) * (p.X - p1.X);
            return d == 0 || (d < 0) == (s + t <= 0);
        }


        private static void CalculateBrickVertices(BrickData b, out PointF[] vertices, out PointF[] textureCoords)
        {
            if (b.Curved)
            {
                // The brick's position is half way between its inner and outer edge, the texture runs along the curve
                var curvePoints = Math.Max(2, b.CurvePoints == 0 ? 4 : b.CurvePoints);
                var segments = curvePoints - 1;
                var innerRadius = b.Length;
                var outerRadius = innerRadius + b.Width;
                var circleCentre = new PointF(b.Location.X - (b.Width / 2) - innerRadius + CurvedOffset.X, b.Location.Y + CurvedOffset.Y);
                var divAngle = b.SectorAngle / segments;
                var startAngle = -(b.SectorAngle / 2.0f);

                var outer = new PointF[curvePoints];
                var inner = new PointF[curvePoints];
                for (int i = 0; i < curvePoints; i++)
                {
                    var angle = startAngle + (divAngle * i);
                    outer[i] = GetBrickAngularPoint(circleCentre, angle, outerRadius);
                    inner[i] = GetBrickAngularPoint(circleCentre, angle, innerRadius);
                }

                vertices = new PointF[segments * 6];
                textureCoords = new PointF[segments * 6];
                for (var i = 0; i < segments; i++)
                {
                    var u0 = (float)i / segments;
                    var u1 = (float)(i + 1) / segments;
                    var j = i * 6;
                    vertices[j + 0] = outer[i];
                    vertices[j + 1] = outer[i + 1];
                    vertices[j + 2] = inner[i];
                    vertices[j + 3] = outer[i + 1];
                    vertices[j + 4] = inner[i + 1];
                    vertices[j + 5] = inner[i];
                    textureCoords[j + 0] = new PointF(u0, 0);
                    textureCoords[j + 1] = new PointF(u1, 0);
                    textureCoords[j + 2] = new PointF(u0, 1);
                    textureCoords[j + 3] = new PointF(u1, 0);
                    textureCoords[j + 4] = new PointF(u1, 1);
                    textureCoords[j + 5] = new PointF(u0, 1);
                }
            }
            else
            {
                var ox = StraightOffset.X;
                var oy = StraightOffset.Y;
                vertices = new PointF[] {
                    new PointF(-b.Length / 2 + ox, -b.Width / 2 + oy),
                    new PointF(b.Length / 2 + ox, -b.Width / 2 + oy),
                    new PointF(b.Length / 2 + ox, b.Width / 2 + oy),
                    new PointF(-b.Length / 2 + ox, -b.Width / 2 + oy),
                    new PointF(b.Length / 2 + ox, b.Width / 2 + oy),
                    new PointF(-b.Length / 2 + ox, b.Width / 2 + oy),
                };
                textureCoords = new PointF[]
                {
                    new PointF(0, 0),
                    new PointF(1, 0),
                    new PointF(1, 1),

                    new PointF(0, 0),
                    new PointF(1, 1),
                    new PointF(0, 1),
                };
            }
        }

        private static PointF GetBrickAngularPoint(PointF circleCentre, float angle, float radius)
        {
            return new PointF((float)Math.Cos(MathExt.ToRadians(angle)) * radius + circleCentre.X,
                (float)Math.Sin(MathExt.ToRadians(angle)) * radius + circleCentre.Y);
        }
    }
}

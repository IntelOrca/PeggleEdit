using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        /// <paramref name="g"/>, which includes the brick's rotation.
        /// </summary>
        public static void Draw(Graphics g, in BrickData brick, PointF location)
        {
            var image = GetBrickImage(brick);
            var backup = g.Transform;
            g.TranslateTransform(ScreenOffset.X, ScreenOffset.Y, MatrixOrder.Append);
            g.DrawImage(image, location.X - (image.Width / 2), location.Y - (image.Height / 2));
            g.Transform = backup;
        }

        public static Bitmap GetBrickImage(in BrickData brick)
        {
            Initialise();
            if (!_cachedBitmaps.TryGetValue(brick, out var result))
            {
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
            var supersample = true;
            CalculateBrickVertices(brickData, out var vertices, out var texCoords);

            // Large enough for the brick (centred, so an even size), at least 100x100
            var extent = 0.0f;
            foreach (var v in vertices)
                extent = Math.Max(extent, Math.Max(Math.Abs(v.X), Math.Abs(v.Y)));
            var size = Math.Min(2048, Math.Max(100, 2 * (int)Math.Ceiling(extent + 2)));
            var bitmapSize = new Size(size, size);
            if (supersample)
            {
                bitmapSize = new Size(bitmapSize.Width * 2, bitmapSize.Height * 2);
            }

            var bitmapLarge = new Bitmap(bitmapSize.Width, bitmapSize.Height);
            try
            {
                var scale = supersample ? 2 : 1;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new PointF((bitmapSize.Width / 2) + (vertices[i].X * scale), (bitmapSize.Height / 2) + (vertices[i].Y * scale));
                }

                var texture = GetBrickTexture(brickData);
                var numTriangles = vertices.Length / 3;
                for (int i = 0; i < numTriangles; i++)
                {
                    SetPixelBufferColor(
                        (u, v) => GetTexture(texture, u, v),
                        (x, y, c) => SetPixel(bitmapLarge, x, y, c),
                        new ReadOnlySpan<PointF>(vertices, i * 3, 3),
                        new ReadOnlySpan<PointF>(texCoords, i * 3, 3));
                }
            }
            catch
            {
            }

            if (supersample)
            {
                var bitmapSmall = new Bitmap(bitmapSize.Width / 2, bitmapSize.Height / 2);
                using (var g = Graphics.FromImage(bitmapSmall))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                    g.DrawImage(bitmapLarge, 0, 0, bitmapLarge.Width / 2, bitmapLarge.Height / 2);
                }
                return bitmapSmall;
            }
            else
            {
                return bitmapLarge;
            }
        }

        private static uint GetTexture(Bitmap bitmap, float u, float v)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            if (u < 0) u = 0;
            if (v < 0) v = 0;
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
            var color00 = bitmap.GetPixel(x0, y0);
            var color10 = bitmap.GetPixel(x1, y0);
            var color01 = bitmap.GetPixel(x0, y1);
            var color11 = bitmap.GetPixel(x1, y1);

            // Perform linear interpolation to get the final texel color
            var interpolatedColor = Color.FromArgb(
                (int)Math.Round((1 - dx) * (1 - dy) * color00.A + dx * (1 - dy) * color10.A + (1 - dx) * dy * color01.A + dx * dy * color11.A),
                (int)Math.Round((1 - dx) * (1 - dy) * color00.R + dx * (1 - dy) * color10.R + (1 - dx) * dy * color01.R + dx * dy * color11.R),
                (int)Math.Round((1 - dx) * (1 - dy) * color00.G + dx * (1 - dy) * color10.G + (1 - dx) * dy * color01.G + dx * dy * color11.G),
                (int)Math.Round((1 - dx) * (1 - dy) * color00.B + dx * (1 - dy) * color10.B + (1 - dx) * dy * color01.B + dx * dy * color11.B)
            );

            return (uint)interpolatedColor.ToArgb();
        }

        private static void SetPixel(Bitmap bitmap, int x, int y, uint colour)
        {
            bitmap.SetPixel(x, y, Color.FromArgb((int)colour));
        }

        private static void SetPixelBufferColor(
            Func<float, float, uint> getTexture,
            Action<int, int, uint> setPixel,
            ReadOnlySpan<PointF> vertices,
            ReadOnlySpan<PointF> texCoords)
        {
            int minX = (int)Math.Min(vertices[0].X, Math.Min(vertices[1].X, vertices[2].X));
            int maxX = (int)Math.Max(vertices[0].X, Math.Max(vertices[1].X, vertices[2].X));
            int minY = (int)Math.Min(vertices[0].Y, Math.Min(vertices[1].Y, vertices[2].Y));
            int maxY = (int)Math.Max(vertices[0].Y, Math.Max(vertices[1].Y, vertices[2].Y));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    // Check if the pixel is inside the triangle
                    // Sample at the centre of the pixel
                    if (PointInTriangle(new PointF(x + 0.5f, y + 0.5f), vertices[0], vertices[1], vertices[2]))
                    {
                        var uv = InterpolateUV(x + 0.5f, y + 0.5f, vertices, texCoords);
                        var c = getTexture(uv.X, uv.Y);
                        setPixel(x, y, c);
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

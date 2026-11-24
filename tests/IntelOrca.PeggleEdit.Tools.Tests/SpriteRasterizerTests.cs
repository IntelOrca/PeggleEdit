using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using IntelOrca.PeggleEdit.Tools.Levels;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Tests the particle rasterizer against a plain (slow) bilinear sampler.
    /// </summary>
    public class SpriteRasterizerTests
    {
        const int Size = 160;

        static Bitmap CreateImage()
        {
            // A 24x16 image with a gradient and partial alpha
            var image = new Bitmap(24, 16, PixelFormat.Format32bppArgb);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                    image.SetPixel(x, y, Color.FromArgb(40 + (x * 9), 255 - (x * 10), 30 + (y * 14), 200 - (x * y % 150)));
            }
            return image;
        }

        static uint[] Rasterize(Sprite sprite, float m11, float m12, float m21, float m22, float dx, float dy, bool additive, byte a)
        {
            var pixels = new uint[Size * Size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = 0xFF204060;
            var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                SpriteRasterizer.Draw(handle.AddrOfPinnedObject(), Size * 4, new Rectangle(0, 0, Size, Size), sprite,
                    m11, m12, m21, m22, dx, dy, 255, 200, 100, a, additive);
            }
            finally
            {
                handle.Free();
            }
            return pixels;
        }

        /// <summary>
        /// The same drawing, one pixel at a time with the full inverse transform and floating point.
        /// </summary>
        static uint[] Reference(Bitmap image, float m11, float m12, float m21, float m22, float dx, float dy, bool additive, byte a)
        {
            var pixels = new uint[Size * Size];
            float det = (m11 * m22) - (m12 * m21);
            float i11 = m22 / det, i12 = -m12 / det, i21 = -m21 / det, i22 = m11 / det;
            double[] Texel(int u, int v)
            {
                if (u < 0 || v < 0 || u >= image.Width || v >= image.Height)
                    return new double[4];
                var c = image.GetPixel(u, v);
                return new double[] { c.A, c.R * c.A / 255.0, c.G * c.A / 255.0, c.B * c.A / 255.0 };
            }
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    double d0 = 0xFF, d1 = 0x20, d2 = 0x40, d3 = 0x60;
                    float px = x + 0.5f - dx, py = y + 0.5f - dy;
                    double u = (i11 * px) + (i21 * py) - 0.5, v = (i12 * px) + (i22 * py) - 0.5;
                    int u0 = (int)Math.Floor(u), v0 = (int)Math.Floor(v);
                    double fu = u - u0, fv = v - v0;
                    var c = new double[4];
                    var t00 = Texel(u0, v0); var t10 = Texel(u0 + 1, v0); var t01 = Texel(u0, v0 + 1); var t11 = Texel(u0 + 1, v0 + 1);
                    for (int k = 0; k < 4; k++)
                        c[k] = (t00[k] * (1 - fu) * (1 - fv)) + (t10[k] * fu * (1 - fv)) + (t01[k] * (1 - fu) * fv) + (t11[k] * fu * fv);
                    double fa = a / 255.0;
                    double sr = c[1] * 1.0 * fa, sg = c[2] * (200 / 255.0) * fa, sb = c[3] * (100 / 255.0) * fa, sa = c[0] * fa;
                    if (additive)
                    {
                        d1 += sr; d2 += sg; d3 += sb;
                    }
                    else
                    {
                        double k = 1 - (sa / 255);
                        d1 = sr + (d1 * k); d2 = sg + (d2 * k); d3 = sb + (d3 * k); d0 = sa + (d0 * k);
                    }
                    uint B(double f) => (uint)Math.Max(0, Math.Min(255, Math.Round(f)));
                    pixels[(y * Size) + x] = (B(d0) << 24) | (B(d1) << 16) | (B(d2) << 8) | B(d3);
                }
            }
            return pixels;
        }

        [Theory]
        [InlineData(1, 0, 0, 1, 20, 30, false)]
        [InlineData(1, 0, 0, 1, 20, 30, true)]
        [InlineData(2.5, 0, 0, 2.5, 13.3, 7.7, true)]
        [InlineData(0.6, 0, 0, 0.75, 40, 41, false)]
        [InlineData(3, 0.5, -0.5, 3, 60, 20, true)]
        [InlineData(-2, 1, -1, -2, 120, 110, false)]
        [InlineData(0, 4, -4, 0, 140, 10, true)]
        [InlineData(2.9, -2.1, 2.1, 2.9, 30, 90, true)]
        public void MatchesPlainSampling(float m11, float m12, float m21, float m22, float dx, float dy, bool additive)
        {
            using (var image = CreateImage())
            {
                var sprite = Sprite.FromImage(image);
                foreach (byte a in new byte[] { 255, 90 })
                {
                    var actual = Rasterize(sprite, m11, m12, m21, m22, dx, dy, additive, a);
                    var expected = Reference(image, m11, m12, m21, m22, dx, dy, additive, a);
                    int worst = 0;
                    string where = "";
                    for (int i = 0; i < actual.Length; i++)
                    {
                        for (int shift = 0; shift < 32; shift += 8)
                        {
                            int d = Math.Abs((int)((actual[i] >> shift) & 0xFF) - (int)((expected[i] >> shift) & 0xFF));
                            if (d > worst)
                            {
                                worst = d;
                                where = $"({i % Size}, {i / Size}): {actual[i]:X8} not {expected[i]:X8}";
                            }
                        }
                    }
                    Assert.True(worst <= 3, $"alpha {a}: a pixel differs by {worst} at {where}");
                }
            }
        }
    }
}

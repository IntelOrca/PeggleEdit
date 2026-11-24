// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2011. http://tedtycoon.co.uk
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// An image as premultiplied ARGB pixels, ready to be drawn by <see cref="SpriteRasterizer"/>. The pixels have a
    /// transparent border of one pixel, so that filtering needs no bounds checks.
    /// </summary>
    internal sealed class Sprite
    {
        private static readonly ConditionalWeakTable<Image, Sprite> Cache = new ConditionalWeakTable<Image, Sprite>();

        public int Width { get; }
        public int Height { get; }

        /// <summary>
        /// The pixels, (Width + 2) × (Height + 2), with the image at (1, 1).
        /// </summary>
        public uint[] Pixels { get; }

        public int Stride => Width + 2;

        private Sprite(int width, int height, uint[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public static Sprite FromImage(Image image)
        {
            if (image == null)
                return null;
            lock (Cache)
            {
                if (Cache.TryGetValue(image, out var sprite))
                    return sprite;
                sprite = Create(image);
                Cache.Add(image, sprite);
                return sprite;
            }
        }

        private static Sprite Create(Image image)
        {
            int w = image.Width;
            int h = image.Height;
            using (var bitmap = new Bitmap(w, h, PixelFormat.Format32bppPArgb))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.Transparent);
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(image, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
                }
                var data = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
                try
                {
                    var row = new int[w];
                    var result = new uint[(w + 2) * (h + 2)];
                    for (int y = 0; y < h; y++)
                    {
                        Marshal.Copy(data.Scan0 + (y * data.Stride), row, 0, w);
                        Buffer.BlockCopy(row, 0, result, (((y + 1) * (w + 2)) + 1) * 4, w * 4);
                    }
                    return new Sprite(w, h, result);
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }
            }
        }

        /// <summary>
        /// A soft white dot, drawn for particles whose image can not be found.
        /// </summary>
        public static Sprite Dot { get; } = CreateDot(16);

        private static Sprite CreateDot(int size)
        {
            var pixels = new uint[(size + 2) * (size + 2)];
            float c = (size - 1) / 2.0f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = (float)Math.Sqrt(((x - c) * (x - c)) + ((y - c) * (y - c))) / (size / 2.0f);
                    uint a = (uint)(Math.Max(0, Math.Min(1, 1.5f - (1.5f * d))) * 255);
                    pixels[((y + 1) * (size + 2)) + x + 1] = (a << 24) | (a << 16) | (a << 8) | a;
                }
            }
            return new Sprite(size, size, pixels);
        }
    }

    /// <summary>
    /// Draws sprites onto a 32-bit premultiplied ARGB pixel buffer as Peggle Nights' 3D renderer does: transformed,
    /// bilinear filtered, tinted by a colour and either alpha blended or added (D3D SRCALPHA, ONE). GDI+ can not add,
    /// and most of the game's emitters are additive.
    /// </summary>
    internal static class SpriteRasterizer
    {
        // Sprites covering more pixels than this are drawn a row per thread
        const int ParallelArea = 24 * 1024;

        private struct Job
        {
            public IntPtr Scan0;
            public int Stride;
            public Sprite Sprite;
            public float I11, I12, I21, I22, Dx, Dy;
            public int Left, Right;
            public uint TintR, TintG, TintB, TintA;
            public bool Additive;
            public bool Exact;
            public int ExactX, ExactY;
        }

        /// <summary>
        /// Draws a sprite with an affine transform from sprite pixel coordinates to buffer pixel coordinates:
        /// x = m11·u + m21·v + dx, y = m12·u + m22·v + dy.
        /// </summary>
        public static void Draw(IntPtr scan0, int targetStride, Rectangle clip, Sprite sprite,
            float m11, float m12, float m21, float m22, float dx, float dy,
            byte r, byte g, byte b, byte a, bool additive)
        {
            if (a == 0)
                return;
            float det = (m11 * m22) - (m12 * m21);
            if (det == 0 || float.IsNaN(det) || float.IsInfinity(det))
                return;

            int sw = sprite.Width;
            int sh = sprite.Height;

            // Bounds of the transformed sprite, with its border, as filtering spreads its edge pixels by half a pixel
            float ul = -0.5f, vt = -0.5f, ur = sw + 0.5f, vb = sh + 0.5f;
            float x0 = (m11 * ul) + (m21 * vt) + dx, y0 = (m12 * ul) + (m22 * vt) + dy;
            float x1 = (m11 * ur) + (m21 * vt) + dx, y1 = (m12 * ur) + (m22 * vt) + dy;
            float x2 = (m11 * ul) + (m21 * vb) + dx, y2 = (m12 * ul) + (m22 * vb) + dy;
            float x3 = (m11 * ur) + (m21 * vb) + dx, y3 = (m12 * ur) + (m22 * vb) + dy;
            int left = Math.Max(clip.Left, (int)Math.Floor(Math.Min(Math.Min(x0, x1), Math.Min(x2, x3))) - 1);
            int right = Math.Min(clip.Right, (int)Math.Ceiling(Math.Max(Math.Max(x0, x1), Math.Max(x2, x3))) + 1);
            int top = Math.Max(clip.Top, (int)Math.Floor(Math.Min(Math.Min(y0, y1), Math.Min(y2, y3))) - 1);
            int bottom = Math.Min(clip.Bottom, (int)Math.Ceiling(Math.Max(Math.Max(y0, y1), Math.Max(y2, y3))) + 1);
            if (left >= right || top >= bottom)
                return;

            var job = new Job
            {
                Scan0 = scan0,
                Stride = targetStride,
                Sprite = sprite,
                I11 = m22 / det,
                I12 = -m12 / det,
                I21 = -m21 / det,
                I22 = m11 / det,
                Dx = dx,
                Dy = dy,
                Left = left,
                Right = right,
                // The tint, with the alpha applied to the colour as the image is premultiplied, out of 65536
                TintR = (uint)(r * a * 65536L / 65025),
                TintG = (uint)(g * a * 65536L / 65025),
                TintB = (uint)(b * a * 65536L / 65025),
                TintA = (uint)(a * 65536L / 255),
                Additive = additive,
                // Whether the sprite is drawn pixel for pixel, so no filtering is needed
                Exact = m11 == 1 && m22 == 1 && m12 == 0 && m21 == 0 && dx == (float)Math.Floor(dx) && dy == (float)Math.Floor(dy),
                ExactX = (int)dx,
                ExactY = (int)dy,
            };

            if ((long)(right - left) * (bottom - top) >= ParallelArea)
                Parallel.For(top, bottom, y => DrawRow(ref job, y));
            else
            {
                for (int y = top; y < bottom; y++)
                    DrawRow(ref job, y);
            }
        }

        private static unsafe void DrawRow(ref Job job, int y)
        {
            var sprite = job.Sprite;
            int sw = sprite.Width;
            int sh = sprite.Height;
            int stride = sprite.Stride;
            uint* row = (uint*)((byte*)job.Scan0 + (y * job.Stride));
            fixed (uint* src = sprite.Pixels)
            {
                if (job.Exact)
                {
                    int v = y - job.ExactY;
                    if (v < 0 || v >= sh)
                        return;
                    int xs = Math.Max(job.Left, job.ExactX);
                    int xe = Math.Min(job.Right, job.ExactX + sw);
                    uint* s = src + ((v + 1) * stride) + 1 - job.ExactX;
                    for (int x = xs; x < xe; x++)
                    {
                        uint c = s[x];
                        if (c != 0)
                            Blend(row + x, c, ref job);
                    }
                    return;
                }

                // Sprite coordinates at the centre of the row's first pixel, and how they change along the row
                float py = y + 0.5f - job.Dy;
                float px = job.Left + 0.5f - job.Dx;
                float u = (job.I11 * px) + (job.I21 * py) - 0.5f;
                float v2 = (job.I12 * px) + (job.I22 * py) - 0.5f;
                float du = job.I11;
                float dv = job.I12;

                // Only the pixels where the sprite (with its border) can be: -1 < u < w and -1 < v < h
                float tMin = 0, tMax = job.Right - job.Left;
                if (!Span(u, du, -1, sw, ref tMin, ref tMax) || !Span(v2, dv, -1, sh, ref tMin, ref tMax))
                    return;
                int start = (int)Math.Ceiling(tMin);
                int end = (int)Math.Ceiling(tMax);
                u += du * start;
                v2 += dv * start;
                for (int i = start; i < end; i++, u += du, v2 += dv)
                {
                    // Fixed point, 8 bits of fraction, in the bordered sprite
                    int fu = (int)((u + 1) * 256);
                    int fv = (int)((v2 + 1) * 256);
                    int iu = fu >> 8;
                    int iv = fv >> 8;
                    if (iu < 0 || iv < 0 || iu > sw || iv > sh)
                        continue;
                    uint wu = (uint)(fu & 255);
                    uint wv = (uint)(fv & 255);
                    uint* p = src + (iv * stride) + iu;
                    uint c00 = p[0], c10 = p[1], c01 = p[stride], c11 = p[stride + 1];
                    if ((c00 | c10 | c01 | c11) == 0)
                        continue;
                    uint w11 = (wu * wv) >> 8;
                    uint w10 = wu - w11;
                    uint w01 = wv - w11;
                    uint w00 = 256 - wu - wv + w11;
                    uint rb = ((((c00 & 0xFF00FF) * w00) + ((c10 & 0xFF00FF) * w10) + ((c01 & 0xFF00FF) * w01) + ((c11 & 0xFF00FF) * w11)) + 0x800080) >> 8;
                    uint ag = (((((c00 >> 8) & 0xFF00FF) * w00) + (((c10 >> 8) & 0xFF00FF) * w10) + (((c01 >> 8) & 0xFF00FF) * w01) + (((c11 >> 8) & 0xFF00FF) * w11)) + 0x800080) >> 8;
                    uint c = ((ag & 0xFF00FF) << 8) | (rb & 0xFF00FF);
                    if (c != 0)
                        Blend(row + job.Left + i, c, ref job);
                }
            }
        }

        /// <summary>
        /// Narrows [tMin, tMax) to where start + step·t is strictly between low and high.
        /// </summary>
        private static bool Span(float start, float step, float low, float high, ref float tMin, ref float tMax)
        {
            if (step == 0)
                return start > low && start < high;
            float a = (low - start) / step;
            float b = (high - start) / step;
            if (a > b)
            {
                var t = a;
                a = b;
                b = t;
            }
            tMin = Math.Max(tMin, (float)Math.Floor(a));
            tMax = Math.Min(tMax, (float)Math.Ceiling(b) + 1);
            return tMin < tMax;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void Blend(uint* dst, uint c, ref Job job)
        {
            uint sr = ((((c >> 16) & 0xFF) * job.TintR) + 32768) >> 16;
            uint sg = ((((c >> 8) & 0xFF) * job.TintG) + 32768) >> 16;
            uint sb = (((c & 0xFF) * job.TintB) + 32768) >> 16;
            uint d = *dst;
            uint dr = (d >> 16) & 0xFF;
            uint dg = (d >> 8) & 0xFF;
            uint db = d & 0xFF;
            uint da = d >> 24;
            if (job.Additive)
            {
                dr += sr;
                dg += sg;
                db += sb;
                if (dr > 255) dr = 255;
                if (dg > 255) dg = 255;
                if (db > 255) db = 255;
            }
            else
            {
                uint sa = (((c >> 24) * job.TintA) + 32768) >> 16;
                uint k = 255 - sa;
                dr = sr + (((dr * k) + 127) / 255);
                dg = sg + (((dg * k) + 127) / 255);
                db = sb + (((db * k) + 127) / 255);
                da = sa + (((da * k) + 127) / 255);
                if (dr > 255) dr = 255;
                if (dg > 255) dg = 255;
                if (db > 255) db = 255;
                if (da > 255) da = 255;
            }
            *dst = (da << 24) | (dr << 16) | (dg << 8) | db;
        }
    }
}

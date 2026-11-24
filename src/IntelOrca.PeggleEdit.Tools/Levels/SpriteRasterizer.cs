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

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// An image as premultiplied ARGB pixels, ready to be drawn by <see cref="SpriteRasterizer"/>.
    /// </summary>
    internal sealed class Sprite
    {
        private static readonly ConditionalWeakTable<Image, Sprite> Cache = new ConditionalWeakTable<Image, Sprite>();

        public int Width { get; }
        public int Height { get; }
        public uint[] Pixels { get; }

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
                    var pixels = new int[w * h];
                    for (int y = 0; y < h; y++)
                        Marshal.Copy(data.Scan0 + (y * data.Stride), pixels, y * w, w);
                    var result = new uint[pixels.Length];
                    Buffer.BlockCopy(pixels, 0, result, 0, pixels.Length * 4);
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
            var pixels = new uint[size * size];
            float c = (size - 1) / 2.0f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = (float)Math.Sqrt(((x - c) * (x - c)) + ((y - c) * (y - c))) / (size / 2.0f);
                    uint a = (uint)(Math.Max(0, Math.Min(1, 1.5f - (1.5f * d))) * 255);
                    pixels[(y * size) + x] = (a << 24) | (a << 16) | (a << 8) | a;
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
        /// <summary>
        /// Draws a sprite with an affine transform from sprite pixel coordinates to buffer pixel coordinates:
        /// x = m11·u + m21·v + dx, y = m12·u + m22·v + dy.
        /// </summary>
        public static unsafe void Draw(BitmapData target, Rectangle clip, Sprite sprite,
            float m11, float m12, float m21, float m22, float dx, float dy,
            byte r, byte g, byte b, byte a, bool additive)
        {
            if (a == 0 && !additive)
                return;
            float det = (m11 * m22) - (m12 * m21);
            if (det == 0 || float.IsNaN(det) || float.IsInfinity(det))
                return;

            int sw = sprite.Width;
            int sh = sprite.Height;

            // Bounds of the transformed sprite
            float x0 = dx, y0 = dy;
            float x1 = (m11 * sw) + dx, y1 = (m12 * sw) + dy;
            float x2 = (m21 * sh) + dx, y2 = (m22 * sh) + dy;
            float x3 = (m11 * sw) + (m21 * sh) + dx, y3 = (m12 * sw) + (m22 * sh) + dy;
            int left = Math.Max(clip.Left, (int)Math.Floor(Math.Min(Math.Min(x0, x1), Math.Min(x2, x3))));
            int right = Math.Min(clip.Right, (int)Math.Ceiling(Math.Max(Math.Max(x0, x1), Math.Max(x2, x3))));
            int top = Math.Max(clip.Top, (int)Math.Floor(Math.Min(Math.Min(y0, y1), Math.Min(y2, y3))));
            int bottom = Math.Min(clip.Bottom, (int)Math.Ceiling(Math.Max(Math.Max(y0, y1), Math.Max(y2, y3))));
            if (left >= right || top >= bottom)
                return;

            // Inverse transform
            float i11 = m22 / det, i12 = -m12 / det, i21 = -m21 / det, i22 = m11 / det;

            // The tint, with the alpha applied to the colour as the image is premultiplied
            float fa = a / 255.0f;
            float fr = r / 255.0f * fa;
            float fg = g / 255.0f * fa;
            float fb = b / 255.0f * fa;

            // Whether the sprite is drawn pixel for pixel, so no filtering is needed
            bool exact = m11 == 1 && m22 == 1 && m12 == 0 && m21 == 0 && dx == (float)Math.Floor(dx) && dy == (float)Math.Floor(dy);

            fixed (uint* src = sprite.Pixels)
            {
                byte* scan0 = (byte*)target.Scan0;
                int stride = target.Stride;
                for (int y = top; y < bottom; y++)
                {
                    uint* row = (uint*)(scan0 + (y * stride));
                    float py = y + 0.5f - dy;
                    for (int x = left; x < right; x++)
                    {
                        float px = x + 0.5f - dx;
                        float u = (i11 * px) + (i21 * py) - 0.5f;
                        float v = (i12 * px) + (i22 * py) - 0.5f;

                        float sa, sr, sg, sb;
                        if (exact)
                        {
                            int iu = (int)Math.Round(u);
                            int iv = (int)Math.Round(v);
                            if (iu < 0 || iv < 0 || iu >= sw || iv >= sh)
                                continue;
                            uint c = src[(iv * sw) + iu];
                            if (c == 0)
                                continue;
                            sa = c >> 24;
                            sr = (c >> 16) & 0xFF;
                            sg = (c >> 8) & 0xFF;
                            sb = c & 0xFF;
                        }
                        else
                        {
                            if (u <= -1 || v <= -1 || u >= sw || v >= sh)
                                continue;
                            int u0 = (int)Math.Floor(u);
                            int v0 = (int)Math.Floor(v);
                            float fu = u - u0;
                            float fv = v - v0;
                            uint c00 = Texel(src, sw, sh, u0, v0);
                            uint c10 = Texel(src, sw, sh, u0 + 1, v0);
                            uint c01 = Texel(src, sw, sh, u0, v0 + 1);
                            uint c11 = Texel(src, sw, sh, u0 + 1, v0 + 1);
                            if ((c00 | c10 | c01 | c11) == 0)
                                continue;
                            float w00 = (1 - fu) * (1 - fv);
                            float w10 = fu * (1 - fv);
                            float w01 = (1 - fu) * fv;
                            float w11 = fu * fv;
                            sa = ((c00 >> 24) * w00) + ((c10 >> 24) * w10) + ((c01 >> 24) * w01) + ((c11 >> 24) * w11);
                            sr = (((c00 >> 16) & 0xFF) * w00) + (((c10 >> 16) & 0xFF) * w10) + (((c01 >> 16) & 0xFF) * w01) + (((c11 >> 16) & 0xFF) * w11);
                            sg = (((c00 >> 8) & 0xFF) * w00) + (((c10 >> 8) & 0xFF) * w10) + (((c01 >> 8) & 0xFF) * w01) + (((c11 >> 8) & 0xFF) * w11);
                            sb = ((c00 & 0xFF) * w00) + ((c10 & 0xFF) * w10) + ((c01 & 0xFF) * w01) + ((c11 & 0xFF) * w11);
                        }

                        sr *= fr;
                        sg *= fg;
                        sb *= fb;
                        uint d = row[x];
                        float dr = (d >> 16) & 0xFF;
                        float dg = (d >> 8) & 0xFF;
                        float db = d & 0xFF;
                        float da = d >> 24;
                        if (additive)
                        {
                            dr += sr;
                            dg += sg;
                            db += sb;
                        }
                        else
                        {
                            sa *= fa;
                            float k = 1 - (sa / 255.0f);
                            dr = sr + (dr * k);
                            dg = sg + (dg * k);
                            db = sb + (db * k);
                            da = sa + (da * k);
                        }
                        row[x] = (ToByte(da) << 24) | (ToByte(dr) << 16) | (ToByte(dg) << 8) | ToByte(db);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint Texel(uint* src, int w, int h, int u, int v)
        {
            if (u < 0 || v < 0 || u >= w || v >= h)
                return 0;
            return src[(v * w) + u];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToByte(float f)
        {
            int i = (int)(f + 0.5f);
            return i < 0 ? 0 : i > 255 ? 255u : (uint)i;
        }
    }
}

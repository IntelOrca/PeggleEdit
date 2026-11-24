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

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace IntelOrca.PeggleEdit.Tools
{
    /// <summary>
    /// Images converted to premultiplied ARGB for drawing. GDI+ draws an image in premultiplied ARGB, converting it
    /// every time it is drawn if it is in another format; converting it once draws exactly the same pixels, in about
    /// half the time. Images are not changed once they are drawn, so each is converted once.
    /// </summary>
    internal static class PremultipliedImage
    {
        private static readonly ConditionalWeakTable<Image, Image> Cache = new ConditionalWeakTable<Image, Image>();

        /// <summary>
        /// Gets the image to draw in place of <paramref name="image"/>.
        /// </summary>
        public static Image Get(Image image)
        {
            if (!(image is Bitmap bitmap) || bitmap.PixelFormat == PixelFormat.Format32bppPArgb)
                return image;
            lock (Cache)
            {
                if (!Cache.TryGetValue(image, out var result))
                {
                    result = Convert(bitmap);
                    Cache.Add(image, result);
                }
                return result;
            }
        }

        /// <summary>
        /// Converts a bitmap to premultiplied ARGB, keeping its resolution, which sets the size it is drawn at.
        /// </summary>
        public static Bitmap Convert(Bitmap bitmap)
        {
            // Animated images keep their frames
            foreach (var dimension in bitmap.FrameDimensionsList)
            {
                if (bitmap.GetFrameCount(new FrameDimension(dimension)) > 1)
                    return bitmap;
            }
            var result = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format32bppPArgb);
            result.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
            return result;
        }
    }
}

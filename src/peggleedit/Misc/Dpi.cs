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
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer
{
    /// <summary>
    /// Scales sizes written for 96 DPI to the DPI of the display. The application is system DPI aware (see
    /// app.manifest), so one factor, for the system DPI, applies to every window.
    /// </summary>
    static class Dpi
    {
        private static float? _factor;

        /// <summary>
        /// Gets the ratio of the display's DPI to 96 DPI, e.g. 1.75 at 168 DPI (175% scaling).
        /// </summary>
        public static float Factor
        {
            get
            {
                if (_factor == null)
                {
                    try
                    {
                        using (var g = Graphics.FromHwnd(IntPtr.Zero))
                            _factor = Math.Max(1f, g.DpiX / 96f);
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

        /// <summary>
        /// Enlarges an image designed for 96 DPI to the display's scale with a high quality filter.
        /// </summary>
        public static Bitmap ScaleImage(Image image)
        {
            var size = Scale(image.Size);
            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            using (var attributes = new ImageAttributes())
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                // Stops the edge pixels blending with transparent black outside the image
                attributes.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(image, new Rectangle(Point.Empty, size), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return result;
        }

        /// <summary>
        /// Gets an image list with the images of one designed for 96 DPI enlarged to the display's scale, or the same
        /// list at 96 DPI.
        /// </summary>
        public static ImageList ScaleImageList(ImageList imageList)
        {
            if (Factor == 1)
                return imageList;

            // Read the images back in full colour (they are kept as added until the list's handle is created)
            if (!imageList.HandleCreated)
                imageList.ColorDepth = ColorDepth.Depth32Bit;

            var result = new ImageList();
            result.ColorDepth = ColorDepth.Depth32Bit;
            result.ImageSize = Scale(imageList.ImageSize);
            foreach (Image image in imageList.Images)
            {
                using (image)
                    result.Images.Add(ScaleImage(image));
            }
            return result;
        }
    }
}

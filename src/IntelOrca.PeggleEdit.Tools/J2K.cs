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
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using CoreJ2K;
using CoreJ2K.j2k.util;
using CoreJ2K.Util;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools
{
    /// <summary>
    /// Decodes JPEG 2000 (.jp2) images.
    /// </summary>
    public static class J2K
    {
        private static bool _registered;

        private static void Register()
        {
            if (!_registered)
            {
                // CoreJ2K writes its messages to the console, such as a warning for each JPX box it does not know
                // ("Unknown box-type: 0x6a706368") in most of the game's images, which would fill the CLI's output.
                FacilityManager.DefaultMsgLogger = new NullMsgLogger();
                _registered = true;
            }
        }

        private sealed class NullMsgLogger : IMsgLogger
        {
            public void printmsg(int sev, string msg) { }
            public void println(string str, int flind, int ind) { }
            public void flush() { }
        }

        public static Image ConvertJPEG2(string src)
        {
            var buffer = File.ReadAllBytes(src);
            return ConvertJPEG2(buffer);
        }

        public static Image ConvertJPEG2(PakRecord record)
        {
            return ConvertJPEG2(record.Buffer);
        }

        /// <summary>
        /// Decodes a .jp2 image with CoreJ2K. Its pixels are a standard decoder's, which differ a little from those
        /// of the game's own codec (by about 2 of 255 on average). The game's codec is not used, as it is a 32-bit
        /// DLL, which a 64-bit process can not load.
        /// </summary>
        public static Image ConvertJPEG2(byte[] buffer)
        {
            Register();
            using (var image = J2kImage.FromBytes(buffer, (ParameterList)null))
            {
                var width = image.Width;
                var height = image.Height;
                var components = image.NumberOfComponents;
                if (components < 1 || components > 4)
                    throw new NotSupportedException($"JPEG 2000 image with {components} components");

                // 1 component is grey, 2 grey and alpha, 3 RGB and 4 RGBA
                var channels = new byte[components][];
                for (var c = 0; c < components; c++)
                    channels[c] = GetComponentBytes(image, c);
                var grey = components < 3;
                var r = channels[0];
                var g = grey ? r : channels[1];
                var b = grey ? r : channels[2];
                var a = components == 2 ? channels[1] : components == 4 ? channels[3] : null;

                var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                try
                {
                    var bitmapData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    try
                    {
                        var row = new int[width];
                        for (var y = 0; y < height; y++)
                        {
                            var i = y * width;
                            for (var x = 0; x < width; x++, i++)
                                row[x] = ((a == null ? 255 : a[i]) << 24) | (r[i] << 16) | (g[i] << 8) | b[i];
                            Marshal.Copy(row, 0, bitmapData.Scan0 + y * bitmapData.Stride, width);
                        }
                    }
                    finally
                    {
                        result.UnlockBits(bitmapData);
                    }
                    return result;
                }
                catch
                {
                    result.Dispose();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets a component's samples (made unsigned by CoreJ2K, and as many as the image's pixels) scaled to 8 bits.
        /// </summary>
        private static byte[] GetComponentBytes(InterleavedImage image, int component)
        {
            var samples = image.GetComponent(component);
            var bits = image.GetBitDepth(component);
            var max = (1L << bits) - 1;
            var result = new byte[samples.Length];
            for (var i = 0; i < samples.Length; i++)
            {
                long v = samples[i];
                v = v < 0 ? 0 : v > max ? max : v;
                result[i] = (byte)(bits == 8 ? v : (v * 255 + max / 2) / max);
            }
            return result;
        }
    }
}

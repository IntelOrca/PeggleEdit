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
using System.Buffers;
using System.Buffers.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CSJ2K.j2k.util;
using CSJ2K.Util;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools
{
    /// <summary>
    /// Provides static methods for operating with the OpenJPEG library.
    /// </summary>
    public static class J2K
    {
        private const string g_peggleJ2KLicenceKeyEncoded =
            "TFU1T2tUNyFMMDZmaUQ5bk1iQ0U5Wm9KdVh4Z3g0emNnVkRkMiFhMXRiMnVIZmhSckxHNXduTnlTUXN3WHFBdDJo";

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        private static bool _registered;
        private static bool _j2kLoaded;
        private static string _pegglePath;

        public static void RegisterPegglePath(string pegglePath)
        {
            _pegglePath = pegglePath;
        }

        private static void Register()
        {
            if (!_registered)
            {
                BitmapImageCreator.Register();
                // CSJ2K writes its messages to the console, such as a warning for each JPX box it does not know
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
        /// Decodes a .jp2 image with the game's own codec when it can be loaded, as its pixels differ a little from
        /// a standard decoder's and are what the game shows, otherwise with CSJ2K, which can not decode images with
        /// an alpha channel.
        /// </summary>
        public static Image ConvertJPEG2(byte[] buffer)
        {
            try
            {
                var image = ConvertJPEG2WithPeggle(buffer);
                if (image != null)
                    return image;
            }
            catch (Exception)
            {
                // Try CSJ2K instead
            }

            Register();
            var outputImage = CSJ2K.J2kImage.FromBytes(buffer);
            return outputImage.As<Bitmap>();
        }

        private static unsafe Image ConvertJPEG2WithPeggle(byte[] buffer)
        {
            if (!TryLoadPeggleJ2kLibrary())
                return null;

            var img = J2KCodec.OpenMemory(buffer, buffer.Length);
            if (img == IntPtr.Zero)
                return null;

            try
            {
                ThrowOnError(J2KCodec.GetInfoEx(img, out var info));
                var result = new Bitmap(info.Width, info.Height);
                try
                {
                    var bitmapData = result.LockBits(new Rectangle(0, 0, info.Width, info.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    try
                    {
                        var ptr = bitmapData.Scan0;
                        var pixelsLength = bitmapData.Stride * bitmapData.Height;
                        var options = "bpp=4,rl=0";
                        var pitch = bitmapData.Stride;
                        ThrowOnError(J2KCodec.Decode(img, ref ptr, ref pixelsLength, options, ref pitch));
                    }
                    finally
                    {
                        result.UnlockBits(bitmapData);
                    }
                    return result;
                }
                catch
                {
                    // Not decoded: the other decoder is tried
                    result.Dispose();
                    throw;
                }
            }
            finally
            {
                J2KCodec.Close(img);
            }
        }

        private static void ThrowOnError(J2KCodec.J2KError err)
        {
            if (err == J2KCodec.J2KError.Success)
                return;

            var errMessage = J2KCodec.GetErrorString(err);
            throw new Exception($"j2k-codec: {errMessage}");
        }

        private static bool TryLoadPeggleJ2kLibrary()
        {
            if (_j2kLoaded)
                return true;

            // The game's codec is a 32-bit DLL, which a 64-bit process can not load
            if (Environment.Is64BitProcess)
                return false;

            if (string.IsNullOrEmpty(_pegglePath))
                return false;

            var peggleDirectory = _pegglePath;
            if (!Directory.Exists(peggleDirectory))
                peggleDirectory = Path.GetDirectoryName(peggleDirectory);

            var libraryPath = Path.Combine(peggleDirectory, "j2k-codec.dll");
            if (!File.Exists(libraryPath))
                return false;

            if (!TryLoadLibrary(libraryPath))
                return false;

            var licenceKey = DecodeKey(g_peggleJ2KLicenceKeyEncoded);
            if (licenceKey == null)
                return false;

            J2KCodec.Unlock(licenceKey);
            _j2kLoaded = true;
            return true;
        }

        private static bool TryLoadLibrary(string path)
        {
            try
            {
                return LoadLibrary(path) != IntPtr.Zero;
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
            {
                // Not running on Windows (e.g. Mono on Linux), the Peggle library can not be used
                return false;
            }
        }

        private static string DecodeKey(string key)
        {
            var utf8 = Encoding.UTF8.GetBytes(key);
            var buffer = new byte[128];
            var op = Base64.DecodeFromUtf8(utf8, buffer, out _, out var written);
            if (op != OperationStatus.Done)
                return null;

            return Encoding.UTF8.GetString(buffer, 0, written);
        }
    }
}

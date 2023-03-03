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

using System.Drawing;
using CSJ2K.Util;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools
{
    /// <summary>
    /// Provides static methods for operating with the OpenJPEG library.
    /// </summary>
    public static class J2K
    {
        private static bool _registered;

        private static void Register()
        {
            if (!_registered)
            {
                BitmapImageCreator.Register();
                _registered = true;
            }
        }

        public static Image ConvertJPEG2(string src)
        {
            Register();
            var outputImage = CSJ2K.J2kImage.FromFile(src);
            return outputImage.As<Bitmap>();
        }

        public static Image ConvertJPEG2(PakRecord record)
        {
            Register();
            var outputImage = CSJ2K.J2kImage.FromBytes(record.Buffer);
            return outputImage.As<Bitmap>();
        }
    }
}

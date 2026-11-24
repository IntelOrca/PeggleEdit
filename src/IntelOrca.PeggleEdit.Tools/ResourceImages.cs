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
using IntelOrca.PeggleEdit.Tools.Properties;

namespace IntelOrca.PeggleEdit.Tools
{
    /// <summary>
    /// The images in the resources that are drawn for every object. Each read of a resource property decodes a new
    /// image, which took most of the time to draw a level, so they are read once (and converted to premultiplied ARGB
    /// to draw faster, see <see cref="PremultipliedImage"/>).
    /// </summary>
    internal static class ResourceImages
    {
        public static readonly Bitmap Peg = PremultipliedImage.Convert(Resources.peg);
        public static readonly Bitmap PegShadow = PremultipliedImage.Convert(Resources.peg_shadow);
        // Drawn with a colour matrix, which works on the colours before they are premultiplied
        public static readonly Bitmap CircleOuter = Resources.circle_outer;
        public static readonly Bitmap CircleInner = PremultipliedImage.Convert(Resources.circle_inner);
        public static readonly Bitmap AnchorPoint = PremultipliedImage.Convert(Resources.anchor_point);
    }
}

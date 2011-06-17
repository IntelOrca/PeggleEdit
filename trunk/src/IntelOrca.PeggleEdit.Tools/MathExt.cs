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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace IntelOrca.PeggleEdit.Tools
{
	/// <summary>
	/// Provides more static methods for common mathmatical functions.
	/// </summary>
	public static class MathExt
	{
		/// <summary>
		/// Keeps an angle between 0 and 360 degrees.
		/// </summary>
		/// <param name="angle">The angle to fix and return.</param>
		/// <returns>The given angle fixed so that it is between 0 and 360 degrees.</returns>
		public static float FixAngle(float angle)
		{
			if (angle < 0) {
				angle += ((int)angle / 360) * 360;
			}
			
			return (float)(angle % 360.0);
		}

		/// <summary>
		/// Converts radians to degrees.
		/// </summary>
		/// <param name="rad">The radians value to convert.</param>
		/// <returns>A double representing the radians given, in degrees.</returns>
		public static double ToDegrees(double rad)
		{
			return rad * 180.0 / Math.PI;
		}

		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		/// <param name="rad">The degrees value to convert.</param>
		/// <returns>A double representing the degrees given, in radians.</returns>
		public static double ToRadians(double deg)
		{
			return deg / 180.0 * Math.PI;
		}

		/// <summary>
		/// Converts radians to degrees.
		/// </summary>
		/// <param name="rad">The radians value to convert.</param>
		/// <returns>A float representing the radians given, in degrees.</returns>
		public static float ToDegrees(float rad)
		{
			return rad * 180.0f / (float)Math.PI;
		}

		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		/// <param name="rad">The degrees value to convert.</param>
		/// <returns>A float representing the degrees given, in radians.</returns>
		public static float ToRadians(float deg)
		{
			return deg / 180.0f * (float)Math.PI;
		}
	}
}

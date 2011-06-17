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
using System.ComponentModel;
using System.Globalization;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
	/// <summary>
	/// Represents the type converter used to convert from strings to percentages and visa versa.
	/// </summary>
	public class PercentageConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (!(value is string)) {
				return base.ConvertFrom(context, culture, value);
			}

			string s = ((string)value).Replace('%', ' ').Trim();
			
			float num;
			if (!float.TryParse(s, out num)) {
				throw new FormatException("Input string was not in a correct format.");
			}

			num /= 100.0f;

			if ((num < 0.0f) || (num > 1.0f)) {
				throw new FormatException(String.Format("Value of '{0}' is not a valid percentage. Percentages should be between 0% and 100%.", s));
			}

			return num;
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null) {
				throw new ArgumentNullException("destinationType");
			}
			if (destinationType == typeof(string)) {
				float num = (float)value;
				float num2 = num * 100.0f;
				return (num2.ToString() + "%");
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

	}
}

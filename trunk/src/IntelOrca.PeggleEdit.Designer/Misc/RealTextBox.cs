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
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer
{
	class RealTextBox : TextBox
	{
		private double mValue;
		private double mMin;
		private double mMax;

		public RealTextBox()
		{
			mMin = Double.MinValue;
			mMax = Double.MaxValue;
			mValue = 0;
			base.Text = mValue.ToString();
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
			string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
			string groupSeparator = numberFormatInfo.NumberGroupSeparator;
			string negativeSign = numberFormatInfo.NegativeSign;

			string keyInput = e.KeyChar.ToString();

			if (keyInput.Equals(negativeSign)) {
			} else if (Char.IsDigit(e.KeyChar)) {
			} else if (e.KeyChar == '-') {
			} else if (e.KeyChar == '\b') {
			} else if (keyInput.Equals(decimalSeparator)) {
			} else {
				e.Handled = true;
			}
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			if (IsValid(base.Text)) {
				mValue = Convert.ToDouble(base.Text);
			}

			base.Text = mValue.ToString();
		}

		private void UpdateText()
		{
			base.Text = mValue.ToString();
		}

		private bool IsValid(double value)
		{
			return (value >= mMin && value <= mMax);
		}

		private bool IsValid(string text)
		{
			double v;
			if (Double.TryParse(text, out v))
				return IsValid(v);
			else
				return false;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string Text
		{
			get
			{
				return base.Text;
			}
		}

		[Category("Appearance")]
		[DefaultValue(0)]
		public double Value
		{
			get
			{
				return Convert.ToDouble(Text);
			}
			set
			{
				if (IsValid(value)) {
					mValue = value;
					UpdateText();
				} else {
					throw new FormatException("Number specified must be a valid real between the set minimum and maximum.");
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(Double.MinValue)]
		public double Minimum
		{
			get
			{
				return mMin;
			}
			set
			{
				if (value > mMax)
					throw new FormatException("Minimum value must be less than the maximum value.");
				else
					mMin = value;
			}
		}

		[Category("Behavior")]
		[DefaultValue(Double.MaxValue)]
		public double Maximum
		{
			get
			{
				return mMax;
			}
			set
			{
				if (value < mMin)
					throw new FormatException("Maximum value must be greater than the minimum value.");
				else
					mMax = value;
			}
		}
	}
}

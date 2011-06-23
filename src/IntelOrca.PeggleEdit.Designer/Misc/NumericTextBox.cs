using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Globalization;

namespace IntelOrca.PeggleEdit.Designer
{
	class NumericTextBox : TextBox
	{
		private int mValue;
		private int mMin;
		private int mMax;

		public NumericTextBox()
		{
			mMin = Int32.MinValue;
			mMax = Int32.MaxValue;
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
			} else {
				e.Handled = true;
			}
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			if (IsValid(base.Text)) {
				mValue = Convert.ToInt32(base.Text);
			}

			base.Text = mValue.ToString();
		}

		private void UpdateText()
		{
			base.Text = mValue.ToString();
		}

		private bool IsValid(int value)
		{
			return (value >= mMin && value <= mMax);
		}

		private bool IsValid(string text)
		{
			int v;
			if (Int32.TryParse(text, out v))
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
		public int Value
		{
			get
			{
				return Convert.ToInt32(Text);
			}
			set
			{
				if (IsValid(value)) {
					mValue = value;
					UpdateText();
				} else {
					throw new FormatException("Number specified must be a valid integer between the set minimum and maximum.");
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(Int32.MinValue)]
		public int Minimum
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
		[DefaultValue(Int32.MaxValue)]
		public int Maximum
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

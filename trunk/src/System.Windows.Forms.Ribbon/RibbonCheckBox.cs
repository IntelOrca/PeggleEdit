using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Windows.Forms
{
    public class RibbonCheckBox : RibbonItem
    {
		public event EventHandler CheckedChanged;
		public Size CheckBoxSize = new Size(13, 13);

		bool _dontUncheck;
        CheckState _checkState;

        public RibbonCheckBox()
        {

        }

        public RibbonCheckBox(string text)
        {
			base.Text = text;
        }

		public override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (_checkState != CheckState.Checked) {
				_checkState = CheckState.Checked;
				_dontUncheck = true;
			}
		}

		public override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (_checkState == CheckState.Checked && !_dontUncheck)
				_checkState = CheckState.Unchecked;

			_dontUncheck = false;

			if (CheckedChanged != null)
				CheckedChanged.Invoke(this, EventArgs.Empty);
		}

        public override void OnPaint(object sender, RibbonElementPaintEventArgs e)
        {
            Owner.Renderer.OnRenderRibbonItem(new RibbonItemRenderEventArgs(Owner, e.Graphics, e.Clip, this));
        }

        public override void SetBounds(System.Drawing.Rectangle bounds)
        {
            base.SetBounds(bounds);
        }

        public override Size MeasureSize(object sender, RibbonElementMeasureSizeEventArgs e)
        {
			Size textSize = TextRenderer.MeasureText(Text, Owner.Font);

			Size size = new Size(CheckBoxSize.Width + 6 + textSize.Width,
				Math.Max(CheckBoxSize.Height + 4, Owner.Font.Height + 4));

            SetLastMeasuredSize(size);
            return size;
        }

        public CheckState CheckedState
        {
            get
            {
                return _checkState;
            }
        }

		public override bool Checked
		{
			get
			{
				return (_checkState == CheckState.Checked);
			}
			set
			{
				_checkState = (value ? CheckState.Checked : CheckState.Unchecked);
			}
		}
    }
}

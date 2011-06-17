using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace System.Windows.Forms
{
    public class RibbonOrbRecentItem
        : RibbonButton
    {
        #region Ctor

        public RibbonOrbRecentItem()
            : base()
        {

        }

        public RibbonOrbRecentItem(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Methods

        internal override Rectangle OnGetImageBounds(RibbonElementSizeMode sMode, System.Drawing.Rectangle bounds)
        {
            return Rectangle.Empty;
        }

        internal override Rectangle OnGetTextBounds(RibbonElementSizeMode sMode, Rectangle bounds)
        {
            Rectangle r = base.OnGetTextBounds(sMode, bounds);

            r.X = Bounds.Left + 3;

            return r;
        }

        #endregion
    }
}

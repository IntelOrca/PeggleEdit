using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// Large menu item with a description bellow the text
    /// </summary>
    public class RibbonDescriptionMenuItem
        : RibbonButton
    {
        #region Fields
        private string _description;
        private Rectangle _descBounds;
        #endregion

        #region Ctor

        public RibbonDescriptionMenuItem()
        {
            DropDownArrowDirection = RibbonArrowDirection.Left;
            SetDropDownMargin(new Padding(10));
        }

        /// <summary>
        /// Creates a new menu item with description
        /// </summary>
        /// <param name="text">Text of the menuitem</param>
        public RibbonDescriptionMenuItem(string text)
            : this(null, text, null)
        { }

        /// <summary>
        /// Creates a new menu item with description
        /// </summary>
        /// <param name="text">Text of the menuitem</param>
        /// <param name="description">Descripion of the menuitem</param>
        public RibbonDescriptionMenuItem(string text, string description)
            : this(null, text, description)
        { }

        /// <summary>
        /// Creates a new menu item with description
        /// </summary>
        /// <param name="image">Image for the menuitem</param>
        /// <param name="text">Text for the menuitem</param>
        /// <param name="description">Description for the menuitem</param>
        public RibbonDescriptionMenuItem(Image image, string text, string description)
        {
            Image = image;
            Text = text;
            Description = description;
        }

        #endregion

        #region Props

        /// <summary>
        /// Gets or sets the bounds of the description
        /// </summary>
        public Rectangle DescriptionBounds
        {
            get { return _descBounds; }
            set { _descBounds = value; }
        }


        /// <summary>
        /// Gets or sets the image of the menu item
        /// </summary>
        public override System.Drawing.Image Image
        {
            get
            {
                return base.Image;
            }
            set
            {
                base.Image = value;

                SmallImage = value;
            }
        }

        /// <summary>
        /// This property is not relevant for this class
        /// </summary>
        [Browsable(false)]
        public override System.Drawing.Image SmallImage
        {
            get
            {
                return base.SmallImage;
            }
            set
            {
                base.SmallImage = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of the button
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }


        #endregion

        #region Methods

        protected override void OnPaintText(RibbonElementPaintEventArgs e)
        {
            if (e.Mode == RibbonElementSizeMode.DropDown)
            {
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Near;

                Owner.Renderer.OnRenderRibbonItemText(new RibbonTextEventArgs(
                    Owner, e.Graphics, e.Clip, this, TextBounds, Text, Color.Empty, FontStyle.Bold, sf));

                sf.Alignment = StringAlignment.Near;

                Owner.Renderer.OnRenderRibbonItemText(new RibbonTextEventArgs(
                    Owner, e.Graphics, e.Clip, this, DescriptionBounds, Description, sf));
            }
            else
            {
                base.OnPaintText(e);
            }
        }

        public override Size MeasureSize(object sender, RibbonElementMeasureSizeEventArgs e)
        {
            Size s = base.MeasureSize(sender, e);

            s.Height = 52;

            SetLastMeasuredSize(s);

            return s;
        }

        internal override Rectangle OnGetTextBounds(RibbonElementSizeMode sMode, Rectangle bounds)
        {
            Rectangle r = base.OnGetTextBounds(sMode, bounds);
            DescriptionBounds = r;

            r.Height = 20;

            DescriptionBounds = Rectangle.FromLTRB(DescriptionBounds.Left, r.Bottom, DescriptionBounds.Right, DescriptionBounds.Bottom);

            return r;
        }

        #endregion
    }
}

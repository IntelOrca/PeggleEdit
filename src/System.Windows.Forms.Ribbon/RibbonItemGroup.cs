/*
 
 2008 José Manuel Menéndez Poo
 * 
 * Please give me credit if you use this code. It's all I ask.
 * 
 * Contact me for more info: menendezpoo@gmail.com
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
    //[Designer("System.Windows.Forms.RibbonQuickAccessToolbarDesigner")]
    [Designer(typeof(RibbonItemGroupDesigner))]
    public class RibbonItemGroup : RibbonItem, 
        IContainsSelectableRibbonItems, IContainsRibbonComponents
    {
        #region Fields
        private RibbonItemGroupItemCollection _items; 
        private bool _drawBackground;

        #endregion

        #region Ctor
        public RibbonItemGroup()
        {
            _items = new RibbonItemGroupItemCollection(this);
            _drawBackground = true;
        }

        public RibbonItemGroup(IEnumerable<RibbonItem> items)
            : this()
        {
            _items.AddRange(items);
        } 
        #endregion

        #region Props

        /// <summary>
        /// This property is not relevant for this class
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool Checked
        {
            get
            {
                return base.Checked;
            }
            set
            {
                base.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the group should
        /// </summary>
        [DefaultValue(true)]
        [Description("Background drawing should be avoided when group contains only TextBoxes and ComboBoxes")]
        public bool DrawBackground
        {
            get { return _drawBackground; }
            set { _drawBackground = value; }
        }

        /// <summary>
        /// Gets the first item of the group
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonItem FirstItem
        {
            get
            {
                if (Items.Count > 0)
                {
                    return Items[0];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the last item of the group
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonItem LastItem
        {
            get
            {
                if (Items.Count > 0)
                {
                    return Items[Items.Count - 1];
                }

                return null;
            }
        }


        /// <summary>
        /// Gets the collection of items of this group
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Content)]
        public RibbonItemGroupItemCollection Items
        {
            get
            {
                return _items;
            }
        } 

        #endregion

        #region Methods

        public override void SetBounds(Rectangle bounds)
        {
            base.SetBounds(bounds);

            int curLeft = bounds.Left;

            foreach (RibbonItem item in Items)
            {
                item.SetBounds(new Rectangle(new Point(curLeft, bounds.Top), item.LastMeasuredSize));

                curLeft = item.Bounds.Right + 1;
            }

        }

        public override void OnPaint(object sender, RibbonElementPaintEventArgs e)
        {
            if (DrawBackground)
            {
                Owner.Renderer.OnRenderRibbonItem(new RibbonItemRenderEventArgs(Owner, e.Graphics, e.Clip, this));
            }

            foreach (RibbonItem item in Items)
            {
                item.OnPaint(this, new RibbonElementPaintEventArgs(item.Bounds, e.Graphics, RibbonElementSizeMode.Compact));
            }

            if (DrawBackground)
            {
                Owner.Renderer.OnRenderRibbonItemBorder(new RibbonItemRenderEventArgs(Owner, e.Graphics, e.Clip, this)); 
            }
        }

        public override Size MeasureSize(object sender, RibbonElementMeasureSizeEventArgs e)
        {
            ///For RibbonItemGroup, size is always compact, and it's designed to be on an horizontal flow
            ///tab panel.
            ///
            int minWidth = 16;
            int widthSum = 0;
            int maxHeight = 16;

            foreach (RibbonItem item in Items)
            {
                Size s = item.MeasureSize(this, new RibbonElementMeasureSizeEventArgs(e.Graphics, RibbonElementSizeMode.Compact));
                widthSum += s.Width + 1;
                maxHeight = Math.Max(maxHeight, s.Height);
            }

            widthSum -= 1;

            widthSum = Math.Max(widthSum, minWidth);

            if (Site != null && Site.DesignMode)
            {
                widthSum += 10;
            }

            Size result = new Size(widthSum, maxHeight);
            SetLastMeasuredSize(result);
            return result;
        }

        /// <param name="ownerPanel">RibbonPanel where this item is located</param>
        internal override void SetOwnerPanel(System.Windows.Forms.RibbonPanel ownerPanel)
        {
            base.SetOwnerPanel(ownerPanel);

            Items.SetOwnerPanel(ownerPanel);
        }

        /// <param name="owner">Ribbon that owns this item</param>
        internal override void SetOwner(System.Windows.Forms.Ribbon owner)
        {
            base.SetOwner(owner);

            Items.SetOwner(owner);
        }

        /// <param name="ownerTab">RibbonTab where this item is located</param>
        internal override void SetOwnerTab(System.Windows.Forms.RibbonTab ownerTab)
        {
            base.SetOwnerTab(ownerTab);

            Items.SetOwnerTab(ownerTab);
        }

        internal override void SetSizeMode(RibbonElementSizeMode sizeMode)
        {
            base.SetSizeMode(sizeMode);

            foreach (RibbonItem item in Items)
            {
                item.SetSizeMode(RibbonElementSizeMode.Compact);
            }
        }

        public override string ToString()
        {
            return "Group: " + Items.Count + " item(s)";
        } 
        #endregion

        #region IContainsRibbonItems Members

        public IEnumerable<RibbonItem> GetItems()
        {
            return Items;
        }

        public Rectangle GetContentBounds()
        {
            return Rectangle.FromLTRB(Bounds.Left + 1, Bounds.Top + 1, Bounds.Right - 1, Bounds.Bottom);
        }

        #endregion

        #region IContainsRibbonComponents Members

        public IEnumerable<Component> GetAllChildComponents()
        {
            return Items.ToArray();
        }

        #endregion
    }
}

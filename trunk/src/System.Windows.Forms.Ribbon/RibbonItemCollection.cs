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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel.Design;

namespace System.Windows.Forms
{
    [Editor("System.Windows.Forms.RibbonItemCollectionEditor", typeof(UITypeEditor))]
    public class RibbonItemCollection
        : List<RibbonItem>
    {
        #region Fields
        private Ribbon _owner;
        private RibbonTab _ownerTab;
        private RibbonPanel _ownerPanel; 

        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new ribbon item collection
        /// </summary>
        internal RibbonItemCollection()
        {

        }

        #endregion

        #region Properties



        /// <summary>
        /// Gets the Ribbon owner of this collection
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Ribbon Owner
        {
            get
            {
                return _owner;
            }
        }

        /// <summary>
        /// Gets the RibbonPanel where this item is located
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonPanel OwnerPanel
        {
            get
            {
                return _ownerPanel;
            }
        }

        /// <summary>
        /// Gets the RibbonTab that contains this item
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonTab OwnerTab
        {
            get
            {
                return _ownerTab;
            }
        } 
        #endregion

        #region Overrides
        /// <summary>
        /// Adds the specified item to the collection
        /// </summary>
        public new void Add(RibbonItem item)
        {
            item.SetOwner(Owner);
            item.SetOwnerPanel(OwnerPanel);
            item.SetOwnerTab(OwnerTab);

            base.Add(item);
        }

        /// <summary>
        /// Adds the specified range of items
        /// </summary>
        /// <param name="items">Items to add</param>
        public new void AddRange(IEnumerable<RibbonItem> items)
        {
            foreach (RibbonItem item in items)
            {
                item.SetOwner(Owner);
                item.SetOwnerPanel(OwnerPanel);
                item.SetOwnerTab(OwnerTab);
            }

            base.AddRange(items);
        }

        /// <summary>
        /// Inserts the specified item at the desired index
        /// </summary>
        /// <param name="index">Desired index of the item</param>
        /// <param name="item">Item to insert</param>
        public new void Insert(int index, RibbonItem item)
        {
            item.SetOwner(Owner);
            item.SetOwnerPanel(OwnerPanel);
            item.SetOwnerTab(OwnerTab);

            base.Insert(index, item);
        } 
        #endregion

        #region Methods

        /// <summary>
        /// Gets the left of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsLeft(IEnumerable<RibbonItem> items)
        {
            if (Count == 0) return 0;

            int min = int.MaxValue;

            foreach (RibbonItem item in items)
            {
                if (item.Bounds.X < min)
                {
                    min = item.Bounds.X;
                }
            }

            return min;
        }

        /// <summary>
        /// Gets the right of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsRight(IEnumerable<RibbonItem> items)
        {
            if (Count == 0) return 0;

            int max = int.MinValue; ;

            foreach (RibbonItem item in items)
            {
                if (item.Bounds.Right > max)
                {
                    max = item.Bounds.Right;
                }
            }

            return max;
        }

        /// <summary>
        /// Gets the top of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsTop(IEnumerable<RibbonItem> items)
        {
            if (Count == 0) return 0;

            int min = int.MaxValue;

            foreach (RibbonItem item in items)
            {
                if (item.Bounds.Y < min)
                {
                    min = item.Bounds.Y;
                }
            }

            return min;
        }

        /// <summary>
        /// Gets the bottom of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsBottom(IEnumerable<RibbonItem> items)
        {
            if (Count == 0) return 0;

            int max = int.MinValue;

            foreach (RibbonItem item in items)
            {
                if (item.Bounds.Bottom > max)
                {
                    max = item.Bounds.Bottom;
                }
            }

            return max;
        }

        /// <summary>
        /// Gets the width of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsWidth(IEnumerable<RibbonItem> items)
        {
            return GetItemsRight(items) - GetItemsLeft(items);
        }

        /// <summary>
        /// Gets the height of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsHeight(IEnumerable<RibbonItem> items)
        {
            return GetItemsBottom(items) - GetItemsTop(items);
        }

        /// <summary>
        /// Gets the bounds of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal Rectangle GetItemsBounds(IEnumerable<RibbonItem> items)
        {
            return Rectangle.FromLTRB(GetItemsLeft(items), GetItemsTop(items), GetItemsRight(items), GetItemsBottom(items));
        }

        /// <summary>
        /// Gets the left of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsLeft()
        {
            if (Count == 0) return 0;

            int min = int.MaxValue;

            foreach (RibbonItem item in this)
            {
                if (item.Bounds.X < min)
                {
                    min = item.Bounds.X;
                }
            }

            return min;
        }

        /// <summary>
        /// Gets the right of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsRight()
        {
            if (Count == 0) return 0;

            int max = int.MinValue; ;

            foreach (RibbonItem item in this)
            {
                if (item.Bounds.Right > max)
                {
                    max = item.Bounds.Right;
                }
            }

            return max;
        }

        /// <summary>
        /// Gets the top of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsTop()
        {
            if (Count == 0) return 0;

            int min = int.MaxValue;

            foreach (RibbonItem item in this)
            {
                if (item.Bounds.Y < min)
                {
                    min = item.Bounds.Y;
                }
            }

            return min;
        }

        /// <summary>
        /// Gets the bottom of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsBottom()
        {
            if (Count == 0) return 0;

            int max = int.MinValue;

            foreach (RibbonItem item in this)
            {
                if (item.Bounds.Bottom > max)
                {
                    max = item.Bounds.Bottom;
                }
            }

            return max;
        }

        /// <summary>
        /// Gets the width of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsWidth()
        {
            return GetItemsRight() - GetItemsLeft();
        }

        /// <summary>
        /// Gets the height of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal int GetItemsHeight()
        {
            return GetItemsBottom() - GetItemsTop();
        }

        /// <summary>
        /// Gets the bounds of items as a group of shapes
        /// </summary>
        /// <returns></returns>
        internal Rectangle GetItemsBounds()
        {
            return Rectangle.FromLTRB(GetItemsLeft(), GetItemsTop(), GetItemsRight(), GetItemsBottom());
        }

        /// <summary>
        /// Moves the bounds of items as a group of shapes
        /// </summary>
        /// <param name="p"></param>
        internal void MoveTo(Point p)
        {
            MoveTo(this, p);
        }

        /// <summary>
        /// Moves the bounds of items as a group of shapes
        /// </summary>
        /// <param name="p"></param>
        internal void MoveTo(IEnumerable<RibbonItem> items, Point p)
        {
            Rectangle oldBounds = GetItemsBounds(items);

            foreach (RibbonItem item in items)
            {
                int dx = item.Bounds.X - oldBounds.Left;
                int dy = item.Bounds.Y - oldBounds.Top;

                item.SetBounds(new Rectangle(new Point(p.X + dx, p.Y + dy), item.Bounds.Size));
            }
        }

        /// <summary>
        /// Centers the items on the specified rectangle
        /// </summary>
        /// <param name="rectangle"></param>
        internal void CenterItemsInto(Rectangle rectangle)
        {
            CenterItemsInto(this, rectangle);
        }

        /// <summary>
        /// Centers the items vertically on the specified rectangle
        /// </summary>
        /// <param name="rectangle"></param>
        internal void CenterItemsVerticallyInto(Rectangle rectangle)
        {
            CenterItemsVerticallyInto(this, rectangle);
        }

        /// <summary>
        /// Centers the items horizontally on the specified rectangle
        /// </summary>
        /// <param name="rectangle"></param>
        internal void CenterItemsHorizontallyInto(Rectangle rectangle)
        {
            CenterItemsHorizontallyInto(this, rectangle);
        }

        /// <summary>
        /// Centers the items on the specified rectangle
        /// </summary>
        /// <param name="rectangle"></param>
        internal void CenterItemsInto(IEnumerable<RibbonItem> items, Rectangle rectangle)
        {
            int x = rectangle.Left + (rectangle.Width - GetItemsWidth()) / 2;
            int y = rectangle.Top + (rectangle.Height - GetItemsHeight()) / 2;
            
            MoveTo(items, new Point(x, y));

        }

        /// <summary>
        /// Centers the items vertically on the specified rectangle
        /// </summary>
        /// <param name="rectangle"></param>
        internal void CenterItemsVerticallyInto(IEnumerable<RibbonItem> items, Rectangle rectangle)
        {
            int x = GetItemsLeft(items);
            int y = rectangle.Top + (rectangle.Height - GetItemsHeight(items)) / 2;

            MoveTo(items, new Point(x, y));
        }

        /// <summary>
        /// Centers the items horizontally on the specified rectangle
        /// </summary>
        /// <param name="rectangle"></param>
        internal void CenterItemsHorizontallyInto(IEnumerable<RibbonItem> items, Rectangle rectangle)
        {
            int x = rectangle.Left + (rectangle.Width - GetItemsWidth(items)) / 2;
            int y = GetItemsTop(items);

            MoveTo(items, new Point(x, y));
        }

        /// <summary>
        /// Sets the owner Ribbon of the collection
        /// </summary>
        /// <param name="owner"></param>
        internal void SetOwner(Ribbon owner)
        {
            _owner = owner;

            foreach (RibbonItem item in this)
            {
                item.SetOwner(owner);
            }
        }

        /// <summary>
        /// Sets the owner Tab of the collection
        /// </summary>
        /// <param name="tab"></param>
        internal void SetOwnerTab(RibbonTab tab)
        {
            _ownerTab = tab;

            foreach (RibbonItem item in this)
            {
                item.SetOwnerTab(tab);
            }
        }

        /// <summary>
        /// Sets the owner panel of the collection
        /// </summary>
        /// <param name="panel"></param>
        internal void SetOwnerPanel(RibbonPanel panel)
        {
            _ownerPanel = panel;

            foreach (RibbonItem item in this)
            {
                item.SetOwnerPanel(panel);
            }
        }

        #endregion
    }
}

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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;

namespace System.Windows.Forms
{
    [ToolboxItem(false)]
    public partial class RibbonDropDown 
        : RibbonPopup
    {
        #region Static

        //private static List<RibbonDropDown> registeredDds = new List<RibbonDropDown>();

        //private static void RegisterDropDown(RibbonDropDown dropDown)
        //{
        //    registeredDds.Add(dropDown);
        //}

        //private static void UnregisterDropDown(RibbonDropDown dropDown)
        //{
        //    registeredDds.Remove(dropDown);
        //}

        //internal static void DismissAll()
        //{
        //    for (int i = 0; i < registeredDds.Count; i++)
        //    {
                
        //        registeredDds[i].Close();
        //    }

        //    registeredDds.Clear();
        //}

        ///// <summary>
        ///// Closes all the dropdowns before the specified dropDown
        ///// </summary>
        ///// <param name="dropDown"></param>
        //internal static void DismissTo(RibbonDropDown dropDown)
        //{
        //    if (dropDown == null) throw new ArgumentNullException("dropDown");

        //    for (int i = registeredDds.Count - 1; i >= 0; i--)
        //    {
        //        if (i >= registeredDds.Count)
        //        {
        //            break;
        //        }

        //        if (registeredDds[i].Equals(dropDown))
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            registeredDds[i].Close();
        //        }
        //    }
        //}

        #endregion

        #region Fields
        private IEnumerable<RibbonItem> _items;
        private bool _showSizingGrip;
        private int _sizingGripHeight;
        private Ribbon _ownerRibbon;
        private RibbonMouseSensor _sensor;
        private RibbonItem _parentItem;
        private bool _ignoreNext;
        private RibbonElementSizeMode _MeasuringSize;
        private bool _resizing;
        private Rectangle _sizingGripBounds;
        private Point _resizeOrigin;
        private Size _resizeSize;
        private ISelectionService _SelectionService;
        private bool _iconsBar;

        #endregion

        #region Ctor

        private RibbonDropDown()
        {
            //RegisterDropDown(this);
            DoubleBuffered = true;
            DrawIconsBar = true;
        }

        internal RibbonDropDown( RibbonItem parentItem, IEnumerable<RibbonItem> items, Ribbon ownerRibbon)
            : this(parentItem, items, ownerRibbon, RibbonElementSizeMode.DropDown)
        {
        }

        internal RibbonDropDown(RibbonItem parentItem, IEnumerable<RibbonItem> items, Ribbon ownerRibbon, RibbonElementSizeMode measuringSize)
            : this()
        {
            _items = items;
            _ownerRibbon = ownerRibbon;
            _sizingGripHeight = 12;
            _parentItem = parentItem;
            _sensor = new RibbonMouseSensor(this, OwnerRibbon, items);
            _MeasuringSize = measuringSize;

            if (Items != null)
                foreach (RibbonItem item in Items)
                {
                    item.SetSizeMode(RibbonElementSizeMode.DropDown);
                    item.SetCanvas(this);
                }

            UpdateSize();
        } 

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if the icons bar should be drawn
        /// </summary>
        public bool DrawIconsBar
        {
            get { return _iconsBar; }
            set { _iconsBar = value; }
        }


        protected override void OnOpening(CancelEventArgs e)
        {
            base.OnOpening(e);

            UpdateItemsBounds();
        }

        /// <summary>
        /// Gets or sets the selection service for the dropdown
        /// </summary>
        internal ISelectionService SelectionService
        {
            get { return _SelectionService; }
            set { _SelectionService = value; }
        }

        /// <summary>
        /// Gets the bounds of the sizing grip
        /// </summary>
        public Rectangle SizingGripBounds
        {
            get { return _sizingGripBounds; }
        }

        /// <summary>
        /// Gets or sets the size for measuring items (by default is DropDown)
        /// </summary>
        public RibbonElementSizeMode MeasuringSize
        {
            get { return _MeasuringSize; }
            set { _MeasuringSize = value; }
        }

        /// <summary>
        /// Gets the parent item of this dropdown
        /// </summary>
        public RibbonItem ParentItem
        {
            get { return _parentItem; }
        }

        /// <summary>
        /// Gets the sennsor of this dropdown
        /// </summary>
        public RibbonMouseSensor Sensor
        {
            get { return _sensor; }
        }

        /// <summary>
        /// Gets the Ribbon this DropDown belongs to
        /// </summary>
        public Ribbon OwnerRibbon
        {
            get { return _ownerRibbon; }
        }

        /// <summary>
        /// Gets the RibbonItem this dropdown belongs to
        /// </summary>
        public IEnumerable<RibbonItem> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the sizing grip should be visible
        /// </summary>
        public bool ShowSizingGrip
        {
            get { return _showSizingGrip; }
            set 
            { 
                _showSizingGrip = value;
                UpdateSize();
                UpdateItemsBounds();
            }
        }

        /// <summary>
        /// Gets or sets the height of the sizing grip area
        /// </summary>
        [DefaultValue(12)]
        public int SizingGripHeight
        {
            get { return _sizingGripHeight; }
            set { _sizingGripHeight = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prevents the form from being hidden the next time the mouse clicks on the form.
        /// It is useful for reacting to clicks of items inside items.
        /// </summary>
        public void IgnoreNextClickDeactivation()
        {
            _ignoreNext = true;
        }

        /// <summary>
        /// Updates the size of the dropdown
        /// </summary>
        private void UpdateSize()
        {
            int heightSum = OwnerRibbon.DropDownMargin.Vertical;
            int maxWidth = 0;
            int scrollableHeight = 0;

            using (Graphics g = CreateGraphics())
            {
                foreach (RibbonItem item in Items)
                {
                    Size s = item.MeasureSize(this, new RibbonElementMeasureSizeEventArgs(g, MeasuringSize));

                    heightSum += s.Height;
                    maxWidth = Math.Max(maxWidth, s.Width);

                    if (item is IScrollableRibbonItem)
                    {
                        scrollableHeight += s.Height;
                    }
                } 
            }

            Size sz = new Size(maxWidth + OwnerRibbon.DropDownMargin.Horizontal, heightSum  + (ShowSizingGrip? SizingGripHeight + 2 : 0));

            Size = sz;

            if (WrappedDropDown != null)
            {
                WrappedDropDown.Size = Size;
            }
        }

        /// <summary>
        /// Updates the bounds of the items
        /// </summary>
        private void UpdateItemsBounds()
        {
            int curTop = OwnerRibbon.DropDownMargin.Top;
            int curLeft = OwnerRibbon.DropDownMargin.Left;
            int itemsWidth = ClientSize.Width - OwnerRibbon.DropDownMargin.Horizontal;

            int scrollableItemsHeight = 0;
            int nonScrollableItemsHeight = 0;
            int scrollableItems = 0;
            int scrollableItemHeight = 0;

            #region Measure scrollable content
            foreach (RibbonItem item in Items)
            {
                if (item is IScrollableRibbonItem)
                {
                    scrollableItemsHeight += item.LastMeasuredSize.Height;
                    scrollableItems++;
                }
                else
                {
                    nonScrollableItemsHeight += item.LastMeasuredSize.Height;
                }
            }

            if (scrollableItems > 0)
            {
                scrollableItemHeight = (Height - nonScrollableItemsHeight - (ShowSizingGrip ? SizingGripHeight : 0)) / scrollableItems;
            }

            #endregion

            foreach (RibbonItem item in Items)
            {
                if (item is IScrollableRibbonItem)
                {
                    item.SetBounds(new Rectangle(curLeft, curTop, itemsWidth, scrollableItemHeight - 1));
                }
                else
                {
                    item.SetBounds(new Rectangle(curLeft, curTop, itemsWidth, item.LastMeasuredSize.Height));
                }

                curTop += item.Bounds.Height;
            }

            if (ShowSizingGrip)
            {
                _sizingGripBounds = Rectangle.FromLTRB(
                    ClientSize.Width - SizingGripHeight, ClientSize.Height - SizingGripHeight,
                    ClientSize.Width, ClientSize.Height);
            }
            else
            {
                _sizingGripBounds = Rectangle.Empty;
            }
        }
        
        #endregion

        #region Overrides

        protected override void OnShowed(EventArgs e)
        {
            base.OnShowed(e);

            foreach (RibbonItem item in Items)
            {
                item.SetSelected(false);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (Cursor == Cursors.SizeNWSE)
            {
                _resizeOrigin = new Point(e.X, e.Y);
                _resizeSize = Size;
                _resizing = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (ShowSizingGrip && SizingGripBounds.Contains(e.X, e.Y))
            {
                Cursor = Cursors.SizeNWSE;
            }
            else if (Cursor == Cursors.SizeNWSE)
            {
                Cursor = Cursors.Default;
            }

            if (_resizing)
            {
                int dx = e.X - _resizeOrigin.X;
                int dy = e.Y - _resizeOrigin.Y;

                int w = _resizeSize.Width + dx;
                int h = _resizeSize.Height + dy;

                if (w != Width || h != Height)
                {
                    Size = new Size(w, h);
                    if (WrappedDropDown != null)
                    {
                        WrappedDropDown.Size = Size;
                    }
                    UpdateItemsBounds();
                    //Invalidate();
                }
                
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_resizing)
            {
                _resizing = false;
                return;
            }

            if (_ignoreNext)
            {
                _ignoreNext = false;
                return;
            }

            if(RibbonDesigner.Current != null)
                Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            OwnerRibbon.Renderer.OnRenderDropDownBackground(
                new RibbonCanvasEventArgs(OwnerRibbon, e.Graphics, new Rectangle(Point.Empty, ClientSize), this, ParentItem));

            foreach (RibbonItem item in Items)
            {
                item.OnPaint(this, new RibbonElementPaintEventArgs(item.Bounds, e.Graphics, RibbonElementSizeMode.DropDown));
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            foreach (RibbonItem item in Items)
            {
                item.SetSelected(false);
            }
        }

        #endregion

        

    }
}
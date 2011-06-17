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
    /// <summary>
    /// Represents a list of buttons that can be navigated
    /// </summary>
    [Designer(typeof(RibbonButtonListDesigner))]
    public sealed class RibbonButtonList : RibbonItem, 
        IContainsSelectableRibbonItems, IScrollableRibbonItem, IContainsRibbonComponents
    {
        #region Subtypes

        public enum ListScrollType
        {
            UpDownButtons,

            Scrollbar
        }

        #endregion

        #region Fields
        private RibbonButtonCollection _buttons;
        private int _itemsInLargeMode;
        private int _itemsInMediumMode;
        private Size _ItemsInDropwDownMode;
        private Rectangle _buttonUpBounds;
        private Rectangle _buttonDownBounds;
        private Rectangle _buttonDropDownBounds;
        private Rectangle _contentBounds;
        private int _controlButtonsWidth;
        private RibbonElementSizeMode _buttonsSizeMode;
        private int _jumpDownSize;
        private int _jumpUpSize;
        private int _offset;
        private bool _buttonDownSelected;
        private bool _buttonDownPressed;
        private bool _buttonUpSelected;
        private bool _buttonUpPressed;
        private bool _buttonDropDownSelected;
        private bool _buttonDropDownPressed;
        private bool _buttonUpEnabled;
        private bool _buttonDownEnabled;
        private RibbonDropDown _dropDown;
        private bool _dropDownVisible;
        private RibbonItemCollection _dropDownItems;
        private Rectangle _thumbBounds;
        private bool _thumbSelected;
        private bool _thumbPressed;
        private int _scrollValue;
        private Rectangle fullContentBounds;
        private ListScrollType _scrollType;
        private bool _scrollBarEnabled;
        private int _thumbOffset;
        private bool _avoidNextThumbMeasure;
        private bool _flowToBottom;
        #endregion

        #region Ctor

        public RibbonButtonList()
        {
            _buttons = new RibbonButtonCollection(this);
            _dropDownItems = new RibbonItemCollection();

            _controlButtonsWidth = 16;
            _itemsInLargeMode = 7;
            _itemsInMediumMode = 3;
            _ItemsInDropwDownMode = new Size(7, 5);
            _buttonsSizeMode = RibbonElementSizeMode.Large;
            _scrollType = ListScrollType.UpDownButtons;
            
        }

        public RibbonButtonList(IEnumerable<RibbonButton> buttons)
            : this(buttons, null)
        {
        }

        public RibbonButtonList(IEnumerable<RibbonButton> buttons, IEnumerable<RibbonItem> dropDownItems)
            : this()
        {
            if (buttons != null)
            {
                List<RibbonButton> items = new List<RibbonButton>(buttons);

                _buttons.AddRange(items.ToArray()); 
            }

            if (dropDownItems != null)
            {
                _dropDownItems.AddRange(dropDownItems);
            }
        }

        #endregion

        #region Props


        [Description("If activated, buttons will flow to bottom inside the list")]
        public bool FlowToBottom
        {
            get { return _flowToBottom; }
            set { _flowToBottom = value; }
        }


        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ScrollBarBounds
        {
            get
            {
                return Rectangle.FromLTRB(ButtonUpBounds.Left, ButtonUpBounds.Top, ButtonDownBounds.Right, ButtonDownBounds.Bottom);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ScrollBarEnabled
        {
            get { return _scrollBarEnabled; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ListScrollType ScrollType
        {
            get { return _scrollType; }
        }

        /// <summary>
        /// Gets the percent of scrolled content
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double ScrolledPercent
        {
            get 
            {
                return ((double)ContentBounds.Top - (double)fullContentBounds.Top) /
                    ((double)fullContentBounds.Height - (double)ContentBounds.Height); 
            }
            set
            {
                _avoidNextThumbMeasure = true;
                ScrollTo(-Convert.ToInt32((double)(fullContentBounds.Height - ContentBounds.Height ) * value));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollMinimum
        {
            get 
            {
                if (ScrollType == ListScrollType.Scrollbar)
                {
                    return ButtonUpBounds.Bottom;
                }
                else
                {
                    return 0;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollMaximum
        {
            get
            {
                if (ScrollType == ListScrollType.Scrollbar)
                {
                    return ButtonDownBounds.Top - ThumbBounds.Height;
                }
                else
                {
                    return 0;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollValue
        {
            get { return _scrollValue; }
            set 
            {
                if (value > ScrollMaximum || value < ScrollMinimum)
                {
                    throw new IndexOutOfRangeException("Scroll value must exist between ScrollMinimum and Scroll Maximum");
                }

                _thumbBounds.Y = value;

                double scrolledPixels = value - ScrollMinimum;
                double pixelsAvailable = ScrollMaximum - ScrollMinimum;

                ScrolledPercent = scrolledPixels / pixelsAvailable;

                _scrollValue = value;
            }
        }

        /// <summary>
        /// Redraws the scroll part of the list
        /// </summary>
        private void RedrawScroll()
        {
            if (Canvas != null) 
                Canvas.Invalidate(Rectangle.FromLTRB(ButtonDownBounds.X, ButtonUpBounds.Y, ButtonDownBounds.Right, ButtonDownBounds.Bottom));
        }

        /// <summary>
        /// Gets if the scrollbar thumb is currently selected
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ThumbSelected
        {
            get { return _thumbSelected; }
        }

        /// <summary>
        /// Gets if the scrollbar thumb is currently pressed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ThumbPressed
        {
            get { return _thumbPressed; }
        }


        /// <summary>
        /// Gets the bounds of the scrollbar thumb
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ThumbBounds
        {
            get 
            {
                return _thumbBounds; 
            }
        }


        /// <summary>
        /// Gets if the DropDown button is present on thelist
        /// </summary>
        public bool ButtonDropDownPresent
        {
            get 
            {
                return ButtonDropDownBounds.Height > 0;
            }
        }

        /// <summary>
        /// Gets the collection of items shown on the dropdown pop-up when Style allows it
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RibbonItemCollection DropDownItems
        {
            get
            {
                return _dropDownItems;
            }
        }

        /// <summary>
        /// Gets or sets the size that the buttons on the list should be
        /// </summary>
        public RibbonElementSizeMode ButtonsSizeMode
        {
            get { return _buttonsSizeMode; }
            set { _buttonsSizeMode = value; if (Owner != null) Owner.OnRegionsChanged(); }
        }

        /// <summary>
        /// Gets a value indicating if the button that scrolls up the content is currently enabled
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ButtonUpEnabled
        {
            get { return _buttonUpEnabled; }
        }

        /// <summary>
        /// Gets a value indicating if the button that scrolls down the content is currently enabled
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ButtonDownEnabled
        {
            get { return _buttonDownEnabled; }
        }

        /// <summary>
        /// Gets a value indicating if the DropDown button is currently selected
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ButtonDropDownSelected
        {
            get { return _buttonDropDownSelected; }
        }

        /// <summary>
        /// Gets a value indicating if the DropDown button is currently pressed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ButtonDropDownPressed
        {
            get { return _buttonDropDownPressed; }
        }

        /// <summary>
        /// Gets a vaule indicating if the button that scrolls down the content is currently selected
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ButtonDownSelected
        {
            get { return _buttonDownSelected; }
        }

        /// <summary>
        /// Gets a vaule indicating if the button that scrolls down the content is currently pressed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ButtonDownPressed
        {
            get { return _buttonDownPressed; }
        }

        /// <summary>
        /// Gets a vaule indicating if the button that scrolls up the content is currently selected
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ButtonUpSelected
        {
            get { return _buttonUpSelected; }
        }

        /// <summary>
        /// Gets a vaule indicating if the button that scrolls up the content is currently pressed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ButtonUpPressed
        {
            get { return _buttonUpPressed; }
        }

        /// <summary>
        /// Gets the bounds of the content where items are shown
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Rectangle ContentBounds
        {
            get { return _contentBounds; }
        }

        /// <summary>
        /// Gets the bounds of the button that scrolls the items up
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ButtonUpBounds
        {
            get { return _buttonUpBounds; }
        }

        /// <summary>
        /// Gets the bounds of the button that scrolls the items down
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ButtonDownBounds
        {
            get { return _buttonDownBounds; }
        }

        /// <summary>
        /// Gets the bounds of the button that scrolls
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ButtonDropDownBounds
        {
            get { return _buttonDropDownBounds; }
        }

        /// <summary>
        /// Gets or sets the with of the buttons that allow to navigate thru the list
        /// </summary>
        [DefaultValue(16)]
        public int ControlButtonsWidth
        {
            get { return _controlButtonsWidth; }
            set { _controlButtonsWidth = value; if (Owner != null) Owner.OnRegionsChanged(); }
        }

        /// <summary>
        /// Gets or sets a value indicating the amount of items to show
        /// (wide) when SizeMode is Large 
        /// </summary>
        [DefaultValue(7)]
        public int ItemsWideInLargeMode
        {
            get { return _itemsInLargeMode; }
            set { _itemsInLargeMode = value; if (Owner != null) Owner.OnRegionsChanged(); }
        }

        /// <summary>
        /// Gets or sets a value indicating the amount of items to show
        /// (wide) when SizeMode is Medium
        /// </summary>
        [DefaultValue(3)]
        public int ItemsWideInMediumMode
        {
            get { return _itemsInMediumMode; }
            set { _itemsInMediumMode = value; if (Owner != null) Owner.OnRegionsChanged(); }
        }

        /// <summary>
        /// Gets or sets a value indicating the amount of items to show
        /// (wide) when SizeMode is Medium
        /// </summary>
        public Size ItemsSizeInDropwDownMode
        {
            get { return _ItemsInDropwDownMode; }
            set { _ItemsInDropwDownMode = value; if (Owner != null) Owner.OnRegionsChanged(); }
        }

        /// <summary>
        /// Gets the collection of buttons of the list
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RibbonButtonCollection Buttons
        {
            get
            {
                return _buttons;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Ignores deactivation of canvas if it is a volatile window
        /// </summary>
        private void IgnoreDeactivation()
        {
            if (Canvas is RibbonPanelPopup)
            {
                (Canvas as RibbonPanelPopup).IgnoreNextClickDeactivation();
            }

            if (Canvas is RibbonDropDown)
            {
                (Canvas as RibbonDropDown).IgnoreNextClickDeactivation();
            }
        }

        /// <summary>
        /// Redraws the control buttons: up, down and dropdown
        /// </summary>
        private void RedrawControlButtons()
        {
            if (Canvas != null)
            {
                if (ScrollType == ListScrollType.Scrollbar)
                {
                    Canvas.Invalidate(ScrollBarBounds);
                }
                else
                {
                    Canvas.Invalidate(Rectangle.FromLTRB(ButtonUpBounds.Left, ButtonUpBounds.Top, ButtonDropDownBounds.Right, ButtonDropDownBounds.Bottom));
                } 
            }
            
        }

        /// <summary>
        /// Pushes the amount of _offset of the top of items
        /// </summary>
        /// <param name="amount"></param>
        private void ScrollOffset(int amount)
        {
            ScrollTo(_offset + amount);
        }

        /// <summary>
        /// Scrolls the content to the specified offset
        /// </summary>
        /// <param name="offset"></param>
        private void ScrollTo(int offset)
        {
            int minOffset = ContentBounds.Y - fullContentBounds.Height + ContentBounds.Height;

            if (offset < minOffset)
            {
                offset = minOffset;
            }

            _offset = offset;
            SetBounds(Bounds);
            RedrawItem();
        }

        /// <summary>
        /// Scrolls the list down
        /// </summary>
        public void ScrollDown()
        {
            ScrollOffset(-(_jumpDownSize + 1));
        }

        /// <summary>
        /// Scrolls the list up
        /// </summary>
        public void ScrollUp()
        {
            ScrollOffset(_jumpDownSize + 1);
        }

        /// <summary>
        /// Shows the drop down items of the button, as if the dropdown part has been clicked
        /// </summary>
        public void ShowDropDown()
        {
            if (DropDownItems.Count == 0)
            {
                SetPressed(false);
                return;
            }

            IgnoreDeactivation();

            _dropDown = new RibbonDropDown(this, DropDownItems, Owner);
            //_dropDown.FormClosed += new FormClosedEventHandler(dropDown_FormClosed);
            //_dropDown.StartPosition = FormStartPosition.Manual;
            _dropDown.ShowSizingGrip = true;
            Point location = Canvas.PointToScreen(new Point(Bounds.Left, Bounds.Top));
            

            SetDropDownVisible(true);
            _dropDown.Show(location);

        }

        private void dropDown_FormClosed(object sender, FormClosedEventArgs e)
        {
            SetDropDownVisible(false);
        }

        /// <summary>
        /// Closes the DropDown if opened
        /// </summary>
        public void CloseDropDown()
        {
            if (_dropDown != null)
            {
                //RibbonDropDown.DismissTo(_dropDown);
            }

            SetDropDownVisible(false);
        }

        /// <summary>
        /// Sets the value of DropDownVisible
        /// </summary>
        /// <param name="visible"></param>
        internal void SetDropDownVisible(bool visible)
        {
            _dropDownVisible = visible;
        }

        #endregion

        #region Overrides
        public override void OnCanvasChanged(EventArgs e)
        {
            base.OnCanvasChanged(e);

            if (Canvas is RibbonDropDown)
            {
                _scrollType = ListScrollType.Scrollbar;
            }
            else
            {
                _scrollType = ListScrollType.UpDownButtons;
            }
        }


        protected override bool ClosesDropDownAt(Point p)
        {
            return !(
                ButtonDropDownBounds.Contains(p) ||
                ButtonDownBounds.Contains(p) ||
                ButtonUpBounds.Contains(p) ||
                (ScrollType == ListScrollType.Scrollbar && ScrollBarBounds.Contains(p))
                );
        }

        internal override void SetOwner(Ribbon owner)
        {
            base.SetOwner(owner);

            _buttons.SetOwner(owner);
            _dropDownItems.SetOwner(owner);
        }

        internal override void SetOwnerPanel(RibbonPanel ownerPanel)
        {
            base.SetOwnerPanel(ownerPanel);

            _buttons.SetOwnerPanel(ownerPanel);
            _dropDownItems.SetOwnerPanel(ownerPanel);
        }

        internal override void SetOwnerTab(RibbonTab ownerTab)
        {
            base.SetOwnerTab(ownerTab);

            _buttons.SetOwnerTab(ownerTab);
            _dropDownItems.SetOwnerTab(OwnerTab);
        }

        public override void OnPaint(object sender, RibbonElementPaintEventArgs e)
        {

            Owner.Renderer.OnRenderRibbonItem(new RibbonItemRenderEventArgs(Owner, e.Graphics, e.Clip, this));
            
            if (e.Mode != RibbonElementSizeMode.Compact)
            {
                Region lastClip = e.Graphics.Clip;
                Region newClip = new Region(lastClip.GetBounds(e.Graphics));
                newClip.Intersect(ContentBounds);
                e.Graphics.SetClip(newClip.GetBounds(e.Graphics));

                foreach (RibbonButton button in Buttons)
                {
                    if(!button.Bounds.IsEmpty)
                    button.OnPaint(this, new RibbonElementPaintEventArgs(button.Bounds, e.Graphics, ButtonsSizeMode));
                }
                e.Graphics.SetClip(lastClip.GetBounds(e.Graphics));
            }
        }

        public override void SetBounds(System.Drawing.Rectangle bounds)
        {
            base.SetBounds(bounds);

            #region Assign control buttons bounds

            if (ScrollType != ListScrollType.Scrollbar)
            {
                #region Custom Buttons
                int cbtns = 3; // Canvas is RibbonDropDown ? 2 : 3;
                int buttonHeight = bounds.Height / cbtns;
                int buttonWidth = _controlButtonsWidth;

                _buttonUpBounds = Rectangle.FromLTRB(bounds.Right - buttonWidth,
                    bounds.Top, bounds.Right, bounds.Top + buttonHeight);

                _buttonDownBounds = Rectangle.FromLTRB(_buttonUpBounds.Left, _buttonUpBounds.Bottom,
                    bounds.Right, _buttonUpBounds.Bottom + buttonHeight);

                if (cbtns == 2)
                {
                    _buttonDropDownBounds = Rectangle.Empty;
                }
                else
                {
                    _buttonDropDownBounds = Rectangle.FromLTRB(_buttonDownBounds.Left, _buttonDownBounds.Bottom,
                    bounds.Right, bounds.Bottom + 1);
                }

                _thumbBounds.Location = Point.Empty;

                #endregion
            }
            else
            {
                #region Scrollbar

                int bwidth = ThumbBounds.Width;
                int bheight = ThumbBounds.Width;

                _buttonUpBounds = Rectangle.FromLTRB(bounds.Right - bwidth,
                    bounds.Top, bounds.Right, bounds.Top + bheight);

                _buttonDownBounds = Rectangle.FromLTRB(_buttonUpBounds.Left, bounds.Height - bheight,
                    bounds.Right, bounds.Height);

                _buttonDropDownBounds = Rectangle.Empty;

                _thumbBounds.X = _buttonUpBounds.Left;

                #endregion
            }

            _contentBounds = Rectangle.FromLTRB(bounds.Left, bounds.Top, _buttonUpBounds.Left, bounds.Bottom);  

            #endregion

            #region Assign buttons regions

            _buttonUpEnabled = _offset < 0;
            if (!_buttonUpEnabled) _offset = 0;
            _buttonDownEnabled = false;

            int curLeft = ContentBounds.Left + 1;
            int curTop = ContentBounds.Top + 1 + _offset;
            int maxBottom = curTop; // int.MinValue;
            int iniTop = curTop;

            foreach (RibbonItem item in Buttons)
            {
                item.SetBounds(Rectangle.Empty);
            }

            for (int i = 0; i < Buttons.Count; i++)
            {
                RibbonButton button = Buttons[i] as RibbonButton; if (button == null) break;

                if (curLeft + button.LastMeasuredSize.Width > ContentBounds.Right)
                {
                    curLeft = ContentBounds.Left + 1;
                    curTop = maxBottom + 1;
                }

                button.SetBounds(new Rectangle(curLeft, curTop, button.LastMeasuredSize.Width, button.LastMeasuredSize.Height));
                
                curLeft = button.Bounds.Right + 1;
                maxBottom = Math.Max(maxBottom, button.Bounds.Bottom);

                if (button.Bounds.Bottom > ContentBounds.Bottom) _buttonDownEnabled = true;

                _jumpDownSize = button.Bounds.Height;
                _jumpUpSize = button.Bounds.Height;
            }

            

            #endregion

            #region Adjust thumb size

            double contentHeight = maxBottom - iniTop;
            double viewHeight = bounds.Height;

            if (contentHeight > viewHeight && contentHeight != 0)
            {
                double viewPercent = contentHeight > viewHeight ? viewHeight / contentHeight : 0.0;
                double availHeight = ButtonDownBounds.Top - ButtonUpBounds.Bottom;
                double thumbHeight = Math.Ceiling(viewPercent * availHeight);

                if (thumbHeight < 30)
                {
                    if (availHeight >= 30)
                    {
                        thumbHeight = 30;
                    }
                    else
                    {
                        thumbHeight = availHeight;
                    }
                }

                _thumbBounds.Height = Convert.ToInt32(thumbHeight);

                fullContentBounds = Rectangle.FromLTRB(ContentBounds.Left, iniTop, ContentBounds.Right, maxBottom);

                _scrollBarEnabled = true;

                UpdateThumbPos();
            }
            else
            {
                _scrollBarEnabled = false;
            }
            
            #endregion
        }

        /// <summary>
        /// Updates the position of the scroll thumb depending on the current offset
        /// </summary>
        private void UpdateThumbPos()
        {
            if (_avoidNextThumbMeasure)
            {
                _avoidNextThumbMeasure = false;
                return;
            }

            double scrolledp = ScrolledPercent;

            if (!double.IsInfinity(scrolledp))
            {
                double availSpace = ScrollMaximum - ScrollMinimum;
                double scrolledSpace = Math.Ceiling(availSpace * ScrolledPercent);

                _thumbBounds.Y = ScrollMinimum + Convert.ToInt32(scrolledSpace);
            }
            else
            {
                _thumbBounds.Y = ScrollMinimum;
            }

            if (_thumbBounds.Y > ScrollMaximum)
            {
                _thumbBounds.Y = ScrollMaximum;
            }
            
        }

        public override Size MeasureSize(object sender, RibbonElementMeasureSizeEventArgs e)
        {
            #region Determine items

            int itemsWide = 0;

            switch (e.SizeMode)
            {
                case RibbonElementSizeMode.DropDown:
                    itemsWide = ItemsSizeInDropwDownMode.Width;
                    break;
                case RibbonElementSizeMode.Large:
                    itemsWide = ItemsWideInLargeMode;
                    break;
                case RibbonElementSizeMode.Medium:
                    itemsWide = ItemsWideInMediumMode;
                    break;
                case RibbonElementSizeMode.Compact:
                    itemsWide = 0;
                    break;
            }

            #endregion

            int height = OwnerPanel.ContentBounds.Height - Owner.ItemPadding.Vertical - 4;
            int scannedItems = 0;
            int widthSum = 1;
            int buttonHeight = 0;
            int heightSum = 0;
            bool sumWidth = true;

            foreach (RibbonButton button in Buttons)
            {
                Size s = button.MeasureSize(this, 
                    new RibbonElementMeasureSizeEventArgs(e.Graphics, this.ButtonsSizeMode));

                if (sumWidth)
                    widthSum += s.Width + 1;

                buttonHeight = button.LastMeasuredSize.Height;
                heightSum += buttonHeight;

                if (++scannedItems == itemsWide) sumWidth = false;
            }

            if (e.SizeMode == RibbonElementSizeMode.DropDown)
            {
                height = buttonHeight * ItemsSizeInDropwDownMode.Height;
            }

            if (ScrollBarRenderer.IsSupported)
            {
                _thumbBounds = new Rectangle(Point.Empty, ScrollBarRenderer.GetSizeBoxSize(e.Graphics, System.Windows.Forms.VisualStyles.ScrollBarState.Normal));
            }
            else
            {
                _thumbBounds = new Rectangle(Point.Empty, new Size(16, 16));
            }

            //if (height < 0)
            //{
            //    throw new Exception("???");
            //}
            SetLastMeasuredSize(new Size(widthSum + ControlButtonsWidth, height));

            return LastMeasuredSize;
        }

        internal override void SetSizeMode(RibbonElementSizeMode sizeMode)
        {
            base.SetSizeMode(sizeMode);

            foreach (RibbonItem item in Buttons)
            {
                item.SetSizeMode(ButtonsSizeMode);
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (ButtonDownPressed && ButtonDownSelected && ButtonDownEnabled)
            {
                ScrollOffset(-1);
            }

            if (ButtonUpPressed && ButtonUpSelected && ButtonUpEnabled)
            {
                ScrollOffset(1);
            }

            bool upCache = _buttonUpSelected;
            bool downCache = _buttonDownSelected;
            bool dropCache = _buttonDropDownSelected;
            bool thumbCache = _thumbSelected;

            _buttonUpSelected = _buttonUpBounds.Contains(e.Location);
            _buttonDownSelected = _buttonDownBounds.Contains(e.Location);
            _buttonDropDownSelected = _buttonDropDownBounds.Contains(e.Location);
            _thumbSelected = _thumbBounds.Contains(e.Location) && ScrollType == ListScrollType.Scrollbar && ScrollBarEnabled;

            if ((upCache != _buttonUpSelected)
                || (downCache != _buttonDownSelected)
                || (dropCache != _buttonDropDownSelected)
                || (thumbCache != _thumbSelected))
            {
                RedrawControlButtons();
            }

            if (ThumbPressed)
            {
                int newval = e.Y - _thumbOffset;

                if (newval < ScrollMinimum)
                {
                    newval = ScrollMinimum;
                }
                else if (newval > ScrollMaximum)
                {
                    newval = ScrollMaximum;
                }

                ScrollValue = newval;
                RedrawScroll();
            }
        }

        public override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            bool mustRedraw = _buttonUpSelected || _buttonDownSelected || _buttonDropDownSelected;

            _buttonUpSelected = false;
            _buttonDownSelected = false;
            _buttonDropDownSelected = false;

            if (mustRedraw)
                RedrawControlButtons();
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (ButtonDownSelected || ButtonUpSelected || ButtonDropDownSelected)
            {
                IgnoreDeactivation();
            }

            if (ButtonDownSelected && ButtonDownEnabled)
            {
                _buttonDownPressed = true;
                ScrollDown();
            }

            if (ButtonUpSelected && ButtonUpEnabled)
            {
                _buttonUpPressed = true;
                ScrollUp();
            }

            if (ButtonDropDownSelected)
            {
                _buttonDropDownPressed = true;
                ShowDropDown();
            }

            if (ThumbSelected)
            {
                _thumbPressed = true;
                _thumbOffset = e.Y - _thumbBounds.Y;
            }
            
            if (
                ScrollType == ListScrollType.Scrollbar &&
                ScrollBarBounds.Contains(e.Location) &&
                e.Y >= ButtonUpBounds.Bottom && e.Y <= ButtonDownBounds.Y &&
                !ThumbBounds.Contains(e.Location) &&
                !ButtonDownBounds.Contains(e.Location) &&
                !ButtonUpBounds.Contains(e.Location))
            {
                if (e.Y < ThumbBounds.Y)
                {
                    ScrollUp();
                }
                else
                {
                    ScrollDown();
                }
            }

        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _buttonDownPressed = false;
            _buttonUpPressed = false;
            _buttonDropDownPressed = false;
            _thumbPressed = false;
        }

        #endregion

        #region IContainsRibbonItems Members

        public IEnumerable<RibbonItem> GetItems()
        {
            return Buttons;
        }

        public Rectangle GetContentBounds()
        {
            return ContentBounds;
        }

        #endregion

        #region IContainsRibbonComponents Members

        public IEnumerable<Component> GetAllChildComponents()
        {
            List<Component> result = new List<Component>(Buttons.ToArray());

            result.AddRange(DropDownItems.ToArray());

            return result;
        }

        #endregion
    }
}

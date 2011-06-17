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
using System.ComponentModel.Design;

namespace System.Windows.Forms
{
    [Designer(typeof(RibbonButtonDesigner))]
    public class RibbonButton : RibbonItem, IContainsRibbonComponents
    {
        #region Fields

        private const int arrowWidth = 5;
        private RibbonButtonStyle _style;
        private Rectangle _dropDownBounds;
        private Rectangle _buttonFaceBounds;
        private RibbonItemCollection _dropDownItems;
        private bool _dropDownPressed;
        private bool _dropDownSelected;
        private Image _smallImage; 
        private Size _dropDownArrowSize;
        private Padding _dropDownMargin;
        private bool _dropDownVisible;
        private RibbonDropDown _dropDown;
        private Rectangle _imageBounds;
        private Rectangle _textBounds;
        private bool _dropDownResizable;
        private bool _checkOnClick;
        private Point _lastMousePos;
        private RibbonArrowDirection _DropDownArrowDirection;

       #endregion

        #region Events

        /// <summary>
        /// Occurs when the dropdown is about to be displayed
        /// </summary>
        public event EventHandler DropDownShowing; 

        #endregion

        #region Ctors

        /// <summary>
        /// Creates a new button
        /// </summary>
        /// <param name="image">Image of the button (32 x 32 suggested)</param>
        /// <param name="smallImage">Image of the button when in medium of compact mode (16 x 16 suggested)</param>
        /// <param name="style">Style of the button</param>
        /// <param name="text">Text of the button</param>
        public RibbonButton()
        {
            _dropDownItems = new RibbonItemCollection();
            _dropDownArrowSize = new Size(5, 3);
            _dropDownMargin = new Padding(6);
            _DropDownArrowDirection = RibbonArrowDirection.Down;
            Image = CreateImage(32);
            SmallImage = CreateImage(16);
        }

        public RibbonButton(string text)
            : this()
        {
            Text = text;
        }

        public RibbonButton(Image smallImage)
            : this()
        {
            SmallImage = smallImage;
        }

        #endregion

        #region Props

        /// <summary>
        /// Gets the DropDown of the button
        /// </summary>
        internal RibbonDropDown DropDown
        {
            get { return _dropDown; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the <see cref="Checked"/> property should be toggled
        /// when button is clicked
        /// </summary>
        [DefaultValue(false)]
        [Description("Toggles the Checked property of the button when clicked")]
        public bool CheckOnClick
        {
            get { return _checkOnClick; }
            set { _checkOnClick = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating if the DropDown should be resizable
        /// </summary>
        [DefaultValue(false)]
        [Description("Makes the DropDown resizable with a grip on the corner")]
        public bool DropDownResizable
        {
            get { return _dropDownResizable; }
            set { _dropDownResizable = value; }
        }

        /// <summary>
        /// Gets the bounds where the <see cref="Image"/> or <see cref="SmallImage"/> will be drawn.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ImageBounds
        {
            get { return _imageBounds; }
        }

        /// <summary>
        /// Gets the bounds where the <see cref="Text"/> of the button will be drawn
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle TextBounds
        {
            get { return _textBounds; }
        }


        /// <summary>
        /// Gets if the DropDown is currently visible
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DropDownVisible
        {
            get { return _dropDownVisible; }
        }

        /// <summary>
        /// Gets or sets the size of the dropdown arrow
        /// </summary>
        public Size DropDownArrowSize
        {
            get { return _dropDownArrowSize; }
            set { _dropDownArrowSize = value; NotifyOwnerRegionsChanged(); }
        }

        /// <summary>
        /// Gets or sets the direction where drop down's arrow should be pointing to
        /// </summary>
        public RibbonArrowDirection DropDownArrowDirection
        {
            get { return _DropDownArrowDirection; }
            set { _DropDownArrowDirection = value; NotifyOwnerRegionsChanged(); }
        }


        /// <summary>
        /// Gets the style of the button
        /// </summary>
        public RibbonButtonStyle Style
        {
            get
            {
                return _style;
            }
            set
            {
                _style = value;

                if (Canvas is RibbonPopup
                    || (OwnerItem != null && OwnerItem.Canvas is RibbonPopup))
                {
                    DropDownArrowDirection = RibbonArrowDirection.Left;
                }

                NotifyOwnerRegionsChanged();
            }
        }

        /// <summary>
        /// Gets the collection of items shown on the dropdown pop-up when Style allows it
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Content)]
        public RibbonItemCollection DropDownItems
        {
            get
            {
                return _dropDownItems;
            }
        }

        /// <summary>
        /// Gets the bounds of the button face
        /// </summary>
        /// <remarks>When Style is different from SplitDropDown and SplitBottomDropDown, ButtonFaceBounds is the same area than DropDownBounds</remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Rectangle ButtonFaceBounds
        {
            get
            {
                return _buttonFaceBounds;
            }
        }

        /// <summary>
        /// Gets the bounds of the dropdown button
        /// </summary>
        /// <remarks>When Style is different from SplitDropDown and SplitBottomDropDown, ButtonFaceBounds is the same area than DropDownBounds</remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Rectangle DropDownBounds
        {
            get
            {
                return _dropDownBounds;
            }
        }

        /// <summary>
        /// Gets if the dropdown part of the button is selected
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DropDownSelected
        {
            get
            {
                return _dropDownSelected;
            }
        }

        /// <summary>
        /// Gets if the dropdown part of the button is pressed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DropDownPressed
        {
            get
            {
                return _dropDownPressed;
            }
        }

        /// <summary>
        /// Gets or sets the image of the button when in compact of medium size mode
        /// </summary>
        [DefaultValue(null)]
        public virtual Image SmallImage
        {
            get
            {
                return _smallImage;
            }
            set 
            {
                _smallImage = value;

                NotifyOwnerRegionsChanged();
            }
        } 

        #endregion

        #region Methods

        /// <summary>
        /// Sets the value of the <see cref="DropDownMargin"/> property
        /// </summary>
        /// <param name="p"></param>
        protected void SetDropDownMargin(Padding p)
        {
            _dropDownMargin = p;
        }

        /// <summary>
        /// Performs a click on the button
        /// </summary>
        public void PerformClick()
        {
            OnClick(EventArgs.Empty);
        }

        /// <summary>
        /// Creates a new Transparent, empty image
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private Image CreateImage(int size)
        {
            Bitmap bmp = new Bitmap(size, size);

            //using (Graphics g = Graphics.FromImage(bmp))
            //{
            //    g.Clear(Color.FromArgb(50, Color.Navy));
            //}

            return bmp;
        }

        /// <summary>
        /// Creates the DropDown menu
        /// </summary>
        protected virtual void CreateDropDown()
        {
            _dropDown = new RibbonDropDown(this, DropDownItems, Owner);
        }

        internal override void SetPressed(bool pressed)
        {
            base.SetPressed(pressed);
        }

        internal override void SetOwner(Ribbon owner)
        {
            base.SetOwner(owner);

            if (_dropDownItems != null) _dropDownItems.SetOwner(owner);
        }

        internal override void SetOwnerPanel(RibbonPanel ownerPanel)
        {
            base.SetOwnerPanel(ownerPanel);

            if (_dropDownItems != null) _dropDownItems.SetOwnerPanel(ownerPanel);
        }

        internal override void SetOwnerTab(RibbonTab ownerTab)
        {
            base.SetOwnerTab(ownerTab);

            if (_dropDownItems != null) _dropDownItems.SetOwnerTab(ownerTab);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnPaint(object sender, RibbonElementPaintEventArgs e)
        {
            if (Owner == null) return;

            OnPaintBackground(e);

            OnPaintImage(e);

            OnPaintText(e);

        }

        /// <summary>
        /// Renders text of the button
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPaintText(RibbonElementPaintEventArgs e)
        {
            if (SizeMode != RibbonElementSizeMode.Compact)
            {
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Near;

                if (SizeMode == RibbonElementSizeMode.Large)
                {
                    sf.Alignment = StringAlignment.Center;

                    if (!string.IsNullOrEmpty(Text) && !Text.Contains(" "))
                    {
                        sf.LineAlignment = StringAlignment.Near;
                    }
                }

                Owner.Renderer.OnRenderRibbonItemText(
                    new RibbonTextEventArgs(Owner, e.Graphics, e.Clip, this, TextBounds, Text, sf));
            }
        }

        /// <summary>
        /// Renders the image of the button
        /// </summary>
        /// <param name="e"></param>
        private void OnPaintImage(RibbonElementPaintEventArgs e)
        {
            RibbonElementSizeMode theSize = GetNearestSize(e.Mode);
            if ((theSize == RibbonElementSizeMode.Large && Image != null) || SmallImage != null)
            {
                Owner.Renderer.OnRenderRibbonItemImage(
                    new RibbonItemBoundsEventArgs(Owner, e.Graphics, e.Clip, this, OnGetImageBounds(theSize, Bounds)));
            }
        }

        /// <summary>
        /// Renders the background of the buton
        /// </summary>
        /// <param name="e"></param>
        private void OnPaintBackground(RibbonElementPaintEventArgs e)
        {
            Owner.Renderer.OnRenderRibbonItem(new RibbonItemRenderEventArgs(Owner, e.Graphics, e.Clip, this));
        }

        /// <summary>
        /// Sets the bounds of the button
        /// </summary>
        /// <param name="bounds"></param>
        public override void SetBounds(System.Drawing.Rectangle bounds)
        {
            base.SetBounds(bounds);

            RibbonElementSizeMode sMode = GetNearestSize(SizeMode);

            _imageBounds = OnGetImageBounds(sMode, bounds);

            _textBounds = OnGetTextBounds(sMode, bounds);

            if (Style == RibbonButtonStyle.SplitDropDown)
            {
                _dropDownBounds = OnGetDropDownBounds(sMode, bounds);
                _buttonFaceBounds = OnGetButtonFaceBounds(sMode, bounds);
            }
        }

        /// <summary>
        /// Sets the bounds of the image of the button when SetBounds is called.
        /// Override this method to change image bounds
        /// </summary>
        /// <param name="sMode">Mode which is being measured</param>
        /// <param name="bounds">Bounds of the button</param>
        /// <remarks>
        /// The measuring occours in the following order:
        /// <list type="">
        /// <item>OnSetImageBounds</item>
        /// <item>OnSetTextBounds</item>
        /// <item>OnSetDropDownBounds</item>
        /// <item>OnSetButtonFaceBounds</item>
        /// </list>
        /// </remarks>
        internal virtual Rectangle OnGetImageBounds(RibbonElementSizeMode sMode, Rectangle bounds)
        {
            if (sMode == RibbonElementSizeMode.Large)// || this is RibbonOrbMenuItem)
            {
                if (Image != null)
                {
                    return new Rectangle(
                    Bounds.Left + ((Bounds.Width - Image.Width) / 2),
                    Bounds.Top + Owner.ItemMargin.Top,
                    Image.Width,
                    Image.Height);
                }
                else
                {
                    return new Rectangle(ContentBounds.Location, new Size(32, 32));
                }
            }
            else
            {
                if (SmallImage != null)
                {
                    return new Rectangle(
                        Bounds.Left + Owner.ItemMargin.Left,
                        Bounds.Top + ((Bounds.Height - SmallImage.Height) / 2),
                        SmallImage.Width,
                        SmallImage.Height);
                }
                else
                {
                    return new Rectangle(ContentBounds.Location, new Size(0, 0));
                }
            } 
        }

        /// <summary>
        /// Sets the bounds of the text of the button when SetBounds is called.
        /// Override this method to change image bounds
        /// </summary>
        /// <param name="sMode">Mode which is being measured</param>
        /// <param name="bounds">Bounds of the button</param>
        /// <remarks>
        /// The measuring occours in the following order:
        /// <list type="">
        /// <item>OnSetImageBounds</item>
        /// <item>OnSetTextBounds</item>
        /// <item>OnSetDropDownBounds</item>
        /// <item>OnSetButtonFaceBounds</item>
        /// </list>
        /// </remarks>
        internal virtual Rectangle OnGetTextBounds(RibbonElementSizeMode sMode, Rectangle bounds)
        {
            int imgw = _imageBounds.Width;
            int imgh = _imageBounds.Height;

            if (sMode == RibbonElementSizeMode.Large)
            {
                return Rectangle.FromLTRB(
                    Bounds.Left + Owner.ItemMargin.Left,
                    Bounds.Top + Owner.ItemMargin.Top + imgh,
                    Bounds.Right - Owner.ItemMargin.Right,
                    Bounds.Bottom - Owner.ItemMargin.Bottom);
            }
            else
            {
                int ddw = Style != RibbonButtonStyle.Normal ? _dropDownMargin.Horizontal : 0;
                return Rectangle.FromLTRB(
                    Bounds.Left + imgw + Owner.ItemMargin.Horizontal + Owner.ItemMargin.Left,
                    Bounds.Top + Owner.ItemMargin.Top,
                    Bounds.Right - ddw,
                    Bounds.Bottom - Owner.ItemMargin.Bottom);

            } 
        }

        /// <summary>
        /// Sets the bounds of the dropdown part of the button when SetBounds is called.
        /// Override this method to change image bounds
        /// </summary>
        /// <param name="sMode">Mode which is being measured</param>
        /// <param name="bounds">Bounds of the button</param>
        /// <remarks>
        /// The measuring occours in the following order:
        /// <list type="">
        /// <item>OnSetImageBounds</item>
        /// <item>OnSetTextBounds</item>
        /// <item>OnSetDropDownBounds</item>
        /// <item>OnSetButtonFaceBounds</item>
        /// </list>
        /// </remarks>
        internal virtual Rectangle OnGetDropDownBounds(RibbonElementSizeMode sMode, Rectangle bounds)
        {
            Rectangle sideBounds = Rectangle.FromLTRB(
                    bounds.Right - _dropDownMargin.Horizontal - 2,
                    bounds.Top, bounds.Right, bounds.Bottom);

            switch (SizeMode)
            {
                case RibbonElementSizeMode.Large:
                case RibbonElementSizeMode.Overflow:
                    return Rectangle.FromLTRB(bounds.Left,
                        bounds.Top + Image.Height + Owner.ItemMargin.Vertical,
                        bounds.Right, bounds.Bottom);

                case RibbonElementSizeMode.DropDown:
                case RibbonElementSizeMode.Medium:
                case RibbonElementSizeMode.Compact:
                    return sideBounds;
            }
            

            return Rectangle.Empty;
        }

        /// <summary>
        /// Sets the bounds of the button face part of the button when SetBounds is called.
        /// Override this method to change image bounds
        /// </summary>
        /// <param name="sMode">Mode which is being measured</param>
        /// <param name="bounds">Bounds of the button</param>
        /// <remarks>
        /// The measuring occours in the following order:
        /// <list type="">
        /// <item>OnSetImageBounds</item>
        /// <item>OnSetTextBounds</item>
        /// <item>OnSetDropDownBounds</item>
        /// <item>OnSetButtonFaceBounds</item>
        /// </list>
        /// </remarks>
        internal virtual Rectangle OnGetButtonFaceBounds(RibbonElementSizeMode sMode, Rectangle bounds)
        {

            Rectangle sideBounds = Rectangle.FromLTRB(
                bounds.Right - _dropDownMargin.Horizontal - 2,
                bounds.Top, bounds.Right, bounds.Bottom);

            switch (SizeMode)
            {
                case RibbonElementSizeMode.Large:
                case RibbonElementSizeMode.Overflow:
                    return Rectangle.FromLTRB(bounds.Left,
                        bounds.Top, bounds.Right, _dropDownBounds.Top);

                case RibbonElementSizeMode.DropDown:
                case RibbonElementSizeMode.Medium:
                case RibbonElementSizeMode.Compact:
                    return Rectangle.FromLTRB(bounds.Left, bounds.Top,
                        _dropDownBounds.Left, bounds.Bottom);

            }
            

            return Rectangle.Empty;
        }

        /// <summary>
        /// Measures the string for the large size
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        public static Size MeasureStringLargeSize(Graphics g, string text, Font font)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Size.Empty;
            }

            Size sz = g.MeasureString(text, font).ToSize();
            string[] words = text.Split(' ');
            string longestWord = string.Empty;
			string shortestWord = string.Empty;
            int width = sz.Width;

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > longestWord.Length)
                {
                    longestWord = words[i];
                }

				if (words[i].Length < shortestWord.Length || shortestWord.Length == 0)
				{
					shortestWord = words[i];
				}
            }

            if (words.Length > 1)
            {
				int lw_w = g.MeasureString(longestWord, font).ToSize().Width;
				int sw_w = 0;

				if (words.Length > 2)
					sw_w = g.MeasureString(shortestWord, font).ToSize().Width;

				width = Math.Max(sz.Width / 2, lw_w + sw_w) + 1;
            }
            else
            {
                return g.MeasureString(text, font).ToSize();
            }

            Size rs = g.MeasureString(text, font, width).ToSize();

            return new Size(rs.Width, rs.Height);
        }

        /// <summary>
        /// Measures the size of the button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override Size MeasureSize(object sender, RibbonElementMeasureSizeEventArgs e)
        {
            RibbonElementSizeMode theSize = GetNearestSize(e.SizeMode);
            int widthSum = Owner.ItemMargin.Horizontal;
            int heightSum = Owner.ItemMargin.Vertical;
            int largeHeight = OwnerPanel == null ? 0 : OwnerPanel.ContentBounds.Height - Owner.ItemPadding.Vertical;// -Owner.ItemMargin.Vertical; //58;

            Size simg = SmallImage != null ? SmallImage.Size : Size.Empty;
            Size img = Image != null ? Image.Size : Size.Empty;
            Size sz = Size.Empty;

            switch (theSize)
            {
                case RibbonElementSizeMode.Large:
                case RibbonElementSizeMode.Overflow:
                    sz = MeasureStringLargeSize(e.Graphics, Text, Owner.Font);
                    if (!string.IsNullOrEmpty(Text))
                    {
                        widthSum += Math.Max(sz.Width + 1, img.Width);
                        heightSum = largeHeight;
                    }
                    else
                    {
                        widthSum += img.Width;
                        heightSum += img.Height;
                    }
                    
                    break;
                case RibbonElementSizeMode.DropDown:
                case RibbonElementSizeMode.Medium:
                    sz = TextRenderer.MeasureText(Text, Owner.Font);
                    if(!string.IsNullOrEmpty(Text)) widthSum += sz.Width + 1;
                    widthSum += simg.Width + Owner.ItemMargin.Horizontal;
                    heightSum += Math.Max(sz.Height, simg.Height);
                    break;
                case RibbonElementSizeMode.Compact:
                    widthSum += simg.Width;
                    heightSum += simg.Height;
                    break;
                default:
                    throw new ApplicationException("SizeMode not supported: " + e.SizeMode.ToString());
            }

            if (theSize == RibbonElementSizeMode.DropDown)
            {
                heightSum += 2;
            }

            if (Style == RibbonButtonStyle.DropDown)
            {
                widthSum += arrowWidth + Owner.ItemMargin.Right;
            }
            else if (Style == RibbonButtonStyle.SplitDropDown)
            {
                widthSum += arrowWidth + Owner.ItemMargin.Horizontal;
            }

            SetLastMeasuredSize(new Size(widthSum, heightSum));

            return LastMeasuredSize;
        }

        /// <summary>
        /// Sets the value of the DropDownPressed property
        /// </summary>
        /// <param name="pressed">Value that indicates if the dropdown button is pressed</param>
        internal void SetDropDownPressed(bool pressed)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Sets the value of the DropDownSelected property
        /// </summary>
        /// <param name="selected">Value that indicates if the dropdown part of the button is selected</param>
        internal void SetDropDownSelected(bool selected)
        {
            //Dont use, an overflow occours
            throw new Exception();
        }

        /// <summary>
        /// Shows the drop down items of the button, as if the dropdown part has been clicked
        /// </summary>
        public void ShowDropDown()
        {
            if (Style == RibbonButtonStyle.Normal || DropDownItems.Count == 0)
            {
                if(DropDown != null)
                    RibbonPopupManager.DismissChildren(DropDown, RibbonPopupManager.DismissReason.NewPopup);
                return;
            }

            if (Style == RibbonButtonStyle.DropDown)
            {
                SetPressed(true);
            }
            else
            {
                _dropDownPressed = true;
            }

            

            CreateDropDown();
            DropDown.MouseEnter += new EventHandler(DropDown_MouseEnter);
            DropDown.Closed += new EventHandler(_dropDown_Closed);
            DropDown.ShowSizingGrip = DropDownResizable;
            
            RibbonPopup canvasdd = Canvas as RibbonPopup;
            Point location = OnGetDropDownMenuLocation();
            Size minsize = OnGetDropDownMenuSize();

            if (!minsize.IsEmpty) DropDown.MinimumSize = minsize;

            OnDropDownShowing(EventArgs.Empty);
            SetDropDownVisible(true);
            DropDown.SelectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            DropDown.Show(location);
        }

        void DropDown_MouseEnter(object sender, EventArgs e)
        {
            SetSelected(true);
            RedrawItem();
        }

        /// <summary>
        /// Gets the location where the dropdown menu will be shown
        /// </summary>
        /// <returns></returns>
        internal virtual Point OnGetDropDownMenuLocation()
        {
            
            Point location = Point.Empty;

            if (Canvas is RibbonDropDown)
            {
                location = Canvas.PointToScreen(new Point(Bounds.Right, Bounds.Top));
            }
            else
            {
                location = Canvas.PointToScreen(new Point(Bounds.Left, Bounds.Bottom));
            }

            return location;
        }

        /// <summary>
        /// Gets the size of the dropdown. If Size.Empty is returned, 
        /// size will be measured automatically
        /// </summary>
        /// <returns></returns>
        internal virtual Size OnGetDropDownMenuSize()
        {
            return Size.Empty;
        }

        /// <summary>
        /// Handles the closing of the dropdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dropDown_Closed(object sender, EventArgs e)
        {
            SetPressed(false);

            _dropDownPressed = false;

            SetDropDownVisible(false);

            SetSelected(false);

            RedrawItem();
        }

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
        /// Closes the DropDown if opened
        /// </summary>
        public void CloseDropDown()
        {
            if (DropDown != null)
            {
                RibbonPopupManager.Dismiss(DropDown, RibbonPopupManager.DismissReason.NewPopup);
            }

            SetDropDownVisible(false);
        }

        /// <summary>
        /// Overriden. Informs the button text
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{1}: {0}", Text, GetType().Name);
        }

        /// <summary>
        /// Sets the value of DropDownVisible
        /// </summary>
        /// <param name="visible"></param>
        internal void SetDropDownVisible(bool visible)
        {
            _dropDownVisible = visible;
        }

        /// <summary>
        /// Raises the DropDownShowing event
        /// </summary>
        /// <param name="e"></param>
        public void OnDropDownShowing(EventArgs e)
        {
            if (DropDownShowing != null)
            {
                DropDownShowing(this, e);
            }
        }

        #endregion

        #region Overrides

        public override void OnCanvasChanged(EventArgs e)
        {
            base.OnCanvasChanged(e);

            if (Canvas is RibbonDropDown)
            {
                DropDownArrowDirection = RibbonArrowDirection.Left;
            }
        }

        protected override bool ClosesDropDownAt(Point p)
        {
            if (Style == RibbonButtonStyle.DropDown)
            {
                return false;
            }
            else if (Style == RibbonButtonStyle.SplitDropDown)
            {
                return ButtonFaceBounds.Contains(p);
            }
            else
            {
                return true;
            }
        }

        internal override void SetSizeMode(RibbonElementSizeMode sizeMode)
        {
            

            if (sizeMode == RibbonElementSizeMode.Overflow)
            {
                base.SetSizeMode(RibbonElementSizeMode.Large);
            }
            else
            {
                base.SetSizeMode(sizeMode);
            }
        }

        internal override void SetSelected(bool selected)
        {
            base.SetSelected(selected);

            SetPressed(false);
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (!Enabled) return;

            if ((DropDownSelected || Style == RibbonButtonStyle.DropDown) && DropDownItems.Count > 0)
            {
                _dropDownPressed = true;
                ShowDropDown();
            }

            base.OnMouseDown(e);
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (!Enabled) return;

            //Detect mouse on dropdwon
            if (Style == RibbonButtonStyle.SplitDropDown)
            {
                bool lastState = _dropDownSelected;

                if (DropDownBounds.Contains(e.X, e.Y))
                {
                    _dropDownSelected = true;
                }
                else
                {
                    _dropDownSelected = false;
                }

                if (lastState != _dropDownSelected) 
                    RedrawItem();

                lastState = _dropDownSelected;
            }

            _lastMousePos = new Point(e.X, e.Y);

            base.OnMouseMove(e);
        }

        public override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            _dropDownSelected = false;
            
        }

        public override void OnClick(EventArgs e)
        {
            if (Style != RibbonButtonStyle.Normal && !ButtonFaceBounds.Contains(_lastMousePos))
            {
                //Clicked the dropdown area, don't raise Click()
                return;
            }

            if(CheckOnClick)
                Checked = !Checked;

            base.OnClick(e);
        }

        #endregion

        #region IContainsRibbonItems Members

        public IEnumerable<RibbonItem> GetItems()
        {
            return DropDownItems;
        }

        #endregion

        #region IContainsRibbonComponents Members

        public IEnumerable<Component> GetAllChildComponents()
        {
            return DropDownItems.ToArray();
        }

        #endregion
    }
}

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

namespace System.Windows.Forms
{
    [DesignTimeVisible(false)]
    public abstract class RibbonItem : Component, IRibbonElement
    {
        #region Fields
        private string _text;
        private Image _image;
        private string _tooltip;
        private bool _checked;
        private bool _selected;
        private Ribbon _owner;
        private Rectangle _bounds;
        private bool _pressed;
        private string _tooltipTitle;
        private Image _tooltipImage;
        private bool _enabled;
        private object _tag;
        private string _altKey;
        private RibbonTab _ownerTab;
        private RibbonPanel _ownerPanel;
        private RibbonElementSizeMode _maxSize;
        private RibbonElementSizeMode _minSize;
        private Size _lastMeasureSize;
        private RibbonItem _ownerItem;
        private RibbonElementSizeMode _sizeMode; 
        private Control _canvas;

      #endregion

        #region Events
        public virtual event EventHandler DoubleClick;

        public virtual event EventHandler Click;

        public virtual event System.Windows.Forms.MouseEventHandler MouseUp;

        public virtual event System.Windows.Forms.MouseEventHandler MouseMove;

        public virtual event System.Windows.Forms.MouseEventHandler MouseDown;

        public virtual event System.Windows.Forms.MouseEventHandler MouseEnter;

        public virtual event System.Windows.Forms.MouseEventHandler MouseLeave;

        public virtual event EventHandler CanvasChanged;

        #endregion

        #region Ctor

        public RibbonItem()
        {
         
            _enabled = true;
            Click += new EventHandler(RibbonItem_Click);
        }

        /// <summary>
        /// Selects the item when in a dropdown, in design mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonItem_Click(object sender, EventArgs e)
        {
            RibbonDropDown dd = Canvas as RibbonDropDown;

            if (dd != null && dd.SelectionService != null)
            {
                dd.SelectionService.SetSelectedComponents(
                    new Component[] { this }, System.ComponentModel.Design.SelectionTypes.Primary);

            }
        }

        #endregion

        #region Props


        /// <summary>
        /// Gets the bounds of the item's content. (It takes the Ribbon.ItemMargin)
        /// </summary>
        /// <remarks>
        /// Although this is the regular item content bounds, it depends on the logic of the item 
        /// and how each item handles its own content.
        /// </remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Rectangle ContentBounds
        {
            get {

                return Rectangle.FromLTRB(
                    Bounds.Left + Owner.ItemMargin.Left,
                    Bounds.Top + Owner.ItemMargin.Top,
                    Bounds.Right - Owner.ItemMargin.Right,
                    Bounds.Bottom - Owner.ItemMargin.Bottom);
            }
        }

        /// <summary>
        /// Gets the control where the item is currently being dawn
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control Canvas
        {
            get
            {
                if (_canvas != null && !_canvas.IsDisposed)
                    return _canvas;
                
                return Owner;
            }
        }

        /// <summary>
        /// Gets the RibbonItemGroup that owns the item (If any)
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonItem OwnerItem
        {
            get { return _ownerItem; }
        }

        /// <summary>
        /// Gets or sets the text that is to be displayed on the item
        /// </summary>
        [DefaultValue("")]
        [Localizable(true)]
        public virtual string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;

                NotifyOwnerRegionsChanged();
            }
        }

        /// <summary>
        /// Gets or sets the image to be displayed on the item
        /// </summary>
        public virtual Image Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;

                NotifyOwnerRegionsChanged();
            }
        }

        /// <summary>
        /// Gets or sets the tool tip text
        /// </summary>
        [DefaultValue("")]
        [Localizable(true)]
        public virtual string ToolTip
        {
            get
            {
                return _tooltip;
            }
            set
            {
                _tooltip = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the item is currently checked
        /// </summary>
        [DefaultValue(false)]
        public virtual bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                _checked = value;

                NotifyOwnerRegionsChanged();
            }
        }

        /// <summary>
        /// Gets the item's current SizeMode
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonElementSizeMode SizeMode
        {
            get { return _sizeMode; }
        }

        /// <summary>
        /// Gets a value indicating whether the item is selected
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Selected
        {
            get
            {
                return _selected;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the state of the item is pressed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Pressed
        {
            get
            {
                return _pressed;
            }
        }

        /// <summary>
        /// Gets the Ribbon owner of this item
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
        /// Gets the bounds of the element relative to the Ribbon control
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle Bounds
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the item is currently enabled
        /// </summary>
        [DefaultValue(true)]
        public virtual bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;

                IContainsSelectableRibbonItems container = this as IContainsSelectableRibbonItems;

                if (container != null)
                {
                    foreach (RibbonItem item in container.GetItems())
                    {
                        item.Enabled = value;
                    }
                }

                NotifyOwnerRegionsChanged();
            }
        }

        /// <summary>
        /// Gets or sets the tool tip title
        /// </summary>
        [DefaultValue("")]
        public string ToolTipTitle
        {
            get
            {
                return _tooltipTitle;
            }
            set
            {
                _tooltipTitle = value;
            }
        }

        /// <summary>
        /// Gets or sets the image of the tool tip
        /// </summary>
        [DefaultValue("")]
        public Image ToolTipImage
        {
            get
            {
                return _tooltipImage;
            }
            set
            {
                _tooltipImage = value;
            }
        }

        /// <summary>
        /// Gets or sets the object that contains data about the control
        /// </summary>
        public object Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }

        /// <summary>
        /// Gets or sets the key combination that activates this element when the Alt key was pressed
        /// </summary>
        [DefaultValue("")]
        public string AltKey
        {
            get
            {
                return _altKey;
            }
            set
            {
                _altKey = value;
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
        /// Gets or sets the maximum size mode of the element
        /// </summary>
        [DefaultValue(RibbonElementSizeMode.None)]
        public RibbonElementSizeMode MaxSizeMode
        {
            get
            {
                return _maxSize;
            }
            set
            {
                _maxSize = value;

                NotifyOwnerRegionsChanged();
            }
        }

        /// <summary>
        /// Gets or sets the minimum size mode of the element
        /// </summary>
        [DefaultValue(RibbonElementSizeMode.None)]
        public RibbonElementSizeMode MinSizeMode
        {
            get
            {
                return _minSize;
            }
            set
            {
                _minSize = value;

                NotifyOwnerRegionsChanged();
            }
        }

        /// <summary>
        /// Gets the last result of  MeasureSize
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size LastMeasuredSize
        {
            get
            {
                return _lastMeasureSize;
            }
        } 
        #endregion

        #region Methods

        /// <summary>
        /// Gets if onwer dropdown must be closed when the item is clicked on the specified point
        /// </summary>
        /// <param name="p">Point to test.</param>
        /// <returns></returns>
        protected virtual bool ClosesDropDownAt(Point p)
        {
            return true;
        }

        /// <summary>
        /// Forces the owner Ribbon to update its regions
        /// </summary>
        protected void NotifyOwnerRegionsChanged()
        {
            if (Owner != null)
            {
                if (Owner == Canvas)
                {
                    Owner.OnRegionsChanged();
                }
                else if (Canvas != null)
                {
                    if (Canvas is RibbonOrbDropDown)
                    {
                        (Canvas as RibbonOrbDropDown).OnRegionsChanged();
                    }
                    else
                    {
                        Canvas.Invalidate(Bounds);
                    }
                }

            }
        }

        /// <summary>
        /// Sets the value of the <see cref="OwnerItem"/> property
        /// </summary>
        /// <param name="item"></param>
        internal virtual void SetOwnerItem(RibbonItem item)
        {
            _ownerItem = item;
        }

        /// <summary>
        /// Sets the Ribbon that owns this item
        /// </summary>
        /// <param name="owner">Ribbon that owns this item</param>
        internal virtual void SetOwner(Ribbon owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Sets the value of the OwnerPanel property
        /// </summary>
        /// <param name="ownerPanel">RibbonPanel where this item is located</param>
        internal virtual void SetOwnerPanel(RibbonPanel ownerPanel)
        {
            _ownerPanel = ownerPanel;
        }

        /// <summary>
        /// Sets the value of the Selected property
        /// </summary>
        /// <param name="selected">Value that indicates if the element is selected</param>
        internal virtual void SetSelected(bool selected)
        {
            if (!Enabled) return;

            _selected = selected;
        }

        /// <summary>
        /// Sets the value of the Pressed property
        /// </summary>
        /// <param name="pressed">Value that indicates if the element is pressed</param>
        internal virtual void SetPressed(bool pressed)
        {
            _pressed = pressed;
        }

        /// <summary>
        /// Sets the value of the OwnerTab property
        /// </summary>
        /// <param name="ownerTab">RibbonTab where this item is located</param>
        internal virtual void SetOwnerTab(RibbonTab ownerTab)
        {
            _ownerTab = ownerTab;
        }

        /// <summary>
        /// Sets the value of the OwnerList property
        /// </summary>
        /// <param name="ownerList"></param>
        internal virtual void SetOwnerGroup(RibbonItemGroup ownerGroup)
        {
            _ownerItem = ownerGroup;
        }

        /// <summary>
        /// Gets the size applying the rules of MaxSizeMode and MinSizeMode properties
        /// </summary>
        /// <param name="sizeMode">Suggested sizeMode</param>
        /// <returns>The nearest size to the specified one</returns>
        protected RibbonElementSizeMode GetNearestSize(RibbonElementSizeMode sizeMode)
        {
            int size = (int)sizeMode;
            int max = (int)MaxSizeMode;
            int min = (int)MinSizeMode;
            int result = (int)sizeMode;

            if (max > 0 && size > max) //Max is specified and value exceeds max
            {
                result = max;
            }

            if (min > 0 && size < min) //Min is specified and value exceeds min
            {
                result = min;
            }

            return (RibbonElementSizeMode)result;
        }

        /// <summary>
        /// Sets the value of the LastMeasuredSize property
        /// </summary>
        /// <param name="size">Size to set to the property</param>
        protected void SetLastMeasuredSize(Size size)
        {
            _lastMeasureSize = size;
        }

        /// <summary>
        /// Sets the value of the SizeMode property
        /// </summary>
        /// <param name="sizeMode"></param>
        internal virtual void SetSizeMode(RibbonElementSizeMode sizeMode)
        {
            _sizeMode = GetNearestSize(sizeMode);
        }

        /// <summary>
        /// Raises the <see cref="CanvasChanged"/> event
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnCanvasChanged(EventArgs e)
        {
            if (CanvasChanged != null)
            {
                CanvasChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the MouseEnter event
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnMouseEnter(MouseEventArgs e)
        {
            if (!Enabled) return;

            if (MouseEnter != null)
            {
                MouseEnter(this, e);
            }
        }

        /// <summary>
        /// Raises the MouseDown event
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnMouseDown(MouseEventArgs e)
        {
            if (!Enabled) return;

            if (MouseDown != null)
            {
                MouseDown(this, e);
            }

			RibbonPopup pop = Canvas as RibbonPopup;

			if (pop != null) {
				if (ClosesDropDownAt(e.Location)) {
					RibbonPopupManager.Dismiss(RibbonPopupManager.DismissReason.ItemClicked);
				}

				OnClick(EventArgs.Empty);
			}

            SetPressed(true);
        }

        /// <summary>
        /// Raises the MouseLeave event
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnMouseLeave(MouseEventArgs e)
        {
            if (!Enabled) return;

            if (MouseLeave != null)
            {
                MouseLeave(this, e);
            }
        }

        /// <summary>
        /// Raises the MouseUp event
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnMouseUp(MouseEventArgs e)
        {
            if (!Enabled) return;

            if (MouseUp != null)
            {
                MouseUp(this, e);
            }

            if (Pressed)
            {
                SetPressed(false);
                RedrawItem();
            }
            
        }

        /// <summary>
        /// Raises the MouseMove event
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnMouseMove(MouseEventArgs e)
        {
            if (!Enabled) return;

            if (MouseMove != null)
            {
                MouseMove(this, e);
            }
        }

        /// <summary>
        /// Raises the Click event
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnClick(EventArgs e)
        {
            if (!Enabled) return;

            if (Click != null)
            {
                Click(this, e);
            }
        }

        /// <summary>
        /// Raises the DoubleClick event
        /// </summary>
        /// <param name="e">Event data</param>
        public virtual void OnDoubleClick(EventArgs e)
        {
            if (!Enabled) return;

            if (DoubleClick != null)
            {
                DoubleClick(this, e);
            }
        }

        /// <summary>
        /// Redraws the item area on the Onwer Ribbon
        /// </summary>
        public virtual void RedrawItem()
        {
            if (Canvas != null)
            {
                Canvas.Invalidate(Rectangle.Inflate(Bounds, 1, 1));
            }
        }

        /// <summary>
        /// Sets the canvas of the item
        /// </summary>
        /// <param name="canvas"></param>
        internal void SetCanvas(Control canvas)
        {
            _canvas = canvas;

            SetCanvas(this as IContainsSelectableRibbonItems, canvas);

            OnCanvasChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Recurse on setting the canvas
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="canvas"></param>
        private void SetCanvas(IContainsSelectableRibbonItems parent, Control canvas)
        {
            if (parent == null) return;

            foreach (RibbonItem item in parent.GetItems())
            {
                item.SetCanvas(canvas);
            }
        }

        #endregion

        #region IRibbonElement Members

        public abstract void OnPaint(object sender, RibbonElementPaintEventArgs e);

        public virtual void SetBounds(Rectangle bounds)
        {
            _bounds = bounds;
        }

        public abstract Size MeasureSize(object sender, RibbonElementMeasureSizeEventArgs e);

        #endregion

    }
}

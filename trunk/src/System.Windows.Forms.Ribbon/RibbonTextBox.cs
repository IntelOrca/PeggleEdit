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
    public class RibbonTextBox 
        : RibbonItem
    {
        #region Fields
        private const int spacing = 3;
        private TextBox _actualTextBox;
        private bool _removingTxt; 
        private bool _labelVisible;
        private bool _imageVisible;
        private Rectangle _labelBounds;
        private Rectangle _imageBounds;
        private int _textboxWidth;
        private Rectangle _textBoxBounds;
        private string _textBoxText;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the <see cref="TextBoxText"/> property value has changed
        /// </summary>
        public event EventHandler TextBoxTextChanged;

        #endregion

        #region Ctor

        public RibbonTextBox()
        {
            _textboxWidth = 100;
        }

        #endregion

        #region Props


        /// <summary>
        /// Gets or sets the text on the textbox
        /// </summary>
        [Description("Text on the textbox")]
        public string TextBoxText
        {
            get { return _textBoxText; }
            set { _textBoxText = value; OnTextChanged(EventArgs.Empty); }
        }

        /// <summary>
        /// Gets the bounds of the text on the textbox
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Rectangle TextBoxTextBounds
        {
            get { return TextBoxBounds; }
        }

        /// <summary>
        /// Gets the bounds of the image
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ImageBounds
        {
            get { return _imageBounds; }
        }

        /// <summary>
        /// Gets the bounds of the label that is shown next to the textbox
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Rectangle LabelBounds
        {
            get { return _labelBounds; }
        }

        /// <summary>
        /// Gets a value indicating if the image is currenlty visible
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ImageVisible
        {
            get { return _imageVisible; }
        }

        /// <summary>
        /// Gets a value indicating if the label is currently visible
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LabelVisible
        {
            get { return _labelVisible; }
        }

        /// <summary>
        /// Gets the bounds of the text
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Rectangle TextBoxBounds
        {
            get 
            {
                return _textBoxBounds;
            }
        }

        /// <summary>
        /// Gets a value indicating if user is currently editing the text of the textbox
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Editing
        {
            get { return _actualTextBox != null; }
        }

        /// <summary>
        /// Gets or sets the width of the textbox
        /// </summary>
        [DefaultValue(100)]
        public int TextBoxWidth
        {
            get { return _textboxWidth; }
            set { _textboxWidth = value; NotifyOwnerRegionsChanged(); }
        }


        #endregion

        #region Methods

        /// <summary>
        /// Starts editing the text and focuses the TextBox
        /// </summary>
        public void StartEdit()
        {
            //if (!Enabled) return;

            PlaceActualTextBox();

            _actualTextBox.SelectAll();
            _actualTextBox.Focus();
        }

        /// <summary>
        /// Ends the editing of the textbox
        /// </summary>
        public void EndEdit()
        {
            RemoveActualTextBox();
        }

        /// <summary>
        /// Places the Actual TextBox on the owner so user can edit the text
        /// </summary>
        protected void PlaceActualTextBox()
        {
            _actualTextBox = new TextBox();

            InitTextBox(_actualTextBox);

            _actualTextBox.TextChanged += new EventHandler(_actualTextbox_TextChanged);
            _actualTextBox.KeyDown += new KeyEventHandler(_actualTextbox_KeyDown);
            _actualTextBox.LostFocus += new EventHandler(_actualTextbox_LostFocus);
            _actualTextBox.VisibleChanged += new EventHandler(_actualTextBox_VisibleChanged);

            _actualTextBox.Visible = true;
            Canvas.Controls.Add(_actualTextBox);

        }

        private void _actualTextBox_VisibleChanged(object sender, EventArgs e)
        {
            if (!(sender as TextBox).Visible && !_removingTxt)
            {
                RemoveActualTextBox();
            }
        }        

        /// <summary>
        /// Removes the actual TextBox that edits the text
        /// </summary>
        protected void RemoveActualTextBox()
        {
            if (_actualTextBox == null || _removingTxt)
            {
                return;
            }
            _removingTxt = true;

            TextBoxText = _actualTextBox.Text;
            _actualTextBox.Visible = false;
            _actualTextBox.Parent.Controls.Remove(_actualTextBox);
            _actualTextBox.Dispose();
            _actualTextBox = null;

            RedrawItem();
            _removingTxt = false;
        }

        /// <summary>
        /// Initializes the texbox that edits the text
        /// </summary>
        /// <param name="t"></param>
        protected virtual void InitTextBox(TextBox t)
        {
            t.Text = this.TextBoxText;
            t.BorderStyle = BorderStyle.None;
            t.Width = TextBoxBounds.Width - 2;

            t.Location = new Point(
                TextBoxBounds.Left + 2,
                Bounds.Top + (Bounds.Height - t.Height) / 2);
        }

        /// <summary>
        /// Handles the LostFocus event of the actual TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _actualTextbox_LostFocus(object sender, EventArgs e)
        {
            RemoveActualTextBox();
        }

        /// <summary>
        /// Handles the KeyDown event of the actual TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _actualTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return ||
                e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Escape)
            {
                RemoveActualTextBox();
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the actual TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _actualTextbox_TextChanged(object sender, EventArgs e)
        {
            //Text = (sender as TextBox).Text;
        }

        /// <summary>
        /// Measures the suposed height of the textobx
        /// </summary>
        /// <returns></returns>
        protected virtual int MeasureHeight()
        {
            return 16 + Owner.ItemMargin.Vertical;
        }

        public override void OnPaint(object sender, RibbonElementPaintEventArgs e)
        {

            Owner.Renderer.OnRenderRibbonItem(new RibbonItemRenderEventArgs(Owner, e.Graphics, Bounds, this));
            
            if(ImageVisible)
                Owner.Renderer.OnRenderRibbonItemImage(new RibbonItemBoundsEventArgs(Owner, e.Graphics, e.Clip, this, _imageBounds));

            StringFormat f = new StringFormat();

            f.Alignment = StringAlignment.Near;
            f.LineAlignment = StringAlignment.Center;
            f.Trimming = StringTrimming.None;
            f.FormatFlags |= StringFormatFlags.NoWrap;

            Owner.Renderer.OnRenderRibbonItemText(new RibbonTextEventArgs(Owner, e.Graphics, Bounds, this, TextBoxTextBounds, TextBoxText, f));
            
            if(LabelVisible)
                Owner.Renderer.OnRenderRibbonItemText(new RibbonTextEventArgs(Owner, e.Graphics, Bounds, this, LabelBounds, Text, f));
        }

        public override void SetBounds(System.Drawing.Rectangle bounds)
        {
            base.SetBounds(bounds);

            _textBoxBounds = Rectangle.FromLTRB(
                bounds.Right - TextBoxWidth,
                bounds.Top,
                bounds.Right,
                bounds.Bottom);

            if (Image != null)
                _imageBounds = new Rectangle(
                    bounds.Left + Owner.ItemMargin.Left,
                    bounds.Top + Owner.ItemMargin.Top, Image.Width, Image.Height);
            else
                _imageBounds = new Rectangle(ContentBounds.Location, Size.Empty);

            _labelBounds = Rectangle.FromLTRB(
                _imageBounds.Right + (_imageBounds.Width > 0 ? spacing :0),
                bounds.Top,
                _textBoxBounds.Left - spacing,
                bounds.Bottom - Owner.ItemMargin.Bottom);

            if (SizeMode == RibbonElementSizeMode.Large)
            {
                _imageVisible = true;
                _labelVisible = true;
            }
            else if (SizeMode == RibbonElementSizeMode.Medium)
            {
                _imageVisible = true;
                _labelVisible = false;
                _labelBounds = Rectangle.Empty;
            }
            else if (SizeMode == RibbonElementSizeMode.Compact)
            {
                _imageBounds = Rectangle.Empty;
                _imageVisible = false;
                _labelBounds = Rectangle.Empty;
                _labelVisible = false;
            }
        }

        public override Size MeasureSize(object sender, RibbonElementMeasureSizeEventArgs e)
        {
            Size size = Size.Empty;

            int w = 0;
            int iwidth = Image != null ? Image.Width + spacing : 0;
            int lwidth = string.IsNullOrEmpty(Text) ? 0 : e.Graphics.MeasureString(Text, Owner.Font).ToSize().Width + spacing;
            int twidth = TextBoxWidth;

            w += TextBoxWidth;

            switch (e.SizeMode)
            {
                case RibbonElementSizeMode.Large:
                    w += iwidth + lwidth;
                    break;
                case RibbonElementSizeMode.Medium:
                    w += iwidth;
                    break;
            }

            SetLastMeasuredSize(new Size(w, MeasureHeight()));

            return LastMeasuredSize;
        }

        public override void OnMouseEnter(MouseEventArgs e)
        {
            if (!Enabled) return;

            base.OnMouseEnter(e);

            Canvas.Cursor = Cursors.IBeam;
        }

        public override void OnMouseLeave(MouseEventArgs e)
        {
            if (!Enabled) return;

            base.OnMouseLeave(e);

            Canvas.Cursor = Cursors.Default;
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (!Enabled) return;

            base.OnMouseDown(e);

            if (TextBoxBounds.Contains(e.X,e.Y))
            {
                StartEdit();
            }
            
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (!Enabled) return;

            base.OnMouseMove(e);

            if (TextBoxBounds.Contains(e.X, e.Y))
            {
                Owner.Cursor = Cursors.IBeam;
            }
            else
            {
                Owner.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Raises the <see cref="TextBoxTextChanged"/> event
        /// </summary>
        /// <param name="e"></param>
        public void OnTextChanged(EventArgs e)
        {
            if (!Enabled) return;

            NotifyOwnerRegionsChanged();

            if (TextBoxTextChanged != null)
            {
                TextBoxTextChanged(this, e);
            }
        }

        #endregion
    }
}

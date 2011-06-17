using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
    [Designer(typeof(RibbonComboBoxDesigner))]
    public class RibbonComboBox
        : RibbonTextBox, IContainsRibbonComponents, IDropDownRibbonItem
    {
        #region Fields

        private RibbonItemCollection _dropDownItems;
        private Rectangle _dropDownBounds;
        private bool _dropDownSelected;
        private bool _dropDownPressed;
        private bool _dropDownVisible;
        private bool _allowTextEdit;
        private bool _dropDownResizable;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the DropDown is about to be displayed
        /// </summary>
        public event EventHandler DropDownShowing;

        #endregion

        #region Ctor

        public RibbonComboBox()
        {
            _dropDownItems = new RibbonItemCollection();
            _dropDownVisible = true;
            _allowTextEdit = true;
        }

        #endregion

        #region Properties

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
        /// Overriden.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public override Rectangle TextBoxTextBounds
        {
            get
            {
                Rectangle r = base.TextBoxTextBounds;

                r.Width -= DropDownButtonBounds.Width;

                return r;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if user is allowed to edit the text by hand
        /// </summary>
        [Description("Allows user to change the text on the ComboBox")]
        [DefaultValue(true)]
        public bool AllowTextEdit
        {
            get { return _allowTextEdit; }
            set { _allowTextEdit = value; }
        }


        /// <summary>
        /// Gets the collection of items to be displayed on the dropdown
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Content)]
        public RibbonItemCollection DropDownItems
        {
            get { return _dropDownItems; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="DropDownShowing"/> event;
        /// </summary>
        /// <param name="e"></param>
        public void OnDropDownShowing(EventArgs e)
        {
            if (DropDownShowing != null)
            {
                DropDownShowing(this, e);
            }
        }

        /// <summary>
        /// Shows the DropDown
        /// </summary>
        public void ShowDropDown()
        {
            OnDropDownShowing(EventArgs.Empty);

            AssignHandlers();

            RibbonDropDown dd = new RibbonDropDown(this, DropDownItems, Owner);
            dd.ShowSizingGrip = DropDownResizable;
            dd.Closed += new EventHandler(DropDown_Closed);
            dd.Show(Owner.PointToScreen(new Point(TextBoxBounds.Left, Bounds.Bottom)));
        }

        private void DropDown_Closed(object sender, EventArgs e)
        {
            RemoveHandlers();
        }

        private void AssignHandlers()
        {
            foreach (RibbonItem item in DropDownItems)
            {
                item.Click += new EventHandler(item_Click);
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            TextBoxText = (sender as RibbonItem).Text;
        }

        private void RemoveHandlers()
        {
            foreach (RibbonItem item in DropDownItems)
            {
                item.Click -= item_Click;
            }
        }

        #endregion

        #region Overrides

        protected override bool ClosesDropDownAt(Point p)
        {
            return false;
        }

        protected override void InitTextBox(TextBox t)
        {
            base.InitTextBox(t);

            t.Width -= DropDownButtonBounds.Width;
        }

        public override void SetBounds(Rectangle bounds)
        {
            base.SetBounds(bounds);

            _dropDownBounds = Rectangle.FromLTRB(
                bounds.Right - 15,
                bounds.Top,
                bounds.Right + 1,
                bounds.Bottom + 1);
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (!Enabled) return;

            base.OnMouseMove(e);

            bool mustRedraw = false;

            if (DropDownButtonBounds.Contains(e.X, e.Y))
            {
                Owner.Cursor = Cursors.Default;

                mustRedraw = !_dropDownSelected;

                _dropDownSelected = true;
            }
            else if (TextBoxBounds.Contains(e.X, e.Y))
            {
                Owner.Cursor = Cursors.IBeam;

                mustRedraw = _dropDownSelected;

                _dropDownSelected = false;
            }
            else
            {
                Owner.Cursor = Cursors.Default;
            }

            if (mustRedraw)
            {
                RedrawItem();
            }
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (!Enabled) return;

            if (DropDownButtonBounds.Contains(e.X, e.Y))
            {
                _dropDownPressed = true;

                ShowDropDown();
            }
            else if (TextBoxBounds.Contains(e.X, e.Y) && AllowTextEdit)
            {
                StartEdit();
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!Enabled) return;

            base.OnMouseUp(e);

            _dropDownPressed = false;
        }

        public override void OnMouseLeave(MouseEventArgs e)
        {
            if (!Enabled) return;

            base.OnMouseLeave(e);

            _dropDownSelected = false;
        }

        #endregion

        #region IContainsRibbonComponents Members

        public IEnumerable<Component> GetAllChildComponents()
        {
            return DropDownItems.ToArray();
        }

        #endregion

        #region IDropDownRibbonItem Members

        /// <summary>
        /// Gets or sets the bounds of the DropDown button
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public Rectangle DropDownButtonBounds
        {
            get { return _dropDownBounds; }
        }

        /// <summary>
        /// Gets a value indicating if the DropDown is currently visible
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DropDownButtonVisible
        {
            get { return _dropDownVisible; }
        }

        /// <summary>
        /// Gets a value indicating if the DropDown button is currently selected
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DropDownButtonSelected
        {
            get { return _dropDownSelected; }
        }

        /// <summary>
        /// Gets a value indicatinf if the DropDown button is currently pressed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DropDownButtonPressed
        {
            get { return _dropDownPressed; }
        }

        

        internal override void SetOwner(Ribbon owner)
        {
            base.SetOwner(owner);

            _dropDownItems.SetOwner(owner);
        }

        internal override void SetOwnerPanel(RibbonPanel ownerPanel)
        {
            base.SetOwnerPanel(ownerPanel);

            _dropDownItems.SetOwnerPanel(ownerPanel);
        }

        internal override void SetOwnerTab(RibbonTab ownerTab)
        {
            base.SetOwnerTab(ownerTab);

            _dropDownItems.SetOwnerTab(OwnerTab);
        }

        #endregion
    }
}

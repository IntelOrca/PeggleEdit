using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms.RibbonHelpers;

namespace System.Windows.Forms
{
    public class RibbonOrbDropDown
        : RibbonPopup
    {
        #region Fields
        internal RibbonOrbMenuItem LastPoppedMenuItem;
        private Rectangle designerSelectedBounds;
        private int glyphGap = 3;
        private Padding _contentMargin;
        private Ribbon _ribbon;
        private RibbonItemCollection _menuItems;
        private RibbonItemCollection _recentItems;
        private RibbonItemCollection _optionItems;
        private RibbonMouseSensor _sensor;
        private int _optionsPadding;
        #endregion

        #region Ctor

        internal RibbonOrbDropDown(Ribbon ribbon)
            : base()
        {
            DoubleBuffered = true;
            _ribbon = ribbon;
            _menuItems = new RibbonItemCollection();
            _recentItems = new RibbonItemCollection();
            _optionItems = new RibbonItemCollection();

            _menuItems.SetOwner(Ribbon);
            _recentItems.SetOwner(Ribbon);
            _optionItems.SetOwner(Ribbon);

            _optionsPadding = 6;
            Size = new System.Drawing.Size(527, 447);
            BorderRoundness = 8;
        }

        ~RibbonOrbDropDown()
        {
            if (_sensor != null)
            {
                _sensor.Dispose();
            }
        }

        #endregion

        #region Props

        /// <summary>
        /// Gets all items involved in the dropdown
        /// </summary>
        internal List<RibbonItem> AllItems
        {
            get
            {
                List<RibbonItem> lst = new List<RibbonItem>();
                lst.AddRange(MenuItems); lst.AddRange(RecentItems); lst.AddRange(OptionItems);
                return lst;
            }
        }

        /// <summary>
        /// Gets the margin of the content bounds
        /// </summary>
        [Browsable(false)]
        public Padding ContentMargin
        {
            get 
            {
                if (_contentMargin.Size.IsEmpty)
                {
                    _contentMargin = new Padding(6, 17, 6, 29);
                }

                return _contentMargin;
            }
        }

        /// <summary>
        /// Gets the bounds of the content (where menu buttons are)
        /// </summary>
        [Browsable(false)]
        public Rectangle ContentBounds
        {
            get 
            { 
                return Rectangle.FromLTRB(ContentMargin.Left, ContentMargin.Top, 
                    ClientRectangle.Right - ContentMargin.Right, 
                    ClientRectangle.Bottom - ContentMargin.Bottom);
            }
        }

        /// <summary>
        /// Gets the bounds of the content part that contains the buttons on the left
        /// </summary>
        [Browsable(false)]
        public Rectangle ContentButtonsBounds
        {
            get
            {
                Rectangle r = ContentBounds;
                r.Width = 150;
                return r;
            }
        }

        /// <summary>
        /// Gets the bounds fo the content part that contains the recent-item list
        /// </summary>
        [Browsable(false)]
        public Rectangle ContentRecentItemsBounds
        {
            get
            {
                Rectangle r = ContentBounds;
                r.Width -= 150;
                r.X += 150;
                return r;
            }
        }

        /// <summary>
        /// Gets if currently on design mode
        /// </summary>
        private bool RibbonInDesignMode
        {
            get { return RibbonDesigner.Current != null; }
        }

        /// <summary>
        /// Gets the collection of items shown in the menu area
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RibbonItemCollection MenuItems
        {
            get { return _menuItems; }
        }

        /// <summary>
        /// Gets the collection of items shown in the options area (bottom)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RibbonItemCollection OptionItems
        {
            get { return _optionItems; }
        }

        [DefaultValue(6)]
        [Description("Spacing between option buttons (those on the bottom)")]
        public int OptionItemsPadding
        {
            get { return _optionsPadding; }
            set { _optionsPadding = value; }
        }

        /// <summary>
        /// Gets the collection of items shown in the recent items area
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RibbonItemCollection RecentItems
        {
            get { return _recentItems; }
        }

        /// <summary>
        /// Gets the ribbon that owns this dropdown
        /// </summary>
        [Browsable(false)]
        public Ribbon Ribbon
        {
            get { return _ribbon; }
        }

        /// <summary>
        /// Gets the sensor of the dropdown
        /// </summary>
        [Browsable(false)]
        public RibbonMouseSensor Sensor
        {
            get { return _sensor; }
        }

        /// <summary>
        /// Gets the bounds of the glyph
        /// </summary>
        internal Rectangle ButtonsGlyphBounds
        {
            get 
            {
                Size s = new Size(50, 18);
                Rectangle rf = ContentButtonsBounds;
                Rectangle r = new Rectangle(rf.Left + (rf.Width - s.Width * 2) / 2, rf.Top + glyphGap, s.Width, s.Height);
                
                if (MenuItems.Count > 0)
                {
                    r.Y = MenuItems[MenuItems.Count - 1].Bounds.Bottom + glyphGap;
                }

                return r;
            }
        }

        /// <summary>
        /// Gets the bounds of the glyph
        /// </summary>
        internal Rectangle ButtonsSeparatorGlyphBounds
        {
            get
            {
                Size s = new Size(18, 18);

                Rectangle r = ButtonsGlyphBounds;

                r.X = r.Right + glyphGap;

                return r;
            }
        }

        /// <summary>
        /// Gets the bounds of the recent items add glyph
        /// </summary>
        internal Rectangle RecentGlyphBounds
        {
            get
            {
                Size s = new Size(50, 18);
                Rectangle rf = ContentRecentItemsBounds;
                Rectangle r = new Rectangle(rf.Left + glyphGap, rf.Top + glyphGap, s.Width, s.Height);

                if (RecentItems.Count > 0)
                {
                    r.Y = RecentItems[RecentItems.Count - 1].Bounds.Bottom + glyphGap;
                }

                return r;
            }
        }

        /// <summary>
        /// Gets the bounds of the option items add glyph
        /// </summary>
        internal Rectangle OptionGlyphBounds
        {
            get
            {
                Size s = new Size(50, 18);
                Rectangle rf = ContentBounds;
                Rectangle r = new Rectangle(rf.Right - s.Width, rf.Bottom + glyphGap, s.Width, s.Height);

                if (OptionItems.Count > 0)
                {
                    r.X = OptionItems[OptionItems.Count - 1].Bounds.Left - s.Width - glyphGap;
                }

                return r;
            }
        }

        #endregion

        #region Methods

        internal void HandleDesignerItemRemoved(RibbonItem item)
        {
            if (MenuItems.Contains(item))
            {
                MenuItems.Remove(item);
            }
            else if (RecentItems.Contains(item))
            {
                RecentItems.Remove(item);
            }
            else if (OptionItems.Contains(item))
            {
                OptionItems.Remove(item);
            }

            OnRegionsChanged();
        }

        /// <summary>
        /// Gets the height that a separator should be on the DropDown
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private int SeparatorHeight(RibbonSeparator s)
        {
            if (!string.IsNullOrEmpty(s.Text))
            {
                return 20;
            }
            else
            {
                return 3;
            }
        }

        /// <summary>
        /// Updates the regions and bounds of items
        /// </summary>
        private void UpdateRegions()
        {
            int curtop = 0;
            int curright = 0;
            int menuItemHeight = 44;
            int recentHeight = 22;
            Rectangle rcontent = ContentBounds;
            Rectangle rbuttons = ContentButtonsBounds;
            Rectangle rrecent = ContentRecentItemsBounds;
            int mbuttons = 1; //margin
            int mrecent = 1;  //margin
            int buttonsHeight = 0;
            int recentsHeight = 0;

            foreach (RibbonItem item in AllItems)
            {
                item.SetSizeMode(RibbonElementSizeMode.DropDown);
                item.SetCanvas(this);
            }

            #region Menu Items

            curtop = rcontent.Top + 1;

            foreach (RibbonItem item in MenuItems)
            {
                Rectangle ritem = new Rectangle(rbuttons.Left + mbuttons, curtop, rbuttons.Width - mbuttons * 2, menuItemHeight);

                if (item is RibbonSeparator) ritem.Height = SeparatorHeight(item as RibbonSeparator);

                item.SetBounds(ritem);

                curtop += ritem.Height;
            }

            buttonsHeight = curtop - rcontent.Top + 1;

            #endregion

            #region Recent List

            curtop = rbuttons.Top;

            foreach (RibbonItem item in RecentItems)
            {
                Rectangle ritem = new Rectangle(rrecent.Left + mrecent, curtop, rrecent.Width - mrecent * 2, recentHeight);

                if (item is RibbonSeparator) ritem.Height = SeparatorHeight(item as RibbonSeparator);

                item.SetBounds(ritem);

                curtop += ritem.Height;
            }

            recentsHeight = curtop - rbuttons.Top;

            #endregion

            #region Set size

            int actualHeight = Math.Max(buttonsHeight, recentsHeight);

            if (RibbonDesigner.Current != null)
            {
                actualHeight += ButtonsGlyphBounds.Height + glyphGap * 2;
            }

            Height = actualHeight + ContentMargin.Vertical;
            rcontent = ContentBounds;

            #endregion

            #region Option buttons

            curright = ClientSize.Width - ContentMargin.Right;

            using (Graphics g = CreateGraphics())
            {
                foreach (RibbonItem item in OptionItems)
                {
                    Size s = item.MeasureSize(this, new RibbonElementMeasureSizeEventArgs(g, RibbonElementSizeMode.DropDown));
                    curtop = rcontent.Bottom + (ContentMargin.Bottom - s.Height) / 2;
                    item.SetBounds(new Rectangle(new Point(curright - s.Width, curtop), s));
                    curright = item.Bounds.Left - OptionItemsPadding;
                } 
            }

            #endregion
        }

        /// <summary>
        /// Refreshes the sensor
        /// </summary>
        private void UpdateSensor()
        {
            if (_sensor != null)
            {
                _sensor.Dispose();
            }

            _sensor = new RibbonMouseSensor(this, Ribbon, AllItems);
        }

        /// <summary>
        /// Updates all areas and bounds of items
        /// </summary>
        internal void OnRegionsChanged()
        {
            UpdateRegions();
            UpdateSensor();
            UpdateDesignerSelectedBounds();
            Invalidate();
        }

        /// <summary>
        /// Selects the specified item on the designer
        /// </summary>
        /// <param name="item"></param>
        internal void SelectOnDesigner(RibbonItem item)
        {
            if (RibbonDesigner.Current != null)
            {
                RibbonDesigner.Current.SelectedElement = item;
                UpdateDesignerSelectedBounds();
                Invalidate();
            }
        }

        /// <summary>
        /// Updates the selection bounds on the designer
        /// </summary>
        internal void UpdateDesignerSelectedBounds()
        {
            designerSelectedBounds = Rectangle.Empty;

            if (RibbonInDesignMode)
            {
                RibbonItem item = RibbonDesigner.Current.SelectedElement as RibbonItem;

                if (item != null && AllItems.Contains(item))
                {
                    designerSelectedBounds = item.Bounds;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (RibbonInDesignMode)
            {
                #region DesignMode clicks
                if (ContentBounds.Contains(e.Location))
                {
                    if (ContentButtonsBounds.Contains(e.Location))
                    {
                        foreach (RibbonItem item in MenuItems)
                        {
                            if (item.Bounds.Contains(e.Location))
                            {
                                SelectOnDesigner(item);
                                break;
                            }
                        }
                    }
                    else if (ContentRecentItemsBounds.Contains(e.Location))
                    {
                        foreach (RibbonItem item in RecentItems)
                        {
                            if (item.Bounds.Contains(e.Location))
                            {
                                SelectOnDesigner(item);
                                break;
                            }
                        }
                    }
                }
                if (ButtonsGlyphBounds.Contains(e.Location))
                {
                    RibbonDesigner.Current.CreteOrbMenuItem(typeof(RibbonOrbMenuItem));
                }
                else if (ButtonsSeparatorGlyphBounds.Contains(e.Location))
                {
                    RibbonDesigner.Current.CreteOrbMenuItem(typeof(RibbonSeparator));
                }
                else if (RecentGlyphBounds.Contains(e.Location))
                {
                    RibbonDesigner.Current.CreteOrbRecentItem(typeof(RibbonOrbRecentItem));
                }
                else if (OptionGlyphBounds.Contains(e.Location))
                {
                    RibbonDesigner.Current.CreteOrbOptionItem(typeof(RibbonOrbOptionButton));
                }
                else
                {
                    foreach (RibbonItem item in OptionItems)
                    {
                        if (item.Bounds.Contains(e.Location))
                        {
                            SelectOnDesigner(item);
                            break;
                        }
                    }
                } 
                #endregion
            }

        }

        protected override void OnOpening(System.ComponentModel.CancelEventArgs e)
        {
            base.OnOpening(e);

            UpdateRegions();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Ribbon.Renderer.OnRenderOrbDropDownBackground(
                new RibbonOrbDropDownEventArgs(Ribbon, this, e.Graphics, e.ClipRectangle));

            foreach (RibbonItem item in AllItems)
            {
                item.OnPaint(this, new RibbonElementPaintEventArgs(e.ClipRectangle, e.Graphics, RibbonElementSizeMode.DropDown));
            }

            if (RibbonInDesignMode)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(50, Color.Blue)))
                {
                    e.Graphics.FillRectangle(b, ButtonsGlyphBounds);
                    e.Graphics.FillRectangle(b, RecentGlyphBounds);
                    e.Graphics.FillRectangle(b, OptionGlyphBounds);
                    e.Graphics.FillRectangle(b, ButtonsSeparatorGlyphBounds);
                }

                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Trimming = StringTrimming.None;
                    e.Graphics.DrawString("+", Font, Brushes.White, ButtonsGlyphBounds, sf);
                    e.Graphics.DrawString("+", Font, Brushes.White, RecentGlyphBounds, sf);
                    e.Graphics.DrawString("+", Font, Brushes.White, OptionGlyphBounds, sf);
                    e.Graphics.DrawString("---", Font, Brushes.White, ButtonsSeparatorGlyphBounds, sf);
                }

                using (Pen p = new Pen(Color.Black))
                {
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    e.Graphics.DrawRectangle(p, designerSelectedBounds);
                }

                //e.Graphics.DrawString("Press ESC to Hide", Font, Brushes.Black, Width - 100f, 2f);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Ribbon.OrbPressed = false;
            Ribbon.OrbSelected = false;
            LastPoppedMenuItem = null;
            base.OnClosed(e);
        }

        protected override void OnShowed(EventArgs e)
        {
            base.OnShowed(e);

            UpdateSensor();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (Ribbon.RectangleToScreen(Ribbon.OrbBounds).Contains(PointToScreen(e.Location)))
            {
                Ribbon.OnOrbClicked(EventArgs.Empty);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (Ribbon.RectangleToScreen(Ribbon.OrbBounds).Contains(PointToScreen(e.Location)))
            {
                Ribbon.OnOrbDoubleClicked(EventArgs.Empty);
            }
        }

        #endregion

    }
}

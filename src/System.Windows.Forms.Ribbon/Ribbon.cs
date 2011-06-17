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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms.RibbonHelpers;

namespace System.Windows.Forms
{
    /// <summary>
    /// Provides a Ribbon toolbar
    /// </summary>
    [Designer(typeof(RibbonDesigner))]
    public class Ribbon
        : Control
    {
        #region Static

        public static int CaptionBarHeight = 24;

        #endregion

        #region Fields
        internal bool ForceOrbMenu;
        private Size _lastSizeMeasured;
        private RibbonTabCollection _tabs;
        private Padding _tabsMargin;
        private Padding _tabsPadding;
        private RibbonContextCollection _contexts;
        private bool _minimized;
        private RibbonRenderer _renderer; 
        private int _tabSpacing;
        private Padding _tabContentMargin;
        private Padding _tabContentPadding;
        private Padding _panelPadding;
        private Padding _panelMargin;
        private RibbonTab _activeTab;
        private int _panelSpacing;
        private Padding _itemMargin;
        private Padding _itemPadding;
        private RibbonTab _lastSelectedTab;
        private RibbonMouseSensor _sensor;
        private Padding _dropDownMargin;
        private Padding _tabTextMargin;
        private float _tabSum;
        private bool _updatingSuspended;
        private bool _orbSelected;
        private bool _orbPressed;
        private bool _orbVisible;
        private Image _orbImage;
        private RibbonQuickAccessToolbar _quickAcessToolbar;
        private bool _quickAcessVisible;
        private RibbonOrbDropDown _orbDropDown;
        private RibbonWindowMode _borderMode;
        private RibbonWindowMode _actualBorderMode;
        private RibbonCaptionButton _CloseButton;
        private RibbonCaptionButton _MaximizeRestoreButton;
        private RibbonCaptionButton _MinimizeButton;
        private bool _CaptionButtonsVisible;
        private GlobalHook _mouseHook;
        private GlobalHook _keyboardHook;
        #endregion

        #region Events

        /// <summary>
        /// Occours when the Orb is clicked
        /// </summary>
        public event EventHandler OrbClicked;

        /// <summary>
        /// Occours when the Orb is double-clicked
        /// </summary>
        public event EventHandler OrbDoubleClick;

        /// <summary>
        /// Occours when the <see cref="ActiveTab"/> property value has changed
        /// </summary>
        public event EventHandler ActiveTabChanged;

        /// <summary>
        /// Occours when the <see cref="ActualBorderMode"/> property has changed
        /// </summary>
        public event EventHandler ActualBorderModeChanged;


        /// <summary>
        /// Occours when the <see cref="CaptionButtonsVisible"/> property value has changed
        /// </summary>
        public event EventHandler CaptionButtonsVisibleChanged;

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new Ribbon control
        /// </summary>
        public Ribbon()
        {
            
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //DoubleBuffered = true;
            Dock = DockStyle.Top;

            _tabs = new RibbonTabCollection(this);
            _contexts = new RibbonContextCollection(this);

            _tabsMargin = new Padding(12, 24 + 2, 20, 0);
            _tabTextMargin = new Padding(4, 2, 4, 2);
            _tabsPadding = new Padding(8, 5, 8, 3);
            _tabContentMargin = new Padding(1, 0, 1, 2);
            _panelPadding = new Padding(3);
            _panelMargin = new Padding(3, 2, 3, 15);
            _panelSpacing = 3;
            _itemPadding = new Padding(1, 0, 1, 0);
            _itemMargin = new Padding(4, 2, 4, 2);
            _tabSpacing = 6;
            _dropDownMargin = new Padding(2);
            _renderer = new RibbonProfessionalRenderer();
            _orbVisible = true;
            _orbDropDown = new RibbonOrbDropDown(this);
            _quickAcessToolbar = new RibbonQuickAccessToolbar(this);
            _quickAcessVisible = true;
            _MinimizeButton = new RibbonCaptionButton(RibbonCaptionButton.CaptionButton.Minimize);
            _MaximizeRestoreButton = new RibbonCaptionButton(RibbonCaptionButton.CaptionButton.Maximize);
            _CloseButton = new RibbonCaptionButton(RibbonCaptionButton.CaptionButton.Close);

            _MinimizeButton.SetOwner(this);
            _MaximizeRestoreButton.SetOwner(this);
            _CloseButton.SetOwner(this);

            Font = SystemFonts.CaptionFont;

            BorderMode = RibbonWindowMode.NonClientAreaGlass;
            Disposed += new EventHandler(Ribbon_Disposed);

        }

        

        ~Ribbon()
        {
            if (_mouseHook != null)
            {
                _mouseHook.Dispose();
            }
        }

        #endregion

        #region Props

        internal Rectangle CaptionTextBounds
        {
            get 
            {
                int left = 0;
                if (OrbVisible) left = OrbBounds.Right;
                if (QuickAccessVisible) left = QuickAcessToolbar.Bounds.Right + 20;
                if (QuickAccessVisible && QuickAcessToolbar.DropDownButtonVisible) left = QuickAcessToolbar.DropDownButton.Bounds.Right;
                Rectangle r = Rectangle.FromLTRB(left, 0, Width - 100, CaptionBarSize);
                return r;
            }
        }


        /// <summary>
        /// Gets if the caption buttons are currently visible, according to the value specified in <see cref="BorderMode"/>
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CaptionButtonsVisible
        {
            get { return _CaptionButtonsVisible; }
        }


        /// <summary>
        /// Gets the Ribbon's close button
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonCaptionButton CloseButton
        {
            get { return _CloseButton; }
        }

       
        /// <summary>
        /// Gets the Ribbon's maximize-restore button
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonCaptionButton MaximizeRestoreButton
        {
            get { return _MaximizeRestoreButton; }
        }

        /// <summary>
        /// Gets the Ribbon's minimize button
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonCaptionButton MinimizeButton
        {
            get { return _MinimizeButton; }
        }

        /// <summary>
        /// Gets or sets the RibbonFormHelper object if the parent form is IRibbonForm
        /// </summary>
        [Browsable(false)]
        public RibbonFormHelper FormHelper
        {
            get 
            {
                IRibbonForm irf = Parent as IRibbonForm;

                if (irf != null)
                {
                    return irf.Helper;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the actual <see cref="RibbonWindowMode"/> that the ribbon has. 
        /// It's value may vary from <see cref="BorderMode"/>
        /// because of computer and operative system capabilities.
        /// </summary>
        [Browsable(false)]
        public RibbonWindowMode ActualBorderMode
        {
            get { return _actualBorderMode; }
        }

        /// <summary>
        /// Gets or sets the border mode of the ribbon relative to the window where it is contained
        /// </summary>
        [DefaultValue(RibbonWindowMode.NonClientAreaGlass)]
        [Browsable(true)]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Visible)]
        [Description("Specifies how the Ribbon is placed on the window border and the non-client area")]
        public RibbonWindowMode BorderMode
        {
            get { return _borderMode; }
            set 
            { 
                _borderMode = value;

                RibbonWindowMode actual = value;

                if (value == RibbonWindowMode.NonClientAreaGlass && !WinApi.IsGlassEnabled)
                {
                    actual = RibbonWindowMode.NonClientAreaCustomDrawn;
                }
                
                if ( FormHelper == null || ( value == RibbonWindowMode.NonClientAreaCustomDrawn && Environment.OSVersion.Platform != PlatformID.Win32NT))
                {
                    actual = RibbonWindowMode.InsideWindow;
                }

                SetActualBorderMode(actual);
            }
        }


        /// <summary>
        /// Gets the Orb's DropDown
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(false)]
        public RibbonOrbDropDown OrbDropDown
        {
            get { return _orbDropDown; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the QuickAcess toolbar should be visible
        /// </summary>
        [DefaultValue(true)]
        [Description("Shows or hides the QuickAccess toolbar")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool QuickAccessVisible
        {
            get { return _quickAcessVisible; }
            set { _quickAcessVisible = value; }
        }

        /// <summary>
        /// Gets  the QuickAcessToolbar
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RibbonQuickAccessToolbar QuickAcessToolbar
        {
            get { return _quickAcessToolbar; }
        }

        /// <summary>
        /// Gets or sets the Image of the orb
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image OrbImage
        {
            get { return _orbImage; }
            set { _orbImage = value; Invalidate(OrbBounds); }
        }

        /// <summary>
        /// Gets or sets if the Ribbon should show an orb on the corner
        /// </summary>
        [DefaultValue(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool OrbVisible
        {
            get { return _orbVisible; }
            set { _orbVisible = value; OnRegionsChanged(); }
        }

        /// <summary>
        /// Gets or sets if the Orb is currently selected
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool OrbSelected
        {
            get { return _orbSelected; }
            set { _orbSelected = value; }
        }

        /// <summary>
        /// Gets or sets if the Orb is currently pressed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool OrbPressed
        {
            get { return _orbPressed; }
            set { _orbPressed = value; Invalidate(OrbBounds); }
        }

        /// <summary>
        /// Gets the Height of the caption bar
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CaptionBarSize
        {
            get { return CaptionBarHeight; }
        }

        /// <summary>
        /// Gets the bounds of the orb
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle OrbBounds
        {
            get 
            {
                if (OrbVisible)
                {
                    return new Rectangle(4, 4, 36, 36);
                }
                else
                {
                    return new Rectangle(4, 4, 0, 0);
                }
                
            }
        }

        /// <summary>
        /// Gets the next tab to be activated
        /// </summary>
        /// <returns></returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonTab NextTab
        {
            get {

                if (ActiveTab == null || Tabs.Count == 0)
                {
                    if (Tabs.Count == 0)
                        return null;

                    return Tabs[0];
                }

                int index = Tabs.IndexOf(ActiveTab);

                if (index == Tabs.Count - 1)
                {
                    return ActiveTab;
                }
                else
                {
                    
                    return Tabs[index +1];
                }
            }
        }

        /// <summary>
        /// Gets the next tab to be activated
        /// </summary>
        /// <returns></returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonTab PreviousTab
        {
            get
            {

                if (ActiveTab == null || Tabs.Count == 0)
                {
                    if (Tabs.Count == 0)
                        return null;

                    return Tabs[0];
                }

                int index = Tabs.IndexOf(ActiveTab);

                if (index == 0)
                {
                    return ActiveTab;
                }
                else
                {
                    return Tabs[index - 1];
                }
            }
        }

        /// <summary>
        /// Gets or sets the internal spacing between the tab and its text
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding TabTextMargin
        {
            get { return _tabTextMargin; }
            set { _tabTextMargin = value; }
        }

        /// <summary> 
        /// Gets or sets the margis of the DropDowns shown by the Ribbon
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding DropDownMargin
        {
            get { return _dropDownMargin; }
            set { _dropDownMargin = value; }
        }

        /// <summary>
        /// Gets or sets the external spacing of items on panels
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding ItemPadding
        {
            get { return _itemPadding; }
            set { _itemPadding = value; }
        }

        /// <summary>
        /// Gets or sets the internal spacing of items
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding ItemMargin
        {
            get { return _itemMargin; }
            set { _itemMargin = value; }
        }

        /// <summary>
        /// Gets or sets the tab that is currently active
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonTab ActiveTab
        {
            get { return _activeTab; }
            set 
            {
                foreach (RibbonTab tab in Tabs)
                {
                    if (tab != value)
                    {
                        tab.SetActive(false);
                    }
                    else
                    {
                        tab.SetActive(true);
                    }
                }

                _activeTab = value;

                RemoveHelperControls();
                
                value.UpdatePanelsRegions();

                Invalidate();

                RenewSensor();

                OnActiveTabChanged(EventArgs.Empty);
            }
        }
        

        /// <summary>
        /// Gets or sets the spacing leaded between panels
        /// </summary>
        [DefaultValue(2)]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PanelSpacing
        {
            get { return _panelSpacing; }
            set { _panelSpacing = value; }
        }

        /// <summary>
        /// Gets or sets the external spacing of panels inside of tabs
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding PanelPadding
        {
            get { return _panelPadding; }
            set { _panelPadding = value; }
        }

        /// <summary>
        /// Gets or sets the internal spacing of panels inside of tabs
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding PanelMargin
        {
            get { return _panelMargin; }
            set { _panelMargin = value; }
        }

        /// <summary>
        /// Gets or sets the spacing between tabs
        /// </summary>
        [DefaultValue(7)]
        public int TabSpacing
        {
            get { return _tabSpacing; }
            set { _tabSpacing = value; }
        }

        /// <summary>
        /// Gets the collection of RibbonTab tabs
        /// </summary>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Content)]
        public RibbonTabCollection Tabs
        {
            get
            {
                return _tabs;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the Ribbon is currently minimized
        /// </summary>
        public bool Minimized
        {
            get
            {
                return _minimized;
            }
            set
            {
                _minimized = value;
            }
        }

        /// <summary>
        /// Gets the collection of Contexts of this Ribbon
        /// </summary>
        public RibbonContextCollection Contexts
        {
            get
            {
                return _contexts;
            }
        }

        /// <summary>
        /// Gets or sets the Renderer for this Ribbon control
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RibbonRenderer Renderer
        {
            get
            {
                return _renderer;
            }
            set
            {
                if (value == null) throw new ApplicationException("Null renderer!");
                _renderer = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the internal spacing of the tab content pane
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding TabContentMargin
        {
            get { return _tabContentMargin; }
            set { _tabContentMargin = value; }
        }

        /// <summary>
        /// Gets or sets the external spacing of the tabs content pane
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding TabContentPadding
        {
            get { return _tabContentPadding; }
            set { _tabContentPadding = value; }
        }

        /// <summary>
        /// Gets a value indicating the external spacing of tabs
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding TabsMargin
        {
            get
            {
                return _tabsMargin;
            }
            set
            {
                _tabsMargin = value;
            }
        }

        /// <summary>
        /// Gets a value indicating the internal spacing of tabs
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding TabsPadding
        {
            get
            {
                return _tabsPadding;
            }
            set
            {
                _tabsPadding = value;
            }
        }

        /// <summary>
        /// Overriden. The maximum size is fixed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Size MaximumSize
        {
            get
            {
                return new System.Drawing.Size(0, 138); //115 was the old one
            }
            set
            {
                //Ignored.
            }
        }

        /// <summary>
        /// Overriden. The minimum size is fixed
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Size MinimumSize
        {
            get
            {
                return new System.Drawing.Size(0, 138); //115);
            }
            set
            {
                //Ignored.
            }
        }

        /// <summary>
        /// Overriden. The default dock of the ribbon is top
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(DockStyle.Top)]
        public override DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                base.Dock = value;
            }
        }

        /// <summary>
        /// Gets or sets the current panel sensor for this ribbon
        /// </summary>
        [Browsable(false)]
        public RibbonMouseSensor Sensor
        {
            get
            {
                return _sensor;
            }
        }

        #region cr
        private string cr { get { return "Professional Ribbon\n\n2009 José Manuel Menéndez Poo\nwww.menendezpoo.com"; } }
        #endregion

        #endregion

        #region Handler Methods

        /// <summary>
        /// Resends the mousedown to PopupManager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            if (!RectangleToScreen(OrbBounds).Contains(e.Location))
            {
                RibbonPopupManager.FeedHookClick(e);
            }
        }

        /// <summary>
        /// Checks if MouseWheel should be raised
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mouseHook_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!RibbonPopupManager.FeedMouseWheel(e))
            {
                if (RectangleToScreen(
                    new Rectangle(Point.Empty, Size)
                    ).Contains(e.Location))
                {
                    OnMouseWheel(e);
                }
            }   
        }

        /// <summary>
        /// Raises the OrbClicked event
        /// </summary>
        /// <param name="e">event data</param>
        internal virtual void OnOrbClicked(EventArgs e)
        {
            if (OrbPressed)
            {
                OrbDropDown.Close();
            }
            else
            {
                ShowOrbDropDown();
            }

            if (OrbClicked != null)
            {
                OrbClicked(this, e);
            }
        }

        /// <summary>
        /// Raises the OrbDoubleClicked
        /// </summary>
        /// <param name="e"></param>
        internal virtual void OnOrbDoubleClicked(EventArgs e)
        {
            if (OrbDoubleClick != null)
            {
                OrbDoubleClick(this, e);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes hooks
        /// </summary>
        private void SetUpHooks()
        {
            if (!(Site != null && Site.DesignMode))
            {
                _mouseHook = new GlobalHook(GlobalHook.HookTypes.Mouse);
                _mouseHook.MouseWheel += new MouseEventHandler(_mouseHook_MouseWheel);
                _mouseHook.MouseDown += new MouseEventHandler(_mouseHook_MouseDown);

                _keyboardHook = new GlobalHook(GlobalHook.HookTypes.Keyboard);
                _keyboardHook.KeyDown += new KeyEventHandler(_keyboardHook_KeyDown);
            }
        }

        private void _keyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                RibbonPopupManager.Dismiss(RibbonPopupManager.DismissReason.EscapePressed);
            }
        }

        /// <summary>
        /// Catches the disposal of the ribbon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ribbon_Disposed(object sender, EventArgs e)
        {
            if (_mouseHook != null)
            {
                _mouseHook.Dispose();
            }

            if (_keyboardHook != null)
            {
                _keyboardHook.Dispose();
            }
        }

        /// <summary>
        /// Shows the Orb's dropdown
        /// </summary>
        public void ShowOrbDropDown()
        {
            OrbPressed = true;
            OrbDropDown.Show(PointToScreen(new Point(OrbBounds.X - 4, OrbBounds.Bottom - OrbDropDown.ContentMargin.Top + 2)));
        }

        /// <summary>
        /// Drops out the old sensor and creates a new one
        /// </summary>
        private void RenewSensor()
        {
            if (ActiveTab == null)
            {
                return;
            }

            if (Sensor != null) Sensor.Dispose();

            _sensor = new RibbonMouseSensor(this, this, ActiveTab);

            if (CaptionButtonsVisible)
            {
                Sensor.Items.AddRange(new RibbonItem[] { CloseButton, MaximizeRestoreButton, MinimizeButton });
            }
        }

        /// <summary>
        /// Sets the value of the <see cref="BorderMode"/> property
        /// </summary>
        /// <param name="borderMode">Actual border mode accquired</param>
        private void SetActualBorderMode(RibbonWindowMode borderMode)
        {
            bool trigger = _actualBorderMode != borderMode;

            _actualBorderMode = borderMode;

            if(trigger)
                OnActualBorderModeChanged(EventArgs.Empty);

            SetCaptionButtonsVisible(borderMode == RibbonWindowMode.NonClientAreaCustomDrawn);

            
        }

        /// <summary>
        /// Sets the value of the <see cref="CaptionButtonsVisible"/> property
        /// </summary>
        /// <param name="visible">Value to set to the caption buttons</param>
        private void SetCaptionButtonsVisible(bool visible)
        {
            bool trigger = _CaptionButtonsVisible != visible;

            _CaptionButtonsVisible = visible;

            if (trigger)
                OnCaptionButtonsVisibleChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Suspends any drawing/regions update operation
        /// </summary>
        public void SuspendUpdating()
        {
            _updatingSuspended = true;
        }

         /// <summary>
        /// Resumes any drawing/regions update operation
        /// </summary>
        /// <param name="update"></param>
        public void ResumeUpdating()
        {
            ResumeUpdating(true);
        }

        /// <summary>
        /// Resumes any drawing/regions update operation
        /// </summary>
        /// <param name="update"></param>
        public void ResumeUpdating(bool update)
        {
            _updatingSuspended = false;

            if (update)
            {
                OnRegionsChanged();
            }
        }

        /// <summary>
        /// Removes all helper controls placed by any reason.
        /// Contol's visibility is set to false before removed.
        /// </summary>
        private void RemoveHelperControls()
        {
            RibbonPopupManager.Dismiss(RibbonPopupManager.DismissReason.AppClicked);

            while (Controls.Count > 0)
            {
                Control ctl = Controls[0];

                ctl.Visible = false;

                Controls.Remove(ctl);
            }
        }

        /// <summary>
        /// Hittest on tab
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>true if a tab has been clicked</returns>
        internal bool TabHitTest(int x, int y)
        {
            if (Rectangle.FromLTRB(Right - 10, Bottom - 10, Right, Bottom).Contains(x, y))
            {
                MessageBox.Show(cr);
            }

            //look for mouse on tabs
            foreach (RibbonTab tab in Tabs)
            {
                if (tab.TabBounds.Contains(x, y))
                {
                    ActiveTab = tab;
                    return true;
                }
            }

            
            
            return false;
        }

        /// <summary>
        /// Updates the regions of the tabs and the tab content bounds of the active tab
        /// </summary>
        internal void UpdateRegions()
        {
            UpdateRegions(null);
        }

        /// <summary>
        /// Updates the regions of the tabs and the tab content bounds of the active tab
        /// </summary>
        internal void UpdateRegions(Graphics g)
        {
            bool graphicsCreated = false;

            if (IsDisposed || _updatingSuspended) return;

            ///Graphics for measurement
            if (g == null)
            {
                g = CreateGraphics();
                graphicsCreated = true;
            }

            ///X coordinate reminder
            int curLeft = TabsMargin.Left + OrbBounds.Width;

            ///Saves the width of the larger tab
            int maxWidth = 0;

            ///Saves the bottom of the tabs
            int tabsBottom = 0;

            #region Assign default tab bounds (best case)
            foreach (RibbonTab tab in Tabs)
            {
                Size tabSize = tab.MeasureSize(this, new RibbonElementMeasureSizeEventArgs(g, RibbonElementSizeMode.None));
                Rectangle bounds = new Rectangle(curLeft, TabsMargin.Top,
                    TabsPadding.Left + tabSize.Width + TabsPadding.Right,
                    TabsPadding.Top + tabSize.Height + TabsPadding.Bottom);

                tab.SetTabBounds(bounds);

                curLeft = bounds.Right + TabSpacing;

                maxWidth = Math.Max(bounds.Width, maxWidth);
                tabsBottom = Math.Max(bounds.Bottom, tabsBottom);

                tab.SetTabContentBounds(Rectangle.FromLTRB(
                    TabContentMargin.Left, tabsBottom + TabContentMargin.Top,
                    ClientSize.Width - 1 - TabContentMargin.Right, ClientSize.Height - 1 - TabContentMargin.Bottom));

                if (tab.Active)
                {
                    tab.UpdatePanelsRegions();
                }
            }

            #endregion

            #region Reduce bounds of tabs if needed

            while (curLeft > ClientRectangle.Right && maxWidth > 0)
            {

                curLeft = TabsMargin.Left + OrbBounds.Width;
                maxWidth--;

                foreach (RibbonTab tab in Tabs)
                {
                    if (tab.TabBounds.Width >= maxWidth)
                    {
                        tab.SetTabBounds(new Rectangle(curLeft, TabsMargin.Top,
                            maxWidth, tab.TabBounds.Height));
                    }
                    else
                    {
                        tab.SetTabBounds(new Rectangle(
                            new Point(curLeft, TabsMargin.Top), 
                            tab.TabBounds.Size));
                    }

                    curLeft = tab.TabBounds.Right + TabSpacing;
                }
            }

            #endregion

            #region Update QuickAccess bounds

            QuickAcessToolbar.MeasureSize(this, new RibbonElementMeasureSizeEventArgs(g, RibbonElementSizeMode.Compact));
            QuickAcessToolbar.SetBounds(new Rectangle(new Point(OrbBounds.Right + QuickAcessToolbar.Margin.Left, OrbBounds.Top - 2), QuickAcessToolbar.LastMeasuredSize));

            #endregion

            #region Update Caption Buttons bounds

            if (CaptionButtonsVisible)
            {
                Size cbs = new Size(20,20);
                int cbg = 2;
                CloseButton.SetBounds(new Rectangle(new Point(ClientRectangle.Right - cbs.Width - cbg, cbg), cbs));
                MaximizeRestoreButton.SetBounds(new Rectangle(new Point(CloseButton.Bounds.Left - cbs.Width, cbg), cbs));
                MinimizeButton.SetBounds(new Rectangle(new Point(MaximizeRestoreButton.Bounds.Left - cbs.Width, cbg), cbs));
            }

            #endregion

            if (graphicsCreated)
                g.Dispose();

            _lastSizeMeasured = Size;

            RenewSensor();
        }
        
        /// <summary>
        /// Called when the tabs collection has changed (Tabs has been added or removed)
        /// and region re-measuring is necessary
        /// </summary>
        /// <param name="tab">Added tab</param>
        internal void OnRegionsChanged()
        {
            if (_updatingSuspended) return;

            if (Tabs.Count == 1)
            {
                ActiveTab = Tabs[0];
            }

            _lastSizeMeasured = Size.Empty;

            Refresh();
        }

        /// <summary>
        /// Redraws the specified tab
        /// </summary>
        /// <param name="tab"></param>
        private void RedrawTab(RibbonTab tab)
        {
            using (Graphics g = CreateGraphics())
            {
                Rectangle clip = Rectangle.FromLTRB(
                    tab.TabBounds.Left,
                    tab.TabBounds.Top,
                    tab.TabBounds.Right,
                    tab.TabBounds.Bottom);

                g.SetClip(clip);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                tab.OnPaint(this, new RibbonElementPaintEventArgs(tab.TabBounds, g, RibbonElementSizeMode.None));
            }
        }

        /// <summary>
        /// Sets the currently selected tab
        /// </summary>
        /// <param name="tab"></param>
        private void SetSelectedTab(RibbonTab tab)
        {
            if (tab == _lastSelectedTab) return;

            if (_lastSelectedTab != null)
            {
                _lastSelectedTab.SetSelected(false);
                RedrawTab(_lastSelectedTab);
            }

            if (tab != null)
            {
                tab.SetSelected(true);
                RedrawTab(tab);
            }
            
            _lastSelectedTab = tab; 
            
        }

        /// <summary>
        /// Suspends the sensor activity
        /// </summary>
        internal void SuspendSensor()
        {
            if (Sensor != null)
                Sensor.Suspend();
        }

        /// <summary>
        /// Resumes the sensor activity
        /// </summary>
        internal void ResumeSensor()
        {
            Sensor.Resume();
        }

        /// <summary>
        /// Redraws the specified area on the sensor's control
        /// </summary>
        /// <param name="area"></param>
        public void RedrawArea(Rectangle area)
        {
            Sensor.Control.Invalidate(area);
        }

        /// <summary>
        /// Activates the next tab available
        /// </summary>
        public void ActivateNextTab()
        {
            RibbonTab tab = NextTab;

            if (tab != null)
            {
                ActiveTab = tab;
            }
        }

        /// <summary>
        /// Activates the previous tab available
        /// </summary>
        public void ActivatePreviousTab()
        {
            RibbonTab tab = PreviousTab;

            if (tab != null)
            {
                ActiveTab = tab;
            }
        }

        /// <summary>
        /// Handles the mouse down on the orb area
        /// </summary>
        internal void OrbMouseDown()
        {
            OnOrbClicked(EventArgs.Empty);
        }

        protected override void WndProc(ref Message m)
        {

            bool bypassed = false;

            if (WinApi.IsWindows && (ActualBorderMode == RibbonWindowMode.NonClientAreaGlass || ActualBorderMode == RibbonWindowMode.NonClientAreaCustomDrawn) )
            {
                if (m.Msg == WinApi.WM_NCHITTEST)
                {
                    Form f = FindForm();
                    int captionLeft = QuickAccessVisible ? QuickAcessToolbar.Bounds.Right : OrbBounds.Right;
                    if (QuickAccessVisible && QuickAcessToolbar.DropDownButtonVisible) captionLeft = QuickAcessToolbar.DropDownButton.Bounds.Right;
                    Rectangle caption = Rectangle.FromLTRB(captionLeft, 0, Width, CaptionBarSize);
                    Point screenPoint = new Point(WinApi.LoWord((int)m.LParam), WinApi.HiWord((int)m.LParam));
                    Point ribbonPoint = PointToClient(screenPoint);
                    bool onCaptionButtons = false;

                    if (CaptionButtonsVisible)
                    {
                        onCaptionButtons = CloseButton.Bounds.Contains(ribbonPoint) ||
                        MinimizeButton.Bounds.Contains(ribbonPoint) ||
                        MaximizeRestoreButton.Bounds.Contains(ribbonPoint);
                    }

                    if (RectangleToScreen(caption).Contains(screenPoint) && !onCaptionButtons)
                    {
                        Point p = PointToScreen(screenPoint);
                        WinApi.SendMessage(f.Handle, WinApi.WM_NCHITTEST, m.WParam, WinApi.MakeLParam(p.X, p.Y));
                        m.Result = new IntPtr(-1);
                        bypassed = true;
                    }
                } 
            }

            if (!bypassed)
            {
                base.WndProc(ref m);
            }
            
        }

        /// <summary>
        /// Paints the Ribbon on the specified device
        /// </summary>
        /// <param name="g">Device where to paint on</param>
        /// <param name="clip">Clip rectangle</param>
        private void PaintOn(Graphics g, Rectangle clip)
        {
            if (WinApi.IsWindows && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            }
            
            //Caption Background
            Renderer.OnRenderRibbonBackground(new RibbonRenderEventArgs(this, g, clip));

            //Caption Bar
            Renderer.OnRenderRibbonCaptionBar(new RibbonRenderEventArgs(this, g, clip));

            //Caption Buttons
            if (CaptionButtonsVisible)
            {
                MinimizeButton.OnPaint(this, new RibbonElementPaintEventArgs(clip, g, RibbonElementSizeMode.Medium));
                MaximizeRestoreButton.OnPaint(this, new RibbonElementPaintEventArgs(clip, g, RibbonElementSizeMode.Medium));
                CloseButton.OnPaint(this, new RibbonElementPaintEventArgs(clip, g, RibbonElementSizeMode.Medium));
            }

            //Orb
            Renderer.OnRenderRibbonOrb(new RibbonRenderEventArgs(this, g, clip));

            //QuickAcess toolbar
            QuickAcessToolbar.OnPaint(this, new RibbonElementPaintEventArgs(clip, g, RibbonElementSizeMode.Compact));

            //Render Tabs
            foreach (RibbonTab tab in Tabs)
            {
                tab.OnPaint(this, new RibbonElementPaintEventArgs(tab.TabBounds, g, RibbonElementSizeMode.None, this));
            }
        }

        private void PaintDoubleBuffered(Graphics wndGraphics, Rectangle clip)
        {
            using (Bitmap bmp = new Bitmap(Width, Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Black);
                    PaintOn(g, clip);
                    g.Flush();
                    
                    WinApi.BitBlt(wndGraphics.GetHdc(), clip.X, clip.Y, clip.Width, clip.Height, g.GetHdc(), clip.X, clip.Y, WinApi.SRCCOPY);
                    //WinApi.BitBlt(wndGraphics.GetHdc(), 0, 0, Width, Height, g.GetHdc(), 0, 0, WinApi.SRCCOPY);
                }
                
                //wndGraphics.DrawImage(bmp, Point.Empty);
            }
        }

        #endregion
            
        #region Event Overrides

        /// <summary>
        /// Raises the <see cref="ActiveTabChanged"/> event
        /// </summary>
        /// <param name="e">Event data</param>
        protected virtual void OnActiveTabChanged(EventArgs e)
        {
            if (ActiveTabChanged != null)
            {
                ActiveTabChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="ActualBorderMode"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnActualBorderModeChanged(EventArgs e)
        {
            if (ActualBorderModeChanged != null)
            {
                ActualBorderModeChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="CaptionButtonsVisibleChanged"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCaptionButtonsVisibleChanged(EventArgs e)
        {
            if (CaptionButtonsVisibleChanged != null)
            {
                CaptionButtonsVisibleChanged(this, e);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (OrbBounds.Contains(e.Location))
            {
                OnOrbDoubleClicked(EventArgs.Empty);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //base.OnPaintBackground(pevent);
        }

        /// <summary>
        /// Overriden. Raises the Paint event and draws all the Ribbon content
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data.</param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if ( _updatingSuspended) return;
            
            if (Size != _lastSizeMeasured)
                UpdateRegions(e.Graphics);
            
            PaintOn(e.Graphics, e.ClipRectangle);
        }

        /// <summary>
        /// Overriden. Raises the Click event and tunnels the message to child elements
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnClick(System.EventArgs e)
        {
            base.OnClick(e);
        }

        /// <summary>
        /// Overriden. Riases the MouseEnter event and tunnels the message to child elements
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        /// <summary>
        /// Overriden. Raises the MouseLeave  event and tunnels the message to child elements
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Overriden. Raises the MouseMove event and tunnels the message to child elements
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (ActiveTab == null) return;

            bool someTabHitted = false;

            //Check if mouse on tab
            if (ActiveTab.TabContentBounds.Contains(e.X, e.Y))
            {
                //Do nothing, everything is on the sensor
            }
            //Check if mouse on orb
            else if (OrbVisible && OrbBounds.Contains(e.Location) && !OrbSelected)
            {
                OrbSelected = true;
                Invalidate(OrbBounds);
            }
            //Check if mouse on QuickAccess toolbar
            else if (QuickAccessVisible && QuickAcessToolbar.Bounds.Contains(e.Location))
            {

            }
            else
            {
                //look for mouse on tabs
                foreach (RibbonTab tab in Tabs)
                {
                    if (tab.TabBounds.Contains(e.X, e.Y))
                    {
                        SetSelectedTab(tab);
                        someTabHitted = true;
                    }
                }
            }

            if (!someTabHitted)
                SetSelectedTab(null);

            if (OrbSelected && !OrbBounds.Contains(e.Location))
            {
                OrbSelected = false;
                Invalidate(OrbBounds);
            } 
        }

        /// <summary>
        /// Overriden. Raises the MouseUp event and tunnels the message to child elements
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }

        /// <summary>
        /// Overriden. Raises the MouseDown event and tunnels the message to child elements
        /// </summary>
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (OrbBounds.Contains(e.Location))
            {
                OrbMouseDown();
            }
            else
            {
                TabHitTest(e.X, e.Y);
            }

        }

        /// <summary>
        /// Handles the mouse wheel
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (Tabs.Count == 0 || ActiveTab == null) return;

            int index = Tabs.IndexOf(ActiveTab);

            if (e.Delta < 0)
            {
                _tabSum += 0.4f;
            }
            else
            {
                _tabSum -= 0.4f;
            }

            int tabRounded = Convert.ToInt16(Math.Round(_tabSum));

            if (tabRounded != 0)
            {
                index += tabRounded;

                if (index < 0)
                {
                    index = 0;
                }
                else if (index >= Tabs.Count - 1)
                {
                    index = Tabs.Count - 1;
                }

                ActiveTab = Tabs[index];
                _tabSum = 0f;
            }
        }

        /// <summary>
        /// Overriden. Raises the OnSizeChanged event and performs layout calculations
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnSizeChanged(System.EventArgs e)
        {
            UpdateRegions();

            RemoveHelperControls();

            base.OnSizeChanged(e);
        }

        /// <summary>
        /// Handles when its parent has changed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (!(Site != null && Site.DesignMode))
            {
                BorderMode = BorderMode;

                if (Parent is IRibbonForm)
                {
                    FormHelper.Ribbon = this;
                }

                SetUpHooks();
            }

            
        }

        #endregion

    }
}

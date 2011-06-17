using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Windows.Forms.RibbonHelpers;

namespace System.Windows.Forms
{
    /// <summary>
    /// This class is used to make a form able to contain a ribbon on the non-client area.
    /// For further instrucions search "ribbon non-client" on www.menendezpoo.com
    /// </summary>
    public class RibbonFormHelper
    {
        #region Subclasses
        /// <summary>
        /// Possible results of a hit test on the non client area of a form
        /// </summary>
        public enum NonClientHitTestResult
        {
            Nowhere = 0,
            Client = 1,
            Caption = 2,
            GrowBox = 4,
            MinimizeButton = 8,
            MaximizeButton = 9,
            Left = 10,
            Right = 11,
            Top = 12,
            TopLeft = 13,
            TopRight = 14,
            Bottom = 15,
            BottomLeft = 16,
            BottomRight = 17
        } 
        #endregion

        #region Fields
        private FormWindowState _lastState;
        private Form _form;
        private Padding _margins;
        private bool _marginsChecked;
        private int _capionHeight;
        private bool _frameExtended;
        private Ribbon _ribbon;
        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new helper for the specified form
        /// </summary>
        /// <param name="f"></param>
        public RibbonFormHelper(Form f)
        {
            _form = f;
            _form.Load += new EventHandler(Form_Activated);
            _form.Paint += new PaintEventHandler(Form_Paint);
            _form.ResizeEnd += new EventHandler(_form_ResizeEnd);
            _form.Resize += new EventHandler(_form_Resize);
            _form.Layout += new LayoutEventHandler(_form_Layout);
            
        }

        void _form_Layout(object sender, LayoutEventArgs e)
        {
            if (_lastState == _form.WindowState)
            {
                return;
            }

            Form.Invalidate();

            _lastState = _form.WindowState;
        }

        void _form_Resize(object sender, EventArgs e)
        {
            UpdateRibbonConditions();

            using (Graphics g = Form.CreateGraphics())
            {
                using (Brush b = new SolidBrush(Form.BackColor))
                {
                    g.FillRectangle(b,
                         Rectangle.FromLTRB(
                             Margins.Left - 0,
                             Margins.Top + 0,
                             Form.Width - Margins.Right - 0,
                             Form.Height - Margins.Bottom - 0));
                }

                //WinApi.FillForGlass(g, new Rectangle(Form.Right - Margins.Right, 0, Margins.Right, Form.Height));
            }
            
        }

        void _form_ResizeEnd(object sender, EventArgs e)
        {
            UpdateRibbonConditions();
            Form.Refresh();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Ribbon related with the form
        /// </summary>
        public Ribbon Ribbon
        {
            get { return _ribbon; }
            set { _ribbon = value; UpdateRibbonConditions(); }
        }

        /// <summary>
        /// Gets or sets the height of the caption bar relative to the form
        /// </summary>
        public int CaptionHeight
        {
            get { return _capionHeight; }
            set { _capionHeight = value; }
        }

        /// <summary>
        /// Gets the form this class is helping
        /// </summary>   
        public Form Form
        {
            get { return _form; }
        }

        /// <summary>
        /// Gets the margins of the non-client area
        /// </summary>
        public Padding Margins
        {
            get { return _margins; }
        }

        /// <summary>
        /// Gets or sets if the margins are already checked by WndProc
        /// </summary>
        private bool MarginsChecked
        {
            get { return _marginsChecked; }
            set { _marginsChecked = value; }
        }

        /// <summary>
        /// Gets if the <see cref="Form"/> is currently in Designer mode
        /// </summary>
        private bool DesignMode
        {
            get { return Form != null && Form.Site != null && Form.Site.DesignMode; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if ribbon should be docked or floating and updates its size
        /// </summary>
        private void UpdateRibbonConditions()
        {
            if (Ribbon == null) return;

            if (Ribbon.ActualBorderMode == RibbonWindowMode.NonClientAreaGlass)
            {
                if (Ribbon.Dock != DockStyle.None)
                {
                    Ribbon.Dock = DockStyle.None;
                }
                Ribbon.SetBounds(Margins.Left, Margins.Bottom - 1, Form.Width - Margins.Horizontal, Ribbon.Height);
            }
            else
            {
                if (Ribbon.Dock != DockStyle.Top)
                {
                    Ribbon.Dock = DockStyle.Top;
                }
            }
        }

        /// <summary>
        /// Called when helped form is activated
        /// </summary>
        /// <param name="sender">Object that raised the event</param>
        /// <param name="e">Event data</param>
        public void Form_Paint(object sender, PaintEventArgs e)
        {
            if (DesignMode) return;

            if (WinApi.IsGlassEnabled)
            {
                WinApi.FillForGlass(e.Graphics, new Rectangle(0, 0, Form.Width, Form.Height));

                using (Brush b = new SolidBrush(Form.BackColor))
                {
                    e.Graphics.FillRectangle(b,
                        Rectangle.FromLTRB(
                            Margins.Left - 0,
                            Margins.Top + 0,
                            Form.Width - Margins.Right - 0,
                            Form.Height - Margins.Bottom - 0));
                } 
            }
            else
            {
                PaintTitleBar(e);
            }

        }

        /// <summary>
        /// Draws the title bar of the form when not in glass
        /// </summary>
        /// <param name="e"></param>
        private void PaintTitleBar(PaintEventArgs e)
        {
            int radius1 = 4, radius2 = radius1 - 0;
            Rectangle rPath = new Rectangle(Point.Empty, Form.Size);
            Rectangle rInner = new Rectangle(Point.Empty, new Size(rPath.Width - 1, rPath.Height - 1));

            using (GraphicsPath path = RibbonProfessionalRenderer.RoundRectangle(rPath, radius1))
            {
                using (GraphicsPath innerPath = RibbonProfessionalRenderer.RoundRectangle(rInner, radius2))
                {
                    if (Ribbon != null && Ribbon.ActualBorderMode == RibbonWindowMode.NonClientAreaCustomDrawn)
                    {
                        RibbonProfessionalRenderer renderer = Ribbon.Renderer as RibbonProfessionalRenderer;

                        if (renderer != null)
                        {
                            using (SolidBrush p = new SolidBrush(renderer.ColorTable.Caption1))
                            {
                                e.Graphics.FillRectangle(p, new Rectangle(0,0,Form.Width,Ribbon.CaptionBarSize));
                            }
                            renderer.DrawCaptionBarBackground(new Rectangle(0, Margins.Bottom - 1, Form.Width, Ribbon.CaptionBarSize), e.Graphics);

                            using (Region rgn = new Region(path))
                            {
                                //Set Window Region
                                Form.Region = rgn;
                                SmoothingMode smbuf = e.Graphics.SmoothingMode;
                                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                using (Pen p = new Pen(renderer.ColorTable.FormBorder, 1))
                                {
                                    e.Graphics.DrawPath(p, innerPath);
                                }
                                e.Graphics.SmoothingMode = smbuf;
                            }
                        }

                        
                    }
                    
                }
            }
        }

        /// <summary>
        /// Called when helped form is activated
        /// </summary>
        /// <param name="sender">Object that raised the event</param>
        /// <param name="e">Event data</param>
        protected virtual void Form_Activated(object sender, EventArgs e)
        {
            if (DesignMode) return;
            WinApi.MARGINS dwmMargins = new WinApi.MARGINS(
                Margins.Left, 
                Margins.Right, 
                Margins.Bottom  + Ribbon.CaptionBarHeight, 
                Margins.Bottom);

            if (WinApi.IsVista && !_frameExtended)
            {
                WinApi.DwmExtendFrameIntoClientArea(Form.Handle, ref dwmMargins);
                _frameExtended = true;
            }
            
        }

        /// <summary>
        /// Processes the WndProc for a form with a Ribbbon. Returns true if message has been handled
        /// </summary>
        /// <param name="m">Message to process</param>
        /// <returns><c>true</c> if message has been handled. <c>false</c> otherwise</returns>
        public virtual bool WndProc(ref Message m)
        {
            if (DesignMode) return false;

            bool handled = false;

            if (WinApi.IsVista)
            {
                #region Checks if DWM processes the message
                IntPtr result;
                int dwmHandled = WinApi.DwmDefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam, out result);

                if (dwmHandled == 1)
                {
                    m.Result = result;
                    handled = true;
                }
                #endregion 
            }

            //if (m.Msg == WinApi.WM_NCLBUTTONUP)
            //{
            //    UpdateRibbonConditions();
            //}

            if (!handled)
            {
                if (m.Msg == WinApi.WM_NCCALCSIZE && (int)m.WParam == 1)
                {
                    #region Catch the margins of what the client area would be
                    WinApi.NCCALCSIZE_PARAMS nccsp = (WinApi.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(WinApi.NCCALCSIZE_PARAMS));

                    if (!MarginsChecked)
                    {
                        //Set what client area would be for passing to DwmExtendIntoClientArea
                        SetMargins(new Padding(
                            nccsp.rect2.Left - nccsp.rect1.Left,
                            nccsp.rect2.Top - nccsp.rect1.Top,
                            nccsp.rect1.Right - nccsp.rect2.Right,
                            nccsp.rect1.Bottom - nccsp.rect2.Bottom));

                        MarginsChecked = true;
                    }

                    Marshal.StructureToPtr(nccsp, m.LParam, false);

                    m.Result = IntPtr.Zero;
                    handled = true;
                    #endregion
                }
                else if (m.Msg == WinApi.WM_NCHITTEST && (int)m.Result == 0)
                {
                    #region Check the Non-client area hit test
                    
                    m.Result = new IntPtr(Convert.ToInt32(NonClientHitTest(new Point(WinApi.LoWord((int)m.LParam), WinApi.HiWord((int)m.LParam)))));
                    handled = true;

                    if(Ribbon != null && Ribbon.ActualBorderMode == RibbonWindowMode.NonClientAreaCustomDrawn)
                        Form.Refresh();
                    #endregion
                }
                //else if ((m.Msg == WinApi.WM_SIZE) && Ribbon != null && Ribbon.ActualBorderMode == RibbonWindowMode.NonClientAreaGlass)
                //{
                //    //UpdateRibbonConditions();
                //}
                else if (
                    (Ribbon != null && Ribbon.ActualBorderMode != RibbonWindowMode.NonClientAreaCustomDrawn) &&
                    (m.Msg == 0x112 || m.Msg == 0x47 || m.Msg == 0x46 || m.Msg == 0x2a2)) //WM_SYSCOMMAND
                {
                    //InvalidateCaption();
                    //using (Graphics g = Form.CreateGraphics())
                    //{
                    //    g.DrawRectangle(Pens.Red, new Rectangle(Form.Width - 200, 0, 200, 50));
                    //}
                }/*
                else if (
               (Ribbon != null && Ribbon.ActualBorderMode == RibbonWindowMode.NonClientAreaCustomDrawn) &&
               (m.Msg == 0x86 || m.Msg == 0x85 || m.Msg == 0xc3c2 || m.Msg == 0xc358) 
                    ) 
                {
                    m.Result = new IntPtr(-1);
                    handled = true;
                }*/

            }
            return handled;
        }

        

        /// <summary>
        /// Performs hit test for mouse on the non client area of the form
        /// </summary>
        /// <param name="form">Form to check bounds</param>
        /// <param name="dwmMargins">Margins of non client area</param>
        /// <param name="lparam">Lparam of</param>
        /// <returns></returns>
        public virtual NonClientHitTestResult NonClientHitTest(Point hitPoint)
        {

            Rectangle topleft = Form.RectangleToScreen(new Rectangle(0, 0, Margins.Left, Margins.Left));

            if (topleft.Contains(hitPoint))
                return NonClientHitTestResult.TopLeft;

            Rectangle topright = Form.RectangleToScreen(new Rectangle(Form.Width - Margins.Right, 0, Margins.Right, Margins.Right));

            if (topright.Contains(hitPoint))
                return NonClientHitTestResult.TopRight;

            Rectangle botleft = Form.RectangleToScreen(new Rectangle(0, Form.Height - Margins.Bottom, Margins.Left, Margins.Bottom));

            if (botleft.Contains(hitPoint))
                return NonClientHitTestResult.BottomLeft;

            Rectangle botright = Form.RectangleToScreen(new Rectangle(Form.Width - Margins.Right, Form.Height - Margins.Bottom, Margins.Right, Margins.Bottom));

            if (botright.Contains(hitPoint))
                return NonClientHitTestResult.BottomRight;

            Rectangle top = Form.RectangleToScreen(new Rectangle(0, 0, Form.Width, Margins.Left));

            if (top.Contains(hitPoint))
                return NonClientHitTestResult.Top;

            Rectangle cap = Form.RectangleToScreen(new Rectangle(0, Margins.Left, Form.Width, Margins.Top - Margins.Left));

            if (cap.Contains(hitPoint))
                return NonClientHitTestResult.Caption;

            Rectangle left = Form.RectangleToScreen(new Rectangle(0, 0, Margins.Left, Form.Height));

            if (left.Contains(hitPoint))
                return NonClientHitTestResult.Left;

            Rectangle right = Form.RectangleToScreen(new Rectangle(Form.Width - Margins.Right, 0, Margins.Right, Form.Height));

            if (right.Contains(hitPoint))
                return NonClientHitTestResult.Right;

            Rectangle bottom = Form.RectangleToScreen(new Rectangle(0, Form.Height - Margins.Bottom, Form.Width, Margins.Bottom));

            if (bottom.Contains(hitPoint))
                return NonClientHitTestResult.Bottom;

            return NonClientHitTestResult.Client;
        }

        /// <summary>
        /// Sets the value of the <see cref="Margins"/> property;
        /// </summary>
        /// <param name="p"></param>
        private void SetMargins(Padding p)
        {
            _margins = p;

            Padding formPadding = p;
            formPadding.Top = p.Bottom - 1;

            if (!DesignMode)
                Form.Padding = formPadding;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.RibbonHelpers;

namespace System.Windows.Forms
{
    public class RibbonForm
        : Form, IRibbonForm
    {

        #region Fields

        private RibbonFormHelper _helper;

        #endregion

        #region Ctor

        public RibbonForm()
        {
            if (WinApi.IsWindows && !WinApi.IsGlassEnabled)
            {
                SetStyle(ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.Opaque, WinApi.IsGlassEnabled);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                DoubleBuffered = true;
            }
            //SetStyle(ControlStyles.EnableNotifyMessage, true);
            

            _helper = new RibbonFormHelper(this);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Just for debugging messages
        /// </summary>
        /// <param name="m"></param>
        protected override void OnNotifyMessage(Message m)
        {
            base.OnNotifyMessage(m);
            Console.WriteLine(m.ToString());
        }

        /// <summary>
        /// Overrides the WndProc funciton
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (!Helper.WndProc(ref m))
            {
                base.WndProc(ref m);
            }
        }

        #endregion

        #region IRibbonForm Members

        /// <summary>
        /// Gets the helper for making the form a ribbon form
        /// </summary>
        public RibbonFormHelper Helper
        {
            get { return _helper; }
        }

        #endregion
    }
}

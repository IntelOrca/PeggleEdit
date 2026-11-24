using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace System.Windows.Forms
{
    /// <summary>
    /// Manages opened popups
    /// </summary>
    public static class RibbonPopupManager
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetMessagePos();

        #region Subclasses

        /// <summary>
        /// Specifies reasons why pop-ups can be dismissed
        /// </summary>
        public enum DismissReason
        {
            /// <summary>
            /// An item has been clicked
            /// </summary>
            ItemClicked,

            /// <summary>
            /// The app has been clicked
            /// </summary>
            AppClicked,

            /// <summary>
            /// A new popup is showing and will hide sibling's popups
            /// </summary>
            NewPopup,

            /// <summary>
            /// The aplication window has been deactivated
            /// </summary>
            AppFocusChanged,

            /// <summary>
            /// User has pressed escape on the keyboard
            /// </summary>
            EscapePressed
        }

        #endregion

        #region Fields

        private static List<RibbonPopup> pops;

        /// <summary>
        /// Watches the application's mouse and keyboard messages for clicks outside of the popups
        /// </summary>
        private static PopupMessageFilter messageFilter;

        /// <summary>
        /// Form that was active when the popups were opened, popups are dismissed when it is deactivated
        /// </summary>
        private static Form ownerForm;

        #endregion

        #region Ctor

        static RibbonPopupManager()
        {
            pops = new List<RibbonPopup>();
        }

        #endregion

        #region Props

        /// <summary>
        /// Gets the last pop-up of the collection
        /// </summary>
        internal static RibbonPopup LastPopup
        {
            get
            {
                if (pops.Count > 0)
                {
                    return pops[pops.Count - 1];
                }

                return null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers a popup existance
        /// </summary>
        /// <param name="p"></param>
        internal static void Register(RibbonPopup p)
        {
            if (!pops.Contains(p))
            {
                pops.Add(p); 
            }

            WatchForDismissal();
        }

        /// <summary>
        /// Makes sure popups get dismissed by clicks outside of them and by the application losing focus.
        /// </summary>
        /// <remarks>
        /// The ribbon's global mouse hook can not be relied upon for this: Windows silently removes
        /// low level hooks whose thread does not respond in time, such as while the application is busy
        /// starting up, after which popups stayed open until an item in them was clicked.
        /// </remarks>
        private static void WatchForDismissal()
        {
            if (messageFilter == null)
            {
                messageFilter = new PopupMessageFilter();
                Application.AddMessageFilter(messageFilter);
            }

            Form form = Form.ActiveForm;
            if (form != null && form != ownerForm)
            {
                if (ownerForm != null)
                {
                    ownerForm.Deactivate -= new EventHandler(ownerForm_Deactivate);
                }
                ownerForm = form;
                ownerForm.Deactivate += new EventHandler(ownerForm_Deactivate);
            }
        }

        private static void ownerForm_Deactivate(object sender, EventArgs e)
        {
            if (pops.Count > 0)
            {
                Dismiss(DismissReason.AppFocusChanged);
            }
        }

        /// <summary>
        /// Unregisters a popup from existance
        /// </summary>
        /// <param name="p"></param>
        internal static void Unregister(RibbonPopup p)
        {
            if (pops.Contains(p))
            {
                pops.Remove(p);
            }
        }

        /// <summary>
        /// Feeds a click generated on the mouse hook
        /// </summary>
        /// <param name="e"></param>
        internal static void FeedHookClick(MouseEventArgs e)
        {
            foreach (RibbonPopup p in pops)
            {
                if (p.WrappedDropDown.Bounds.Contains(e.Location))
                {
                    return;
                }
            }

            //Clicking the item that opened the popups toggles them itself
            if (IsOnPopupOwner(e.Location))
            {
                return;
            }

            //If click was in no dropdown, let's go everyone
            Dismiss(DismissReason.AppClicked);
        }

        /// <summary>
        /// Gets if the screen location is on the button or orb that opened the first popup
        /// </summary>
        private static bool IsOnPopupOwner(Point location)
        {
            if (pops.Count == 0)
            {
                return false;
            }

            RibbonDropDown dropDown = pops[0] as RibbonDropDown;
            if (dropDown != null)
            {
                RibbonButton button = dropDown.ParentItem as RibbonButton;
                if (button != null && button.Canvas != null && !button.Canvas.IsDisposed)
                {
                    Rectangle bounds = button.Style == RibbonButtonStyle.DropDown ? button.Bounds : button.DropDownBounds;
                    return button.Canvas.RectangleToScreen(bounds).Contains(location);
                }
                return false;
            }

            RibbonOrbDropDown orbDropDown = pops[0] as RibbonOrbDropDown;
            if (orbDropDown != null && orbDropDown.Ribbon != null && !orbDropDown.Ribbon.IsDisposed)
            {
                return orbDropDown.Ribbon.RectangleToScreen(orbDropDown.Ribbon.OrbBounds).Contains(location);
            }

            return false;
        }

        /// <summary>
        /// Dismisses the popups when the application is clicked outside of them, or escape is pressed.
        /// </summary>
        private class PopupMessageFilter : IMessageFilter
        {
            private const int WM_KEYDOWN = 0x0100;
            private const int WM_NCLBUTTONDOWN = 0x00A1;
            private const int WM_NCRBUTTONDOWN = 0x00A4;
            private const int WM_NCMBUTTONDOWN = 0x00A7;
            private const int WM_NCXBUTTONDOWN = 0x00AB;
            private const int WM_LBUTTONDOWN = 0x0201;
            private const int WM_RBUTTONDOWN = 0x0204;
            private const int WM_MBUTTONDOWN = 0x0207;
            private const int WM_XBUTTONDOWN = 0x020B;

            public bool PreFilterMessage(ref Message m)
            {
                if (pops.Count == 0)
                {
                    return false;
                }

                switch (m.Msg)
                {
                    case WM_LBUTTONDOWN:
                    case WM_RBUTTONDOWN:
                    case WM_MBUTTONDOWN:
                    case WM_XBUTTONDOWN:
                    case WM_NCLBUTTONDOWN:
                    case WM_NCRBUTTONDOWN:
                    case WM_NCMBUTTONDOWN:
                    case WM_NCXBUTTONDOWN:
                        // Where the click was, rather than where the mouse is now
                        uint position = GetMessagePos();
                        Point location = new Point((short)(position & 0xFFFF), (short)(position >> 16));
                        FeedHookClick(new MouseEventArgs(MouseButtons.None, 0, location.X, location.Y, 0));
                        break;
                    case WM_KEYDOWN:
                        if ((Keys)(m.WParam.ToInt64() & 0xFFFF) == Keys.Escape)
                        {
                            Dismiss(DismissReason.EscapePressed);
                            return true;
                        }
                        break;
                }

                return false;
            }
        }

        /// <summary>
        /// Feeds mouse Wheel. If applied on a IScrollableItem it's sended to it
        /// </summary>
        /// <param name="e"></param>
        /// <returns>True if handled. False otherwise</returns>
        internal static bool FeedMouseWheel(MouseEventArgs e)
        {
            RibbonDropDown dd = LastPopup as RibbonDropDown;
            
            if (dd != null)
            {
                foreach (RibbonItem item in dd.Items)
                {
                    if (dd.RectangleToScreen(item.Bounds).Contains(e.Location))
                    {
                        IScrollableRibbonItem sc = item as IScrollableRibbonItem;

                        if (sc != null)
                        {
                            if (e.Delta < 0)
                            {
                                sc.ScrollDown();
                            }
                            else
                            {
                                sc.ScrollUp();
                            }

                            return true;
                        }
                    }
                    
                }
            }

            return false;
        }

        /// <summary>
        /// Closes all children of specified pop-up
        /// </summary>
        /// <param name="parent">Pop-up of which children will be closed</param>
        /// <param name="reason">Reason for dismissing</param>
        public static void DismissChildren(RibbonPopup parent, DismissReason reason)
        {
            int index = pops.IndexOf(parent);

            if (index >= 0)
            {
                Dismiss(index + 1, reason);
            }
        }

        /// <summary>
        /// Closes all currently registered pop-ups
        /// </summary>
        /// <param name="reason"></param>
        public static void Dismiss(DismissReason reason)
        {
            Dismiss(0, reason);
        }

        /// <summary>
        /// Closes specified pop-up and all its descendants
        /// </summary>
        /// <param name="startPopup">Pop-up to close (and its descendants)</param>
        /// <param name="reason">Reason for closing</param>
        public static void Dismiss(RibbonPopup startPopup, DismissReason reason)
        {
            int index = pops.IndexOf(startPopup);

            if (index >= 0)
            {
                Dismiss(index, reason);
            }
        }

        /// <summary>
        /// Closes pop-up of the specified index and all its descendants
        /// </summary>
        /// <param name="startPopup">Index of the pop-up to close</param>
        /// <param name="reason">Reason for closing</param>
        private static void Dismiss(int startPopup, DismissReason reason)
        {
            for (int i = pops.Count - 1; i >= startPopup; i--)
            {
                pops[i].Close();
            }
        }

        #endregion
    }
}

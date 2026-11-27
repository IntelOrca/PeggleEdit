// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2011. http://tedtycoon.co.uk
//
// PeggleEdit is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeggleEdit is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeggleEdit. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    [GuidAttribute("1A01E050-F11A-47C5-B62B-000000000001")]
    class LevelToolWindow : Form
    {
        MainMDIForm mParent;
        LevelEditor mLevelEditor;

        public LevelToolWindow(MainMDIForm parent, Level level)
        {
            mParent = parent;

            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Text = level.Info.Name;
            this.KeyPreview = true;

            this.Icon = Icon.FromHandle(Resources.level_16.GetHicon());

            mLevelEditor = new LevelEditor();
            mLevelEditor.Level = level;

            mLevelEditor.Location = new Point(0, 0);
            Rectangle bounds = mLevelEditor.Level.Bounds;
            mLevelEditor.Size = mLevelEditor.ZoomSize(new Size(bounds.Width, bounds.Height));

            mLevelEditor.Level.ShowAnchorsAlways = Settings.Default.ShowAnchorsAlways;

            mLevelEditor.UpdatedRedrawed += new EventHandler(mLevelEditor_UpdatedRedrawed);
            mLevelEditor.SelectionChanged += new EventHandler(mLevelEditor_SelectionChanged);
            mLevelEditor.MouseMove += new MouseEventHandler(mLevelEditor_MouseMove);
            mLevelEditor.FrameStarting += mLevelEditor_FrameStarting;
            mLevelEditor.MouseWheel += mLevelEditor_MouseWheel;
            mLevelEditor.ZoomChanged += (sender, e) => mParent.LevelZoomChanged(mLevelEditor);
            mLevelEditor.ShownObjectsChanged += (sender, e) => mParent.LevelZoomChanged(mLevelEditor);
            mParent.Activated += mParent_Activated;

            this.Controls.Add(mLevelEditor);

            // Around the level when it is smaller than the window, as its margin is
            this.BackColor = SystemColors.AppWorkspace;
            this.AutoScroll = true;
        }

        #region Zoom

        // Mouse wheel movement towards the next zoom step, as a precise wheel or touchpad moves in smaller steps
        int mWheelZoom;

        /// <summary>
        /// Zooms the level, keeping the point of the level at <paramref name="anchor"/> (in this window's client
        /// coordinates) where it is, or the point in the middle of the window when it is null.
        /// </summary>
        public void SetZoomLevel(float zoomLevel, Point? anchor = null)
        {
            var oldZoom = mLevelEditor.Zoom;
            var at = anchor ?? new Point(ClientSize.Width / 2, ClientSize.Height / 2);
            // The point of the level (in unzoomed units) under the anchor
            var levelPoint = new PointF((at.X - mLevelEditor.Left) / oldZoom, (at.Y - mLevelEditor.Top) / oldZoom);

            // Drawn once it has been resized and scrolled, as otherwise the level as it was is moved on screen first
            using (PlatformServices.Current.SuspendDrawing(this))
            {
                mLevelEditor.ZoomLevel = zoomLevel;
                if (mLevelEditor.Zoom != oldZoom)
                    ScrollTo(levelPoint, at);
            }
        }

        /// <summary>
        /// Scrolls so that a point of the level (in unzoomed units) is at a point of the window, as near as it can be,
        /// with the level in the middle of the window in each direction it is smaller than the window.
        /// </summary>
        private void ScrollTo(PointF levelPoint, Point at)
        {
            var zoom = mLevelEditor.Zoom;
            AutoScrollPosition = Point.Empty;
            mLevelEditor.Location = Point.Empty;
            PerformLayout();
            AutoScrollPosition = new Point(
                Math.Max(0, (int)Math.Round((levelPoint.X * zoom) - at.X)),
                Math.Max(0, (int)Math.Round((levelPoint.Y * zoom) - at.Y)));
            LayoutEditor();
        }

        void mLevelEditor_MouseWheel(object sender, MouseEventArgs e)
        {
            HandleMouseWheel(PointToClient(mLevelEditor.PointToScreen(e.Location)), e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Over the space around a level smaller than the window
            if (!HandleMouseWheel(e.Location, e))
                base.OnMouseWheel(e);
        }

        /// <summary>
        /// Zooms with Ctrl + wheel, around the mouse at <paramref name="location"/> (in this window's client
        /// coordinates), and scrolls sideways with Shift + wheel. Returns whether the wheel was used.
        /// </summary>
        private bool HandleMouseWheel(Point location, MouseEventArgs e)
        {
            var modifiers = Control.ModifierKeys;
            if (modifiers == Keys.Control)
            {
                // Ctrl + wheel zooms in and out around the mouse
                if (e is HandledMouseEventArgs handled)
                    handled.Handled = true;
                mWheelZoom += e.Delta;
                var steps = mWheelZoom / SystemInformation.MouseWheelScrollDelta;
                if (steps == 0)
                    return true;
                mWheelZoom -= steps * SystemInformation.MouseWheelScrollDelta;

                var zoomLevel = mLevelEditor.ZoomLevel;
                for (int i = 0; i < Math.Abs(steps); i++)
                    zoomLevel = mLevelEditor.GetNextZoomLevel(zoomLevel, Math.Sign(steps));
                SetZoomLevel(zoomLevel, location);
                return true;
            }
            else if (modifiers == Keys.Shift)
            {
                // Shift + wheel scrolls sideways
                if (e is HandledMouseEventArgs handled)
                    handled.Handled = true;
                var position = AutoScrollPosition;
                AutoScrollPosition = new Point(-position.X - e.Delta, -position.Y);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Keeps the scroll position when the level is focused (e.g. clicked), rather than scrolling to show its top
        /// left.
        /// </summary>
        protected override Point ScrollToControl(Control activeControl) => DisplayRectangle.Location;

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            // The scroll range is worked out from where the level is, so again once it has moved (and the scroll bars
            // shown or hidden then can change where it goes)
            for (int i = 0; i < 3 && LayoutEditor(); i++)
                base.OnLayout(levent);
        }

        /// <summary>
        /// Puts the level in the middle of the window in each direction it is smaller than the window (with no scroll
        /// bar that way), and at the start of the scrolled area in each it is larger, so that the scroll range is the
        /// level's size. Returns whether it moved.
        /// </summary>
        private bool LayoutEditor()
        {
            if (mLevelEditor == null)
                return false;
            var client = ClientSize;
            var size = mLevelEditor.Size;
            var location = mLevelEditor.Location;
            var scroll = AutoScrollPosition;
            var x = size.Width < client.Width ? (client.Width - size.Width) / 2 : scroll.X;
            var y = size.Height < client.Height ? (client.Height - size.Height) / 2 : scroll.Y;
            if (x == location.X && y == location.Y)
                return false;
            mLevelEditor.Location = new Point(x, y);
            return true;
        }

        #endregion

        /// <summary>
        /// Shows the level's name, with an * after it if the level has changed since it was saved.
        /// </summary>
        public void UpdateTitle(bool modified)
        {
            var title = Level.Info.Name + (modified ? "*" : "");
            if (Text != title)
                Text = title;
        }

        protected override void OnShown(EventArgs e)
        {
            CentreScrollPosition();

            // Some Windows Forms implementations (Mono) raise Shown before the dock container has sized
            // the window, so centre again once it has been laid out if the size has changed.
            var shownSize = Size;
            BeginInvoke(new Action(() =>
            {
                if (!IsDisposed && !Disposing && Size != shownSize)
                    CentreScrollPosition();
            }));

            base.OnShown(e);
        }

        private void CentreScrollPosition()
        {
            // The middle of the level in the middle of the window
            var bounds = mLevelEditor.Level.Bounds;
            ScrollTo(new PointF(bounds.Width / 2f, bounds.Height / 2f), new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        }

        /// <summary>
        /// Moves the preview on for each frame. Only the level being worked on plays its preview: it pauses while
        /// another level, or another application, is active, and plays on from where it was when it is active again.
        /// </summary>
        void mLevelEditor_FrameStarting(object sender, EventArgs e)
        {
            var level = mLevelEditor.Level;
            bool playing = level.ShowPreview && IsActiveLevel;
            level.PreviewPaused = !playing;
            if (playing)
                level.UpdatePreview();
            mLevelEditor.Animating = playing;
        }

        private bool IsActiveLevel => mParent.GetFocusedLevelEditor() == mLevelEditor && Form.ActiveForm != null;

        void mParent_Activated(object sender, EventArgs e)
        {
            // Resumes the preview
            mLevelEditor.RequestRedraw(null);
        }

        void mLevelEditor_MouseMove(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Default;

            mParent.SetStatusLocation(mLevelEditor.Level.GetVirtualXY(e.Location));
        }

        void mLevelEditor_UpdatedRedrawed(object sender, EventArgs e)
        {
            if (mParent.GetFocusedLevelEditor() == mLevelEditor)
            {
                //Update entry list
                mParent.UpdateEntryList(mLevelEditor);
            }
        }

        private void mLevelEditor_SelectionChanged(object sender, EventArgs e)
        {
            UpdateToolWindows();

            if (mLevelEditor.SelectedEntries.Count == 0)
                mParent.SetStatus(String.Empty);
            else if (mLevelEditor.SelectedEntries.Count == 1)
            {
                LevelEntry selentry = mLevelEditor.SelectedEntries[0];
                mParent.SetStatus("Entry: {0} ({1})", selentry.GetEntryIndex(), selentry.ToString());
            }
            else
                mParent.SetStatus("{0} / {1} pegs selected", mLevelEditor.SelectedEntries.Count, mLevelEditor.Level.Entries.Count);
        }

        private void UpdateToolWindows()
        {
            //Update properties
            mParent.UpdateProperties(mLevelEditor.SelectedEntries.ToArray());
        }

        protected override void OnGotFocus(EventArgs e)
        {
            mParent.LevelWindowHasFocus(this);
            // Resumes the preview
            mLevelEditor.RequestRedraw(null);

            base.OnGotFocus(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                mParent.Activated -= mParent_Activated;
            base.Dispose(disposing);
        }

        public LevelEditor LevelEditor
        {
            get
            {
                return mLevelEditor;
            }
        }

        public Level Level
        {
            get
            {
                return mLevelEditor.Level;
            }
        }
    }
}

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

using System.Drawing;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    internal class DrawEditorTool : EditorTool
    {
        LevelEntry mEntry;
        bool mDraw;
        bool mAvoidOverlapping;
        int mWidth;
        int mHeight;

        public DrawEditorTool(LevelEntry le, bool draw)
        {
            mEntry = le;
            mDraw = draw;
        }

        public DrawEditorTool(LevelEntry le, bool draw, int width, int height)
        {
            mEntry = le;
            mDraw = draw;
            mAvoidOverlapping = true;
            mWidth = width;
            mHeight = height;
        }

        public override void Activate()
        {
            base.Activate();

            Editor.ClearSelection();
            Editor.UpdateRedraw();
        }

        public override void MouseDown(MouseButtons button, Point location, Keys modifierKeys)
        {
            MouseMove(button, location, modifierKeys);
        }

        public override void MouseMove(MouseButtons button, Point location, Keys modifierKeys)
        {
            location = Editor.Level.GetVirtualXY(location);

            //Snap
            PointF le_location = location;
            if (Settings.Default.ShowGrid & Settings.Default.SnapToGrid)
            {
                le_location = new PointF(Editor.SnapToGrid((float)location.X), Editor.SnapToGrid((float)location.Y));
            }

            RectangleF lookRange = new RectangleF(le_location.X - (mWidth / 2), le_location.Y - (mHeight / 2), mWidth, mHeight);

            Editor.SetProvisonalEntry(null);

            if ((!Editor.Level.IsObjectIn(lookRange)) || (!mAvoidOverlapping))
            {

                var entry = (LevelEntry)mEntry.Clone();
                entry.Level = Editor.Level;
                entry.X = le_location.X;
                entry.Y = le_location.Y;

                if (button == MouseButtons.Left)
                {
                    Editor.CreateUndoPoint();
                    Editor.Level.Entries.Add(entry);
                }
                else
                {
                    Editor.SetProvisonalEntry(entry);
                }

                Editor.UpdateRedraw();

                // Have we finished
                if (!mDraw && button == MouseButtons.Left && (modifierKeys & Keys.Control) == 0)
                {
                    Finish();
                }
            }
        }

        public override object Clone()
        {
            DrawEditorTool tool = new DrawEditorTool(mEntry, mDraw);
            CloneTo(tool);
            tool.mAvoidOverlapping = mAvoidOverlapping;
            tool.mWidth = mWidth;
            tool.mHeight = mHeight;

            return tool;
        }
    }
}

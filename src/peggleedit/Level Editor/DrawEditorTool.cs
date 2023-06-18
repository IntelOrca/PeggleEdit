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
        private LevelEntry _entry;
        private bool _draw;
        private bool _avoidOverlapping;
        private int _width;
        private int _height;
        private bool _drawing;

        public LevelEntry Template => _entry;

        public DrawEditorTool(LevelEntry le, bool draw)
        {
            _entry = le;
            _draw = draw;
        }

        public DrawEditorTool(LevelEntry le, bool draw, int width, int height)
        {
            _entry = le;
            _draw = draw;
            _avoidOverlapping = true;
            _width = width;
            _height = height;
        }

        public override void Activate()
        {
            Editor.ClearSelection();
            Editor.UpdateRedraw();
        }

        public override void Deactivate()
        {
            Editor.SetProvisonalEntry(null);
        }

        public override void MouseDown(MouseButtons button, Point location, Keys modifierKeys)
        {
            MouseMove(button, location, modifierKeys);
        }

        public override void MouseUp(MouseButtons button, Point location, Keys modifierKeys)
        {
            _drawing = false;
        }

        public override void MouseMove(MouseButtons button, Point location, Keys modifierKeys)
        {
            Editor.SetProvisonalEntry(null);

            // Get location to place
            location = Editor.Level.GetVirtualXY(location);
            var placeLocation = (PointF)location;
            if (Settings.Default.ShowGrid & Settings.Default.SnapToGrid)
            {
                placeLocation = new PointF(Editor.SnapToGrid(location.X), Editor.SnapToGrid(location.Y));
            }

            // Check if area is clear
            var lookRange = new RectangleF(placeLocation.X - (_width / 2), placeLocation.Y - (_height / 2), _width, _height);
            if (((modifierKeys & Keys.Control) != 0 || !Editor.Level.IsObjectIn(lookRange)) || (!_avoidOverlapping))
            {
                var entry = (LevelEntry)_entry.Clone();
                entry.Level = Editor.Level;
                entry.X = placeLocation.X;
                entry.Y = placeLocation.Y;

                if (button == MouseButtons.Left)
                {
                    if (!_drawing)
                    {
                        _drawing = true;
                        Editor.CreateUndoPoint();
                    }
                    Editor.Level.Entries.Add(entry);
                }
                else
                {
                    Editor.SetProvisonalEntry(entry);
                }

                Editor.UpdateRedraw();

                // Have we finished
                if (!_draw && button == MouseButtons.Left && (modifierKeys & Keys.Control) == 0)
                {
                    Finish();
                }
            }
        }

        public override object Clone()
        {
            DrawEditorTool tool = new DrawEditorTool(_entry, _draw);
            CloneTo(tool);
            tool._avoidOverlapping = _avoidOverlapping;
            tool._width = _width;
            tool._height = _height;

            return tool;
        }
    }
}

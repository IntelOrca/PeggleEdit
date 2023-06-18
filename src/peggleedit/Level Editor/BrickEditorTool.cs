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
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    internal class BrickEditorTool : EditorTool
    {
        private bool _curved;
        private float _straightLength = 30;
        private float _curvedSectorAngle = 30;
        private float _curvedInnerRadius = 35;
        private float _rotation = 90;
        private int _width = 38;
        private int _height = 38;
        private bool _drawing;

        private PointF? _connectPosition;

        public bool Curved => _curved;

        public BrickEditorTool(bool curved)
        {
            _curved = curved;
        }

        public BrickEditorTool(Brick template)
        {
            _rotation = template.Rotation;
            _curved = template.Curved;
            if (_curved)
            {
                _curvedInnerRadius = template.InnerRadius;
                _curvedSectorAngle = template.SectorAngle;
            }
            else
            {
                _straightLength = template.Length;
            }
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

            // Check if we are connecting to another brick
            var newBrick = CreateBrick();
            var connectedToBrick = false;
            if ((modifierKeys & Keys.Shift) == 0)
            {
                foreach (var entry in Editor.Level.Entries)
                {
                    if (entry is Brick brick)
                    {
                        var distance = placeLocation.Subtract(brick.RightSidePosition).GetLength();
                        if (distance < 24)
                        {
                            newBrick.TextureFlip = brick.TextureFlip;
                            if (!_curved)
                            {
                                newBrick.LeftSideAngle = brick.RightSideAngle + 180;
                                newBrick.LeftSidePosition = brick.RightSidePosition;
                            }
                            else
                            {
                                newBrick.LeftSideAngle = brick.RightSideAngle;
                                newBrick.LeftSidePosition = brick.RightSidePosition;
                                var distA = newBrick.RightSidePosition.Subtract(placeLocation).GetLength();
                                newBrick.RightSideAngle = brick.RightSideAngle + 180;
                                newBrick.RightSidePosition = brick.RightSidePosition;
                                var distB = newBrick.LeftSidePosition.Subtract(placeLocation).GetLength();
                                if (distA < distB)
                                {
                                    newBrick.LeftSideAngle = brick.RightSideAngle;
                                    newBrick.LeftSidePosition = brick.RightSidePosition;
                                }
                                else
                                {
                                    newBrick.TextureFlip = !brick.TextureFlip;
                                }
                            }

                            _connectPosition = brick.RightSidePosition;
                            placeLocation = newBrick.Location;
                            connectedToBrick = true;
                            break;
                        }
                        distance = placeLocation.Subtract(brick.LeftSidePosition).GetLength();
                        if (distance < 24)
                        {
                            newBrick.TextureFlip = brick.TextureFlip;
                            if (!_curved)
                            {
                                newBrick.RightSideAngle = brick.LeftSideAngle + 180;
                                newBrick.RightSidePosition = brick.LeftSidePosition;
                            }
                            else
                            {
                                newBrick.LeftSideAngle = brick.LeftSideAngle;
                                newBrick.LeftSidePosition = brick.LeftSidePosition;
                                var distA = newBrick.LeftSidePosition.Subtract(placeLocation).GetLength();
                                newBrick.RightSideAngle = brick.LeftSideAngle + 180;
                                newBrick.RightSidePosition = brick.LeftSidePosition;
                                var distB = newBrick.LeftSidePosition.Subtract(placeLocation).GetLength();
                                if (distA < distB)
                                {
                                    newBrick.LeftSideAngle = brick.LeftSideAngle;
                                    newBrick.LeftSidePosition = brick.LeftSidePosition;
                                    newBrick.TextureFlip = !brick.TextureFlip;
                                }
                                else
                                {
                                }
                            }

                            _connectPosition = brick.LeftSidePosition;
                            placeLocation = newBrick.Location;
                            connectedToBrick = true;
                            break;
                        }
                    }
                }
            }

            _width = _height = 8;
            var lookRange = new RectangleF(placeLocation.X - (_width / 2), placeLocation.Y - (_height / 2), _width, _height);
            if ((modifierKeys & Keys.Control) == 0 && Editor.Level.IsObjectIn(lookRange))
            {
                // Don't allow placement
                _connectPosition = null;
                return;
            }
            else if (!connectedToBrick)
            {
                _connectPosition = null;
                newBrick.X = placeLocation.X;
                newBrick.Y = placeLocation.Y;
            }

            if (button == MouseButtons.Left)
            {
                if (!_drawing)
                {
                    _drawing = true;
                    Editor.CreateUndoPoint();
                }
                Editor.Level.Entries.Add(newBrick);
            }
            else
            {
                Editor.SetProvisonalEntry(newBrick);
            }

            Editor.UpdateRedraw();
        }

        private Brick CreateBrick()
        {
            var brick = new Brick(Editor.Level);
            brick.Curved = _curved;
            if (_curved)
            {
                brick.SectorAngle = _curvedSectorAngle;
                brick.InnerRadius = _curvedInnerRadius;
            }
            else
            {
                brick.Length = _straightLength;
            }
            brick.Rotation = _rotation;
            return brick;
        }

        public override void Draw(Graphics g)
        {
            var origin = Editor.Level.GetActualXY(0, 0);
            g.TranslateTransform(origin.X, origin.Y);

            // Draw cross
            if (_connectPosition is PointF pos)
            {
                var pen0 = new Pen(Color.White, 2);
                var pen1 = new Pen(Color.Magenta, 1);

                var rect = new RectangleF(pos.X, pos.Y, 0, 0);
                rect.Inflate(4, 4);
                g.DrawLine(pen0, rect.Left, rect.Top, rect.Right, rect.Bottom);
                g.DrawLine(pen0, rect.Right, rect.Top, rect.Left, rect.Bottom);
                g.DrawLine(pen1, rect.Left, rect.Top, rect.Right, rect.Bottom);
                g.DrawLine(pen1, rect.Right, rect.Top, rect.Left, rect.Bottom);
            }
        }

        public override object Clone()
        {
            var tool = new BrickEditorTool(_curved);
            CloneTo(tool);
            tool._curved = _curved;
            tool._straightLength = _straightLength;
            tool._curvedSectorAngle = _curvedSectorAngle;
            tool._curvedInnerRadius = _curvedInnerRadius;
            tool._rotation = _rotation;
            return tool;
        }
    }
}

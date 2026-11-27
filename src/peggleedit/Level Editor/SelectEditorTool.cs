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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    internal class SelectEditorTool : EditorTool
    {
        private State _state;
        private Point _selectionStart;
        private Rectangle _selectionRect;
        private List<PointF> _objectPoints = new List<PointF>();
        private LevelEntry _dragObject;
        private bool _firstObjectMovement;
        private List<LevelEntry> _previousSelection = new List<LevelEntry>();
        private int _pointMoveIndex;
        private bool _commandKeyDown;
        private PointF _placePosition;

        /// <summary>
        /// How near a point of an object the mouse is to pick it, in level units: its handle (8 units each way), or 8
        /// pixels on screen when zoomed out so far that the handle is smaller.
        /// </summary>
        private float PointTolerance => Math.Max(8, Editor.PixelsToUnits(8));

        public override void MouseDown(MouseButtons button, Point location, Keys modifierKeys)
        {
            var level = Editor.Level;
            var vl = Editor.Level.GetVirtualXY(location);

            Editor.SetProvisonalEntry(null);

            foreach (LevelEntry e in Editor.SelectedEntries)
            {
                // Not the points of objects that are hidden or locked (e.g. selected in the Entry List)
                if (e is IPointContainer pc && level.IsPickable(e))
                {
                    for (var i = 0; i < pc.InteractionPointCount; i++)
                    {
                        var p = pc.GetInteractionPoint(i);

                        // On the point's handle, or near enough on screen when zoomed out
                        var rect = new RectangleF();
                        rect.Offset(p);
                        rect.Inflate(PointTolerance, PointTolerance);
                        if (rect.Contains(vl))
                        {
                            _selectionStart = location;
                            _dragObject = e;
                            _pointMoveIndex = i;
                            _firstObjectMovement = true;
                            _objectPoints.Clear();
                            _objectPoints.Add(p);
                            _state = State.MovingPoints;
                            return;
                        }
                    }
                }
            }

            // Get the pegs at this point, of those that are shown
            LevelEntry[] les = level.GetPickableObjectsIn(new RectangleF(vl.X, vl.Y, 1, 1));

            // Get top most object
            LevelEntry top_le = null;
            if (les.Length > 0)
            {
                top_le = les[les.Length - 1];
            }

            // Select the one thats already selected if possible
            LevelEntry le = null;
            foreach (LevelEntry entry in les)
            {
                if (Editor.SelectedEntries.Contains(entry))
                {
                    le = entry;
                    break;
                }
            }

            if (top_le != le)
                le = null;

            // Select whatever one is there
            if (le == null && les.Length > 0)
                le = top_le;

            // Did we click on an object
            _clickedEntry = le;
            if (le != null)
            {
                // The object's group with it, unless Alt is down to pick just the object
                var picked = (modifierKeys & Keys.Alt) != 0 ? new[] { le } : level.GetGroup(le);
                if (Editor.SelectedEntries.Contains(le))
                {
                    if ((modifierKeys & Keys.Control) != 0)
                    {
                        foreach (var entry in picked)
                            Editor.SelectedEntries.Remove(entry);
                    }
                    else if ((modifierKeys & Keys.Alt) != 0)
                    {
                        // Just the object, out of its selected group
                        Editor.SelectedEntries.Clear();
                        Editor.SelectedEntries.Add(le);
                    }
                }
                else
                {
                    // Unless control is down, clear selection
                    if ((modifierKeys & Keys.Control) == 0)
                        Editor.SelectedEntries.Clear();

                    // Add the new peg to the selection
                    foreach (var entry in picked)
                    {
                        if (!Editor.SelectedEntries.Contains(entry))
                            Editor.SelectedEntries.Add(entry);
                    }
                }

                if (button == MouseButtons.Left && (modifierKeys & Keys.Control) == 0)
                {
                    //Set the selection start to here
                    _selectionStart = location;

                    //Store all the original poisitons of the objects
                    _objectPoints.Clear();
                    for (int i = 0; i < Editor.SelectedEntries.Count; i++)
                    {
                        LevelEntry objs = Editor.SelectedEntries[i];
                        _objectPoints.Add(new PointF(objs.X, objs.Y));
                    }

                    //We are moving the pegs
                    _dragObject = le;
                    _state = State.MovingObjects;
                    _firstObjectMovement = true;
                }
            }
            else
            {
                // Unless control is down, clear selection
                if ((modifierKeys & Keys.Control) == 0)
                {
                    _previousSelection.Clear();
                    Editor.SelectedEntries.Clear();
                }
                else
                {
                    _previousSelection.Clear();
                    _previousSelection.AddRange(Editor.SelectedEntries.ToArray());
                }

                // Start selection rectangle
                _selectionStart = location;
                _state = State.Selecting;
            }

            Editor.UpdateRedraw();

            Editor.CheckSelectionChanged();
        }


        public override void MouseMove(MouseButtons button, Point location, Keys modifierKeys)
        {
            _placePosition = Editor.Level.GetVirtualXY(location);
            if (button != MouseButtons.Left)
            {
                // Redraw only to take away a brick that was being placed, not on every move of the mouse
                if (Editor.ProvisionalEntry != null)
                    Editor.SetProvisonalEntry(null);
                if (Editor.SelectedEntries.Count != 0)
                {
                    PlaceBrick(true, false, false, true);
                }
                return;
            }

            switch (_state)
            {
                case State.Selecting:
                {
                    if (location.X < _selectionStart.X)
                    {
                        _selectionRect.X = location.X;
                        _selectionRect.Width = _selectionStart.X - location.X;
                    }
                    else
                    {
                        _selectionRect.X = _selectionStart.X;
                        _selectionRect.Width = location.X - _selectionStart.X;
                    }

                    if (location.Y < _selectionStart.Y)
                    {
                        _selectionRect.Y = location.Y;
                        _selectionRect.Height = _selectionStart.Y - location.Y;
                    }
                    else
                    {
                        _selectionRect.Y = _selectionStart.Y;
                        _selectionRect.Height = location.Y - _selectionStart.Y;
                    }

                    Editor.SelectedEntries.Clear();
                    Editor.SelectedEntries.AddRange(_previousSelection);

                    // With the rest of their groups, unless Alt is down
                    var vl = Editor.Level.GetVirtualXY(_selectionRect.Location);
                    var boxed = Editor.Level.GetPickableObjectsIn(new RectangleF(
                         vl.X, vl.Y,
                         _selectionRect.Width, _selectionRect.Height));
                    var selected = new HashSet<LevelEntry>(Editor.SelectedEntries.Cast<LevelEntry>());
                    var groups = (modifierKeys & Keys.Alt) == 0 && boxed.Any(x => x.Group != 0) ? Editor.Level.GetGroups() : null;
                    foreach (var boxedEntry in boxed)
                    {
                        var picked = groups != null && boxedEntry.Group != 0 && groups.TryGetValue(boxedEntry.Group, out var members) ?
                            members : new List<LevelEntry> { boxedEntry };
                        foreach (var entry in picked)
                        {
                            if (selected.Add(entry))
                                Editor.SelectedEntries.Add(entry);
                        }
                    }

                    Editor.UpdateRedraw();
                    Editor.CheckSelectionChanged();
                    break;
                }
                case State.MovingObjects:
                {
                    // Calculate the delta by how much the mouse has moved
                    var delta = location.Subtract(_selectionStart);
                    if (delta.IsEmpty)
                        break;

                    // Create undo point if first call
                    if (_firstObjectMovement)
                    {
                        Editor.CreateUndoPoint();
                        _firstObjectMovement = false;
                    }

                    MoveSelectingObjectsBy(delta.X, delta.Y, true);

                    Editor.UpdateRedraw();
                    break;
                }
                case State.MovingPoints:
                {
                    if (_dragObject is IPointContainer pc)
                    {
                        if (_pointMoveIndex < 0 || _pointMoveIndex >= pc.InteractionPointCount)
                            break;

                        var delta = location.Subtract(_selectionStart);
                        if (delta.IsEmpty)
                            break;

                        var p = _objectPoints[0].Add(delta);
                        for (var i = 0; i < pc.InteractionPointCount; i++)
                        {
                            if (i != _pointMoveIndex)
                            {
                                var other = pc.GetInteractionPoint(i);
                                if (p.GetLength(other) < PointTolerance)
                                {
                                    p = other;
                                    break;
                                }
                            }
                        }

                        // Create undo point if first call
                        if (_firstObjectMovement)
                        {
                            Editor.CreateUndoPoint();
                            _firstObjectMovement = false;
                        }

                        pc.SetInteractionPoint(_pointMoveIndex, p);
                        Editor.UpdateRedraw();
                    }
                    break;
                }
            }
        }

        private void MoveSelectingObjectsBy(int deltaX, int deltaY, bool snap)
        {
            for (int i = 0; i < Editor.SelectedEntries.Count; i++)
            {
                LevelEntry obj = Editor.SelectedEntries[i];
                // Locked objects (e.g. selected in the Entry List) are not moved in the level
                if (obj.Locked)
                    continue;

                //Calculate the new x and y with the delta
                float newX = _objectPoints[i].X + deltaX;
                float newY = _objectPoints[i].Y + deltaY;

                //Snap if it is the dragged object
                if (snap && (Settings.Default.SnapToGrid & Settings.Default.ShowGrid) && obj == _dragObject)
                {
                    float snapX = Editor.SnapToGrid(newX);
                    float snapY = Editor.SnapToGrid(newY);

                    //Check if there was a snap
                    if (snapX != newX || snapY != newY)
                    {
                        //Calculate delta
                        deltaX = (int)(snapX - _objectPoints[i].X);
                        deltaY = (int)(snapY - _objectPoints[i].Y);

                        //Move the selected objects by delta
                        MoveSelectingObjectsBy(deltaX, deltaY, false);

                        //Finish
                        return;
                    }
                }

                obj.X = newX;
                obj.Y = newY;
            }
        }

        // The object last clicked in the level, which a brick is added on to when its whole group is selected
        private LevelEntry _clickedEntry;

        /// <summary>
        /// Gets the object a brick is added on to: the one selected, or the one clicked when its group is selected.
        /// </summary>
        private LevelEntry GetPlacementAnchor()
        {
            var selectedEntries = Editor.SelectedEntries;
            if (selectedEntries.Count == 1)
                return selectedEntries[0];
            var clicked = _clickedEntry;
            if (clicked != null && clicked.Group != 0 && selectedEntries.Contains(clicked) &&
                selectedEntries.Cast<LevelEntry>().All(x => x.Group == clicked.Group))
                return clicked;
            return null;
        }

        private void PlaceBrick(bool curved, bool swapDirection, bool force, bool provisional)
        {
            var level = Editor.Level;
            var selectedEntries = Editor.SelectedEntries;
            var anchor = GetPlacementAnchor();
            if (anchor != null)
            {
                if (anchor is Brick prevBrick)
                {
                    var provisionalBricks = new List<Brick>();

                    if (curved)
                    {
                        if (prevBrick.Curved)
                        {
                            for (var i = 0; i < 2; i++)
                            {
                                var brick = new Brick(level);
                                brick.Curved = true;
                                brick.Thickness = prevBrick.Thickness;
                                brick.Radius = prevBrick.Radius;
                                brick.Angle = prevBrick.Angle;

                                if (swapDirection)
                                {
                                    brick.TextureFlip = !prevBrick.TextureFlip;
                                    brick.Rotation = prevBrick.Rotation + 180;
                                    if (i == 0)
                                    {
                                        brick.LeftSidePosition = prevBrick.LeftSidePosition;
                                    }
                                    else
                                    {
                                        brick.RightSidePosition = prevBrick.RightSidePosition;
                                    }
                                }
                                else
                                {
                                    brick.TextureFlip = prevBrick.TextureFlip;
                                    if (i == 0)
                                    {
                                        brick.Rotation = prevBrick.Rotation - prevBrick.Angle;
                                    }
                                    else
                                    {
                                        brick.Rotation = prevBrick.Rotation + prevBrick.Angle;
                                    }

                                    var newRotation = MathExt.ToRadians(-brick.Rotation);
                                    var radius = prevBrick.MidRadius;
                                    var origin = prevBrick.Origin;
                                    brick.Location = origin.Add(new PointF(
                                        (float)(Math.Cos(newRotation) * radius),
                                        (float)(Math.Sin(newRotation) * radius)));
                                }
                                provisionalBricks.Add(brick);
                            }
                        }
                        else
                        {
                            for (var i = 0; i < 4; i++)
                            {
                                var brick = new Brick(level);
                                brick.Curved = true;
                                brick.Thickness = prevBrick.Thickness;
                                brick.Radius = prevBrick.Radius;
                                brick.MidRadius = prevBrick.Radius * 1.875f;
                                brick.Angle = (int)Math.Floor(prevBrick.Radius * 360 / (Math.PI * brick.MidRadius * 2));
                                brick.Angle = 360.0f / (int)(360 / brick.Angle);

                                if (i == 0)
                                {
                                    brick.RightSideAngle = prevBrick.RightSideAngle + 180;
                                    brick.RightSidePosition = prevBrick.RightSidePosition;
                                    brick.TextureFlip = !prevBrick.TextureFlip;
                                }
                                else if (i == 1)
                                {
                                    brick.LeftSideAngle = prevBrick.RightSideAngle;
                                    brick.LeftSidePosition = prevBrick.RightSidePosition;
                                    brick.TextureFlip = prevBrick.TextureFlip;
                                }
                                else if (i == 2)
                                {
                                    brick.RightSideAngle = prevBrick.LeftSideAngle + 180;
                                    brick.RightSidePosition = prevBrick.LeftSidePosition;
                                    brick.TextureFlip = prevBrick.TextureFlip;
                                }
                                else
                                {
                                    brick.LeftSideAngle = prevBrick.LeftSideAngle;
                                    brick.LeftSidePosition = prevBrick.LeftSidePosition;
                                    brick.TextureFlip = !prevBrick.TextureFlip;
                                }
                                provisionalBricks.Add(brick);
                            }
                        }
                    }
                    else
                    {
                        for (var i = 0; i < 2; i++)
                        {
                            var brick = new Brick(level);
                            brick.TextureFlip = prevBrick.TextureFlip;
                            brick.Thickness = prevBrick.Thickness;
                            if (prevBrick.Curved)
                                brick.Radius = (int)Math.Round(Math.PI * prevBrick.MidRadius * 2 * prevBrick.Angle / 360);
                            else
                                brick.Radius = prevBrick.Radius;

                            if (i == 0)
                            {
                                brick.LeftSideAngle = prevBrick.RightSideAngle + 180;
                                brick.LeftSidePosition = prevBrick.RightSidePosition;
                            }
                            else
                            {
                                brick.RightSideAngle = prevBrick.LeftSideAngle + 180;
                                brick.RightSidePosition = prevBrick.LeftSidePosition;
                            }
                            provisionalBricks.Add(brick);
                        }
                    }

                    var bestBrick = (Brick)null;
                    var bestDistance = float.MaxValue;
                    foreach (var brick in provisionalBricks)
                    {
                        if (!force && level.GetObjectAt(brick.Location.X, brick.Location.Y) != null)
                        {
                            continue;
                        }

                        var dist = brick.Location.Subtract(_placePosition).GetLength();
                        if (dist < bestDistance)
                        {
                            bestDistance = dist;
                            bestBrick = brick;
                        }
                    }

                    if (bestBrick != null)
                    {
                        if (provisional)
                        {
                            Editor.SetProvisonalEntry(bestBrick);
                        }
                        else
                        {
                            Editor.CreateUndoPoint();
                            level.Entries.Add(bestBrick);
                            Editor.ShowEntries(bestBrick);
                            Editor.ChangeSelection(bestBrick);
                            Editor.UpdateRedraw();
                        }
                    }
                }
            }
            else if (selectedEntries.Count == 0 && !swapDirection)
            {
                var brick = new Brick(level);
                brick.Location = _placePosition;
                brick.Rotation = 90;
                if (curved)
                {
                    brick.Curved = true;
                    brick.InnerRadius = 35;
                    brick.Angle = 30;
                }
                if (provisional)
                {
                    Editor.SetProvisonalEntry(brick);
                }
                else
                {
                    Editor.Level.Entries.Add(brick);
                    Editor.ShowEntries(brick);
                    Editor.ChangeSelection(brick);
                    Editor.UpdateRedraw();
                }
            }
        }

        public override void MouseUp(MouseButtons button, Point location, Keys modifierKeys)
        {
            if (button == MouseButtons.Right)
            {
                ShowContextMenu(location);
                return;
            }

            _state = State.None;
            _selectionRect = Rectangle.Empty;

            Editor.UpdateRedraw();
            Editor.InvokeSelectionChangedEvent();
        }

        public override void KeyDown(KeyEventArgs e)
        {
            float x = 0f;
            float y = 0f;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    y = -1f;
                    break;
                case Keys.Down:
                    y = 1f;
                    break;
                case Keys.Left:
                    x = -1f;
                    break;
                case Keys.Right:
                    x = 1f;
                    break;
                case Keys.B:
                case Keys.V:
                    if (!_commandKeyDown)
                    {
                        _commandKeyDown = true;
                        PlaceBrick(!e.Shift, e.KeyCode == Keys.V, e.Control, false);
                    }
                    break;
                default:
                    return;
            }

            if (e.Modifiers == Keys.Control)
            {
                x *= 5f;
                y *= 5f;
            }
            else if (e.Modifiers == Keys.Alt)
            {
                x /= 16f;
                y /= 16f;
            }

            Editor.MoveObjects(x, y);
        }

        public override void KeyUp(KeyEventArgs e)
        {
            base.KeyUp(e);
            _commandKeyDown = false;
        }

        public override void Draw(Graphics g)
        {
            // Draw the mouse selection
            if (_state == State.Selecting)
            {
                // Lines of the same width on screen whatever the zoom
                RectangleF rect = _selectionRect;
                var line = Editor.OverlayLineWidth;
                using (var brush = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                    g.FillRectangle(brush, rect);
                using (var outer = new Pen(Color.CornflowerBlue, line))
                    g.DrawRectangle(outer, rect.X, rect.Y, rect.Width, rect.Height);

                rect.Inflate(-line, -line);
                using (var inner = new Pen(Color.SkyBlue, line))
                    g.DrawRectangle(inner, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        public override object Clone()
        {
            EditorTool tool = new SelectEditorTool();
            CloneTo(tool);

            return tool;
        }

        #region Context Menu

        private void ShowContextMenu(Point location)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            if (Editor.SelectedEntries.Count > 0)
            {
                menu.Items.Add(new ToolStripMenuItem("Bring to Front", Resources.bring_to_front_16, new EventHandler(mnuBringToFront)));
                menu.Items.Add(new ToolStripMenuItem("Send to Back", Resources.send_to_back_16, new EventHandler(mnuSendToBack)));
                menu.Items.Add(new ToolStripSeparator());
            }

            if (Editor.SelectedEntries.Count > 0)
            {
                menu.Items.Add(new ToolStripMenuItem("Cut", Resources.cut_16, new EventHandler(mnuCut)));
                menu.Items.Add(new ToolStripMenuItem("Copy", Resources.copy_16, new EventHandler(mnuCopy)));
            }
            menu.Items.Add(new ToolStripMenuItem("Paste", Resources.paste_16, new EventHandler(mnuPaste)));
            if (Editor.SelectedEntries.Count > 0)
            {
                menu.Items.Add(new ToolStripMenuItem("Delete", Resources.delete_16, new EventHandler(mnuDelete)));
            }

            menu.Show(Editor, Editor.ZoomPoint(location));
        }

        private void mnuBringToFront(object sender, EventArgs e)
        {
            Editor.BringObjectsToFront();
        }

        private void mnuSendToBack(object sender, EventArgs e)
        {
            Editor.SendObjectsToBack();
        }

        private void mnuCut(object sender, EventArgs e)
        {
            Editor.CutObjects();
        }

        private void mnuCopy(object sender, EventArgs e)
        {
            Editor.CopyObjects();
        }

        private void mnuPaste(object sender, EventArgs e)
        {
            Editor.PasteObjects();
        }

        private void mnuDelete(object sender, EventArgs e)
        {
            Editor.DeleteObjects();
        }

        #endregion

        private enum State
        {
            None,
            Selecting,
            MovingObjects,
            MovingPoints,
        }
    }
}

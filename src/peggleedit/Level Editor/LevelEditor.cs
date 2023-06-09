﻿// This file is part of PeggleEdit.
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    class LevelEditor : Control
    {
        private Level mLevel;

        private EditorTool mSelectedTool;

        private Stack<LevelEntry[]> mHistory = new Stack<LevelEntry[]>();

        private LevelEntryCollection mOldSelectedEntries = new LevelEntryCollection();
        private LevelEntryCollection mSelectedEntries = new LevelEntryCollection();
        private List<PointF> mObjectPoints = new List<PointF>();

        private LevelEntryCollection mCopiedEntries = new LevelEntryCollection();

        private bool _commandKeyDown;
        private PointF _placePosition;

        public event EventHandler UpdatedRedrawed;
        public event EventHandler SelectionChanged;

        public LevelEditor()
        {
            DoubleBuffered = true;

            mSelectedTool = new SelectEditorTool();
            mSelectedTool.Editor = this;
        }

        #region Editing and Selecting

        public LevelEntryCollection GetSelectedObjectsInZOrder()
        {
            LevelEntryCollection objects = new LevelEntryCollection();
            foreach (LevelEntry le in Level.Entries)
            {
                if (!mSelectedEntries.Contains(le))
                    continue;

                objects.Add(le);
            }

            return objects;
        }

        public LevelEntryCollection GetSelectedObjects()
        {
            LevelEntryCollection objects = new LevelEntryCollection();
            foreach (LevelEntry le in mSelectedEntries)
            {
                objects.Add(le);
            }

            return objects;
        }

        public void CreateUndoPoint()
        {
            mLevel.UpdateMovementLinksWrite();
            LevelEntryCollection copies = new LevelEntryCollection();
            foreach (LevelEntry le in mLevel.Entries)
            {
                copies.Add((LevelEntry)le.Clone());
            }

            //if (mHistory.Count >= 100) {
            //    Queue<LevelEntry[]> tmpQueue = new Queue<LevelEntry[]>();
            //    while (mHistory.Count > 0) {
            //        tmpQueue.Enqueue(mHistory.Pop());
            //    }

            //    for (int i = 0; i < 99; i++) {
            //        mHistory.Push(tmpQueue.Dequeue());
            //    }
            //}

            mHistory.Push(copies.ToArray());
        }

        public void Undo()
        {
            if (mHistory.Count > 0)
            {
                LevelEntry[] entries = mHistory.Pop();
                mLevel.Entries.Clear();
                mLevel.Entries.AddRange(entries);

                mLevel.UpdateMovementLinksRead();
                UpdateRedraw();
            }
        }

        public void UpdateRedraw()
        {
            UnselectNonExistantPegs();

            Invalidate();

            if (UpdatedRedrawed != null)
                UpdatedRedrawed(this, EventArgs.Empty);
        }

        private void UnselectNonExistantPegs()
        {
            for (int i = 0; i < mSelectedEntries.Count; i++)
            {
                if (!mLevel.Entries.Contains(mSelectedEntries[i]))
                {
                    mSelectedEntries.RemoveAt(i);
                    i--;
                }
            }

            CheckSelectionChanged();

        }

        public void CheckSelectionChanged()
        {
            //Checks if the selection has changed, and only invokes change event if different

            //Check if selection count is different
            if (mSelectedEntries.Count != mOldSelectedEntries.Count)
            {
                UpdateOldSelection();
                InvokeSelectionChangedEvent();
                return;
            }

            //Check all elements exist
            foreach (LevelEntry le in mSelectedEntries)
            {
                if (!mOldSelectedEntries.Contains(le))
                {
                    UpdateOldSelection();
                    InvokeSelectionChangedEvent();
                    return;
                }
            }

            //No change
        }

        private void UpdateOldSelection()
        {
            //Update all the selection attributes on the old entries
            foreach (LevelEntry le in mOldSelectedEntries)
            {
                le.Selected = false;
            }

            mOldSelectedEntries.Clear();
            mOldSelectedEntries.AddRange(mSelectedEntries);

            //Update all the selection attributes on the new entries
            foreach (LevelEntry le in mSelectedEntries)
            {
                le.Selected = true;
            }
        }

        public void InvokeSelectionChangedEvent()
        {
            if (SelectionChanged != null)
                SelectionChanged.Invoke(this, EventArgs.Empty);
        }

        public float SnapToGrid(float p)
        {
            float rindex = (float)Math.Round(p / (float)Settings.Default.GridSize);
            float remainder = p % Settings.Default.GridSize;
            if (remainder <= Settings.Default.SnapThreshold || remainder >= (Settings.Default.GridSize - Settings.Default.SnapThreshold))
            {
                return rindex * Settings.Default.GridSize;
            }

            return p;
        }

        #endregion

        #region Editing Functions

        public void ClearSelection()
        {
            mSelectedEntries.Clear();

            CheckSelectionChanged();

        }

        public void AddToSelection(LevelEntry entry)
        {
            mSelectedEntries.Add(entry);

            CheckSelectionChanged();

        }

        public void SelectAllObjects()
        {
            mSelectedEntries.Clear();
            mSelectedEntries.AddRange(mLevel.Entries);

            UpdateRedraw();

            CheckSelectionChanged();

        }

        public void DeleteObjects()
        {
            if (mSelectedEntries.Count == 0)
                return;

            CreateUndoPoint();

            foreach (LevelEntry le in mSelectedEntries)
            {
                le.OnDelete();
                mLevel.Entries.Remove(le);
            }
            mSelectedEntries.Clear();

            UpdateRedraw();

            CheckSelectionChanged();

        }

        public void CutObjects()
        {
            CopyObjects();
            DeleteObjects();
        }

        public void CopyObjects()
        {
            mCopiedEntries.Clear();
            mCopiedEntries.AddRange(mSelectedEntries);
        }

        public void PasteObjects()
        {
            if (mCopiedEntries.Count == 0)
                return;

            CreateUndoPoint();

            mSelectedEntries.Clear();

            foreach (LevelEntry le in mCopiedEntries)
            {
                LevelEntry cpyLE = (LevelEntry)le.Clone();

                cpyLE.X += 20;
                cpyLE.Y += 20;

                cpyLE.Level = mLevel;
                mLevel.Entries.Add(cpyLE);
                mSelectedEntries.Add(cpyLE);
            }

            UpdateRedraw();

            CheckSelectionChanged();

        }

        public void MoveObjects(float x, float y)
        {
            if (x != 0 || y != 0)
            {
                LevelEntryCollection LevelObjects = GetSelectedObjects();

                if (LevelObjects.Count == 0)
                    return;

                CreateUndoPoint();

                foreach (LevelEntry p in LevelObjects)
                {
                    p.X += x;
                    p.Y += y;
                }

                UpdateRedraw();

                InvokeSelectionChangedEvent();
            }
        }

        public bool AreSelectedObjectsBricksWithSameGeometry()
        {
            bool firstBrick = true;
            RectangleF threshold = RectangleF.Empty;
            float threshold_value = 4.0f;

            LevelEntryCollection objects = GetSelectedObjects();
            foreach (LevelEntry le in objects)
            {
                Brick b = le as Brick;
                if (b == null)
                    return false;

                PointF cc = b.GetCentrePoint();

                if (firstBrick)
                {
                    threshold = new RectangleF(cc.X - (threshold_value / 2), cc.Y - (threshold_value / 2), threshold_value, threshold_value);
                    firstBrick = false;
                }
                else
                {
                    if (!threshold.Contains(cc))
                        return false;
                }
            }

            return true;
        }

        public void RotateBricks(float angle)
        {
            LevelEntryCollection objects = GetSelectedObjects();
            if (objects.Count == 0)
                return;

            PointF cc = ((Brick)(objects[0])).GetCentrePoint();

            foreach (Brick b in objects)
            {
                b.Rotation += angle;
                float radius = b.InnerRadius + (b.Width / 2.0f);
                b.X = cc.X + ((float)Math.Cos(MathExt.ToRadians(-b.Rotation)) * radius);
                b.Y = cc.Y + ((float)Math.Sin(MathExt.ToRadians(-b.Rotation)) * radius);
            }


            //Matrix mx = new Matrix();
            //mx.RotateAt(angle, cc);

            //foreach (Brick b in objects) {
            //    PointF[] pnts = new PointF[] { new PointF(b.X, b.Y) };
            //    mx.TransformPoints(pnts);
            //    b.X = pnts[0].X;
            //    b.Y = pnts[0].Y;

            //    b.Angle = FixAngle(b.Angle);
            //    b.Angle -= angle;
            //}

            UpdateRedraw();
        }

        public void RotateObjects(float angle)
        {
            //Check if all objects are bricks with same geometry
            if (AreSelectedObjectsBricksWithSameGeometry())
            {
                RotateBricks(angle);
                return;
            }

            LevelEntryCollection objects = new LevelEntryCollection(GetSelectedObjects());
            if (objects.Count == 1)
            {
                if (objects[0] is Brick)
                {
                    CreateUndoPoint();

                    ((Brick)objects[0]).Rotation = MathExt.FixAngle(((Brick)objects[0]).Rotation + angle);

                    UpdateRedraw();
                }
                return;
            }

            if (objects.Count < 2)
                return;

            CreateUndoPoint();

            //Get resize rect
            float left = float.MaxValue;
            float top = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MinValue;

            foreach (LevelEntry obj in objects)
            {
                if (left > obj.X)
                    left = obj.X;
                if (right < obj.X)
                    right = obj.X;
                if (top > obj.Y)
                    top = obj.Y;
                if (bottom < obj.Y)
                    bottom = obj.Y;
            }

            float width = right - left;
            float height = bottom - top;

            Matrix matrix = new Matrix();
            matrix.RotateAt(angle, new PointF(left + (width / 2.0f), top + (height / 2.0f)));

            foreach (LevelEntry obj in objects)
            {
                PointF[] pnts = new PointF[] { new PointF(obj.X, obj.Y) };
                matrix.TransformPoints(pnts);
                obj.X = pnts[0].X;
                obj.Y = pnts[0].Y;

                if (obj is Brick)
                {
                    Brick brick = (Brick)obj;
                    brick.Rotation = MathExt.FixAngle(brick.Rotation);
                    brick.Rotation -= angle;
                }
            }

            UpdateRedraw();
        }

        public void FlipObjectsHorizontally()
        {
            CreateUndoPoint();

            LevelEntryCollection objects = new LevelEntryCollection(GetSelectedObjects());
            if (objects.Count < 2)
                return;

            objects.Sort(new Comparison<LevelEntry>(CompareObjectsByX));

            float startX = objects[0].X;
            float width = objects[objects.Count - 1].X - objects[0].X;

            foreach (LevelEntry o in objects)
            {
                o.X = (startX + width) - (o.X - startX);

                if (o is Brick)
                {
                    Brick brick = (Brick)o;
                    brick.Rotation = MathExt.FixAngle(brick.Rotation);

                    if (brick.Rotation < 180.0f)
                        brick.Rotation = 180.0f - brick.Rotation;
                    else
                        brick.Rotation = 540.0f - brick.Rotation;
                }
            }

            UpdateRedraw();
        }

        public void FlipPegsVertically()
        {
            CreateUndoPoint();

            LevelEntryCollection objects = new LevelEntryCollection(GetSelectedObjects());
            if (objects.Count < 2)
                return;

            objects.Sort(new Comparison<LevelEntry>(CompareObjectsByY));

            float startY = objects[0].Y;
            float height = objects[objects.Count - 1].Y - objects[0].Y;

            foreach (LevelEntry o in objects)
            {
                o.Y = (startY + height) - (o.Y - startY);

                if (o is Brick)
                {
                    Brick brick = (Brick)o;
                    brick.Rotation = MathExt.FixAngle(brick.Rotation);
                    brick.Rotation = 360.0f - brick.Rotation;
                }
            }

            UpdateRedraw();
        }

        public void AlignObjectXs()
        {
            LevelEntryCollection objects = GetSelectedObjects();

            if (objects.Count < 2)
                return;

            CreateUndoPoint();

            float x = objects[0].X;
            foreach (LevelEntry o in objects)
            {
                o.X = x;
            }

            UpdateRedraw();
        }

        public void AlignObjectYs()
        {
            LevelEntryCollection objects = GetSelectedObjects();

            if (objects.Count < 2)
                return;

            CreateUndoPoint();

            float y = objects[0].Y;
            foreach (LevelEntry o in objects)
            {
                o.Y = y;
            }

            UpdateRedraw();
        }

        public void SpaceObjectXsEqually()
        {
            LevelEntryCollection objects = GetSelectedObjects();

            if (objects.Count < 3)
                return;

            CreateUndoPoint();

            //Sort list by X
            objects.Sort(new Comparison<LevelEntry>(CompareObjectsByX));

            //Common spacing
            float xSpacing = objects[1].X - objects[0].X;

            for (int i = 2; i < objects.Count; i++)
            {
                objects[i].X = objects[i - 1].X + xSpacing;
            }

            UpdateRedraw();
        }

        public void SpaceObjectYsEqually()
        {
            LevelEntryCollection objects = GetSelectedObjects();

            if (objects.Count < 3)
                return;

            CreateUndoPoint();

            //Sort list by Y
            objects.Sort(new Comparison<LevelEntry>(CompareObjectsByY));

            //Common spacing
            float ySpacing = objects[1].Y - objects[0].Y;

            for (int i = 2; i < objects.Count; i++)
            {
                objects[i].Y = objects[i - 1].Y + ySpacing;
            }

            UpdateRedraw();
        }

        public void BringObjectsForward()
        {
            LevelEntryCollection objs = GetSelectedObjectsInZOrder();

            foreach (LevelEntry obj in objs)
            {
                int index = mLevel.Entries.IndexOf(obj);

                //Top of the list, no change
                if (index == mLevel.Entries.Count - 1)
                    continue;

                //Move up one
                int new_index = index + 1;

                LevelEntry eo = mLevel.Entries[new_index];
                if (objs.Contains(eo))
                    continue;

                mLevel.Entries[index] = mLevel.Entries[new_index];
                mLevel.Entries[new_index] = obj;
            }

            UpdateRedraw();
        }

        public void SendObjectsBackward()
        {
            LevelEntryCollection objs = GetSelectedObjectsInZOrder();

            foreach (LevelEntry obj in objs)
            {
                int index = mLevel.Entries.IndexOf(obj);

                //Top of the list, no change
                if (index == 0)
                    continue;

                //Move up one
                int new_index = index - 1;

                LevelEntry eo = mLevel.Entries[new_index];
                if (objs.Contains(eo))
                    continue;

                mLevel.Entries[index] = mLevel.Entries[new_index];
                mLevel.Entries[new_index] = obj;
            }

            UpdateRedraw();
        }

        public void BringObjectsToFront()
        {
            LevelEntryCollection objs = GetSelectedObjectsInZOrder();
            //objs.Reverse();

            foreach (LevelEntry obj in objs)
            {
                //Remove from list altogether
                mLevel.Entries.Remove(obj);

                //Add again at the bottom of the list
                mLevel.Entries.Add(obj);
            }

            UpdateRedraw();
        }

        public void SendObjectsToBack()
        {
            //Reverse the order so that selected objects maintain their inner z order
            LevelEntryCollection objs = GetSelectedObjectsInZOrder();
            objs.Reverse();

            foreach (LevelEntry obj in objs)
            {
                //Remove from list altogether
                mLevel.Entries.Remove(obj);

                //Add again at the top of the list
                mLevel.Entries.Insert(0, obj);
            }

            UpdateRedraw();
        }

        public void RemoveDuplicateObjects()
        {
            if (mSelectedEntries.Count == 0)
                SelectAllObjects();

            if (mSelectedEntries.Count == 0)
                return;

            CreateUndoPoint();

            foreach (LevelEntry obj1 in GetSelectedObjects())
            {
                if (!mLevel.Entries.Contains(obj1))
                    continue;

                foreach (LevelEntry obj2 in GetSelectedObjects())
                {
                    if (!mLevel.Entries.Contains(obj1))
                        continue;

                    if (obj1 == obj2)
                        continue;

                    if (obj1.MovementLink != null || obj2.MovementLink != null)
                        continue;

                    if (obj1.X == obj2.X && obj1.Y == obj2.Y)
                    {
                        obj2.OnDelete();
                        mLevel.Entries.Remove(obj2);
                    }
                }
            }

            UpdateRedraw();

            CheckSelectionChanged();

        }

        public void RemoveOffscreenObjects()
        {
            bool firstOne = true;
            LevelEntryCollection removes = new LevelEntryCollection();
            RectangleF insideRect = RectangleF.FromLTRB(-Level.DrawAdjustX, -Level.DrawAdjustY, 800 - Level.DrawAdjustX, 600 - Level.DrawAdjustY);
            foreach (LevelEntry o in mLevel.Entries)
            {
                PointF pegPnt = new PointF(o.X, o.Y);

                if (!insideRect.Contains(pegPnt))
                {
                    if (firstOne)
                    {
                        CreateUndoPoint();
                        firstOne = false;
                    }

                    removes.Add((LevelEntry)o);
                }
            }

            foreach (LevelEntry le in removes)
            {
                le.OnDelete();
                mLevel.Entries.Remove(le);
            }

            UpdateRedraw();

            CheckSelectionChanged();

        }

        private void PlaceBrick(bool curved, bool swapDirection, bool force)
        {
            if (SelectedEntries.Count == 1)
            {
                if (SelectedEntries[0] is Brick prevBrick)
                {
                    var provisionalBricks = new List<Brick>();

                    if (curved)
                    {
                        if (prevBrick.Curved)
                        {
                            for (var i = 0; i < 2; i++)
                            {
                                var brick = new Brick(Level);
                                brick.Curved = true;
                                brick.Width = prevBrick.Width;
                                brick.Length = prevBrick.Length;
                                brick.SectorAngle = prevBrick.SectorAngle;

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
                                        brick.Rotation = prevBrick.Rotation - prevBrick.SectorAngle;
                                    }
                                    else
                                    {
                                        brick.Rotation = prevBrick.Rotation + prevBrick.SectorAngle;
                                    }

                                    var newRotation = MathExt.ToRadians(-brick.Rotation);
                                    var radius = prevBrick.Radius;
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
                                var brick = new Brick(Level);
                                brick.Curved = true;
                                brick.Width = prevBrick.Width;
                                brick.Length = prevBrick.Length;
                                brick.Radius = prevBrick.Length * 1.875f;
                                brick.SectorAngle = (int)Math.Floor(prevBrick.Length * 360 / (Math.PI * brick.Radius * 2));
                                brick.SectorAngle = 360.0f / (int)(360 / brick.SectorAngle);

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
                            var brick = new Brick(Level);
                            brick.TextureFlip = prevBrick.TextureFlip;
                            brick.Width = prevBrick.Width;
                            if (prevBrick.Curved)
                                brick.Length = (int)Math.Round(Math.PI * prevBrick.Radius * 2 * prevBrick.SectorAngle / 360);
                            else
                                brick.Length = prevBrick.Length;

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
                        if (!force && Level.GetObjectAt(brick.Location.X, brick.Location.Y) != null)
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
                        CreateUndoPoint();
                        Level.Entries.Add(bestBrick);
                        ClearSelection();
                        AddToSelection(bestBrick);
                        UpdateRedraw();
                    }
                }
            }
            else if (SelectedEntries.Count == 0 && !swapDirection)
            {
                var brick = new Brick(Level);
                brick.Location = _placePosition;
                brick.Rotation = 90;
                if (curved)
                {
                    brick.Curved = true;
                    brick.InnerRadius = 35;
                    brick.SectorAngle = 30;
                }
                Level.Entries.Add(brick);
                AddToSelection(brick);
                UpdateRedraw();
            }
        }

        #endregion

        #region Helper Functions

        private int CompareObjectsByX(LevelEntry a, LevelEntry b)
        {
            if (a.X < b.X)
                return -1;
            else if (a.X > b.X)
                return 1;
            else
                return 0;
        }

        private int CompareObjectsByY(LevelEntry a, LevelEntry b)
        {
            if (a.Y < b.Y)
                return -1;
            else if (a.Y > b.Y)
                return 1;
            else
                return 0;
        }

        #endregion

        #region Painting

        private void Draw(Graphics g)
        {
            //Draw the level
            if (mLevel == null)
            {
                g.FillRectangle(Brushes.Black, Bounds);
            }
            else
            {
                mLevel.UsePegTextures = Settings.Default.UsePegTextures;
                mLevel.Draw(g);
            }

            DrawGrid(g);

            if (mSelectedTool != null)
                mSelectedTool.Draw(g);

            //Draw the individual object selection rectangles
            foreach (LevelEntry le in GetSelectedObjects())
                DrawEntrySelection(g, le);
        }

        private void DrawEntrySelection(Graphics g, LevelEntry le)
        {
            //Set the pen
            Pen selectedEntryPen = new Pen(Brushes.White, 1.0f);
            selectedEntryPen.DashPattern = new float[] { 1, 1 };

            Rectangle rect = new Rectangle();

            Point p = mLevel.GetActualXY(le.Bounds.Location);

            rect = Rectangle.Round(le.Bounds);
            rect.Location = p;
            rect.Inflate(5, 5);

            g.DrawRectangle(selectedEntryPen, rect);
        }

        private void DrawGrid(Graphics g)
        {
            if (!Settings.Default.ShowGrid)
                return;

            int x_lines = Width / Settings.Default.GridSize;
            int y_lines = Height / Settings.Default.GridSize;

            Pen p = new Pen(Color.FromArgb(64, 0, 0, 255));

            Point pnt = Level.GetActualXY(0, 0);

            for (int y = pnt.Y; y >= 0; y -= Settings.Default.GridSize)
            {
                g.DrawLine(p, 0, y, Width, y);
            }
            for (int y = pnt.Y; y < Height; y += Settings.Default.GridSize)
            {
                g.DrawLine(p, 0, y, Width, y);
            }

            for (int x = pnt.X; x >= 0; x -= Settings.Default.GridSize)
            {
                g.DrawLine(p, x, 0, x, Height);
            }
            for (int x = pnt.X; x < Width; x += Settings.Default.GridSize)
            {
                g.DrawLine(p, x, 0, x, Height);
            }
        }

        #endregion

        #region Overidden Events

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            if (mLevel == null)
                return;

            if (mSelectedTool != null)
                mSelectedTool.MouseDown(e.Button, e.Location, Control.ModifierKeys);
        }

        private LevelEntry lastOverObject;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mLevel == null)
                return;

            Point virtualLocation = Level.GetVirtualXY(e.Location);
            _placePosition = virtualLocation;

            LevelEntry entry = Level.GetObjectAt(virtualLocation.X, virtualLocation.Y);
            if (entry == null)
            {
                if (lastOverObject != null)
                {
                    lastOverObject.MouseOver = false;
                    lastOverObject = null;

                    UpdateRedraw();
                }
            }
            else
            {
                entry.MouseOver = true;
                if (lastOverObject != entry)
                {
                    if (lastOverObject != null)
                        lastOverObject.MouseOver = false;
                    lastOverObject = entry;

                    UpdateRedraw();
                }
            }

            if (mSelectedTool != null)
                mSelectedTool.MouseMove(e.Button, e.Location, Control.ModifierKeys);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (mSelectedTool != null)
                mSelectedTool.MouseUp(e.Button, e.Location, Control.ModifierKeys);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            OnKeyDown(new KeyEventArgs(keyData));

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            int x = 0;
            int y = 0;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    y = -1;
                    break;
                case Keys.Down:
                    y = 1;
                    break;
                case Keys.Left:
                    x = -1;
                    break;
                case Keys.Right:
                    x = 1;
                    break;
                case Keys.B:
                case Keys.V:
                    if (!_commandKeyDown)
                    {
                        _commandKeyDown = true;
                        PlaceBrick(!e.Shift, e.KeyCode == Keys.V, e.Control);
                    }
                    break;
                default:
                    return;
            }

            if (e.Modifiers == Keys.Control)
            {
                x *= 5;
                y *= 5;
            }

            MoveObjects(x, y);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _commandKeyDown = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Draw(e.Graphics);
        }

        #endregion

        #region Properties

        public Level Level
        {
            get
            {
                return mLevel;
            }
            set
            {
                mLevel = value;

                ClearSelection();

                UpdateRedraw();
            }
        }

        public EditorTool SelectedTool
        {
            get
            {
                return mSelectedTool;
            }
            set
            {
                mSelectedTool = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LevelEntryCollection SelectedEntries
        {
            get
            {
                return mSelectedEntries;
            }
            set
            {
                mSelectedEntries = value;

                CheckSelectionChanged();

            }
        }

        public int PegCount
        {
            get
            {
                if (mLevel == null)
                    return 0;
                else
                    return mLevel.Entries.Count;
            }
        }

        public int ObjectSelectionCount
        {
            get
            {
                return GetSelectedObjects().Count;
            }
        }

        #endregion
    }
}

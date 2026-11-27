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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
    internal class LevelEditor : Control
    {
        private Level mLevel;

        private EditorTool mSelectedTool;

        private Stack<(LevelEntry[] Entries, (int Index, int Magic)[] NullEntries)> mHistory =
            new Stack<(LevelEntry[] Entries, (int Index, int Magic)[] NullEntries)>();

        private LevelEntryCollection mOldSelectedEntries = new LevelEntryCollection();
        private LevelEntryCollection mSelectedEntries = new LevelEntryCollection();
        private List<PointF> mObjectPoints = new List<PointF>();

        private LevelEntry _provisionalEntry;
        private Point _lastMousePosition;

        public event EventHandler UpdatedRedrawed;
        public event EventHandler SelectionChanged;

        // The buffer the editor is drawn on before it is shown. The control is the size of the level with its margin,
        // larger than the buffer WinForms keeps for double buffering, which would make a new one for every paint.
        private readonly BufferedGraphicsContext _bufferContext = new BufferedGraphicsContext();

        public LevelEditor()
        {
            // Everything is drawn in OnPaint, on the editor's own buffer
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque, true);

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
            foreach (LevelEntry le in mLevel.Entries)
            {
                le.Selected = SelectedEntries.Contains(le);
            }

            mLevel.UpdateMoverLinksWrite();
            var copies = new LevelEntryCollection();
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

            mHistory.Push((copies.ToArray(), mLevel.NullEntries.ToArray()));
        }

        public void Undo()
        {
            if (mHistory.Count > 0)
            {
                var (entries, nullEntries) = mHistory.Pop();
                mLevel.Entries.Clear();
                mLevel.Entries.AddRange(entries);
                mLevel.NullEntries.Clear();
                mLevel.NullEntries.AddRange(nullEntries);

                mLevel.UpdateMoverLinksRead();

                if (mSelectedTool is SelectEditorTool)
                    ChangeSelection(entries.Where(x => x.Selected));
                UpdateRedraw();
            }
        }

        public void UpdateRedraw()
        {
            UnselectNonExistantPegs();

            RequestRedraw(null);

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

        public LevelEntry ProvisionalEntry => _provisionalEntry;

        public void SetProvisonalEntry(LevelEntry entry)
        {
            _provisionalEntry = entry;
            if (_provisionalEntry != null)
                _provisionalEntry.Provisional = true;
            UpdateRedraw();
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

        public void ChangeSelection(LevelEntry entry) => ChangeSelection(new[] { entry });

        public void ChangeSelection(IEnumerable<LevelEntry> entries)
        {
            mSelectedEntries.Clear();
            mSelectedEntries.AddRange(entries);
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
            if (CopyObjects())
                DeleteObjects();
        }

        public bool CopyObjects()
        {
            if (mSelectedEntries.Count == 0)
                return false;

            var data = LevelEntryClipboard.Serialize(GetSelectedObjectsInZOrder().Cast<LevelEntry>());
            var dataObject = new DataObject();
            dataObject.SetData(LevelEntryClipboard.FormatName, false, new MemoryStream(data));
            try
            {
                Clipboard.SetDataObject(dataObject, true, 10, 100);
                return true;
            }
            catch (ExternalException)
            {
                MessageBox.Show("Unable to copy to the clipboard.", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static LevelEntry[] GetClipboardEntries()
        {
            try
            {
                if (Clipboard.GetData(LevelEntryClipboard.FormatName) is MemoryStream ms)
                    return LevelEntryClipboard.Deserialize(ms.ToArray());
            }
            catch (Exception)
            {
                // Clipboard is unavailable or holds data PeggleEdit does not understand
            }
            return null;
        }

        public void PasteObjects()
        {
            var entries = GetClipboardEntries();
            if (entries == null || entries.Length == 0)
                return;

            CreateUndoPoint();

            mSelectedEntries.Clear();

            // Only the anchor of the base movement is absolute, so move each base movement once
            // to keep entries that share movements in the same relative position
            var offsetMovements = new HashSet<Mover>();
            foreach (LevelEntry cpyLE in entries)
            {
                var baseMovement = cpyLE.GetBaseMover();
                if (baseMovement == null)
                {
                    cpyLE.X += 20;
                    cpyLE.Y += 20;
                }
                else if (offsetMovements.Add(baseMovement))
                {
                    baseMovement.AnchorPointX += 20;
                    baseMovement.AnchorPointY += 20;
                }

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
                float radius = b.InnerRadius + (b.Thickness / 2.0f);
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

                    if (obj1.MoverLink != null || obj2.MoverLink != null)
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

        /// <summary>
        /// Removes the pegs and bricks that no shot from the cannon can hit before it hits anything else (see
        /// <see cref="CannonReach"/>), in normal play or in any challenge in the pack that plays the level, returning
        /// how many were removed.
        /// </summary>
        public int RemoveUnreachableObjects()
        {
            var shots = CannonReach.GetShots(Tools.Pack.LevelPack.Current, mLevel);
            var removes = mLevel.Entries.Cast<LevelEntry>().Where(x => CannonReach.IsUnreachable(x, shots)).ToList();
            if (removes.Count == 0)
                return 0;

            CreateUndoPoint();
            foreach (LevelEntry le in removes)
            {
                le.OnDelete();
                mLevel.Entries.Remove(le);
            }

            UpdateRedraw();
            CheckSelectionChanged();
            return removes.Count;
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
            // Draw the level
            if (mLevel == null)
            {
                g.FillRectangle(Brushes.Black, Bounds);
            }
            else
            {
                mLevel.UsePegTextures = Settings.Default.UsePegTextures;
                mLevel.Devices = Settings.Default.Devices;
                mLevel.SoftwareRendering = !Settings.Default.Acceleration3D;
                mLevel.Draw(g);
            }

            if (_provisionalEntry != null)
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                var backupTransform = g.Transform;
                var origin = mLevel.GetActualXY(0, 0);
                g.TranslateTransform(origin.X, origin.Y);
                _provisionalEntry.Draw(g);
                g.Transform = backupTransform;
                g.SmoothingMode = SmoothingMode.Default;
            }

            DrawGrid(g);

            if (mSelectedTool != null)
                mSelectedTool.Draw(g);

            // Draw the individual object selection rectangles
            foreach (LevelEntry le in GetSelectedObjects())
                DrawEntrySelection(g, le);
        }

        private void DrawEntrySelection(Graphics g, LevelEntry le)
        {
            var bounds = le.Bounds;
            var origin = mLevel.GetActualXY(0, 0);
            var corners = new[]
            {
                new PointF(origin.X + bounds.Left, origin.Y + bounds.Top),
                new PointF(origin.X + bounds.Right, origin.Y + bounds.Bottom),
            };

            // Drawn on whole device pixels with a pen of whole pixels, at the same size wherever the object is, so that
            // the dashes look the same as it moves (zoomed, a part of a pixel blurred or moved them). The object's edge
            // is put on a pixel as its image is (see DrawImageSnapped), and the box a whole number of pixels (5 at 96
            // DPI, whatever the zoom) out from it, so that it stays the same distance from the object.
            g.TransformPoints(CoordinateSpace.Device, CoordinateSpace.World, corners);
            int margin = (int)Math.Round(5 * Dpi.Factor);
            int left = (int)Tools.Extensions.GraphicsExtensions.RoundToPixel(corners[0].X) - margin;
            int top = (int)Tools.Extensions.GraphicsExtensions.RoundToPixel(corners[0].Y) - margin;
            int width = (int)Math.Round(corners[1].X - corners[0].X) + (2 * margin);
            int height = (int)Math.Round(corners[1].Y - corners[0].Y) + (2 * margin);
            int penWidth = Math.Max(1, (int)Math.Round(Dpi.Factor));

            var state = g.Save();
            g.ResetTransform();
            g.SmoothingMode = SmoothingMode.None;
            using (var pen = new Pen(Color.White, penWidth))
            {
                pen.DashPattern = new float[] { 1, 1 };
                g.DrawRectangle(pen, left, top, width, height);
            }
            g.Restore(state);
        }

        private void DrawGrid(Graphics g)
        {
            if (!Settings.Default.ShowGrid)
                return;

            // The grid is drawn in unzoomed coordinates, so it covers the editor's unzoomed size
            int width = (int)Math.Ceiling(Width / Zoom);
            int height = (int)Math.Ceiling(Height / Zoom);

            using (var p = new Pen(Color.FromArgb(64, 0, 0, 255), OverlayLineWidth))
            {
                Point pnt = Level.GetActualXY(0, 0);

                for (int y = pnt.Y; y >= 0; y -= Settings.Default.GridSize)
                {
                    g.DrawLine(p, 0, y, width, y);
                }
                for (int y = pnt.Y; y < height; y += Settings.Default.GridSize)
                {
                    g.DrawLine(p, 0, y, width, y);
                }

                for (int x = pnt.X; x >= 0; x -= Settings.Default.GridSize)
                {
                    g.DrawLine(p, x, 0, x, height);
                }
                for (int x = pnt.X; x < width; x += Settings.Default.GridSize)
                {
                    g.DrawLine(p, x, 0, x, height);
                }
            }
        }

        #endregion

        #region Overidden Events

        protected override void OnMouseDown(MouseEventArgs e)
        {
            e = Unzoom(e);
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
            e = Unzoom(e);
            base.OnMouseMove(e);

            if (mLevel == null)
                return;

            var virtualLocation = Level.GetVirtualXY(e.Location);
            var prevObject = lastOverObject;
            if (lastOverObject != null)
            {
                lastOverObject.MouseOver = false;
                lastOverObject = null;
            }

            var entry = Level.GetObjectAt(virtualLocation.X, virtualLocation.Y);
            if (entry != null && entry.HitTest(virtualLocation))
            {
                entry.MouseOver = true;
                lastOverObject = entry;
            }

            // Only how the objects are drawn has changed, not the objects
            if (lastOverObject != prevObject)
                RequestRedraw(null);

            AimCannon(virtualLocation);

            if (mSelectedTool != null)
                mSelectedTool.MouseMove(e.Button, e.Location, Control.ModifierKeys);

            _lastMousePosition = e.Location;
        }

        /// <summary>
        /// Turns the cannon to point at the mouse, as the game does, so that it can be seen how near it comes to the
        /// level's objects. Only the cannon is drawn again.
        /// </summary>
        private void AimCannon(Point virtualLocation)
        {
            var oldAngle = Cannon.GetAngle(mLevel.CannonTarget);
            mLevel.CannonTarget = virtualLocation;
            if (!mLevel.ShowCannon || Cannon.GetAngle(mLevel.CannonTarget) == oldAngle)
                return;

            var bounds = Cannon.Bounds;
            var topLeft = mLevel.GetActualXY(bounds.Left, bounds.Top);
            var bottomRight = mLevel.GetActualXY(bounds.Right, bounds.Bottom);
            // The bounds are in level coordinates, which are zoomed on screen
            var zoomedTopLeft = ZoomPoint(new Point(topLeft.X - 1, topLeft.Y - 1));
            var zoomedBottomRight = ZoomPoint(new Point(bottomRight.X + 1, bottomRight.Y + 1));
            RequestRedraw(Rectangle.FromLTRB(zoomedTopLeft.X - 1, zoomedTopLeft.Y - 1, zoomedBottomRight.X + 1, zoomedBottomRight.Y + 1));
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            e = Unzoom(e);
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
            mSelectedTool?.KeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            mSelectedTool?.KeyUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var clip = e.ClipRectangle;
            if (clip.Width <= 0 || clip.Height <= 0)
                return;

            var maximum = _bufferContext.MaximumBuffer;
            if (clip.Width > maximum.Width || clip.Height > maximum.Height)
                _bufferContext.MaximumBuffer = new Size(Math.Max(clip.Width, maximum.Width), Math.Max(clip.Height, maximum.Height));
            using (var buffer = _bufferContext.Allocate(e.Graphics, clip))
            {
                var g = buffer.Graphics;
                g.SetClip(clip);
                base.OnPaint(new PaintEventArgs(g, clip));
                if (Zoom != 1)
                    g.ScaleTransform(Zoom, Zoom);
                Draw(g);
                buffer.Render(e.Graphics);
            }

            // A paint not in a frame (e.g. the editor being uncovered) can be the first since the preview was turned on
            if (!_inFrame)
                ScheduleFrame();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bufferContext.Dispose();
                mLevel?.ReleaseDrawingResources();
                _frameTimer?.Dispose();
                _frameTimer = null;
                SetWaitingForIdle(false);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Frames

        // The least time between frames, as the preview's timer had
        private static readonly TimeSpan FrameInterval = TimeSpan.FromMilliseconds(10);

        // How long an overdue frame waits for the messages before it (e.g. input) to be handled, at most
        private const int OverdueFrameWait = 15;

        private readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();
        private TimeSpan _lastFrame = -FrameInterval;
        private TimeSpan _lastFrameEnd = -FrameInterval;
        private TimeSpan _lastFrameDuration;

        // The least time left between the end of a frame and the start of the next, as a part of how long the frame
        // took, so that the rest of the application (painting other windows, timers, input) is never starved by frames
        // that take longer than the frame interval (e.g. a large level with emitters, zoomed for a high DPI display)
        private const double FrameGapRatio = 0.2;
        private static readonly TimeSpan MinFrameGap = TimeSpan.FromMilliseconds(2);
        private static readonly TimeSpan MaxFrameGap = TimeSpan.FromMilliseconds(30);
        private System.Threading.Timer _frameTimer;
        private bool _frameScheduled;
        private bool _waitingForIdle;
        private bool _inFrame;

        // Each frame scheduled is numbered, and a frame posted for one that has been drawn is ignored (the timer and the
        // idle event can both post it). Posted while a frame was drawn, it ran the next straight after it, as the posts
        // waiting are all handled before any other message: each frame then started the next with no gap, as the timer
        // for one fired during the next, so input was never handled again.
        private int _frameNumber;

        // When the frame scheduled is due
        private TimeSpan _frameDue;

        // The timer can fire up to a tick of the system clock early
        private static readonly TimeSpan TimerResolution = TimeSpan.FromMilliseconds(16);

        // While a frame runs, a frame asked for is scheduled when it ends, as the gap after it depends on how long it took
        private bool _inRunFrame;
        private bool _frameRequested;

        // What is drawn in the next frame: everything, or this area (nothing when it is empty)
        private bool _redrawAll;
        private Rectangle _redrawArea;

        /// <summary>
        /// Raised at the start of each frame, before it is drawn, e.g. to move the preview on. It can set
        /// <see cref="Animating"/>.
        /// </summary>
        public event EventHandler FrameStarting;

        /// <summary>
        /// Gets or sets whether all of the editor is drawn every frame, e.g. while the preview plays.
        /// </summary>
        public bool Animating { get; set; }

        /// <summary>
        /// Draws an area of the editor (in control coordinates), or with null all of it, again in the next frame.
        /// <para>
        /// The editor is drawn in frames, so that a stream of changes (e.g. the cannon following the mouse) is drawn
        /// once a frame rather than once for each change, and so that the mouse can not hold the drawing back: paints
        /// and timer messages are only handled when no input is waiting, which a fast mouse can keep from happening. A
        /// frame that is due is drawn as soon as the messages waiting have been handled, and is otherwise posted by a
        /// timer, as posted messages are handled before input.
        /// </para>
        /// </summary>
        public void RequestRedraw(Rectangle? area)
        {
            if (area is Rectangle rect)
                _redrawArea = _redrawArea.IsEmpty ? rect : Rectangle.Union(_redrawArea, rect);
            else
                _redrawAll = true;
            ScheduleFrame();
        }

        private void ScheduleFrame()
        {
            if (_frameScheduled || IsDisposed)
                return;
            if (_inRunFrame)
            {
                _frameRequested = true;
                return;
            }
            if (!IsHandleCreated)
            {
                // Drawn when it is shown (OnHandleCreated starts the frames)
                Invalidate();
                return;
            }

            _frameScheduled = true;
            Interlocked.Increment(ref _frameNumber);
            var gap = TimeSpan.FromTicks((long)(_lastFrameDuration.Ticks * FrameGapRatio));
            if (gap < MinFrameGap)
                gap = MinFrameGap;
            if (gap > MaxFrameGap)
                gap = MaxFrameGap;
            var due = _lastFrame + FrameInterval;
            if (due < _lastFrameEnd + gap)
                due = _lastFrameEnd + gap;
            var wait = (int)Math.Ceiling((due - _clock.Elapsed).TotalMilliseconds);
            if (wait <= 0)
            {
                SetWaitingForIdle(true);
                // Without idle time (e.g. a mouse moving fast enough to always have input waiting, or in a modal loop
                // such as a window being moved, which has no idle event), it is posted
                wait = OverdueFrameWait;
            }
            _frameDue = due;
            if (_frameTimer == null)
                _frameTimer = new System.Threading.Timer(_ => PostFrame());
            _frameTimer.Change(wait, Timeout.Infinite);
        }

        private void SetWaitingForIdle(bool value)
        {
            if (_waitingForIdle == value)
                return;
            _waitingForIdle = value;
            if (value)
                Application.Idle += Application_Idle;
            else
                Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            // Posted, as the message loop waits for a message after the idle event
            SetWaitingForIdle(false);
            PostFrame();
        }

        // Called on a thread pool thread, by the timer
        private void PostFrame()
        {
            try
            {
                BeginInvoke(new Action<int>(RunFrame), Volatile.Read(ref _frameNumber));
            }
            catch (InvalidOperationException)
            {
                // The handle has been destroyed: OnHandleCreated starts the frames again
            }
        }

        private void RunFrame(int number)
        {
            // The timer and the idle event can both post the frame
            if (!_frameScheduled || IsDisposed || number != _frameNumber)
                return;
            var early = _frameDue - _clock.Elapsed;
            if (early > TimerResolution)
            {
                // Posted for the frame before, by a timer that fired as this one was scheduled
                _frameTimer?.Change((int)Math.Ceiling(early.TotalMilliseconds), Timeout.Infinite);
                return;
            }
            _frameScheduled = false;
            SetWaitingForIdle(false);
            _frameTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            _lastFrame = _clock.Elapsed;
            _inRunFrame = true;
            try
            {
                FrameStarting?.Invoke(this, EventArgs.Empty);
                if (Animating || _redrawAll)
                    Invalidate();
                else if (!_redrawArea.IsEmpty)
                    Invalidate(_redrawArea);
                _redrawAll = false;
                _redrawArea = Rectangle.Empty;
                // What was asked for so far is drawn in this frame
                _frameRequested = false;

                // Drawn now, rather than when there are no other messages
                _inFrame = true;
                try
                {
                    Update();
                }
                finally
                {
                    _inFrame = false;
                }
            }
            finally
            {
                _inRunFrame = false;
            }
            _lastFrameEnd = _clock.Elapsed;
            _lastFrameDuration = _lastFrameEnd - _lastFrame;

            if (Animating || _frameRequested)
                ScheduleFrame();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _frameScheduled = false;
            RequestRedraw(null);
        }

        #endregion

        #region Zoom

        /// <summary>
        /// The zoom levels the user can choose, as parts of the level's size at 100%.
        /// </summary>
        public static readonly float[] ZoomLevels = { 0.25f, 1 / 3f, 0.5f, 2 / 3f, 0.75f, 1, 1.25f, 1.5f, 2, 3, 4, 6, 8 };

        // The editor is a control as large as the zoomed level, and Windows gives the mouse's position in a control as
        // a 16 bit number, so the control is kept smaller than that
        private const int MaxControlSize = 32000;

        private float mZoomLevel = 1;

        /// <summary>
        /// Raised when <see cref="ZoomLevel"/> changes.
        /// </summary>
        public event EventHandler ZoomChanged;

        /// <summary>
        /// Gets or sets the zoom the user has chosen, 1 for 100%. It is kept within <see cref="ZoomLevels"/>' range,
        /// and to what the editor can be at most (see <see cref="MaxZoomLevel"/>). The editor is resized to fit.
        /// </summary>
        public float ZoomLevel
        {
            get => mZoomLevel;
            set
            {
                value = Math.Max(ZoomLevels[0], Math.Min(MaxZoomLevel, value));
                if (value == mZoomLevel)
                    return;
                mZoomLevel = value;
                if (mLevel != null)
                    Size = ZoomSize(mLevel.Bounds.Size);
                RequestRedraw(null);
                ZoomChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the most the level can be zoomed: the largest of <see cref="ZoomLevels"/> at which the editor is not too
        /// large for Windows. It is less on a display with a high DPI scale.
        /// </summary>
        public float MaxZoomLevel
        {
            get
            {
                var largest = mLevel == null ? 1 : Math.Max(mLevel.Bounds.Width, mLevel.Bounds.Height);
                var result = ZoomLevels[0];
                foreach (var level in ZoomLevels)
                {
                    if (largest * Dpi.Factor * level <= MaxControlSize)
                        result = level;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the next zoom level in <see cref="ZoomLevels"/> from the current one, larger when
        /// <paramref name="direction"/> is positive, or the current one if there is no other that way.
        /// </summary>
        public float GetNextZoomLevel(int direction) => GetNextZoomLevel(mZoomLevel, direction);

        /// <summary>
        /// Gets the next zoom level in <see cref="ZoomLevels"/> from <paramref name="zoomLevel"/>, larger when
        /// <paramref name="direction"/> is positive, or <paramref name="zoomLevel"/> if there is no other that way.
        /// </summary>
        public float GetNextZoomLevel(float zoomLevel, int direction)
        {
            // Allowing for a zoom level that is not one of the steps
            var max = MaxZoomLevel;
            if (direction > 0)
                return ZoomLevels.Where(x => x > zoomLevel + 0.001f && x <= max).DefaultIfEmpty(zoomLevel).Min();
            return ZoomLevels.Where(x => x < zoomLevel - 0.001f).DefaultIfEmpty(zoomLevel).Max();
        }

        /// <summary>
        /// Gets how much the level is enlarged on screen: the zoom the user has chosen, times the display's DPI scale,
        /// so that at 100% the level is the same physical size as at 96 DPI, but drawn sharply at the display's
        /// resolution rather than stretched by Windows. The editor works in unzoomed coordinates: what is drawn is
        /// scaled up, and mouse positions (given to the tools and in the mouse events) are scaled down.
        /// </summary>
        public float Zoom => Dpi.Factor * mZoomLevel;

        /// <summary>
        /// Converts a distance on screen, in pixels at 96 DPI, to level units at the current zoom, for what is the same
        /// size on screen whatever the zoom: what the editor draws over the level (e.g. the grid), which is not part of
        /// it, and how near the mouse has to be to something.
        /// </summary>
        public float PixelsToUnits(float pixels) => pixels / mZoomLevel;

        /// <summary>
        /// Gets the width, in level units, of a line that is one pixel at 96 DPI whatever the zoom.
        /// </summary>
        public float OverlayLineWidth => PixelsToUnits(1);

        /// <summary>
        /// Gets the size of the editor on screen for the given unzoomed size.
        /// </summary>
        public Size ZoomSize(Size size) => new Size((int)Math.Ceiling(size.Width * Zoom), (int)Math.Ceiling(size.Height * Zoom));

        /// <summary>
        /// Converts an unzoomed point (as given to the tools) to a point on the control.
        /// </summary>
        public Point ZoomPoint(Point p) => new Point((int)Math.Round(p.X * Zoom), (int)Math.Round(p.Y * Zoom));

        private MouseEventArgs Unzoom(MouseEventArgs e)
        {
            if (Zoom == 1)
                return e;
            var location = new Point((int)Math.Floor(e.X / Zoom), (int)Math.Floor(e.Y / Zoom));
            return new MouseEventArgs(e.Button, e.Clicks, location.X, location.Y, e.Delta);
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
                mSelectedTool.MouseMove(MouseButtons.None, _lastMousePosition, Keys.None);
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

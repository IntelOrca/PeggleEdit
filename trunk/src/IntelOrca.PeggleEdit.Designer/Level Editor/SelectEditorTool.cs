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
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Designer
{
	class SelectEditorTool : EditorTool
	{
		Point mSelectionStart;
		Rectangle mSelectionRect;
		List<PointF> mObjectPoints = new List<PointF>();
		LevelEntry mDragObject;
		bool mFirstObjectMovement;
		bool mMovingObjects;
		bool mSelecting;

		public override void MouseDown(MouseButtons button, Point location, Keys modifierKeys)
		{
			Level level = Editor.Level;

			Point vl = Editor.Level.GetVirtualXY(location);

			//Get the pegs at this point
			LevelEntry[] les = level.GetObjectsIn(new RectangleF(vl.X, vl.Y, 1, 1));

			//Get top most object
			LevelEntry top_le = null;
			if (les.Length > 0) {
				top_le = les[les.Length - 1];
			}

			//Select the one thats already selected if possible
			LevelEntry le = null;
			foreach (LevelEntry entry in les) {
				if (Editor.SelectedEntries.Contains(entry)) {
					le = entry;
					break;
				}
			}

			if (top_le != le)
				le = null;

			//Select whatever one is there
			if (le == null && les.Length > 0)
				le = top_le;

			//Did we click on an object
			if (le != null) {
				if (Editor.SelectedEntries.Contains(le)) {

				} else {
					//Unless control is down, clear selection
					if ((modifierKeys & Keys.Control) == 0)
						Editor.SelectedEntries.Clear();

					//Add the new peg to the selection
					Editor.SelectedEntries.Add(le);
				}

				if (button == MouseButtons.Left) {
					//Set the selection start to here
					mSelectionStart = location;

					//Store all the original poisitons of the objects
					mObjectPoints.Clear();
					for (int i = 0; i < Editor.SelectedEntries.Count; i++) {
						LevelEntry objs = Editor.SelectedEntries[i];
						mObjectPoints.Add(new PointF(objs.X, objs.Y));
					}

					//We are moving the pegs
					mDragObject = le;
					mMovingObjects = true;
					mFirstObjectMovement = true;
				}
			} else {
				Editor.SelectedEntries.Clear();

				//Start selection rectangle
				mSelectionStart = location;
				mSelecting = true;
			}

			Editor.UpdateRedraw();

			Editor.CheckSelectionChanged();
		}


		public override void MouseMove(MouseButtons button, Point location, Keys modifierKeys)
		{
			if (button != MouseButtons.Left)
				return;

			if (mSelecting) {
				if (location.X < mSelectionStart.X) {
					mSelectionRect.X = location.X;
					mSelectionRect.Width = mSelectionStart.X - location.X;
				} else {
					mSelectionRect.X = mSelectionStart.X;
					mSelectionRect.Width = location.X - mSelectionStart.X;
				}

				if (location.Y < mSelectionStart.Y) {
					mSelectionRect.Y = location.Y;
					mSelectionRect.Height = mSelectionStart.Y - location.Y;
				} else {
					mSelectionRect.Y = mSelectionStart.Y;
					mSelectionRect.Height = location.Y - mSelectionStart.Y;
				}

				Editor.SelectedEntries.Clear();

				Point vl = Editor.Level.GetVirtualXY(mSelectionRect.Location);

				Editor.SelectedEntries.AddRange(Editor.Level.GetObjectsIn(new RectangleF(
					 vl.X, vl.Y,
					 mSelectionRect.Width, mSelectionRect.Height)));

				Editor.UpdateRedraw();
				Editor.CheckSelectionChanged();
			} else if (mMovingObjects) {
				//Create undo point if first call
				if (mFirstObjectMovement) {
					Editor.CreateUndoPoint();
					mFirstObjectMovement = false;
				}

				//Calculate the delta by how much the mouse has moved
				int dx = location.X - mSelectionStart.X;
				int dy = location.Y - mSelectionStart.Y;

				MoveSelectingObjectsBy(dx, dy, true);

				Editor.UpdateRedraw();
			}
		}

		private void MoveSelectingObjectsBy(int deltaX, int deltaY, bool snap)
		{
			for (int i = 0; i < Editor.SelectedEntries.Count; i++) {
				LevelEntry obj = Editor.SelectedEntries[i];

				//Calculate the new x and y with the delta
				float newX = mObjectPoints[i].X + deltaX;
				float newY = mObjectPoints[i].Y + deltaY;

				//Snap if it is the dragged object
				if (snap && (Settings.SnapToGrid & Settings.ShowGrid) && obj == mDragObject) {
					float snapX = Editor.SnapToGrid(newX);
					float snapY = Editor.SnapToGrid(newY);

					//Check if there was a snap
					if (snapX != newX || snapY != newY) {
						//Calculate delta
						deltaX = (int)(snapX - mObjectPoints[i].X);
						deltaY = (int)(snapY - mObjectPoints[i].Y);

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

		public override void MouseUp(MouseButtons button, Point location, Keys modifierKeys)
		{
			if (button == MouseButtons.Right) {
				ShowContextMenu(location);
				return;
			}

			mMovingObjects = false;
			mSelecting = false;
			mSelectionRect = Rectangle.Empty;

			Editor.UpdateRedraw();
			Editor.InvokeSelectionChangedEvent();
		}

		public override void Draw(Graphics g)
		{
			Rectangle rect;

			//Draw the mouse selection
			if (mSelecting) {
				rect = mSelectionRect;
				g.FillRectangle(new SolidBrush(Color.FromArgb(40, 255, 255, 255)), rect);
				g.DrawRectangle(Pens.CornflowerBlue, rect);

				rect.Inflate(-1, -1);
				g.DrawRectangle(Pens.SkyBlue, rect);
			}

			//Draw the individual peg selection rectangles
			List<LevelEntry> objs = Editor.GetSelectedObjects();

			//Set the pen
			Pen selectedPegPen = new Pen(Brushes.White, 1.0f);
			selectedPegPen.DashPattern = new float[] { 1, 1 };

			rect = new Rectangle();

			foreach (LevelEntry o in objs) {
				Point p = Editor.Level.GetActualXY(o.Bounds.Location);

				rect = Rectangle.Round(o.Bounds);
				rect.Location = p;
				//rect.X = p.X - (rect.Width / 2);
				//rect.Y = p.Y - (rect.Height / 2);
				rect.Inflate(5, 5);

				g.DrawRectangle(selectedPegPen, rect);
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

			if (Editor.SelectedEntries.Count > 0) {
				menu.Items.Add(new ToolStripMenuItem("Bring to Front", Resources.bring_to_front_16, new EventHandler(mnuBringToFront)));
				menu.Items.Add(new ToolStripMenuItem("Send to Back", Resources.send_to_back_16, new EventHandler(mnuSendToBack)));
				menu.Items.Add(new ToolStripSeparator());
			}

			if (Editor.SelectedEntries.Count > 0) {
				menu.Items.Add(new ToolStripMenuItem("Cut", Resources.cut_16, new EventHandler(mnuCut)));
				menu.Items.Add(new ToolStripMenuItem("Copy", Resources.copy_16, new EventHandler(mnuCopy)));
			}
			menu.Items.Add(new ToolStripMenuItem("Paste", Resources.paste_16, new EventHandler(mnuPaste)));
			if (Editor.SelectedEntries.Count > 0) {
				menu.Items.Add(new ToolStripMenuItem("Delete", Resources.delete_16, new EventHandler(mnuDelete)));
			}

			menu.Show(Editor, location);
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
	}
}

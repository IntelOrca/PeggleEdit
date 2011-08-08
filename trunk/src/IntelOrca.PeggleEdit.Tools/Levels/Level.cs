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
using System.Drawing.Drawing2D;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Properties;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
	/// <summary>
	/// Represents a level containing level entries and methods to draw the level.
	/// </summary>
	public class Level
	{
		public const int DrawAdjustX = 73;
		public const int DrawAdjustY = 43;

		public static Color ShadowColour = Color.FromArgb(128, Color.Black);
		public static Point ShadowOffset = new Point(-4, 3);

		static Image mInterface = Resources._interface;
		static Image mInterfaceCollision = Resources.interface_collision;

		LevelInfo mInfo = LevelInfo.DefaultInfo;
		Image mBackground;
		LevelEntryCollection mEntries = new LevelEntryCollection();

		private bool mShowingBackground = true;
		private bool mShowingInterface = true;
		private bool mShowingEntries = true;
		private bool mShowPreview = false;
		private bool mShowCollision = false;
		private bool mShowAnchorsAlways = false;

		private int mCanvasMarginWidth = 500;
		private int mCanvasMarginHeight = 500;

		private float mScale = 1.0f;

		//private Bitmap mCanvasBufferImage;
		//private Graphics mCanvasBufferGraphics;

		public Level()
		{
			//mCanvasBufferImage = new Bitmap(Bounds.Width, Bounds.Height);
			//mCanvasBufferGraphics = Graphics.FromImage(mCanvasBufferImage);
		}

		#region Painting

		public void Draw(Graphics g)
		{
			g.FillRectangle(SystemBrushes.AppWorkspace, Bounds);

			g.DrawRectangle(Pens.Black, mCanvasMarginWidth - 1, mCanvasMarginHeight - 1, 800 + 1, 600 + 1);

			g.TranslateTransform(mCanvasMarginWidth, mCanvasMarginHeight);
			GraphicsContainer containerState = g.BeginContainer();
			g.SmoothingMode = SmoothingMode.HighQuality;

			DrawCanvas(g);

			g.SmoothingMode = SmoothingMode.Default;
			g.EndContainer(containerState);
			g.TranslateTransform(-mCanvasMarginWidth, -mCanvasMarginHeight);


			//mCanvasBufferGraphics.FillRectangle(SystemBrushes.AppWorkspace, Bounds);

			//mCanvasBufferGraphics.DrawRectangle(Pens.Black, mCanvasMarginWidth - 1, mCanvasMarginHeight - 1, 800 + 1, 600 + 1);

			//DrawCanvas(mCanvasBufferGraphics);


			//Rectangle src = Bounds;
			//Rectangle dest = new Rectangle(0, 0, (int)(Bounds.Width * mScale), (int)(Bounds.Height * mScale));
			//dest.X = (Bounds.Width / 2) - (dest.Width / 2);
			//dest.Y = (Bounds.Height / 2) - (dest.Height / 2);

			//g.DrawImage(mCanvasBufferImage, dest, src, GraphicsUnit.Pixel);
		}

		private void DrawCanvas(Graphics g)
		{
			//Level bounds
			Rectangle bounds = new Rectangle(0, 0, 800, 600);

			//Black background
			g.FillRectangle(Brushes.Black, bounds);

			//Gray background
			//HatchBrush brush = new HatchBrush(HatchStyle.Percent50, Color.Gray);
			//g.FillRectangle(brush, 0, 0, 800, 600);

			//Set the drawing quality to high
			g.SmoothingMode = SmoothingMode.HighQuality;

			//Background
			if (mShowingBackground)
				DrawBackground(g);

			if (mShowingEntries)
				DrawEntries(g);

			if (mShowingInterface)
				DrawInterface(g);
		}

		private void DrawBackground(Graphics g)
		{
			if (mBackground == null)
				return;

			if (mBackground.Size == new Size(646, 543)) {
				g.DrawImage(mBackground, 73, 53, mBackground.Width, mBackground.Height);
			} else {
				Rectangle rect = new Rectangle(400 - (mBackground.Width / 2), 300 - (mBackground.Height / 2), mBackground.Width, mBackground.Height);
				g.DrawImage(mBackground, rect);
			}
		}

		private void DrawEntries(Graphics g)
		{
			g.TranslateTransform(Level.DrawAdjustX, Level.DrawAdjustY);
			GraphicsContainer containerState = g.BeginContainer();
			g.SmoothingMode = SmoothingMode.HighQuality;

			List<int> hidetype = new List<int>();
			//hidetype.AddRange(new int[] { 5, 6 });	

			if (mShowingEntries) {
				foreach (LevelEntry le in mEntries) {
					if (hidetype.Contains(le.Type))
						continue;

					if (le.HasShadow)
						le.DrawShadow(g);
				}

				foreach (LevelEntry le in mEntries) {
					if (hidetype.Contains(le.Type))
						continue;

					le.Draw(g);
				}
			}

			g.EndContainer(containerState);
			g.TranslateTransform(-Level.DrawAdjustX, -Level.DrawAdjustY);
		}

		private void DrawInterface(Graphics g)
		{
			if (mShowCollision)
				g.DrawImage(mInterfaceCollision, 0, 0, 800, 600);
			else
				g.DrawImage(mInterface, 0, 0, 800, 600);
		}

		public void DrawAnchorPoint(Graphics g, float x, float y)
		{
			if (mShowPreview)
				return;

			Rectangle src = new Rectangle(0, 0, Resources.anchor_point.Width, Resources.anchor_point.Height);
			Rectangle dst = new Rectangle((int)(x - (Resources.anchor_point.Width / 2)), (int)(y - (Resources.anchor_point.Height / 2)), Resources.anchor_point.Width, Resources.anchor_point.Height);
			g.DrawImage(Resources.anchor_point, dst, src, GraphicsUnit.Pixel);
		}

		public Image GetThumbnail()
		{
			bool oInterface = mShowingInterface;
			bool oAllPegsBlue = mShowPreview;

			mShowingInterface = false;
			mShowPreview = true;

			Bitmap fs = new Bitmap(800, 600);
			Graphics fsG = Graphics.FromImage(fs);
			DrawCanvas(fsG);
			fsG.Dispose();

			mShowingInterface = oInterface;
			mShowPreview = oAllPegsBlue;

			Rectangle sRect = new Rectangle(100, 64, 600, 531);
			Size tbSize = new Size(200, 177);
			Bitmap tb = new Bitmap(tbSize.Width, tbSize.Height);
			Graphics tbG = Graphics.FromImage(tb);
			tbG.InterpolationMode = InterpolationMode.HighQualityBicubic;
			tbG.DrawImage(fs, new Rectangle(Point.Empty, tb.Size), sRect, GraphicsUnit.Pixel);
			tbG.Dispose();

			fs.Dispose();
			return tb;
		}

		#endregion

		#region Helper Functions

		public int GetISX()
		{
			return (Bounds.Width / 2) - ((int)(Bounds.Width * mScale) / 2);
		}

		public int GetISY()
		{
			return (Bounds.Height / 2) - ((int)(Bounds.Height * mScale) / 2);
		}

		public Point GetVirtualXY(int x, int y)
		{
			return GetVirtualXY(new Point(x, y));
		}

		public Point GetVirtualXY(Point p)
		{
			float x = p.X - GetISX() - ((mCanvasMarginWidth + Level.DrawAdjustX) * mScale);
			float y = p.Y - GetISY() - ((mCanvasMarginHeight + Level.DrawAdjustY) * mScale);

			return new Point((int)x, (int)y);
		}

		public Point GetActualXY(int x, int y)
		{
			return GetActualXY(new Point(x, y));
		}

		public Point GetActualXY(float x, float y)
		{
			return GetActualXY((int)Math.Round(x), (int)Math.Round(y));
		}

		public Point GetActualXY(PointF p)
		{
			return GetActualXY(Point.Round(p));
		}

		public Point GetActualXY(Point p)
		{
			float x = p.X + GetISX() + ((mCanvasMarginWidth + Level.DrawAdjustX) * mScale);
			float y = p.Y + GetISY() + ((mCanvasMarginHeight + Level.DrawAdjustY) * mScale);

			return new Point((int)x, (int)y);
		}

		public LevelEntry GetObjectAt(float x, float y)
		{
			foreach (LevelEntry le in mEntries) {
				RectangleF pBounds = le.Bounds;
				if (pBounds.Contains(x, y)) {
					return le;
				}
			}

			return null;
		}

		public LevelEntry[] GetObjectsIn(RectangleF rect)
		{
			LevelEntryCollection entries = new LevelEntryCollection();
			foreach (LevelEntry le in mEntries) {
				RectangleF pBounds = le.Bounds;
				if (pBounds.IntersectsWith(rect)) {
					entries.Add(le);
				}
			}

			return entries.ToArray();
		}

		public bool IsObjectIn(RectangleF rect)
		{
			foreach (LevelEntry le in mEntries) {
				RectangleF pBounds = le.Bounds;
				if (pBounds.IntersectsWith(rect)) {
					return true;
				}
			}

			return false;
		}

		public LevelEntry GetPegAt(float x, float y)
		{
			foreach (LevelEntry le in mEntries) {
				if (le.HasPegInfo) {
					RectangleF pBounds = new RectangleF(le.X - 10, le.Y - 10, 20, 20);
					if (pBounds.Contains(x, y)) {
						return le;
					}
				} else if (le is Brick) {
					Brick b = (Brick)le;
					RectangleF bBounds = new RectangleF(b.X - (b.Width / 2), b.Y - (b.Width / 2), b.Width, b.Width);
					if (bBounds.Contains(x, y)) {
						return b;
					}
				}
			}

			return null;
		}

		public LevelEntry[] GetPegsIn(RectangleF rect)
		{
			LevelEntryCollection entries = new LevelEntryCollection();
			foreach (LevelEntry le in mEntries) {
				if (le.HasPegInfo) {
					RectangleF pBounds = new RectangleF(le.X - 10, le.Y - 10, 20, 20);
					if (pBounds.IntersectsWith(rect)) {
						entries.Add(le);
					}
				} else if (le is Brick) {
					Brick b = (Brick)le;
					RectangleF bBounds = new RectangleF(b.X - (b.Width / 2), b.Y - (b.Width / 2), b.Width, b.Width);
					if (bBounds.IntersectsWith(rect)) {
						entries.Add(b);
					}
				}
			}

			return entries.ToArray();
		}

		public bool IsPegIn(RectangleF rect)
		{
			foreach (LevelEntry le in mEntries) {
				if (le.HasPegInfo) {
					RectangleF pBounds = new RectangleF(le.X - 10, le.Y - 10, 20, 20);
					if (pBounds.IntersectsWith(rect)) {
						return true;
					}
				} else if (le is Brick) {
					Brick b = (Brick)le;
					RectangleF bBounds = new RectangleF(b.X - (b.Width / 2), b.Y - (b.Width / 2), b.Width, b.Width);
					if (bBounds.IntersectsWith(rect)) {
						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region Previewing

		private ulong mPreviewTime;
		private int mFirstPreviewUpdate;

		public void ResetPreview()
		{
			mFirstPreviewUpdate = Environment.TickCount;
			mPreviewTime = 0;
		}

		public void UpdatePreview()
		{
			if (mFirstPreviewUpdate != 0) {
				mPreviewTime = (ulong)((Environment.TickCount - mFirstPreviewUpdate) / 10);
			}
		}

		public void UpdatePreview(int elapsedTime)
		{
			mPreviewTime += (ulong)(elapsedTime / 10);
		}

		#endregion

		#region Properties

		public Rectangle Bounds
		{
			get
			{
				return new Rectangle(0, 0, (mCanvasMarginWidth * 2) + 800, (mCanvasMarginHeight * 2) + 600);
			}
		}

		public Rectangle CanvasBounds
		{
			get
			{
				return new Rectangle(mCanvasMarginWidth, mCanvasMarginHeight, 800, 600);
			}
		}

		public LevelEntryCollection Entries
		{
			get
			{
				return mEntries;
			}
		}

		public LevelInfo Info
		{
			get
			{
				return mInfo;
			}
			set
			{
				mInfo = value;
			}
		}

		public Image Background
		{
			get
			{
				return mBackground;
			}
			set
			{
				mBackground = value;
			}
		}

		public bool ShowingBackground
		{
			get
			{
				return mShowingBackground;
			}
			set
			{
				mShowingBackground = value;
			}
		}

		public bool ShowingInterface
		{
			get
			{
				return mShowingInterface;
			}
			set
			{
				mShowingInterface = value;
			}
		}
		public bool ShowingPegs
		{
			get
			{
				return mShowingEntries;
			}
			set
			{
				mShowingEntries = value;
			}
		}

		public bool ShowPreview
		{
			get
			{
				return mShowPreview;
			}
			set
			{
				mShowPreview = value;
			}
		}

		public ulong PreviewTime
		{
			get
			{
				return mPreviewTime;
			}
		}

		public bool ShowCollision
		{
			get
			{
				return mShowCollision;
			}
			set
			{
				mShowCollision = value;
			}
		}

		public bool ShowAnchorsAlways
		{
			get
			{
				return mShowAnchorsAlways;
			}
			set
			{
				mShowAnchorsAlways = value;
			}
		}

		#endregion
	}
}

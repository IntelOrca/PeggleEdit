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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
	/// <summary>
	/// Represents the brick generator level entry with function capabilities.
	/// </summary>
	public class BrickGenerator : LevelEntry, ICloneable, IEntryFunction
	{
		int mNumberOfBricks;
		int mMaxNumberOfBricks;
		float mInnerRadius;
		float mAngularOffset;
		float mBrickWidth;

		public BrickGenerator(Level level)
			: base(level)
		{
			mMaxNumberOfBricks = 18;
			mNumberOfBricks = 18;

			mInnerRadius = 90.0f;
			mAngularOffset = 0.0f;
			mBrickWidth = 20.0f;
		}

		public void Execute()
		{
			int index = 0;
			for (float a = mAngularOffset; a < 360 + mAngularOffset; a += SectorAngles) {
				float angle = MathExt.ToRadians(a);

				Brick b = new Brick(Level);
				b.X = X + ((float)Math.Cos(angle) * BrickRadius);
				b.Y = Y + ((float)Math.Sin(angle) * BrickRadius);
				b.SectorAngle = SectorAngles;
				b.Width = mBrickWidth;
				b.Length = mInnerRadius;
				b.Rotation = -a;
				b.Curved = true;

				Level.Entries.Add(b);

				index++;
				if (index == mNumberOfBricks)
					break;
			}

			Level.Entries.Remove(this);
		}

		public override void ReadData(BinaryReader br, int version)
		{
			byte hl = br.ReadByte();
			if (hl > 0) {
				X = br.ReadSingle();
				Y = br.ReadSingle();
			}

			mNumberOfBricks = br.ReadInt32();
			mMaxNumberOfBricks = br.ReadInt32();
			mInnerRadius = br.ReadSingle();
			mAngularOffset = br.ReadSingle();
			mBrickWidth = br.ReadSingle();
		}

		public override void WriteData(BinaryWriter bw, int version)
		{
			if (!HasMovementInfo) {
				bw.Write((byte)1);
				bw.Write(X);
				bw.Write(Y);
			} else {
				bw.Write((byte)0);
			}

			bw.Write(mNumberOfBricks);
			bw.Write(mMaxNumberOfBricks);
			bw.Write(mInnerRadius);
			bw.Write(mAngularOffset);
			bw.Write(mBrickWidth);
		}

		public override void Draw(Graphics g)
		{
			//Don't show in show collision mode
			if (Level.ShowCollision)
				return;

			base.Draw(g);

			PointF location = DrawLocation;

			Pen brickPen = new Pen(Color.FromArgb(128, Color.Orange), mBrickWidth);
			Pen circlePen = new Pen(Color.Black);

			circlePen.DashStyle = DashStyle.Custom;
			circlePen.DashPattern = new float[] { 2, 4 };

			g.DrawArc(brickPen, MidBounds, mAngularOffset - (SectorAngles / 2.0f), mNumberOfBricks * SectorAngles);

			g.DrawEllipse(circlePen, Bounds);
			g.DrawEllipse(circlePen, InnerBounds);

			int index = 0;
			for (float a = mAngularOffset; a < 360 + mAngularOffset; a += SectorAngles) {
				for (float hs = -0.5f; hs <= 0.5f; hs += 1.0f) {
					float angle = MathExt.ToRadians(a + (hs * SectorAngles));
					float ix = location.X + ((float)Math.Cos(angle) * InnerRadius);
					float iy = location.Y + ((float)Math.Sin(angle) * InnerRadius);
					float ox = location.X + ((float)Math.Cos(angle) * OuterRadius);
					float oy = location.Y + ((float)Math.Sin(angle) * OuterRadius);
					g.DrawLine(circlePen, ix, iy, ox, oy);
				}

				index++;
				if (index == mNumberOfBricks)
					break;
			}
		}

		public override object Clone()
		{
			BrickGenerator cpyBG = new BrickGenerator(Level);
			base.CloneTo(cpyBG);

			cpyBG.mNumberOfBricks = mNumberOfBricks;
			cpyBG.mMaxNumberOfBricks = mMaxNumberOfBricks;
			cpyBG.mInnerRadius = mInnerRadius;
			cpyBG.mAngularOffset = mAngularOffset;

			cpyBG.mBrickWidth = mBrickWidth;

			return cpyBG;
		}

		[DisplayName("Max Number of Bricks")]
		[Description("The number of bricks that can the circle is divided up into.")]
		[Category("Bricks")]
		[DefaultValue(18)]
		public int MaxNumberOfBricks
		{
			get
			{
				return mMaxNumberOfBricks;
			}
			set
			{
				mMaxNumberOfBricks = value;
				mNumberOfBricks = value;
			}
		}

		[DisplayName("Number of Bricks")]
		[Description("The number of bricks that will be generated.")]
		[Category("Bricks")]
		[DefaultValue(18)]
		public int NumberOfBricks
		{
			get
			{
				return mNumberOfBricks;
			}
			set
			{
				mNumberOfBricks = value;
			}
		}

		[DisplayName("Inner Radius")]
		[Description("The radius from the centre of the circle to the inside edge of the bricks.")]
		[Category("Bricks")]
		[DefaultValue(90.0f)]
		public float InnerRadius
		{
			get
			{
				return mInnerRadius;
			}
			set
			{
				mInnerRadius = value;
			}
		}

		[DisplayName("Mid Radius")]
		[Description("The radius from the centre of the circle to the centre of the bricks.")]
		[Category("Bricks")]
		[DefaultValue(110.0f)]
		public float BrickRadius
		{
			get
			{
				return mInnerRadius + (mBrickWidth / 2.0f);
			}
			set
			{
				mInnerRadius = value - (mBrickWidth / 2.0f);
			}
		}

		[DisplayName("Outer Radius")]
		[Description("The radius from the centre of the circle to the outside edge of the bricks.")]
		[Category("Bricks")]
		[DefaultValue(110.0f)]
		public float OuterRadius
		{
			get
			{
				return mInnerRadius + mBrickWidth;
			}
			set
			{
				mInnerRadius = value - mBrickWidth;
			}
		}

		[DisplayName("Angle of Sectors")]
		[Description("The angle of each brick sector in degrees.")]
		[Category("Bricks")]
		[DefaultValue(110.0f)]
		public float SectorAngles
		{
			get
			{
				return 360.0f / mMaxNumberOfBricks;
			}
			set
			{
				mMaxNumberOfBricks = (int)Math.Round(360.0f / value);
				mNumberOfBricks = mMaxNumberOfBricks;
			}
		}

		[DisplayName("Angular Offset")]
		[Description("The angular offset to where the bricks start in degrees.")]
		[Category("Bricks")]
		[DefaultValue(0.0f)]
		public float AngularOffset
		{
			get
			{
				return mAngularOffset;
			}
			set
			{
				mAngularOffset = value;
			}
		}

		[DisplayName("Brick Width")]
		[Description("The width of the bricks.")]
		[Category("Bricks")]
		[DefaultValue(20.0f)]
		public float BrickWidth
		{
			get
			{
				return mBrickWidth;
			}
			set
			{
				mBrickWidth = value;
			}
		}

		[Browsable(false)]
		public RectangleF MidBounds
		{
			get
			{
				return new RectangleF(X - BrickRadius, Y - BrickRadius, BrickRadius * 2, BrickRadius * 2);
			}
		}

		[Browsable(false)]
		public RectangleF InnerBounds
		{
			get
			{
				return new RectangleF(X - InnerRadius, Y - InnerRadius, InnerRadius * 2, InnerRadius * 2);
			}
		}

		public override int Type
		{
			get
			{
				return 1002;
			}
		}

		public override RectangleF Bounds
		{
			get
			{
				return new RectangleF(DrawX - OuterRadius, DrawY - OuterRadius, OuterRadius * 2, OuterRadius * 2);
			}
		}
	}
}

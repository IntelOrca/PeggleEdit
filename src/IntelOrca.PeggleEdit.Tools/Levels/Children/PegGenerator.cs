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
	/// Represents the peg generator level entry which has function capabilities.
	/// </summary>
	public class PegGenerator : LevelEntry, ICloneable, IEntryFunction
	{
		int mNumberOfPegs;
		int mMaxNumberOfPegs;
		float mRadiusX;
		float mRadiusY;
		float mAngularOffset;

		public PegGenerator(Level level)
			: base(level)
		{
			mMaxNumberOfPegs = 9;
			mNumberOfPegs = 9;

			mRadiusX = 90.0f;
			mRadiusY = 90.0f;
		}

		public void Execute()
		{
			int index = 0;
			float da = 360.0f / (float)mMaxNumberOfPegs;
			for (float a = mAngularOffset; a < 360 + mAngularOffset; a += da) {
				float angle = MathExt.ToRadians(a);

				Circle p = new Circle(Level);
				p.PegInfo = new PegInfo(p, true, false);
				p.X = X + ((float)Math.Cos(angle) * mRadiusX);
				p.Y = Y + ((float)Math.Sin(angle) * mRadiusY);

				Level.Entries.Add(p);

				index++;
				if (index == mNumberOfPegs)
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

			mNumberOfPegs = br.ReadInt32();
			mMaxNumberOfPegs = br.ReadInt32();
			mRadiusX = br.ReadSingle();
			mRadiusY = br.ReadSingle();
			mAngularOffset = br.ReadSingle();
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

			bw.Write(mNumberOfPegs);
			bw.Write(mMaxNumberOfPegs);
			bw.Write(mRadiusX);
			bw.Write(mRadiusY);
			bw.Write(mAngularOffset);
		}

		public override void Draw(Graphics g)
		{
			if (Level.ShowCollision)
				return;

			base.Draw(g);

			PointF location = DrawLocation;

			Brush pegBrush = new SolidBrush(Color.FromArgb(128, Color.Orange));
			Pen circlePen = new Pen(Color.Black);

			circlePen.DashStyle = DashStyle.Custom;
			circlePen.DashPattern = new float[] { 2, 4 };

			g.DrawEllipse(circlePen, Bounds);

			int index = 0;
			float da = 360.0f / (float)mMaxNumberOfPegs;
			for (float a = mAngularOffset; a < 360 + mAngularOffset; a += da) {
				float angle = MathExt.ToRadians(a);
				float x = location.X + ((float)Math.Cos(angle) * mRadiusX);
				float y = location.Y + ((float)Math.Sin(angle) * mRadiusY);

				g.FillEllipse(pegBrush, x - 10.0f, y - 10.0f, 20.0f, 20.0f);
				g.DrawEllipse(circlePen, x - 10.0f, y - 10.0f, 20.0f, 20.0f);

				index++;
				if (index == mNumberOfPegs)
					break;
			}
		}

		public override object Clone()
		{
			PegGenerator cpyPG = new PegGenerator(Level);
			base.CloneTo(cpyPG);

			cpyPG.mNumberOfPegs = mNumberOfPegs;
			cpyPG.mMaxNumberOfPegs = mMaxNumberOfPegs;
			cpyPG.mRadiusX = mRadiusX;
			cpyPG.mRadiusY = mRadiusY;
			cpyPG.mAngularOffset = mAngularOffset;

			return cpyPG;
		}

		[DisplayName("Max Number of Pegs")]
		[Description("The number of pegs that can the circle is divided up into.")]
		[Category("Pegs")]
		[DefaultValue(18)]
		public int MaxNumberOfPegs
		{
			get
			{
				return mMaxNumberOfPegs;
			}
			set
			{
				mMaxNumberOfPegs = value;
				mNumberOfPegs = value;
			}
		}

		[DisplayName("Number of Pegs")]
		[Description("The number of Pegs that will be generated.")]
		[Category("Pegs")]
		[DefaultValue(18)]
		public int NumberOfPegs
		{
			get
			{
				return mNumberOfPegs;
			}
			set
			{
				mNumberOfPegs = value;
			}
		}

		[DisplayName("Horizontal Radius")]
		[Description("The horizontal radius of the circle of pegs.")]
		[Category("Pegs")]
		[DefaultValue(90.0f)]
		public float RadiusX
		{
			get
			{
				return mRadiusX;
			}
			set
			{
				mRadiusX = value;
			}
		}

		[DisplayName("Vetical Radius")]
		[Description("The vertical radius of the circle of pegs.")]
		[Category("Pegs")]
		[DefaultValue(90.0f)]
		public float RadiusY
		{
			get
			{
				return mRadiusY;
			}
			set
			{
				mRadiusY = value;
			}
		}

		[DisplayName("Angular Offset")]
		[Description("The angular offset to where the pegs start in degrees.")]
		[Category("Pegs")]
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

		public override int Type
		{
			get
			{
				return 1001;
			}
		}

		public override RectangleF Bounds
		{
			get
			{
				return new RectangleF(DrawX - mRadiusX, DrawY - mRadiusY, mRadiusX * 2, mRadiusY * 2);
			}
		}
	}
}

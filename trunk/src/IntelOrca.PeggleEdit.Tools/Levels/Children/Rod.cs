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
using System.IO;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
	/// <summary>
	/// Represents the rod level entry.
	/// </summary>
	public class Rod : LevelEntry
	{
		PointF mPointA;
		PointF mPointB;

		public Rod(Level level)
			: base(level)
		{

		}

		public override void ReadData(BinaryReader br, int version)
		{
			FlagGroup fA = new FlagGroup(br.ReadByte());

			mPointA = new PointF(br.ReadSingle(), br.ReadSingle());
			mPointB = new PointF(br.ReadSingle(), br.ReadSingle());

			if (fA[0]) {
				float e = br.ReadSingle();
			}

			if (fA[1]) {
				float f = br.ReadSingle();
			}
		}

		public override void WriteData(BinaryWriter bw, int version)
		{
			FlagGroup fA = new FlagGroup();
			bw.Write(fA.Int8);

			bw.Write(mPointA.X);
			bw.Write(mPointA.Y);
			bw.Write(mPointB.X);
			bw.Write(mPointB.Y);
		}

		public override void Draw(Graphics g)
		{
			if (Level.ShowCollision && !Collision)
				return;

			Pen p = new Pen(Color.White, 2);
			g.DrawLine(p, mPointA, mPointB);
		}

		public override object Clone()
		{
			Rod cpyRod = new Rod(Level);
			cpyRod.mPointA = mPointA;
			cpyRod.mPointB = mPointB;

			return cpyRod;
		}

		[DisplayName("Point A, X")]
		[Description("The X co-ordinate of point A.")]
		[Category("Layout")]
		public float PointAX
		{
			get
			{
				return mPointA.X;
			}
			set
			{
				mPointA.X = value;
			}
		}

		[DisplayName("Point A, Y")]
		[Description("The Y co-ordinate of point A.")]
		[Category("Layout")]
		public float PointAY
		{
			get
			{
				return mPointA.Y;
			}
			set
			{
				mPointA.Y = value;
			}
		}

		[DisplayName("Point B, X")]
		[Description("The X co-ordinate of point B.")]
		[Category("Layout")]
		public float PointBX
		{
			get
			{
				return mPointB.X;
			}
			set
			{
				mPointB.X = value;
			}
		}

		[DisplayName("Point B, Y")]
		[Description("The Y co-ordinate of point B.")]
		[Category("Layout")]
		public float PointBY
		{
			get
			{
				return mPointB.Y;
			}
			set
			{
				mPointB.Y = value;
			}
		}

		[Browsable(false)]
		public PointF PointA
		{
			get
			{
				return mPointA;
			}
			set
			{
				mPointA = value;
			}
		}

		[Browsable(false)]
		public PointF PointB
		{
			get
			{
				return mPointB;
			}
			set
			{
				mPointB = value;
			}
		}

		public override float X
		{
			get
			{
				return mPointA.X + ((mPointB.X - mPointA.X) / 2);
			}
			set
			{
				float d = (mPointB.X - mPointA.X) / 2;
				mPointA.X = value - d;
				mPointB.X = value + d;
			}
		}

		public override float Y
		{
			get
			{
				return mPointA.Y + ((mPointB.Y - mPointA.Y) / 2);
			}
			set
			{
				float d = (mPointB.Y - mPointA.Y) / 2;
				mPointA.Y = value - d;
				mPointB.Y = value + d;
			}
		}

		public override RectangleF Bounds
		{
			get
			{
				float minX = Math.Min(mPointA.X, mPointB.X);
				float minY = Math.Min(mPointA.Y, mPointB.Y);
				float maxX = Math.Max(mPointA.X, mPointB.X);
				float maxY = Math.Max(mPointA.Y, mPointB.Y);
				RectangleF b = new RectangleF(minX, minY, maxX - minX, maxY - minY);
				b.Inflate(2.0f, 2.0f);
				return b;
			}
		}

		public override int Type
		{
			get
			{
				return 2;
			}
		}
	}
}

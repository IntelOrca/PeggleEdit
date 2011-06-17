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
	/// Represents the brick level entry.
	/// </summary>
	public class Brick : LevelEntry, ICloneable
	{
		private byte mType;
		private float mSectorAngle;
		private float mWidth;
		private float mLength;
		private float mAngle;

		private int mCurvePoints;
		private bool mCurved;
		private bool mTextureFlip;

		public Brick(Level level)
			: base(level)
		{
			PegInfo = new PegInfo(this, true, false);

			mWidth = 20.0f;
			mLength = 30.0f;
			mCurvePoints = 4;
			mCurved = false;
		}

		public override void ReadData(BinaryReader br, int version)
		{
			FlagGroup fA = new FlagGroup(br.ReadByte());
			FlagGroup fB = new FlagGroup();
			if (version >= 0x23)
				fB = new FlagGroup(br.ReadByte());
			if (fA[2])
				br.ReadSingle();
			if (fA[3])
				br.ReadSingle();
			if (fA[5])
				br.ReadSingle();
			if (fA[1])
				br.ReadByte();
			if (fA[4]) {
				X = br.ReadSingle();
				Y = br.ReadSingle();
			}

			//if arg0 (lots)

			if (fB[0])
				br.ReadByte();
			if (fB[1])
				br.ReadInt32();
			if (fB[2])
				br.ReadInt16();

			mCurved = true;

			FlagGroup fC = new FlagGroup(br.ReadInt16());
			if (fC[8])
				br.ReadSingle();
			if (fC[9])
				br.ReadSingle();
			if (fC[2]) {
				mType = br.ReadByte();
				if (mType == 5)
					mCurved = false;
			}
			if (fC[3]) {
				mCurvePoints = br.ReadByte() + 2;
			}
			if (fC[5])
				br.ReadSingle();	//Left angle
			if (fC[6])
				br.ReadSingle();	//Right angle

			//if arg0+3		ReadSingle		ReadSingle

			if (fC[4])
				mSectorAngle = br.ReadSingle();
			if (fC[7])
				mWidth = br.ReadSingle();

			mTextureFlip = fC[10];

			mLength = br.ReadSingle();
			mAngle = br.ReadSingle();

			byte[] hmm = br.ReadBytes(4);

			//for (int i = 0; i < 4; i++)
			//	br.ReadByte();
		}

		public override void WriteData(BinaryWriter bw, int version)
		{
			FlagGroup fA = new FlagGroup();
			FlagGroup fB = new FlagGroup();
			FlagGroup fC = new FlagGroup();

			if (!HasMovementInfo)
				fA[4] = true;

			if (mCurved) {
				fC[4] = true;

				if (mCurvePoints != 4)
					fC[3] = true;

			} else {
				fC[2] = true;
			}

			fC[7] = true;

			if (mTextureFlip)
				fC[10] = true;

			//Write data

			bw.Write(fA.Int8);

			if (version >= 0x23)
				bw.Write(fB.Int8);

			if (fA[4]) {
				bw.Write(X);
				bw.Write(Y);
			}

			bw.Write(fC.Int16);

			if (fC[2]) {
				bw.Write((byte)5);
			}

			if (fC[3]) {
				bw.Write((byte)(mCurvePoints + 2));
			}

			if (fC[4])
				bw.Write(mSectorAngle);
			if (fC[7])
				bw.Write(mWidth);

			bw.Write(mLength);
			bw.Write(mAngle);

			bw.Write(new byte[] { 0, 0, 0, 0 });
		}

		public override void Draw(Graphics g)
		{
			//Show only if its collidable
			if (!HasPegInfo) {
				if (Level.ShowCollision && !Collision)
					return;
			}

			base.Draw(g);

			//Get location and angle
			PointF location = DrawLocation;
			float angle = mAngle;
			if (MovementInfo != null) {
				angle = MovementInfo.GetEstimatedMoveAngle(mAngle);
			}

			//Calculate the brick destination rectangle
			float height = GetHeight();
			if (Curved) {
				//Rotate the matrix
				Matrix mx = new Matrix();
				mx.RotateAt(-angle, location);
				g.Transform = mx;

				DrawCurvedBrick(g, location);

				g.Transform = new Matrix();
			} else {
				//Rotate the matrix
				Matrix mx = new Matrix();
				mx.RotateAt(-angle + 90.0f, new PointF(location.X, location.Y));
				g.Transform = mx;

				DrawStrightBrick(g, location);

				g.Transform = new Matrix();
			}
		}

		private void DrawStrightBrick(Graphics g, PointF location)
		{
			RectangleF dest = new RectangleF(location.X - (Length / 2), location.Y - (Width / 2), Length, Width);
			DrawBrick(g, dest, PegInfo.GetOuterColour());

			dest.Inflate(-2, -5);

			if (mTextureFlip)
				dest.Y += 3.0f;
			else
				dest.Y -= 3.0f;

			DrawBrick(g, dest, PegInfo.GetInnerColour());
		}

		private void DrawCurvedBrick(Graphics g, PointF location)
		{
			if (Level.ShowCollision) {
				DrawCurvedBrick(g, Color.White, location, false);
				return;
			}

			float shadingOffset = 8.0f;
			if (mTextureFlip)
				shadingOffset = 2.0f;

			DrawCurvedBrick(g, PegInfo.GetOuterColour(), location, false);
			mWidth /= 2.0f;
			location.X += shadingOffset;
			DrawCurvedBrick(g, PegInfo.GetInnerColour(), location, true);
			location.X -= shadingOffset;
			mWidth *= 2.0f;
		}

		private void DrawCurvedBrick(Graphics g, Color c, PointF location, bool inner)
		{
			PointF offset = new PointF(-10, 0);

			location.X += offset.X;
			location.Y += offset.Y;

			float hangle = mSectorAngle / 2.0f;
			float hhangle = (mSectorAngle / 2.0f) * (0.4f);
			float width = mWidth;
			float inner_radius = mLength;
			float outer_radius = inner_radius + mWidth;
			PointF circleCentre = new PointF(location.X - inner_radius, location.Y);

			/*
			PointF[] pnts = new PointF[8];

			pnts[0] = GetBrickAngularPoint(circleCentre, -hangle, outer_radius);
			pnts[1] = GetBrickAngularPoint(circleCentre, -hhangle, outer_radius);
			pnts[2] = GetBrickAngularPoint(circleCentre, hhangle, outer_radius);
			pnts[3] = GetBrickAngularPoint(circleCentre, hangle, outer_radius);
			pnts[4] = GetBrickAngularPoint(circleCentre, hangle, radius);
			pnts[5] = GetBrickAngularPoint(circleCentre, hhangle, radius);
			pnts[6] = GetBrickAngularPoint(circleCentre, -hhangle, radius);
			pnts[7] = GetBrickAngularPoint(circleCentre, -hangle, radius);
			*/

			float div_angle = mSectorAngle / (mCurvePoints - 1);
			float cur_angle = -(mSectorAngle / 2.0f);

			PointF[] o_pnts = new PointF[mCurvePoints];
			PointF[] i_pnts = new PointF[mCurvePoints];

			for (int i = 0; i < mCurvePoints; i++) {
				o_pnts[i] = GetBrickAngularPoint(circleCentre, cur_angle, outer_radius);
				i_pnts[i] = GetBrickAngularPoint(circleCentre, cur_angle, inner_radius);

				cur_angle += div_angle;
			}

			PointF[] pnts = new PointF[o_pnts.Length + i_pnts.Length];
			Array.Copy(o_pnts, 0, pnts, 0, o_pnts.Length);

			//Inner points need to be reversed
			for (int i = 0; i < i_pnts.Length; i++) {
				pnts[pnts.Length - i - 1] = i_pnts[i];
			}


			bool mouseOverOnly = true;
			bool drawInnerCircle = true;
			bool drawOuterCircle = true;

			if (!inner) {
				if (!mouseOverOnly || (mouseOverOnly && MouseOver)) {
				if (drawInnerCircle)
					g.DrawEllipse(Pens.Magenta, circleCentre.X - inner_radius, circleCentre.Y - inner_radius, inner_radius * 2, inner_radius * 2);
				if (drawOuterCircle)
					g.DrawEllipse(Pens.White, circleCentre.X - outer_radius, circleCentre.Y - outer_radius, outer_radius * 2, outer_radius * 2);
				}
			}



			//////////////////////////////////////////////////////////////
			/*
			
			Bitmap bmp = new Bitmap(32, 20);
			Graphics gg = Graphics.FromImage(bmp);
			gg.DrawImage(mBrick, new Rectangle(0, 0, 32, 20), new Rectangle(0, 0, 32, 20), GraphicsUnit.Pixel);
			gg.Dispose();
			
			g.SmoothingMode = SmoothingMode.Default;

			//float totalDistance = MathExt.Distance(pnts[0], pnts[1]) + MathExt.Distance(pnts[1], pnts[2]) + MathExt.Distance(pnts[2], pnts[3]);
			//float multiplier = totalDistance / (float)bmp.Width;

			int index = 0;

			List<PointF> lps = new List<PointF>();
			lps.AddRange(MathExt.LinePoints(pnts[0], pnts[1]));
			lps.AddRange(MathExt.LinePoints(pnts[1], pnts[2]));
			lps.AddRange(MathExt.LinePoints(pnts[2], pnts[3]));
			float totalDistance = lps.Count;
			float multiplier = totalDistance / (float)bmp.Width;

			foreach (PointF p in lps) {
				float ox = index / multiplier;
				Color c = bmp.GetPixel((int)ox, 0);

				g.FillRectangle(new SolidBrush(c), p.X, p.Y, 1, 1);
				index++;
			}


			g.SmoothingMode = SmoothingMode.HighQuality;
			*/
			//////////////////////////////////////////////////////////////

			g.FillPolygon(new SolidBrush(c), pnts);



			location.X -= offset.X;
			location.Y -= offset.Y;
		}

		private PointF GetBrickAngularPoint(PointF circleCentre, float angle, float radius)
		{
			return new PointF((float)Math.Cos(MathExt.ToRadians(angle)) * radius + circleCentre.X,
				(float)Math.Sin(MathExt.ToRadians(angle)) * radius + circleCentre.Y);
		}

		private void DrawBrick(Graphics g, RectangleF dest, Color c)
		{
			int yType = 0;
			if (PegInfo.CanBeOrange)
				yType = 1;
			if (PegInfo.QuickDisappear)
				yType += 4;

			if (Level.ShowPreview)
				yType = 0;

			if (Level.ShowCollision)
				g.FillRectangle(Brushes.White, dest);
			else {
				g.FillRectangle(new SolidBrush(c), dest);
				//g.DrawImage(mBrick, dest, new RectangleF(0, yType * 20, 32, 20), GraphicsUnit.Pixel);
			}
		}

		public PointF GetCentrePoint()
		{
			PointF cc = new PointF();
			cc.X = X - (mLength * (float)Math.Cos(MathExt.ToRadians(mAngle)));
			cc.Y = Y - (mLength * (float)Math.Sin(MathExt.ToRadians(mAngle)));
			return cc;
		}

		public override object Clone()
		{
			Brick cpyBrick = new Brick(Level);
			base.CloneTo(cpyBrick);

			cpyBrick.mType = mType;
			cpyBrick.mSectorAngle = mSectorAngle;
			cpyBrick.mWidth = mWidth;
			cpyBrick.mLength = mLength;
			cpyBrick.mAngle = mAngle;

			cpyBrick.mCurvePoints = mCurvePoints;
			cpyBrick.mCurved = mCurved;
			cpyBrick.mTextureFlip = mTextureFlip;

			return cpyBrick;
		}

		private float GetHeight()
		{
			return (2 * (float)Math.Sin((SectorAngle / 2) / 180.0 * Math.PI) * Length);
		}

		[EntryProperty(EntryPropertyType.Element, 0.0f)]
		[Description("How much the brick has been rotated in degrees.")]
		[Category("Layout")]
		[DefaultValue(90.0f)]
		public float Rotation
		{
			get
			{
				return mAngle;
			}
			set
			{
				mAngle = value;
			}
		}

		[EntryProperty(EntryPropertyType.Element, 0.0f)]
		[DisplayName("Sector Angle")]
		[Description("The angle of the sector if curved in degrees.")]
		[Category("Layout")]
		[DefaultValue(0.0f)]
		public float SectorAngle
		{
			get
			{
				return mSectorAngle;
			}
			set
			{
				mSectorAngle = value;
			}
		}

		[EntryProperty(EntryPropertyType.Element, 0.0f)]
		[Description("The length of the brick or radius of the sector if the brick is curved.")]
		[Category("Layout")]
		[DefaultValue(30.0f)]
		public float Length
		{
			get
			{
				return mLength;
			}
			set
			{
				mLength = value;
			}
		}

		[EntryProperty(EntryPropertyType.Element, 0.0f)]
		[DisplayName("Inner Radius")]
		[Description("The radius of the sector if the brick is curved or the length of the brick when the brick is straight.")]
		[Category("Layout")]
		[DefaultValue(30.0f)]
		public float InnerRadius
		{
			get
			{
				return mLength;
			}
			set
			{
				mLength = value;
			}
		}

		[EntryProperty(EntryPropertyType.Element, 0.0f)]
		[DisplayName("Outer Radius")]
		[Description("The outer radius of the sector if the brick is curved.")]
		[Category("Layout")]
		[DefaultValue(50.0f)]
		public float OuterRadius
		{
			get
			{
				return mLength + mWidth;
			}
			set
			{
				mLength = value - mWidth;
			}
		}

		[EntryProperty(EntryPropertyType.Element, 0.0f)]
		[Description("The width of the brick.")]
		[Category("Layout")]
		[DefaultValue(20.0f)]
		public float Width
		{
			get
			{
				return mWidth;
			}
			set
			{
				mWidth = value;
			}
		}

		[EntryProperty(EntryPropertyType.Element, 0)]
		[DisplayName("Curve Points")]
		[Description("The number of cuts in the brick which make up the curve. Basically how round the brick is, you would set a higher value if the sector angle was higher.")]
		[Category("Layout")]
		[DefaultValue(4)]
		public int CurvePoints
		{
			get
			{
				return mCurvePoints;
			}
			set
			{
				if (value < 2 || value > 257)
					throw new Exception("Curve points must be between 2 and 257.");

				mCurvePoints = value;
			}
		}

		[EntryProperty(EntryPropertyType.Element, false)]
		[Description("Whether the brick is curved or not.")]
		[Category("Layout")]
		[DefaultValue(false)]
		public bool Curved
		{
			get
			{
				return mCurved;
			}
			set
			{
				mCurved = value;
			}
		}

		[EntryProperty(EntryPropertyType.Element, false)]
		[DisplayName("Texture Flip")]
		[Description("Whether to flip the brick texture or not (i.e. change which side the light reflection is). Used to maintain the light reflection along a path of bricks.")]
		[Category("Appearance")]
		[DefaultValue(false)]
		public bool TextureFlip
		{
			get
			{
				return mTextureFlip;
			}
			set
			{
				mTextureFlip = value;
			}
		}

		public override int Type
		{
			get
			{
				return 6;
			}
		}

		public override RectangleF Bounds
		{
			get
			{
				return new RectangleF(
					DrawX - (mWidth / 2),
					DrawY - (mWidth / 2),
					mWidth, mWidth);
			}
		}
	}
}

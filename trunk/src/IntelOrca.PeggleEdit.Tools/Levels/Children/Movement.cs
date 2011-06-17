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
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
	/// <summary>
	/// The type of movement.
	/// </summary>
	public enum MovementType
	{
		NoMovement,
		VerticalCycle,
		HorizontalCycle,
		Circle,
		HorizontalInfinity,
		VericalInfinity,
		HorizontalArc,
		VerticalArc,
		Rotate,
		RotateBackAndForth,
		Unused,
		VerticalWrap,
		HorizontalWrap,
		RotateAroundCircle,
		RetraceCircle,
		WeirdShape,
	}

	/// <summary>
	/// Represents information about the movement path of a level entry and contains methods for calculating and drawing the path.
	/// </summary>
	[TypeConverterAttribute(typeof(ExpandableObjectConverter))]
	public class Movement : ICloneable, ILocation, IMovementContainer, ILevelChild
	{
		Level mLevel;

		IMovementContainer mMovementLink;
		int mMovementLinkREQUIRED;

		MovementType mType;
		short mRadius1;
		short mTimePeriod;
		short mRadius2;
		short mOffset;
		short mPause1;
		short mPause2;
		byte mPhase1;
		byte mPhase2;
		float mStartPhase;
		float mPostDelayPhase;
		float mMoveRotation;
		float mMaxAngle;
		float mRotation;


		bool mReverse;

		PointF mAnchorPoint;

		Movement mBaseMovement;

		float mUnknown8;
		float mUnknown10;
		float mUnknown11;

		float mObjectX;
		float mObjectY;

		public Movement(Level level)
		{
			mLevel = level;
		}

		private int GetMovementLinkIndex(object dml)
		{
			int mi = 2;
			foreach (LevelEntry le in mLevel.Entries) {
				mi++;
				if (le == dml)
					return mi;
				Movement m = le.MovementInfo;
				while (m != null) {
					mi++;
					if (m == dml)
						return mi;
					m = m.MovementInfo;
				}
			}
			return 0;
		}

		private IMovementContainer GetMovementLink(int dmi)
		{
			int mi = 2;
			foreach (LevelEntry le in mLevel.Entries) {
				mi++;
				if (dmi == mi)
					return le;
				Movement m = le.MovementInfo;
				while (m != null) {
					mi++;
					if (dmi == mi)
						return m;
					m = m.MovementInfo;
				}
			}
			return null;
		}

		public void ReadData(BinaryReader br, int version)
		{
			int mlink = br.ReadInt32();
			if (mlink == 0)
				return;
			if (mlink != 1) {
				if (mLevel == null)
					mMovementLinkREQUIRED = mlink;
				else
					mMovementLink = GetMovementLink(mlink);
				return;
			}

			sbyte movementShape = br.ReadSByte();
			mType = (MovementType)Math.Abs(movementShape);
			if (movementShape < 0) {
				mReverse = true;
			}

			int type = (int)mType;
			if (mReverse)
				type = (256 - type);

			mAnchorPoint.X = br.ReadSingle();
			mAnchorPoint.Y = br.ReadSingle();

			mTimePeriod = br.ReadInt16();

			FlagGroup fA = new FlagGroup(br.ReadInt16());

			if (fA[0])
				mOffset = br.ReadInt16();
			if (fA[1])
				mRadius1 = br.ReadInt16();
			if (fA[2])
				mStartPhase = br.ReadSingle();
			if (fA[3])
				mMoveRotation = MathExt.ToDegrees(br.ReadSingle());
			if (fA[4])
				mRadius2 = br.ReadInt16();
			if (fA[5])
				mPause1 = br.ReadInt16();
			if (fA[6])
				mPause2 = br.ReadInt16();
			if (fA[7])
				mPhase1 = br.ReadByte();
			if (fA[8])
				mPhase2 = br.ReadByte();
			if (fA[9])
				mPostDelayPhase = br.ReadSingle();
			if (fA[10])
				mMaxAngle = br.ReadSingle();
			if (fA[11])
				mUnknown8 = br.ReadSingle();
			if (fA[14])
				mRotation = MathExt.ToDegrees(br.ReadSingle());
			if (fA[12]) {
				mUnknown10 = br.ReadSingle();
				mUnknown11 = br.ReadSingle();
				//Another movement
				mBaseMovement = new Movement(mLevel);
				mBaseMovement.ReadData(br, version);
				//mBaseMovement = null;
			}
			if (fA[13]) {
				mObjectX = br.ReadSingle();
				mObjectY = br.ReadSingle();
			}
		}

		public void WriteData(BinaryWriter bw, int version)
		{
			FlagGroup fA = new FlagGroup();

			fA[0] = (mOffset != 0.0f);
			fA[1] = (mRadius1 != 0.0f);
			fA[2] = (mStartPhase != 0.0f);
			fA[3] = (mMoveRotation != 0.0f);
			fA[4] = (mRadius2 != 0.0f);
			fA[5] = (mPause1 != 0.0f);
			fA[6] = (mPause2 != 0.0f);
			fA[7] = (mPhase1 != 0.0f);
			fA[8] = (mPhase2 != 0.0f);
			fA[9] = (mPostDelayPhase != 0.0f);
			fA[10] = (mMaxAngle != 0.0f);
			fA[11] = (mUnknown8 != 0.0f);
			fA[14] = (mRotation != 0.0f);
			fA[12] = (mUnknown10 != 0.0f || mUnknown11 != 0.0f);
			fA[13] = (mObjectX != 0.0f || mObjectY != 0.0f);

			if (mBaseMovement != null)
				fA[12] = true;

			//Write data

			if (mMovementLink == null)
				bw.Write(1);
			else {
				bw.Write(GetMovementLinkIndex(mMovementLink));
				return;
			}

			int type = (int)mType;
			if (mReverse)
				type = (256 - type);
			bw.Write((byte)type);

			bw.Write(mAnchorPoint.X);
			bw.Write(mAnchorPoint.Y);

			bw.Write((short)mTimePeriod);

			bw.Write(fA.Int16);

			if (fA[0])
				bw.Write(mOffset);
			if (fA[1])
				bw.Write((short)mRadius1);
			if (fA[2])
				bw.Write(mStartPhase);
			if (fA[3])
				bw.Write(MathExt.ToRadians(mMoveRotation));
			if (fA[4])
				bw.Write((short)mRadius2);
			if (fA[5])
				bw.Write(mPause1);
			if (fA[6])
				bw.Write(mPause2);
			if (fA[7])
				bw.Write(mPhase1);
			if (fA[8])
				bw.Write(mPhase2);
			if (fA[9])
				bw.Write(mPostDelayPhase);
			if (fA[10])
				bw.Write(mMaxAngle);
			if (fA[11])
				bw.Write(mUnknown8);
			if (fA[14])
				bw.Write(MathExt.ToRadians(mRotation));
			if (fA[12]) {
				bw.Write(mUnknown10);
				bw.Write(mUnknown11);
				mBaseMovement.WriteData(bw, version);
			}
			if (fA[13]) {
				bw.Write(mObjectX);
				bw.Write(mObjectY);
			}
		}

		public void DrawAnchors(Graphics g, Level level)
		{
			PointF position = GetAnchorPoint();
			level.DrawAnchorPoint(g, position.X, position.Y);
	
			if (mBaseMovement != null)
				mBaseMovement.DrawAnchors(g, level);
		}

		public void DrawPath(Graphics g)
		{
			//if (mBaseMovement != null)
			//	mBaseMovement.DrawPath(g);

			if (mBaseMovement != null)
				mBaseMovement.DrawPath(g);

			PointF anchor = GetAnchorPoint();

			try {

				Matrix mx = new Matrix();
				mx.RotateAt(-mMoveRotation, new PointF(anchor.X, anchor.Y));

				g.Transform = mx;

				Pen pen = new Pen(Color.White);
				pen.DashPattern = new float[] { 6.0f, 8.0f };
				pen.DashStyle = DashStyle.Custom;

				switch (mType) {
					case MovementType.VerticalCycle:
						g.DrawLine(pen, anchor.X, anchor.Y - GetYRadius(),
							anchor.X, anchor.Y + GetYRadius());
						break;
					case MovementType.HorizontalCycle:
						g.DrawLine(pen, anchor.X - GetXRadius(), anchor.Y,
							anchor.X + GetXRadius(), anchor.Y);
						break;
					case MovementType.Circle:
					case MovementType.RotateAroundCircle:
						g.DrawEllipse(pen, anchor.X - GetXRadius(), anchor.Y - GetYRadius(),
										GetXRadius() * 2, GetYRadius() * 2);
						break;
					case MovementType.HorizontalInfinity:
						g.DrawBezier(pen,
							anchor.X, anchor.Y,
							anchor.X - (GetXRadius() * 1.35f), anchor.Y - (GetYRadius() * 1.8f),
							anchor.X - (GetXRadius() * 1.35f), anchor.Y + (GetYRadius() * 1.8f),
							anchor.X, anchor.Y);
						g.DrawBezier(pen,
							anchor.X, anchor.Y,
							anchor.X + (GetXRadius() * 1.35f), anchor.Y - (GetYRadius() * 1.8f),
							anchor.X + (GetXRadius() * 1.35f), anchor.Y + (GetYRadius() * 1.8f),
							anchor.X, anchor.Y);
						break;
					case MovementType.VericalInfinity:
						g.DrawBezier(pen,
							anchor.X, anchor.Y,
							anchor.X - GetXRadius(), anchor.Y - GetYRadius(),
							anchor.X + GetXRadius(), anchor.Y - GetYRadius(),
							anchor.X, anchor.Y);
						g.DrawBezier(pen,
							anchor.X, anchor.Y,
							anchor.X - GetXRadius(), anchor.Y + GetYRadius(),
							anchor.X + GetXRadius(), anchor.Y + GetYRadius(),
							anchor.X, anchor.Y);
						break;
					case MovementType.VerticalArc:
						g.DrawArc(pen, anchor.X - GetXRadius(), anchor.Y - GetYRadius(),
							GetXRadius() * 2, GetYRadius() * 2, -90.0f, 180.0f);
						break;
					case MovementType.HorizontalArc:
						g.DrawArc(pen, anchor.X - GetXRadius(), anchor.Y - GetYRadius(),
										GetXRadius() * 2, GetYRadius() * 2, 180.0f, 180.0f);
						break;
					case MovementType.HorizontalWrap:
						if (mReverse) {
							g.DrawLine(pen, anchor.X, anchor.Y,
								anchor.X - GetXRadius(), anchor.Y);
						} else {
							g.DrawLine(pen, anchor.X, anchor.Y,
								anchor.X + GetXRadius(), anchor.Y);
						}
						break;
					case MovementType.VerticalWrap:
						if (mReverse) {
							g.DrawLine(pen, anchor.X, anchor.Y,
								anchor.X, anchor.Y + GetYRadius());
						} else {
							g.DrawLine(pen, anchor.X, anchor.Y,
								anchor.X, anchor.Y - GetYRadius());
						}
						break;
				}

			} catch {

			} finally {
				g.Transform = new Matrix();
			}
		}

		public PointF GetEstimatedMovePosition()
		{
			PointF locationA = PointF.Empty;
			if (mBaseMovement != null)
				locationA = mBaseMovement.GetEstimatedMovePosition();

			PointF locationB = GetMyEstimatedMovePosition();

			return AddPoints(locationA, locationB);
		}

		public PointF GetMyEstimatedMovePosition()
		{
			if (mTimePeriod == 0)
				return mAnchorPoint;

			float angle, circle_x, circle_y;
			float phase = GetCurrentPhase();
			if (!mReverse && mType != MovementType.HorizontalWrap && mType != MovementType.VerticalWrap)
				phase = 100.0f - phase;

			Matrix matrix = new Matrix();
			matrix.RotateAt(-mMoveRotation, new PointF(mAnchorPoint.X, mAnchorPoint.Y));

			switch (mType) {
				case MovementType.VerticalCycle:
					angle = (phase * (float)(2 * Math.PI)) + (float)Math.PI;
					circle_y = (float)Math.Sin(angle) * (float)GetYRadius();
					return TransformPoint(matrix, new PointF(mAnchorPoint.X, mAnchorPoint.Y + circle_y));

				case MovementType.HorizontalCycle:
					angle = (phase * (float)(2 * Math.PI)) + (float)Math.PI;
					circle_x = (float)Math.Cos(angle) * (float)GetXRadius();
					return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y));

				case MovementType.Circle:
				case MovementType.RotateAroundCircle:
					angle = phase * (float)(2 * Math.PI);
					circle_x = (float)Math.Cos(angle) * (float)GetXRadius();
					circle_y = (float)Math.Sin(angle) * (float)GetYRadius();
					return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y + circle_y));

				case MovementType.HorizontalInfinity:
					angle = phase * (float)(2 * Math.PI);
					circle_x = (float)Math.Sin(angle + (Math.PI / 2)) * GetXRadius();
					circle_y = (float)Math.Sin(2 * angle) * GetYRadius() * 0.5f;
					return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y + circle_y));

				case MovementType.VericalInfinity:
					angle = phase * (float)(2 * Math.PI);
					circle_x = (float)Math.Sin(2 * angle) * GetXRadius();
					circle_y = (float)Math.Sin(angle + (Math.PI / 2)) * GetYRadius();
					return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y + circle_y));

				case MovementType.HorizontalArc:
					angle = phase * (float)(2 * Math.PI);
					circle_x = (float)Math.Cos(angle) * (float)GetXRadius();
					circle_y = (float)Math.Sin(angle) * (float)GetYRadius();
					return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y - Math.Abs(circle_y)));

				case MovementType.VerticalArc:
					angle = phase * (float)(2 * Math.PI) + (float)(Math.PI / 2);
					circle_x = (float)Math.Cos(angle) * (float)GetXRadius();
					circle_y = (float)Math.Sin(angle) * (float)GetYRadius();
					return TransformPoint(matrix, new PointF(mAnchorPoint.X + Math.Abs(circle_x), mAnchorPoint.Y - circle_y));

				case MovementType.HorizontalWrap:
					angle = (phase * GetXRadius()) % GetXRadius();
					if (mReverse)
						return TransformPoint(matrix, new PointF(mAnchorPoint.X - angle, mAnchorPoint.Y));
					else
						return TransformPoint(matrix, new PointF(mAnchorPoint.X + angle, mAnchorPoint.Y));

				case MovementType.VerticalWrap:
					angle = (phase * GetYRadius()) % GetYRadius();
					if (mReverse)
						return TransformPoint(matrix, new PointF(mAnchorPoint.X, mAnchorPoint.Y + angle));
					else
						return TransformPoint(matrix, new PointF(mAnchorPoint.X, mAnchorPoint.Y - angle));
				default:
					return new PointF(mAnchorPoint.X, mAnchorPoint.Y);
			}
		}

		public float GetEstimatedMoveAngle(float initialAngle)
		{
			float phase = GetCurrentPhase();

			if (!mReverse)
				phase = 1.0f - phase;

			switch (mType) {
				case MovementType.Rotate:
					return (initialAngle) - (phase * 360.0f);
				case MovementType.RotateAroundCircle:
					return (initialAngle + Rotation) - (phase * 360.0f);
				case MovementType.RotateBackAndForth:
					return (initialAngle) - (((float)Math.Sin(MathExt.ToRadians(phase * 360.0f))) * 180.0f);
				default:
					return initialAngle;
			}
		}

		private float GetCurrentPhase()
		{
			if (mLevel.ShowPreview) {
				ulong previewTime = mLevel.PreviewTime;

				double time_period = mTimePeriod;
				double total_time = GetTotalCycleTime();
				double ph1, ph2;
				double pa1, pa2;
				
				if (mPhase1 > mPhase2) {
					ph1 = (double)mPhase2 / 100.0;
					pa1 = (double)mPause2;
					ph2 = (double)mPhase1 / 100.0;
					pa2 = (double)mPause1;
				} else {
					ph1 = (double)mPhase1 / 100.0;
					pa1 = (double)mPause1;
					ph2 = (double)mPhase2 / 100.0;
					pa2 = (double)mPause2;
				}

				double ph_offset = (double)mStartPhase % 1.0;
				double remainder_time = 0;


				//ph1 = (ph1 + mStartPhase) % 1.0;
				//ph2 = (ph2 + mStartPhase) % 1.0;

				//if (ph_offset < ph1)
				//    remainder_time = (mTimePeriod * ph_offset);
				//else if (ph_offset < ph2)
				//    remainder_time = (mTimePeriod * ph_offset) + pa1;
				//else
				//    remainder_time = (mTimePeriod * ph_offset) + pa1 + pa2;

				remainder_time = ((double)previewTime + remainder_time) % total_time;

				double stg1time = ph1 * time_period;
				double stg2time = (ph2 * time_period) - stg1time;
				double stg3time = time_period - stg2time - stg1time;

				if (remainder_time > stg1time)
					remainder_time -= stg1time;
				else
					return (float)((remainder_time / time_period) + ph_offset);

				if (remainder_time > pa1)
					remainder_time -= pa1;
				else
					return (float)(ph1 + ph_offset);

				if (remainder_time > stg2time)
					remainder_time -= stg2time;
				else
					return (float)(ph1 + (remainder_time / time_period) + ph_offset);

				if (remainder_time > pa2)
					remainder_time -= pa2;
				else
					return (float)(ph2 + ph_offset);

				return (float)(ph2 + (remainder_time / time_period) + ph_offset);

				//Stage 1
				//Pause 1
				//Stage 2
				//Pause 2
				//Stage 3

				//double additionalPhase = (double)previewTime / GetTotalCycleTime();
				//additionalPhase = (additionalPhase * 100.0f) % 100;
				//additionalPhase /= 100.0f;

				//return mStartPhase + (float)additionalPhase;
			} else {
				return mStartPhase;
			}
		}

		public float GetTotalCycleTime()
		{
			return mTimePeriod + mPause1 + mPause2;
		}

		private PointF TransformPoint(Matrix matrix, PointF pnt)
		{
			PointF[] pnts = new PointF[] { pnt };
			matrix.TransformPoints(pnts);
			return pnts[0];
		}

		private PointF GetAnchorPoint()
		{
			if (mBaseMovement != null) {
				return AddPoints(mBaseMovement.GetEstimatedMovePosition(), mAnchorPoint);
			} else {
				return mAnchorPoint;
			}
		}

		private PointF AddPoints(PointF a, PointF b)
		{
			PointF locationSum = new PointF();
			locationSum.X = a.X + b.X;
			locationSum.Y = a.Y + b.Y;

			return locationSum;
		}

		private float GetEllipseCircumference(float w, float h)
		{
			return (float)(Math.PI * Math.Sqrt(((w * w) + (h * h)) / 2));
		}

		private float GetSpeed()
		{
			//Get time in seconds
			float time = mTimePeriod / 100;

			//Stop divide by zero
			if (time == 0.0f)
				return 0.0f;

			switch (mType) {
				case MovementType.VerticalCycle:
					return (GetYRadius() * 2) / time;
				case MovementType.HorizontalCycle:
					return (GetXRadius() * 2) / time;
				case MovementType.Circle:
				case MovementType.RotateAroundCircle:
				case MovementType.VerticalArc:
				case MovementType.HorizontalArc:
					return GetEllipseCircumference(mRadius1 * 2, mRadius2 * 2) / time;
				case MovementType.HorizontalInfinity:
				case MovementType.VericalInfinity:
					return 0.0f;
				case MovementType.HorizontalWrap:
					return GetXRadius() / time;
				case MovementType.VerticalWrap:
					return GetYRadius() / time;
			}

			return 0.0f;
		}

		private float GetXRadius()
		{
			return mRadius1;
		}

		private float GetYRadius()
		{
			if (mRadius2 == 0.0f)
				return mRadius1;
			else
				return mRadius2;
		}

		public float AnchorPointX
		{
			get
			{
				return mAnchorPoint.X;
			}
			set
			{
				mAnchorPoint.X = value;
			}
		}

		public float AnchorPointY
		{
			get
			{
				return mAnchorPoint.Y;
			}
			set
			{
				mAnchorPoint.Y = value;
			}
		}

		[Description("The type of movement path.")]
		public MovementType Type
		{
			get
			{
				return mType;
			}
			set
			{
				mType = value;
			}
		}

		[Description("Whether the movement path is reversed or not. (e.g. Clockwise or Anti-clockwise)")]
		[DefaultValue(false)]
		public bool Reverse
		{
			get
			{
				return mReverse;
			}
			set
			{
				mReverse = value;
			}
		}

		[DisplayName("Time Period")]
		[Description("The time period in deciseconds. (100 = 1 second)")]
		public short TimePeriod
		{
			get
			{
				return mTimePeriod;
			}
			set
			{
				mTimePeriod = value;
			}
		}

		[DisplayName("Radius 1")]
		[Description("The horizontal (X) radius of the movement cycle.")]
		public short Radius1
		{
			get
			{
				return mRadius1;
			}
			set
			{
				mRadius1 = value;
			}
		}

		[DisplayName("Radius 2")]
		[Description("The vertical (X) radius of the movement cycle. If this is set to 0, then it will take the value of radius 1.")]
		public short Radius2
		{
			get
			{
				return mRadius2;
			}
			set
			{
				mRadius2 = value;
			}
		}

		[Description("The percentage phase of the current object. The value is written as a decimal (e.g. 0.4 is 40%).")]
		[DefaultValue(0.0f)]
		[TypeConverter(typeof(PercentageConverter))]
		public float Phase
		{
			get
			{
				return mStartPhase;
			}
			set
			{
				mStartPhase = value;
			}
		}

		[DisplayName("Movement Angle")]
		[Description("The angle of the movement path in degrees.")]
		[DefaultValue(0.0f)]
		public float MovementAngle
		{
			get
			{
				return mMoveRotation;
			}
			set
			{
				mMoveRotation = value;
			}
		}

		[Description("The speed of the object in pixels per second.")]
		[ReadOnly(true)]
		public float Speed
		{
			get
			{
				return (float)Math.Round(GetSpeed(), 2);
			}
		}

		[DisplayName("Base Movement")]
		[Description("The movement properties of the movement properties.")]
		[EditorAttribute(typeof(MovementUITypeEditor), typeof(UITypeEditor))]
		public Movement MovementInfo
		{
			get
			{
				return mBaseMovement;
			}
			set
			{
				mBaseMovement = value;
			}
		}

		public object Clone()
		{
			Movement cpyMovement = new Movement(mLevel);

			if (mBaseMovement != null)
				cpyMovement.mBaseMovement = (Movement)mBaseMovement.Clone();

			cpyMovement.mType = mType;
			cpyMovement.mReverse = mReverse;
			cpyMovement.mTimePeriod = mTimePeriod;
			cpyMovement.mRadius1 = mRadius1;
			cpyMovement.mRadius2 = mRadius2;
			cpyMovement.mStartPhase = mStartPhase;
			cpyMovement.mMoveRotation = mMoveRotation;
			cpyMovement.mAnchorPoint = mAnchorPoint;

			cpyMovement.mOffset = mOffset;
			cpyMovement.mPause1 = mPause1;
			cpyMovement.mPause2 = mPause2;
			cpyMovement.mPhase1 = mPhase1;
			cpyMovement.mPhase2 = mPhase2;
			cpyMovement.mPostDelayPhase = mPostDelayPhase;
			cpyMovement.mMaxAngle = mMaxAngle;
			cpyMovement.mUnknown8 = mUnknown8;
			cpyMovement.mRotation = mRotation;
			cpyMovement.mUnknown10 = mUnknown10;
			cpyMovement.mUnknown11 = mUnknown11;
			cpyMovement.mObjectX = mObjectX;
			cpyMovement.mObjectY = mObjectY;

			return cpyMovement;
		}

		[Browsable(false)]
		public PointF Location
		{
			get
			{
				return mAnchorPoint;
			}
			set
			{
				mAnchorPoint = value;
			}
		}

		[DisplayName("Movement Link IDX")]
		[Description("If this is set, the object will take the movement properties of the object with this MUID.")]
		public int MovementLinkIDX
		{
			get
			{
				return GetMovementLinkIndex(mMovementLink);
			}
			set
			{
				IMovementContainer mc = GetMovementLink(value);
				if (mc == null)
					throw new Exception("This link index doesn't exist.");
				else
					mMovementLink = mc;
			}
		}

		[DefaultValue(0)]
		public short Offset
		{
			get
			{
				return mOffset;
			}
			set
			{
				mOffset = value;
			}
		}

		[Description("When the object reaches the percentage specified in Phase 1 of moving around the path, the object will pause for this amount of time in deciseconds. (100 = 1 second).")]
		[DefaultValue(0)]
		public short Pause1
		{
			get
			{
				return mPause1;
			}
			set
			{
				mPause1 = value;
			}
		}

		[Description("When the object reaches the percentage specified in Phase 2 of moving around the path, the object will pause for this amount of time in deciseconds. (100 = 1 second).")]
		[DefaultValue(0)]
		public short Pause2
		{
			get
			{
				return mPause2;
			}
			set
			{
				mPause2 = value;
			}
		}

		[Description("When the object reaches this percentage of moving around the path, the object will pause for the amount of time specified in Pause 1.")]
		[DefaultValue(0)]
		public byte Phase1
		{
			get
			{
				return mPhase1;
			}
			set
			{
				mPhase1 = value;
			}
		}

		[Description("When the object reaches this percentage of moving around the path, the object will pause for the amount of time specified in Pause 2.")]
		[DefaultValue(0)]
		public byte Phase2
		{
			get
			{
				return mPhase2;
			}
			set
			{
				mPhase2 = value;
			}
		}

		[DefaultValue(0.0f)]
		public float PostDelayPhase
		{
			get
			{
				return mPostDelayPhase;
			}
			set
			{
				mPostDelayPhase = value;
			}
		}

		[DefaultValue(0.0f)]
		public float MaxAngle
		{
			get
			{
				return mMaxAngle;
			}
			set
			{
				mMaxAngle = value;
			}
		}

		[DefaultValue(0.0f)]
		public float Unknown8
		{
			get
			{
				return mUnknown8;
			}
			set
			{
				mUnknown8 = value;
			}
		}

		[Description("An offset to the angle of rotation in degrees.")]
		[DefaultValue(0.0f)]
		public float Rotation
		{
			get
			{
				return mRotation;
			}
			set
			{
				mRotation = value;
			}
		}

		[DefaultValue(0.0f)]
		public float Unknown10
		{
			get
			{
				return mUnknown10;
			}
			set
			{
				mUnknown10 = value;
			}
		}

		[DefaultValue(0.0f)]
		public float Unknown11
		{
			get
			{
				return mUnknown11;
			}
			set
			{
				mUnknown11 = value;
			}
		}

		public float ObjectX
		{
			get
			{
				return mObjectX;
			}
			set
			{
				mObjectX = value;
			}
		}

		public float ObjectY
		{
			get
			{
				return mObjectY;
			}
			set
			{
				mObjectY = value;
			}
		}

		[Browsable(false)]
		public Level Level
		{
			get
			{
				return mLevel;
			}
			set
			{
				mLevel = value;

				if (mMovementLinkREQUIRED != 0) {
					mMovementLink = GetMovementLink(mMovementLinkREQUIRED);
					mMovementLinkREQUIRED = 0;
				}
			}
		}

		[Description("The movement link IDX of this object. Other objects can reference this object's movement information by setting their Movement Link IDX to this number.")]
		public int MUID
		{
			get
			{
				return GetMovementLinkIndex(this);
			}
		}
	}
}

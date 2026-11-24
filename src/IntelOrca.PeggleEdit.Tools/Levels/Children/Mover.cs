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
    public enum MoverType
    {
        NoMovement,
        VerticalCycle,
        HorizontalCycle,
        Circle,
        HorizontalInfinity,
        VerticalInfinity,
        HorizontalArc,
        VerticalArc,
        Rotate,
        RotateBackAndForth,
        Unused,
        VerticalWrap,
        HorizontalWrap,
        RotateAroundCircle,
        SwingAroundCircle,
        FlowerLoop,
    }

    /// <summary>
    /// Represents information about the movement path of a level entry and contains methods for calculating and drawing the path.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Mover : ICloneable, ILocation, IMovable, ILevelChild
    {
        Level mLevel;

        MoverType mType;
        short mRadius;
        short mTime;
        short mRadius2;
        short mOffset;
        short mPause1;
        short mPause2;
        byte mPhase1;
        byte mPhase2;
        float mPhase;
        float mPostDelayPhase;
        // Angles are kept in radians, as stored in the level file, so that they round trip exactly
        float mMoveRotation;
        float mMaxAngle;
        float mRotation;

        bool mReverse;

        PointF mAnchorPoint;

        float mCurrentAngle;

        float mCurrentX;
        float mCurrentY;

        // Flags as read, excluding the sub-movement, so that values present in the file but zero are kept
        int mValueFlags;

        [DefaultValue(0.0f)]
        [DisplayName("Sub-movement X Offset")]
        public float ParentLastX { get; set; }

        [DefaultValue(0.0f)]
        [DisplayName("Sub-movement Y Offset")]
        public float ParentLastY { get; set; }

        [DisplayName("Parent Mover")]
        [Description("Another mover that this mover moves along with, so that its whole path follows the other mover.")]
        [Editor(typeof(MoverUITypeEditor), typeof(UITypeEditor))]
        public MoverLink MoverLink { get; set; }

        public Mover(Level level)
        {
            mLevel = level;
        }

        public void ReadData(BinaryReader br, int version)
        {
            sbyte movementShape = br.ReadSByte();
            mType = (MoverType)Math.Abs((int)movementShape);
            if (movementShape < 0)
            {
                mReverse = true;
            }

            int type = (int)mType;
            if (mReverse)
                type = (256 - type);

            mAnchorPoint.X = br.ReadSingle();
            mAnchorPoint.Y = br.ReadSingle();

            mTime = br.ReadInt16();

            FlagGroup fA = new FlagGroup(br.ReadInt16());
            mValueFlags = fA.Int32 & ~(1 << 12);

            if (fA[0])
                mOffset = br.ReadInt16();
            if (fA[1])
                mRadius = br.ReadInt16();
            if (fA[2])
                mPhase = br.ReadSingle();
            if (fA[3])
                mMoveRotation = br.ReadSingle();
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
                mCurrentAngle = br.ReadSingle();
            if (fA[14])
                mRotation = br.ReadSingle();
            if (fA[12])
            {
                ParentLastX = br.ReadSingle();
                ParentLastY = br.ReadSingle();
                MoverLink = new MoverLink(mLevel);
                MoverLink.ReadData(br, version);
            }
            if (fA[13])
            {
                mCurrentX = br.ReadSingle();
                mCurrentY = br.ReadSingle();
            }
        }

        public void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup();

            fA[0] = (mOffset != 0.0f);
            fA[1] = (mRadius != 0.0f);
            fA[2] = (mPhase != 0.0f);
            fA[3] = (mMoveRotation != 0.0f);
            fA[4] = (mRadius2 != 0.0f);
            fA[5] = (mPause1 != 0.0f);
            fA[6] = (mPause2 != 0.0f);
            fA[7] = (mPhase1 != 0.0f);
            fA[8] = (mPhase2 != 0.0f);
            fA[9] = (mPostDelayPhase != 0.0f);
            fA[10] = (mMaxAngle != 0.0f);
            fA[11] = (mCurrentAngle != 0.0f);
            fA[14] = (mRotation != 0.0f);
            fA[12] = (ParentLastX != 0.0f || ParentLastY != 0.0f);
            fA[13] = (mCurrentX != 0.0f || mCurrentY != 0.0f);

            if (MoverLink != null)
                fA[12] = true;

            fA.Int32 |= mValueFlags;

            int type = (int)mType;
            if (mReverse)
                type = (256 - type);
            bw.Write((byte)type);

            bw.Write(mAnchorPoint.X);
            bw.Write(mAnchorPoint.Y);

            bw.Write((short)mTime);

            bw.Write(fA.Int16);

            if (fA[0])
                bw.Write(mOffset);
            if (fA[1])
                bw.Write((short)mRadius);
            if (fA[2])
                bw.Write(mPhase);
            if (fA[3])
                bw.Write(mMoveRotation);
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
                bw.Write(mCurrentAngle);
            if (fA[14])
                bw.Write(mRotation);
            if (fA[12])
            {
                bw.Write(ParentLastX);
                bw.Write(ParentLastY);

                var movementLink = MoverLink;
                if (MoverLink == null)
                    movementLink = new MoverLink(mLevel);
                movementLink.WriteData(bw, version);
            }
            if (fA[13])
            {
                bw.Write(mCurrentX);
                bw.Write(mCurrentY);
            }
        }

        public void DrawAnchors(Graphics g, Level level)
        {
            PointF position = GetAnchorPoint();
            level.DrawAnchorPoint(g, position.X, position.Y);

            MoverLink?.Mover?.DrawAnchors(g, level);
        }

        public void DrawPath(Graphics g)
        {
            MoverLink?.Mover?.DrawPath(g);

            PointF anchor = GetAnchorPoint();

            // Build on the current transform rather than replacing it, otherwise everything drawn
            // afterwards is offset on GDI+ implementations that do not isolate the transform within
            // the level's graphics container (e.g. Wine).
            Matrix backupTransform = g.Transform;
            Pen pen = null;
            try
            {
                using (Matrix mx = g.Transform)
                {
                    mx.Translate(-ParentLastX, -ParentLastY);
                    mx.RotateAt(-MoveRotation, new PointF(anchor.X, anchor.Y));
                    g.Transform = mx;
                }

                pen = new Pen(Color.White);
                pen.DashPattern = new float[] { 6.0f, 8.0f };
                pen.DashStyle = DashStyle.Custom;

                switch (mType)
                {
                    case MoverType.VerticalCycle:
                        g.DrawLine(pen, anchor.X, anchor.Y - GetYRadius(),
                            anchor.X, anchor.Y + GetYRadius());
                        break;
                    case MoverType.HorizontalCycle:
                        g.DrawLine(pen, anchor.X - GetXRadius(), anchor.Y,
                            anchor.X + GetXRadius(), anchor.Y);
                        break;
                    case MoverType.Circle:
                    case MoverType.RotateAroundCircle:
                        g.DrawEllipse(pen, anchor.X - GetXRadius(), anchor.Y - GetYRadius(),
                                        GetXRadius() * 2, GetYRadius() * 2);
                        break;
                    case MoverType.HorizontalInfinity:
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
                    case MoverType.VerticalInfinity:
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
                    case MoverType.VerticalArc:
                        g.DrawArc(pen, anchor.X - GetXRadius(), anchor.Y - GetYRadius(),
                            GetXRadius() * 2, GetYRadius() * 2, -90.0f, 180.0f);
                        break;
                    case MoverType.HorizontalArc:
                        g.DrawArc(pen, anchor.X - GetXRadius(), anchor.Y - GetYRadius(),
                                        GetXRadius() * 2, GetYRadius() * 2, 180.0f, 180.0f);
                        break;
                    case MoverType.HorizontalWrap:
                        if (mReverse)
                        {
                            g.DrawLine(pen, anchor.X, anchor.Y,
                                anchor.X - GetXRadius(), anchor.Y);
                        }
                        else
                        {
                            g.DrawLine(pen, anchor.X, anchor.Y,
                                anchor.X + GetXRadius(), anchor.Y);
                        }
                        break;
                    case MoverType.VerticalWrap:
                        if (mReverse)
                        {
                            g.DrawLine(pen, anchor.X, anchor.Y,
                                anchor.X, anchor.Y + GetYRadius());
                        }
                        else
                        {
                            g.DrawLine(pen, anchor.X, anchor.Y,
                                anchor.X, anchor.Y - GetYRadius());
                        }
                        break;
                    case MoverType.SwingAroundCircle:
                        g.DrawEllipse(pen, anchor.X - GetXRadius(), anchor.Y - GetYRadius(),
                            GetXRadius() * 2, GetYRadius() * 2);
                        break;
                }

            }
            catch
            {

            }
            finally
            {
                pen?.Dispose();
                g.Transform = backupTransform;
                backupTransform.Dispose();
            }
        }

        public bool HasEntryParent()
        {
            foreach (LevelEntry entry in mLevel.Entries)
            {
                if (entry.MoverLink?.Mover == this)
                {
                    return true;
                }
            }
            return false;
        }

        public PointF GetEstimatedMovePosition()
        {
            PointF locationA = PointF.Empty;
            if (MoverLink?.Mover is Mover subMovement)
                locationA = subMovement.GetEstimatedMovePosition();

            PointF locationB = GetMyEstimatedMovePosition();

            return AddPoints(new PointF(-ParentLastX, -ParentLastY), AddPoints(locationA, locationB));
        }

        public PointF GetMyEstimatedMovePosition()
        {
            if (mTime == 0 || !HasEntryParent())
                return mAnchorPoint;

            float angle, circle_x, circle_y;
            float phase = GetCurrentPhase();
            if (!mReverse && mType != MoverType.HorizontalWrap && mType != MoverType.VerticalWrap)
                phase = 100.0f - phase;

            Matrix matrix = new Matrix();
            matrix.RotateAt(-MoveRotation, new PointF(mAnchorPoint.X, mAnchorPoint.Y));

            switch (mType)
            {
                case MoverType.VerticalCycle:
                    angle = (phase * (float)(2 * Math.PI)) + (float)Math.PI;
                    circle_y = (float)Math.Sin(angle) * (float)GetYRadius();
                    return TransformPoint(matrix, new PointF(mAnchorPoint.X, mAnchorPoint.Y + circle_y));

                case MoverType.HorizontalCycle:
                    angle = (phase * (float)(2 * Math.PI)) + (float)Math.PI;
                    circle_x = (float)Math.Cos(angle) * (float)GetXRadius();
                    return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y));

                case MoverType.Circle:
                case MoverType.RotateAroundCircle:
                    angle = phase * (float)(2 * Math.PI);
                    circle_x = (float)Math.Cos(angle) * (float)GetXRadius();
                    circle_y = (float)Math.Sin(angle) * (float)GetYRadius();
                    return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y + circle_y));

                case MoverType.HorizontalInfinity:
                    angle = phase * (float)(2 * Math.PI);
                    circle_x = (float)Math.Sin(angle + (Math.PI / 2)) * GetXRadius();
                    circle_y = (float)Math.Sin(2 * angle) * GetYRadius() * 0.5f;
                    return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y + circle_y));

                case MoverType.VerticalInfinity:
                    angle = phase * (float)(2 * Math.PI);
                    circle_x = (float)Math.Sin(2 * angle) * GetXRadius();
                    circle_y = (float)Math.Sin(angle + (Math.PI / 2)) * GetYRadius();
                    return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y + circle_y));

                case MoverType.HorizontalArc:
                    angle = phase * (float)(2 * Math.PI);
                    circle_x = (float)Math.Cos(angle) * (float)GetXRadius();
                    circle_y = (float)Math.Sin(angle) * (float)GetYRadius();
                    return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y - Math.Abs(circle_y)));

                case MoverType.VerticalArc:
                    angle = phase * (float)(2 * Math.PI) + (float)(Math.PI / 2);
                    circle_x = (float)Math.Cos(angle) * (float)GetXRadius();
                    circle_y = (float)Math.Sin(angle) * (float)GetYRadius();
                    return TransformPoint(matrix, new PointF(mAnchorPoint.X + Math.Abs(circle_x), mAnchorPoint.Y - circle_y));

                case MoverType.HorizontalWrap:
                    angle = (phase * GetXRadius()) % GetXRadius();
                    if (mReverse)
                        return TransformPoint(matrix, new PointF(mAnchorPoint.X - angle, mAnchorPoint.Y));
                    else
                        return TransformPoint(matrix, new PointF(mAnchorPoint.X + angle, mAnchorPoint.Y));

                case MoverType.VerticalWrap:
                    angle = (phase * GetYRadius()) % GetYRadius();
                    if (mReverse)
                        return TransformPoint(matrix, new PointF(mAnchorPoint.X, mAnchorPoint.Y + angle));
                    else
                        return TransformPoint(matrix, new PointF(mAnchorPoint.X, mAnchorPoint.Y - angle));
                case MoverType.SwingAroundCircle:
                    var phase2 = (float)Math.Sin(phase * 2 * Math.PI);
                    angle = (float)(phase2 * Math.PI - (mPhase * 2 * Math.PI));
                    circle_x = (float)Math.Cos(angle) * GetXRadius();
                    circle_y = (float)Math.Sin(angle) * GetYRadius();
                    return TransformPoint(matrix, new PointF(mAnchorPoint.X + circle_x, mAnchorPoint.Y + circle_y));
                default:
                    return new PointF(mAnchorPoint.X, mAnchorPoint.Y);
            }
        }

        public float GetEstimatedMoveAngle(float initialAngle)
        {
            float phase = GetCurrentPhase();

            if (!mReverse)
                phase = 1.0f - phase;

            switch (mType)
            {
                case MoverType.Rotate:
                    return (initialAngle) - (phase * 360.0f);
                case MoverType.RotateAroundCircle:
                    return (initialAngle + Rotation) - (phase * 360.0f);
                case MoverType.RotateBackAndForth:
                    return (initialAngle) - (((float)Math.Sin(MathExt.ToRadians(phase * 360.0f))) * 180.0f);
                case MoverType.SwingAroundCircle:
                    var phase2 = (float)Math.Sin(phase * 2 * Math.PI);
                    return initialAngle + Rotation + (mPhase * 360) - (phase2 * 180);
                default:
                    return initialAngle;
            }
        }

        private float GetCurrentPhase()
        {
            var startPhrase = mPhase;
            if (mType == MoverType.SwingAroundCircle)
                startPhrase = 0;
            if (mLevel.ShowPreview)
            {
                ulong previewTime = mLevel.PreviewTime;

                double time_period = mTime;
                double total_time = GetTotalCycleTime();
                double ph1, ph2;
                double pa1, pa2;

                if (mPhase1 > mPhase2)
                {
                    ph1 = (double)mPhase2 / 100.0;
                    pa1 = (double)mPause2;
                    ph2 = (double)mPhase1 / 100.0;
                    pa2 = (double)mPause1;
                }
                else
                {
                    ph1 = (double)mPhase1 / 100.0;
                    pa1 = (double)mPause1;
                    ph2 = (double)mPhase2 / 100.0;
                    pa2 = (double)mPause2;
                }

                double ph_offset = startPhrase % 1.0;
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
            }
            else
            {
                return startPhrase;
            }
        }

        public float GetTotalCycleTime()
        {
            return mTime + mPause1 + mPause2;
        }

        private PointF TransformPoint(Matrix matrix, PointF pnt)
        {
            PointF[] pnts = new PointF[] { pnt };
            matrix.TransformPoints(pnts);
            return pnts[0];
        }

        private PointF GetAnchorPoint()
        {
            if (MoverLink?.Mover is Mover subMovement)
            {
                return AddPoints(subMovement.GetEstimatedMovePosition(), mAnchorPoint);
            }
            else
            {
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
            float time = mTime / 100;

            //Stop divide by zero
            if (time == 0.0f)
                return 0.0f;

            switch (mType)
            {
                case MoverType.VerticalCycle:
                    return (GetYRadius() * 2) / time;
                case MoverType.HorizontalCycle:
                    return (GetXRadius() * 2) / time;
                case MoverType.Circle:
                case MoverType.RotateAroundCircle:
                case MoverType.VerticalArc:
                case MoverType.HorizontalArc:
                case MoverType.SwingAroundCircle:
                    return GetEllipseCircumference(mRadius * 2, mRadius2 * 2) / time;
                case MoverType.HorizontalInfinity:
                case MoverType.VerticalInfinity:
                    return 0.0f;
                case MoverType.HorizontalWrap:
                    return GetXRadius() / time;
                case MoverType.VerticalWrap:
                    return GetYRadius() / time;
            }

            return 0.0f;
        }

        private float GetXRadius()
        {
            return mRadius;
        }

        private float GetYRadius()
        {
            if (mRadius2 == 0.0f)
                return mRadius;
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

        [Description("The shape of the movement path.")]
        public MoverType Type
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

        [DisplayName("Time")]
        [Description("How long one full trip around the path takes, in hundredths of a second (100 = 1 second), not counting pauses. Smaller is faster.")]
        public short Time
        {
            get
            {
                return mTime;
            }
            set
            {
                mTime = value;
            }
        }

        [DisplayName("Radius")]
        [Description("The horizontal size of the movement path.")]
        public short Radius
        {
            get
            {
                return mRadius;
            }
            set
            {
                mRadius = value;
            }
        }

        [DisplayName("Y Radius")]
        [Description("The vertical size of the movement path. 0 uses Radius.")]
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

        [Description("Where on the path the object starts, as a fraction of the way round (e.g. 0.25 is a quarter of the way). Phase 1 and Phase 2 are measured from this point.")]
        [DefaultValue(0.0f)]
        [TypeConverter(typeof(PercentageConverter))]
        public float Phase
        {
            get
            {
                return mPhase;
            }
            set
            {
                mPhase = value;
            }
        }

        [DisplayName("Path Rotate")]
        [Description("Rotates the whole movement path around its centre, in degrees.")]
        [DefaultValue(0.0f)]
        public float MoveRotation
        {
            get
            {
                return MathExt.ToDegrees(mMoveRotation);
            }
            set
            {
                mMoveRotation = MathExt.ToRadians(value);
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

        object ICloneable.Clone() => Clone();

        public Mover Clone()
        {
            Mover cpyMovement = new Mover(mLevel);

            cpyMovement.MoverLink = MoverLink?.Clone();
            cpyMovement.mType = mType;
            cpyMovement.mReverse = mReverse;
            cpyMovement.mTime = mTime;
            cpyMovement.mRadius = mRadius;
            cpyMovement.mRadius2 = mRadius2;
            cpyMovement.mPhase = mPhase;
            cpyMovement.mMoveRotation = mMoveRotation;
            cpyMovement.mAnchorPoint = mAnchorPoint;

            cpyMovement.mOffset = mOffset;
            cpyMovement.mPause1 = mPause1;
            cpyMovement.mPause2 = mPause2;
            cpyMovement.mPhase1 = mPhase1;
            cpyMovement.mPhase2 = mPhase2;
            cpyMovement.mPostDelayPhase = mPostDelayPhase;
            cpyMovement.mMaxAngle = mMaxAngle;
            cpyMovement.mCurrentAngle = mCurrentAngle;
            cpyMovement.mRotation = mRotation;
            cpyMovement.ParentLastX = ParentLastX;
            cpyMovement.ParentLastY = ParentLastY;
            cpyMovement.mCurrentX = mCurrentX;
            cpyMovement.mCurrentY = mCurrentY;
            cpyMovement.mValueFlags = mValueFlags;

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

        [Description("Moves the object's timing forward by this many hundredths of a second, as if it had already been moving that long. Use it to stagger objects that share the same path.")]
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

        [DisplayName("Pause 1")]
        [Description("How long the object stops when it reaches Phase 1, in hundredths of a second (100 = 1 second). 0 turns off both pauses.")]
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

        [DisplayName("Pause 2")]
        [Description("How long the object stops when it reaches Phase 2, in hundredths of a second (100 = 1 second). Only works when Pause 1 is set.")]
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

        [DisplayName("Phase 1")]
        [Description("How far round the path, in percent from the starting Phase, the object goes before it stops for Pause 1.")]
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

        [DisplayName("Phase 2")]
        [Description("How far round the path, in percent from the starting Phase, the object goes before it stops for Pause 2. Should be at least Phase 1.")]
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

        [DisplayName("Post Phase")]
        [Description("Only used with pauses: where the object starts in the whole cycle including the pauses, as a fraction. Use it to start an object part way through a pause.")]
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

        [DisplayName("Max Angle")]
        [Description("How far the object swings either side of the middle for Rotate Back And Forth and Swing Around Circle, in radians. 0 means a half turn either side.")]
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

        [DisplayName("Current Angle")]
        [Description("The angle of the object when the level was saved. This is game state that the game recalculates, not a setting.")]
        [DefaultValue(0.0f)]
        public float CurrentAngle
        {
            get
            {
                return mCurrentAngle;
            }
            set
            {
                mCurrentAngle = value;
            }
        }

        [Description("Extra rotation added to the object's own angle, in degrees.")]
        [DefaultValue(0.0f)]
        public float Rotation
        {
            get
            {
                return MathExt.ToDegrees(mRotation);
            }
            set
            {
                mRotation = MathExt.ToRadians(value);
            }
        }

        public float CurrentX
        {
            get
            {
                return mCurrentX;
            }
            set
            {
                mCurrentX = value;
            }
        }

        public float CurrentY
        {
            get
            {
                return mCurrentY;
            }
            set
            {
                mCurrentY = value;
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
                if (MoverLink != null)
                    MoverLink.Level = value;
            }
        }

        [Description("The movement link IDX of this object. Other objects can reference this object's movement information by setting their Mover Link IDX to this number.")]
        public int MUID => mLevel.GetMoverId(this);

        public override string ToString() => $"Mover #{MUID}";
    }
}

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
            if (!HasPath)
                return;

            // The path goes where the parent mover has moved the anchor to
            PointF anchor = GetAnchorPoint();
            anchor = new PointF(anchor.X - ParentLastX, anchor.Y - ParentLastY);

            // The path is drawn through where the object goes, so that it is the shape the object follows
            bool wraps = mType == MoverType.HorizontalWrap || mType == MoverType.VerticalWrap;
            const int Segments = 120;
            var points = new PointF[Segments + 1];
            for (int i = 0; i <= Segments; i++)
            {
                // A wrap is drawn up to where it jumps back to the start
                double u = (double)i / Segments;
                points[i] = GetPosition(wraps ? u * 0.999 : u, anchor);
                // GDI+ fails on points it can not draw (e.g. from a NaN angle in the file)
                if (!IsDrawable(points[i]))
                    return;
            }

            using (var pen = new Pen(Color.White))
            {
                pen.DashPattern = new float[] { 6.0f, 8.0f };
                pen.DashStyle = DashStyle.Custom;
                g.DrawLines(pen, points);
            }
        }

        private static bool IsDrawable(PointF p) =>
            !float.IsNaN(p.X) && !float.IsNaN(p.Y) && Math.Abs(p.X) < 1e6f && Math.Abs(p.Y) < 1e6f;

        /// <summary>
        /// Whether the movement moves the object along a path, rather than only turning it or holding it still.
        /// </summary>
        private bool HasPath =>
            mType != MoverType.NoMovement && mType != MoverType.Rotate && mType != MoverType.RotateBackAndForth &&
            mType != MoverType.Unused;

        /// <summary>
        /// Whether this is the mover of an entry made by a generator to be drawn, which is not in the level's entries.
        /// </summary>
        [Browsable(false)]
        public bool IsGenerated { get; set; }

        /// <summary>
        /// Whether an entry is moved by this mover: it is the entry's mover, or a parent mover owned by the entry's
        /// mover (as an applied generator on an ellipse has, which is not marked as generated once the level is read
        /// again).
        /// </summary>
        public bool HasEntryParent()
        {
            if (IsGenerated)
                return true;
            // While the level is drawn or run, its entries' movers are known, rather than looked for each time
            var entryMovers = mLevel.EntryMovers;
            if (entryMovers != null)
                return entryMovers.Contains(this);
            foreach (var mover in Level.GetEntryMovers(mLevel.Entries))
            {
                if (mover == this)
                    return true;
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

        // While set, positions are worked out for the start, not for the preview time
        bool mAtStart;

        /// <summary>
        /// Gets where this mover's own path puts an object when it starts (not counting a parent mover).
        /// </summary>
        public PointF GetStartPosition()
        {
            mAtStart = true;
            try
            {
                return GetMyEstimatedMovePosition();
            }
            finally
            {
                mAtStart = false;
            }
        }

        /// <summary>
        /// Forgets which values were in the file the mover was read from, so that only values that are set are written.
        /// </summary>
        internal void ForgetFileValues() => mValueFlags = 0;

        public PointF GetMyEstimatedMovePosition()
        {
            if (mTime <= 0 || mType == MoverType.NoMovement || (!mAtStart && !HasEntryParent()))
                return mAnchorPoint;
            return GetPosition(GetCycle(GetMoveTime()), mAnchorPoint);
        }

        /// <summary>
        /// Where the movement puts an object when it is <paramref name="u"/> of the way through its cycle, as the game's
        /// mover update works it out: the shape, then turned anticlockwise about the anchor by Path Rotate.
        /// </summary>
        private PointF GetPosition(double u, PointF anchor)
        {
            var (x, y) = GetShapeOffset(u);
            if (mMoveRotation != 0)
            {
                double c = Math.Cos(mMoveRotation), s = Math.Sin(mMoveRotation);
                (x, y) = (x * c + y * s, y * c - x * s);
            }
            return new PointF((float)(anchor.X + x), (float)(anchor.Y + y));
        }

        /// <summary>
        /// Where the shape of the movement is, from the anchor, before Path Rotate turns it (y is down the screen). The
        /// radii are used as they are, so a negative one flips the shape, and the y-radius is the radius when it is 0.
        /// </summary>
        private (double X, double Y) GetShapeOffset(double u)
        {
            double r = mRadius;
            double ry = GetYRadius();
            double dir = mReverse ? -1 : 1;
            double a = 2 * Math.PI * u * dir;
            switch (mType)
            {
                case MoverType.VerticalCycle:
                    return (0, -ry * Math.Sin(a));
                case MoverType.HorizontalCycle:
                    return (r * Math.Cos(a), 0);
                case MoverType.Circle:
                case MoverType.RotateAroundCircle:
                    return (r * Math.Cos(a), -ry * Math.Sin(a));
                case MoverType.HorizontalInfinity:
                    return (r * Math.Cos(a), -ry * Math.Sin(2 * a) * 0.5);
                case MoverType.VerticalInfinity:
                    return (r * Math.Sin(2 * a), -ry * Math.Sin(a));
                case MoverType.HorizontalArc:
                {
                    // Eases back and forth over the max angle (half a turn when 0), whichever way it is reversed
                    double t = GetArcAngle(u);
                    return (r * Math.Cos(t), -ry * Math.Sin(t));
                }
                case MoverType.VerticalArc:
                {
                    double t = GetArcAngle(u);
                    return (r * Math.Sin(t), -ry * Math.Cos(t));
                }
                case MoverType.VerticalWrap:
                    return (0, -(u - Math.Truncate(u)) * ry * dir);
                case MoverType.HorizontalWrap:
                    return ((u - Math.Truncate(u)) * r * dir, 0);
                case MoverType.SwingAroundCircle:
                {
                    double theta = GetSwingAngle(u);
                    return (r * Math.Cos(theta), -ry * Math.Sin(theta));
                }
                case MoverType.FlowerLoop:
                    return (r * (0.8 * Math.Cos(4 * a) + 0.2 * Math.Cos(3 * a)),
                            -ry * (0.8 * Math.Sin(3 * a) + 0.17725 * Math.Sin(4 * a)));
                default:
                    return (0, 0);
            }
        }

        /// <summary>
        /// How far round an arc is: from 0 to the max angle (half a turn when 0) and back, slowing at each end.
        /// </summary>
        private double GetArcAngle(double u) =>
            (Math.Sin((2 * Math.PI * u) - (Math.PI / 2)) + 1) * SwingAmplitude * 0.5;

        public float GetEstimatedMoveAngle(float initialAngle)
        {
            // Without a time (or a type) the game leaves the object at its anchor, turned only by the rotation
            if (mTime <= 0 || mType == MoverType.NoMovement)
                return mType == MoverType.RotateAroundCircle || mType == MoverType.SwingAroundCircle ? initialAngle + Rotation : initialAngle;

            float phase = GetCurrentPhase();

            if (!mReverse)
                phase = 1.0f - phase;

            switch (mType)
            {
                case MoverType.Rotate:
                    // With a max angle, the game holds the object turned by it instead (seen in the game)
                    if (mMaxAngle != 0)
                        return initialAngle + (float)MathExt.ToDegrees(mReverse ? -mMaxAngle : mMaxAngle);
                    return (initialAngle) - (phase * 360.0f);
                case MoverType.RotateAroundCircle:
                    if (HasEllipseAngle)
                        return initialAngle + Rotation + (float)MathExt.ToDegrees(GetEllipseAngle(2 * Math.PI * GetCycle(GetMoveTime()) * (mReverse ? -1 : 1)));
                    return (initialAngle + Rotation) - (phase * 360.0f);
                case MoverType.RotateBackAndForth:
                    return initialAngle + (float)MathExt.ToDegrees(GetBackAndForthAngle());
                case MoverType.SwingAroundCircle:
                    // The object turns as it goes round the circle (but not by the path rotation that turns its position)
                    if (HasEllipseAngle)
                        return initialAngle + Rotation + (float)MathExt.ToDegrees(GetEllipseAngle(GetSwingAngle(GetCycle(GetMoveTime()))));
                    return initialAngle + Rotation + (float)MathExt.ToDegrees(GetSwingAngle(GetCycle(GetMoveTime())));
                default:
                    return initialAngle;
            }
        }

        /// <summary>
        /// Whether an object going round an ellipse is turned to face along it: when it has a y-radius (even one the same
        /// as the radius) and a radius, the game turns it by the direction a quarter turn on round the ellipse instead
        /// of by how far round it is.
        /// </summary>
        private bool HasEllipseAngle => mRadius2 != 0 && mRadius != 0;

        /// <summary>
        /// The angle, anticlockwise on screen, of where an object a quarter turn on from <paramref name="a"/> round the
        /// ellipse is, from its centre.
        /// </summary>
        private double GetEllipseAngle(double a)
        {
            double b = a + (Math.PI / 2);
            return Math.Atan2(GetYRadius() * Math.Sin(b), mRadius * Math.Cos(b));
        }

        /// <summary>
        /// The angle, in radians anticlockwise on screen, of swing around circle:
        /// dir × (MaxAngle × sin(2π × (u − Phase)) + PathRotate + 2π × Phase), where dir is 1, or -1 when reversed, and
        /// a max angle of 0 is half a turn. The phase is the centre of the swing, not when it starts. The object turns
        /// by this angle. Its position is then also turned anticlockwise around the anchor by the path rotation, as for
        /// every mover, so the path rotation moves it twice (seen in the game).
        /// </summary>
        private double GetSwingAngle(double u)
        {
            double time = u - mPhase;
            double dir = mReverse ? -1 : 1;
            return dir * ((SwingAmplitude * Math.Sin(2 * Math.PI * time)) + (2 * Math.PI * mPhase) + mMoveRotation);
        }

        /// <summary>
        /// The angle, in radians anticlockwise on screen, that rotate back and forth turns an object by:
        /// dir × (MaxAngle × sin(2π × u) + PathRotate), where u starts at the phase, as for other movers
        /// (seen in the game: unlike swing around circle, the phase is not the centre of the swing).
        /// </summary>
        private double GetBackAndForthAngle()
        {
            double time = GetCycle(GetMoveTime());
            double dir = mReverse ? -1 : 1;
            return dir * ((SwingAmplitude * Math.Sin(2 * Math.PI * time)) + mMoveRotation);
        }

        private double SwingAmplitude => mMaxAngle != 0 ? mMaxAngle : Math.PI;

        /// <summary>
        /// The game's clock for the movement (MoveTime): the preview's ticks, or 0 when it starts.
        /// </summary>
        private long GetMoveTime() => !mAtStart && mLevel != null && mLevel.ShowPreview ? (long)mLevel.PreviewTime : 0;

        private float GetCurrentPhase() => mTime > 0 ? (float)GetCycle(GetMoveTime()) : mPhase;

        /// <summary>
        /// How far through its cycle the movement is at a tick of the game's clock, as the game's phase function
        /// works it out, so in whole ticks: the offset and the phase move the clock on, and with a first pause
        /// the clock stops for Pause 1 ticks at Phase 1 % of the time and for Pause 2 ticks at Phase 2 %, with the post
        /// phase moving the clock on through the whole cycle including the pauses. It can be below 0 (a negative offset).
        /// </summary>
        private double GetCycle(long moveTime)
        {
            long time = mTime;
            if (time <= 0)
                return 0;
            long m = mOffset + moveTime;
            if (mPause1 > 0)
            {
                long length = time + mPause1 + mPause2;
                if (length > 0)
                {
                    long p1 = time * mPhase1 / 100;
                    long p2 = (mPhase2 - mPhase1) * time / 100;
                    long r = (Truncate(mPostDelayPhase * length) + m) % length;
                    if (r <= p1)
                        m = r;
                    else if (r <= p1 + mPause1)
                        m = p1;
                    else if (r <= p1 + mPause1 + p2)
                        m = r - mPause1;
                    else if (r <= p1 + mPause1 + p2 + mPause2)
                        m = p1 + p2;
                    else
                        m = r - mPause1 - mPause2;
                }
            }
            // Phase × Time and the clock are added as floats before the fraction is dropped (fimul, fiadd, ftol)
            return Truncate((double)(float)((double)mPhase * time) + m) % time / (double)time;
        }

        /// <summary>
        /// Rounds to a float, then drops the fraction, as the game does: it runs the FPU at single precision (as Direct3D
        /// sets it), so that a phase of 0.7 (0.69999999 as a float) of 1000 ticks is 700 ticks rather than 699.
        /// </summary>
        private static long Truncate(double value) => (long)(float)value;

        /// <summary>
        /// The phase that starts a mover with this time at <paramref name="tick"/>: the nearest float to
        /// tick ÷ time that the game does not take for the tick before.
        /// </summary>
        internal static float GetTickPhase(long tick, short time)
        {
            float phase = (float)((double)tick / time);
            while (Truncate((double)phase * time) < tick)
                phase = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(phase), 0) + 1), 0);
            return phase;
        }

        public float GetTotalCycleTime()
        {
            return mTime + mPause1 + mPause2;
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

        /// <summary>
        /// The path rotation in radians anticlockwise, as stored in the level file.
        /// </summary>
        internal float MoveRotationRadians
        {
            get => mMoveRotation;
            set => mMoveRotation = value;
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

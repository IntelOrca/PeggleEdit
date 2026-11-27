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
using System.Drawing.Design;
using System.IO;
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Extensions;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// Represents the polygon level entry.
    /// </summary>
    /// <summary>
    /// Which side of a poly the ball is kept on, from poly_normaldir in PopCap's level editor.
    /// </summary>
    public enum PolyNormalDir : sbyte
    {
        KeepIn = -1,
        Both = 0,
        KeepOut = 1,
    }

    public class Poly : LevelEntry, ICloneable, IPointContainer
    {
        byte mNormalDir;

        float mRotation;
        float mScale;
        int mGrowType;
        List<PointF> mPoints;

        // Which optional values were present, and unknown values, preserved so that they are written back as read
        int mUnknownFlagsA;
        int mUnknownFlagsB;
        bool mHasRotation;
        bool mHasAngularVelocity;
        float mAngularVelocity;
        bool mHasScale;
        bool mHasNormalDir;
        bool mHasGrowTimer;
        byte mGrowTimer;
        bool mHasGrowType;
        bool mHasFlashTimer;
        short mFlashTimer;

        public Poly(Level level)
            : base(level)
        {
            mPoints = new List<PointF>();
        }

        public override void ReadData(BinaryReader br, int version)
        {
            FlagGroup fA = new FlagGroup(br.ReadByte());
            FlagGroup fB = new FlagGroup();
            if (version >= 0x23)
                fB = new FlagGroup(br.ReadByte());

            mUnknownFlagsA = fA.Int32 & ((1 << 0) | (1 << 6) | (1 << 7));
            mUnknownFlagsB = fB.Int32 & ~((1 << 0) | (1 << 1) | (1 << 2));

            mHasRotation = fA[2];
            if (fA[2])
                mRotation = br.ReadSingle();
            mHasAngularVelocity = fA[3];
            if (fA[3])
                mAngularVelocity = br.ReadSingle();
            mHasScale = fA[5];
            if (fA[5])
                mScale = br.ReadSingle();
            mHasNormalDir = fA[1];
            if (fA[1])
                mNormalDir = br.ReadByte();
            if (fA[4])
            {
                ReadPosition(br);
            }

            int numPoints = br.ReadInt32();
            for (int i = 0; i < numPoints; i++)
            {
                mPoints.Add(new PointF(br.ReadSingle(), br.ReadSingle()));
            }

            mHasGrowTimer = fB[0];
            if (fB[0])
                mGrowTimer = br.ReadByte();

            mHasGrowType = fB[1];
            if (fB[1])
                mGrowType = br.ReadInt32();

            mHasFlashTimer = fB[2];
            if (fB[2])
                mFlashTimer = br.ReadInt16();
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup(mUnknownFlagsA);
            FlagGroup fB = new FlagGroup(mUnknownFlagsB);

            fA[1] = mHasNormalDir;
            fA[2] = mHasRotation;
            fA[3] = mHasAngularVelocity;
            fA[5] = mHasScale;
            fB[0] = mHasGrowTimer;
            fB[1] = mHasGrowType;
            fB[2] = mHasFlashTimer;

            if (!HasMover)
                fA[4] = true;

            //Write data

            bw.Write(fA.Int8);

            if (version >= 0x23)
                bw.Write(fB.Int8);

            if (fA[2])
                bw.Write(mRotation);
            if (fA[3])
                bw.Write(mAngularVelocity);
            if (fA[5])
                bw.Write(mScale);
            if (fA[1])
                bw.Write(mNormalDir);
            if (fA[4])
            {
                bw.Write(X);
                bw.Write(Y);
            }

            bw.Write(mPoints.Count);
            foreach (PointF p in mPoints)
            {
                bw.Write(p.X);
                bw.Write(p.Y);
            }

            if (version >= 0x23)
            {
                if (fB[0])
                    bw.Write(mGrowTimer);
                if (fB[1])
                    bw.Write(mGrowType);
                if (fB[2])
                    bw.Write(mFlashTimer);
            }
        }

        public override void Draw(Graphics g)
        {
            if (Level.ShowCollision && !Collidable)
                return;

            base.Draw(g);

            if (LevelPack.Current == null)
                return;

            var location = DrawLocation;
            var angle = 0.0f;
            if (AngleOverride is float overrideAngle)
            {
                angle = overrideAngle;
            }
            else if (MoverLink?.Mover is Mover movement)
            {
                // The game turns a moving poly as its mover does, whatever its own rotation
                angle = movement.GetEstimatedMoveAngle(angle);
            }
            else
            {
                angle = StaticAngle;
            }

            var backupMatrix = g.Transform;
            g.TranslateTransform(location.X, location.Y);
            g.RotateTransform(angle);

            var polygonImage = GetPolygonImage();
            if (polygonImage != null && (!Level.ShowPreview || Visible))
            {
                DrawObjectImage(g, polygonImage, PointF.Empty);
            }

            // In the game a poly with no image is drawn in its outline colour, so not at all when that is black
            bool hidden = Level.ShowPreview && !Level.ShowCollision && (!Visible || OutlineColor.ToArgb() == Color.Black.ToArgb());
            if ((polygonImage == null || !Level.ShowPreview || Level.ShowCollision) && !hidden)
            {
                if (Level.ShowCollision && MathExt.IsPolygonClosed(mPoints))
                {
                    g.FillPolygon(Brushes.White, mPoints.Take(mPoints.Count - 1).ToArray());
                }
                else
                {
                    for (var i = 0; i < mPoints.Count - 1; i++)
                    {
                        var p0 = mPoints[i];
                        var p1 = mPoints[i + 1];
                        g.DrawLine(Pens.White, p0, p1);
                    }
                }
            }

            g.Transform = backupMatrix;

            if (MouseOver || Selected)
            {
                var anchorOutline = new Pen(Color.FromArgb(0x23, 0x53, 0xDC));
                var anchorBrush = new SolidBrush(Color.FromArgb(196, 0x23, 0xB0, 0xDC));
                foreach (var p in mPoints)
                {
                    g.DrawSquare(anchorOutline, anchorBrush, Turn(p).Add(DrawLocation), 8);
                }
            }
        }

        /// <summary>
        /// The angle the poly is drawn at, in degrees clockwise, instead of its mover's, e.g. a raised flipper.
        /// </summary>
        internal float? AngleOverride { get; set; }

        /// <summary>
        /// The angle a poly that does not move is turned by, in degrees clockwise: its rotation, which is anticlockwise.
        /// Its points, as edited and picked, are turned by it too.
        /// </summary>
        private float StaticAngle => MoverLink?.Mover == null && AngleOverride == null && mHasRotation ? -MathExt.ToDegrees(mRotation) : 0;

        /// <summary>
        /// Turns one of the poly's points (from its position) as it is drawn when it does not move.
        /// </summary>
        private PointF Turn(PointF p) => Turn(p, StaticAngle);

        private static PointF Turn(PointF p, float degrees)
        {
            if (degrees == 0)
                return p;
            double radians = degrees * Math.PI / 180;
            double cos = Math.Cos(radians), sin = Math.Sin(radians);
            return new PointF((float)((p.X * cos) - (p.Y * sin)), (float)((p.X * sin) + (p.Y * cos)));
        }

        public Image GetPolygonImage()
        {
            return LevelPack.Current.GetImage(ImageFilename)?.Image;
        }

        public void SetPoints(PointF[] pnts)
        {
            mPoints = new List<PointF>(pnts);
        }

        public PointF[] GetPoints()
        {
            return mPoints.ToArray();
        }

        public override object Clone()
        {
            Poly cpyPolygon = new Poly(Level);
            base.CloneTo(cpyPolygon);

            cpyPolygon.mPoints = new List<PointF>(mPoints.ToArray());
            cpyPolygon.mNormalDir = mNormalDir;
            cpyPolygon.mRotation = mRotation;
            cpyPolygon.mScale = mScale;
            cpyPolygon.mGrowType = mGrowType;
            cpyPolygon.mUnknownFlagsA = mUnknownFlagsA;
            cpyPolygon.mUnknownFlagsB = mUnknownFlagsB;
            cpyPolygon.mHasRotation = mHasRotation;
            cpyPolygon.mHasAngularVelocity = mHasAngularVelocity;
            cpyPolygon.mAngularVelocity = mAngularVelocity;
            cpyPolygon.mHasScale = mHasScale;
            cpyPolygon.mHasNormalDir = mHasNormalDir;
            cpyPolygon.mHasGrowTimer = mHasGrowTimer;
            cpyPolygon.mGrowTimer = mGrowTimer;
            cpyPolygon.mHasGrowType = mHasGrowType;
            cpyPolygon.mHasFlashTimer = mHasFlashTimer;
            cpyPolygon.mFlashTimer = mFlashTimer;

            return cpyPolygon;
        }

        public override int Type
        {
            get
            {
                return LevelEntryTypes.Poly;
            }
        }

        public override RectangleF Bounds
        {
            get
            {
                float left = float.MaxValue;
                float top = float.MaxValue;
                float right = float.MinValue;
                float bottom = float.MinValue;
                foreach (PointF point in mPoints)
                {
                    var pnt = Turn(point);
                    left = Math.Min(pnt.X, left);
                    top = Math.Min(pnt.Y, top);
                    right = Math.Max(pnt.X, right);
                    bottom = Math.Max(pnt.Y, bottom);
                }

                PointF location = DrawLocation;
                return RectangleF.FromLTRB(left + location.X, top + location.Y, right + location.X, bottom + location.Y);
            }
        }

        [Category("Behaviour")]
        [EditorAttribute(typeof(PolyPointsUITypeEditor), typeof(UITypeEditor))]
        public PointF[] Points
        {
            get
            {
                return mPoints.ToArray();
            }
            set
            {
                mPoints = new List<PointF>(value);
            }
        }

        [DisplayName("Normal Dir")]
        [Description("Which side of the poly the ball is kept on: Keep Out keeps the ball outside, Keep In keeps it inside, Both collides on both sides.")]
        [Category("Behaviour")]
        [DefaultValue(PolyNormalDir.KeepOut)]
        public PolyNormalDir NormalDir
        {
            get
            {
                return mHasNormalDir ? (PolyNormalDir)(sbyte)mNormalDir : PolyNormalDir.KeepOut;
            }
            set
            {
                mNormalDir = (byte)(sbyte)value;
                mHasNormalDir |= value != PolyNormalDir.KeepOut;
            }
        }

        [DisplayName("Rotation")]
        [Description("The rotation of the poly in degrees, anticlockwise around its position, used when it has no movement. A moving poly is turned as its movement turns it instead.")]
        [Category("Layout")]
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
                mHasRotation |= value != 0;
            }
        }

        [DisplayName("Scale")]
        [Description("The scale the poly was last given in PopCap's level editor, which changed its points to that size. The game does not use it when it draws the poly or the ball hits it, as the points are already that size.")]
        [Category("Layout")]
        [DefaultValue(1.0f)]
        public float Scale
        {
            get
            {
                return mHasScale ? mScale : 1.0f;
            }
            set
            {
                mScale = value;
                mHasScale |= value != 1.0f;
            }
        }

        [DisplayName("Grow Type")]
        [Description("How the poly appears and disappears when it is shown or hidden: 0 none, 1 reveal left to right, 2 reveal right to left, 3 move left to right, 4 move right to left, 5 from the middle out. Add 0x1000 (4096) to flash when it has finished. Only stored from file version 0x23 (Peggle).")]
        [Category("Behaviour")]
        [DefaultValue(0)]
        public int GrowType
        {
            get
            {
                return mGrowType;
            }
            set
            {
                mGrowType = value;
                mHasGrowType |= value != 0;
            }
        }

        [DisplayName("Mirror")]
        [Description("Whether the image is drawn mirrored.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool Mirror
        {
            get
            {
                return (mUnknownFlagsA & (1 << 7)) != 0;
            }
            set
            {
                mUnknownFlagsA = value ? mUnknownFlagsA | (1 << 7) : mUnknownFlagsA & ~(1 << 7);
            }
        }

        [DisplayName("Image Tile")]
        [Description("Whether the image is tiled across the poly.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool ImageTile
        {
            get
            {
                return (mUnknownFlagsA & (1 << 6)) != 0;
            }
            set
            {
                mUnknownFlagsA = value ? mUnknownFlagsA | (1 << 6) : mUnknownFlagsA & ~(1 << 6);
            }
        }

        public override bool HitTest(RectangleF rect)
        {
            rect.Offset(-Location.X, -Location.Y);
            rect.Inflate(8, 8);
            return MathExt.RectIntersectsPolygon(rect, StaticAngle == 0 ? mPoints : mPoints.Select(p => Turn(p)).ToList());
        }

        public int InteractionPointCount => mPoints.Count;
        public PointF GetInteractionPoint(int index) => Turn(mPoints[index]).Add(Location);
        public void SetInteractionPoint(int index, PointF value) => mPoints[index] = Turn(value.Subtract(Location), -StaticAngle);
    }
}

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
    /// The shape of a brick, from brick_type in PopCap's level editor.
    /// </summary>
    public enum BrickType : byte
    {
        NormalCurved,
        OuterCurved,
        OuterCurved90,
        InnerCurved,
        InnerCurved90,
        Straight,
    }

    /// <summary>
    /// Which ends of a brick are anchored, from brick_anchor in PopCap's level editor.
    /// </summary>
    public enum BrickAnchor
    {
        None,
        Left,
        Right,
        Both,
    }

    /// <summary>
    /// Represents the brick level entry.
    /// </summary>
    public class Brick : LevelEntry, ICloneable
    {
        private byte mType;
        private float mAngle;
        private float mThickness;
        private float mRadius;
        private float mRotation;

        private int mCurvePoints;
        private bool mTextureFlip;

        // Unknown values, preserved so that they are written back as read
        private int mUnknownFlagsA;
        private int mUnknownFlagsB;
        private int mUnknownFlagsC;
        private bool mHasType;
        private bool mHasCurvePoints;
        private bool mHasAngle;
        private bool mHasThickness = true;
        private bool mHasLeftAngle;
        private float mLeftAngle;
        private bool mHasRightAngle;
        private float mRightAngle;
        private float mRightAngle2;
        private bool mHasPolyRotation;
        private float mPolyRotation;
        private bool mHasAngularVelocity;
        private float mAngularVelocity;
        private bool mHasScale;
        private float mScale;
        private bool mHasNormalDir;
        private byte mNormalDir;
        private bool mHasGrowTimer;
        private byte mGrowTimer;
        private bool mHasGrowType;
        private int mGrowType;
        private bool mHasFlashTimer;
        private short mFlashTimer;
        private bool mHasPivotOffsetX;
        private float mPivotOffsetX;
        private bool mHasPivotOffsetY;
        private float mPivotOffsetY;
        private uint mOutlineIndices;

        public Brick(Level level)
            : base(level)
        {
            PegInfo = new PegInfo(this, true, false);

            mThickness = 20.0f;
            mRadius = 30.0f;
            mCurvePoints = 4;
            mType = (byte)BrickType.Straight;
        }

        public override void ReadData(BinaryReader br, int version)
        {
            FlagGroup fA = new FlagGroup(br.ReadByte());
            FlagGroup fB = new FlagGroup();
            if (version >= 0x23)
                fB = new FlagGroup(br.ReadByte());
            mUnknownFlagsA = fA.Int32 & ((1 << 0) | (1 << 6) | (1 << 7));
            mUnknownFlagsB = fB.Int32 & ~((1 << 0) | (1 << 1) | (1 << 2));
            mHasPolyRotation = fA[2];
            if (fA[2])
                mPolyRotation = br.ReadSingle();
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

            //if arg0 (lots)

            mHasGrowTimer = fB[0];
            if (fB[0])
                mGrowTimer = br.ReadByte();
            mHasGrowType = fB[1];
            if (fB[1])
                mGrowType = br.ReadInt32();
            mHasFlashTimer = fB[2];
            if (fB[2])
                mFlashTimer = br.ReadInt16();

            // The game's default when the type is not stored
            mType = (byte)BrickType.NormalCurved;

            FlagGroup fC = new FlagGroup(br.ReadInt16());
            mUnknownFlagsC = fC.Int32 & ((1 << 0) | (1 << 1) | (0x1F << 11));
            mHasPivotOffsetX = fC[8];
            if (fC[8])
                mPivotOffsetX = br.ReadSingle();
            mHasPivotOffsetY = fC[9];
            if (fC[9])
                mPivotOffsetY = br.ReadSingle();
            mHasType = fC[2];
            if (fC[2])
            {
                mType = br.ReadByte();
            }
            mHasCurvePoints = fC[3];
            if (fC[3])
            {
                mCurvePoints = br.ReadByte() + 2;
            }

            mHasLeftAngle = fC[5];
            if (fC[5])
            {
                mLeftAngle = br.ReadSingle();
            }
            mHasRightAngle = fC[6];
            if (fC[6])
            {
                mRightAngle = br.ReadSingle();
                mRightAngle2 = br.ReadSingle();
            }

            mHasAngle = fC[4];
            if (fC[4])
                mAngle = br.ReadSingle();

            mHasThickness = fC[7];
            if (fC[7])
                mThickness = br.ReadSingle();

            mTextureFlip = fC[10];

            mRadius = br.ReadSingle();
            mRotation = br.ReadSingle();

            mOutlineIndices = br.ReadUInt32();
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup(mUnknownFlagsA);
            FlagGroup fB = new FlagGroup(mUnknownFlagsB);
            FlagGroup fC = new FlagGroup(mUnknownFlagsC);

            fA[1] = mHasNormalDir;
            fA[2] = mHasPolyRotation;
            fA[3] = mHasAngularVelocity;
            fA[5] = mHasScale;
            fB[0] = mHasGrowTimer;
            fB[1] = mHasGrowType;
            fB[2] = mHasFlashTimer;
            fC[8] = mHasPivotOffsetX;
            fC[9] = mHasPivotOffsetY;

            if (!HasMover)
                fA[4] = true;

            fC[5] = mHasLeftAngle;
            fC[6] = mHasRightAngle;

            // Keep the curve values that were in the file, otherwise only write them if needed
            fC[3] = mHasCurvePoints;
            fC[4] = mHasAngle;
            if (Curved)
            {
                if (mAngle != 0.0f)
                    fC[4] = true;

                if (mCurvePoints != 4)
                    fC[3] = true;
            }

            // The type is only needed when it is not the game's default, normal curved
            fC[2] = mHasType || mType != (byte)BrickType.NormalCurved;

            // Only write the width if it was in the file or has been changed from the default
            fC[7] = (mHasThickness || mThickness != 20.0f);

            if (mTextureFlip)
                fC[10] = true;

            //Write data

            bw.Write(fA.Int8);

            if (version >= 0x23)
                bw.Write(fB.Int8);

            if (fA[2])
                bw.Write(mPolyRotation);
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

            if (version >= 0x23)
            {
                if (fB[0])
                    bw.Write(mGrowTimer);
                if (fB[1])
                    bw.Write(mGrowType);
                if (fB[2])
                    bw.Write(mFlashTimer);
            }

            bw.Write(fC.Int16);

            if (fC[8])
                bw.Write(mPivotOffsetX);
            if (fC[9])
                bw.Write(mPivotOffsetY);
            if (fC[2])
            {
                bw.Write(mType);
            }

            if (fC[3])
            {
                bw.Write((byte)(mCurvePoints - 2));
            }

            if (fC[5])
                bw.Write(mLeftAngle);
            if (fC[6])
            {
                bw.Write(mRightAngle);
                bw.Write(mRightAngle2);
            }

            if (fC[4])
                bw.Write(mAngle);
            if (fC[7])
                bw.Write(mThickness);

            bw.Write(mRadius);
            bw.Write(mRotation);

            bw.Write(mOutlineIndices);
        }

        // The game's brick shadows, measured against Peggle Nights screenshots
        private static readonly Point ShadowOffset = new Point(-4, 2);
        private static readonly Color ShadowColour = Color.FromArgb(144, Color.Black);

        public override void Draw(Graphics g) => Draw(g, false);
        public override void RenderShadow(Graphics g) => Draw(g, true);

        private void Draw(Graphics g, bool shadow)
        {
            if (shadow && Level.ShowCollision)
                return;

            // Show only if its collidable
            if (!HasPegInfo)
            {
                if (Level.ShowCollision && !Collidable)
                    return;
            }

            base.Draw(g);

            //Get location and angle
            PointF location = DrawLocation;
            float angle = mRotation;
            if (MoverLink?.Mover is Mover movement)
            {
                angle = movement.GetEstimatedMoveAngle(mRotation);
            }

            if (Level.UsePegTextures)
            {
                // HACK 90 degree rotations seem to use a different rendering algorithm
                //      that makes them appear out of place with other rotations
                if (angle == (int)angle && ((int)angle % 90) == 0)
                {
                    angle += 0.0001f;
                }
            }

            var backupTransform = g.Transform;
            var mx = g.Transform;
            if (shadow)
                mx.Translate(ShadowOffset.X, ShadowOffset.Y);
            if (Curved)
            {
                mx.RotateAt(-angle, location);
                g.Transform = mx;
                DrawCurvedBrick(g, location, shadow);
            }
            else
            {
                mx.RotateAt(-angle + 90.0f, new PointF(location.X, location.Y));
                g.Transform = mx;
                DrawStrightBrick(g, location, shadow);
            }
            g.Transform = backupTransform;

            if (!shadow && Curved)
                DrawCircleGuide(g, angle);
        }

        private Color GetOuterColour() => HasPegInfo ? PegInfo.GetOuterColour() : Color.Gray;
        private Color GetInnerColour() => HasPegInfo ? PegInfo.GetInnerColour() : Color.DimGray;

        private void DrawStrightBrick(Graphics g, PointF location, bool shadow)
        {
            var dest = new RectangleF(location.X - (Radius / 2), location.Y - (Thickness / 2), Radius, Thickness);
            if (shadow)
            {
                g.FillRectangle(new SolidBrush(ShadowColour), dest);
            }
            else if (Level.ShowCollision)
            {
                g.FillRectangle(Brushes.White, dest);
            }
            else if (!Level.UsePegTextures || !HasPegInfo)
            {
                g.FillRectangle(new SolidBrush(GetOuterColour()), dest);
                dest.Inflate(-2, -5);
                dest.Y += mTextureFlip ? 3.0f : -3.0f;
                g.FillRectangle(new SolidBrush(GetInnerColour()), dest);
            }
            else
            {
                BrickImage.Draw(g, BrickData, location);
            }

            if (Provisional)
                g.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), dest);
        }

        private void DrawCurvedBrick(Graphics g, PointF location, bool shadow)
        {
            if (shadow)
            {
                DrawCurvedBrick(g, ShadowColour, location, true);
            }
            else if (Level.ShowCollision)
            {
                DrawCurvedBrick(g, Color.White, location, false);
            }
            else if (!Level.UsePegTextures || !HasPegInfo)
            {
                var shadingOffset = mTextureFlip ? 2 : 8;
                DrawCurvedBrick(g, GetOuterColour(), location, false);
                mThickness /= 2.0f;
                location.X += shadingOffset;
                DrawCurvedBrick(g, GetInnerColour(), location, true);
                location.X -= shadingOffset;
                mThickness *= 2.0f;
            }
            else
            {
                BrickImage.Draw(g, BrickData, location);
            }

            if (Provisional)
                DrawCurvedBrick(g, Color.FromArgb(128, 255, 255, 255), location, false);
        }

        private void DrawCurvedBrick(Graphics g, Color c, PointF location, bool inner)
        {
            PointF offset = new PointF(-10, 0);

            location.X += offset.X;
            location.Y += offset.Y;

            float hangle = mAngle / 2.0f;
            float hhangle = (mAngle / 2.0f) * (0.4f);
            float width = mThickness;
            float inner_radius = mRadius;
            float outer_radius = inner_radius + mThickness;
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

            float div_angle = mAngle / (mCurvePoints - 1);
            float cur_angle = -(mAngle / 2.0f);

            PointF[] o_pnts = new PointF[mCurvePoints];
            PointF[] i_pnts = new PointF[mCurvePoints];

            for (int i = 0; i < mCurvePoints; i++)
            {
                o_pnts[i] = GetBrickAngularPoint(circleCentre, cur_angle, outer_radius);
                i_pnts[i] = GetBrickAngularPoint(circleCentre, cur_angle, inner_radius);

                cur_angle += div_angle;
            }

            PointF[] pnts = new PointF[o_pnts.Length + i_pnts.Length];
            Array.Copy(o_pnts, 0, pnts, 0, o_pnts.Length);

            //Inner points need to be reversed
            for (int i = 0; i < i_pnts.Length; i++)
            {
                pnts[pnts.Length - i - 1] = i_pnts[i];
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

        private void DrawCircleGuide(Graphics g, float rotation)
        {
            var mouseOverOnly = true;
            var drawInnerCircle = true;
            var drawOuterCircle = true;
            if (!mouseOverOnly || (mouseOverOnly && MouseOver))
            {
                var circleCentre = GetOrigin(DrawLocation, rotation);
                var innerRadius = InnerRadius;
                var outerRadius = OuterRadius;
                if (drawInnerCircle)
                    g.DrawEllipse(Pens.Magenta, circleCentre.X - innerRadius, circleCentre.Y - innerRadius, innerRadius * 2, innerRadius * 2);
                if (drawOuterCircle)
                    g.DrawEllipse(Pens.White, circleCentre.X - outerRadius, circleCentre.Y - outerRadius, outerRadius * 2, outerRadius * 2);
            }
        }

        private PointF GetBrickAngularPoint(PointF circleCentre, float angle, float radius)
        {
            return new PointF((float)Math.Cos(MathExt.ToRadians(angle)) * radius + circleCentre.X,
                (float)Math.Sin(MathExt.ToRadians(angle)) * radius + circleCentre.Y);
        }

        public PointF GetCentrePoint()
        {
            PointF cc = new PointF();
            cc.X = X - (mRadius * (float)Math.Cos(MathExt.ToRadians(mRotation)));
            cc.Y = Y - (mRadius * (float)Math.Sin(MathExt.ToRadians(mRotation)));
            return cc;
        }

        public override object Clone()
        {
            Brick cpyBrick = new Brick(Level);
            base.CloneTo(cpyBrick);

            cpyBrick.mType = mType;
            cpyBrick.mAngle = mAngle;
            cpyBrick.mThickness = mThickness;
            cpyBrick.mRadius = mRadius;
            cpyBrick.mRotation = mRotation;

            cpyBrick.mCurvePoints = mCurvePoints;
            cpyBrick.mTextureFlip = mTextureFlip;

            cpyBrick.mHasType = mHasType;
            cpyBrick.mHasCurvePoints = mHasCurvePoints;
            cpyBrick.mHasAngle = mHasAngle;
            cpyBrick.mHasThickness = mHasThickness;
            cpyBrick.mHasLeftAngle = mHasLeftAngle;
            cpyBrick.mLeftAngle = mLeftAngle;
            cpyBrick.mHasRightAngle = mHasRightAngle;
            cpyBrick.mRightAngle = mRightAngle;
            cpyBrick.mRightAngle2 = mRightAngle2;
            cpyBrick.mUnknownFlagsA = mUnknownFlagsA;
            cpyBrick.mUnknownFlagsB = mUnknownFlagsB;
            cpyBrick.mUnknownFlagsC = mUnknownFlagsC;
            cpyBrick.mHasPolyRotation = mHasPolyRotation;
            cpyBrick.mPolyRotation = mPolyRotation;
            cpyBrick.mHasAngularVelocity = mHasAngularVelocity;
            cpyBrick.mAngularVelocity = mAngularVelocity;
            cpyBrick.mHasScale = mHasScale;
            cpyBrick.mScale = mScale;
            cpyBrick.mHasNormalDir = mHasNormalDir;
            cpyBrick.mNormalDir = mNormalDir;
            cpyBrick.mHasGrowTimer = mHasGrowTimer;
            cpyBrick.mGrowTimer = mGrowTimer;
            cpyBrick.mHasGrowType = mHasGrowType;
            cpyBrick.mGrowType = mGrowType;
            cpyBrick.mHasFlashTimer = mHasFlashTimer;
            cpyBrick.mFlashTimer = mFlashTimer;
            cpyBrick.mHasPivotOffsetX = mHasPivotOffsetX;
            cpyBrick.mPivotOffsetX = mPivotOffsetX;
            cpyBrick.mHasPivotOffsetY = mHasPivotOffsetY;
            cpyBrick.mPivotOffsetY = mPivotOffsetY;
            cpyBrick.mOutlineIndices = mOutlineIndices;

            return cpyBrick;
        }

        private float GetHeight()
        {
            return (2 * (float)Math.Sin((Angle / 2) / 180.0 * Math.PI) * Radius);
        }

        private PointF GetOrigin(PointF location, float rotation)
        {
            var radius = MidRadius;
            var angle = MathExt.ToRadians(-rotation);
            var originDelta = new PointF(
                (float)(Math.Cos(angle) * radius),
                (float)(Math.Sin(angle) * radius));
            return location.Subtract(originDelta);
        }

        private BrickData BrickData
        {
            get
            {
                var brickData = new BrickData();
                if (!Level.ShowPreview && HasPegInfo)
                {
                    brickData.CanBeOrange = PegInfo.Variable;
                    brickData.QuickDisappear = PegInfo.Crumble;
                }
                brickData.TextureFlip = TextureFlip;
                brickData.Length = Radius;
                brickData.Width = Thickness;
                if (Curved)
                {
                    brickData.Curved = true;
                    brickData.SectorAngle = Angle;
                    brickData.CurvePoints = CurvePoints;
                }
                return brickData;
            }
        }

        public float MidRadius
        {
            get => InnerRadius + (Thickness / 2);
            set => InnerRadius = value - (Thickness / 2);
        }

        public PointF Origin
        {
            get
            {
                return GetOrigin(Location, Rotation);
            }
            set
            {
                var delta = Location.Subtract(Origin);
                Location = value.Add(delta);
            }
        }

        public float LeftSideAngle
        {
            get
            {
                if (Curved)
                    return MathExt.FixAngle((360 - Rotation) - 90 - (Angle / 2));
                else
                    return MathExt.FixAngle(360 - Rotation - 90);
            }
            set
            {
                if (Curved)
                    Rotation = MathExt.FixAngle(360 + 90 - (Angle / 2) - value);
                else
                    Rotation = MathExt.FixAngle(360 - 90 - value);
            }
        }

        public PointF LeftSidePosition
        {
            get
            {
                if (Curved)
                {
                    var angle = MathExt.ToRadians(-Rotation - (Angle / 2));
                    var radius = MidRadius;
                    return Origin.Add(new PointF(
                        (float)(radius * Math.Cos(angle)),
                        (float)(radius * Math.Sin(angle))));
                }
                else
                {
                    var delta = new PointF(
                        (float)((Radius / 2) * Math.Cos(MathExt.ToRadians(LeftSideAngle))),
                        (float)((Radius / 2) * Math.Sin(MathExt.ToRadians(LeftSideAngle))));
                    return Location.Add(delta);
                }
            }
            set
            {
                var delta = Location.Subtract(LeftSidePosition);
                Location = value.Add(delta);
            }
        }

        public float RightSideAngle
        {
            get
            {
                if (Curved)
                    return MathExt.FixAngle((360 - Rotation) + 90 + (Angle / 2));
                else
                    return MathExt.FixAngle(360 - Rotation - 90 + 180);
            }
            set
            {
                if (Curved)
                    Rotation = MathExt.FixAngle(90 + (Angle / 2) + 360 - value);
                else
                    Rotation = MathExt.FixAngle(360 - 90 + 180 - value);
            }
        }

        public PointF RightSidePosition
        {
            get
            {
                if (Curved)
                {
                    var angle = MathExt.ToRadians(-Rotation + (Angle / 2));
                    var radius = MidRadius;
                    return Origin.Add(new PointF(
                        (float)(radius * Math.Cos(angle)),
                        (float)(radius * Math.Sin(angle))));
                }
                else
                {
                    var delta = new PointF(
                        (float)((Radius / 2) * Math.Cos(MathExt.ToRadians(RightSideAngle))),
                        (float)((Radius / 2) * Math.Sin(MathExt.ToRadians(RightSideAngle))));
                    return Location.Add(delta);
                }
            }
            set
            {
                var delta = Location.Subtract(RightSidePosition);
                Location = value.Add(delta);
            }
        }

        [EntryProperty(EntryPropertyType.Element, 0.0f)]
        [Description("How much the brick has been rotated in degrees.")]
        [Category("Layout")]
        [DefaultValue(90.0f)]
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

        [EntryProperty(EntryPropertyType.Element, 0.0f)]
        [DisplayName("Angle")]
        [Description("The angle of the arc of a curved brick in degrees.")]
        [Category("Layout")]
        [DefaultValue(0.0f)]
        public float Angle
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
        [DisplayName("Radius")]
        [Description("The radius of a curved brick, or the length of a straight brick.")]
        [Category("Layout")]
        [DefaultValue(30.0f)]
        public float Radius
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

        [EntryProperty(EntryPropertyType.Element, 0.0f)]
        [DisplayName("Inner Radius")]
        [Description("The radius of the sector if the brick is curved or the length of the brick when the brick is straight.")]
        [Category("Layout")]
        [DefaultValue(30.0f)]
        public float InnerRadius
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

        [EntryProperty(EntryPropertyType.Element, 0.0f)]
        [DisplayName("Outer Radius")]
        [Description("The outer radius of the sector if the brick is curved.")]
        [Category("Layout")]
        [DefaultValue(50.0f)]
        public float OuterRadius
        {
            get
            {
                return mRadius + mThickness;
            }
            set
            {
                mRadius = value - mThickness;
            }
        }

        [EntryProperty(EntryPropertyType.Element, 0.0f)]
        [DisplayName("Thickness")]
        [Description("The thickness of the brick.")]
        [Category("Layout")]
        [DefaultValue(20.0f)]
        public float Thickness
        {
            get
            {
                return mThickness;
            }
            set
            {
                mThickness = value;
            }
        }

        [EntryProperty(EntryPropertyType.Element, 0)]
        [DisplayName("Curve Points")]
        [Description("The number of points along the curve: how round the brick is. Use a higher value for a larger angle. Stored as the number of subdivisions, which is this value minus 2 (PopCap: Subdivisions).")]
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

        [DisplayName("Type")]
        [Description("The shape of the brick: curved (normal, outer, inner, and the 90 variants) or straight. PeggleEdit draws all of the curved types like a normal curved brick.")]
        [Category("Layout")]
        public BrickType BrickType
        {
            get
            {
                return (BrickType)mType;
            }
            set
            {
                mType = (byte)value;
                if (Curved && mAngle == 0)
                {
                    mAngle = 30;
                }
            }
        }

        /// <summary>
        /// Whether the brick is any of the curved types rather than straight.
        /// </summary>
        [Browsable(false)]
        public bool Curved
        {
            get
            {
                return mType != (byte)BrickType.Straight;
            }
            set
            {
                if (value != Curved)
                {
                    mType = (byte)(value ? BrickType.NormalCurved : BrickType.Straight);
                }
                if (mAngle == 0)
                {
                    mAngle = 30;
                }
            }
        }

        [DisplayName("Anchor")]
        [Description("Which ends of the brick are anchored. Changing the thickness in PopCap's level editor clears the anchors.")]
        [Category("Layout")]
        [DefaultValue(BrickAnchor.None)]
        public BrickAnchor Anchor
        {
            get
            {
                return (BrickAnchor)(mUnknownFlagsC & 3);
            }
            set
            {
                mUnknownFlagsC = (mUnknownFlagsC & ~3) | ((int)value & 3);
            }
        }

        [DisplayName("Left Angle")]
        [Description("The angle of the left end of the brick. PeggleEdit does not show it when drawing the brick.")]
        [Category("Layout")]
        [DefaultValue(0.0f)]
        public float LeftAngle
        {
            get
            {
                return mLeftAngle;
            }
            set
            {
                mLeftAngle = value;
                mHasLeftAngle |= value != 0;
            }
        }

        [DisplayName("Right Angle")]
        [Description("The angle of the right end of the brick. PeggleEdit does not show it when drawing the brick.")]
        [Category("Layout")]
        [DefaultValue(0.0f)]
        public float RightAngle
        {
            get
            {
                return mRightAngle;
            }
            set
            {
                // The game stores the right angle twice
                mRightAngle = value;
                mRightAngle2 = value;
                mHasRightAngle |= value != 0;
            }
        }

        [EntryProperty(EntryPropertyType.Element, false)]
        [DisplayName("Texture Flip")]
        [Description("Whether to flip the brick texture (i.e. change which side the light reflection is). Used to keep the light reflection consistent along a path of bricks.")]
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
                return LevelEntryTypes.Brick;
            }
        }

        public override RectangleF Bounds
        {
            get
            {
                return new RectangleF(
                    DrawX - (mThickness / 2),
                    DrawY - (mThickness / 2),
                    mThickness, mThickness);
            }
        }
    }
}

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

        // Unknown values, preserved so that they are written back as read
        private int mUnknownFlagsA;
        private int mUnknownFlagsB;
        private int mUnknownFlagsC;
        private bool mHasType;
        private bool mHasCurvePoints;
        private bool mHasSectorAngle;
        private bool mHasWidth = true;
        private bool mHasUnknownLeftAngle;
        private float mUnknownLeftAngle;
        private bool mHasUnknownRightAngle;
        private float mUnknownRightAngle;
        private float mUnknownRightAngle2;
        private bool mHasUnknown0;
        private float mUnknown0;
        private bool mHasUnknown1;
        private float mUnknown1;
        private bool mHasUnknown2;
        private float mUnknown2;
        private bool mHasUnknown3;
        private byte mUnknown3;
        private bool mHasUnknown4;
        private byte mUnknown4;
        private bool mHasUnknown5;
        private int mUnknown5;
        private bool mHasUnknown6;
        private short mUnknown6;
        private bool mHasUnknown7;
        private float mUnknown7;
        private bool mHasUnknown8;
        private float mUnknown8;
        private uint mUnknown9;

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
            mUnknownFlagsA = fA.Int32 & ((1 << 0) | (1 << 6) | (1 << 7));
            mUnknownFlagsB = fB.Int32 & ~((1 << 0) | (1 << 1) | (1 << 2));
            mHasUnknown0 = fA[2];
            if (fA[2])
                mUnknown0 = br.ReadSingle();
            mHasUnknown1 = fA[3];
            if (fA[3])
                mUnknown1 = br.ReadSingle();
            mHasUnknown2 = fA[5];
            if (fA[5])
                mUnknown2 = br.ReadSingle();
            mHasUnknown3 = fA[1];
            if (fA[1])
                mUnknown3 = br.ReadByte();
            if (fA[4])
            {
                X = br.ReadSingle();
                Y = br.ReadSingle();
            }

            //if arg0 (lots)

            mHasUnknown4 = fB[0];
            if (fB[0])
                mUnknown4 = br.ReadByte();
            mHasUnknown5 = fB[1];
            if (fB[1])
                mUnknown5 = br.ReadInt32();
            mHasUnknown6 = fB[2];
            if (fB[2])
                mUnknown6 = br.ReadInt16();

            mCurved = true;

            FlagGroup fC = new FlagGroup(br.ReadInt16());
            mUnknownFlagsC = fC.Int32 & ((1 << 0) | (1 << 1) | (0x1F << 11));
            mHasUnknown7 = fC[8];
            if (fC[8])
                mUnknown7 = br.ReadSingle();
            mHasUnknown8 = fC[9];
            if (fC[9])
                mUnknown8 = br.ReadSingle();
            mHasType = fC[2];
            if (fC[2])
            {
                mType = br.ReadByte();
                if (mType == 5)
                    mCurved = false;
            }
            mHasCurvePoints = fC[3];
            if (fC[3])
            {
                mCurvePoints = br.ReadByte() + 2;
            }

            mHasUnknownLeftAngle = fC[5];
            if (fC[5])
            {
                mUnknownLeftAngle = br.ReadSingle();
            }
            mHasUnknownRightAngle = fC[6];
            if (fC[6])
            {
                mUnknownRightAngle = br.ReadSingle();
                mUnknownRightAngle2 = br.ReadSingle();
            }

            mHasSectorAngle = fC[4];
            if (fC[4])
                mSectorAngle = br.ReadSingle();

            mHasWidth = fC[7];
            if (fC[7])
                mWidth = br.ReadSingle();

            mTextureFlip = fC[10];

            mLength = br.ReadSingle();
            mAngle = br.ReadSingle();

            mUnknown9 = br.ReadUInt32();
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup(mUnknownFlagsA);
            FlagGroup fB = new FlagGroup(mUnknownFlagsB);
            FlagGroup fC = new FlagGroup(mUnknownFlagsC);

            fA[1] = mHasUnknown3;
            fA[2] = mHasUnknown0;
            fA[3] = mHasUnknown1;
            fA[5] = mHasUnknown2;
            fB[0] = mHasUnknown4;
            fB[1] = mHasUnknown5;
            fB[2] = mHasUnknown6;
            fC[8] = mHasUnknown7;
            fC[9] = mHasUnknown8;

            if (!HasMovementInfo)
                fA[4] = true;

            fC[5] = mHasUnknownLeftAngle;
            fC[6] = mHasUnknownRightAngle;

            // Keep the curve values that were in the file, otherwise only write them if needed
            fC[3] = mHasCurvePoints;
            fC[4] = mHasSectorAngle;
            if (mCurved)
            {
                if (mSectorAngle != 0.0f)
                    fC[4] = true;

                if (mCurvePoints != 4)
                    fC[3] = true;

                if (mHasType && mType != 5)
                    fC[2] = true;
            }
            else
            {
                fC[2] = true;
            }

            // Only write the width if it was in the file or has been changed from the default
            fC[7] = (mHasWidth || mWidth != 20.0f);

            if (mTextureFlip)
                fC[10] = true;

            //Write data

            bw.Write(fA.Int8);

            if (version >= 0x23)
                bw.Write(fB.Int8);

            if (fA[2])
                bw.Write(mUnknown0);
            if (fA[3])
                bw.Write(mUnknown1);
            if (fA[5])
                bw.Write(mUnknown2);
            if (fA[1])
                bw.Write(mUnknown3);
            if (fA[4])
            {
                bw.Write(X);
                bw.Write(Y);
            }

            if (version >= 0x23)
            {
                if (fB[0])
                    bw.Write(mUnknown4);
                if (fB[1])
                    bw.Write(mUnknown5);
                if (fB[2])
                    bw.Write(mUnknown6);
            }

            bw.Write(fC.Int16);

            if (fC[8])
                bw.Write(mUnknown7);
            if (fC[9])
                bw.Write(mUnknown8);
            if (fC[2])
            {
                bw.Write(mCurved ? mType : (byte)5);
            }

            if (fC[3])
            {
                bw.Write((byte)(mCurvePoints - 2));
            }

            if (fC[5])
                bw.Write(mUnknownLeftAngle);
            if (fC[6])
            {
                bw.Write(mUnknownRightAngle);
                bw.Write(mUnknownRightAngle2);
            }

            if (fC[4])
                bw.Write(mSectorAngle);
            if (fC[7])
                bw.Write(mWidth);

            bw.Write(mLength);
            bw.Write(mAngle);

            bw.Write(mUnknown9);
        }

        public override void Draw(Graphics g) => Draw(g, false);
        public override void DrawShadow(Graphics g) => Draw(g, true);

        private void Draw(Graphics g, bool shadow)
        {
            if (shadow && Level.ShowCollision)
                return;

            // Show only if its collidable
            if (!HasPegInfo)
            {
                if (Level.ShowCollision && !Collision)
                    return;
            }

            base.Draw(g);

            //Get location and angle
            PointF location = DrawLocation;
            float angle = mAngle;
            if (MovementLink?.Movement is Movement movement)
            {
                angle = movement.GetEstimatedMoveAngle(mAngle);
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
                mx.Translate(Level.ShadowOffset.X, Level.ShadowOffset.Y);
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
            var dest = new RectangleF(location.X - (Length / 2), location.Y - (Width / 2), Length, Width);
            if (shadow)
            {
                g.FillRectangle(new SolidBrush(Level.ShadowColour), dest);
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
                var brick = BrickImage.GetBrickImage(BrickData);
                g.DrawImage(brick, location.X - (brick.Width / 2), location.Y - (brick.Height / 2));
            }

            if (Provisional)
                g.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), dest);
        }

        private void DrawCurvedBrick(Graphics g, PointF location, bool shadow)
        {
            if (shadow)
            {
                DrawCurvedBrick(g, Level.ShadowColour, location, true);
            }
            else if (Level.ShowCollision)
            {
                DrawCurvedBrick(g, Color.White, location, false);
            }
            else if (!Level.UsePegTextures || !HasPegInfo)
            {
                var shadingOffset = mTextureFlip ? 2 : 8;
                DrawCurvedBrick(g, GetOuterColour(), location, false);
                mWidth /= 2.0f;
                location.X += shadingOffset;
                DrawCurvedBrick(g, GetInnerColour(), location, true);
                location.X -= shadingOffset;
                mWidth *= 2.0f;
            }
            else
            {
                var brick = BrickImage.GetBrickImage(BrickData);
                g.DrawImage(brick, location.X - (brick.Width / 2), location.Y - (brick.Height / 2));
            }

            if (Provisional)
                DrawCurvedBrick(g, Color.FromArgb(128, 255, 255, 255), location, false);
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

            cpyBrick.mHasType = mHasType;
            cpyBrick.mHasCurvePoints = mHasCurvePoints;
            cpyBrick.mHasSectorAngle = mHasSectorAngle;
            cpyBrick.mHasWidth = mHasWidth;
            cpyBrick.mHasUnknownLeftAngle = mHasUnknownLeftAngle;
            cpyBrick.mUnknownLeftAngle = mUnknownLeftAngle;
            cpyBrick.mHasUnknownRightAngle = mHasUnknownRightAngle;
            cpyBrick.mUnknownRightAngle = mUnknownRightAngle;
            cpyBrick.mUnknownRightAngle2 = mUnknownRightAngle2;
            cpyBrick.mUnknownFlagsA = mUnknownFlagsA;
            cpyBrick.mUnknownFlagsB = mUnknownFlagsB;
            cpyBrick.mUnknownFlagsC = mUnknownFlagsC;
            cpyBrick.mHasUnknown0 = mHasUnknown0;
            cpyBrick.mUnknown0 = mUnknown0;
            cpyBrick.mHasUnknown1 = mHasUnknown1;
            cpyBrick.mUnknown1 = mUnknown1;
            cpyBrick.mHasUnknown2 = mHasUnknown2;
            cpyBrick.mUnknown2 = mUnknown2;
            cpyBrick.mHasUnknown3 = mHasUnknown3;
            cpyBrick.mUnknown3 = mUnknown3;
            cpyBrick.mHasUnknown4 = mHasUnknown4;
            cpyBrick.mUnknown4 = mUnknown4;
            cpyBrick.mHasUnknown5 = mHasUnknown5;
            cpyBrick.mUnknown5 = mUnknown5;
            cpyBrick.mHasUnknown6 = mHasUnknown6;
            cpyBrick.mUnknown6 = mUnknown6;
            cpyBrick.mHasUnknown7 = mHasUnknown7;
            cpyBrick.mUnknown7 = mUnknown7;
            cpyBrick.mHasUnknown8 = mHasUnknown8;
            cpyBrick.mUnknown8 = mUnknown8;
            cpyBrick.mUnknown9 = mUnknown9;

            return cpyBrick;
        }

        private float GetHeight()
        {
            return (2 * (float)Math.Sin((SectorAngle / 2) / 180.0 * Math.PI) * Length);
        }

        private PointF GetOrigin(PointF location, float rotation)
        {
            var radius = Radius;
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
                    brickData.CanBeOrange = PegInfo.CanBeOrange;
                    brickData.QuickDisappear = PegInfo.QuickDisappear;
                }
                brickData.TextureFlip = TextureFlip;
                brickData.Length = Length;
                brickData.Width = Width;
                if (Curved)
                {
                    brickData.Curved = true;
                    brickData.SectorAngle = SectorAngle;
                }
                return brickData;
            }
        }

        public float Radius
        {
            get => InnerRadius + (Width / 2);
            set => InnerRadius = value - (Width / 2);
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
                    return MathExt.FixAngle((360 - Rotation) - 90 - (SectorAngle / 2));
                else
                    return MathExt.FixAngle(360 - Rotation - 90);
            }
            set
            {
                if (Curved)
                    Rotation = MathExt.FixAngle(360 + 90 - (SectorAngle / 2) - value);
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
                    var angle = MathExt.ToRadians(-Rotation - (SectorAngle / 2));
                    var radius = Radius;
                    return Origin.Add(new PointF(
                        (float)(radius * Math.Cos(angle)),
                        (float)(radius * Math.Sin(angle))));
                }
                else
                {
                    var delta = new PointF(
                        (float)((Length / 2) * Math.Cos(MathExt.ToRadians(LeftSideAngle))),
                        (float)((Length / 2) * Math.Sin(MathExt.ToRadians(LeftSideAngle))));
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
                    return MathExt.FixAngle((360 - Rotation) + 90 + (SectorAngle / 2));
                else
                    return MathExt.FixAngle(360 - Rotation - 90 + 180);
            }
            set
            {
                if (Curved)
                    Rotation = MathExt.FixAngle(90 + (SectorAngle / 2) + 360 - value);
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
                    var angle = MathExt.ToRadians(-Rotation + (SectorAngle / 2));
                    var radius = Radius;
                    return Origin.Add(new PointF(
                        (float)(radius * Math.Cos(angle)),
                        (float)(radius * Math.Sin(angle))));
                }
                else
                {
                    var delta = new PointF(
                        (float)((Length / 2) * Math.Cos(MathExt.ToRadians(RightSideAngle))),
                        (float)((Length / 2) * Math.Sin(MathExt.ToRadians(RightSideAngle))));
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
                if (mSectorAngle == 0)
                {
                    mSectorAngle = 30;
                }
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
                return LevelEntryTypes.Brick;
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

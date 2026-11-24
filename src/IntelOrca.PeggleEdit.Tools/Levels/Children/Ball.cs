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
using IntelOrca.PeggleEdit.Tools.Extensions;
using IntelOrca.PeggleEdit.Tools.Pack;
using IntelOrca.PeggleEdit.Tools.Properties;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// Represents the circle / peg level entry.
    /// </summary>
    public class Ball : LevelEntry, ICloneable
    {
        private float mRadius;

        // Unknown flags, preserved so that they are written back as read
        private int mUnknownFlagsA = 1 << 0; // Make it bounce
        private int mUnknownFlagsB;

        public Ball(Level level)
            : base(level)
        {
            mRadius = 10.0f;
        }

        public override void ReadData(BinaryReader br, int version)
        {
            FlagGroup fA = new FlagGroup(br.ReadByte());
            mUnknownFlagsA = fA.Int32 & ~(1 << 1);

            if (version >= 0x52)
            {
                FlagGroup fB = new FlagGroup(br.ReadByte());
                mUnknownFlagsB = fB.Int32;
            }

            if (fA[1])
            {
                ReadPosition(br);
            }


            mRadius = br.ReadSingle();
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup(mUnknownFlagsA);
            FlagGroup fB = new FlagGroup(mUnknownFlagsB);

            if (!HasMover)
                fA[1] = true;

            bw.Write(fA.Int8);

            if (version >= 0x52)
            {
                bw.Write(fB.Int8);
            }

            if (fA[1])
            {
                bw.Write(X);
                bw.Write(Y);
            }

            bw.Write(mRadius);
        }

        public override void RenderShadow(Graphics g)
        {
            if (Level.ShowCollision)
                return;
            if (!HasPegInfo && Level.ShowPreview && !Visible)
                return;

            PointF location = DrawLocation;
            if (MoverLink?.Mover is Mover movement)
            {
                location = movement.GetEstimatedMovePosition();
            }

            if (HasPegInfo)
            {
                // The game draws its peg shadow image (22x22 for a 10 radius peg) down and to the left of the peg
                var shadow = ResourceImages.PegShadow;
                var scale = mRadius / 10.0f;
                var size = new SizeF(shadow.Width * scale, shadow.Height * scale);
                var dst = new RectangleF(
                    location.X + PegShadowOffset.X - (size.Width / 2),
                    location.Y + PegShadowOffset.Y - (size.Height / 2),
                    size.Width,
                    size.Height);
                g.DrawImage(shadow, dst);
            }
            else
            {
                using (var brush = new SolidBrush(Level.ShadowColour))
                    g.FillEllipse(brush, location.X - mRadius + Level.ShadowOffset.X, location.Y - mRadius + Level.ShadowOffset.Y, mRadius * 2, mRadius * 2);
            }
        }

        private static readonly PointF PegShadowOffset = new PointF(-3, 3);

        public override void Draw(Graphics g)
        {
            //Show only if its collidable
            if (!HasPegInfo)
            {
                if (Level.ShowCollision && !Collidable)
                    return;
            }

            base.Draw(g);

            var circleImage = GetCircleImage();
            var location = DrawLocation;
            var drawbounds = new RectangleF(location.X - mRadius, location.Y - mRadius, mRadius * 2, mRadius * 2);
            if (HasPegInfo)
            {
                if (Level.UsePegTextures)
                {
                    var srcRect = new Rectangle(0, 0, 20, 20);
                    if (!Level.ShowPreview)
                    {
                        if (PegInfo.Variable)
                            srcRect.Y += 20;
                        if (PegInfo.Crumble)
                            srcRect.Y += 80;
                    }

                    var dstRect = drawbounds;
                    g.DrawImage(ResourceImages.Peg, dstRect, srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    // Draw the PeggleEdit style peg
                    g.FillEllipse(new SolidBrush(PegInfo.GetOuterColour()), drawbounds);
                    drawbounds.Inflate(-2, -2);
                    g.FillEllipse(new SolidBrush(PegInfo.GetInnerColour()), drawbounds);
                }
            }
            else if (!Level.ShowPreview || Visible)
            {
                // Draw the circle image
                if (circleImage == null)
                {
                    var colour = OutlineColor;

                    var dst = new RectangleF(location.X - Radius, location.Y - Radius, Radius * 2, Radius * 2);
                    g.DrawImageWithColour(ResourceImages.CircleOuter, dst, colour);
                    g.DrawImage(ResourceImages.CircleInner, dst);
                }
                else
                {
                    DrawObjectImage(g, circleImage, location);
                }
            }

            if (Level.ShowCollision && Collidable)
            {
                g.FillEllipse(Brushes.White, drawbounds);
            }

            if (Provisional)
                g.FillEllipse(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), drawbounds);
        }

        public virtual Image GetCircleImage()
        {
            return LevelPack.Current.GetImage(ImageFilename)?.Image;
        }

        [EntryProperty(EntryPropertyType.Element, 0.0f)]
        [Description("The radius of the collision boundary. Check the collision checkbox on the view tab to see the current collision radius.")]
        [Category("Behaviour")]
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

        public override int Type
        {
            get
            {
                return LevelEntryTypes.Ball;
            }
        }

        public override RectangleF Bounds
        {
            get
            {
                return new RectangleF(DrawX - mRadius, DrawY - mRadius, mRadius * 2, mRadius * 2);
            }
        }

        public override object Clone()
        {
            Ball newCircle = new Ball(Level);
            base.CloneTo(newCircle);
            newCircle.mRadius = mRadius;
            newCircle.mUnknownFlagsA = mUnknownFlagsA;
            newCircle.mUnknownFlagsB = mUnknownFlagsB;

            return newCircle;
        }
    }
}

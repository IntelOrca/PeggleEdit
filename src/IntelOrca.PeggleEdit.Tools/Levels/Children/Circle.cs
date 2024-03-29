﻿// This file is part of PeggleEdit.
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
    public class Circle : LevelEntry, ICloneable
    {
        private float mRadius;

        public Circle(Level level)
            : base(level)
        {
            mRadius = 10.0f;
        }

        public override void ReadData(BinaryReader br, int version)
        {
            FlagGroup fA = new FlagGroup(br.ReadByte());

            if (version >= 0x52)
            {
                FlagGroup fB = new FlagGroup(br.ReadByte());
            }

            if (fA[1])
            {
                X = br.ReadSingle();
                Y = br.ReadSingle();
            }


            mRadius = br.ReadSingle();
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup();
            FlagGroup fB = new FlagGroup();

            //Make it bouce
            fA[0] = true;

            if (!HasMovementInfo)
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

        public override void DrawShadow(Graphics g)
        {
            if (Level.ShowCollision)
                return;
            if (!HasPegInfo && Level.ShowPreview && !Visible)
                return;

            PointF location = DrawLocation;
            if (MovementLink?.Movement is Movement movement)
            {
                location = movement.GetEstimatedMovePosition();
            }

            g.FillEllipse(new SolidBrush(Level.ShadowColour), location.X - mRadius + Level.ShadowOffset.X, location.Y - mRadius + Level.ShadowOffset.Y, mRadius * 2, mRadius * 2);
        }

        public override void Draw(Graphics g)
        {
            //Show only if its collidable
            if (!HasPegInfo)
            {
                if (Level.ShowCollision && !Collision)
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
                        if (PegInfo.CanBeOrange)
                            srcRect.Y += 20;
                        if (PegInfo.QuickDisappear)
                            srcRect.Y += 80;
                    }

                    var dstRect = drawbounds;
                    g.DrawImage(Resources.peg, dstRect, srcRect, GraphicsUnit.Pixel);
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
                    var colour = OutlineColour;
                    if (colour == Color.Black)
                        colour = Color.White;

                    var dst = new RectangleF(location.X - Radius, location.Y - Radius, Radius * 2, Radius * 2);
                    g.DrawImageWithColour(Resources.circle_outer, dst, colour);
                    g.DrawImage(Resources.circle_inner, dst);
                }
                else
                {
                    g.DrawImage(circleImage, location.X - (circleImage.Width / 2), location.Y - (circleImage.Height / 2), circleImage.Width, circleImage.Height);
                }
            }

            if (Level.ShowCollision && Collision)
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
                return LevelEntryTypes.Circle;
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
            Circle newCircle = new Circle(Level);
            base.CloneTo(newCircle);
            newCircle.mRadius = mRadius;

            return newCircle;
        }
    }
}

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
    public class Polygon : LevelEntry, ICloneable, IPointContainer
    {
        byte mNormalDir;

        float mRotation;
        float mScale;
        int mGrowType;
        List<PointF> mPoints;

        public Polygon(Level level)
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


            if (fA[2])
                mRotation = br.ReadSingle();
            if (fA[3])
                br.ReadSingle();
            if (fA[5])
                mScale = br.ReadSingle();
            if (fA[1])
                mNormalDir = br.ReadByte();
            if (fA[4])
            {
                X = br.ReadSingle();
                Y = br.ReadSingle();
            }

            int numPoints = br.ReadInt32();
            for (int i = 0; i < numPoints; i++)
            {
                mPoints.Add(new PointF(br.ReadSingle(), br.ReadSingle()));
            }

            if (fB[0])
                br.ReadByte();

            if (fB[1])
                mGrowType = br.ReadInt32();
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup();
            FlagGroup fB = new FlagGroup();

            if (!HasMovementInfo)
                fA[4] = true;

            //Write data

            bw.Write(fA.Int8);

            if (version >= 0x23)
                bw.Write(fB.Int8);

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
        }

        public override void Draw(Graphics g)
        {
            if (Level.ShowCollision && !Collision)
                return;

            base.Draw(g);

            if (LevelPack.Current == null)
                return;

            var location = DrawLocation;
            var polygonImage = GetPolygonImage();
            if (polygonImage != null)
            {
                g.DrawImage(polygonImage, location.X - (polygonImage.Width / 2), location.Y - (polygonImage.Height / 2), polygonImage.Width, polygonImage.Height);
            }

            if (polygonImage == null || !Level.ShowPreview || Level.ShowCollision)
            {
                var backupMatrix = g.Transform;
                g.TranslateTransform(location.X, location.Y);

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

                g.Transform = backupMatrix;
            }

            if (MouseOver || Selected)
            {
                var anchorOutline = new Pen(Color.FromArgb(0x23, 0x53, 0xDC));
                var anchorBrush = new SolidBrush(Color.FromArgb(196, 0x23, 0xB0, 0xDC));
                foreach (var p in mPoints)
                {
                    g.DrawSquare(anchorOutline, anchorBrush, p.Add(DrawLocation), 8);
                }
            }
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
            Polygon cpyPolygon = new Polygon(Level);
            base.CloneTo(cpyPolygon);

            cpyPolygon.mPoints = new List<PointF>(mPoints.ToArray());

            return cpyPolygon;
        }

        public override int Type
        {
            get
            {
                return LevelEntryTypes.Polygon;
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
                foreach (PointF pnt in mPoints)
                {
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
        [EditorAttribute(typeof(PolygonPointsUITypeEditor), typeof(UITypeEditor))]
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

        public override bool HitTest(RectangleF rect)
        {
            rect.Offset(-Location.X, -Location.Y);
            rect.Inflate(8, 8);
            return MathExt.RectIntersectsPolygon(rect, mPoints);
        }

        public int InteractionPointCount => mPoints.Count;
        public PointF GetInteractionPoint(int index) => mPoints[index].Add(Location);
        public void SetInteractionPoint(int index, PointF value) => mPoints[index] = value.Subtract(Location);
    }
}

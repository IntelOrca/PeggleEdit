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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using IntelOrca.PeggleEdit.Tools.Extensions;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// Represents the teleport level entry.
    /// </summary>
    public class Teleport : LevelEntry, IPointContainer
    {
        LevelEntry mEntry;

        int mWidth;
        int mHeight;

        public Teleport(Level level)
            : base(level)
        {
            mWidth = 20;
            mHeight = 20;

            mEntry = new Polygon(level);
            mEntry.X = 50;
            mEntry.Y = 50;

        }

        public override void ReadData(BinaryReader br, int version)
        {
            FlagGroup fA = new FlagGroup(br.ReadByte());
            mWidth = br.ReadInt32();
            mHeight = br.ReadInt32();
            if (fA[1])
                br.ReadInt16();
            if (fA[3])
                br.ReadInt32();
            if (fA[5])
                br.ReadInt32();
            if (fA[4])
            {
                mEntry = LevelEntryFactory.CreateLevelEntry(br, version);
            }
            if (fA[2])
            {
                X = br.ReadSingle();
                Y = br.ReadSingle();
            }

            if (fA[6])
            {
                br.ReadSingle();
                br.ReadSingle();
            }
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup();

            if (!HasMovementInfo)
                fA[2] = true;

            if (mEntry != null)
                fA[4] = true;

            //Write data
            bw.Write(fA.Int8);
            bw.Write(mWidth);
            bw.Write(mHeight);

            if (fA[4])
            {
                mEntry.Write(bw, version);
            }

            if (fA[2])
            {
                bw.Write(X);
                bw.Write(Y);
            }

            if (fA[6])
            {
                bw.Write(0.0f);
                bw.Write(0.0f);
            }
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);

            // Draw the destination entry
            if (mEntry != null)
                mEntry.Draw(g);

            // Draw teleport
            var image = LevelPack.Current.GetImage(ImageFilename)?.Image;
            if (image == null)
            {
                g.SmoothingMode = SmoothingMode.None;
                var drawBounds = Bounds;
                g.FillRectangle(Brushes.Black, drawBounds);
                drawBounds.Inflate(-1, -1);
                g.FillRectangle(Brushes.White, drawBounds);
                drawBounds.Inflate(-3, -3);
                g.FillRectangle(Brushes.Black, drawBounds);
                g.SmoothingMode = SmoothingMode.HighQuality;
            }
            else
            {
                g.DrawImage(image, DrawLocation.X - (image.Width / 2), DrawLocation.Y - (image.Height / 2), image.Width, image.Height);
            }

            // Collision
            if (Level.ShowCollision && Collision)
            {
                g.FillRectangle(Brushes.White, Bounds);
            }

            if (MouseOver || Selected)
            {
                var anchorOutline = new Pen(Color.FromArgb(0x23, 0x53, 0xDC));
                var anchorBrush = new SolidBrush(Color.FromArgb(196, 0x23, 0xB0, 0xDC));

                //Draw line to destination
                Pen pen = new Pen(Color.White, 1.0f);
                pen.DashStyle = DashStyle.Custom;
                pen.DashPattern = new float[] { 2, 4 };
                pen.CustomEndCap = new AdjustableArrowCap(10.0f, 10.0f, false);
                g.DrawLine(pen, DrawX, DrawY, DestinationX, DestinationY);
                g.DrawSquare(anchorOutline, anchorBrush, Destination, 8);
            }
        }

        public override object Clone()
        {
            Teleport cpyTeleport = new Teleport(Level);
            base.CloneTo(cpyTeleport);
            cpyTeleport.mWidth = mWidth;
            cpyTeleport.mHeight = mHeight;
            if (mEntry != null)
                cpyTeleport.mEntry = (LevelEntry)mEntry.Clone();

            return cpyTeleport;
        }

        public override int Type
        {
            get
            {
                return LevelEntryTypes.Teleport;
            }
        }

        public LevelEntry Entry
        {
            get
            {
                return mEntry;
            }
            set
            {
                mEntry = value;
            }
        }

        [Browsable(false)]
        public PointF Destination
        {
            get => mEntry?.Location ?? new PointF();
            set
            {
                if (mEntry != null)
                {
                    mEntry.Location = value;
                }
            }
        }

        [Category("Behaviour")]
        public float DestinationX
        {
            get
            {
                if (mEntry == null)
                    return float.NaN;
                else
                    return mEntry.X;
            }
            set
            {
                if (mEntry != null)
                    mEntry.X = value;
            }
        }

        [Category("Behaviour")]
        public float DestinationY
        {
            get
            {
                if (mEntry == null)
                    return float.NaN;
                else
                    return mEntry.Y;
            }
            set
            {
                if (mEntry != null)
                    mEntry.Y = value;
            }
        }

        public int Width
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

        public int Height
        {
            get
            {
                return mHeight;
            }
            set
            {
                mHeight = value;
            }
        }

        public override Level Level
        {
            set
            {
                if (mEntry != null)
                    mEntry.Level = value;

                base.Level = value;
            }
        }

        [Browsable(false)]
        public override RectangleF Bounds
        {
            get
            {
                return new RectangleF(DrawX - (mWidth / 2), DrawY - (mHeight / 2), mWidth, mHeight);
            }
        }

        public int InteractionPointCount => 1;

        public PointF GetInteractionPoint(int index)
        {
            return Destination;
        }

        public void SetInteractionPoint(int index, PointF value)
        {
            Destination = value;
        }
    }
}

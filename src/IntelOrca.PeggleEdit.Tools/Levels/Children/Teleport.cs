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

        // Unknown values, preserved so that they are written back as read
        int mUnknownFlags;
        bool mHasUnknown0;
        short mUnknown0;
        bool mHasUnknown1;
        int mUnknown1;
        bool mHasUnknown2;
        int mUnknown2;
        bool mHasUnknown3;
        float mUnknown3X;
        float mUnknown3Y;

        // Destination entry id when it is not stored inline (1), preserved so that it is written back as read
        bool mHasEntryId;
        int mEntryId;

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
            mUnknownFlags = fA.Int32 & ((1 << 0) | (1 << 7));
            mWidth = br.ReadInt32();
            mHeight = br.ReadInt32();
            mHasUnknown0 = fA[1];
            if (fA[1])
                mUnknown0 = br.ReadInt16();
            mHasUnknown1 = fA[3];
            if (fA[3])
                mUnknown1 = br.ReadInt32();
            mHasUnknown2 = fA[5];
            if (fA[5])
                mUnknown2 = br.ReadInt32();
            mHasEntryId = false;
            if (fA[4])
            {
                var entryId = br.ReadInt32();
                if (entryId == 1)
                {
                    mEntry = LevelEntryFactory.CreateLevelEntry(entryId, br, version);
                }
                else
                {
                    mEntry = null;
                    mHasEntryId = true;
                    mEntryId = entryId;
                }
            }
            else
            {
                mEntry = null;
            }
            if (fA[2])
            {
                X = br.ReadSingle();
                Y = br.ReadSingle();
            }

            mHasUnknown3 = fA[6];
            if (fA[6])
            {
                mUnknown3X = br.ReadSingle();
                mUnknown3Y = br.ReadSingle();
            }
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup(mUnknownFlags);

            fA[1] = mHasUnknown0;
            fA[3] = mHasUnknown1;
            fA[5] = mHasUnknown2;
            fA[6] = mHasUnknown3;

            if (!HasMovementInfo)
                fA[2] = true;

            if (mEntry != null || mHasEntryId)
                fA[4] = true;

            //Write data
            bw.Write(fA.Int8);
            bw.Write(mWidth);
            bw.Write(mHeight);

            if (fA[1])
                bw.Write(mUnknown0);
            if (fA[3])
                bw.Write(mUnknown1);
            if (fA[5])
                bw.Write(mUnknown2);

            if (fA[4])
            {
                if (mEntry != null)
                    mEntry.Write(bw, version);
                else
                    bw.Write(mEntryId);
            }

            if (fA[2])
            {
                bw.Write(X);
                bw.Write(Y);
            }

            if (fA[6])
            {
                bw.Write(mUnknown3X);
                bw.Write(mUnknown3Y);
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
            cpyTeleport.mUnknownFlags = mUnknownFlags;
            cpyTeleport.mHasUnknown0 = mHasUnknown0;
            cpyTeleport.mUnknown0 = mUnknown0;
            cpyTeleport.mHasUnknown1 = mHasUnknown1;
            cpyTeleport.mUnknown1 = mUnknown1;
            cpyTeleport.mHasUnknown2 = mHasUnknown2;
            cpyTeleport.mUnknown2 = mUnknown2;
            cpyTeleport.mHasUnknown3 = mHasUnknown3;
            cpyTeleport.mUnknown3X = mUnknown3X;
            cpyTeleport.mUnknown3Y = mUnknown3Y;
            cpyTeleport.mHasEntryId = mHasEntryId;
            cpyTeleport.mEntryId = mEntryId;
            cpyTeleport.mEntry = (LevelEntry)mEntry?.Clone();

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

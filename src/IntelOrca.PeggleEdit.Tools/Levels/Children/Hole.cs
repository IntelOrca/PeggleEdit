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
    public class Hole : LevelEntry, IPointContainer
    {
        LevelEntry mDestinationEntry;

        int mWidth;
        int mHeight;

        // Unknown values, preserved so that they are written back as read
        int mUnknownFlags;
        bool mHasActiveTimer;
        short mActiveTimer;
        bool mHasScore;
        int mScore;
        bool mHasOutDelay;
        int mOutDelay;
        bool mHasVelocity;
        float mVelocityX;
        float mVelocityY;

        // Destination entry id when it is not stored inline (1), preserved so that it is written back as read
        bool mHasDestinationLink;
        int mDestinationLink;

        /// <summary>
        /// Whether the destination is an object elsewhere in the level, given by its number, rather than an entry of
        /// the hole's own.
        /// </summary>
        internal bool HasDestinationLink => mHasDestinationLink;

        /// <summary>
        /// The number of the object that is the destination, when <see cref="HasDestinationLink"/>.
        /// </summary>
        internal int DestinationLink
        {
            get => mDestinationLink;
            set => mDestinationLink = value;
        }

        /// <summary>
        /// The object that <see cref="DestinationLink"/> refers to, found when the level is read, so that the number
        /// can follow it as the level is changed. Null if it was not found.
        /// </summary>
        internal object DestinationLinkTarget { get; set; }

        /// <summary>
        /// Removes the destination link, once the object it refers to has been removed.
        /// </summary>
        internal void ClearDestinationLink()
        {
            mHasDestinationLink = false;
            DestinationLinkTarget = null;
        }

        /// <summary>
        /// Makes the destination the given object, which is numbered when the level is written.
        /// </summary>
        internal void SetDestinationLink(object target)
        {
            mDestinationEntry = null;
            mHasDestinationLink = true;
            mDestinationLink = 0;
            DestinationLinkTarget = target;
        }

        public Hole(Level level)
            : base(level)
        {
            mWidth = 20;
            mHeight = 20;

            mDestinationEntry = new Poly(level);
            mDestinationEntry.X = 50;
            mDestinationEntry.Y = 50;

        }

        public override void ReadData(BinaryReader br, int version)
        {
            FlagGroup fA = new FlagGroup(br.ReadByte());
            mUnknownFlags = fA.Int32 & ((1 << 0) | (1 << 7));
            mWidth = br.ReadInt32();
            mHeight = br.ReadInt32();
            mHasActiveTimer = fA[1];
            if (fA[1])
                mActiveTimer = br.ReadInt16();
            mHasScore = fA[3];
            if (fA[3])
                mScore = br.ReadInt32();
            mHasOutDelay = fA[5];
            if (fA[5])
                mOutDelay = br.ReadInt32();
            mHasDestinationLink = false;
            if (fA[4])
            {
                var entryId = br.ReadInt32();
                if (entryId == 1)
                {
                    mDestinationEntry = LevelEntryFactory.CreateLevelEntry(entryId, br, version);
                }
                else
                {
                    mDestinationEntry = null;
                    mHasDestinationLink = true;
                    mDestinationLink = entryId;
                }
            }
            else
            {
                mDestinationEntry = null;
            }
            if (fA[2])
            {
                ReadPosition(br);
            }

            mHasVelocity = fA[6];
            if (fA[6])
            {
                mVelocityX = br.ReadSingle();
                mVelocityY = br.ReadSingle();
            }
        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup(mUnknownFlags);

            fA[1] = mHasActiveTimer;
            fA[3] = mHasScore;
            fA[5] = mHasOutDelay;
            fA[6] = mHasVelocity;

            if (!HasMover)
                fA[2] = true;

            if (mDestinationEntry != null || mHasDestinationLink)
                fA[4] = true;

            //Write data
            bw.Write(fA.Int8);
            bw.Write(mWidth);
            bw.Write(mHeight);

            if (fA[1])
                bw.Write(mActiveTimer);
            if (fA[3])
                bw.Write(mScore);
            if (fA[5])
                bw.Write(mOutDelay);

            if (fA[4])
            {
                if (mDestinationEntry != null)
                    mDestinationEntry.Write(bw, version);
                else
                    bw.Write(mDestinationLink);
            }

            if (fA[2])
            {
                bw.Write(X);
                bw.Write(Y);
            }

            if (fA[6])
            {
                bw.Write(mVelocityX);
                bw.Write(mVelocityY);
            }
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);

            // The game draws neither an invisible hole nor where a hole sends the ball
            if (Level.ShowPreview && !Level.ShowCollision && !Visible)
                return;

            // Draw the destination entry
            if (mDestinationEntry != null && (!Level.ShowPreview || Level.ShowCollision))
                mDestinationEntry.Draw(g);

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
                DrawObjectImage(g, image, DrawLocation);
            }

            // Collision
            if (Level.ShowCollision && Collidable)
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
            Hole cpyTeleport = new Hole(Level);
            base.CloneTo(cpyTeleport);
            cpyTeleport.mWidth = mWidth;
            cpyTeleport.mHeight = mHeight;
            cpyTeleport.mUnknownFlags = mUnknownFlags;
            cpyTeleport.mHasActiveTimer = mHasActiveTimer;
            cpyTeleport.mActiveTimer = mActiveTimer;
            cpyTeleport.mHasScore = mHasScore;
            cpyTeleport.mScore = mScore;
            cpyTeleport.mHasOutDelay = mHasOutDelay;
            cpyTeleport.mOutDelay = mOutDelay;
            cpyTeleport.mHasVelocity = mHasVelocity;
            cpyTeleport.mVelocityX = mVelocityX;
            cpyTeleport.mVelocityY = mVelocityY;
            cpyTeleport.mHasDestinationLink = mHasDestinationLink;
            cpyTeleport.mDestinationLink = mDestinationLink;
            cpyTeleport.DestinationLinkTarget = DestinationLinkTarget;
            cpyTeleport.mDestinationEntry = (LevelEntry)mDestinationEntry?.Clone();

            return cpyTeleport;
        }

        public override int Type
        {
            get
            {
                return LevelEntryTypes.Hole;
            }
        }

        public LevelEntry DestinationEntry
        {
            get
            {
                return mDestinationEntry;
            }
            set
            {
                mDestinationEntry = value;
            }
        }

        [Browsable(false)]
        public PointF Destination
        {
            get => mDestinationEntry?.Location ?? new PointF();
            set
            {
                if (mDestinationEntry != null)
                {
                    mDestinationEntry.Location = value;
                }
            }
        }

        [Category("Behaviour")]
        public float DestinationX
        {
            get
            {
                if (mDestinationEntry == null)
                    return float.NaN;
                else
                    return mDestinationEntry.X;
            }
            set
            {
                if (mDestinationEntry != null)
                    mDestinationEntry.X = value;
            }
        }

        [Category("Behaviour")]
        public float DestinationY
        {
            get
            {
                if (mDestinationEntry == null)
                    return float.NaN;
                else
                    return mDestinationEntry.Y;
            }
            set
            {
                if (mDestinationEntry != null)
                    mDestinationEntry.Y = value;
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

        [DisplayName("Score")]
        [Description("The points scored when the ball goes into the hole.")]
        [Category("Behaviour")]
        [DefaultValue(0)]
        public int Score
        {
            get
            {
                return mScore;
            }
            set
            {
                mScore = value;
                mHasScore |= value != 0;
            }
        }

        [DisplayName("Out Delay")]
        [Description("How long the ball stays in the hole before it comes out of the destination.")]
        [Category("Behaviour")]
        [DefaultValue(0)]
        public int OutDelay
        {
            get
            {
                return mOutDelay;
            }
            set
            {
                mOutDelay = value;
                mHasOutDelay |= value != 0;
            }
        }

        [DisplayName("Circular")]
        [Description("Whether the hole is round rather than rectangular.")]
        [Category("Behaviour")]
        [DefaultValue(false)]
        public bool Circular
        {
            get
            {
                return (mUnknownFlags & 1) != 0;
            }
            set
            {
                mUnknownFlags = value ? mUnknownFlags | 1 : mUnknownFlags & ~1;
            }
        }

        public override Level Level
        {
            set
            {
                if (mDestinationEntry != null)
                    mDestinationEntry.Level = value;

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

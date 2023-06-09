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
using System.Drawing.Design;
using System.IO;
using System.Text;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// The base class for all level entries which are stored in levels.
    /// </summary>
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public abstract class LevelEntry : ICloneable, ILocation, IMovable, ILevelChild
    {
        Level mLevel;

        [Browsable(false)]
        public bool MouseOver { get; set; }
        [Browsable(false)]
        public bool Selected { get; set; }
        [Browsable(false)]
        public bool Provisional { get; set; }

        float mX;
        float mY;

        string mImageFilename;


        PegInfo mPegInfo;

        string mID;
        string mLogic;
        bool mCollision;
        bool mVisible;
        bool mCanMove;
        bool mBackground;
        bool mBallStopReset;
        bool mForeground;
        bool mDrawSort;
        bool mForeground2;
        bool mDrawFloat;
        bool mBaseObject;
        float mRolly;
        float mBouncy;
        Color mSolidColour;
        Color mOutlineColour;
        float mImageDX;
        float mImageDY;
        float mImageRotation;
        byte mSound;
        float mMaxBounceVelocity;
        int mSubID;
        byte mFlipperFlags;
        bool mShadow = true;

        [EntryProperty(EntryPropertyType.SubElement, null)]
        [Description("The movement properties of the object.")]
        [Category("Behaviour")]
        [Editor(typeof(MovementUITypeEditor), typeof(UITypeEditor))]
        public MovementLink MovementLink { get; set; }

        public LevelEntry(Level level)
        {
            mLevel = level;

            mCanMove = true;
            mCollision = true;
            mVisible = true;

            mSolidColour = Color.Black;
            mOutlineColour = Color.Black;
        }

        public void ReadGenericData(BinaryReader br, int version)
        {
            FlagGroup f = new FlagGroup(br.ReadInt32());

            mCollision = f[5];
            mVisible = f[6];
            mCanMove = f[7];
            mBackground = f[14];
            mBaseObject = f[15];
            mBallStopReset = f[20];
            mForeground = f[22];
            mDrawSort = f[24];
            mForeground2 = f[25];
            mDrawFloat = f[28];

            if (f[0])
                mRolly = br.ReadSingle();
            if (f[1])
                mBouncy = br.ReadSingle();
            if (f[4])
                br.ReadInt32();
            if (f[8])
                mSolidColour = Color.FromArgb(br.ReadInt32());
            if (f[9])
                mOutlineColour = Color.FromArgb(br.ReadInt32());
            if (f[10])
            {
                mImageFilename = LevelReader.ReadPopcapString(br);
            }
            if (f[11])
                mImageDX = br.ReadSingle();
            if (f[12])
                mImageDY = br.ReadSingle();
            if (f[13])
                mImageRotation = MathExt.ToDegrees(br.ReadSingle());
            if (f[16])
                br.ReadInt32();
            if (f[17])
                mID = LevelReader.ReadPopcapString(br);
            if (f[18])
                br.ReadInt32();
            if (f[19])
                mSound = br.ReadByte();
            if (f[21])
                mLogic = LevelReader.ReadPopcapString(br);
            if (f[23])
                mMaxBounceVelocity = br.ReadSingle();
            if (f[26])
                mSubID = br.ReadInt32();
            if (f[27])
                mFlipperFlags = br.ReadByte();
            if (f[2])
            {
                mPegInfo = new PegInfo(this);
                mPegInfo.ReadData(br, version);
            }
            if (f[3])
            {
                MovementLink = new MovementLink(mLevel);
                MovementLink.ReadData(br, version);
            }

            if (f[30] && version >= 0x50)
                mShadow = f[30];
        }

        public void WriteGenericData(BinaryWriter bw, int version)
        {
            //Set flags
            FlagGroup f = new FlagGroup();

            f[0] = (mRolly != 0.0f);
            f[1] = (mBouncy != 0.0f);
            f[2] = HasPegInfo;
            f[3] = HasMovementInfo;
            f[5] = mCollision;
            f[6] = mVisible;
            f[7] = mCanMove;
            f[8] = (mSolidColour != Color.Black);
            f[9] = (mOutlineColour != Color.Black);
            f[10] = !String.IsNullOrEmpty(mImageFilename);
            f[11] = (mImageDX != 0.0f);
            f[12] = (mImageDY != 0.0f);
            f[13] = (mImageRotation != 0.0f);
            f[14] = mBackground;
            f[15] = mBaseObject;
            f[17] = !String.IsNullOrEmpty(mID);
            f[19] = (mSound != 0);
            f[20] = mBallStopReset;
            f[21] = !String.IsNullOrEmpty(mLogic);
            f[22] = mForeground;
            f[23] = (mMaxBounceVelocity != 0.0f);
            f[24] = mDrawSort;
            f[25] = mForeground2;
            f[26] = (mSubID != 0);
            f[27] = (mFlipperFlags != 0);
            f[28] = mDrawFloat;
            f[30] = (mShadow && version >= 0x50);

            //Write data
            bw.Write(f.Int32);

            if (f[0])
                bw.Write(mRolly);
            if (f[1])
                bw.Write(mBouncy);
            if (f[8])
                bw.Write(mSolidColour.ToArgb());
            if (f[9])
                bw.Write(mOutlineColour.ToArgb());
            if (f[10])
            {
                LevelWriter.WritePopcapString(bw, mImageFilename);
            }
            if (f[11])
                bw.Write(mImageDX);
            if (f[12])
                bw.Write(mImageDY);
            if (f[13])
                bw.Write(MathExt.ToRadians(mImageRotation));
            if (f[17])
            {
                LevelWriter.WritePopcapString(bw, mID);
            }
            if (f[19])
                bw.Write(mSolidColour.ToArgb());
            if (f[21])
            {
                LevelWriter.WritePopcapString(bw, mLogic);
            }
            if (f[23])
                bw.Write(mMaxBounceVelocity);
            if (f[26])
                bw.Write(mSubID);
            if (f[27])
                bw.Write(mFlipperFlags);
            if (f[2])
                mPegInfo.WriteData(bw, version);
            if (f[3])
                MovementLink.WriteData(bw, version);
        }

        public virtual void ReadData(BinaryReader br, int version)
        {

        }

        public virtual void WriteData(BinaryWriter bw, int version)
        {

        }

        public virtual void DrawShadow(Graphics g)
        {

        }

        public virtual void Draw(Graphics g)
        {
            if (Level.ShowPreview && !mVisible)
                return;

            if (MovementLink?.Movement is Movement movement)
            {
                if (MouseOver || Selected)
                {
                    movement.DrawPath(g);
                }
                if (!mLevel.ShowCollision && (MouseOver || Selected || Level.ShowAnchorsAlways))
                {
                    movement.DrawAnchors(g, mLevel);
                }
            }
        }

        public virtual void OnDelete()
        {

        }

        public Movement GetBaseMovement()
        {
            Movement mb = null;
            var m = MovementLink?.Movement;
            while (m != null)
            {
                mb = m;
                m = m.MovementLink?.Movement;
            }
            return mb;
        }

        public Movement GetMovement()
        {
            return MovementLink?.Movement;
        }

        private Movement GetHighestSelectedMovement()
        {
            Movement mb = null;
            var m = MovementLink?.Movement;
            while (m != null)
            {
                mb = m;
                m = m.MovementLink?.Movement;
                var entry = mLevel.GetFirstLevelEntryWithMovement(m);
                if (entry == null || !entry.Selected)
                {
                    break;
                }
            }
            return mb;
        }

        public PointF GetSelectedMovePosition()
        {
            var movement = GetHighestSelectedMovement();
            if (movement != null)
            {
                return new PointF(movement.AnchorPointX, movement.AnchorPointY);
            }
            return new PointF(mX, mY);
        }
        
        public void SetSelectedMovePosition(PointF value)
        {
            var movement = GetHighestSelectedMovement();
            if (movement != null)
            {
                movement.AnchorPointX = value.X;
                movement.AnchorPointY = value.Y;
            }
            else
            {
                mX = value.X;
                mY = value.Y;
            }
        }

        public int GetEntryIndex()
        {
            return mLevel.Entries.IndexOf(this);
        }

        public void Write(BinaryWriter bw, int version)
        {
            bw.Write(1);
            bw.Write(Type);
            WriteGenericData(bw, version);
            WriteData(bw, version);
        }

        public bool HitTest(PointF position)
        {
            return HitTest(new RectangleF(position.X, position.Y, 1, 1));
        }

        public virtual bool HitTest(RectangleF rect)
        {
            return Bounds.IntersectsWith(rect);
        }

        public override string ToString()
        {
            return String.Format("{0}, ({1}, {2})", this.GetType().Name, X, Y);
        }

        [Browsable(false)]
        public virtual int Type
        {
            get
            {
                return 0;
            }
        }

        [Browsable(false)]
        public virtual RectangleF Bounds
        {
            get
            {
                return RectangleF.Empty;
            }
        }

        [Browsable(false)]
        public bool HasPegInfo
        {
            get
            {
                return (mPegInfo != null);
            }
        }

        [Browsable(false)]
        public bool HasMovementInfo
        {
            get
            {
                return (MovementLink != null);
            }
        }

        [EntryProperty(EntryPropertyType.SubElement, null)]
        [DisplayName("Peg Properties")]
        [Description("The peg properties of the object.")]
        [Category("Behaviour")]
        public PegInfo PegInfo
        {
            get
            {
                return mPegInfo;
            }
            set
            {
                mPegInfo = value;
            }
        }

        [EntryProperty(EntryPropertyType.Element, 0.0f)]
        [Description("The X co-ordinate of the object.")]
        [Category("Layout")]
        public virtual float X
        {
            get => GetSelectedMovePosition().X;
            set
            {
                var pos = GetSelectedMovePosition();
                pos.X = value;
                SetSelectedMovePosition(pos);
            }
        }

        [EntryProperty(EntryPropertyType.Element, 0.0f)]
        [Description("The Y co-ordinate position of the object.")]
        [Category("Layout")]
        public virtual float Y
        {
            get => GetSelectedMovePosition().Y;
            set
            {
                var pos = GetSelectedMovePosition();
                pos.Y = value;
                SetSelectedMovePosition(pos);
            }
        }

        [Browsable(false)]
        public virtual float DrawX => DrawLocation.X;

        [Browsable(false)]
        public virtual float DrawY => DrawLocation.Y;

        [Browsable(false)]
        public virtual PointF DrawLocation
        {
            get
            {
                if (MovementLink?.Movement is Movement movement)
                {
                    return movement.GetEstimatedMovePosition();
                }
                return Location;
            }
        }

        public virtual object Clone()
        {
            throw new NotImplementedException();
        }

        protected void CloneTo(LevelEntry dest)
        {
            dest.Selected = Selected;

            dest.mX = mX;
            dest.mY = mY;

            if (HasPegInfo)
            {
                dest.mPegInfo = (PegInfo)mPegInfo.Clone();
                dest.mPegInfo.Parent = dest;
            }

            dest.MovementLink = MovementLink?.Clone();
            dest.mImageFilename = mImageFilename;
            dest.mID = mID;
            dest.mLogic = mLogic;
            dest.mCollision = mCollision;
            dest.mVisible = mVisible;
            dest.mCanMove = mCanMove;
            dest.mBackground = mBackground;
            dest.mBallStopReset = mBallStopReset;
            dest.mForeground = mForeground;
            dest.mDrawSort = mDrawSort;
            dest.mForeground2 = mForeground2;
            dest.mDrawFloat = mDrawFloat;
            dest.mBaseObject = mBaseObject;
            dest.mRolly = mRolly;
            dest.mBouncy = mBouncy;
            dest.mSolidColour = mSolidColour;
            dest.mOutlineColour = mOutlineColour;
            dest.mImageDX = mImageDX;
            dest.mImageDY = mImageDY;
            dest.mImageRotation = mImageRotation;
            dest.mSound = mSound;
            dest.mMaxBounceVelocity = mMaxBounceVelocity;
            dest.mSubID = mSubID;
            dest.mFlipperFlags = mFlipperFlags;
            dest.mShadow = mShadow;
        }

        [Browsable(false)]
        public PointF Location
        {
            get
            {
                return new PointF(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [DisplayName("Has Shadow")]
        [Description("Whether or not to display a shadow for this object.")]
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool HasShadow
        {
            get
            {
                return mShadow;
            }
            set
            {
                mShadow = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [Description("Whether or not the ball reacts with the object or not. Pegs and Bricks seem to ignore this attribute.")]
        [Category("Behaviour")]
        [DefaultValue(true)]
        public bool Collision
        {
            get
            {
                return mCollision;
            }
            set
            {
                mCollision = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [Description("Whether or not object can be seen or not. Pegs and Bricks seem to ignore this attribute.")]
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool Visible
        {
            get
            {
                return mVisible;
            }
            set
            {
                mVisible = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, null)]
        [DisplayName("Image Filename")]
        [Description("The filename to the image in the PAK file.")]
        [Category("Appearance")]
        [DefaultValue("")]
        public string ImageFilename
        {
            get
            {
                return mImageFilename;
            }
            set
            {
                mImageFilename = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, null)]
        [Description("The identifier of the object. There are pre-defined identifiers for the objects to be certain things.")]
        [Category("Behaviour")]
        [DefaultValue("")]
        public string ID
        {
            get
            {
                return mID;
            }
            set
            {
                mID = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, null)]
        public string Logic
        {
            get
            {
                return mLogic;
            }
            set
            {
                mLogic = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [DisplayName("Can Move")]
        [Description("This attribute must be set to true if the object has movement information but does not make the object move itself. Use Movement for that.")]
        [Category("Behaviour")]
        [DefaultValue(true)]
        public bool CanMove
        {
            get
            {
                return mCanMove;
            }
            set
            {
                mCanMove = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [Description("Whether or not to draw the object behind everything apart from the background image.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool Background
        {
            get
            {
                return mBackground;
            }
            set
            {
                mBackground = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [Description("Whether or not to draw the object in front of everything other than the HUD.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool Foreground
        {
            get
            {
                return mForeground;
            }
            set
            {
                mForeground = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [Description("Whether or not to draw the object in front of everything including the HUD. Foreground must be set to true for this to be applicable.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool Foreground2
        {
            get
            {
                return mForeground2;
            }
            set
            {
                mForeground2 = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [DefaultValue(false)]
        public bool BallStopReset
        {
            get
            {
                return mBallStopReset;
            }
            set
            {
                mBallStopReset = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [DefaultValue(false)]
        public bool DrawSort
        {
            get
            {
                return mDrawSort;
            }
            set
            {
                mDrawSort = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [DefaultValue(false)]
        public bool DrawFloat
        {
            get
            {
                return mDrawFloat;
            }
            set
            {
                mDrawFloat = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [DefaultValue(false)]
        public bool BaseObject
        {
            get
            {
                return mBaseObject;
            }
            set
            {
                mBaseObject = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, 0.0f)]
        [DisplayName("Rolly")]
        [Description("The multiplier of how rolly the object is. 1.0 would be no effect, 0.5 would be half as rolly and 2.0 would be double as rolly. 0.0 indicates not to use the attribute.")]
        [Category("Behaviour")]
        [DefaultValue(0.0f)]
        public float Rolly
        {
            get
            {
                return mRolly;
            }
            set
            {
                mRolly = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, 0.0f)]
        [DisplayName("Bouncy")]
        [Description("The multiplier of how bouncy the object is. 1.0 would be no effect, 0.5 would be half as bouncy and 2.0 would be double as bouncy. 0.0 indicates not to use the attribute.")]
        [Category("Behaviour")]
        [DefaultValue(0.0f)]
        public float Bouncy
        {
            get
            {
                return mBouncy;
            }
            set
            {
                mBouncy = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, typeof(Color), "Black")]
        [DisplayName("Solid Colour")]
        [Description("The colour of the inside of the object if there is no image specified. Black specifies not to use the attribute.")]
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public Color SolidColour
        {
            get
            {
                return mSolidColour;
            }
            set
            {
                mSolidColour = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, typeof(Color), "Black")]
        [DisplayName("Outline Colour")]
        [Description("The colour of the outline of the object if there is no image specified. Black specifies not to use the attribute.")]
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public Color OutlineColour
        {
            get
            {
                return mOutlineColour;
            }
            set
            {
                mOutlineColour = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, 0.0f)]
        [DisplayName("Image DX")]
        [Description("The horizontal offset for where the specified image is drawn.")]
        [Category("Appearance")]
        [DefaultValue(0.0f)]
        public float ImageDX
        {
            get
            {
                return mImageDX;
            }
            set
            {
                mImageDX = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, 0.0f)]
        [DisplayName("Image DY")]
        [Description("The vertical offset for where the specified image is drawn.")]
        [Category("Appearance")]
        [DefaultValue(0.0f)]
        public float ImageDY
        {
            get
            {
                return mImageDY;
            }
            set
            {
                mImageDY = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, 0.0f)]
        [DisplayName("Image Rotation")]
        [Description("The angular offset for where the specified image is drawn in degrees.")]
        [Category("Appearance")]
        [DefaultValue(0.0f)]
        public float ImageRotation
        {
            get
            {
                return mImageRotation;
            }
            set
            {
                mImageRotation = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, (byte)0)]
        [DefaultValue((byte)0)]
        public byte Sound
        {
            get
            {
                return mSound;
            }
            set
            {
                mSound = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, 0.0f)]
        [DefaultValue(0.0f)]
        public float MaxBounceVelocity
        {
            get
            {
                return mMaxBounceVelocity;
            }
            set
            {
                mMaxBounceVelocity = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, 0)]
        [DefaultValue(0)]
        public int SubID
        {
            get
            {
                return mSubID;
            }
            set
            {
                mSubID = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, (byte)0)]
        [DefaultValue((byte)0)]
        public byte FlipperFlags
        {
            get
            {
                return mFlipperFlags;
            }
            set
            {
                mFlipperFlags = value;
            }
        }

        [Browsable(false)]
        public virtual Level Level
        {
            get
            {
                return mLevel;
            }
            set
            {
                mLevel = value;
                if (MovementLink != null)
                    MovementLink.Level = Level;
            }
        }
    }
}

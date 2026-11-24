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
using System.Drawing.Design;
using System.IO;
using IntelOrca.PeggleEdit.Tools.Extensions;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// The values of FlipperFlags, from physobj_flipperflags in PopCap's level editor.
    /// </summary>
    public enum FlipperSetting : byte
    {
        Default,
        Off,
        On,
    }

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

        string mId;
        string mLogic;
        bool mCollidable;
        bool mVisible;
        bool mMoving;
        bool mBackground;
        bool mStopReset;
        bool mForeground;
        bool mDrawSort;
        bool mForeground2;
        bool mDrawFloat;
        bool mBaseObject;
        float mRolly;
        float mBouncy;
        // Colours are stored as the game stores them, 0xRRGGBB (the top byte is kept as read). The game treats a solid
        // colour of 0 as no colour, and its default outline colour is 0xFFFFFF.
        const uint NoSolidColor = 0;
        const uint DefaultOutlineColor = 0xFFFFFF;
        uint mSolidColor = NoSolidColor;
        uint mOutlineColor = DefaultOutlineColor;
        float mImageDX;
        float mImageDY;
        float mImageRotation; // In radians, as stored in the level file, so that it round trips exactly
        byte mSound;
        float mMaxBounceVelocity;
        int mSubId;
        byte mFlipperFlags;
        bool mDrawShadow = true;

        // Unknown values, preserved so that they are written back as read
        bool mHasMoveTime;
        int mMoveTime;
        bool mHasDisappearTimer;
        int mDisappearTimer;
        bool mHasStopMovingTimer;
        int mStopMovingTimer;
        int mUnknownFlags;

        // Flags for values that are only written when not the default, so that a default value present in the file is kept
        const int ValueFlagsMask = (1 << 0) | (1 << 1) | (0x3F << 8) | (1 << 17) | (1 << 19) | (1 << 21) | (1 << 23) | (1 << 26) | (1 << 27);
        int mValueFlags;

        [EntryProperty(EntryPropertyType.SubElement, null)]
        [DisplayName("Mover")]
        [Description("The movement of the object.")]
        [Category("Behaviour")]
        [Editor(typeof(MoverUITypeEditor), typeof(UITypeEditor))]
        public MoverLink MoverLink { get; set; }

        public LevelEntry(Level level)
        {
            mLevel = level;

            mMoving = true;
            mCollidable = true;
            mVisible = true;
        }

        public void ReadGenericData(BinaryReader br, int version)
        {
            var f = version < 0x0F ?
                new FlagGroup(br.ReadUInt24()) :
                new FlagGroup(br.ReadInt32());

            mValueFlags = f.Int32 & ValueFlagsMask;
            mUnknownFlags = f.Int32 & ((1 << 29) | (1 << 31));
            if (version < 0x50)
                mUnknownFlags |= f.Int32 & (1 << 30);

            mCollidable = f[5];
            mVisible = f[6];
            mMoving = f[7];
            mBackground = f[14];
            mBaseObject = f[15];
            mStopReset = f[20];
            mForeground = f[22];

            if (f[0])
                mRolly = br.ReadSingle();
            if (f[1])
                mBouncy = br.ReadSingle();
            mHasMoveTime = f[4];
            if (f[4])
                mMoveTime = br.ReadInt32();
            if (f[8])
                mSolidColor = br.ReadUInt32();
            if (f[9])
                mOutlineColor = br.ReadUInt32();
            if (f[10])
            {
                mImageFilename = LevelReader.ReadPopcapString(br);
            }
            if (f[11])
                mImageDX = br.ReadSingle();
            if (f[12])
                mImageDY = br.ReadSingle();
            if (f[13])
                mImageRotation = br.ReadSingle();
            mHasDisappearTimer = f[16];
            if (f[16])
                mDisappearTimer = br.ReadInt32();
            if (f[17])
                mId = LevelReader.ReadPopcapString(br);
            mHasStopMovingTimer = f[18];
            if (f[18])
                mStopMovingTimer = br.ReadInt32();
            if (f[19])
                mSound = br.ReadByte();
            if (f[21])
                mLogic = LevelReader.ReadPopcapString(br);

            mDrawSort = f[24];
            mForeground2 = f[25];
            mDrawFloat = f[28];
            if (f[23])
                mMaxBounceVelocity = br.ReadSingle();
            if (f[26])
                mSubId = br.ReadInt32();
            if (f[27])
                mFlipperFlags = br.ReadByte();

            if (version >= 0x50)
            {
                mDrawShadow = f[30];
            }

            if (f[2])
            {
                mPegInfo = new PegInfo(this);
                mPegInfo.ReadData(br, version);
            }
            else
            {
                mPegInfo = null;
            }
            if (f[3])
            {
                MoverLink = new MoverLink(mLevel);
                MoverLink.ReadData(br, version);
            }
        }

        public void WriteGenericData(BinaryWriter bw, int version)
        {
            //Set flags
            FlagGroup f = new FlagGroup(mUnknownFlags);

            f[0] = (mRolly != 0.0f);
            f[1] = (mBouncy != 0.0f);
            f[2] = HasPegInfo;
            f[3] = HasMover;
            f[4] = mHasMoveTime;
            f[5] = mCollidable;
            f[6] = mVisible;
            f[7] = mMoving;
            f[8] = (mSolidColor != NoSolidColor);
            f[9] = (mOutlineColor != DefaultOutlineColor);
            f[10] = !String.IsNullOrEmpty(mImageFilename);
            f[11] = (mImageDX != 0.0f);
            f[12] = (mImageDY != 0.0f);
            f[13] = (mImageRotation != 0.0f);
            f[14] = mBackground;
            f[15] = mBaseObject;
            f[16] = mHasDisappearTimer;
            f[17] = !String.IsNullOrEmpty(mId);
            f[18] = mHasStopMovingTimer;
            f[19] = (mSound != 0);
            f[20] = mStopReset;
            f[21] = !String.IsNullOrEmpty(mLogic);
            f[22] = mForeground;
            f[23] = (mMaxBounceVelocity != 0.0f);
            f[24] = mDrawSort;
            f[25] = mForeground2;
            f[26] = (mSubId != 0);
            f[27] = (mFlipperFlags != 0);
            f[28] = mDrawFloat;
            if (version >= 0x50)
                f[30] = mDrawShadow;
            f.Int32 |= mValueFlags;

            // Before version 0x0F the flags are 24 bits, so values for later bits can not be stored
            if (version < 0x0F)
                f.Int32 &= 0xFFFFFF;

            //Write data
            if (version < 0x0F)
            {
                bw.Write((ushort)f.Int32);
                bw.Write((byte)(f.Int32 >> 16));
            }
            else
            {
                bw.Write(f.Int32);
            }

            if (f[0])
                bw.Write(mRolly);
            if (f[1])
                bw.Write(mBouncy);
            if (f[4])
                bw.Write(mMoveTime);
            if (f[8])
                bw.Write(mSolidColor);
            if (f[9])
                bw.Write(mOutlineColor);
            if (f[10])
            {
                LevelWriter.WritePopcapString(bw, mImageFilename);
            }
            if (f[11])
                bw.Write(mImageDX);
            if (f[12])
                bw.Write(mImageDY);
            if (f[13])
                bw.Write(mImageRotation);
            if (f[16])
                bw.Write(mDisappearTimer);
            if (f[17])
            {
                LevelWriter.WritePopcapString(bw, mId);
            }
            if (f[18])
                bw.Write(mStopMovingTimer);
            if (f[19])
                bw.Write(mSound);
            if (f[21])
            {
                LevelWriter.WritePopcapString(bw, mLogic);
            }
            if (f[23])
                bw.Write(mMaxBounceVelocity);
            if (f[26])
                bw.Write(mSubId);
            if (f[27])
                bw.Write(mFlipperFlags);
            if (f[2])
                mPegInfo.WriteData(bw, version);
            if (f[3])
                MoverLink.WriteData(bw, version);
        }

        public virtual void ReadData(BinaryReader br, int version)
        {

        }

        public virtual void WriteData(BinaryWriter bw, int version)
        {

        }

        public virtual void RenderShadow(Graphics g)
        {

        }

        /// <summary>
        /// Draws the object's image as the game does: centred on <paramref name="location"/>, moved by the image's
        /// centre offset (ImageDX, ImageDY).
        /// </summary>
        protected void DrawObjectImage(Graphics g, Image image, PointF location)
        {
            g.DrawImage(PremultipliedImage.Get(image),
                location.X + mImageDX - (image.Width / 2.0f),
                location.Y + mImageDY - (image.Height / 2.0f),
                image.Width,
                image.Height);
        }

        public virtual void Draw(Graphics g)
        {
            if (Level.ShowPreview && !mVisible)
                return;

            if (MoverLink?.Mover is Mover movement)
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

        public Mover GetBaseMover()
        {
            Mover mb = null;
            var m = MoverLink?.Mover;
            while (m != null)
            {
                mb = m;
                m = m.MoverLink?.Mover;
            }
            return mb;
        }

        public Mover GetMover()
        {
            return MoverLink?.Mover;
        }

        private Mover GetHighestSelectedMover()
        {
            Mover mb = null;
            var m = MoverLink?.Mover;
            while (m != null)
            {
                mb = m;
                m = m.MoverLink?.Mover;
                var entry = mLevel.GetFirstLevelEntryWithMover(m);
                if (entry == null || !entry.Selected)
                {
                    break;
                }
            }
            return mb;
        }

        /// <summary>
        /// Reads the entry's position, as stored in the level file (not that of its mover).
        /// </summary>
        protected void ReadPosition(BinaryReader br)
        {
            mX = br.ReadSingle();
            mY = br.ReadSingle();
        }

        public PointF GetSelectedMovePosition()
        {
            var movement = GetHighestSelectedMover();
            if (movement != null)
            {
                return new PointF(movement.AnchorPointX, movement.AnchorPointY);
            }
            return new PointF(mX, mY);
        }

        public void SetSelectedMovePosition(PointF value)
        {
            var movement = GetHighestSelectedMover();
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
        public bool HasMover
        {
            get
            {
                return (MoverLink != null);
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
                if (MoverLink?.Mover is Mover movement)
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
            else
            {
                dest.mPegInfo = null;
            }

            dest.MoverLink = MoverLink?.Clone();
            dest.mImageFilename = mImageFilename;
            dest.mId = mId;
            dest.mLogic = mLogic;
            dest.mCollidable = mCollidable;
            dest.mVisible = mVisible;
            dest.mMoving = mMoving;
            dest.mBackground = mBackground;
            dest.mStopReset = mStopReset;
            dest.mForeground = mForeground;
            dest.mDrawSort = mDrawSort;
            dest.mForeground2 = mForeground2;
            dest.mDrawFloat = mDrawFloat;
            dest.mBaseObject = mBaseObject;
            dest.mRolly = mRolly;
            dest.mBouncy = mBouncy;
            dest.mSolidColor = mSolidColor;
            dest.mOutlineColor = mOutlineColor;
            dest.mImageDX = mImageDX;
            dest.mImageDY = mImageDY;
            dest.mImageRotation = mImageRotation;
            dest.mSound = mSound;
            dest.mMaxBounceVelocity = mMaxBounceVelocity;
            dest.mSubId = mSubId;
            dest.mFlipperFlags = mFlipperFlags;
            dest.mDrawShadow = mDrawShadow;
            dest.mHasMoveTime = mHasMoveTime;
            dest.mMoveTime = mMoveTime;
            dest.mHasDisappearTimer = mHasDisappearTimer;
            dest.mDisappearTimer = mDisappearTimer;
            dest.mHasStopMovingTimer = mHasStopMovingTimer;
            dest.mStopMovingTimer = mStopMovingTimer;
            dest.mUnknownFlags = mUnknownFlags;
            dest.mValueFlags = mValueFlags;
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
        [DisplayName("Draw Shadow")]
        [Description("Whether a shadow is drawn for this object. Only stored in Peggle Nights levels.")]
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool DrawShadow
        {
            get
            {
                return mDrawShadow;
            }
            set
            {
                mDrawShadow = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [DisplayName("Collidable")]
        [Description("Whether the ball bounces off this object. When disabled, the ball passes straight through it.")]
        [Category("Behaviour")]
        [DefaultValue(true)]
        public bool Collidable
        {
            get
            {
                return mCollidable;
            }
            set
            {
                mCollidable = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [DisplayName("Visible")]
        [Description("Whether the object is drawn.")]
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
        [DisplayName("Image")]
        [Description("The image drawn for this object: a path in the PAK file, without the extension (PopCap: Image Name).")]
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
        [DisplayName("Id")]
        [Description("The identifier of the object, used by the game logic and by other objects to refer to it. Some identifiers have a built-in meaning, e.g. freeball.")]
        [Category("Behaviour")]
        [DefaultValue("")]
        public string Id
        {
            get
            {
                return mId;
            }
            set
            {
                mId = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, null)]
        [DisplayName("Logic")]
        [Description("Game logic attached to this object.")]
        [Category("Behaviour")]
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
        [DisplayName("Moving")]
        [Description("Whether the object moves along its movement path. When disabled, the object keeps its movement but stays still until the game logic starts it.")]
        [Category("Behaviour")]
        [DefaultValue(true)]
        public bool Moving
        {
            get
            {
                return mMoving;
            }
            set
            {
                mMoving = value;
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
        [DisplayName("Stop Reset")]
        [Description("The exact effect is not known.")]
        [Category("Behaviour")]
        [DefaultValue(false)]
        public bool StopReset
        {
            get
            {
                return mStopReset;
            }
            set
            {
                mStopReset = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, false)]
        [DisplayName("Draw Sort")]
        [Description("The exact effect is not known.")]
        [Category("Appearance")]
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
        [DisplayName("Draw Float")]
        [Description("The exact effect is not known.")]
        [Category("Appearance")]
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
        [DisplayName("Base Object")]
        [Description("The exact effect is not known.")]
        [Category("Behaviour")]
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
        [Description("How rolly the surface of the object is. 0 uses the game default.")]
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
        [Description("How bouncy the object is. 0 uses the game default. The PopCap level editor offers 0.9 (ball standard), 0.8 (semi bouncy), 0.75 (poly standard), 0.65 (less bouncy) and 0.55 (little bounce).")]
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

        [EntryProperty(EntryPropertyType.Attribute, typeof(Color), "")]
        [DisplayName("Solid Color")]
        [Description("The fill colour of the object when it has no image. Leave empty for no fill colour.")]
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color SolidColor
        {
            get
            {
                return (mSolidColor & 0xFFFFFF) == NoSolidColor ? Color.Empty : ToColor(mSolidColor);
            }
            set
            {
                if (value.IsEmpty)
                {
                    mSolidColor = NoSolidColor;
                }
                else
                {
                    // 0 means no colour, so black is stored as 0x000001, as PopCap's level editor does
                    uint rgb = FromColor(value);
                    mSolidColor = (mSolidColor & 0xFF000000) | (rgb == 0 ? 1 : rgb);
                }
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, typeof(Color), "White")]
        [DisplayName("Outline Color")]
        [Description("The outline colour of the object when it has no image.")]
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "White")]
        public Color OutlineColor
        {
            get
            {
                return ToColor(mOutlineColor);
            }
            set
            {
                mOutlineColor = (mOutlineColor & 0xFF000000) | FromColor(value);
            }
        }

        /// <summary>
        /// Whether an outline colour other than the game's default is set.
        /// </summary>
        [Browsable(false)]
        public bool HasOutlineColor => mOutlineColor != DefaultOutlineColor;

        private static Color ToColor(uint rgb) => Color.FromArgb(255, (int)((rgb >> 16) & 0xFF), (int)((rgb >> 8) & 0xFF), (int)(rgb & 0xFF));

        private static uint FromColor(Color colour) => (uint)colour.ToArgb() & 0xFFFFFF;

        [EntryProperty(EntryPropertyType.Attribute, 0.0f)]
        [DisplayName("Image Center DX")]
        [Description("The horizontal offset of the centre of the image (PopCap: center-dx).")]
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
        [DisplayName("Image Center DY")]
        [Description("The vertical offset of the centre of the image (PopCap: center-dy).")]
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
        [Description("The rotation of the image in degrees.")]
        [Category("Appearance")]
        [DefaultValue(0.0f)]
        public float ImageRotation
        {
            get
            {
                return MathExt.ToDegrees(mImageRotation);
            }
            set
            {
                mImageRotation = MathExt.ToRadians(value);
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, (byte)0)]
        [DisplayName("Sound")]
        [Description("The sound played when the ball hits the object. PopCap levels use 1 for free ball holes, 2 for bumper holes, 3 for flippers and 4 for fever bonus objects.")]
        [Category("Behaviour")]
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
        [DisplayName("Max Bounce Vel")]
        [Description("The maximum speed of the ball after it bounces off this object. 0 specifies not to use the attribute.")]
        [Category("Behaviour")]
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
        [DisplayName("Sub Id")]
        [Description("A second identifier of the object, used by the game logic.")]
        [Category("Behaviour")]
        [DefaultValue(0)]
        public int SubId
        {
            get
            {
                return mSubId;
            }
            set
            {
                mSubId = value;
            }
        }

        [EntryProperty(EntryPropertyType.Attribute, FlipperSetting.Default)]
        [DisplayName("Flipper Flags")]
        [Description("Whether the object is taken away while Claude's flippers are out (PopCap: FlipperFlags). Default takes it away if it is in the way of a flipper, Off always takes it away and On never does.")]
        [Category("Behaviour")]
        [DefaultValue(FlipperSetting.Default)]
        public FlipperSetting FlipperFlags
        {
            get
            {
                return (FlipperSetting)mFlipperFlags;
            }
            set
            {
                mFlipperFlags = (byte)value;
            }
        }

        /// <summary>
        /// Whether the game takes the object away while Claude's flippers are out.
        /// </summary>
        [DisplayName("Taken Away By Flippers")]
        [Description("Whether the game takes this object away while Claude's flippers are out, by its Flipper Flags and whether it is in the way of a flipper. Turn on Flippers in View > Game to see which objects are taken away.")]
        [Category("Behaviour")]
        public bool TakenAwayByFlippers => BaseObjects.IsTakenAwayByFlippers(this);

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
                if (MoverLink != null)
                    MoverLink.Level = Level;
            }
        }
    }
}

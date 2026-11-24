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
using System.Drawing.Drawing2D;
using System.IO;
using IntelOrca.PeggleEdit.Tools.Extensions;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// Represents the emitter level entry.
    /// </summary>
    public class Emitter : LevelEntry
    {
        bool mAdditive;
        bool mRandomStart;

        string mImageFirstCopy;
        int mWidth;
        int mHeight;

        int mDataVersion;
        int mFastForward;
        float mHardwareRate;
        string mTriggerObj;
        byte mRateIsLooping;
        bool mHasPosCurves;
        int mUnknownFlags = 1 << 14; // Unknown flag bits; bit 14 is TriggerSleep, which PopCap sets on almost every emitter
        CurvedVal mPosX = new CurvedVal();
        CurvedVal mPosY = new CurvedVal();

        string mImage;
        float mRandRate;
        float mSoftwareRate;
        int mMax;
        float mSpiralCount;
        float mAlphaFadeAt;
        float mAlphaInto;
        float mDuration;
        CurvedVal mRate = new CurvedVal();
        CurvedVal mDistrib = new CurvedVal();

        bool mChangeRotation;
        CurvedVal mRotation = new CurvedVal();
        CurvedVal mRotationVel = new CurvedVal();
        float mRotationRand;

        bool mChangeScale;
        CurvedVal mScale = new CurvedVal();
        CurvedVal mScaleVel = new CurvedVal();
        float mScaleRand;

        bool mChangeColor;
        CurvedVal mColorR = new CurvedVal();
        CurvedVal mColorG = new CurvedVal();
        CurvedVal mColorB = new CurvedVal();

        bool mChangeAlpha;
        CurvedVal mAlpha = new CurvedVal();

        bool mChangeVelocity;
        CurvedVal mVelX = new CurvedVal();
        CurvedVal mVelY = new CurvedVal();
        float mVelXRand;
        float mVelYRand;
        float mAccX;
        float mGravity;

        bool mChangePolar;
        float mPolarVel;
        float mPolarVelRand;
        float mPolarAcc;
        float mVelRot;
        float mVelRotRand;

        bool mHasVelocity;
        float mVelocityX;
        float mVelocityY;

        public Emitter(Level level)
            : base(level)
        {
            Visible = true;

            DefaultValues();
        }

        public void DefaultValues()
        {
            mDataVersion = 2;
            mFastForward = (int)0;
            mHardwareRate = 1.0f;
            mRateIsLooping = (byte)1;

            mAlpha = 1.0f;
            mMax = 100;
            mWidth = 100;
            mHeight = 100;
            mDistrib = 1.0f;
            mRate = 0.01f;
            mAlphaFadeAt = 1.9f;
            mAlphaInto = 0.1f;
            mDuration = 2.0f;
        }

        public override void ReadData(BinaryReader br, int version)
        {
            FlagGroup fA;

            mDataVersion = br.ReadInt32();

            fA = new FlagGroup(br.ReadInt16());
            mUnknownFlags = fA.Int32 & ((1 << 0) | (1 << 1) | (1 << 3) | (1 << 14) | (1 << 15));

            mAdditive = fA[2];

            mRandomStart = fA[4];
            mHasVelocity = fA[6];
            mChangeScale = fA[7];
            mChangeColor = fA[8];
            mChangeAlpha = fA[9];
            mChangeVelocity = fA[10];
            mChangePolar = fA[11];
            mChangeRotation = fA[12];
            mHasPosCurves = fA[13];

            mImageFirstCopy = LevelReader.ReadPopcapString(br);
            mWidth = br.ReadInt32();
            mHeight = br.ReadInt32();

            if (mDataVersion >= 2)
            {
                mFastForward = br.ReadInt32();
                mHardwareRate = br.ReadSingle();
                mTriggerObj = LevelReader.ReadPopcapString(br);
                mRateIsLooping = br.ReadByte();

                if (fA[13])
                {
                    mPosX = new CurvedVal(br);
                    mPosY = new CurvedVal(br);
                }
            }

            if (fA[5])
            {
                ReadPosition(br);
            }

            mImage = LevelReader.ReadPopcapString(br);
            mRandRate = br.ReadSingle();
            mSoftwareRate = br.ReadSingle();
            mSpiralCount = br.ReadSingle();
            mMax = br.ReadInt32();

            mAlphaFadeAt = br.ReadSingle();
            mAlphaInto = br.ReadSingle();
            mDuration = br.ReadSingle();

            mRate = new CurvedVal(br);
            mDistrib = new CurvedVal(br);

            if (fA[12])
            {
                mRotation = new CurvedVal(br);
                mRotationVel = new CurvedVal(br);
                mRotationRand = br.ReadSingle();
            }

            if (fA[7])
            {
                mScale = new CurvedVal(br);
                mScaleVel = new CurvedVal(br);
                mScaleRand = br.ReadSingle();
            }

            if (fA[8])
            {
                mColorR = new CurvedVal(br);
                mColorG = new CurvedVal(br);
                mColorB = new CurvedVal(br);
            }

            if (fA[9])
            {
                mAlpha = new CurvedVal(br);
            }

            if (fA[10])
            {
                mVelX = new CurvedVal(br);
                mVelY = new CurvedVal(br);
                mVelXRand = br.ReadSingle();
                mVelYRand = br.ReadSingle();
                mAccX = br.ReadSingle();
                mGravity = br.ReadSingle();
            }

            if (fA[11])
            {
                mPolarVel = br.ReadSingle();
                mPolarVelRand = br.ReadSingle();
                mPolarAcc = br.ReadSingle();
                mVelRot = br.ReadSingle();
                mVelRotRand = br.ReadSingle();
            }

            if (fA[6])
            {
                mVelocityX = br.ReadSingle();
                mVelocityY = br.ReadSingle();
            }


        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup(mUnknownFlags);

            fA[2] = mAdditive;
            fA[4] = mRandomStart;

            if (!HasMover)
                fA[5] = true;

            fA[6] = mHasVelocity;
            fA[7] = mChangeScale;
            fA[8] = mChangeColor;
            fA[9] = mChangeAlpha;
            fA[10] = mChangeVelocity;
            fA[11] = mChangePolar;
            fA[12] = mChangeRotation;
            fA[13] = mHasPosCurves;

            bw.Write(mDataVersion);

            bw.Write(fA.Int16);

            LevelWriter.WritePopcapString(bw, mImageFirstCopy);
            bw.Write(mWidth);
            bw.Write(mHeight);

            if (mDataVersion >= 2)
            {
                bw.Write(mFastForward);
                bw.Write(mHardwareRate);
                LevelWriter.WritePopcapString(bw, mTriggerObj);
                bw.Write(mRateIsLooping);

                if (fA[13])
                {
                    mPosX.WriteData(bw);
                    mPosY.WriteData(bw);
                }
            }

            if (fA[5])
            {
                bw.Write(X);
                bw.Write(Y);
            }

            LevelWriter.WritePopcapString(bw, mImage);
            bw.Write(mRandRate);
            bw.Write(mSoftwareRate);
            bw.Write(mSpiralCount);
            bw.Write(mMax);

            bw.Write(mAlphaFadeAt);
            bw.Write(mAlphaInto);
            bw.Write(mDuration);

            mRate.WriteData(bw);
            mDistrib.WriteData(bw);

            if (fA[12])
            {
                mRotation.WriteData(bw);
                mRotationVel.WriteData(bw);
                bw.Write(mRotationRand);
            }

            if (fA[7])
            {
                mScale.WriteData(bw);
                mScaleVel.WriteData(bw);
                bw.Write(mScaleRand);
            }

            if (fA[8])
            {
                mColorR.WriteData(bw);
                mColorG.WriteData(bw);
                mColorB.WriteData(bw);
            }

            if (fA[9])
            {
                mAlpha.WriteData(bw);
            }

            if (fA[10])
            {
                mVelX.WriteData(bw);
                mVelY.WriteData(bw);

                bw.Write(mVelXRand);
                bw.Write(mVelYRand);
                bw.Write(mAccX);
                bw.Write(mGravity);
            }

            if (fA[11])
            {
                bw.Write(mPolarVel);
                bw.Write(mPolarVelRand);
                bw.Write(mPolarAcc);
                bw.Write(mVelRot);
                bw.Write(mVelRotRand);
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

            // In the preview, the level draws the game's debug box, with the number of particles
            if (Level.ShowPreview)
                return;

            if (!ShowsDebugBox)
            {
                var bounds = Bounds;
                g.DrawRectangle(Pens.Blue, bounds);
                bounds.Inflate(-1, -1);
                g.DrawRectangle(Pens.LightBlue, bounds);
            }
            else
            {
                DrawDebugBox(g);
            }
        }

        /// <summary>
        /// Whether the game draws a debug box around the emitter: whenever its outline colour is not black, including
        /// when it is not set, as the game's default outline colour is white.
        /// </summary>
        [Browsable(false)]
        public bool ShowsDebugBox => OutlineColor.ToArgb() != Color.Black.ToArgb();

        /// <summary>
        /// Draws the game's debug box: a band 3 pixels wide in the outline colour just inside the emitter's area, with
        /// a black line around it (measured in game), and the number of particles next to it.
        /// </summary>
        internal void DrawDebugBox(Graphics g, int? particles = null, int? peak = null)
        {
            var mode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.None;
            var location = DrawLocation;
            int left = (int)(location.X - (mWidth / 2));
            int top = (int)(location.Y - (mHeight / 2));
            int right = left + mWidth;
            int bottom = top + mHeight;
            g.DrawRectangle(Pens.Black, left - 1, top - 1, right - left + 2, bottom - top + 2);
            using (var band = new SolidBrush(OutlineColor))
            {
                g.FillRectangle(band, left, top, right - left + 1, 3);
                g.FillRectangle(band, left, bottom - 2, right - left + 1, 3);
                g.FillRectangle(band, left, top, 3, bottom - top + 1);
                g.FillRectangle(band, right - 2, top, 3, bottom - top + 1);
            }
            if (particles is int count)
            {
                using (var font = new Font("Arial", 15, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    float x = right + 16;
                    float y = location.Y - 14;
                    g.DrawString($"{count} particles", font, Brushes.White, x, y);
                    g.DrawString($"({peak ?? count} peak)", font, Brushes.White, x, y + 15);
                }
            }
            g.SmoothingMode = mode;
        }

        public override object Clone()
        {
            Emitter cpyEmitter = new Emitter(Level);
            base.CloneTo(cpyEmitter);

            cpyEmitter.mImageFirstCopy = mImageFirstCopy;
            cpyEmitter.mImage = mImage;
            cpyEmitter.mAdditive = mAdditive;

            cpyEmitter.mChangeAlpha = mChangeAlpha;
            cpyEmitter.mAlpha = (CurvedVal)mAlpha.Clone();

            cpyEmitter.mRandomStart = mRandomStart;
            cpyEmitter.mMax = mMax;
            cpyEmitter.mSpiralCount = mSpiralCount;

            cpyEmitter.mWidth = mWidth;
            cpyEmitter.mHeight = mHeight;
            cpyEmitter.mDistrib = (CurvedVal)mDistrib.Clone();

            cpyEmitter.mRate = (CurvedVal)mRate.Clone();
            cpyEmitter.mAlphaFadeAt = mAlphaFadeAt;
            cpyEmitter.mAlphaInto = mAlphaInto;
            cpyEmitter.mDuration = mDuration;

            cpyEmitter.mChangeRotation = mChangeRotation;
            cpyEmitter.mRotation = (CurvedVal)mRotation.Clone();
            cpyEmitter.mRotationVel = (CurvedVal)mRotationVel.Clone();
            cpyEmitter.mRotationRand = mRotationRand;

            cpyEmitter.mChangeScale = mChangeScale;
            cpyEmitter.mScale = (CurvedVal)mScale.Clone();
            cpyEmitter.mScaleVel = (CurvedVal)mScaleVel.Clone();
            cpyEmitter.mScaleRand = mScaleRand;

            cpyEmitter.mChangeVelocity = mChangeVelocity;
            cpyEmitter.mVelX = (CurvedVal)mVelX.Clone();
            cpyEmitter.mVelY = (CurvedVal)mVelY.Clone();
            cpyEmitter.mVelXRand = mVelXRand;
            cpyEmitter.mVelYRand = mVelYRand;
            cpyEmitter.mAccX = mAccX;
            cpyEmitter.mGravity = mGravity;

            cpyEmitter.mDataVersion = mDataVersion;
            cpyEmitter.mFastForward = mFastForward;
            cpyEmitter.mHardwareRate = mHardwareRate;
            cpyEmitter.mTriggerObj = mTriggerObj;
            cpyEmitter.mRateIsLooping = mRateIsLooping;
            cpyEmitter.mHasPosCurves = mHasPosCurves;
            cpyEmitter.mUnknownFlags = mUnknownFlags;
            cpyEmitter.mPosX = (CurvedVal)mPosX.Clone();
            cpyEmitter.mPosY = (CurvedVal)mPosY.Clone();
            cpyEmitter.mRandRate = mRandRate;
            cpyEmitter.mSoftwareRate = mSoftwareRate;

            cpyEmitter.mChangeColor = mChangeColor;
            cpyEmitter.mColorR = (CurvedVal)mColorR.Clone();
            cpyEmitter.mColorG = (CurvedVal)mColorG.Clone();
            cpyEmitter.mColorB = (CurvedVal)mColorB.Clone();

            cpyEmitter.mChangePolar = mChangePolar;
            cpyEmitter.mPolarVel = mPolarVel;
            cpyEmitter.mPolarVelRand = mPolarVelRand;
            cpyEmitter.mPolarAcc = mPolarAcc;
            cpyEmitter.mVelRot = mVelRot;
            cpyEmitter.mVelRotRand = mVelRotRand;

            cpyEmitter.mHasVelocity = mHasVelocity;
            cpyEmitter.mVelocityX = mVelocityX;
            cpyEmitter.mVelocityY = mVelocityY;

            return cpyEmitter;
        }

        public override int Type
        {
            get
            {
                return LevelEntryTypes.Emitter;
            }
        }

        public override RectangleF Bounds
        {
            get
            {
                return new RectangleF(DrawX - (mWidth / 2), DrawY - (mHeight / 2), mWidth, mHeight);
            }
        }

        [DisplayName("Outline Color")]
        [Description("Unless this is black, the game draws a box of this colour around the emitter with the number of particles next to it, for debugging. The game's default (when it is not set) is white, so emitters normally set it to black.")]
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Black")]
        public new Color OutlineColor
        {
            get => base.OutlineColor;
            set => base.OutlineColor = value;
        }

        public float SpiralCount
        {
            get
            {
                return mSpiralCount;
            }
            set
            {
                mSpiralCount = value;
            }
        }

        public int DataVersion
        {
            get
            {
                return mDataVersion;
            }
            set
            {
                mDataVersion = value;
            }
        }

        public CurvedVal PosX
        {
            get
            {
                return mPosX;
            }
            set
            {
                mPosX = value;
            }
        }

        public CurvedVal PosY
        {
            get
            {
                return mPosY;
            }
            set
            {
                mPosY = value;
            }
        }

        /// <summary>
        /// Whether the offset curves (PosX and PosY) are stored.
        /// </summary>
        [Browsable(false)]
        public bool HasPosCurves => mHasPosCurves;

        [DisplayName("Software Rate")]
        [Description("The emit rate multiplier used without 3D acceleration. With 0 the emitter is not shown at all without 3D acceleration.")]
        [Category("Timing")]
        public float SoftwareRate
        {
            get
            {
                return mSoftwareRate;
            }
            set
            {
                mSoftwareRate = value;
            }
        }

        [DisplayName("Particle Image (first copy)")]
        [Description("The game stores the image twice and only uses the second copy, Particle Image. Normally both are the same.")]
        [Category("Appearance")]
        [DefaultValue("")]
        public string ImageFirstCopy
        {
            get
            {
                return mImageFirstCopy;
            }
            set
            {
                mImageFirstCopy = value;
            }
        }

        [DisplayName("Particle Image")]
        [Description("The image of the emitted objects.")]
        [Category("Appearance")]
        [DefaultValue("")]
        public string Image
        {
            get
            {
                return mImage;
            }
            set
            {
                mImage = value;
            }
        }

        [DisplayName("Additive")]
        [Description("Draw the emitted objects with additive blending, so dark areas become transparent.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool Additive
        {
            get
            {
                return mAdditive;
            }
            set
            {
                mAdditive = value;
            }
        }

        private bool GetFlag(int bit) => (mUnknownFlags & (1 << bit)) != 0;

        private void SetFlag(int bit, bool value)
        {
            if (value)
                mUnknownFlags |= 1 << bit;
            else
                mUnknownFlags &= ~(1 << bit);
        }

        [DisplayName("Circular")]
        [Description("Emit from an ellipse instead of a rectangle, and emit a third more often (PopCap: emitter_circular). Only used when Random Start is on.")]
        [Category("Layout")]
        [DefaultValue(false)]
        public bool Circular
        {
            get => GetFlag(0);
            set => SetFlag(0, value);
        }

        [DisplayName("Attached")]
        [Description("Emitted objects move with the emitter when it moves, instead of being left behind (PopCap: emitter_attached).")]
        [Category("Behaviour")]
        [DefaultValue(false)]
        public bool Attached
        {
            get => GetFlag(1);
            set => SetFlag(1, value);
        }

        [DisplayName("Orient To Velocity")]
        [Description("Turn each emitted object to face the way it is moving (PopCap: emitter_orientrottovel).")]
        [Category("Rotation")]
        [DefaultValue(false)]
        public bool OrientRotToVel
        {
            get => GetFlag(3);
            set => SetFlag(3, value);
        }

        [DisplayName("Starts Emitting")]
        [Description("Whether the emitter emits from the start. When off, it waits until the ball hits the object whose id is the Trigger Object, and each hit turns it on or off (PopCap: emitter_triggersleep).")]
        [Category("Behaviour")]
        [DefaultValue(true)]
        public bool TriggerSleep
        {
            get => GetFlag(14);
            set => SetFlag(14, value);
        }

        [DisplayName("Change Opacity")]
        [Description("Whether the opacity is set. When off, the objects are opaque apart from fading in and out.")]
        [Category("Opacity")]
        [DefaultValue(false)]
        public bool ChangeAlpha
        {
            get
            {
                return mChangeAlpha;
            }
            set
            {
                mChangeAlpha = value;
            }
        }

        [Description("How opaque the emitted objects are over their life. 0.0 is transparent, 1.0 is opaque (PopCap: Alpha).")]
        [Category("Opacity")]
        [DefaultValue(1.0f)]
        public CurvedVal Alpha
        {
            get
            {
                return mAlpha;
            }
            set
            {
                mAlpha = value;
            }
        }

        [DisplayName("Random Start")]
        [Description("Whether objects are emitted at random positions within the area.")]
        [Category("Behaviour")]
        [DefaultValue(false)]
        public bool RandomStart
        {
            get
            {
                return mRandomStart;
            }
            set
            {
                mRandomStart = value;
            }
        }

        [DisplayName("Max")]
        [Description("The maximum number of emitted objects at one time. While there are this many, no more are emitted.")]
        [Category("Behaviour")]
        [DefaultValue(100)]
        public int Max
        {
            get
            {
                return mMax;
            }
            set
            {
                mMax = value;
            }
        }


        [DisplayName("Distrib")]
        [Description("How far out over the area the objects are emitted, e.g. 0.5 for the middle half. A curve gives how often each distance is used.")]
        [Category("Layout")]
        [DefaultValue(1.0f)]
        public CurvedVal Distrib
        {
            get
            {
                return mDistrib;
            }
            set
            {
                mDistrib = value;
            }
        }

        [Description("The width of the area in which the objects can be emitted.")]
        [Category("Layout")]
        [DefaultValue(0.0f)]
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

        [Description("The height of the area in which the objects can be emitted.")]
        [Category("Layout")]
        [DefaultValue(0.0f)]
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

        [DisplayName("Duration")]
        [Description("How long each emitted object lasts before disappearing, in seconds.")]
        [Category("Timing")]
        [DefaultValue(0.0f)]
        public float Duration
        {
            get
            {
                return mDuration;
            }
            set
            {
                mDuration = value;
            }
        }

        [DisplayName("Rate")]
        [Description("How many objects are emitted every hundredth of a second: 0.01 is one a second, 1 is a hundred a second. A curve is stepped through by its rate every hundredth of a second.")]
        [Category("Timing")]
        [DefaultValue(0.0f)]
        public CurvedVal Rate
        {
            get
            {
                return mRate;
            }
            set
            {
                mRate = value;
            }
        }

        [DisplayName("Rand Rate")]
        [Description("A random amount from 0 to this added to the rate every hundredth of a second.")]
        [Category("Timing")]
        [DefaultValue(0.0f)]
        public float RandRate
        {
            get
            {
                return mRandRate;
            }
            set
            {
                mRandRate = value;
            }
        }

        [DisplayName("Alpha Into")]
        [Description("How long an emitted object takes to fade in, in seconds.")]
        [Category("Timing")]
        [DefaultValue(0.1f)]
        public float AlphaInto
        {
            get
            {
                return mAlphaInto;
            }
            set
            {
                mAlphaInto = value;
            }
        }

        [DisplayName("Fade Out Duration")]
        [Description("The duration of the fading out before the emitted object disappears. Based on life duration.")]
        [Category("Timing")]
        [DefaultValue(0.1f)]
        public float FadeOutTime
        {
            get
            {
                return (float)Math.Round(mDuration - mAlphaFadeAt, 4);
            }
            set
            {
                mAlphaFadeAt = (float)Math.Round(mDuration - value, 4);
            }
        }

        [DisplayName("Alpha Fade At")]
        [Description("How long before an emitted object starts to fade out, in seconds.")]
        [Category("Timing")]
        [DefaultValue(0.0f)]
        public float AlphaFadeAt
        {
            get
            {
                return mAlphaFadeAt;
            }
            set
            {
                mAlphaFadeAt = value;
            }
        }

        [DisplayName("Change Velocity")]
        [Description("If this is set to false, the velocity attributes will be ignored.")]
        [Category("Velocity")]
        [DefaultValue(false)]
        public bool ChangeVelocity
        {
            get
            {
                return mChangeVelocity;
            }
            set
            {
                mChangeVelocity = value;
            }
        }

        [DisplayName("Vel X")]
        [Description("The horizontal velocity of the emitted objects. Negative moves left and positive moves right.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public CurvedVal VelX
        {
            get
            {
                return mVelX;
            }
            set
            {
                mVelX = value;
            }
        }

        [DisplayName("Vel Y")]
        [Description("The vertical velocity of the emitted objects. Negative moves up and positive moves down.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public CurvedVal VelY
        {
            get
            {
                return mVelY;
            }
            set
            {
                mVelY = value;
            }
        }

        [DisplayName("Vel X Rand")]
        [Description("A random amount added to the horizontal velocity of each emitted object.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public float VelXRand
        {
            get
            {
                return mVelXRand;
            }
            set
            {
                mVelXRand = value;
            }
        }

        [DisplayName("Vel Y Rand")]
        [Description("A random amount added to the vertical velocity of each emitted object.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public float VelYRand
        {
            get
            {
                return mVelYRand;
            }
            set
            {
                mVelYRand = value;
            }
        }

        [DisplayName("Acc X")]
        [Description("The horizontal acceleration of the emitted objects. Negative accelerates left and positive accelerates right.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public float AccX
        {
            get
            {
                return mAccX;
            }
            set
            {
                mAccX = value;
            }
        }

        [DisplayName("Gravity")]
        [Description("The vertical acceleration of the emitted objects. Negative accelerates up and positive accelerates down.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public float Gravity
        {
            get
            {
                return mGravity;
            }
            set
            {
                mGravity = value;
            }
        }

        [DisplayName("Change Scale")]
        [Description("If this is set to false, the scale attributes will be ignored.")]
        [Category("Scale")]
        [DefaultValue(false)]
        public bool ChangeScale
        {
            get
            {
                return mChangeScale;
            }
            set
            {
                mChangeScale = value;
            }
        }

        [DisplayName("Scale")]
        [Description("The scale of the emitted objects over their life.")]
        [Category("Scale")]
        [DefaultValue(0.0f)]
        public CurvedVal Scale
        {
            get
            {
                return mScale;
            }
            set
            {
                mScale = value;
            }
        }

        [DisplayName("Scale Vel")]
        [Description("How much the scale of the emitted objects changes every hundredth of a second. Negative makes them smaller, positive makes them larger. An object disappears when this shrinks it to nothing.")]
        [Category("Scale")]
        [DefaultValue(0.0f)]
        public CurvedVal ScaleVel
        {
            get
            {
                return mScaleVel;
            }
            set
            {
                mScaleVel = value;
            }
        }

        [DisplayName("Scale Rand")]
        [Description("A random amount added to the initial scale of each emitted object.")]
        [Category("Scale")]
        [DefaultValue(0.0f)]
        public float ScaleRand
        {
            get
            {
                return mScaleRand;
            }
            set
            {
                mScaleRand = value;
            }
        }

        [DisplayName("Change Rotation")]
        [Description("If this is set to false, the rotation attributes will be ignored.")]
        [Category("Rotation")]
        [DefaultValue(false)]
        public bool ChangeRotation
        {
            get
            {
                return mChangeRotation;
            }
            set
            {
                mChangeRotation = value;
            }
        }

        [DisplayName("Rotation")]
        [Description("The rotation of the emitted objects in degrees over their life. Positive turns anticlockwise.")]
        [Category("Rotation")]
        [DefaultValue(0.0f)]
        public CurvedVal Rotation
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

        [DisplayName("Rotation Vel")]
        [Description("How fast the emitted objects turn, in degrees every hundredth of a second (1 is 100 degrees a second). Positive turns anticlockwise.")]
        [Category("Rotation")]
        [DefaultValue(0.0f)]
        public CurvedVal RotationVel
        {
            get
            {
                return mRotationVel;
            }
            set
            {
                mRotationVel = value;
            }
        }

        [DisplayName("Rotation Rand")]
        [Description("A random amount from 0 to this, in degrees, added to the rotation of each emitted object.")]
        [Category("Rotation")]
        [DefaultValue(0.0f)]
        public float RotationRand
        {
            get
            {
                return mRotationRand;
            }
            set
            {
                mRotationRand = value;
            }
        }

        [DisplayName("Change Colour")]
        [Description("If this is set to false, the colour attributes will be ignored.")]
        [Category("Colour")]
        [DefaultValue(false)]
        public bool ChangeColor
        {
            get
            {
                return mChangeColor;
            }
            set
            {
                mChangeColor = value;
            }
        }

        [DisplayName("Color R")]
        [Description("The red component of the colour of the emitted objects.")]
        [Category("Colour")]
        [DefaultValue(1.0f)]
        public CurvedVal ColorR
        {
            get
            {
                return mColorR;
            }
            set
            {
                mColorR = value;
            }
        }

        [DisplayName("Color G")]
        [Description("The green component of the colour of the emitted objects.")]
        [Category("Colour")]
        [DefaultValue(1.0f)]
        public CurvedVal ColorG
        {
            get
            {
                return mColorG;
            }
            set
            {
                mColorG = value;
            }
        }

        [DisplayName("Color B")]
        [Description("The blue component of the colour of the emitted objects.")]
        [Category("Colour")]
        [DefaultValue(1.0f)]
        public CurvedVal ColorB
        {
            get
            {
                return mColorB;
            }
            set
            {
                mColorB = value;
            }
        }

        [DisplayName("Change Direction")]
        [Description("If this is set to false, the direction attributes will be ignored.")]
        [Category("Direction")]
        [DefaultValue(false)]
        public bool ChangePolar
        {
            get
            {
                return mChangePolar;
            }
            set
            {
                mChangePolar = value;
            }
        }

        [DisplayName("Polar Vel")]
        [Description("The speed of the emitted objects in the direction given by Vel Rot.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float PolarVel
        {
            get
            {
                return mPolarVel;
            }
            set
            {
                mPolarVel = value;
            }
        }

        [DisplayName("Polar Vel Rand")]
        [Description("A random amount added to Polar Vel for each emitted object.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float PolarVelRand
        {
            get
            {
                return mPolarVelRand;
            }
            set
            {
                mPolarVelRand = value;
            }
        }

        [DisplayName("Polar Acc")]
        [Description("The acceleration of the emitted objects in the direction given by Vel Rot.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float PolarAcc
        {
            get
            {
                return mPolarAcc;
            }
            set
            {
                mPolarAcc = value;
            }
        }

        [DisplayName("Vel Rot")]
        [Description("The direction of the polar velocity and acceleration in degrees.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float VelRot
        {
            get
            {
                return mVelRot;
            }
            set
            {
                mVelRot = value;
            }
        }

        [DisplayName("Vel Rot Rand")]
        [Description("A random amount added to Vel Rot for each emitted object, in degrees.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float VelRotRand
        {
            get
            {
                return mVelRotRand;
            }
            set
            {
                mVelRotRand = value;
            }
        }

        [DisplayName("Has Velocity")]
        [Description("Whether the level stores how far the emitter moved in the last frame. This is game state, not a setting.")]
        [Category("Game State")]
        [DefaultValue(false)]
        public bool HasVelocity
        {
            get
            {
                return mHasVelocity;
            }
            set
            {
                mHasVelocity = value;
            }
        }

        [DisplayName("Velocity X")]
        [Description("How far the emitter moved horizontally in the last frame. This is game state, not a setting.")]
        [Category("Game State")]
        [DefaultValue(0.0f)]
        public float VelocityX
        {
            get
            {
                return mVelocityX;
            }
            set
            {
                mVelocityX = value;
            }
        }

        [DisplayName("Velocity Y")]
        [Description("How far the emitter moved vertically in the last frame. This is game state, not a setting.")]
        [Category("Game State")]
        [DefaultValue(0.0f)]
        public float VelocityY
        {
            get
            {
                return mVelocityY;
            }
            set
            {
                mVelocityY = value;
            }
        }

        [DisplayName("Fast Forward")]
        [Description("How far the emitter is run ahead when the level starts, in hundredths of a second.")]
        [Category("Timing")]
        public int FastForward
        {
            get
            {
                return mFastForward;
            }
            set
            {
                mFastForward = value;
            }
        }

        [DisplayName("Hardware Rate")]
        [Description("The emit rate multiplier used with 3D acceleration, which is how nearly everyone plays.")]
        [Category("Timing")]
        public float HardwareRate
        {
            get
            {
                return mHardwareRate;
            }
            set
            {
                mHardwareRate = value;
            }
        }

        [DisplayName("Trigger Object")]
        [Description("The id of an object that triggers the emitter (PopCap: TriggerObj).")]
        [Category("Behaviour")]
        public string TriggerObj
        {
            get
            {
                return mTriggerObj;
            }
            set
            {
                mTriggerObj = value;
            }
        }

        [DisplayName("Rate Is Looping")]
        [Description("Whether the emit rate curve loops (1) or plays once (0).")]
        [Category("Timing")]
        public byte RateIsLooping
        {
            get
            {
                return mRateIsLooping;
            }
            set
            {
                mRateIsLooping = value;
            }
        }
    }
}

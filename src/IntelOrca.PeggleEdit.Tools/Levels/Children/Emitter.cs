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
        bool mTransparancy;
        bool mRandomStartPosition;

        string mImage;
        int mWidth;
        int mHeight;

        int mMainVar;
        int mMainVar0;
        float mMainVar1;
        string mMainVar2;
        byte mMainVar3;
        bool mHasUnknown01;
        int mUnknownFlags = 1 << 14; // Needed to make it visible?
        VariableFloat mUnknown0 = new VariableFloat();
        VariableFloat mUnknown1 = new VariableFloat();

        string mEmitImage;
        float mUnknownEmitRate;
        float mUnknown2;
        int mMaxQuantity;
        float mRotation;
        float mTimeBeforeFadeOut;
        float mFadeInTime;
        float mLifeDuration;
        VariableFloat mEmitRate = new VariableFloat();
        VariableFloat mEmitAreaMultiplier = new VariableFloat();

        bool mChangeRotation;
        VariableFloat mInitialRotation = new VariableFloat();
        VariableFloat mRotationVelocity = new VariableFloat();
        float mRotationUnknown;

        bool mChangeScale;
        VariableFloat mMinScale = new VariableFloat();
        VariableFloat mScaleVelocity = new VariableFloat();
        float mMaxRandScale;

        bool mChangeColour;
        VariableFloat mColourRed = new VariableFloat();
        VariableFloat mColourGreen = new VariableFloat();
        VariableFloat mColourBlue = new VariableFloat();

        bool mChangeOpacity;
        VariableFloat mOpacity = new VariableFloat();

        bool mChangeVelocity;
        VariableFloat mMinVelocityX = new VariableFloat();
        VariableFloat mMinVelocityY = new VariableFloat();
        float mMaxVelocityX;
        float mMaxVelocityY;
        float mAccelerationX;
        float mAccelerationY;

        bool mChangeDirection;
        float mDirectionSpeed;
        float mDirectionRandomSpeed;
        float mDirectionAcceleration;
        float mDirectionAngle;
        float mDirectionRandomAngle;

        bool mChangeUnknown;
        float mUnknownA;
        float mUnknownB;

        public Emitter(Level level)
            : base(level)
        {
            Visible = true;
            OutlineColour = Color.Black;

            DefaultValues();
        }

        public void DefaultValues()
        {
            mMainVar = 2;
            mMainVar0 = (int)0;
            mMainVar1 = 1.0f;
            mMainVar3 = (byte)1;

            mOpacity = 1.0f;
            mMaxQuantity = 100;
            mWidth = 100;
            mHeight = 100;
            mEmitAreaMultiplier = 1.0f;
            mEmitRate = 0.01f;
            mTimeBeforeFadeOut = 1.9f;
            mFadeInTime = 0.1f;
            mLifeDuration = 2.0f;
        }

        public override void ReadData(BinaryReader br, int version)
        {
            FlagGroup fA;

            mMainVar = br.ReadInt32();

            fA = new FlagGroup(br.ReadInt16());
            mUnknownFlags = fA.Int32 & ((1 << 0) | (1 << 1) | (1 << 3) | (1 << 14) | (1 << 15));

            mTransparancy = fA[2];

            mRandomStartPosition = fA[4];
            mChangeUnknown = fA[6];
            mChangeScale = fA[7];
            mChangeColour = fA[8];
            mChangeOpacity = fA[9];
            mChangeVelocity = fA[10];
            mChangeDirection = fA[11];
            mChangeRotation = fA[12];
            mHasUnknown01 = fA[13];

            mImage = LevelReader.ReadPopcapString(br);
            mWidth = br.ReadInt32();
            mHeight = br.ReadInt32();

            if (mMainVar == 2)
            {
                mMainVar0 = br.ReadInt32();
                mMainVar1 = br.ReadSingle();
                mMainVar2 = LevelReader.ReadPopcapString(br);
                mMainVar3 = br.ReadByte();

                if (fA[13])
                {
                    mUnknown0 = new VariableFloat(br);
                    mUnknown1 = new VariableFloat(br);
                }
            }

            if (fA[5])
            {
                X = br.ReadSingle();
                Y = br.ReadSingle();
            }

            mEmitImage = LevelReader.ReadPopcapString(br);
            mUnknownEmitRate = br.ReadSingle();
            mUnknown2 = br.ReadSingle();
            mRotation = br.ReadSingle();
            mMaxQuantity = br.ReadInt32();

            mTimeBeforeFadeOut = br.ReadSingle();
            mFadeInTime = br.ReadSingle();
            mLifeDuration = br.ReadSingle();

            mEmitRate = new VariableFloat(br);
            mEmitAreaMultiplier = new VariableFloat(br);

            if (fA[12])
            {
                mInitialRotation = new VariableFloat(br);
                mRotationVelocity = new VariableFloat(br);
                mRotationUnknown = br.ReadSingle();
            }

            if (fA[7])
            {
                mMinScale = new VariableFloat(br);
                mScaleVelocity = new VariableFloat(br);
                mMaxRandScale = br.ReadSingle();
            }

            if (fA[8])
            {
                mColourRed = new VariableFloat(br);
                mColourGreen = new VariableFloat(br);
                mColourBlue = new VariableFloat(br);
            }

            if (fA[9])
            {
                mOpacity = new VariableFloat(br);
            }

            if (fA[10])
            {
                mMinVelocityX = new VariableFloat(br);
                mMinVelocityY = new VariableFloat(br);
                mMaxVelocityX = br.ReadSingle();
                mMaxVelocityY = br.ReadSingle();
                mAccelerationX = br.ReadSingle();
                mAccelerationY = br.ReadSingle();
            }

            if (fA[11])
            {
                mDirectionSpeed = br.ReadSingle();
                mDirectionRandomSpeed = br.ReadSingle();
                mDirectionAcceleration = br.ReadSingle();
                mDirectionAngle = br.ReadSingle();
                mDirectionRandomAngle = br.ReadSingle();
            }

            if (fA[6])
            {
                mUnknownA = br.ReadSingle();
                mUnknownB = br.ReadSingle();
            }


        }

        public override void WriteData(BinaryWriter bw, int version)
        {
            FlagGroup fA = new FlagGroup(mUnknownFlags);

            fA[2] = mTransparancy;
            fA[4] = mRandomStartPosition;

            if (!HasMovementInfo)
                fA[5] = true;

            fA[6] = mChangeUnknown;
            fA[7] = mChangeScale;
            fA[8] = mChangeColour;
            fA[9] = mChangeOpacity;
            fA[10] = mChangeVelocity;
            fA[11] = mChangeDirection;
            fA[12] = mChangeRotation;
            fA[13] = mHasUnknown01;

            bw.Write(mMainVar);

            bw.Write(fA.Int16);

            LevelWriter.WritePopcapString(bw, mImage);
            bw.Write(mWidth);
            bw.Write(mHeight);

            if (mMainVar == 2)
            {
                bw.Write(mMainVar0);
                bw.Write(mMainVar1);
                LevelWriter.WritePopcapString(bw, mMainVar2);
                bw.Write(mMainVar3);

                if (fA[13])
                {
                    mUnknown0.WriteData(bw);
                    mUnknown1.WriteData(bw);
                }
            }

            if (fA[5])
            {
                bw.Write(X);
                bw.Write(Y);
            }

            LevelWriter.WritePopcapString(bw, mEmitImage);
            bw.Write(mUnknownEmitRate);
            bw.Write(mUnknown2);
            bw.Write(mRotation);
            bw.Write(mMaxQuantity);

            bw.Write(mTimeBeforeFadeOut);
            bw.Write(mFadeInTime);
            bw.Write(mLifeDuration);

            mEmitRate.WriteData(bw);
            mEmitAreaMultiplier.WriteData(bw);

            if (fA[12])
            {
                mInitialRotation.WriteData(bw);
                mRotationVelocity.WriteData(bw);
                bw.Write(mRotationUnknown);
            }

            if (fA[7])
            {
                mMinScale.WriteData(bw);
                mScaleVelocity.WriteData(bw);
                bw.Write(mMaxRandScale);
            }

            if (fA[8])
            {
                mColourRed.WriteData(bw);
                mColourGreen.WriteData(bw);
                mColourBlue.WriteData(bw);
            }

            if (fA[9])
            {
                mOpacity.WriteData(bw);
            }

            if (fA[10])
            {
                mMinVelocityX.WriteData(bw);
                mMinVelocityY.WriteData(bw);

                bw.Write(mMaxVelocityX);
                bw.Write(mMaxVelocityY);
                bw.Write(mAccelerationX);
                bw.Write(mAccelerationY);
            }

            if (fA[11])
            {
                bw.Write(mDirectionSpeed);
                bw.Write(mDirectionRandomSpeed);
                bw.Write(mDirectionAcceleration);
                bw.Write(mDirectionAngle);
                bw.Write(mDirectionRandomAngle);
            }

            if (fA[6])
            {
                bw.Write(mUnknownA);
                bw.Write(mUnknownB);
            }
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);

            var hasOutline = OutlineColour.R != 0 || OutlineColour.G != 0 || OutlineColour.B != 0;
            if (!Level.ShowPreview && !hasOutline)
            {
                var bounds = Bounds;
                g.DrawRectangle(Pens.Blue, bounds);
                bounds.Inflate(-1, -1);
                g.DrawRectangle(Pens.LightBlue, bounds);
            }
            else if (!Level.ShowPreview || hasOutline)
            {
                g.SmoothingMode = SmoothingMode.None;
                var bounds = Bounds;
                g.DrawRectangle(Pens.Black, bounds);
                bounds.Inflate(-2, -2);
                g.DrawRectangle(new Pen(OutlineColour, 3), bounds);
                g.SmoothingMode = SmoothingMode.HighQuality;
            }
        }

        public override object Clone()
        {
            Emitter cpyEmitter = new Emitter(Level);
            base.CloneTo(cpyEmitter);

            cpyEmitter.mImage = mImage;
            cpyEmitter.mEmitImage = mEmitImage;
            cpyEmitter.mTransparancy = mTransparancy;

            cpyEmitter.mChangeOpacity = mChangeOpacity;
            cpyEmitter.mOpacity = (VariableFloat)mOpacity.Clone();

            cpyEmitter.mRandomStartPosition = mRandomStartPosition;
            cpyEmitter.mMaxQuantity = mMaxQuantity;
            cpyEmitter.mRotation = mRotation;

            cpyEmitter.mWidth = mWidth;
            cpyEmitter.mHeight = mHeight;
            cpyEmitter.mEmitAreaMultiplier = (VariableFloat)mEmitAreaMultiplier.Clone();

            cpyEmitter.mEmitRate = (VariableFloat)mEmitRate.Clone();
            cpyEmitter.mTimeBeforeFadeOut = mTimeBeforeFadeOut;
            cpyEmitter.mFadeInTime = mFadeInTime;
            cpyEmitter.mLifeDuration = mLifeDuration;

            cpyEmitter.mChangeRotation = mChangeRotation;
            cpyEmitter.mInitialRotation = (VariableFloat)mInitialRotation.Clone();
            cpyEmitter.mRotationVelocity = (VariableFloat)mRotationVelocity.Clone();
            cpyEmitter.mRotationUnknown = mRotationUnknown;

            cpyEmitter.mChangeScale = mChangeScale;
            cpyEmitter.mMinScale = (VariableFloat)mMinScale.Clone();
            cpyEmitter.mScaleVelocity = (VariableFloat)mScaleVelocity.Clone();
            cpyEmitter.mMaxRandScale = mMaxRandScale;

            cpyEmitter.mChangeVelocity = mChangeVelocity;
            cpyEmitter.mMinVelocityX = (VariableFloat)mMinVelocityX.Clone();
            cpyEmitter.mMinVelocityY = (VariableFloat)mMinVelocityY.Clone();
            cpyEmitter.mMaxVelocityX = mMaxVelocityX;
            cpyEmitter.mMaxVelocityY = mMaxVelocityY;
            cpyEmitter.mAccelerationX = mAccelerationX;
            cpyEmitter.mAccelerationY = mAccelerationY;

            cpyEmitter.mMainVar = mMainVar;
            cpyEmitter.mMainVar0 = mMainVar0;
            cpyEmitter.mMainVar1 = mMainVar1;
            cpyEmitter.mMainVar2 = mMainVar2;
            cpyEmitter.mMainVar3 = mMainVar3;
            cpyEmitter.mHasUnknown01 = mHasUnknown01;
            cpyEmitter.mUnknownFlags = mUnknownFlags;
            cpyEmitter.mUnknown0 = (VariableFloat)mUnknown0.Clone();
            cpyEmitter.mUnknown1 = (VariableFloat)mUnknown1.Clone();
            cpyEmitter.mUnknownEmitRate = mUnknownEmitRate;
            cpyEmitter.mUnknown2 = mUnknown2;

            cpyEmitter.mChangeColour = mChangeColour;
            cpyEmitter.mColourRed = (VariableFloat)mColourRed.Clone();
            cpyEmitter.mColourGreen = (VariableFloat)mColourGreen.Clone();
            cpyEmitter.mColourBlue = (VariableFloat)mColourBlue.Clone();

            cpyEmitter.mChangeDirection = mChangeDirection;
            cpyEmitter.mDirectionSpeed = mDirectionSpeed;
            cpyEmitter.mDirectionRandomSpeed = mDirectionRandomSpeed;
            cpyEmitter.mDirectionAcceleration = mDirectionAcceleration;
            cpyEmitter.mDirectionAngle = mDirectionAngle;
            cpyEmitter.mDirectionRandomAngle = mDirectionRandomAngle;

            cpyEmitter.mChangeUnknown = mChangeUnknown;
            cpyEmitter.mUnknownA = mUnknownA;
            cpyEmitter.mUnknownB = mUnknownB;

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

        public float Rotation
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

        public int MainVar
        {
            get
            {
                return mMainVar;
            }
            set
            {
                mMainVar = value;
            }
        }

        public VariableFloat Unknown0
        {
            get
            {
                return mUnknown0;
            }
            set
            {
                mUnknown0 = value;
            }
        }

        public VariableFloat Unknown1
        {
            get
            {
                return mUnknown1;
            }
            set
            {
                mUnknown1 = value;
            }
        }

        public float Unknown2
        {
            get
            {
                return mUnknown2;
            }
            set
            {
                mUnknown2 = value;
            }
        }

        [DisplayName("Image (first copy)")]
        [Description("The game stores the image of the emitted objects twice and uses the second copy, Emission Image. Normally both are the same (PopCap: Image).")]
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

        [DisplayName("Emission Image")]
        [Description("The image of the emitted objects (PopCap: Image).")]
        [Category("Appearance")]
        [DefaultValue("")]
        public string EmitImageFilename
        {
            get
            {
                return mEmitImage;
            }
            set
            {
                mEmitImage = value;
            }
        }

        [DisplayName("Additive")]
        [Description("Draw the emitted objects with additive blending, so dark areas become transparent.")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool Transparancy
        {
            get
            {
                return mTransparancy;
            }
            set
            {
                mTransparancy = value;
            }
        }

        [DisplayName("Change Opacity")]
        [Description("If this is set to false, the opacity attributes will be ignored.")]
        [Category("Opacity")]
        [DefaultValue(false)]
        public bool ChangeOpacity
        {
            get
            {
                return mChangeOpacity;
            }
            set
            {
                mChangeOpacity = value;
            }
        }

        [Description("How opaque the emitted objects are. 0.0 is transparent, 1.0 is opaque (PopCap: Alpha).")]
        [Category("Opacity")]
        [DefaultValue(1.0f)]
        public VariableFloat Opacity
        {
            get
            {
                return mOpacity;
            }
            set
            {
                mOpacity = value;
            }
        }

        [DisplayName("Random Start")]
        [Description("Whether objects are emitted at random positions within the area.")]
        [Category("Behaviour")]
        [DefaultValue(false)]
        public bool RandomStartPosition
        {
            get
            {
                return mRandomStartPosition;
            }
            set
            {
                mRandomStartPosition = value;
            }
        }

        [DisplayName("Max")]
        [Description("The maximum number of emitted objects at one time.")]
        [Category("Behaviour")]
        [DefaultValue(100)]
        public int MaxQuantity
        {
            get
            {
                return mMaxQuantity;
            }
            set
            {
                mMaxQuantity = value;
            }
        }


        [DisplayName("Distrib")]
        [Description("How the emitted objects are distributed over the area given by the width and height, e.g. 0.5 for half of the area.")]
        [Category("Layout")]
        [DefaultValue(1.0f)]
        public VariableFloat EmitAreaMultiplier
        {
            get
            {
                return mEmitAreaMultiplier;
            }
            set
            {
                mEmitAreaMultiplier = value;
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
        [Description("How long each emitted object lasts before disappearing.")]
        [Category("Timing")]
        [DefaultValue(0.0f)]
        public float LifeDuration
        {
            get
            {
                return mLifeDuration;
            }
            set
            {
                mLifeDuration = value;
            }
        }

        [DisplayName("Rate")]
        [Description("The time interval between each object being emitted in hundredths of a second. 1.0 = 100 times per second.")]
        [Category("Timing")]
        [DefaultValue(0.0f)]
        public VariableFloat EmitRate
        {
            get
            {
                return mEmitRate;
            }
            set
            {
                mEmitRate = value;
            }
        }

        [DisplayName("Rand Rate")]
        [Description("Random variation of the emit rate.")]
        [Category("Timing")]
        [DefaultValue(0.0f)]
        public float UnknownEmitRate
        {
            get
            {
                return mUnknownEmitRate;
            }
            set
            {
                mUnknownEmitRate = value;
            }
        }

        [DisplayName("Alpha Into")]
        [Description("How long an emitted object takes to fade in.")]
        [Category("Timing")]
        [DefaultValue(0.1f)]
        public float FadeInTime
        {
            get
            {
                return mFadeInTime;
            }
            set
            {
                mFadeInTime = value;
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
                return (float)Math.Round(mLifeDuration - mTimeBeforeFadeOut, 4);
            }
            set
            {
                mTimeBeforeFadeOut = (float)Math.Round(mLifeDuration - value, 4);
            }
        }

        [DisplayName("Alpha Fade At")]
        [Description("How long before an emitted object starts to fade out, in seconds.")]
        [Category("Timing")]
        [DefaultValue(0.0f)]
        public float TimeBeforeFadeOut
        {
            get
            {
                return mTimeBeforeFadeOut;
            }
            set
            {
                mTimeBeforeFadeOut = value;
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
        public VariableFloat MinVelocityX
        {
            get
            {
                return mMinVelocityX;
            }
            set
            {
                mMinVelocityX = value;
            }
        }

        [DisplayName("Vel Y")]
        [Description("The vertical velocity of the emitted objects. Negative moves up and positive moves down.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public VariableFloat MinVelocityY
        {
            get
            {
                return mMinVelocityY;
            }
            set
            {
                mMinVelocityY = value;
            }
        }

        [DisplayName("Vel X Rand")]
        [Description("A random amount added to the horizontal velocity of each emitted object.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public float MaxVelocityX
        {
            get
            {
                return mMaxVelocityX;
            }
            set
            {
                mMaxVelocityX = value;
            }
        }

        [DisplayName("Vel Y Rand")]
        [Description("A random amount added to the vertical velocity of each emitted object.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public float MaxVelocityY
        {
            get
            {
                return mMaxVelocityY;
            }
            set
            {
                mMaxVelocityY = value;
            }
        }

        [DisplayName("Acc X")]
        [Description("The horizontal acceleration of the emitted objects. Negative accelerates left and positive accelerates right.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public float AccelerationX
        {
            get
            {
                return mAccelerationX;
            }
            set
            {
                mAccelerationX = value;
            }
        }

        [DisplayName("Gravity")]
        [Description("The vertical acceleration of the emitted objects. Negative accelerates up and positive accelerates down.")]
        [Category("Velocity")]
        [DefaultValue(0.0f)]
        public float AccelerationY
        {
            get
            {
                return mAccelerationY;
            }
            set
            {
                mAccelerationY = value;
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
        [Description("The initial scale of the emitted objects.")]
        [Category("Scale")]
        [DefaultValue(0.0f)]
        public VariableFloat MinScale
        {
            get
            {
                return mMinScale;
            }
            set
            {
                mMinScale = value;
            }
        }

        [DisplayName("Scale Vel")]
        [Description("How fast the scale of the emitted objects changes. Negative makes them smaller, positive makes them larger.")]
        [Category("Scale")]
        [DefaultValue(0.0f)]
        public VariableFloat ScaleVelocity
        {
            get
            {
                return mScaleVelocity;
            }
            set
            {
                mScaleVelocity = value;
            }
        }

        [DisplayName("Scale Rand")]
        [Description("A random amount added to the initial scale of each emitted object.")]
        [Category("Scale")]
        [DefaultValue(0.0f)]
        public float MaxRandScale
        {
            get
            {
                return mMaxRandScale;
            }
            set
            {
                mMaxRandScale = value;
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
        [Description("The initial rotation of the emitted objects in degrees.")]
        [Category("Rotation")]
        [DefaultValue(0.0f)]
        public VariableFloat InitialRotation
        {
            get
            {
                return mInitialRotation;
            }
            set
            {
                mInitialRotation = value;
            }
        }

        [DisplayName("Rotation Vel")]
        [Description("The angular velocity of the emitted objects. 0.01 = 1 degree per second.")]
        [Category("Rotation")]
        [DefaultValue(0.0f)]
        public VariableFloat RotationVelocity
        {
            get
            {
                return mRotationVelocity;
            }
            set
            {
                mRotationVelocity = value;
            }
        }

        [DisplayName("Rotation Rand")]
        [Description("A random amount added to the initial rotation of each emitted object.")]
        [Category("Rotation")]
        [DefaultValue(0.0f)]
        public float RotationUnknown
        {
            get
            {
                return mRotationUnknown;
            }
            set
            {
                mRotationUnknown = value;
            }
        }

        [DisplayName("Change Colour")]
        [Description("If this is set to false, the colour attributes will be ignored.")]
        [Category("Colour")]
        [DefaultValue(false)]
        public bool ChangeColour
        {
            get
            {
                return mChangeColour;
            }
            set
            {
                mChangeColour = value;
            }
        }

        [DisplayName("Color R")]
        [Description("The red component of the colour of the emitted objects.")]
        [Category("Colour")]
        [DefaultValue(1.0f)]
        public VariableFloat ColourRed
        {
            get
            {
                return mColourRed;
            }
            set
            {
                mColourRed = value;
            }
        }

        [DisplayName("Color G")]
        [Description("The green component of the colour of the emitted objects.")]
        [Category("Colour")]
        [DefaultValue(1.0f)]
        public VariableFloat ColourGreen
        {
            get
            {
                return mColourGreen;
            }
            set
            {
                mColourGreen = value;
            }
        }

        [DisplayName("Color B")]
        [Description("The blue component of the colour of the emitted objects.")]
        [Category("Colour")]
        [DefaultValue(1.0f)]
        public VariableFloat ColourBlue
        {
            get
            {
                return mColourBlue;
            }
            set
            {
                mColourBlue = value;
            }
        }

        [DisplayName("Change Direction")]
        [Description("If this is set to false, the direction attributes will be ignored.")]
        [Category("Direction")]
        [DefaultValue(false)]
        public bool ChangeDirection
        {
            get
            {
                return mChangeDirection;
            }
            set
            {
                mChangeDirection = value;
            }
        }

        [DisplayName("Polar Vel")]
        [Description("The speed of the emitted objects in the direction given by Vel Rot.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float DirectionVelocity
        {
            get
            {
                return mDirectionSpeed;
            }
            set
            {
                mDirectionSpeed = value;
            }
        }

        [DisplayName("Polar Vel Rand")]
        [Description("A random amount added to Polar Vel for each emitted object.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float DirectionRandomSpeed
        {
            get
            {
                return mDirectionRandomSpeed;
            }
            set
            {
                mDirectionRandomSpeed = value;
            }
        }

        [DisplayName("Polar Acc")]
        [Description("The acceleration of the emitted objects in the direction given by Vel Rot.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float DirectionAcceleration
        {
            get
            {
                return mDirectionAcceleration;
            }
            set
            {
                mDirectionAcceleration = value;
            }
        }

        [DisplayName("Vel Rot")]
        [Description("The direction of the polar velocity and acceleration in degrees.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float DirectionAngle
        {
            get
            {
                return mDirectionAngle;
            }
            set
            {
                mDirectionAngle = value;
            }
        }

        [DisplayName("Vel Rot Rand")]
        [Description("A random amount added to Vel Rot for each emitted object, in degrees.")]
        [Category("Direction")]
        [DefaultValue(0.0f)]
        public float DirectionRandomAngle
        {
            get
            {
                return mDirectionRandomAngle;
            }
            set
            {
                mDirectionRandomAngle = value;
            }
        }

        [DisplayName("Has Move Delta")]
        [Description("Whether the level stores how far the emitter moved in the last frame. This is game state, not a setting.")]
        [Category("Game State")]
        [DefaultValue(false)]
        public bool ChangeUnknown
        {
            get
            {
                return mChangeUnknown;
            }
            set
            {
                mChangeUnknown = value;
            }
        }

        [DisplayName("Move Delta X")]
        [Description("How far the emitter moved horizontally in the last frame. This is game state, not a setting.")]
        [Category("Game State")]
        [DefaultValue(0.0f)]
        public float UnknownA
        {
            get
            {
                return mUnknownA;
            }
            set
            {
                mUnknownA = value;
            }
        }

        [DisplayName("Move Delta Y")]
        [Description("How far the emitter moved vertically in the last frame. This is game state, not a setting.")]
        [Category("Game State")]
        [DefaultValue(0.0f)]
        public float UnknownB
        {
            get
            {
                return mUnknownB;
            }
            set
            {
                mUnknownB = value;
            }
        }

        [DisplayName("Fast Forward")]
        [Description("How far the emitter is run ahead when the level starts, in hundredths of a second.")]
        [Category("Timing")]
        public int MainVar0
        {
            get
            {
                return mMainVar0;
            }
            set
            {
                mMainVar0 = value;
            }
        }

        [DisplayName("Hardware Rate")]
        [Description("The emit rate multiplier used with hardware acceleration.")]
        [Category("Timing")]
        public float MainVar1
        {
            get
            {
                return mMainVar1;
            }
            set
            {
                mMainVar1 = value;
            }
        }

        [DisplayName("Trigger Object")]
        [Description("The id of an object that triggers the emitter (PopCap: TriggerObj).")]
        [Category("Behaviour")]
        public string MainVar2
        {
            get
            {
                return mMainVar2;
            }
            set
            {
                mMainVar2 = value;
            }
        }

        [DisplayName("Rate Is Looping")]
        [Description("Whether the emit rate curve loops (1) or plays once (0).")]
        [Category("Timing")]
        public byte MainVar3
        {
            get
            {
                return mMainVar3;
            }
            set
            {
                mMainVar3 = value;
            }
        }
    }
}

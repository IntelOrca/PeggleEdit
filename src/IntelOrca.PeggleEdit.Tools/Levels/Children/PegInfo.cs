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

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// Represents the information about a level entry which is a peg.
    /// </summary>
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    /// <summary>
    /// The kind of a peg, from peg_type in PopCap's level editor.
    /// </summary>
    public enum PegType : byte
    {
        None,
        Normal,
        Goal,
        Score,
        Powerup,
    }

    public sealed class PegInfo : ICloneable
    {
        private LevelEntry mParent;
        private byte mType = 1;
        private bool mVariable;
        private bool mCrumble;

        // Unknown values, preserved so that they are written back as read
        private int mUnknownFlags;
        private bool mHasHitTime;
        private int mHitTime;
        private bool mHasGlowTimer;
        private int mGlowTimer;
        private bool mHasGlowState;
        private byte mGlowState;
        private bool mHasCrumbleTimer;
        private byte mCrumbleTimer;

        public PegInfo(LevelEntry parent)
        {
            mParent = parent;
        }

        public PegInfo(LevelEntry parent, bool canBeOrange, bool quickDisappear)
            : this(parent)
        {
            mVariable = canBeOrange;
            mCrumble = quickDisappear;
        }

        public void ReadData(BinaryReader br, int version)
        {
            mType = br.ReadByte();
            FlagGroup f2 = new FlagGroup(br.ReadByte());
            mUnknownFlags = f2.Int32 & ((1 << 0) | (1 << 6));
            if (f2[1])
                mVariable = true;
            mHasHitTime = f2[2];
            if (f2[2])
                mHitTime = br.ReadInt32();
            if (f2[3])
                mCrumble = true;
            mHasGlowTimer = f2[4];
            if (f2[4])
                mGlowTimer = br.ReadInt32();
            mHasGlowState = f2[5];
            if (f2[5])
                mGlowState = br.ReadByte();
            mHasCrumbleTimer = f2[7];
            if (f2[7])
                mCrumbleTimer = br.ReadByte();
        }

        public void WriteData(BinaryWriter bw, int version)
        {
            bw.Write(mType);
            FlagGroup f2 = new FlagGroup(mUnknownFlags);
            if (mVariable)
                f2[1] = true;
            f2[2] = mHasHitTime;
            if (mCrumble)
                f2[3] = true;
            f2[4] = mHasGlowTimer;
            f2[5] = mHasGlowState;
            f2[7] = mHasCrumbleTimer;
            bw.Write(f2.Int8);
            if (f2[2])
                bw.Write(mHitTime);
            if (f2[4])
                bw.Write(mGlowTimer);
            if (f2[5])
                bw.Write(mGlowState);
            if (f2[7])
                bw.Write(mCrumbleTimer);
        }

        public Color GetOuterColour()
        {
            if (Variable && (!mParent.Level.ShowPreview))
            {
                if (Crumble)
                    return Color.FromArgb(234, 140, 22);
                else
                    return Color.FromArgb(234, 140, 22);
            }
            else
            {
                if (Crumble && (!mParent.Level.ShowPreview))
                    return Color.FromArgb(83, 124, 217);
                else
                    return Color.FromArgb(83, 124, 217);
            }
        }

        public Color GetInnerColour()
        {
            if (Variable && (!mParent.Level.ShowPreview))
            {
                if (Crumble)
                    return Color.FromArgb(255, 250, 202);
                else
                    return Color.FromArgb(131, 35, 6);
            }
            else
            {
                if (Crumble && (!mParent.Level.ShowPreview))
                    return Color.FromArgb(214, 254, 255);
                else
                    return Color.FromArgb(13, 50, 167);
            }
        }

        [EntryProperty(EntryPropertyType.Element, false)]
        [DisplayName("Variable")]
        [Description("When enabled, the game may pick this peg to be an orange, purple (bonus) or green (power-up) peg. When disabled, the peg always keeps its own type, e.g. always blue.")]
        [Category("Behaviour")]
        [DefaultValue(true)]
        public bool Variable
        {
            get
            {
                return mVariable;
            }
            set
            {
                mVariable = value;
            }
        }

        [EntryProperty(EntryPropertyType.Element, false)]
        [DisplayName("Crumble")]
        [Description("When enabled, the peg crumbles away shortly after the ball hits it, so that the ball can not get stuck on it. Normally used for pegs at the bottom of circles and similar places.")]
        [Category("Behaviour")]
        [DefaultValue(false)]
        public bool Crumble
        {
            get
            {
                return mCrumble;
            }
            set
            {
                mCrumble = value;
            }
        }

        [DisplayName("Type")]
        [Description("The kind of peg: Normal (blue), Goal (orange), Score (purple) or Powerup (green). When Variable is enabled, the game chooses the orange, purple and green pegs itself.")]
        [Category("Behaviour")]
        [DefaultValue(PegType.Normal)]
        public PegType Type
        {
            get
            {
                return (PegType)mType;
            }
            set
            {
                mType = (byte)value;
            }
        }

        [Browsable(false)]
        public LevelEntry Parent
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }

        public object Clone()
        {
            PegInfo pi = new PegInfo(mParent);
            pi.mVariable = mVariable;
            pi.mCrumble = mCrumble;
            pi.mType = mType;
            pi.mUnknownFlags = mUnknownFlags;
            pi.mHasHitTime = mHasHitTime;
            pi.mHitTime = mHitTime;
            pi.mHasGlowTimer = mHasGlowTimer;
            pi.mGlowTimer = mGlowTimer;
            pi.mHasGlowState = mHasGlowState;
            pi.mGlowState = mGlowState;
            pi.mHasCrumbleTimer = mHasCrumbleTimer;
            pi.mCrumbleTimer = mCrumbleTimer;
            return pi;
        }
    }
}

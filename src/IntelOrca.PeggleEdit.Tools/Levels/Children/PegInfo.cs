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
	public sealed class PegInfo : ICloneable
	{
		private LevelEntry mParent;
		private byte mType;
		private bool mVariable;
		private bool mCrumble;

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
			if (f2[1])
				mVariable = true;
			if (f2[2])
				br.ReadInt32();
			if (f2[3])
				mCrumble = true;
			if (f2[4])
				br.ReadInt32();
			if (f2[5])
				br.ReadByte();
			if (f2[7])
				br.ReadByte();
		}

		public void WriteData(BinaryWriter bw, int version)
		{
			bw.Write((byte)1);
			FlagGroup f2 = new FlagGroup();
			if (mVariable)
				f2[1] = true;
			if (mCrumble)
				f2[3] = true;
			bw.Write(f2.Int8);
		}

		public Color GetOuterColour()
		{
			if (CanBeOrange && (!mParent.Level.ShowPreview)) {
				if (QuickDisappear)
					return Color.FromArgb(234, 140, 22);
				else
					return Color.FromArgb(234, 140, 22);
			} else {
				if (QuickDisappear && (!mParent.Level.ShowPreview))
					return Color.FromArgb(83, 124, 217);
				else
					return Color.FromArgb(83, 124, 217);
			}
		}

		public Color GetInnerColour()
		{
			if (CanBeOrange && (!mParent.Level.ShowPreview)) {
				if (QuickDisappear)
					return Color.FromArgb(255, 250, 202);
				else
					return Color.FromArgb(131, 35, 6);
			} else {
				if (QuickDisappear && (!mParent.Level.ShowPreview))
					return Color.FromArgb(214, 254, 255);
				else
					return Color.FromArgb(13, 50, 167);
			}
		}

		[EntryProperty(EntryPropertyType.Element, false)]
		[DisplayName("Can be Orange")]
		[Description("States whether the peg(s) can be orange or not. You may want to set this for pegs that are harder to get.")]
		[Category("Behaviour")]
		[DefaultValue(true)]
		public bool CanBeOrange
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
		[DisplayName("Quick Disappear")]
		[Description("States whether the peg will disappear shortly after the ball has touched it. This is normally set for pegs at the bottom of circles etc. to make sure the ball does not get stuck for too long.")]
		[Category("Behaviour")]
		[DefaultValue(false)]
		public bool QuickDisappear
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
			return pi;
		}
	}
}

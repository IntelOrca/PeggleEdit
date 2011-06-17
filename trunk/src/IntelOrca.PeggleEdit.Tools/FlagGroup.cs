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
using System.Linq;
using System.Text;

namespace IntelOrca.PeggleEdit.Tools
{
	/// <summary>
	/// Represents a group of flags stored as an integer.
	/// </summary>
	public struct FlagGroup
	{
		public const int MAX_FLAGS = 32;
		int mFlags;

		public FlagGroup(int flags)
		{
			mFlags = 0;
			Int32 = flags;
		}

		public FlagGroup(short flags)
		{
			mFlags = 0;
			Int16 = flags;
		}

		public FlagGroup(byte flags)
		{
			mFlags = 0;
			Int8 = flags;
		}

		/// <summary>
		/// Gets a flag from the integer.
		/// </summary>
		/// <param name="i">The flag index.</param>
		/// <returns>a boolean of the flag.</returns>
		public bool GetFlag(int i)
		{
			i = GetIPow(i);
			return ((mFlags & i) > 0);
		}

		/// <summary>
		/// Sets a flag from the integer.
		/// </summary>
		/// <param name="i">The flag index.</param>
		/// <param name="value">The value to set the flag to.</param>
		public void SetFlag(int i, bool value)
		{
			i = GetIPow(i);
			if (value) {
				mFlags |= i;
			} else {
				mFlags &= ~i;
			}
		}

		/// <summary>
		/// Gets a string showing all the flags by showing the integer in binary.
		/// </summary>
		/// <returns>the integer in a binary format.</returns>
		public override string ToString()
		{
			string s = "";
			for (int i = MAX_FLAGS - 1; i >= 0; i--) {
				if (GetFlag(i))
					s += "1";
				else
					s += "0";
				if (i % 4 == 0)
					s += " ";
			}
			return s;
		}

		/// <summary>
		/// Gets or sets a flag from the integer.
		/// </summary>
		/// <param name="i">The flag index.</param>
		/// <returns>a boolean of the flag.</returns>
		public bool this[int i]
		{
			get
			{
				return GetFlag(i);
			}
			set
			{
				SetFlag(i, value);
			}
		}

		/// <summary>
		/// Gets or sets the first byte of the integer.
		/// </summary>
		public byte Int8
		{
			get
			{
				byte[] bytes = BitConverter.GetBytes(mFlags);
				return bytes[0];
			}
			set
			{
				byte[] bytes = new byte[4];
				bytes[0] = value;
				mFlags = BitConverter.ToInt32(bytes, 0);
			}
		}

		/// <summary>
		/// Gets or sets the first two bytes of the integer.
		/// </summary>
		public short Int16
		{
			get
			{
				byte[] bytes = BitConverter.GetBytes(mFlags);
				return BitConverter.ToInt16(bytes, 0);
			}
			set
			{
				byte[] twobytes = BitConverter.GetBytes(value);
				byte[] bytes = new byte[4];
				bytes[0] = twobytes[0];
				bytes[1] = twobytes[1];
				mFlags = BitConverter.ToInt32(bytes, 0);
			}
		}

		/// <summary>
		/// Gets or sets the integer.
		/// </summary>
		public int Int32
		{
			get
			{
				return mFlags;
			}
			set
			{
				mFlags = value;
			}
		}

		/// <summary>
		/// Gets the bit index of the flag
		/// </summary>
		/// <param name="i">The flag index.</param>
		/// <returns>the bit index of the flag.</returns>
		int GetIPow(int i)
		{
			return (int)Math.Pow(2, i);
		}
	}
}

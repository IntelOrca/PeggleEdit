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

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
	/// <summary>
	/// Represents a level entry which can't be read.
	/// </summary>
	public class UnknownEntry : LevelEntry
	{
		int mType;

		public UnknownEntry(Level level, int type)
			: base(level)
		{
			mType = type;
		}

		public override object Clone()
		{
			return new UnknownEntry(Level, mType);
		}

		public override int Type
		{
			get
			{
				return mType;
			}
		}
	}
}

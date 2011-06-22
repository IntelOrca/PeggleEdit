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

using System.IO;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
	/// <summary>
	/// Defines static members for creating new instances of Level Entries of a given type of from a reader.
	/// </summary>
	public static class LevelEntryFactory
	{
		/// <summary>
		/// Creates a new instance of a level entry of the given type.
		/// </summary>
		/// <param name="type">The type of level entry to create.</param>
		/// <param name="level">The level (parent) of the entry.</param>
		/// <returns>A new instance of a level entry of the given type.</returns>
		public static LevelEntry CreateLevelEntry(int type, Level level)
		{
			switch (type) {
				case 2:			//Rod
					return new Rod(level);
				case 3:			//Polygon
					return new Polygon(level);
				case 5:			//Circle
					return new Circle(level);
				case 6:			//Brick
					return new Brick(level);
				case 8:			//Teleport
					return new Teleport(level);
				case 9:			//Effect
					return new Emitter(level);

				//PeggleEdit's own entries
				case 1001:
					return new PegGenerator(level);
				case 1002:
					return new BrickGenerator(level);

				default:
					return new UnknownEntry(level, type);
			}
		}

		/// <summary>
		/// Creates a new instance of a level entry and assigns it with data read from a binary reader.
		/// </summary>
		/// <param name="br">The binary reader to use when reading the level entry data.</param>
		/// <param name="version">The version of the file the data is in.</param>
		/// <returns>A new instance of a level entry with data read by the binary reader.</returns>
		public static LevelEntry CreateLevelEntry(BinaryReader br, int version)
		{
			int sb1 = br.ReadInt32();
			if (sb1 == 0)
				return null;
			if (sb1 != 1) {
				//Do something weird
				return null;
			}

			int type = br.ReadInt32();

			LevelEntry entry = CreateLevelEntry(type, null);
			entry.ReadGenericData(br, version);
			entry.ReadData(br, version);

			return entry;
		}
	}
}

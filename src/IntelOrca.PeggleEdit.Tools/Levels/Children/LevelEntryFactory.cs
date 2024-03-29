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
            switch (type)
            {
                case LevelEntryTypes.Rod:
                    return new Rod(level);
                case LevelEntryTypes.Polygon:
                    return new Polygon(level);
                case LevelEntryTypes.Circle:
                    return new Circle(level);
                case LevelEntryTypes.Brick:
                    return new Brick(level);
                case LevelEntryTypes.Teleport:
                    return new Teleport(level);
                case LevelEntryTypes.Emitter:
                    return new Emitter(level);

                //PeggleEdit's own entries
                case LevelEntryTypes.PegGenerator:
                    return new PegGenerator(level);
                case LevelEntryTypes.BrickGenerator:
                    return new BrickGenerator(level);
                case LevelEntryTypes.PegCurveGenerator:
                    return new PegCurveGenerator(level);
                case LevelEntryTypes.BrickCurveGenerator:
                    return new BrickCurveGenerator(level);

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
            int magic = br.ReadInt32();
            if (magic == 0)
                return null;
            else if (magic != 1)
                return null;

            var type = br.ReadInt32();
            var entry = CreateLevelEntry(type, null);
            entry.ReadGenericData(br, version);
            entry.ReadData(br, version);
            return entry;
        }
    }
}

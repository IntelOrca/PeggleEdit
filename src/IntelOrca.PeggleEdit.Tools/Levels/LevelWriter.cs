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
using System.IO;
using System.Text;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// Represents a writer that can write levels.
    /// </summary>
    public class LevelWriter : IDisposable
    {
        public const int PegoFileVersion = 0x35;
        public const int DeluxeFileVersion = 0x42;
        public const int DefaultFileVersion = 0x52;

        Stream mBaseStream;
        Level mLevel;
        int mFileVersion;

        public LevelWriter(string path)
            : this(new FileStream(path, FileMode.Create))
        {
        }

        public LevelWriter(Stream stream)
        {
            mBaseStream = stream;
        }

        public bool Write(Level level, int version)
        {
            level.UpdateMoverLinksWrite();

            mLevel = level;
            mFileVersion = version;

            BinaryWriter bw = new BinaryWriter(mBaseStream);

            //Level version
            bw.Write(version);

            //Unknown, normally 1
            bw.Write(mLevel.HeaderByte);

            //Num. entries
            var nullEntries = mLevel.PreservedNullEntries;
            bw.Write(mLevel.Entries.Count + nullEntries.Count);

            //Entry loop, with any null entries put back where they were read
            int nullIndex = 0;
            for (int i = 0; i < mLevel.Entries.Count; i++)
            {
                for (; nullIndex < nullEntries.Count && nullEntries[nullIndex].Index <= i; nullIndex++)
                    bw.Write(nullEntries[nullIndex].Magic);
                mLevel.Entries[i].Write(bw, version);
            }
            for (; nullIndex < nullEntries.Count; nullIndex++)
                bw.Write(nullEntries[nullIndex].Magic);

            bw.Close();

            return true;
        }

        public static void WritePopcapString(BinaryWriter bw, string sz)
        {
            if (String.IsNullOrEmpty(sz))
            {
                bw.Write((short)0);
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(sz);
                bw.Write((short)bytes.Length);
                bw.Write(bytes);
            }
        }

        public void Dispose()
        {
            mBaseStream.Dispose();
        }
    }
}

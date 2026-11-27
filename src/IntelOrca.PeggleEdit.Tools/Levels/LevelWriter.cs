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
using System.IO;
using System.Text;
using IntelOrca.PeggleEdit.Tools.Extensions;
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
            mLevel = level;
            mFileVersion = version;

            // The game sees the entries the generators generate, in their place
            var entries = level.ExpandGenerators(out var generators);
            var restore = new List<Action>();
            try
            {
                var links = level.CopyLinksToGenerators(entries, generators, restore);
                level.UpdateMoverLinksWrite(entries, generators);
                WriteLevel(level, entries, generators, links, version);
            }
            finally
            {
                // Put the links back as the editor has them, numbered as the editor numbers its entries
                foreach (var action in restore)
                    action();
                if (generators.Count != 0)
                    level.UpdateMoverLinksWrite();
            }
            return true;
        }

        private void WriteLevel(Level level, List<LevelEntry> entries, List<EditorData.GeneratorRecord> generators,
            List<EditorData.LinkRecord> links, int version)
        {
            var ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            //Level version
            bw.Write(version);

            //Unknown, normally 1
            bw.Write(mLevel.HeaderByte);

            //Num. entries
            var nullEntries = mLevel.NullEntries;
            bw.Write(entries.Count + nullEntries.Count);

            //Entry loop, with any null entries put back where they were read
            int nullIndex = 0;
            var offsets = new long[entries.Count + 1];
            for (int i = 0; i < entries.Count; i++)
            {
                for (; nullIndex < nullEntries.Count && nullEntries[nullIndex].Index <= i; nullIndex++)
                    bw.Write(nullEntries[nullIndex].Magic);
                bw.Flush();
                offsets[i] = ms.Position;
                entries[i].Write(bw, version);
            }
            bw.Flush();
            offsets[entries.Count] = ms.Position;
            for (; nullIndex < nullEntries.Count; nullIndex++)
                bw.Write(nullEntries[nullIndex].Magic);

            // PeggleEdit's own data goes after the last entry, where the game does not read
            var objects = EditorData.GetObjects(level.Entries);
            if (generators.Count != 0 || level.CustomThumbnail || objects.Count != 0)
            {
                bw.Flush();
                var written = ms.ToArray();
                for (int i = 0; i < generators.Count; i++)
                {
                    var record = generators[i];
                    long start = offsets[record.First];
                    long end = offsets[record.First + record.Count];
                    record.Hash = new ReadOnlySpan<byte>(written, (int)start, (int)(end - start)).CalculateFnv1a();
                    generators[i] = record;
                }
                for (int i = 0; i < links.Count; i++)
                {
                    if (links[i].HolderGenerator < 0)
                    {
                        var link = links[i];
                        link.HolderMoverId = Level.GetGameMoverId(link.Copy, entries);
                        links[i] = link;
                    }
                }
                EditorData.Write(bw, ms.ToArray(), generators, links, objects, level.CustomThumbnail, version);
            }

            bw.Flush();
            ms.WriteTo(mBaseStream);
            mBaseStream.Close();
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

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
    /// Represents a reader that can read levels.
    /// </summary>
    public class LevelReader : IDisposable
    {
        Stream mBaseStream;
        int mFileVersion;
        int mNumEntries;
        Level mLevel;
        string mError;

        public LevelReader(string path)
            : this(new FileStream(path, FileMode.Open))
        {
        }

        public LevelReader(byte[] buffer)
            : this(new MemoryStream(buffer))
        {
        }

        public LevelReader(Stream s)
        {
            mBaseStream = s;
        }

        public Level Read()
        {
            int entry_index = -1;

            try
            {
                byte[] data;
                using (var ms = new MemoryStream())
                {
                    mBaseStream.CopyTo(ms);
                    data = ms.ToArray();
                }

                int levelEnd = ReadEntries(data, ref entry_index);
                entry_index = -1;
                var status = ReadEditorData(data, levelEnd);
                if (status == EditorData.Status.Stale || status == EditorData.Status.Unreadable)
                {
                    // Something went wrong part way through: read the level again, with its generated entries
                    ReadEntries(data, ref entry_index);
                }
                mLevel.EditorDataStatus = status;
                mLevel.SetOriginalData(data);
                return mLevel;
            }
            catch (Exception ex)
            {
                if (entry_index != -1)
                {
                    mError = String.Format("Entry {0}: {1}", entry_index, ex.Message);
                }
                else
                {
                    mError = ex.Message;
                }

                return null;
            }
        }

        /// <summary>
        /// Reads the header and the entries into a new level, with their links set.
        /// </summary>
        /// <returns>Where the entries end.</returns>
        private int ReadEntries(byte[] data, ref int entry_index)
        {
            mLevel = new Level();
            BinaryReader br = new BinaryReader(new MemoryStream(data));

            //Read file version
            mFileVersion = br.ReadInt32();

            mLevel.FileVersion = mFileVersion;
            mLevel.HeaderByte = br.ReadByte();

            mNumEntries = br.ReadInt32();

            //Read entries
            mEntryBytes = new List<(int Start, int End)>();
            for (entry_index = 0; entry_index < mNumEntries; entry_index++)
            {
                int start = (int)br.BaseStream.Position;
                int magic = br.ReadInt32();
                var entry = LevelEntryFactory.CreateLevelEntry(magic, br, mFileVersion);
                if (entry != null)
                {
                    entry.Level = mLevel;
                    mLevel.Entries.Add(entry);
                    mEntryBytes.Add((start, (int)br.BaseStream.Position));
                }
                else
                {
                    mLevel.NullEntries.Add((mLevel.Entries.Count, magic));
                }
            }

            mLevel.UpdateMoverLinksRead();
            return (int)br.BaseStream.Position;
        }

        // Where each entry (not counting null entries) is in the file
        List<(int Start, int End)> mEntryBytes;

        /// <summary>
        /// Reads PeggleEdit's data after the last entry, putting the generators back in place of the entries they
        /// generated. The data is only used if the generated entries are as PeggleEdit wrote them, so that nothing is
        /// lost when they were changed by something else.
        /// </summary>
        private EditorData.Status ReadEditorData(byte[] data, int levelEnd)
        {
            var status = EditorData.Find(data, levelEnd, out var chunks);
            if (status != EditorData.Status.Valid)
                return status;

            try
            {
                var generatorsChunk = chunks.FindIndex(x => x.Tag == EditorData.GeneratorsTag);
                if (generatorsChunk < 0)
                    return status;
                var generators = EditorData.ReadGenerators(data, chunks[generatorsChunk], mFileVersion);
                if (!EditorData.AreRangesValid(generators, mLevel.Entries.Count))
                    return EditorData.Status.Unreadable;
                foreach (var record in generators)
                {
                    int start = record.Count == 0 ? 0 : mEntryBytes[record.First].Start;
                    int end = record.Count == 0 ? 0 : mEntryBytes[record.First + record.Count - 1].End;
                    if (new ReadOnlySpan<byte>(data, start, end - start).CalculateFnv1a() != record.Hash)
                        return EditorData.Status.Stale;
                }
                var linksChunk = chunks.FindIndex(x => x.Tag == EditorData.LinksTag);
                var links = linksChunk < 0 ? new List<EditorData.LinkRecord>() : EditorData.ReadLinks(data, chunks[linksChunk]);

                mLevel.RestoreGenerators(generators, links);
                return status;
            }
            catch (Exception)
            {
                return EditorData.Status.Unreadable;
            }
        }

        private void SeekToNextEntry(BinaryReader br)
        {
            for (; ; )
            {
                if (br.BaseStream.Position > br.BaseStream.Length - 8)
                    return;

                uint int1 = br.ReadUInt32();
                uint int2 = br.ReadUInt32();

                if (int1 == 1 && int2 < 10 && int2 > 0)
                {
                    br.BaseStream.Position -= 8;
                    return;
                }

                br.BaseStream.Position -= 7;
            }
        }

        public static string ReadPopcapString(BinaryReader br)
        {
            short l = br.ReadInt16();
            return Encoding.UTF8.GetString(br.ReadBytes(l));
        }

        public void Dispose()
        {
            mBaseStream.Dispose();
        }

        public string Error
        {
            get
            {
                return mError;
            }
        }
    }
}
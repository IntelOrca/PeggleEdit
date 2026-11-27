using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IntelOrca.PeggleEdit.Tools.Extensions;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// PeggleEdit's own data about a level, such as its generators, kept after the last entry of the level file. The
    /// game reads only the entries the header says there are and ignores the rest of the file, so the level stays
    /// playable.
    /// </summary>
    /// <remarks>
    /// Layout, little endian, straight after the last entry:
    /// <list type="bullet">
    /// <item>8 bytes: "PEGLEDIT"</item>
    /// <item>i32: format version, 1</item>
    /// <item>u64: FNV-1a hash of the level before this data, so that data left behind by a tool that changed the
    /// level without knowing about it is not used</item>
    /// <item>i32: size of the chunks that follow</item>
    /// <item>chunks: 4 byte ASCII tag, i32 size, then the data. Unknown chunks are skipped.</item>
    /// </list>
    /// The GENS chunk holds the generators: i32 count, then for each generator the index of the first entry it
    /// generated, the number of entries it generated (i32 each), an FNV-1a hash (u64) of those entries as written, and
    /// the generator written as a level entry. The generated entries are in the level in its place, so that the game
    /// sees them; PeggleEdit replaces them by the generator when it reads the level, if they still match the hash.
    /// The LINK chunk holds links to generators' movers (see <see cref="LinkRecord"/>): i32 count, then five i32s for
    /// each.
    /// The THMB chunk, which is empty, says that the level's thumbnail is one chosen by the user, so PeggleEdit keeps
    /// it rather than making a new one when the level changes. It is kept even when the level has been changed by
    /// another program since.
    /// The OBJS chunk holds the groups and locks of objects (see <see cref="ObjectRecord"/>): i32 count, then three
    /// i32s for each object in a group or locked.
    /// The GENS chunk is always written first, even with no generators.
    /// </remarks>
    public static class EditorData
    {
        public const string MagicText = "PEGLEDIT";
        public const int FormatVersion = 1;
        public const string GeneratorsTag = "GENS";
        public const string LinksTag = "LINK";
        public const string CustomThumbnailTag = "THMB";
        public const string ObjectsTag = "OBJS";

        static readonly byte[] Magic = Encoding.ASCII.GetBytes(MagicText);
        const int HeaderSize = 8 + 4 + 8 + 4;
        const int ChunkHeaderSize = 8;

        /// <summary>
        /// What was found after the last entry of a level.
        /// </summary>
        public enum Status
        {
            /// <summary>Nothing, or something other than PeggleEdit's data.</summary>
            None,
            /// <summary>PeggleEdit's data for this level.</summary>
            Valid,
            /// <summary>PeggleEdit's data, but the level has been changed since by something else.</summary>
            Stale,
            /// <summary>PeggleEdit's data in a newer format, or damaged.</summary>
            Unreadable,
        }

        /// <summary>
        /// A message for the user about why a level's generators could not be read, or null if there is nothing to say.
        /// </summary>
        public static string GetWarning(Status status)
        {
            const string kept = "Any pegs and bricks its generators generated are kept as ordinary objects.";
            switch (status)
            {
                case Status.Stale:
                    return "The level was changed by another program after PeggleEdit saved it, so its generators, groups and locks could not be restored. " + kept;
                case Status.Unreadable:
                    return "The level's PeggleEdit data is from a newer version of PeggleEdit, or is damaged, so its generators, groups and locks could not be restored. " + kept;
                default:
                    return null;
            }
        }

        public struct Chunk
        {
            public string Tag;
            public int Offset;
            public int Length;
        }

        public struct GeneratorRecord
        {
            public int First;
            public int Count;
            /// <summary>FNV-1a hash of the generated entries as written, so that changes to them are noticed.</summary>
            public ulong Hash;
            public LevelEntry Generator;
        }

        /// <summary>
        /// A link to a generator's mover. The game has no generator to link to, so the mover holding the link is
        /// written with its own copy of the generator's mover instead, which moves the same way; this record turns
        /// the copy back into the link.
        /// </summary>
        public struct LinkRecord
        {
            /// <summary>The id (as the game numbers objects) of the copy, when it is held by an entry in the level.</summary>
            public int HolderMoverId;
            /// <summary>The index of the generator holding the copy, or -1 when it is held by an entry.</summary>
            public int HolderGenerator;
            /// <summary>How many owned movers down the holding generator's links the copy is.</summary>
            public int HolderDepth;
            /// <summary>The index of the generator linked to.</summary>
            public int Generator;
            /// <summary>How many owned movers down the generator's links the mover linked to is: 0 for its mover.</summary>
            public int Depth;

            /// <summary>The copy, while writing.</summary>
            internal Mover Copy;
        }

        /// <summary>
        /// An object that is in a group, or locked.
        /// </summary>
        public struct ObjectRecord
        {
            /// <summary>
            /// The index of the object among the level's objects as PeggleEdit has them, with each generator in place of
            /// the entries it generated (not counting null entries).
            /// </summary>
            public int Index;
            /// <summary>The group the object is in, or 0 for none.</summary>
            public int Group;
            /// <summary>Bit 0: the object is locked.</summary>
            public int Flags;

            public const int LockedFlag = 1;
        }

        /// <summary>
        /// Looks for PeggleEdit's data after the level, which ends at <paramref name="levelEnd"/>, and lists its chunks
        /// when it is valid, or stale (the level has changed since, but the data can still be read).
        /// </summary>
        public static Status Find(byte[] data, int levelEnd, out List<Chunk> chunks)
        {
            chunks = null;
            if (data.Length - levelEnd < Magic.Length)
                return Status.None;
            for (int i = 0; i < Magic.Length; i++)
            {
                if (data[levelEnd + i] != Magic[i])
                    return Status.None;
            }
            if (data.Length - levelEnd < HeaderSize)
                return Status.Unreadable;

            int version = BitConverter.ToInt32(data, levelEnd + 8);
            ulong hash = BitConverter.ToUInt64(data, levelEnd + 12);
            int size = BitConverter.ToInt32(data, levelEnd + 20);
            int start = levelEnd + HeaderSize;
            if (version != FormatVersion || size < 0 || size > data.Length - start)
                return Status.Unreadable;
            var status = new ReadOnlySpan<byte>(data, 0, levelEnd).CalculateFnv1a() == hash ? Status.Valid : Status.Stale;

            var result = new List<Chunk>();
            int position = start;
            int end = start + size;
            while (position < end)
            {
                if (end - position < ChunkHeaderSize)
                    return Status.Unreadable;
                var tag = Encoding.ASCII.GetString(data, position, 4);
                int length = BitConverter.ToInt32(data, position + 4);
                position += ChunkHeaderSize;
                if (length < 0 || length > end - position)
                    return Status.Unreadable;
                result.Add(new Chunk { Tag = tag, Offset = position, Length = length });
                position += length;
            }
            chunks = result;
            return status;
        }

        /// <summary>
        /// Writes PeggleEdit's data for a level whose file so far is <paramref name="levelPart"/>.
        /// </summary>
        public static void Write(BinaryWriter bw, byte[] levelPart, IReadOnlyList<GeneratorRecord> generators, IReadOnlyList<LinkRecord> links,
            IReadOnlyList<ObjectRecord> objects, bool customThumbnail, int version)
        {
            var chunks = new List<KeyValuePair<string, byte[]>>();
            var payload = new MemoryStream();
            using (var pw = new BinaryWriter(payload, Encoding.UTF8, true))
            {
                pw.Write(generators.Count);
                foreach (var record in generators)
                {
                    pw.Write(record.First);
                    pw.Write(record.Count);
                    pw.Write(record.Hash);
                    record.Generator.Write(pw, version);
                }
            }
            chunks.Add(new KeyValuePair<string, byte[]>(GeneratorsTag, payload.ToArray()));
            if (links.Count != 0)
                chunks.Add(new KeyValuePair<string, byte[]>(LinksTag, WriteLinks(links)));
            if (customThumbnail)
                chunks.Add(new KeyValuePair<string, byte[]>(CustomThumbnailTag, new byte[0]));
            if (objects.Count != 0)
                chunks.Add(new KeyValuePair<string, byte[]>(ObjectsTag, WriteObjects(objects)));
            bw.Write(Build(levelPart, chunks));
        }

        public static byte[] WriteObjects(IReadOnlyList<ObjectRecord> objects)
        {
            var payload = new MemoryStream();
            using (var pw = new BinaryWriter(payload, Encoding.UTF8, true))
            {
                pw.Write(objects.Count);
                foreach (var record in objects)
                {
                    pw.Write(record.Index);
                    pw.Write(record.Group);
                    pw.Write(record.Flags);
                }
            }
            return payload.ToArray();
        }

        /// <summary>
        /// Reads the groups and locks of objects from an OBJS chunk.
        /// </summary>
        /// <exception cref="InvalidDataException">The chunk is damaged.</exception>
        public static List<ObjectRecord> ReadObjects(byte[] data, Chunk chunk)
        {
            var br = new BinaryReader(new MemoryStream(data, chunk.Offset, chunk.Length));
            int count = br.ReadInt32();
            if (count < 0 || count != (chunk.Length - 4) / 12 || (chunk.Length - 4) % 12 != 0)
                throw new InvalidDataException("bad object count");
            var result = new List<ObjectRecord>(count);
            for (int i = 0; i < count; i++)
                result.Add(new ObjectRecord { Index = br.ReadInt32(), Group = br.ReadInt32(), Flags = br.ReadInt32() });
            return result;
        }

        /// <summary>
        /// Gets the groups and locks of a level's objects, as PeggleEdit has them.
        /// </summary>
        public static List<ObjectRecord> GetObjects(System.Collections.IEnumerable levelEntries)
        {
            var entries = levelEntries.Cast<LevelEntry>().ToList();
            var result = new List<ObjectRecord>();
            for (int i = 0; i < entries.Count; i++)
            {
                var le = entries[i];
                if (le.Group != 0 || le.Locked)
                    result.Add(new ObjectRecord { Index = i, Group = le.Group, Flags = le.Locked ? ObjectRecord.LockedFlag : 0 });
            }
            return result;
        }

        /// <summary>
        /// Puts the groups and locks of objects back on a level's objects, as PeggleEdit has them. Records for objects
        /// that are not there are left out.
        /// </summary>
        public static void SetObjects(System.Collections.IEnumerable levelEntries, IEnumerable<ObjectRecord> objects)
        {
            var entries = levelEntries.Cast<LevelEntry>().ToList();
            foreach (var record in objects)
            {
                if (record.Index < 0 || record.Index >= entries.Count)
                    continue;
                entries[record.Index].Group = Math.Max(0, record.Group);
                entries[record.Index].Locked = (record.Flags & ObjectRecord.LockedFlag) != 0;
            }
        }

        public static byte[] WriteLinks(IReadOnlyList<LinkRecord> links)
        {
            var payload = new MemoryStream();
            using (var pw = new BinaryWriter(payload, Encoding.UTF8, true))
            {
                pw.Write(links.Count);
                foreach (var link in links)
                {
                    pw.Write(link.HolderMoverId);
                    pw.Write(link.HolderGenerator);
                    pw.Write(link.HolderDepth);
                    pw.Write(link.Generator);
                    pw.Write(link.Depth);
                }
            }
            return payload.ToArray();
        }

        /// <summary>
        /// Reads the links from a LINK chunk.
        /// </summary>
        /// <exception cref="InvalidDataException">The chunk is damaged.</exception>
        public static List<LinkRecord> ReadLinks(byte[] data, Chunk chunk)
        {
            var br = new BinaryReader(new MemoryStream(data, chunk.Offset, chunk.Length));
            int count = br.ReadInt32();
            if (count < 0 || count != (chunk.Length - 4) / 20 || (chunk.Length - 4) % 20 != 0)
                throw new InvalidDataException("bad link count");
            var result = new List<LinkRecord>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(new LinkRecord
                {
                    HolderMoverId = br.ReadInt32(),
                    HolderGenerator = br.ReadInt32(),
                    HolderDepth = br.ReadInt32(),
                    Generator = br.ReadInt32(),
                    Depth = br.ReadInt32(),
                });
            }
            return result;
        }

        /// <summary>
        /// Builds PeggleEdit's data, with the given chunks, for a level whose file so far is <paramref name="levelPart"/>.
        /// </summary>
        public static byte[] Build(byte[] levelPart, IEnumerable<KeyValuePair<string, byte[]>> chunks)
        {
            var body = new MemoryStream();
            using (var cw = new BinaryWriter(body, Encoding.UTF8, true))
            {
                foreach (var chunk in chunks)
                {
                    cw.Write(Encoding.ASCII.GetBytes(chunk.Key));
                    cw.Write(chunk.Value.Length);
                    cw.Write(chunk.Value);
                }
            }

            var result = new MemoryStream();
            using (var bw = new BinaryWriter(result, Encoding.UTF8, true))
            {
                bw.Write(Magic);
                bw.Write(FormatVersion);
                bw.Write(levelPart.CalculateFnv1a());
                bw.Write((int)body.Length);
                bw.Write(body.ToArray());
            }
            return result.ToArray();
        }

        /// <summary>
        /// Reads the generators from a GENS chunk.
        /// </summary>
        /// <exception cref="InvalidDataException">The chunk is damaged.</exception>
        internal static List<GeneratorRecord> ReadGenerators(byte[] data, Chunk chunk, int version)
        {
            var br = new BinaryReader(new MemoryStream(data, chunk.Offset, chunk.Length));
            int count = br.ReadInt32();
            if (count < 0 || count > chunk.Length / 24)
                throw new InvalidDataException("bad generator count");
            var result = new List<GeneratorRecord>(count);
            for (int i = 0; i < count; i++)
            {
                var record = new GeneratorRecord { First = br.ReadInt32(), Count = br.ReadInt32(), Hash = br.ReadUInt64() };
                record.Generator = LevelEntryFactory.CreateLevelEntry(br, version);
                if (!(record.Generator is IEntryGenerator))
                    throw new InvalidDataException("not a generator");
                result.Add(record);
            }
            return result;
        }

        /// <summary>
        /// Checks that the generators replace entries <c>0 ≤ First</c>, in order, without overlapping, within
        /// <paramref name="entryCount"/> entries.
        /// </summary>
        internal static bool AreRangesValid(IReadOnlyList<GeneratorRecord> generators, int entryCount)
        {
            int next = 0;
            foreach (var record in generators)
            {
                if (record.First < next || record.Count < 0 || record.Count > entryCount - record.First)
                    return false;
                next = record.First + record.Count;
            }
            return true;
        }
    }
}

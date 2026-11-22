using System;
using System.Collections.Generic;
using System.IO;
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
    /// generated, the number of entries it generated (i32 each), and the generator written as a level entry. The
    /// generated entries are in the level in its place, so that the game sees them; PeggleEdit replaces them by the
    /// generator when it reads the level.
    /// </remarks>
    public static class EditorData
    {
        public const string MagicText = "PEGLEDIT";
        public const int FormatVersion = 1;
        public const string GeneratorsTag = "GENS";

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
            const string kept = "The pegs and bricks they generated are kept as ordinary objects.";
            switch (status)
            {
                case Status.Stale:
                    return "The level was changed by another program after PeggleEdit saved it, so its generators could not be restored. " + kept;
                case Status.Unreadable:
                    return "The level's PeggleEdit data is from a newer version of PeggleEdit, or is damaged, so its generators could not be restored. " + kept;
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
            public LevelEntry Generator;
        }

        /// <summary>
        /// Looks for PeggleEdit's data after the level, which ends at <paramref name="levelEnd"/>, and lists its chunks
        /// when it is valid.
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
            if (new ReadOnlySpan<byte>(data, 0, levelEnd).CalculateFnv1a() != hash)
                return Status.Stale;

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
            return Status.Valid;
        }

        /// <summary>
        /// Writes PeggleEdit's data for a level whose file so far is <paramref name="levelPart"/>.
        /// </summary>
        public static void Write(BinaryWriter bw, byte[] levelPart, IReadOnlyList<GeneratorRecord> generators, int version)
        {
            var payload = new MemoryStream();
            using (var pw = new BinaryWriter(payload, Encoding.UTF8, true))
            {
                pw.Write(generators.Count);
                foreach (var record in generators)
                {
                    pw.Write(record.First);
                    pw.Write(record.Count);
                    record.Generator.Write(pw, version);
                }
            }
            bw.Write(Build(levelPart, new[] { new KeyValuePair<string, byte[]>(GeneratorsTag, payload.ToArray()) }));
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
            if (count < 0 || count > chunk.Length / 16)
                throw new InvalidDataException("bad generator count");
            var result = new List<GeneratorRecord>(count);
            for (int i = 0; i < count; i++)
            {
                var record = new GeneratorRecord { First = br.ReadInt32(), Count = br.ReadInt32() };
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

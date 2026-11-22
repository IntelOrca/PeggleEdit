using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using IntelOrca.PeggleEdit.Tools.Pack;
using Xunit;
using Xunit.Abstractions;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Checks every level shipped with Pego, Peggle and Peggle Nights, and any custom level packs installed with
    /// them. The games can not be distributed with the tests, so set PEGGLE_GAMES_DIR to a folder containing the
    /// installed games (searched recursively for .dat and .pak files) to run these.
    /// </summary>
    public class GameCorpusTests
    {
        const string EnvironmentVariable = "PEGGLE_GAMES_DIR";

        readonly ITestOutputHelper mOutput;

        public GameCorpusTests(ITestOutputHelper output)
        {
            mOutput = output;
        }

        [Fact]
        public void EveryLevel_RoundTripsThroughXml()
        {
            var failures = new List<string>();
            int levels = 0, others = 0, rejected = 0;
            foreach (var (name, data) in GetCorpus())
            {
                // A file converts exactly when PeggleEdit can open it
                bool loads = Loads(data);
                string xml;
                try
                {
                    xml = LevelXml.Export(data);
                }
                catch (InvalidDataException ex)
                {
                    if (loads)
                        failures.Add($"{name}: PeggleEdit can open it, but: {ex.Message}");
                    rejected++;
                    continue;
                }
                if (!loads)
                {
                    failures.Add($"{name}: PeggleEdit can not open it, but it was exported");
                    continue;
                }

                var back = LevelXml.Import(xml);
                if (LevelXml.Export(back) != xml)
                    failures.Add($"{name}: xml -> dat -> xml differs");
                if (IsLevel(data))
                {
                    levels++;
                    if (!back.SequenceEqual(data))
                        failures.Add($"{name}: dat -> xml -> dat differs");
                    var model = LevelXmlTests.ModelRoundTrip(data);
                    if (HasGeneratorEntries(data))
                    {
                        // Saved by an older PeggleEdit with its generators in the entries, which the game can not
                        // read. They are now saved as the entries they generate, and come back as generators.
                        var problem = CheckGeneratorsSavedForTheGame(data, model);
                        if (problem != null)
                            failures.Add($"{name}: {problem}");
                    }
                    else if (!model.SequenceEqual(data))
                    {
                        failures.Add($"{name}: dat -> Level -> dat differs");
                    }
                }
                else
                {
                    // e.g. Pego's level editor globals: an empty level followed by other data, which is not kept
                    others++;
                    if (back.Length > data.Length || !back.SequenceEqual(data.Take(back.Length)))
                        failures.Add($"{name}: dat -> xml -> dat is not the start of the file");
                }
            }
            mOutput.WriteLine($"{levels} levels round tripped, {others} other files converted without their excess data, {rejected} files rejected.");
            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }

        /// <summary>
        /// Whether the file is a level, rather than another file using the same extension such as a trailer
        /// recording or Pego's level editor globals (which has a level header with no entries, then other data).
        /// </summary>
        static bool Loads(byte[] data)
        {
            // PeggleEdit opens files with any header byte, but only 1 is a level (other PopCap games' .dat files are not)
            if (data.Length < 5 || data[4] != 1)
                return false;
            using (var reader = new LevelReader(data))
                return reader.Read() != null;
        }

        static Level Read(byte[] data)
        {
            using (var reader = new LevelReader(data))
                return reader.Read();
        }

        static bool HasGeneratorEntries(byte[] data) =>
            Read(data).Entries.ToArray().Any(x => x is IEntryGenerator);

        static string CheckGeneratorsSavedForTheGame(byte[] data, byte[] saved)
        {
            var names = XElement.Parse(LevelXml.Export(saved)).Elements().Select(x => x.Name.LocalName).ToList();
            if (names.Take(names.Count - 1).Any(x => x.EndsWith("Generator")))
                return "a generator is still in the entries the game reads";
            if (names.LastOrDefault() != "PeggleEdit")
                return "the generators are not in PeggleEdit's data";
            var before = Read(data).Entries.ToArray().Select(x => x.GetType());
            var after = Read(saved).Entries.ToArray().Select(x => x.GetType());
            if (!before.SequenceEqual(after))
                return "the generators did not come back where they were";
            return null;
        }

        static bool IsLevel(byte[] data)
        {
            if (data.Length < 9 || (BitConverter.ToInt32(data, 5) == 0 && data.Length != 9))
                return false;
            using (var reader = new LevelReader(data))
                return reader.Read() != null;
        }

        IEnumerable<(string Name, byte[] Data)> GetCorpus()
        {
            var root = Environment.GetEnvironmentVariable(EnvironmentVariable);
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                mOutput.WriteLine($"Skipped: set {EnvironmentVariable} to the folder with the installed games.");
                yield break;
            }
            root = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);

            foreach (var path in Directory.GetFiles(root, "*.dat", SearchOption.AllDirectories))
            {
                // Player profiles, high scores etc.
                if (path.IndexOf(@"\userdata\", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                yield return (path.Substring(root.Length + 1), File.ReadAllBytes(path));
            }
            foreach (var path in Directory.GetFiles(root, "*.pak", SearchOption.AllDirectories))
            {
                var pak = new PakCollection(path);
                foreach (var record in pak.Records)
                {
                    if (record.FileName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
                        yield return (path.Substring(root.Length + 1) + "|" + record.FileName, record.Buffer);
                }
            }
        }
    }
}

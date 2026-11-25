using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using IntelOrca.PeggleEdit.Tools.Pack.CFG;
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

        [Fact]
        public void EveryImage_Decodes()
        {
            var failures = new List<string>();
            int images = 0, transparent = 0;
            foreach (var (name, data) in GetFiles(".jp2"))
            {
                images++;
                try
                {
                    using (var image = (Bitmap)J2K.ConvertJPEG2(data))
                    {
                        if (image == null || image.Width <= 0 || image.Height <= 0)
                            failures.Add($"{name}: no image");
                        else if (HasTransparency(image))
                            transparent++;
                    }
                }
                catch (Exception ex)
                {
                    failures.Add($"{name}: {ex.Message}");
                }
            }
            mOutput.WriteLine($"{images} images decoded ({(Environment.Is64BitProcess ? 64 : 32)}-bit), {transparent} with transparency.");
            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
            if (images > 0)
            {
                // The games have images with an alpha channel, which CSJ2K could not decode
                Assert.True(transparent > 0, "No image has transparency");
            }
        }

        static bool HasTransparency(Bitmap image)
        {
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var row = new int[image.Width];
                for (int y = 0; y < image.Height; y++)
                {
                    Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, image.Width);
                    foreach (var pixel in row)
                    {
                        if ((uint)pixel >> 24 != 255)
                            return true;
                    }
                }
                return false;
            }
            finally
            {
                image.UnlockBits(data);
            }
        }

        [Fact]
        public void EveryGame_OpensItsLevels()
        {
            var root = Environment.GetEnvironmentVariable(EnvironmentVariable);
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                mOutput.WriteLine($"Skipped: set {EnvironmentVariable} to the folder with the installed games.");
                return;
            }

            // The games, extracted or not: those with a levels\stages.cfg in their directory or their main.pak
            var games = Directory.GetFiles(root, "stages.cfg", SearchOption.AllDirectories)
                .Where(x => Path.GetFileName(Path.GetDirectoryName(x)).Equals("levels", StringComparison.OrdinalIgnoreCase))
                .Select(LevelPack.GetGameDirectory)
                .Concat(Directory.GetFiles(root, "main.pak", SearchOption.AllDirectories)
                    .Where(x => new PakCollection(x).GetRecord(@"levels\stages.cfg") != null)
                    .Select(Path.GetDirectoryName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var failures = new List<string>();
            foreach (var game in games)
            {
                var pack = new LevelPack();
                var problems = pack.OpenGame(game);
                failures.AddRange(problems.Select(x => $"{game}: {x}"));
                if (pack.Levels.Count == 0)
                    failures.Add($"{game}: no levels");
                var backgrounds = pack.Levels.Count(x => x.Background != null);
                mOutput.WriteLine($"{game}: {pack.Levels.Count} levels, {backgrounds} with a background, {pack.ChallengePages.Count} challenge pages");

                // Saved as a level pack, the challenges keep everything they had in the game
                var trophy = File.Exists(Path.Combine(game, "main.pak"))
                    ? new PakCollection(Path.Combine(game, "main.pak")).GetRecord(@"levels\trophy.cfg")?.Buffer
                    : null;
                trophy = trophy ?? File.ReadAllBytes(Path.Combine(game, "levels", "trophy.cfg"));
                var pages = CFGDocument.Read(System.Text.Encoding.UTF8.GetString(trophy)).Blocks[0].GetBlocks("page");
                for (int i = 0; i < pages.Length; i++)
                {
                    var written = pack.ChallengePages[i].GetCFGBlock();
                    failures.AddRange(MissingProperties(pages[i], written).Select(x => $"{game}: page {pages[i].Value}: {x}"));
                    var trophies = pages[i].GetBlocks("trophy");
                    var writtenTrophies = written.GetBlocks("trophy");
                    for (int j = 0; j < trophies.Length; j++)
                        failures.AddRange(MissingProperties(trophies[j], writtenTrophies[j]).Select(x => $"{game}: {trophies[j].Value}: {x}"));
                }
            }
            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }

        /// <summary>
        /// The properties of <paramref name="original"/> that <paramref name="written"/> does not have, leaving out flags
        /// that are false (the default), and with the powerups in any order.
        /// </summary>
        static IEnumerable<string> MissingProperties(CFGBlock original, CFGBlock written)
        {
            string Values(CFGProperty p, bool sort)
            {
                var values = Enumerable.Range(0, p.Count).Select(i => p[i]);
                return string.Join(", ", sort ? values.OrderBy(x => x, StringComparer.OrdinalIgnoreCase) : values);
            }

            foreach (var property in original.Properties)
            {
                if (property.Count == 1 && property[0].Equals("false", StringComparison.OrdinalIgnoreCase))
                    continue;
                var sort = property.Name.Equals("Powerups", StringComparison.OrdinalIgnoreCase);
                var expected = Values(property, sort);
                if (!written.GetProperties(property.Name).Any(x => Values(x, sort).Equals(expected, StringComparison.OrdinalIgnoreCase)))
                    yield return $"{property.Name}: {expected} is missing";
            }
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

        IEnumerable<(string Name, byte[] Data)> GetCorpus() =>
            // Leaving out player profiles, high scores etc.
            GetFiles(".dat").Where(x => (@"\" + x.Name).IndexOf(@"\userdata\", StringComparison.OrdinalIgnoreCase) < 0);

        /// <summary>
        /// Gets the files with the extension in the games, loose and in their .pak files.
        /// </summary>
        IEnumerable<(string Name, byte[] Data)> GetFiles(string extension)
        {
            var root = Environment.GetEnvironmentVariable(EnvironmentVariable);
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                mOutput.WriteLine($"Skipped: set {EnvironmentVariable} to the folder with the installed games.");
                yield break;
            }
            root = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);

            foreach (var path in Directory.GetFiles(root, "*" + extension, SearchOption.AllDirectories))
                yield return (path.Substring(root.Length + 1), File.ReadAllBytes(path));
            foreach (var path in Directory.GetFiles(root, "*.pak", SearchOption.AllDirectories))
            {
                var pak = new PakCollection(path);
                foreach (var record in pak.Records)
                {
                    if (record.FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                        yield return (path.Substring(root.Length + 1) + "|" + record.FileName, record.Buffer);
                }
            }
        }
    }
}

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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Extensions;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Pack.CFG;
using IntelOrca.PeggleEdit.Tools.Pack.Challenge;

namespace IntelOrca.PeggleEdit.Tools.Pack
{
    /// <summary>
    /// Represents a level pack containing information about the pack and the levels and images in it.
    /// </summary>
    public class LevelPack
    {
        private static string _pegglePath;
        private static int _mainPakLoad;
        private static PakCollection _mainPak { get; set; }

        private readonly List<LevelInfo> mLevelInfos = new List<LevelInfo>();

        public static LevelPack Current { get; set; }

        public List<Level> Levels { get; } = new List<Level>();
        public List<ChallengePage> ChallengePages { get; } = new List<ChallengePage>();
        public Dictionary<string, PakImage> Images { get; } = new Dictionary<string, PakImage>(StringComparer.OrdinalIgnoreCase);
        public string Name { get; set; } = "Untitled Pack";
        public string Description { get; set; } = "Type your description here.";

        /// <summary>
        /// Points the game file lookups (images, levels\_base.dat) at a game directory, and its main.pak or
        /// <paramref name="mainPakPath"/> if given (e.g. a game's renamed main.pak).
        /// </summary>
        public static void RegisterPegglePath(string pegglePath, string mainPakPath = null)
        {
            if (_pegglePath != pegglePath || _mainPakPath != mainPakPath)
            {
                _pegglePath = pegglePath;
                _mainPakPath = mainPakPath;
                _mainPakLoad = 0;
                _mainPak = null;
                _mainImageCache.Clear();
            }
        }

        private static string _mainPakPath;

        /// <summary>
        /// Whether the file is a game's own .pak (main.pak), with the game's levels\stages.cfg, rather than a level
        /// pack.
        /// </summary>
        public static bool IsGamePak(string path)
        {
            try
            {
                return File.Exists(path) && new PakCollection(path).GetRecord(Path.Combine("levels", "stages.cfg")) != null;
            }
            catch
            {
                return false;
            }
        }

        private static readonly string[] ImageExtensions = { ".jp2", ".jpg", ".png", ".gif" };
        private static readonly Dictionary<string, PakImage> _mainImageCache = new Dictionary<string, PakImage>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The paths the game looks for an image at: images given by a level object are just a name, which the game
        /// finds in images\levels.
        /// </summary>
        private static IEnumerable<string> GetImageCandidates(string key)
        {
            yield return key;
            if (!key.Contains("\\"))
                yield return Path.Combine("images", "levels", key);
        }

        private static void LoadMainPak()
        {
            if (_mainPakLoad != 0)
                return;

            _mainPakLoad = 2;
            if (string.IsNullOrEmpty(_pegglePath))
                return;

            var mainPak = _mainPakPath ?? Path.Combine(_pegglePath, "main.pak");
            try
            {
                if (File.Exists(mainPak))
                {
                    _mainPak = new PakCollection(mainPak);
                    _mainPakLoad = 1;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// The game directory registered with <see cref="RegisterPegglePath"/>, if any.
        /// </summary>
        public static string PegglePath => _pegglePath;

        /// <summary>
        /// Gets a file of the game (e.g. levels\_base.dat), from main.pak or from the game directory when it has been
        /// extracted, or null if it can not be found.
        /// </summary>
        public static byte[] GetGameFile(string path) => GetMainFile(path);

        /// <summary>
        /// Gets a file of the game, from main.pak or from the game directory when it has been extracted.
        /// </summary>
        private static byte[] GetMainFile(string path)
        {
            LoadMainPak();
            if (_mainPakLoad == 1)
            {
                var record = _mainPak.GetRecord(path);
                if (record != null)
                    return record.Buffer;
            }
            if (!string.IsNullOrEmpty(_pegglePath))
            {
                try
                {
                    var realFileName = Path.Combine(_pegglePath, path);
                    if (File.Exists(realFileName))
                        return File.ReadAllBytes(realFileName);
                }
                catch
                {
                }
            }
            return null;
        }

        private PakImage GetMainPakImage(string key)
        {
            if (_mainImageCache.TryGetValue(key, out var cached))
                return cached;

            PakImage result = null;
            foreach (var candidate in GetImageCandidates(key))
            {
                result = FindImage(candidate, GetMainFile);
                if (result != null)
                    break;
            }
            _mainImageCache[key] = result;
            return result;
        }

        // The pack's images combined with their alpha images, by name, with the images they were made from
        private readonly Dictionary<string, (PakImage Colour, PakImage Alpha, PakImage Combined)> _combinedImageCache =
            new Dictionary<string, (PakImage, PakImage, PakImage)>(StringComparer.OrdinalIgnoreCase);

        private PakImage GetPackImage(string key)
        {
            foreach (var candidate in GetImageCandidates(key))
            {
                foreach (var extension in ImageExtensions)
                {
                    if (Images.TryGetValue(candidate + extension, out var image))
                    {
                        var alpha = FindPackAlphaImage(candidate);
                        if (alpha == null)
                            return image;

                        // Combined once, and again only if either image is replaced
                        if (_combinedImageCache.TryGetValue(candidate, out var cached) &&
                            cached.Colour == image && cached.Alpha == alpha)
                        {
                            return cached.Combined;
                        }
                        var combined = CombineAlpha(image, alpha) ?? image;
                        _combinedImageCache[candidate] = (image, alpha, combined);
                        return combined;
                    }
                }
            }
            return null;
        }

        private PakImage FindPackAlphaImage(string key)
        {
            foreach (var extension in ImageExtensions)
            {
                if (Images.TryGetValue(key + "_" + extension, out var image))
                    return image;
            }
            return null;
        }

        /// <summary>
        /// Finds an image as the game does: a colour image (.jp2, .jpg, .png or .gif) which takes its alpha from an image
        /// of the same name ending with an underscore if there is one, or only an alpha image (drawn black).
        /// </summary>
        private static PakImage FindImage(string key, Func<string, byte[]> getFile)
        {
            var alpha = FindAlphaImage(key, getFile);
            foreach (var extension in ImageExtensions)
            {
                var data = getFile(key + extension);
                if (data != null)
                {
                    var image = new PakImage(key + extension, data);
                    return alpha == null ? image : CombineAlpha(image, alpha) ?? image;
                }
            }
            if (alpha != null)
                return CombineAlpha(null, alpha);
            return null;
        }

        private static PakImage FindAlphaImage(string key, Func<string, byte[]> getFile)
        {
            foreach (var extension in ImageExtensions)
            {
                var path = key + "_" + extension;
                var data = getFile(path);
                if (data != null)
                    return new PakImage(path, data);
            }
            return null;
        }

        /// <summary>
        /// Makes a 32-bit image from a colour image (or black if there is none) and the brightness of an alpha image.
        /// </summary>
        private static PakImage CombineAlpha(PakImage colour, PakImage alpha)
        {
            var alphaImage = alpha.Image;
            if (alphaImage == null)
                return null;

            var colourImage = colour?.Image;
            var size = colourImage?.Size ?? alphaImage.Size;
            var rect = new Rectangle(Point.Empty, size);
            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                if (colourImage != null)
                    g.DrawImage(colourImage, rect, rect, GraphicsUnit.Pixel);
                else
                    g.Clear(Color.Black);
            }

            using (var alphaCopy = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(alphaCopy))
                {
                    g.DrawImage(alphaImage, rect, new Rectangle(Point.Empty, alphaImage.Size), GraphicsUnit.Pixel);
                }
                var dstData = result.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var srcData = alphaCopy.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    var dst = new int[dstData.Stride / 4 * size.Height];
                    var src = new int[srcData.Stride / 4 * size.Height];
                    Marshal.Copy(dstData.Scan0, dst, 0, dst.Length);
                    Marshal.Copy(srcData.Scan0, src, 0, src.Length);
                    for (var i = 0; i < dst.Length; i++)
                    {
                        // The alpha is the red channel of the (grey) alpha image
                        var a = (src[i] >> 16) & 0xFF;
                        dst[i] = (dst[i] & 0x00FFFFFF) | (a << 24);
                    }
                    Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
                }
                finally
                {
                    result.UnlockBits(dstData);
                    alphaCopy.UnlockBits(srcData);
                }
            }

            var name = Path.ChangeExtension(colour?.FileName ?? alpha.FileName.Replace("_.", "."), ".png");
            return new PakImage(name, result);
        }

        public PakImage GetImage(string key)
        {
            if (key == null)
                return null;

            key = key.Replace("/", "\\");
            return GetPackImage(key) ?? GetMainPakImage(key);
        }

        public bool Open(string path)
        {
            try
            {
                var pakFile = new PakCollection(path);
                OpenFromPack(pakFile, path);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Open Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public bool Save(string path)
        {
            try
            {
                var pakFile = SaveToPack(path);
                pakFile.Save(path);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void OpenFromPack(PakCollection pakFile, string path)
        {
            var cfgFileName = GetCfgFileName(path);
            var cfgRecord = GetCFGRecord(cfgFileName, pakFile);
            if (cfgRecord == null)
            {
                throw new InvalidDataException("Unable to find a cfg file in the level pack.");
            }

            ParseCFG(Encoding.UTF8.GetString(cfgRecord.Buffer));

            // Load levels
            foreach (var linfo in mLevelInfos)
            {
                var buffer = pakFile.GetRecord(Path.Combine("levels", linfo.Filename + ".dat")).Buffer;
                using (var levelReader = new LevelReader(buffer))
                {
                    var level = levelReader.Read();
                    if (level == null)
                    {
                        MessageBox.Show($"Unable to open {linfo.Name}", "Open Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }

                    level.Info = linfo;
                    level.Background = GetImage(pakFile, Path.Combine("levels", linfo.Filename));
                    level.Thumbnail = GetImage(pakFile, Path.Combine("levels", "cached_thumbnails", linfo.Filename));
                    level.Hash = GetLevelData(level).CalculateFnv1a();
                    Levels.Add(level);

                    var warning = EditorData.GetWarning(level.EditorDataStatus);
                    if (warning != null)
                        MessageBox.Show($"{linfo.Name}: {warning}", "Open Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // Load any images
            foreach (var record in pakFile)
            {
                if (record.FileName.StartsWith("levels\\"))
                    continue;

                var fileName = Path.GetFileName(record.FileName);
                if (fileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".jp2", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase))
                {
                    Images.Add(record.FileName, new PakImage(fileName, record.Buffer));
                }
            }
        }

        /// <summary>
        /// Gets the game directory of a path given to <see cref="OpenGame"/>: the directory itself, the directory of a
        /// .pak file, or the directory above the levels directory of a stages.cfg.
        /// </summary>
        public static string GetGameDirectory(string path)
        {
            path = Path.GetFullPath(path);
            // A trailing separator would make the name empty, but a drive's root keeps its own
            if (path.Length > Path.GetPathRoot(path).Length)
                path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (Directory.Exists(path))
                return path;
            var directory = Path.GetDirectoryName(path);
            if (Path.GetFileName(path).Equals("stages.cfg", StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileName(directory).Equals("levels", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetDirectoryName(directory);
            }
            return directory;
        }

        /// <summary>
        /// Opens the levels a game comes with (Peggle, Peggle Nights or Pego) into this pack, so that they can be looked
        /// at, copied from, or saved as a level pack of their own: those its levels\stages.cfg lists, in order, then any
        /// other levels it has, and the challenges of its levels\trophy.cfg. The path is the game's directory, its
        /// main.pak (or another .pak of it), or its levels\stages.cfg. Files are read from the .pak first, then from the
        /// game directory, for a game that has been extracted.
        /// </summary>
        /// <returns>The levels that could not be read.</returns>
        public IReadOnlyList<string> OpenGame(string path)
        {
            var gameDirectory = GetGameDirectory(path);
            PakCollection pak = null;
            if (path.EndsWith(".pak", StringComparison.OrdinalIgnoreCase) && File.Exists(path))
            {
                // A game's .pak (which may have been renamed), not a level pack, whose levels would be taken as the game's
                pak = new PakCollection(path);
                if (pak.GetRecord(Path.Combine("levels", "stages.cfg")) == null)
                {
                    throw new InvalidDataException(
                        $"'{Path.GetFileName(path)}' is not a game's main.pak, as it has no levels\\stages.cfg. A level pack is opened with Open.");
                }
            }
            else if (File.Exists(Path.Combine(gameDirectory, "main.pak")))
            {
                pak = new PakCollection(Path.Combine(gameDirectory, "main.pak"));
            }

            byte[] GetFile(string name)
            {
                var record = pak?.GetRecord(name);
                if (record != null)
                    return record.Buffer;
                var filePath = Path.Combine(gameDirectory, name);
                return File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;
            }

            PakImage GetImage(string name)
            {
                foreach (var extension in new[] { ".jp2", ".jpg", ".png" })
                {
                    var data = GetFile(name + extension);
                    if (data != null)
                        return new PakImage(Path.GetFileName(name) + extension, data);
                }
                return null;
            }

            var stages = GetFile(Path.Combine("levels", "stages.cfg"));
            if (stages == null)
                throw new InvalidDataException($"No levels\\stages.cfg was found in '{gameDirectory}'.");

            Name = Path.GetFileName(gameDirectory);
            Description = $"The levels that {Name} comes with.";
            Levels.Clear();
            ChallengePages.Clear();

            // The levels the stages list, then any others in the levels directory (but not the base level that the
            // others are drawn over, or any others starting with _)
            var infos = new List<LevelInfo>();
            var listed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var document = CFGDocument.Read(Encoding.UTF8.GetString(stages));
            foreach (var block in document.Blocks[0].GetBlocks("stage"))
            {
                foreach (var property in block)
                {
                    if (!property.Name.Equals("level", StringComparison.OrdinalIgnoreCase) || property.Count == 0)
                        continue;

                    var fileName = property[0];
                    if (!listed.Add(fileName))
                        continue;

                    // Peggle's stages give no ace score
                    var info = LevelInfo.DefaultInfo;
                    info.Filename = fileName;
                    info.Name = property.Count > 1 ? property[1] : fileName;
                    if (property.Count > 2 && int.TryParse(property[2], out var aceScore))
                        info.AceScore = aceScore;
                    infos.Add(info);
                }
            }
            var others = (pak?.GetRecords("levels").Select(x => x.FileName) ?? Enumerable.Empty<string>())
                .Concat(Directory.Exists(Path.Combine(gameDirectory, "levels"))
                    ? Directory.GetFiles(Path.Combine(gameDirectory, "levels"), "*.dat")
                    : new string[0])
                .Where(x => x.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileNameWithoutExtension)
                .Where(x => !x.StartsWith("_"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(x => !listed.Contains(x))
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
            foreach (var fileName in others)
            {
                var info = LevelInfo.DefaultInfo;
                info.Filename = fileName;
                info.Name = fileName;
                infos.Add(info);
            }

            var problems = new List<string>();
            foreach (var info in infos)
            {
                var data = GetFile(Path.Combine("levels", info.Filename + ".dat"));
                if (data == null)
                {
                    problems.Add($"{info.Name}: levels\\{info.Filename}.dat was not found");
                    continue;
                }

                Level level;
                string error;
                using (var reader = new LevelReader(data))
                {
                    level = reader.Read();
                    error = reader.Error;
                }
                if (level == null)
                {
                    problems.Add($"{info.Name}: levels\\{info.Filename}.dat could not be read ({error})");
                    continue;
                }
                level.Info = info;
                level.Background = GetImage(Path.Combine("levels", info.Filename));
                level.Thumbnail = GetImage(Path.Combine("levels", "cached_thumbnails", info.Filename));
                level.Hash = GetLevelData(level).CalculateFnv1a();
                Levels.Add(level);
            }

            var trophy = GetFile(Path.Combine("levels", "trophy.cfg"));
            if (trophy != null)
            {
                foreach (var block in CFGDocument.Read(Encoding.UTF8.GetString(trophy)).Blocks[0].GetBlocks("page"))
                    ChallengePages.Add(new ChallengePage(block));
            }
            return problems;
        }

        /// <summary>
        /// Gets a level file name like the given one that no other level in the pack uses (each level is saved as
        /// levels\&lt;file name&gt;.dat): the name itself if it is free, otherwise the name with _2, _3 and so on after it.
        /// </summary>
        /// <param name="filename">The file name wanted.</param>
        /// <param name="level">The level being named, whose own file name does not count as used.</param>
        public string GetUniqueLevelFilename(string filename, Level level = null)
        {
            var used = new HashSet<string>(
                Levels.Where(x => x != level).Select(x => x.Info.Filename),
                StringComparer.OrdinalIgnoreCase);
            if (!used.Contains(filename))
                return filename;
            for (int i = 2; ; i++)
            {
                var candidate = $"{filename}_{i}";
                if (!used.Contains(candidate))
                    return candidate;
            }
        }

        /// <summary>
        /// Gets the file names that more than one level of the pack has, whose levels would be saved over each other.
        /// </summary>
        public IReadOnlyList<string> GetDuplicateLevelFilenames()
        {
            return Levels
                .GroupBy(x => x.Info.Filename, StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();
        }

        private PakCollection SaveToPack(string path)
        {
            // Levels with the same file name would be written to the same file, losing all but one of them
            var duplicates = GetDuplicateLevelFilenames();
            if (duplicates.Count != 0)
            {
                throw new InvalidDataException(
                    $"More than one level has the file name {String.Join(", ", duplicates.Select(x => $"'{x}'"))}, so they would be saved over each other. " +
                    "Give each level its own file name in its Properties (right-click it in the Pack Explorer).");
            }

            var pakFile = new PakCollection();

            // CFG
            var cfgName = GetCfgFileName(path);
            var cfgRecord = new PakRecord(pakFile, cfgName, DateTime.Now);
            cfgRecord.Buffer = Encoding.UTF8.GetBytes(WriteCFG());
            pakFile.Records.Add(cfgRecord);

            // Levels
            foreach (var lvl in Levels)
            {
                var datRecord = new PakRecord(pakFile, Path.Combine("levels", $"{lvl.Info.Filename}.dat"), DateTime.Now);
                datRecord.Buffer = GetLevelData(lvl);
                pakFile.Records.Add(datRecord);

                if (lvl.Background != null)
                {
                    var extension = Path.GetExtension(lvl.Background.FileName);
                    var bgImageRecord = new PakRecord(pakFile, Path.Combine("levels", $"{lvl.Info.Filename}{extension}"), DateTime.Now);
                    bgImageRecord.Buffer = lvl.Background.Data;
                    pakFile.Records.Add(bgImageRecord);
                }

                var oldHash = lvl.Hash;
                var newHash = datRecord.Buffer.CalculateFnv1a();
                if (oldHash != newHash)
                {
                    lvl.Hash = newHash;
                    lvl.Thumbnail = null;
                }

                if (lvl.Thumbnail == null)
                {
                    try
                    {
                        lvl.Thumbnail = new PakImage($"{lvl.Info.Filename}.png", lvl.GetThumbnail());
                    }
                    catch
                    {
                    }
                }
                if (lvl.Thumbnail != null)
                {
                    var extension = Path.GetExtension(lvl.Thumbnail.FileName);
                    var tbRecord = new PakRecord(pakFile, Path.Combine("levels", "cached_thumbnails", $"{lvl.Info.Filename}{extension}"), DateTime.Now);
                    tbRecord.Buffer = lvl.Thumbnail.Data;
                    pakFile.Records.Add(tbRecord);
                }
            }

            // Images
            foreach (var kvp in Images)
            {
                var iRecord = new PakRecord(pakFile, kvp.Key, DateTime.Now);
                iRecord.Buffer = kvp.Value.Data;
                pakFile.Records.Add(iRecord);
            }

            return pakFile;
        }

        #region Changes since saved

        private SavedState _savedState;

        /// <summary>
        /// Takes the pack as it is now as saved (or as opened), so that <see cref="IsModified()"/> says whether it has
        /// been changed since.
        /// </summary>
        public void MarkSaved() => _savedState = new SavedState(this);

        /// <summary>
        /// Whether anything that would be saved has changed since <see cref="MarkSaved"/>: a level, the levels or their
        /// order, a background, the pack's details, its challenges or its images. Changing something and changing it
        /// back again is no change.
        /// </summary>
        public bool IsModified() => IsModified(Levels);

        /// <summary>
        /// As <see cref="IsModified()"/>, but comparing the objects of only the given levels (which is the slow part),
        /// e.g. those that have been edited.
        /// </summary>
        public bool IsModified(IEnumerable<Level> levelsToCompare)
        {
            if (_savedState == null || !_savedState.PackMatches(this))
                return true;
            foreach (var level in levelsToCompare)
            {
                if (Levels.Contains(level) && IsLevelModified(level))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Whether a level has changed since <see cref="MarkSaved"/>: its objects, details or background. A level that
        /// was not in the pack then has.
        /// </summary>
        public bool IsLevelModified(Level level) => _savedState == null || !_savedState.LevelMatches(level, GetLevelData(level));

        /// <summary>
        /// The levels that have changed since <see cref="MarkSaved"/>.
        /// </summary>
        public IReadOnlyList<Level> GetModifiedLevels() => Levels.Where(IsLevelModified).ToArray();

        /// <summary>
        /// What is saved of a pack, at one time.
        /// </summary>
        private sealed class SavedState
        {
            private readonly string _cfg;
            private readonly Level[] _levels;
            private readonly Dictionary<Level, (byte[] Data, LevelInfo Info, PakImage Background)> _levelStates =
                new Dictionary<Level, (byte[], LevelInfo, PakImage)>();
            private readonly KeyValuePair<string, PakImage>[] _images;

            public SavedState(LevelPack pack)
            {
                _cfg = pack.WriteCFG(signature: false);
                _levels = pack.Levels.ToArray();
                foreach (var level in _levels)
                    _levelStates[level] = (pack.GetLevelData(level), level.Info, level.Background);
                _images = GetImages(pack);
            }

            private static KeyValuePair<string, PakImage>[] GetImages(LevelPack pack) =>
                pack.Images.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray();

            /// <summary>
            /// Whether all but the levels' objects are as they were: the details, levels and order (in the cfg), the
            /// levels themselves, their backgrounds and the images.
            /// </summary>
            public bool PackMatches(LevelPack pack)
            {
                if (pack.Levels.Count != _levels.Length || pack.Images.Count != _images.Length)
                    return false;
                for (int i = 0; i < _levels.Length; i++)
                {
                    if (pack.Levels[i] != _levels[i] || !SameImage(pack.Levels[i].Background, _levelStates[_levels[i]].Background))
                        return false;
                }
                // By name as it is saved, so that a change of case is a change
                var images = GetImages(pack);
                for (int i = 0; i < _images.Length; i++)
                {
                    if (images[i].Key != _images[i].Key || !SameImage(images[i].Value, _images[i].Value))
                        return false;
                }
                return pack.WriteCFG(signature: false) == _cfg;
            }

            public bool LevelMatches(Level level, byte[] data)
            {
                if (!_levelStates.TryGetValue(level, out var saved))
                    return false;
                return saved.Info.Equals(level.Info) && SameImage(saved.Background, level.Background) &&
                    data.AsSpan().SequenceEqual(saved.Data);
            }

            /// <summary>
            /// Whether two images would be saved the same: the same type (which is the extension of a background
            /// saved) and data, e.g. the same file added again.
            /// </summary>
            private static bool SameImage(PakImage a, PakImage b)
            {
                if (a == b)
                    return true;
                if (a == null || b == null || a.Data == null || b.Data == null)
                    return false;
                return String.Equals(Path.GetExtension(a.FileName), Path.GetExtension(b.FileName), StringComparison.OrdinalIgnoreCase) &&
                    a.Data.AsSpan().SequenceEqual(b.Data);
            }
        }

        #endregion

        private PakRecord GetCFGRecord(string pakFilename, PakCollection pakFile)
        {
            var cfgRecord = pakFile.GetRecord(pakFilename + ".cfg");
            if (cfgRecord == null)
            {
                // Missing cfg or named wrong
                cfgRecord = pakFile
                    .GetRecords()
                    .FirstOrDefault(x => x.FileName.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase));
            }
            return cfgRecord;
        }

        private byte[] GetLevelData(Level level)
        {
            var ms = new MemoryStream();
            var writer = new LevelWriter(ms);
            writer.Write(level, LevelWriter.DefaultFileVersion);
            return ms.ToArray();
        }

        private void ParseCFG(string cfg)
        {
            var document = CFGDocument.Read(cfg);
            var stages = document.Blocks[0].GetBlocks("stage");
            foreach (var block in stages)
            {
                foreach (var property in block)
                {
                    switch (property.Name.ToLower())
                    {
                        case "name":
                            Name = property[0];
                            break;
                        case "desc":
                            Description = property[0];
                            break;
                        case "level":
                            var levelInfo = new LevelInfo
                            {
                                Filename = property[0],
                                Name = property[1],
                                AceScore = Convert.ToInt32(property[2]),
                                MinStage = Convert.ToInt32(property[3])
                            };
                            mLevelInfos.Add(levelInfo);
                            break;
                    }
                }
            }

            var challengePages = document.Blocks[0].GetBlocks("page");
            foreach (var block in challengePages)
            {
                ChallengePages.Add(new ChallengePage(block));
            }
        }

        private string WriteCFG(bool signature = true)
        {
            var document = new CFGDocument();
            var docblock = new CFGBlock();

            // Number of stages needed
            var numStages = (int)Math.Ceiling(Levels.Count / 5.0f);
            for (var i = 0; i < numStages; i++)
            {
                var block = new CFGBlock();
                block.Name = "Stage";

                block.Properties.Add(new CFGProperty("Name", Name));
                block.Properties.Add(new CFGProperty("Desc", Description));

                var lowerBound = i * 5;
                var upperBound = Math.Min(Levels.Count, (i + 1) * 5);
                for (var j = lowerBound; j < upperBound; j++)
                {
                    var lvl = Levels[j];
                    block.Properties.Add(new CFGProperty("Level", lvl.Info.Filename, lvl.Info.Name, lvl.Info.AceScore.ToString(), lvl.Info.MinStage.ToString()));
                }

                docblock.Blocks.Add(block);
            }

            // Challenges
            foreach (var page in ChallengePages)
            {
                docblock.Blocks.Add(page.GetCFGBlock());
            }

            document.Blocks.Add(docblock);

            var writer = new CFGWriter(document) { IncludeSignature = signature };
            return writer.GetText();
        }

        /// <summary>
        /// Opens a level pack that has been exported to a folder.
        /// </summary>
        /// <returns>Whether it was opened.</returns>
        public bool Import(string selectedPath)
        {
            try
            {
                var pak = new PakCollection();
                var files = Directory.GetFiles(selectedPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var relativePath = file.Remove(0, selectedPath.Length + 1);
                    var relativeDirectory = Path.GetDirectoryName(relativePath);
                    pak.ImportFile(relativeDirectory, file);
                }
                OpenFromPack(pak, selectedPath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Import Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void Export(string selectedPath)
        {
            try
            {
                var pakFile = SaveToPack(selectedPath);
                pakFile.Export(selectedPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetCfgFileName(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            return fileName + ".cfg";
        }

        private PakImage GetImage(PakCollection collection, string fileName)
        {
            try
            {
                var record = collection.FindFirstRecordWithExtension(fileName, ".jp2", ".jpg", ".png");
                if (record != null)
                {
                    return GetImageFromRecord(record);
                }
            }
            catch
            {
            }
            return null;
        }

        private static PakImage GetImageFromRecord(PakRecord record)
        {
            var fileName = Path.GetFileName(record.FileName);
            return new PakImage(fileName, record.Buffer);
        }
    }
}

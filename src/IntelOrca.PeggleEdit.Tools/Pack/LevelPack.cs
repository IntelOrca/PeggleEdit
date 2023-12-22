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
using System.Linq;
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

        public static void RegisterPegglePath(string pegglePath)
        {
            _pegglePath = pegglePath;
        }

        private PakImage GetMainPakImage(string key)
        {
            if (_mainPakLoad == 0)
            {
                if (string.IsNullOrEmpty(_pegglePath))
                    return null;

                _mainPakLoad = 2;
                var mainPak = Path.Combine(_pegglePath, "main.pak");
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

            if (_mainPakLoad != 0)
            {
                key = key.Replace("/", "\\");
                foreach (var extension in new[] { ".jp2", ".jpg", ".png" })
                {
                    var key2 = key + extension;
                    if (_mainPakLoad == 1)
                    {
                        var record = _mainPak.GetRecord(key2);
                        if (record != null)
                        {
                            return new PakImage(record.FileName, record.Buffer);
                        }
                    }
                    else
                    {
                        var realFileName = Path.Combine(_pegglePath, key2);
                        try
                        {
                            if (File.Exists(realFileName))
                            {
                                return new PakImage(realFileName);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return null;
        }

        public PakImage GetImage(string key)
        {
            if (key == null)
                return null;

            key = key.Replace("/", "\\");
            foreach (var extension in new[] { ".jp2", ".jpg", ".png" })
            {
                var key2 = key + extension;
                if (Images.TryGetValue(key2, out var image))
                {
                    return image;
                }
            }
            return GetMainPakImage(key);
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

        private PakCollection SaveToPack(string path)
        {
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

        private string WriteCFG()
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

            var writer = new CFGWriter(document);
            return writer.GetText();
        }

        public void Import(string selectedPath)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

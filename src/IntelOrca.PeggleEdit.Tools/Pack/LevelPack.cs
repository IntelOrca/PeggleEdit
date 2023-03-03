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
using System.Text;
using System.Windows.Forms;
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
        public static LevelPack Current { get; set; }

        private readonly List<LevelInfo> mLevelInfos = new List<LevelInfo>();

        public List<Level> Levels { get; } = new List<Level>();
        public List<ChallengePage> ChallengePages { get; } = new List<ChallengePage>();
        public Dictionary<string, Image> Images { get; } = new Dictionary<string, Image>();
        public string Name { get; set; } = "Untitled Pack";
        public string Description { get; set; } = "Type your description here.";

        public bool Open(string path)
        {
            var successful = false;
            try
            {
                var pakFilename = Path.GetFileNameWithoutExtension(path);
                var pakFile = new PakCollection(path);

                var cfgRecord = GetCFGRecord(pakFilename, pakFile);
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
                        level.Background = GetBackground(pakFile, linfo.Filename);
                        Levels.Add(level);
                    }
                }

                // Load any images
                foreach (var record in pakFile)
                {
                    if (record.FileName.StartsWith("levels\\"))
                        continue;

                    if (record.FileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Images.Add(record.FileName, GetImageFromBuffer(record.Buffer));
                    }
                }

                successful = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Open Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return successful;
        }

        public bool Save(string path)
        {
            var successful = false;
            try
            {
                var pakFilename = Path.GetFileNameWithoutExtension(path);
                var pakFile = new PakCollection();

                // CFG
                var cfgRecord = new PakRecord(pakFile, pakFilename + ".cfg", DateTime.Now);
                cfgRecord.Buffer = Encoding.UTF8.GetBytes(WriteCFG());
                pakFile.Records.Add(cfgRecord);

                // Levels
                foreach (var lvl in Levels)
                {
                    var datRecord = new PakRecord(pakFile, Path.Combine("levels", $"{lvl.Info.Filename}.dat"), DateTime.Now);
                    datRecord.Buffer = GetLevelData(lvl);

                    if (lvl.Background != null)
                    {
                        var bgImageRecord = new PakRecord(pakFile, Path.Combine("levels", $"{lvl.Info.Filename}.png"), DateTime.Now);
                        bgImageRecord.Buffer = GetImageData(lvl.Background);
                        pakFile.Records.Add(bgImageRecord);
                    }

                    var tbRecord = new PakRecord(pakFile, Path.Combine("levels", "cached_thumbnails", $"{lvl.Info.Filename}.png"), DateTime.Now);
                    tbRecord.Buffer = GetImageData(lvl.GetThumbnail());

                    pakFile.Records.Add(datRecord);
                    pakFile.Records.Add(tbRecord);
                }

                // Images
                foreach (var kvp in Images)
                {
                    var iRecord = new PakRecord(pakFile, kvp.Key, DateTime.Now);
                    iRecord.Buffer = GetImageData(kvp.Value);
                    pakFile.Records.Add(iRecord);
                }

                pakFile.Save(path);

                successful = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return successful;
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

        private byte[] GetImageData(Image img)
        {
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        private Image GetImageFromBuffer(byte[] buffer)
        {
            Image img;
            try
            {
                var ms = new MemoryStream(buffer);
                img = Image.FromStream(ms);
            }
            catch
            {
                img = null;
            }
            return img;
        }

        private Image GetBackground(PakCollection collection, string levelFilename)
        {
            try
            {
                var fileName = Path.Combine("levels", levelFilename);
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

        private static Image GetImageFromRecord(PakRecord record)
        {
            return record.FileName.EndsWith(".jp2", StringComparison.OrdinalIgnoreCase) ?
                J2K.ConvertJPEG2(record) :
                Image.FromStream(new MemoryStream(record.Buffer));
        }
    }
}

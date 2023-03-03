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
using System.Drawing;
using System.IO;
using System.Text;
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Pack;
using IntelOrca.PeggleEdit.Tools.Pack.CFG;

namespace IntelOrca.PeggleEdit.Designer
{
    class PeggleNightsStoryExtractor
    {
        private readonly PakCollection mPakCollection;
        private readonly LevelPack mPack;

        public PeggleNightsStoryExtractor(string path)
        {
            mPakCollection = new PakCollection();
            mPakCollection.Open(path);

            mPack = new LevelPack();

            AddLevels();
        }

        public LevelPack Extract()
        {
            return mPack;
        }

        private void AddLevels()
        {
            var reader = new CFGReader(Encoding.ASCII.GetString(mPakCollection.GetRecord("levels\\stages.cfg").Buffer));
            var stages = reader.Document.Blocks[0].GetBlocks("stage");
            foreach (var block in stages)
            {
                foreach (var property in block)
                {
                    if (string.Equals(property.Name, "level", StringComparison.OrdinalIgnoreCase))
                    {
                        AddLevel(property[0], property[1], Convert.ToInt32(property[2]));
                    }
                }
            }
        }

        private void AddLevel(string name, string displayname, int acescore)
        {
            var filename = Path.Combine("levels", name + ".dat");
            var record = mPakCollection.GetRecord(filename);
            using (var reader = new LevelReader(record.Buffer))
            {
                var level = reader.Read();
                if (level == null)
                    throw new Exception($"Unable to read {name}");

                var info = new LevelInfo(name, displayname, acescore, -3);
                level.Info = info;
                level.Background = GetBackground(name);

                mPack.Levels.Add(level);
            }
        }

        private Image GetBackground(string levelFilename)
        {
            try
            {
                var fileName = Path.Combine("levels", levelFilename);
                var record = mPakCollection.FindFirstRecordWithExtension(fileName, ".jp2", ".jpg", ".png");
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

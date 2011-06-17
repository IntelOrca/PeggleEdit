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
using System.Linq;
using System.Text;
using IntelOrca.PeggleEdit.Tools.Pack;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Pack.CFG;
using System.IO;
using System.Drawing;
using IntelOrca.PeggleEdit.Tools;
using System.Drawing.Imaging;

namespace IntelOrca.PeggleEdit.Designer
{
	class PeggleNightsStoryExtractor
	{
		PakCollection mPakCollection;

		LevelPack mPack;

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
			CFGReader reader = new CFGReader(Encoding.ASCII.GetString(mPakCollection.GetRecord("levels\\stages.cfg").Buffer));
			CFGBlock[] stages = reader.Document.Blocks[0].GetBlocks("stage");
			foreach (CFGBlock block in stages) {
				foreach (CFGProperty property in block) {
					if (property.Name.ToLower() == "level") {
						AddLevel(property[0], property[1], Convert.ToInt32(property[2]));
					}
				}
			}
		}

		private void AddLevel(string name, string displayname, int acescore)
		{
			PakRecord record = mPakCollection.GetRecord("levels\\" + name + ".dat");

			LevelReader reader = new LevelReader(record.Buffer);
			Level level = reader.Read();
			reader.Dispose();

			if (level == null)
				throw new Exception("Unable to read " + name);

			LevelInfo info = new LevelInfo(name, displayname, acescore, -3);
			level.Info = info;

			Image bgImage = GetBackground(name);
			level.Background = bgImage;

			mPack.Levels.Add(level);
		}

		private Image GetBackground(string levelFilename)
		{
			string filename = "levels\\" + levelFilename;
			string jp2 = Path.ChangeExtension(filename, ".jp2");
			string jpg = Path.ChangeExtension(filename, ".jpg");
			string png = Path.ChangeExtension(filename, ".png");

			PakRecord record;
			record = mPakCollection.GetRecord(jp2);
			if (record != null) {
				byte[] buffer;
				OpenJPEG.ConvertJPEG2(record, out buffer, ImageFormat.Jpeg);
				return Image.FromStream(new MemoryStream(buffer));
			} else {
				record = mPakCollection.GetRecord(jpg);
				if (record != null)
					return GetImageFromBuffer(record.Buffer);

				record = mPakCollection.GetRecord(png);
				if (record != null)
					return GetImageFromBuffer(record.Buffer);
			}

			return null;
		}

		private Image GetImageFromBuffer(byte[] buffer)
		{
			Image img;
			try {
				MemoryStream ms = new MemoryStream(buffer);
				img = Image.FromStream(ms);
				//ms.Close();
			} catch {
				img = null;
			}

			return img;
		}
	}
}

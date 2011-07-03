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
using System.Text;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Tools.Pack.Challenge;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Pack.CFG;

namespace IntelOrca.PeggleEdit.Tools.Pack
{
	/// <summary>
	/// Represents a level pack containing information about the pack and the levels and images in it.
	/// </summary>
	public class LevelPack
	{
		string mName = "Untitled Pack";
		string mDescription = "Type your description here.";
		List<Level> mLevels = new List<Level>();
		List<ChallengePage> mChallengePages = new List<ChallengePage>();
		Dictionary<string, Image> mImages = new Dictionary<string, Image>();

		List<LevelInfo> mLevelInfos = new List<LevelInfo>();

		public bool Open(string path)
		{
			bool successful = false;
			try {
				string pakFilename = Path.GetFileNameWithoutExtension(path);

				PakCollection pakFile = new PakCollection();
				pakFile.Open(path);

				string cfgFile = UTF8Encoding.ASCII.GetString(pakFile.GetRecord(pakFilename + ".cfg").Buffer);
				ParseCFG(cfgFile);

				//Load levels
				foreach (LevelInfo linfo in mLevelInfos) {
					byte[] buffer = pakFile.GetRecord("levels\\" + linfo.Filename + ".dat").Buffer;

					LevelReader levelReader = new LevelReader(buffer);
					Level level = levelReader.Read();
					levelReader.Dispose();

					if (level == null) {
						MessageBox.Show("Unable to open " + linfo.Name, "Open Level Pack" , MessageBoxButtons.OK, MessageBoxIcon.Error);
						continue;
					}

					mLevels.Add(level);
					level.Info = linfo;
					level.Background = GetBackground(pakFile, linfo.Filename);
				}

				//Load any images
				foreach (PakRecord record in pakFile) {
					if (record.FileName.StartsWith("levels\\"))
						continue;

					if (Path.GetExtension(record.FileName) == ".png") {
						mImages.Add(record.FileName, GetImageFromBuffer(record.Buffer));
					}
				}

				successful = true;
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "Open Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return successful;
		}

		public bool Save(string path)
		{
			bool successful = false;
			try {

				string pakFilename = Path.GetFileNameWithoutExtension(path);

				PakCollection pakFile = new PakCollection();

				//CFG
				PakRecord cfgRecord = new PakRecord(pakFile, pakFilename + ".cfg", DateTime.Now);
				cfgRecord.Buffer = Encoding.UTF8.GetBytes(WriteCFG());
				pakFile.Records.Add(cfgRecord);

				//Levels
				foreach (Level lvl in mLevels) {
					PakRecord datRecord = new PakRecord(pakFile, String.Format("levels\\{0}.dat", lvl.Info.Filename), DateTime.Now);
					datRecord.Buffer = GetLevelData(lvl);

					if (lvl.Background != null) {
						PakRecord bgImageRecord = new PakRecord(pakFile, String.Format("levels\\{0}.png", lvl.Info.Filename), DateTime.Now);
						bgImageRecord.Buffer = GetImageData(lvl.Background);
						pakFile.Records.Add(bgImageRecord);
					}

					PakRecord tbRecord = new PakRecord(pakFile, String.Format("levels\\cached_thumbnails\\{0}.png", lvl.Info.Filename), DateTime.Now);
					tbRecord.Buffer = GetImageData(lvl.GetThumbnail());

					pakFile.Records.Add(datRecord);

					pakFile.Records.Add(tbRecord);
				}

				//Images
				foreach (string s in mImages.Keys) {
					PakRecord iRecord = new PakRecord(pakFile, s, DateTime.Now);
					iRecord.Buffer = GetImageData(mImages[s]);
					pakFile.Records.Add(iRecord);
				}

				pakFile.Save(path);

				successful = true;
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "Save Level Pack", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return successful;
		}

		private byte[] GetLevelData(Level level)
		{
			MemoryStream ms = new MemoryStream();
			LevelWriter writer = new LevelWriter(ms);
			writer.Write(level, LevelWriter.DefaultFileVersion);
			return ms.ToArray();
		}

		private void ParseCFG(string cfg)
		{
			CFGDocument document = CFGDocument.Read(cfg);

			CFGBlock[] stages = document.Blocks[0].GetBlocks("stage");
			foreach (CFGBlock block in stages) {
				foreach (CFGProperty property in block) {
					switch (property.Name.ToLower()) {
						case "name":
							mName = property[0];
							break;
						case "desc":
							mDescription = property[0];
							break;
						case "level":
							LevelInfo levelInfo = new LevelInfo();
							levelInfo.Filename = property[0];
							levelInfo.Name = property[1];
							levelInfo.AceScore = Convert.ToInt32(property[2]);
							levelInfo.MinStage = Convert.ToInt32(property[3]);
							mLevelInfos.Add(levelInfo);
							break;
					}
				}
			}

			CFGBlock[] challenge_pages = document.Blocks[0].GetBlocks("page");
			foreach (CFGBlock block in challenge_pages) {
				mChallengePages.Add(new ChallengePage(block));
			}
		}

		private string WriteCFG()
		{
			CFGDocument document = new CFGDocument();
			CFGBlock docblock = new CFGBlock();

			//Number of stages needed
			int numStages = (int)Math.Ceiling((float)mLevels.Count / 5.0f);
			for (int i = 0; i < numStages; i++) {
				CFGBlock block = new CFGBlock();
				block.Name = "Stage";

				block.Properties.Add(new CFGProperty("Name", mName));
				block.Properties.Add(new CFGProperty("Desc", mDescription));

				for (int j = i * 5; j < Math.Min(mLevels.Count, (i + 1) * 5); j++) {
				    Level lvl = mLevels[j];
				    block.Properties.Add(new CFGProperty("Level", lvl.Info.Filename, lvl.Info.Name, lvl.Info.AceScore.ToString(), lvl.Info.MinStage.ToString()));
				}

				docblock.Blocks.Add(block);
			}

			//Challenges
			foreach (ChallengePage page in mChallengePages) {
				docblock.Blocks.Add(page.GetCFGBlock());
			}

			document.Blocks.Add(docblock);

			CFGWriter writer = new CFGWriter(document);
			return writer.GetText();
		}

		private byte[] GetImageData(Image img)
		{
			MemoryStream ms = new MemoryStream();
			img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
			return ms.ToArray();
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

		private Image GetBackground(PakCollection collection, string levelFilename)
		{
			string filename = "levels\\" + levelFilename;
			string jp2 = Path.ChangeExtension(filename, ".jp2");
			string jpg = Path.ChangeExtension(filename, ".jpg");
			string png = Path.ChangeExtension(filename, ".png");

			PakRecord record;
			record = collection.GetRecord(jp2);
			if (record != null) {
				byte[] buffer;
				OpenJPEG.ConvertJPEG2(record, out buffer, ImageFormat.Jpeg);
				return Image.FromStream(new MemoryStream(buffer));
			} else {
				record = collection.GetRecord(jpg);
				if (record != null)
					return GetImageFromBuffer(record.Buffer);

				record = collection.GetRecord(png);
				if (record != null)
					return GetImageFromBuffer(record.Buffer);
			}

			return null;
		}

		public string Name
		{
			get
			{
				return mName;
			}
			set
			{
				mName = value;
			}
		}

		public string Description
		{
			get
			{
				return mDescription;
			}
			set
			{
				mDescription = value;
			}
		}

		public List<Level> Levels
		{
			get
			{
				return mLevels;
			}
		}

		public List<ChallengePage> ChallengePages
		{
			get
			{
				return mChallengePages;
			}
		}

		public Dictionary<string, Image> Images
		{
			get
			{
				return mImages;
			}
		}

		private static LevelPack mCurrentPack;
		public static LevelPack Current
		{
			get
			{
				return mCurrentPack;
			}
			set
			{
				mCurrentPack = value;
			}
		}
	}
}

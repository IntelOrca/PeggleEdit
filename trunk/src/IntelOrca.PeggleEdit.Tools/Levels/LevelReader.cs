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
using System.IO;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
	/// <summary>
	/// Represents a reader that can read levels.
	/// </summary>
	public class LevelReader : IDisposable
	{
		Stream mBaseStream;
		int mFileVersion;
		int mNumEntries;
		Level mLevel;
		string mError;

		public LevelReader(string path)
			: this(new FileStream(path, FileMode.Open))
		{
		}

		public LevelReader(byte[] buffer)
			: this(new MemoryStream(buffer))
		{
		}

		public LevelReader(Stream s)
		{
			mBaseStream = s;
		}

		public Level Read()
		{
			int entry_index = -1;

			mLevel = new Level();

			try {
				BinaryReader br = new BinaryReader(mBaseStream);

				//Read file version
				mFileVersion = br.ReadInt32();

				//Read byte
				br.ReadByte();

				mNumEntries = br.ReadInt32();

				//Read entries
				for (entry_index = 0; entry_index < mNumEntries; entry_index++) {
					LevelEntry entry = LevelEntryFactory.CreateLevelEntry(br, mFileVersion);
					entry.Level = mLevel;
					mLevel.Entries.Add(entry);
				}

				return mLevel;
			} catch (Exception ex) {
				if (entry_index != -1) {
					mError = String.Format("Entry {0}: {1}", entry_index, ex.Message);
				} else {
					mError = ex.Message;
				}

				return null;
			}
		}

		private void SeekToNextEntry(BinaryReader br)
		{
			for (; ; ) {
				if (br.BaseStream.Position > br.BaseStream.Length - 8)
					return;

				uint int1 = br.ReadUInt32();
				uint int2 = br.ReadUInt32();

				if (int1 == 1 && int2 < 10 && int2 > 0) {
					br.BaseStream.Position -= 8;
					return;
				}

				br.BaseStream.Position -= 7;
			}
		}

		public void Dispose()
		{
			mBaseStream.Dispose();
		}

		public string Error
		{
			get
			{
				return mError;
			}
		}
	}
}
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
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Tools.Pack
{
	/// <summary>
	/// Represents a collection of files to read and save as *.pak files.
	/// </summary>
	public class PakCollection : IEnumerable<PakRecord>
	{
		const byte ENCRYPTION_KEY = 0xF7;
		const uint FILE_HEADER = 0xBAC04AC0;
		const uint FILE_VERSION = 0x0;
		const byte FILEFLAGS_END = 0x80;

		private List<PakRecord> mRecords;
		private string mFilename;

		private ToolStripProgressBar mProgressBar;

		public PakCollection()
		{
			mRecords = new List<PakRecord>();
			mFilename = null;
		}

		public void New()
		{
			mRecords.Clear();
			mFilename = null;
		}

		#region Load and Save

		public bool Open(string filename)
		{
			byte[] buffer;

			//Clear the collection
			New();

			//Prepare the progress bar
			if (mProgressBar != null) {
				mProgressBar.Minimum = 0;
				mProgressBar.Maximum = 100;
				mProgressBar.Value = 0;
				mProgressBar.Visible = true;
			}
			
			//Decrypt the file
			buffer = DecryptFile(filename);

			//Update progress bar
			if (mProgressBar != null) {
				mProgressBar.Value += 33;
			}

			MemoryStream ms = new MemoryStream(buffer);
			BinaryReader br = new BinaryReader(ms);

			//Check file header
			uint magic = br.ReadUInt32();
			if (magic != FILE_HEADER)
				throw new InvalidDataException("Invalid PAK file!");

			//Check version
			uint version = br.ReadUInt32();
			if (version != FILE_VERSION)
				throw new InvalidDataException("This version is not supported!");

			//Address list
			List<int> startPositions = new List<int>();
			List<int> fileSizes = new List<int>();
			int position = 0;

			for (; ; ) {
				byte flags = br.ReadByte();
				if ((flags & FILEFLAGS_END) > 0)
					break;

				//Load file name
				string name = String.Empty;
				byte nameLength = br.ReadByte();
				for (int i = 0; i < nameLength; i++) {
					name += (char)br.ReadByte();
				}

				//Load file size
				int srcSize = br.ReadInt32();

				//Load file time
				DateTime fileTime = DateTime.FromFileTime(br.ReadInt64());

				//Start position of buffer and size
				startPositions.Add(position);
				fileSizes.Add(srcSize);

				//Add record to collection
				mRecords.Add(new PakRecord(this, name, fileTime));

				//Increase position by file size
				position += srcSize;
			}

			//Update progress bar
			if (mProgressBar != null) {
				mProgressBar.Value += 33;
			}

			float progress = 66.0f;
			float progressInc = 33f / mRecords.Count;

			int offset = (int)ms.Position;
			for (int i = 0; i < mRecords.Count; i++) {
				int startPos = startPositions[i] + offset;
				int size = fileSizes[i];
				mRecords[i].Buffer = br.ReadBytes(size);

				progress += progressInc;

				if (mProgressBar != null) {
					mProgressBar.Value = Convert.ToInt32(progress);
				}
			}

			if (mProgressBar != null) {
				mProgressBar.Value = 100;
				mProgressBar.Visible = false;
			}

			mFilename = filename;

			return true;
		}

		public bool Save()
		{
			return Save(mFilename);
		}

		public bool Save(string filename)
		{
			byte[] buffer;

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);

			//Write file header
			bw.Write((uint)FILE_HEADER);

			//Write version
			bw.Write((uint)FILE_VERSION);

			//Address list
			foreach (PakRecord record in mRecords) {
				//Write flags
				bw.Write((byte)0);

				//Write file name
				int nameLength = record.FileName.Length;
				bw.Write((byte)nameLength);
				byte[] name = new byte[nameLength];
				for (int i = 0; i < nameLength; i++) {
					name[i] = (byte)record.FileName[i];
				}
				bw.Write(name);

				//Write file size
				bw.Write(record.Buffer.Length);

				//Write file time
				long ft = record.FileTime.ToFileTime();
				bw.Write(ft);
			}

			//End of files
			bw.Write((byte)FILEFLAGS_END);

			//Write file buffers
			foreach (PakRecord record in mRecords) {
				bw.Write(record.Buffer);
			}

			buffer = ms.ToArray();

			bw.Close();
			ms.Close();

			//Encrypt and save the file
			bool result = EncryptFile(filename, buffer);

			if (result)
				mFilename = filename;

			return result;
		}

		private bool EncryptFile(string filename, byte[] buffer)
		{
			//Encrypt file
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = (byte)(buffer[i] ^ ENCRYPTION_KEY);
			}

			//Save file
			FileStream fs = new FileStream(filename, FileMode.Create);
			fs.Write(buffer, 0, buffer.Length);
			fs.Close();

			return true;
		}

		private byte[] DecryptFile(string filename)
		{
			int length;
			byte[] buffer;

			//Load file
			FileStream fs = new FileStream(filename, FileMode.Open);
			length = (int)fs.Length;

			//Create buffer
			buffer = new byte[length];

			//Read file into buffer
			fs.Read(buffer, 0, length);
			fs.Close();

			//Decrypt file
			for (int i = 0; i < length; i++) {
				buffer[i] = (byte)(buffer[i] ^ ENCRYPTION_KEY);
			}

			return buffer;
		}

		public string Filename
		{
			get
			{
				return mFilename;
			}
			set
			{
				mFilename = value;
			}
		}

		#endregion

		public PakRecord ImportFile(string directory, string path)
		{
			PakRecord record = PakRecord.FromFile(this, path);
			record.FileName = Path.Combine(directory, record.FileName);
			mRecords.Add(record);
			return record;
		}

		public void Export(string directory)
		{
			string path;

			//Prepare progress bar
			if (mProgressBar != null) {
				mProgressBar.Minimum = 0;
				mProgressBar.Maximum = mRecords.Count;
				mProgressBar.Value = 0;
				mProgressBar.Visible = true;
			}

			foreach (PakRecord record in mRecords) {
				record.SaveFileIn(directory, out path);

				if (mProgressBar != null) {
					mProgressBar.Value++;
					Application.DoEvents();
				}
			}

			//Hide progress bar
			if (mProgressBar != null) {
				mProgressBar.Visible = false;
			}
		}

		public PakRecord[] GetRecords(string directory)
		{
			List<PakRecord> records = new List<PakRecord>();

			foreach (PakRecord record in mRecords) {
				if (String.Compare(Path.GetDirectoryName(record.FileName), directory, true) == 0) {
					records.Add(record);
				}
			}
			
			return records.ToArray();
		}

		public PakRecord GetRecord(string path)
		{
			foreach (PakRecord record in mRecords) {
				if (String.Compare(record.FileName, path, true) == 0) {
					return record;
				}
			}

			return null;
		}

		public List<PakRecord> Records
		{
			get
			{
				return mRecords;
			}
			set
			{
				mRecords = value;
			}
		}

		public ToolStripProgressBar ProgressBar
		{
			get
			{
				return mProgressBar;
			}
			set
			{
				mProgressBar = value;
			}
		}

		public IEnumerator<PakRecord> GetEnumerator()
		{
			return mRecords.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mRecords.GetEnumerator();
		}
	}
}

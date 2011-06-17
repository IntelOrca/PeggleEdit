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

namespace IntelOrca.PeggleEdit.Tools.Pack
{
	/// <summary>
	/// Represents a record (file) in a pak collection.
	/// </summary>
	public class PakRecord
	{
		private PakCollection mCollection;
		private string mFileName;
		private DateTime mFileTime;
		private byte[] mBuffer;

		public PakRecord(PakCollection collection)
		{
			mCollection = collection;
			mFileName = String.Empty;
			mFileTime = DateTime.MinValue;
			mBuffer = null;
		}

		public PakRecord(PakCollection collection, string filename, DateTime filetime)
		{
			mCollection = collection;
			mFileName = filename;
			mFileTime = filetime;
			mBuffer = null;
		}

		public static PakRecord FromFile(PakCollection collection, string filename)
		{
			PakRecord record = new PakRecord(collection);
			record.FileName = Path.GetFileName(filename);
			record.FileTime = File.GetLastWriteTime(filename);
			record.Buffer = File.ReadAllBytes(filename);
			return record;
		}

		public bool SaveFileIn(string directory, out string path)
		{
			return SaveFileIn(directory, out path, false);
		}

		public bool SaveFileIn(string directory, out string path, bool temporary)
		{
			
			if (mBuffer == null)
				throw new NullReferenceException("Buffer was null!");

			//Combind the filename and the path
			path = Path.Combine(directory, mFileName);

			//Collect only the path, if it needs creating
			string dir = Path.GetDirectoryName(path);

			try {
				//Create directory if not existant
				if (!Directory.Exists(dir)) {
					Directory.CreateDirectory(dir);
				}

				//Write the file
				File.WriteAllBytes(path, mBuffer);

				//If temporary set the file to temporary and read-only
				if (temporary) {
					File.SetAttributes(path, FileAttributes.Temporary | FileAttributes.ReadOnly);
				}

				//Return the path of the created file
				return true;
			} catch {
				//Error
				return false;
			}
		}

		public void SaveFileAs(string path)
		{
			if (mBuffer == null)
				throw new NullReferenceException("Buffer was null!");
			File.WriteAllBytes(path, mBuffer);
		}

		public override string ToString()
		{
			return mFileName;
		}

		#region Properties

		public PakCollection Collection
		{
			get
			{
				return mCollection;
			}
			set
			{
				mCollection = value;
			}
		}

		public string FileName
		{
			get
			{
				return mFileName;
			}
			set
			{
				mFileName = value;
			}
		}

		public DateTime FileTime
		{
			get
			{
				return mFileTime;
			}
			set
			{
				mFileTime = value;
			}
		}

		public byte[] Buffer
		{
			get
			{
				return mBuffer;
			}
			set
			{
				mBuffer = value;
			}
		}

		public int FileSize
		{
			get
			{
				return mBuffer.Length;
			}
		}

	#endregion
	}
}

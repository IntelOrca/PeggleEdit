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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CSJ2K.Util;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Tools
{
	/// <summary>
	/// Provides static methods for operating with the OpenJPEG library.
	/// </summary>
	public static class OpenJPEG
	{
		public static void ConvertAllFiles(PakCollection collection, ImageFormat format)
		{
			int convertsNeeded = 0;
			foreach (PakRecord record in collection.Records) {
				string ext = Path.GetExtension(record.FileName);
				if (ext == ".j2k" || ext == ".jp2" || ext == ".j2c") {
					convertsNeeded++;
				}
			}

			ToolStripProgressBar pb = collection.ProgressBar;
			if (pb != null) {
				pb.Minimum = 0;
				pb.Maximum = convertsNeeded;
				pb.Value = 0;
				pb.Visible = true;
			}

			foreach (PakRecord record in collection.Records) {
				string ext = Path.GetExtension(record.FileName);
				if (ext == ".j2k" || ext == ".jp2" || ext == ".j2c") {
					byte[] newBuffer;
					ConvertJPEG2(record, out newBuffer, format);

					record.FileName = Path.ChangeExtension(record.FileName, ".png");
					record.Buffer = newBuffer;

					if (pb != null) {
						pb.Value++;
						Application.DoEvents();
					}
				}
			}

			if (pb != null) {
				pb.Visible = false;
			}
		}

		public static bool ConvertJPEG2(string src, string dest, ImageFormat format)
		{
			CallJ2K(src, dest);

			Image img = Image.FromFile(dest);

			img.Save(dest + ".tmp", format);

			img.Dispose();

			File.Delete(dest);
			File.Move(dest + ".tmp", dest);

			return true;
		}

		public static bool ConvertJPEG2(string src, out byte[] dest, ImageFormat format)
		{
			string srcFileName = Path.GetFileName(src);
			string destPath = Path.Combine(TempDirectory, Path.ChangeExtension(srcFileName, ".png"));

			if (ConvertJPEG2(src, destPath, format)) {
				dest = File.ReadAllBytes(destPath);
				File.Delete(destPath);
				return true;
			} else {
				dest = null;
				return false;
			}
		}

		private static bool _registered;
		public static bool ConvertJPEG2(PakRecord record, out byte[] dest, ImageFormat format)
		{
			if (!_registered)
			{
				BitmapImageCreator.Register();
				_registered = true;
			}
			var outputImage = CSJ2K.J2kImage.FromBytes(record.Buffer);
			var bitmapImage = outputImage.As<Bitmap>();

			var ms = new MemoryStream();
			bitmapImage.Save(ms, format);
			dest = ms.ToArray();
			return true;
		}

		private static void CallJ2K(string src, string dest)
		{
			if (!Directory.Exists(TempDirectory))
				Directory.CreateDirectory(TempDirectory);

			if (File.Exists(dest))
				File.Delete(dest);

			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = "j2k_to_image.exe";
			psi.Arguments = String.Format("-i \"{0}\" -o \"{1}\"", src, dest);
			psi.WorkingDirectory = Application.StartupPath;
			psi.WindowStyle = ProcessWindowStyle.Hidden;
			Process process = new Process();
			process.StartInfo = psi;
			process.Start();
			while (!process.HasExited) {
			}
		}

		private static string TempDirectory
		{
			get
			{
				return Path.Combine(Path.GetTempPath(), Application.ProductName);
			}
		}
	}
}

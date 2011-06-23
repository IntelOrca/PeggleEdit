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

using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Win32;
using System;

namespace IntelOrca.PeggleEdit.Designer
{
	static class Settings
	{
		private static string mFilename = "peggleedit.cfg";

		public static string PeggleNightsExePath = @"C:\Program Files\PopCap Games\Peggle Nights\PeggleNights.exe";
		public static bool ShowGrid = false;
		public static bool SnapToGrid = true;
		public static int GridSize = 20;
		public static int SnapThreshold = 5;

		public static bool ShowAnchorsAlways = false;

		public static Size MDIFormSize = new Size(800, 600);
		public static bool MDIMaximized = false;

		public static List<string> RecentPackFiles = new List<string>();

		public static void Load()
		{
			if (!File.Exists(mFilename))
				return;

			FileStream fs = null;
			BinaryReader br = null;

			try {
				fs = new FileStream(mFilename, FileMode.Open);
				br = new BinaryReader(fs);

				ShowGrid = br.ReadBoolean();
				SnapToGrid = br.ReadBoolean();
				GridSize = br.ReadInt32();
				SnapThreshold = br.ReadInt32();

				ShowAnchorsAlways = br.ReadBoolean();

				MDIFormSize = new Size(br.ReadInt32(), br.ReadInt32());
				MDIMaximized = br.ReadBoolean();

				PeggleNightsExePath = br.ReadString();

				int recentPackFiles = br.ReadByte();
				for (int i = 0; i < recentPackFiles; i++)
					RecentPackFiles.Add(br.ReadString());
			} catch {
				MessageBox.Show("There was a problem with the configuration file.");
			} finally {
				if (fs != null)
					fs.Close();

				if (br != null)
					br.Close();
			}
		}

		public static void Save()
		{
			FileStream fs = null;
			BinaryWriter bw = null;

			try {
				fs = new FileStream(mFilename, FileMode.Create);
				bw = new BinaryWriter(fs);

				bw.Write(ShowGrid);
				bw.Write(SnapToGrid);
				bw.Write(GridSize);
				bw.Write(SnapThreshold);

				bw.Write(ShowAnchorsAlways);

				bw.Write(MDIFormSize.Width);
				bw.Write(MDIFormSize.Height);
				bw.Write(MDIMaximized);

				bw.Write(PeggleNightsExePath);

				bw.Write((byte)RecentPackFiles.Count);
				for (int i = 0; i < RecentPackFiles.Count; i++)
					bw.Write(RecentPackFiles[i]);
			} catch {
				MessageBox.Show("There was a problem saving configuration file.");
			} finally {
				if (fs != null)
					fs.Close();

				if (bw != null)
					bw.Close();
			}
		}

		public static void SetupFileAssociation()
		{
			//Setup application
			RegistryKey keyPeggleEdit = Registry.ClassesRoot.CreateSubKey("PeggleEdit");
			RegistryKey keyShell = keyPeggleEdit.CreateSubKey("Shell");
			RegistryKey keyOpen = keyShell.CreateSubKey("Open");
			RegistryKey keyCommand = keyOpen.CreateSubKey("Command");

			keyOpen.SetValue(String.Empty, "Open with PeggleEdit");
			keyCommand.SetValue(String.Empty, String.Format("\"{0}\" \"%1\"", Application.ExecutablePath));

			//Setup file extension
			RegistryKey keyPAK = Registry.ClassesRoot.CreateSubKey(".pak");
			keyPAK.SetValue(String.Empty, "PeggleEdit");
		}

		public static string Filename
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
	}
}

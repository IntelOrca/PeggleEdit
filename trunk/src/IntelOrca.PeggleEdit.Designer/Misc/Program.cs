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
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;

namespace IntelOrca.PeggleEdit.Designer
{
	static class Program
	{
		public const bool AppBetaRelease = false;
		public const string AppTitle = "PeggleEdit";
		public const string AppVersion = "0.4.2 Pre-Release Beta";
		public const string AppVersionName = "Buzzard";
		public const string AppAuthor = "Ted John";
		public const string AppYear = "2010 - 11";
		public const string AppWebsite = "http://tedtycoon.co.uk";

		private static string _tempPath;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			_tempPath = Path.Combine(Path.GetTempPath(), Application.ProductName);

			Settings.Load();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainMDIForm());
			//Application.Run(new MainForm());

			Settings.Save();
		}

		public static Icon AppIcon
		{
			get
			{
				return Resources.orca;
			}
		}

		public static string TempPath
		{
			get
			{
				return _tempPath;
			}
		}
	}
}
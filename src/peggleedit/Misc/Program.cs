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
using System.Reflection;
using System.Windows.Forms;
using IntelOrca.PeggleEdit.Designer.Properties;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Designer
{
    static class Program
    {
        public const bool AppBetaRelease = true;
        public const string AppTitle = "PeggleEdit";
        public static string AppVersion => CurrentVersion.ToString(3);
        public const string AppVersionName = "Eagle";
        public const string AppAuthor = "Ted John";
        public const string AppYear = "2010-2023";
        public const string AppWebsite = "http://intelorca.co.uk/PeggleEdit";

        public static Version CurrentVersion = GetVersion();

        private static string _tempPath;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "unpack")
            {
                if (args.Length > 1)
                {
                    var pakFile = args[1];
                    var targetPath = Path.GetDirectoryName(pakFile);
                    try
                    {
                        var collection = new PakCollection(pakFile);
                        collection.Export(targetPath);
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.Error.WriteLine("usage: peggleedit unpack <pak_file>");
                }
                return 1;
            }

            _tempPath = Path.Combine(Path.GetTempPath(), Application.ProductName);

            Settings.Load();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainMDIForm());

            if (args.Length > 0)
            {
                MainMDIForm.Instance.OpenPack(args[0]);
            }

            Settings.Save();
            return 0;
        }

        private static Version GetVersion()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            return new Version(version.Major, version.Minor, version.Build);
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
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
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Win32;

namespace IntelOrca.PeggleEdit.Designer
{
    public class Settings
    {
        public static Settings Default { get; private set; } = new Settings();

        public string PeggleNightsExePath { get; set; } = @"C:\Program Files\PopCap Games\Peggle Nights\PeggleNights.exe";
        public bool ShowGrid { get; set; }
        public bool SnapToGrid { get; set; } = true;
        public int GridSize { get; set; } = 20;
        public int SnapThreshold { get; set; } = 5;

        public bool ShowAnchorsAlways { get; set; }

        public Size MDIFormSize { get; set; } = new Size(800, 600);
        public bool MDIMaximized { get; set; }

        public List<string> RecentPackFiles { get; set; } = new List<string>();

        public bool HideVersionNotification { get; set; }
        public string CurrentVersion { get; set; }
        public string LatestVersionAvailable { get; set; }

        private static string GetDataPath(string fileName)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configPath = Path.Combine(appDataPath, "peggleedit", fileName);
            return configPath;
        }

        private static string GetConfigPath() => GetDataPath("config.json");
        public static string GetLayoutPath() => GetDataPath("layout.xml");

        public static void Load()
        {
            try
            {
                var configPath = GetConfigPath();
                if (!File.Exists(configPath))
                    return;

                var json = File.ReadAllText(configPath);
                Default = JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });

                if (Default.RecentPackFiles == null)
                    Default.RecentPackFiles = new List<string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Failed to open configuration file.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public static void Save()
        {
            var configPath = GetConfigPath();
            try
            {
                var configDirectory = Path.GetDirectoryName(configPath);
                Directory.CreateDirectory(configDirectory);
                var json = JsonSerializer.Serialize(Default, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Failed to write configuration file.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public static void SetupFileAssociation()
        {
            try
            {
                // Setup application
                var keyPeggleEdit = Registry.ClassesRoot.CreateSubKey("PeggleEdit");
                var keyShell = keyPeggleEdit.CreateSubKey("Shell");
                var keyOpen = keyShell.CreateSubKey("Open");
                var keyCommand = keyOpen.CreateSubKey("Command");
                keyOpen.SetValue(string.Empty, "Open with PeggleEdit");
                keyCommand.SetValue(string.Empty, $"\"{Application.ExecutablePath}\" \"%1\"");

                // Setup file extension
                var keyPak = Registry.ClassesRoot.CreateSubKey(".pak");
                keyPak.SetValue(string.Empty, "PeggleEdit");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Failed to edit registry for file association.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

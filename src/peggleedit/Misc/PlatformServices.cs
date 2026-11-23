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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace IntelOrca.PeggleEdit.Designer
{
    /// <summary>
    /// Operating system services used by the editor.
    /// </summary>
    interface IPlatformServices
    {
        /// <summary>
        /// Adds a file to the operating system's list of recently used documents.
        /// </summary>
        void AddRecentDocument(string path);

        /// <summary>
        /// Shortens a path to fit within the given number of characters by replacing directories with an ellipsis.
        /// </summary>
        string CompactPath(string path, int length);

        /// <summary>
        /// Gets whether the given window accepts input, i.e. it is not disabled by a modal dialog.
        /// </summary>
        bool AcceptsInput(Form form);
    }

    static class PlatformServices
    {
        /// <summary>
        /// Gets the services for the current platform: Win32 on Windows and under Wine, otherwise
        /// a portable implementation (e.g. Mono on Linux or macOS).
        /// </summary>
        public static IPlatformServices Current { get; } = Environment.OSVersion.Platform == PlatformID.Win32NT
            ? (IPlatformServices)new Win32PlatformServices()
            : new PortablePlatformServices();
    }

    /// <summary>
    /// Implements <see cref="IPlatformServices"/> using the Windows API.
    /// </summary>
    sealed class Win32PlatformServices : IPlatformServices
    {
        private const uint SHARD_PATHW = 0x00000003;

        [DllImport("shell32.dll")]
        private static extern void SHAddToRecentDocs(uint uFlags, [MarshalAs(UnmanagedType.LPWStr)] string pv);

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        [DllImport("user32.dll")]
        private static extern bool IsWindowEnabled(IntPtr hWnd);

        public void AddRecentDocument(string path)
        {
            SHAddToRecentDocs(SHARD_PATHW, path);
        }

        public string CompactPath(string path, int length)
        {
            var sb = new StringBuilder(capacity: length);
            PathCompactPathEx(sb, path, length, 0);
            return sb.ToString();
        }

        public bool AcceptsInput(Form form)
        {
            return IsWindowEnabled(form.Handle);
        }
    }

    /// <summary>
    /// Implements <see cref="IPlatformServices"/> without any native APIs.
    /// </summary>
    sealed class PortablePlatformServices : IPlatformServices
    {
        public void AddRecentDocument(string path)
        {
        }

        public string CompactPath(string path, int length)
        {
            // Like PathCompactPathEx, length includes the null terminator
            var maxChars = length - 1;
            if (path.Length <= maxChars)
                return path;

            // Too short for an ellipsis, keep the end of the path
            const string ellipsis = "...";
            if (maxChars <= ellipsis.Length)
                return path.Substring(path.Length - Math.Max(0, maxChars));

            // Keep as much of the directory as fits: <directory>...\<file name>
            var fileName = Path.GetFileName(path);
            var directoryLength = maxChars - ellipsis.Length - 1 - fileName.Length;
            if (directoryLength > 0)
                return path.Substring(0, directoryLength) + ellipsis + Path.DirectorySeparatorChar + fileName;

            // Otherwise keep the end of the file name: ...<file name>
            return ellipsis + fileName.Substring(fileName.Length - Math.Min(fileName.Length, maxChars - ellipsis.Length));
        }

        public bool AcceptsInput(Form form)
        {
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm != form && openForm.Modal)
                    return false;
            }
            return form.Enabled;
        }
    }
}

// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2026. http://tedtycoon.co.uk
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

namespace IntelOrca.PeggleEdit.Tools
{
    /// <summary>
    /// A list of recently opened files, most recent first, with each file in it once however its path was written (e.g.
    /// in another case on Windows, with / for \, or relative).
    /// </summary>
    public static class RecentFiles
    {
        /// <summary>
        /// The most files kept.
        /// </summary>
        public const int MaxCount = 10;

        // Paths on Windows are not case sensitive
        private static readonly StringComparer PathComparer =
            Path.DirectorySeparatorChar == '\\' ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        /// <summary>
        /// Gets the full path of a file (or folder, without a separator at its end), which is what it is listed and
        /// compared by.
        /// </summary>
        private static string GetFullPath(string path)
        {
            try
            {
                path = Path.GetFullPath(path);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException || ex is System.Security.SecurityException)
            {
                return path;
            }
            var root = Path.GetPathRoot(path) ?? "";
            var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return trimmed.Length < root.Length ? root : trimmed;
        }

        /// <summary>
        /// Puts <paramref name="path"/> first, as a full path (as one relative to the current folder would be another
        /// file later), removing any other entry for the same file.
        /// </summary>
        public static void Add(List<string> files, string path)
        {
            files.Insert(0, GetFullPath(path));
            RemoveDuplicates(files);
        }

        /// <summary>
        /// Removes the entries for a file that is earlier in the list, and empty entries, and keeps at most
        /// <see cref="MaxCount"/>.
        /// </summary>
        public static void RemoveDuplicates(List<string> files)
        {
            var seen = new HashSet<string>(PathComparer);
            files.RemoveAll(x => String.IsNullOrWhiteSpace(x) || !seen.Add(GetFullPath(x)));
            if (files.Count > MaxCount)
                files.RemoveRange(MaxCount, files.Count - MaxCount);
        }
    }
}

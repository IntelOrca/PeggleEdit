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

namespace IntelOrca.PeggleEdit.Tools
{
    /// <summary>
    /// The game's images that PeggleEdit draws itself, embedded so that they do not have to be found in the game's
    /// install: those of the free ball bucket and the devices of the powers, the cannon and the lightning rod. They are
    /// kept under the paths the game has them at, e.g. images\levels\catchera.jp2.
    /// </summary>
    public static class GameFiles
    {
        const string Prefix = @"GameFiles\";

        private static readonly Dictionary<string, string> _resourceNames = GetResourceNames();

        private static Dictionary<string, string> GetResourceNames()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in typeof(GameFiles).Assembly.GetManifestResourceNames())
            {
                var path = name.Replace('/', '\\');
                if (path.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                    result[path.Substring(Prefix.Length)] = name;
            }
            return result;
        }

        /// <summary>
        /// Gets an embedded game file by its path in the game (e.g. images\ip\seams.gif), or null if it is not one of them.
        /// </summary>
        public static byte[] Get(string path)
        {
            if (path == null || !_resourceNames.TryGetValue(path.Replace('/', '\\'), out var name))
                return null;
            using (var stream = typeof(GameFiles).Assembly.GetManifestResourceStream(name))
            using (var memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                return memory.ToArray();
            }
        }
    }
}

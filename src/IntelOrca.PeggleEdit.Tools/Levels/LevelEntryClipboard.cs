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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// Converts level entries to and from a self contained binary form so that they can be
    /// transferred between levels or between instances of PeggleEdit via the clipboard.
    /// </summary>
    public static class LevelEntryClipboard
    {
        /// <summary>
        /// The name of the clipboard format used to store level entries.
        /// </summary>
        public const string FormatName = "PeggleEdit.LevelEntries";

        /// <summary>
        /// Serializes the given entries in the level file format. Movement links between the given
        /// entries are preserved, links to movements of entries that are not given are replaced by a
        /// copy of that movement.
        /// </summary>
        public static byte[] Serialize(IEnumerable<LevelEntry> entries)
        {
            var sources = entries.ToArray();
            var level = new Level();
            var clones = new LevelEntry[sources.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                clones[i] = (LevelEntry)sources[i].Clone();
                clones[i].Level = level;
                level.Entries.Add(clones[i]);
            }

            // Map each owned movement to its clone so that links can point to the cloned movements
            var movementMap = new Dictionary<Movement, Movement>();
            for (int i = 0; i < sources.Length; i++)
            {
                var srcLink = sources[i].MovementLink;
                var dstLink = clones[i].MovementLink;
                while (srcLink != null && dstLink != null && srcLink.OwnsMovement && srcLink.Movement != null)
                {
                    movementMap[srcLink.Movement] = dstLink.Movement;
                    srcLink = srcLink.Movement.MovementLink;
                    dstLink = dstLink.Movement?.MovementLink;
                }
            }

            for (int i = 0; i < sources.Length; i++)
            {
                var srcLink = sources[i].MovementLink;
                var dstLink = clones[i].MovementLink;
                while (srcLink != null && dstLink != null && srcLink.Movement != null)
                {
                    if (!srcLink.OwnsMovement)
                    {
                        if (movementMap.TryGetValue(srcLink.Movement, out var linkedMovement))
                        {
                            dstLink.InternalMovement = linkedMovement;
                            break;
                        }

                        // Linked movement is not being copied, so take ownership of a copy of it
                        dstLink.InternalLinkId = 1;
                    }
                    srcLink = srcLink.Movement.MovementLink;
                    dstLink = dstLink.Movement?.MovementLink;
                }
            }

            using (var ms = new MemoryStream())
            {
                new LevelWriter(ms).Write(level, LevelWriter.DefaultFileVersion);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserializes entries previously serialized with <see cref="Serialize"/>.
        /// </summary>
        /// <returns>The entries, or null if the data is invalid.</returns>
        public static LevelEntry[] Deserialize(byte[] data)
        {
            var level = new LevelReader(data).Read();
            return level?.Entries.ToArray();
        }
    }
}

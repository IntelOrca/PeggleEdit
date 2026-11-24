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
        /// Serializes the given entries in the level file format. Mover links between the given
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
            var movementMap = new Dictionary<Mover, Mover>();
            for (int i = 0; i < sources.Length; i++)
            {
                var srcLink = sources[i].MoverLink;
                var dstLink = clones[i].MoverLink;
                while (srcLink != null && dstLink != null && srcLink.OwnsMover && srcLink.Mover != null)
                {
                    movementMap[srcLink.Mover] = dstLink.Mover;
                    srcLink = srcLink.Mover.MoverLink;
                    dstLink = dstLink.Mover?.MoverLink;
                }
            }

            for (int i = 0; i < sources.Length; i++)
            {
                var srcLink = sources[i].MoverLink;
                var dstLink = clones[i].MoverLink;
                while (srcLink != null && dstLink != null && srcLink.Mover != null)
                {
                    if (!srcLink.OwnsMover)
                    {
                        if (movementMap.TryGetValue(srcLink.Mover, out var linkedMovement))
                        {
                            dstLink.InternalMover = linkedMovement;
                            break;
                        }

                        // Linked movement is not being copied, so take ownership of a copy of it
                        dstLink.InternalLinkId = 1;
                    }
                    srcLink = srcLink.Mover.MoverLink;
                    dstLink = dstLink.Mover?.MoverLink;
                }
            }

            var external = MapDestinationLinks(sources, clones, movementMap);

            byte[] data;
            using (var ms = new MemoryStream())
            {
                new LevelWriter(ms).Write(level, LevelWriter.DefaultFileVersion);
                data = ms.ToArray();
            }
            _lastExternalLinks = (data, external);
            return data;
        }

        // The objects that holes in the entries last serialized go to that were not copied with them, by the index of
        // the hole, so that pasting them back into the same level keeps the links
        private static (byte[] Data, List<(int Index, object Target)> Targets) _lastExternalLinks;

        /// <summary>
        /// Points the holes whose destination is another object at the copy of that object, if it is copied too.
        /// Otherwise the hole's link is written as no object (0), and the object is returned with the hole's index.
        /// </summary>
        private static List<(int Index, object Target)> MapDestinationLinks(LevelEntry[] sources, LevelEntry[] clones,
            Dictionary<Mover, Mover> movementMap)
        {
            var copies = new Dictionary<object, object>();
            for (int i = 0; i < sources.Length; i++)
                copies[sources[i]] = clones[i];
            foreach (var pair in movementMap)
                copies[pair.Key] = pair.Value;

            var external = new List<(int Index, object Target)>();
            for (int i = 0; i < clones.Length; i++)
            {
                if (!(clones[i] is Hole hole) || !hole.HasDestinationLink || hole.DestinationLinkTarget == null)
                    continue;
                if (copies.TryGetValue(hole.DestinationLinkTarget, out var copy))
                {
                    hole.DestinationLinkTarget = copy;
                }
                else
                {
                    external.Add((i, hole.DestinationLinkTarget));
                    hole.DestinationLinkTarget = null;
                    hole.DestinationLink = 0;
                }
            }
            return external;
        }

        /// <summary>
        /// Deserializes entries previously serialized with <see cref="Serialize"/>.
        /// </summary>
        /// <returns>The entries, or null if the data is invalid.</returns>
        public static LevelEntry[] Deserialize(byte[] data)
        {
            var level = new LevelReader(data).Read();
            if (level == null)
                return null;

            // A hole whose destination was not copied goes to the same object if it is pasted into the level it was
            // copied from (in this process), and has no destination otherwise
            var entries = level.Entries.ToArray();
            var last = _lastExternalLinks;
            var targets = last.Data != null && last.Data.SequenceEqual(data) ? last.Targets : null;
            foreach (var entry in entries)
            {
                if (entry is Hole hole && hole.HasDestinationLink && hole.DestinationLinkTarget == null && hole.DestinationLink == 0)
                    hole.ClearDestinationLink();
            }
            if (targets != null)
            {
                foreach (var (index, target) in targets)
                {
                    if (index < entries.Length && entries[index] is Hole hole)
                        hole.SetDestinationLink(target);
                }
            }
            return entries;
        }
    }
}

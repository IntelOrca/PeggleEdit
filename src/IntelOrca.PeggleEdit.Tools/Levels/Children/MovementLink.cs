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
using System.ComponentModel;
using System.IO;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// Represents information about the movement path of a level entry and contains methods for calculating and drawing the path.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MovementLink : ICloneable
    {
        private Level mLevel;

        [Browsable(false)]
        public int InternalLinkId { get; set; }

        [Browsable(false)]
        public Movement InternalMovement { get; set; }

        [Browsable(false)]
        public bool OwnsMovement => InternalLinkId == 1;

        public MovementLink(Level level)
        {
            Level = level;
        }

        public void ReadData(BinaryReader br, int version)
        {
            InternalLinkId = br.ReadInt32();
            if (InternalLinkId == 1)
            {
                InternalMovement = new Movement(Level);
                InternalMovement.ReadData(br, version);
            }
        }

        public void WriteData(BinaryWriter bw, int version)
        {
            bw.Write(InternalLinkId);
            if (InternalLinkId == 1)
            {
                InternalMovement.WriteData(bw, version);
            }
        }

        object ICloneable.Clone() => Clone();

        public MovementLink Clone()
        {
            var result = new MovementLink(Level);
            result.InternalLinkId = InternalLinkId;
            result.InternalMovement = InternalMovement.Clone();
            return result;
        }

        [Browsable(false)]
        public Level Level
        {
            get => mLevel;
            set
            {
                mLevel = value;
                if (InternalMovement != null)
                    InternalMovement.Level = value;
            }
        }

        [DisplayName("Link ID")]
        [Description("If this is set, the object will take the movement properties of the object with this MUID.")]
        [DefaultValue(1)]
        public int LinkId
        {
            get
            {
                if (InternalLinkId == 0 || InternalLinkId == 1)
                    return InternalLinkId;
                return mLevel.GetMovementId(InternalMovement);
            }
            set
            {
                if (value == 0)
                {
                    InternalLinkId = value;
                    InternalMovement = null;
                }
                if (value == 1)
                {
                    InternalLinkId = value;
                    InternalMovement = new Movement(mLevel);
                }
                else if (value != InternalLinkId && InternalMovement?.MUID != value)
                {
                    InternalMovement = Level.GetMovementFromId(value);
                    if (InternalMovement != null)
                    {
                        InternalLinkId = value;
                    }
                    else
                    {
                        InternalLinkId = 0;
                    }
                }
            }
        }

        public Movement Movement
        {
            get => InternalMovement;
            set
            {
                if (value == null)
                {
                    LinkId = 0;
                }
                else
                {
                    LinkId = 1;
                    InternalMovement = value;
                }
            }
        }

        public override string ToString()
        {
            if (LinkId == 0)
                return "Link (None)";
            else
                return $"Link ({Movement})";
        }
    }
}

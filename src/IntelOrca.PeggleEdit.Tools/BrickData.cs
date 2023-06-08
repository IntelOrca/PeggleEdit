using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace IntelOrca.PeggleEdit.Tools
{
    internal struct BrickData : IEquatable<BrickData>
    {
        private const int FLAG_TEXTURE_FLIP = 1 << 0;
        private const int FLAG_ORANGE = 1 << 1;
        private const int FLAG_CRUMBLE = 1 << 2;
        private const int FLAG_CURVED = 1 << 3;

        private byte _flags;

        public PointF Location { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Rotation { get; set; }
        public float SectorAngle { get; set; }

        public bool Curved
        {
            get => (_flags & FLAG_CURVED) != 0;
            set => _flags = (byte)(value ? _flags | FLAG_CURVED : _flags & ~FLAG_CURVED);
        }

        public bool CanBeOrange
        {
            get => (_flags & FLAG_ORANGE) != 0;
            set => _flags = (byte)(value ? _flags | FLAG_ORANGE : _flags & ~FLAG_ORANGE);
        }

        public bool QuickDisappear
        {
            get => (_flags & FLAG_CRUMBLE) != 0;
            set => _flags = (byte)(value ? _flags | FLAG_CRUMBLE : _flags & ~FLAG_CRUMBLE);
        }

        public bool TextureFlip
        {
            get => (_flags & FLAG_TEXTURE_FLIP) != 0;
            set => _flags = (byte)(value ? _flags | FLAG_TEXTURE_FLIP : _flags & ~FLAG_TEXTURE_FLIP);
        }

        public override bool Equals(object obj)
        {
            return obj is BrickData data && Equals(data);
        }

        public bool Equals(BrickData other)
        {
            return _flags == other._flags &&
                   Location == other.Location &&
                   Length == other.Length &&
                   Width == other.Width &&
                   Rotation == other.Rotation &&
                   SectorAngle == other.SectorAngle;
        }

        public override int GetHashCode()
        {
            int hashCode = -282440784;
            hashCode = hashCode * -1521134295 + _flags.GetHashCode();
            hashCode = hashCode * -1521134295 + Location.GetHashCode();
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Rotation.GetHashCode();
            hashCode = hashCode * -1521134295 + SectorAngle.GetHashCode();
            return hashCode;
        }
    }
}

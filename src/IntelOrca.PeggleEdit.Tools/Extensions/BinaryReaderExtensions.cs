using System.IO;

namespace IntelOrca.PeggleEdit.Tools.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public static int ReadInt24(this BinaryReader br)
        {
            var ab = br.ReadUInt16();
            var c = br.ReadByte();
            var result = ab | (c << 16);
            if ((c & 0x80) != 0)
                return (int)((0xFF << 24) | result);
            return result;
        }

        public static int ReadUInt24(this BinaryReader br)
        {
            var ab = br.ReadUInt16();
            var c = br.ReadByte();
            return ab | (c << 16);
        }
    }
}

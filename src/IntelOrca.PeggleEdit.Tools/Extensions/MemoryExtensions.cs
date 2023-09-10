using System;

namespace IntelOrca.PeggleEdit.Tools.Extensions
{
    public static class MemoryExtensions
    {
        public static ulong CalculateFnv1a(this byte[] data) => CalculateFnv1a(new ReadOnlySpan<byte>(data));
        public static ulong CalculateFnv1a(this ReadOnlyMemory<byte> data) => CalculateFnv1a(data.Span);
        public static ulong CalculateFnv1a(this ReadOnlySpan<byte> data)
        {
            var hash = 0x0CBF29CE484222325UL;
            for (int i = 0; i < data.Length; i++)
            {
                hash ^= data[i];
                hash *= 0x100000001B3UL;
            }
            return hash;
        }
    }
}

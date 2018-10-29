namespace com.github.neoresearch.NeoDataStructure
{
    using System.Collections.Generic;
    using System.Linq;
    using System;

    public static class TrieTools
    {
        public static byte[] CompactEncode(this byte[] hexarray)
        {
            var term = hexarray[hexarray.Length - 1] == 16 ? 1 : 0;
            if (term != 0)
            {
                Array.Resize(ref hexarray, hexarray.Length - 1);
            }

            var oddlen = hexarray.Length % 2;
            var flags = 2 * term + oddlen;
            {
                var incValue = oddlen != 0 ? 1 : 2;
                var hexarray2 = new byte[hexarray.Length + incValue];
                hexarray2[0] = (byte) flags;
                if (oddlen == 0)
                {
                    hexarray2[1] = 0x00;
                }

                Array.Copy(hexarray, 0, hexarray2, incValue, hexarray.Length);
                hexarray = hexarray2;
            }

            var o = new List<byte>();
            for (var i = 0; i < hexarray.Length; i += 2)
            {
                o.Add((byte) (16 * hexarray[i] + hexarray[i + 1]));
            }

            return o.ToArray();
        }

        public static string ByteToHexString(this byte hexchar) => hexchar.ToString("x2");

        public static string ByteToHexString(this byte[] hexchar) =>
            string.Join(" ", hexchar.Select(x => x.ByteToHexString()).ToList());
    }
}
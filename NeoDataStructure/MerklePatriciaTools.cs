namespace com.github.neoresearch.NeoDataStructure
{
    using System.Security.Cryptography;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System;

    /// <summary>
    /// Merkle Patricia tools.
    /// </summary>
    public static class MerklePatriciaTools
    {
        private static byte[] ProcessByteArray(byte[] hexarray)
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
            return hexarray;
        }

        /// <summary>
        /// Compact encode implementation for byte array.
        /// </summary>
        /// <param name="hexarray">The byte array to be encoded.</param>
        /// <returns>Encoded byte array.</returns>
        public static byte[] CompactEncode(this byte[] hexarray)
        {
            hexarray = ProcessByteArray(hexarray);
            var o = new List<byte>();
            for (var i = 0; i < hexarray.Length; i += 2)
            {
                o.Append((byte) (16 * hexarray[i] + hexarray[i + 1]));
            }

            return o.ToArray();
        }

        /// <summary>
        /// Compact encode implementation from string to byte array.
        /// </summary>
        /// <param name="hexarray">The string to be encoded.</param>
        /// <returns>The encoded byte array.</returns>
        public static byte[] CompactEncode(this string hexarray) => CompactEncode(Encoding.UTF8.GetBytes(hexarray));

        /// <summary>
        /// Compact encode implementation from byte array to string.
        /// </summary>
        /// <param name="hexarray">The byte array to be encoded.</param>
        /// <returns>The string encoded.</returns>
        public static string CompactEncodeString(this byte[] hexarray)
        {
            hexarray = ProcessByteArray(hexarray);
            var o = new StringBuilder();
            for (var i = 0; i < hexarray.Length; i += 2)
            {
                o.Append((byte) (16 * hexarray[i] + hexarray[i + 1]));
            }

            return o.ToString();
        }

        /// <summary>
        /// Compatc encode implementation for a string.
        /// </summary>
        /// <param name="hexarray">String to be encoded.</param>
        /// <returns>The ecoded string.</returns>
        public static string CompactEncodeString(this string hexarray) =>
            CompactEncodeString(Encoding.UTF8.GetBytes(hexarray));

        /// <summary>
        /// Converts a byte to an hexadecimal string.
        /// </summary>
        /// <param name="hexchar">Byte to be converted.</param>
        /// <returns>The converted string.</returns>
        public static string ByteToHexString(this byte hexchar) => hexchar.ToString("x2");

        /// <summary>
        /// Converts a byte array to an hexadecimal string.
        /// </summary>
        /// <param name="hexchar">Byte array to be converted.</param>
        /// <returns>The converted string.</returns>
        public static string ByteToHexString(this byte[] hexchar) =>
            string.Join(" ", hexchar.Select(x => x.ByteToHexString()).ToList());

        /// <summary>
        /// Converts a string to SHA256.
        /// </summary>
        /// <param name="randomString">String to be converted.</param>
        /// <param name="uppercase">Use uppercase charecters.</param>
        /// <returns>The converted string.</returns>
        public static string Sha256(this string randomString, bool uppercase = false)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            foreach (var theByte in crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString)))
            {
                hash.Append(theByte.ToString(uppercase ? "X2" : "x2"));
            }

            return hash.ToString();
        }
    }
}
namespace com.github.neoresearch.NeoDataStructure
{
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// String help functions.
    /// </summary>
    public static class StringTools
    {
        /// <summary>
        /// Checks if the string is empty or null.
        /// </summary>
        /// <param name="text">The string to be tested.</param>
        /// <returns>true in the case it is empty or null.</returns>
        public static bool IsEmpty(this string text)
        {
            return text == null || text.Trim().Length == 0;
        }

        /// <summary>
        /// Checks if two strings are equal, null safe.
        /// </summary>
        /// <param name="text">First string.</param>
        /// <param name="textB">Second string.</param>
        /// <param name="nullToNullEquals">Indicate if two null are considered equal.</param>
        /// <returns>true or false according to the result.</returns>
        public static bool IsEquals(this string text, string textB, bool nullToNullEquals = false)
        {
            if (text == null)
            {
                return textB == null && nullToNullEquals;
            }

            return textB != null && text.Equals(textB);
        }

        /// <summary>
        /// Calculates the SHA256 hash of the string array.
        /// </summary>
        /// <param name="node">Array of string.</param>
        /// <returns>The generated hash.</returns>
        public static string Hash(this string[] node)
        {
            var linha = new StringBuilder();
            for (var i = 0; i < node.Length; i++)
            {
                linha.Append(i).Append(',').Append(node[i] ?? string.Empty);
            }

            return linha.ToString().Sha256();
        }

        /// <summary>
        /// Converts from hex to integer.
        /// </summary>
        /// <param name="text">Hex string to be converted.</param>
        /// <returns>The integer value.</returns>
        public static int FromHex(this string text) => int.Parse(text, System.Globalization.NumberStyles.HexNumber);

        /// <summary>
        /// Generates a random string.
        /// </summary>
        /// <param name="length">The required size.</param>
        /// <returns>The generated string</returns>
        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
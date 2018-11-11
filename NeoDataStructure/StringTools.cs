namespace com.github.neoresearch.NeoDataStructure
{
    using System.Text;

    public static class StringTools
    {
        public static bool IsEmpty(this string text)
        {
            return text == null || text.Trim().Length == 0;
        }

        public static bool IsEquals(this string text, string textB, bool nullToNullEquals = false)
        {
            if (text == null)
            {
                return textB == null && nullToNullEquals;
            }

            return textB != null && text.Equals(textB);
        }

        public static string Hash(this string[] node)
        {
            var linha = new StringBuilder();
            for (var i = 0; i < node.Length; i++)
            {
                linha.Append(i).Append(',').Append(node[i] ?? string.Empty);
            }

            return linha.ToString().Sha256();
        }

        public static int FromHex(this string text) => int.Parse(text, System.Globalization.NumberStyles.HexNumber);
    }
}
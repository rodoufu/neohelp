namespace com.github.neoresearch.NeoDataStructure
{
    public static class StringTools
    {
        public static bool IsEmpty(this string text)
        {
            return text == null || text.Trim().Length == 0;
        }
    }
}
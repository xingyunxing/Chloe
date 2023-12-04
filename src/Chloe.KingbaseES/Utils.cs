namespace Chloe.KingbaseES
{
    internal static class Utils
    {
        public static string QuoteName(string name, bool convertToLowercase)
        {
            if (convertToLowercase)
                return string.Concat(UtilConstants.LeftQuoteChar, name.ToLower(), UtilConstants.RightQuoteChar);

            return string.Concat(UtilConstants.LeftQuoteChar, name, UtilConstants.RightQuoteChar);
        }
    }
}

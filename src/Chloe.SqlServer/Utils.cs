namespace Chloe.SqlServer
{
    internal static class Utils
    {
        public static string QuoteName(string name)
        {
            return string.Concat(UtilConstants.LeftQuoteChar, name, UtilConstants.RightQuoteChar);
        }
    }
}

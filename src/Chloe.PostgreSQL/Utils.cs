using Chloe.Reflection;

namespace Chloe.PostgreSQL
{
    internal static class Utils
    {
        public static string QuoteName(string name, bool convertToLowercase)
        {
            if (convertToLowercase)
                return string.Concat("\"", name.ToLower(), "\"");

            return string.Concat("\"", name, "\"");
        }
    }
}

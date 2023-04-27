using Chloe.Reflection;

namespace Chloe.SqlServer.Odbc
{
    internal static class Utils
    {
        public static string QuoteName(string name)
        {
            return string.Concat("[", name, "]");
        }
    }
}

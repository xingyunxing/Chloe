using Chloe.Annotations;

namespace Chloe.MySql
{
    public static class MySqlFunctions
    {
        [DbFunctionAttribute("FIND_IN_SET")]
        public static bool FindInSet(string str, string strList)
        {
            throw new NotSupportedException();
        }
    }
}

using Chloe.DbExpressions;
using System.Reflection;

namespace Chloe.Oracle
{
    public static class OracleSemantics
    {
        internal static readonly PropertyInfo PropertyInfo_ROWNUM = typeof(OracleSemantics).GetProperty(nameof(OracleSemantics.ROWNUM));
        internal static readonly DbMemberAccessExpression DbMemberExpression_ROWNUM = DbExpression.MemberAccess(OracleSemantics.PropertyInfo_ROWNUM, null);

        public static decimal ROWNUM
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

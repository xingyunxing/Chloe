using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;
using System.Reflection;

namespace Chloe.Oracle.PropertyHandlers
{
    class ROWNUM_Handler : PropertyHandlerBase
    {
        public override MemberInfo GetCanProcessProperty()
        {
            return OracleSemantics.PropertyInfo_ROWNUM;
        }

        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("ROWNUM");
        }
    }
}

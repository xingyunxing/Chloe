using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;
using System.Collections;
using System.Reflection;

namespace Chloe.MySql.MethodHandlers
{
    class Contains_Handler : Contains_HandlerBase
    {
        protected override void Method_String_Contains(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Object.Accept(generator);

            generator.SqlBuilder.Append(" LIKE ");
            generator.SqlBuilder.Append("CONCAT(");
            generator.SqlBuilder.Append("'%',");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(",'%'");
            generator.SqlBuilder.Append(")");
        }
    }
}

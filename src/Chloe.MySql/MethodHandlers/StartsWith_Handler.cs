using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.MySql.MethodHandlers
{
    class StartsWith_Handler : StartsWith_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" LIKE ");
            generator.SqlBuilder.Append("CONCAT(");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(",'%'");
            generator.SqlBuilder.Append(")");
        }
    }
}

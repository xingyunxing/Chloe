using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.Dameng.PropertyHandlers
{
    class Date_Handler : Date_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("TRUNC(");
            exp.Expression.Accept(generator);
            generator.SqlBuilder.Append(",'DD')");
        }
    }
}

using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.SQLite.MethodHandlers
{
    class Replace_Handler : Replace_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("REPLACE(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(",");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(",");
            exp.Arguments[1].Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}

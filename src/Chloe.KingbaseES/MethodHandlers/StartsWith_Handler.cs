using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.KingbaseES.MethodHandlers
{
    class StartsWith_Handler : StartsWith_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" like ");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(" || '%'");
        }
    }
}

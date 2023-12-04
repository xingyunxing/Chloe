using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.KingbaseES.MethodHandlers
{
    class AddMilliseconds_Handler : AddMilliseconds_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            //(NOW() +  MAKE_INTERVAL(secs:= (3/1000)))
            generator.SqlBuilder.Append("(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" + ");
            generator.SqlBuilder.Append("MAKE_INTERVAL");
            generator.SqlBuilder.Append("(secs:=(");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(" / 1000)))");
        }
    }
}

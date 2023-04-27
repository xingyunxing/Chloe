using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.SQLite.MethodHandlers
{
    class DiffYears_Handler : DiffYears_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.Append_DiffYears(generator, exp.Arguments[0], exp.Arguments[1]);
        }
    }
}

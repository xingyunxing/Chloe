using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.SQLite.PropertyHandlers
{
    class Date_Handler : Date_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("DATETIME(DATE(");
            exp.Expression.Accept(generator);
            generator.SqlBuilder.Append("))");
        }
    }
}

using Chloe.DbExpressions;
using Chloe.RDBMS;
using System.Linq;

namespace ChloeDemo
{
    class PostgreSQL_StringLike_MethodHandler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != typeof(DbFunctions))
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(" LIKE '%' || ");
            exp.Arguments[1].Accept(generator);
            generator.SqlBuilder.Append(" || '%'");
        }
    }
}

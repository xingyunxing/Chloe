using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Chloe.PostgreSQL.MethodHandlers
{
    class LongCount_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            //Sql.LongCount<TField>(TField field)
            if (exp.Arguments.Count == 1)
            {
                SqlGenerator.Aggregate_LongCount(generator, exp.Arguments[0]);
                return;
            }

            SqlGenerator.Aggregate_LongCount(generator);
        }
    }
}

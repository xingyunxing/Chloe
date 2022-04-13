using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Chloe.SqlServer.MethodHandlers
{
    class Count_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            //Sql.Count<TField>(TField field)
            if (exp.Arguments.Count == 1)
            {
                SqlGenerator.Aggregate_Count(generator, exp.Arguments[0]);
                return;
            }

            SqlGenerator.Aggregate_Count(generator);
        }
    }
}

using Chloe.DbExpressions;

namespace Chloe.RDBMS
{
    public interface IMethodHandler
    {
        bool CanProcess(DbMethodCallExpression exp);
        void Process(DbMethodCallExpression exp, SqlGeneratorBase generator);
    }
}

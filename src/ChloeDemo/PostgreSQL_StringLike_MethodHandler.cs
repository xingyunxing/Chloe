using Chloe.DbExpressions;
using Chloe.RDBMS;
using System.Linq;

namespace ChloeDemo
{
    public class PostgreSQL_StringLike_MethodHandler : IMethodHandler
    {
        /// <summary>
        /// 判断是否可以解析传入的方法。
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != typeof(DbFunctions))
                return false;

            return true;
        }

        /// <summary>
        /// 解析传入的方法。
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="generator"></param>
        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(" LIKE '%' || ");
            exp.Arguments[1].Accept(generator);
            generator.SqlBuilder.Append(" || '%'");
        }
    }
}

using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using Chloe.RDBMS;
using System.Reflection;

namespace Chloe.SqlServer.Odbc.MethodHandlers
{
    class IsEqual_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            MethodInfo method = exp.Method;
            return PublicHelper.Is_Sql_IsEqual_Method(method);
        }
        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            Method_Sql_Equals(exp, generator);
        }

        static void Method_Sql_Equals(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            DbExpression left = exp.Arguments[0];
            DbExpression right = exp.Arguments[1];

            left = DbExpressionExtension.StripInvalidConvert(left);
            right = DbExpressionExtension.StripInvalidConvert(right);

            //明确 left right 其中一边一定为 null
            if (DbExpressionExtension.AffirmExpressionRetValueIsNull(right))
            {
                left.Accept(generator);
                generator.SqlBuilder.Append(" IS NULL");
                return;
            }

            if (DbExpressionExtension.AffirmExpressionRetValueIsNull(left))
            {
                right.Accept(generator);
                generator.SqlBuilder.Append(" IS NULL");
                return;
            }

            SqlGenerator.AmendDbInfo(left, right);

            left.Accept(generator);
            generator.SqlBuilder.Append(" = ");
            right.Accept(generator);
        }
    }
}

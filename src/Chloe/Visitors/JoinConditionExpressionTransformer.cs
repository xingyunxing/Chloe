using Chloe.DbExpressions;
using System.Reflection;

namespace Chloe.Visitors
{
    public class JoinConditionExpressionTransformer : DbExpressionVisitor
    {
        static readonly JoinConditionExpressionTransformer _joinConditionExpressionParser = new JoinConditionExpressionTransformer();

        public static DbExpression Transform(DbExpression exp)
        {
            return exp.Accept(_joinConditionExpressionParser);
        }
        public override DbExpression Visit(DbEqualExpression exp)
        {
            /*
             * join 的条件不考虑 null 问题
             */

            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            MethodInfo method_Sql_IsEqual = PublicConstants.MethodInfo_Sql_IsEqual.MakeGenericMethod(left.Type);

            /* Sql.IsEqual(left, right) */
            DbMethodCallExpression left_equals_right = DbExpression.MethodCall(null, method_Sql_IsEqual, new List<DbExpression>(2) { left.Accept(this), right.Accept(this) });

            return left_equals_right;
        }
        public override DbExpression Visit(DbNotEqualExpression exp)
        {
            /*
             * join 的条件不考虑 null 问题
             */

            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            MethodInfo method_Sql_IsNotEqual = PublicConstants.MethodInfo_Sql_IsNotEqual.MakeGenericMethod(left.Type);

            /* Sql.IsNotEqual(left, right) */
            DbMethodCallExpression left_not_equals_right = DbExpression.MethodCall(null, method_Sql_IsNotEqual, new List<DbExpression>(2) { left.Accept(this), right.Accept(this) });

            return left_not_equals_right;
        }
    }
}

using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using System.Reflection;

namespace Chloe.RDBMS.MethodHandlers
{
    public class Compare_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            return exp.Method.DeclaringType == PublicConstants.TypeOfSql;
        }
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            DbExpression left = exp.Arguments[0];
            DbExpression right = exp.Arguments[2];

            CompareType compareType = (CompareType)exp.Arguments[1].Evaluate();

            DbExpression newExp = null;
            switch (compareType)
            {
                case CompareType.eq:
                    {
                        MethodInfo method_Sql_IsEqual = PublicConstants.MethodInfo_Sql_IsEqual.MakeGenericMethod(left.Type);

                        /* Sql.IsEqual(left, right) */
                        DbMethodCallExpression left_equals_right = DbExpression.MethodCall(null, method_Sql_IsEqual, new List<DbExpression>(2) { left, right });

                        newExp = left_equals_right;
                    }
                    break;
                case CompareType.neq:
                    {
                        MethodInfo method_Sql_IsNotEqual = PublicConstants.MethodInfo_Sql_IsNotEqual.MakeGenericMethod(left.Type);

                        /* Sql.IsNotEqual(left, right) */
                        DbMethodCallExpression left_not_equals_right = DbExpression.MethodCall(null, method_Sql_IsNotEqual, new List<DbExpression>(2) { left, right });

                        newExp = left_not_equals_right;
                    }
                    break;
                case CompareType.gt:
                    {
                        newExp = new DbGreaterThanExpression(left, right);
                    }
                    break;
                case CompareType.gte:
                    {
                        newExp = new DbGreaterThanOrEqualExpression(left, right);
                    }
                    break;
                case CompareType.lt:
                    {
                        newExp = new DbLessThanExpression(left, right);
                    }
                    break;
                case CompareType.lte:
                    {
                        newExp = new DbLessThanOrEqualExpression(left, right);
                    }
                    break;
                default:
                    throw new NotSupportedException("CompareType: " + compareType.ToString());
            }

            newExp.Accept(generator);
        }
    }
}

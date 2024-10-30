using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    /// <summary>
    /// 判断表达式树中是否存在 lambda 的变量包装
    /// </summary>
    public class DoesLambdaVariableWrapperExistJudgment : ExpressionTraversal
    {
        bool _exists;

        public static bool Exists(QueryExpression queryExpression)
        {
            DoesLambdaVariableWrapperExistJudgment judgment = new DoesLambdaVariableWrapperExistJudgment();
            judgment.Visit(queryExpression);
            return judgment._exists;
        }

        public static bool Exists(Expression expression)
        {
            DoesLambdaVariableWrapperExistJudgment judgment = new DoesLambdaVariableWrapperExistJudgment();
            judgment.Visit(expression);
            return judgment._exists;
        }

        public override void Visit(Expression exp)
        {
            if (this._exists)
                return;

            if (exp == null)
                return;

            base.Visit(exp);
        }

        public override void Visit(QueryExpression exp)
        {
            if (this._exists)
                return;

            base.Visit(exp);
        }

        protected override void VisitMemberAccess(MemberExpression exp)
        {
            if (exp.Expression != null && Utils.IsVariableWrapperType(exp.Expression.Type))
            {
                if (typeof(LambdaExpression).IsAssignableFrom(exp.Type))
                {
                    this._exists = true;
                    return;
                }
            }

            base.Visit(exp.Expression);
        }

    }
}

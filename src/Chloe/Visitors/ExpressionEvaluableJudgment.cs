using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    public class ExpressionEvaluableJudgment : ExpressionTraversal
    {
        bool _isEvaluable = true;

        public static bool IsEvaluable(Expression exp)
        {
            ExpressionEvaluableJudgment judgment = new ExpressionEvaluableJudgment();
            judgment.Visit(exp);
            return judgment._isEvaluable;
        }

        public override void Visit(Expression exp)
        {
            if (!this._isEvaluable)
                return;

            if (exp == null)
                return;

            base.Visit(exp);
        }

        public override void Visit(QueryExpression exp)
        {
            this._isEvaluable = false;
        }

        protected override void VisitParameter(ParameterExpression exp)
        {
            this._isEvaluable = false;
        }
    }
}

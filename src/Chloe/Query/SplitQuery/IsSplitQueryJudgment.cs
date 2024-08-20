using Chloe.QueryExpressions;
using Chloe.Visitors;
using System.Linq.Expressions;

namespace Chloe.Query
{
    public class IsSplitQueryJudgment : ExpressionTraversal
    {
        bool _splitQuerySupported = true;
        bool _existSplitQueryCall = false;

        public static bool IsSplitQuery(QueryExpression exp)
        {
            IsSplitQueryJudgment judgment = new IsSplitQueryJudgment();
            judgment.Visit(exp);

            return judgment._existSplitQueryCall && judgment._splitQuerySupported;
        }


        public override void Visit(Expression exp)
        {
            return;
        }

        public override void Visit(QueryExpression exp)
        {
            if (!this._splitQuerySupported)
                return;

            base.Visit(exp);
        }


        protected override void VisitAggregateQuery(AggregateQueryExpression exp)
        {
            this._splitQuerySupported = false;
        }

        protected override void VisitDistinct(DistinctExpression exp)
        {
            this._splitQuerySupported = false;
        }

        protected override void VisitGroupingQuery(GroupingQueryExpression exp)
        {
            this._splitQuerySupported = false;
        }

        protected override void VisitJoinQuery(JoinQueryExpression exp)
        {
            this._splitQuerySupported = false;
        }

        protected override void VisitSelect(SelectExpression exp)
        {
            this._splitQuerySupported = false;
        }

        protected override void VisitSplitQuery(SplitQueryExpression exp)
        {
            this._existSplitQueryCall = true;
        }
    }
}

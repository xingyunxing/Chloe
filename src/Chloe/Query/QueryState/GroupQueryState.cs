using Chloe.QueryExpressions;

namespace Chloe.Query.QueryState
{
    class GroupQueryState : QueryStateBase
    {
        public GroupQueryState(QueryContext context, QueryModel queryModel) : base(context, queryModel)
        {
        }


        public override IQueryState Accept(WhereExpression exp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(AggregateQueryExpression exp)
        {
            if (this.QueryModel.Orderings.Count > 0)
            {
                QueryModel queryModel = this.QueryModel.Clone(false);
                GroupQueryState groupQueryState = new GroupQueryState(this.Context, queryModel);
                return groupQueryState.Accept(exp);
            }

            GeneralQueryState state = this.AsSubQueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(GroupingQueryExpression exp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.Accept(exp);
        }
    }
}

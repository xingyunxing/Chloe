namespace Chloe.Query.QueryState
{
    class AggregateQueryState : QueryStateBase, IQueryState
    {
        public AggregateQueryState(QueryContext context, QueryModel queryModel) : base(context, queryModel)
        {
        }
    }
}

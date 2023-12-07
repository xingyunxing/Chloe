
namespace Chloe.Query.QueryState
{
    class GeneralQueryState : QueryStateBase, IQueryState
    {
        public GeneralQueryState(QueryContext context, QueryModel queryModel) : base(context, queryModel)
        {
        }

        public override QueryModel ToFromQueryModel()
        {
            QueryModel newQueryModel = new QueryModel(this.QueryModel.Options, this.QueryModel.ScopeParameters, this.QueryModel.ScopeTables);
            newQueryModel.FromTable = this.QueryModel.FromTable;
            newQueryModel.ResultModel = this.QueryModel.ResultModel;
            newQueryModel.Condition = this.QueryModel.Condition;
            if (!this.QueryModel.Options.IgnoreFilters)
            {
                newQueryModel.GlobalFilters.AppendRange(this.QueryModel.GlobalFilters);
                newQueryModel.ContextFilters.AppendRange(this.QueryModel.ContextFilters);
            }

            return newQueryModel;
        }

    }
}

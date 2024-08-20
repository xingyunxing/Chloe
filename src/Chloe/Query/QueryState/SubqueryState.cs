using Chloe.QueryExpressions;

namespace Chloe.Query.QueryState
{
    internal abstract class SubqueryState : QueryStateBase
    {
        protected SubqueryState(QueryContext context, QueryModel queryModel) : base(context, queryModel)
        {
        }

        public override IQueryState Accept(WhereExpression exp)
        {
            IQueryState state = this.AsSubqueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(OrderExpression exp)
        {
            IQueryState state = this.AsSubqueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(SkipExpression exp)
        {
            GeneralQueryState subqueryState = this.AsSubqueryState();
            SkipQueryState state = new SkipQueryState(this.QueryContext, subqueryState.QueryModel, exp.Count);
            return state;
        }
        public override IQueryState Accept(TakeExpression exp)
        {
            GeneralQueryState subqueryState = this.AsSubqueryState();
            TakeQueryState state = new TakeQueryState(this.QueryContext, subqueryState.QueryModel, exp.Count);
            return state;
        }
        public override IQueryState Accept(AggregateQueryExpression exp)
        {
            IQueryState state = this.AsSubqueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(GroupingQueryExpression exp)
        {
            IQueryState state = this.AsSubqueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(DistinctExpression exp)
        {
            IQueryState state = this.AsSubqueryState();
            return state.Accept(exp);
        }

        public override MappingData GenerateMappingData()
        {
            ComplexObjectModel complexObjectModel = this.QueryModel.ResultModel as ComplexObjectModel;

            if (complexObjectModel == null)
                return base.GenerateMappingData();

            if (complexObjectModel.HasMany())
            {
                return base.AsSubqueryState().GenerateMappingData();
            }

            return base.GenerateMappingData();
        }
    }
}

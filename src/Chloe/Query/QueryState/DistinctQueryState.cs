using Chloe.DbExpressions;
using Chloe.QueryExpressions;

namespace Chloe.Query.QueryState
{
    class DistinctQueryState : SubQueryState
    {
        public DistinctQueryState(QueryContext context, QueryModel queryModel) : base(context, queryModel)
        {
        }

        public override IQueryState Accept(SelectExpression exp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.Accept(exp);
        }

        public override DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = base.CreateSqlQuery();
            sqlQuery.IsDistinct = true;

            return sqlQuery;
        }
    }
}

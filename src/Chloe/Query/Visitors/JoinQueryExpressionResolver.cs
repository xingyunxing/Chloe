using Chloe.DbExpressions;
using Chloe.QueryExpressions;
using Chloe.Query.QueryState;
using System.Linq.Expressions;

namespace Chloe.Query.Visitors
{
    class JoinQueryExpressionResolver : QueryExpressionVisitor<JoinQueryResult>
    {
        QueryContext _queryContext;
        QueryModel _queryModel;
        JoinType _joinType;

        LambdaExpression _conditionExpression;
        ScopeParameterDictionary _scopeParameters;

        JoinQueryExpressionResolver(QueryContext queryContext, QueryModel queryModel, JoinType joinType, LambdaExpression conditionExpression, ScopeParameterDictionary scopeParameters)
        {
            this._queryContext = queryContext;
            this._queryModel = queryModel;
            this._joinType = joinType;
            this._conditionExpression = conditionExpression;
            this._scopeParameters = scopeParameters;
        }

        public static JoinQueryResult Resolve(QueryContext queryContext, JoinQueryInfo joinQueryInfo, QueryModel queryModel, ScopeParameterDictionary scopeParameters)
        {
            JoinQueryExpressionResolver resolver = new JoinQueryExpressionResolver(queryContext, queryModel, joinQueryInfo.JoinType, joinQueryInfo.Condition, scopeParameters);
            return joinQueryInfo.Query.QueryExpression.Accept(resolver);
        }

        public override JoinQueryResult VisitRootQuery(RootQueryExpression exp)
        {
            QueryStateBase queryState = new RootQueryState(this._queryContext, exp, this._scopeParameters, this._queryModel.ScopeTables, a => { return this._queryModel.GenerateUniqueTableAlias(a); });
            JoinQueryResult result = queryState.ToJoinQueryResult(this._joinType, this._conditionExpression, this._scopeParameters, this._queryModel.ScopeTables, null);
            return result;
        }
        public override JoinQueryResult VisitWhere(WhereExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult VisitOrder(OrderExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult VisitSelect(SelectExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult VisitSkip(SkipExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult VisitTake(TakeExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult VisitAggregateQuery(AggregateQueryExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult VisitJoinQuery(JoinQueryExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult VisitGroupingQuery(GroupingQueryExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult VisitDistinct(DistinctExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult VisitInclude(IncludeExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }

        JoinQueryResult Visit(QueryExpression exp)
        {
            QueryStateBase state = QueryExpressionResolver.Resolve(this._queryContext, exp, this._scopeParameters, this._queryModel.ScopeTables);
            JoinQueryResult ret = state.ToJoinQueryResult(this._joinType, this._conditionExpression, this._scopeParameters, this._queryModel.ScopeTables, a => { return this._queryModel.GenerateUniqueTableAlias(a); });
            return ret;
        }

    }
}

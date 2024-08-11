using Chloe.DbExpressions;
using Chloe.QueryExpressions;
using Chloe.Query.QueryState;
using Chloe.Utility;
using System.Linq.Expressions;

namespace Chloe.Query.Visitors
{
    class QueryExpressionResolver : QueryExpressionResolverBase
    {
        QueryContext _queryContext;
        ScopeParameterDictionary _scopeParameters;
        StringSet _scopeTables;

        QueryExpressionResolver(QueryContext queryContext, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            this._queryContext = queryContext;
            this._scopeParameters = scopeParameters;
            this._scopeTables = scopeTables;
        }
        public static QueryStateBase Resolve(QueryContext queryContext, QueryExpression queryExpression, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            QueryExpressionResolver resolver = new QueryExpressionResolver(queryContext, scopeParameters, scopeTables);
            return (QueryStateBase)queryExpression.Accept(resolver);
        }

        public override IQueryState Visit(RootQueryExpression exp)
        {
            IQueryState queryState = new RootQueryState(this._queryContext, exp, this._scopeParameters, this._scopeTables);
            return queryState;
        }

        public override IQueryState Visit(JoinQueryExpression exp)
        {
            QueryStateBase qs = QueryExpressionResolver.Resolve(this._queryContext, exp.PrevExpression, this._scopeParameters, this._scopeTables);

            QueryModel queryModel = qs.ToFromQueryModel();

            List<IObjectModel> modelList = new List<IObjectModel>();
            modelList.Add(queryModel.ResultModel);

            foreach (JoinQueryInfo joinQueryInfo in exp.JoinedQueries)
            {
                ScopeParameterDictionary scopeParameters = queryModel.ScopeParameters.Clone(queryModel.ScopeParameters.Count + modelList.Count);
                for (int i = 0; i < modelList.Count; i++)
                {
                    ParameterExpression p = joinQueryInfo.Condition.Parameters[i];
                    scopeParameters[p] = modelList[i];
                }

                JoinQueryResult joinQueryResult = JoinQueryExpressionResolver.Resolve(this._queryContext, joinQueryInfo, queryModel, scopeParameters);

                var nullChecking = DbExpression.CaseWhen(new DbCaseWhenExpression.WhenThenExpressionPair(joinQueryResult.JoinTable.Condition, DbConstantExpression.One), DbConstantExpression.Null, DbConstantExpression.One.Type);

                if (joinQueryInfo.JoinType == JoinType.LeftJoin)
                {
                    joinQueryResult.ResultModel.SetNullChecking(nullChecking);
                }
                else if (joinQueryInfo.JoinType == JoinType.RightJoin)
                {
                    foreach (IObjectModel item in modelList)
                    {
                        item.SetNullChecking(nullChecking);
                    }
                }
                else if (joinQueryInfo.JoinType == JoinType.FullJoin)
                {
                    joinQueryResult.ResultModel.SetNullChecking(nullChecking);
                    foreach (IObjectModel item in modelList)
                    {
                        item.SetNullChecking(nullChecking);
                    }
                }

                joinQueryResult.JoinTable.AppendTo(queryModel.FromTable);
                modelList.Add(joinQueryResult.ResultModel);
            }

            ScopeParameterDictionary scopeParameters1 = queryModel.ScopeParameters.Clone(queryModel.ScopeParameters.Count + modelList.Count);
            for (int i = 0; i < modelList.Count; i++)
            {
                ParameterExpression p = exp.Selector.Parameters[i];
                scopeParameters1[p] = modelList[i];
            }
            IObjectModel model = SelectorResolver.Resolve(this._queryContext, exp.Selector, queryModel.Options, scopeParameters1, queryModel.ScopeTables);
            queryModel.ResultModel = model;

            GeneralQueryState queryState = new GeneralQueryState((qs as QueryStateBase).QueryContext, queryModel);
            return queryState;
        }
    }
}

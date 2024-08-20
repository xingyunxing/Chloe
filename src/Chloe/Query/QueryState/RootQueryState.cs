using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.QueryExpressions;
using Chloe.Utility;
using System.Linq.Expressions;
using Chloe.Visitors;

namespace Chloe.Query.QueryState
{
    internal sealed class RootQueryState : QueryStateBase
    {
        RootQueryExpression _rootQueryExp;
        public RootQueryState(QueryContext queryContext, RootQueryExpression rootQueryExp, ScopeParameterDictionary scopeParameters, StringSet scopeTables) : this(queryContext, rootQueryExp, scopeParameters, scopeTables, null)
        {
        }
        public RootQueryState(QueryContext queryContext, RootQueryExpression rootQueryExp, ScopeParameterDictionary scopeParameters, StringSet scopeTables, Func<string, string> tableAliasGenerator)
          : base(queryContext, CreateQueryModel(queryContext, rootQueryExp, scopeParameters, scopeTables, tableAliasGenerator))
        {
            this._rootQueryExp = rootQueryExp;
        }

        public override QueryModel ToFromQueryModel()
        {
            QueryModel newQueryModel = new QueryModel(this.QueryModel.Options, this.QueryModel.ScopeParameters, this.QueryModel.ScopeTables, this.QueryModel.TouchedTables);
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

        public override IQueryState Accept(IncludeExpression exp)
        {
            ComplexObjectModel owner = (ComplexObjectModel)this.QueryModel.ResultModel;
            owner.Include(exp.NavigationNode, this.QueryModel, false);

            return this;
        }
        public override IQueryState Accept(IgnoreAllFiltersExpression exp)
        {
            this.QueryModel.Options.IgnoreFilters = true;
            return this;
        }

        public override JoinQueryResult ToJoinQueryResult(JoinType joinType, LambdaExpression conditionExpression, ScopeParameterDictionary scopeParameters, StringSet scopeTables, Func<string, string> tableAliasGenerator)
        {
            if (this.QueryModel.Condition != null)
            {
                return base.ToJoinQueryResult(joinType, conditionExpression, scopeParameters, scopeTables, tableAliasGenerator);
            }

            scopeParameters = scopeParameters.Clone(conditionExpression.Parameters.Last(), this.QueryModel.ResultModel);
            DbExpression condition = GeneralExpressionParser.Parse(this.QueryContext, conditionExpression, scopeParameters, scopeTables, this.QueryModel);
            DbJoinTableExpression joinTable = new DbJoinTableExpression(joinType.AsDbJoinType(), this.QueryModel.FromTable.Table, condition);

            if (!this.QueryModel.Options.IgnoreFilters)
            {
                joinTable.Condition = joinTable.Condition.And(this.QueryModel.ContextFilters).And(this.QueryModel.GlobalFilters);
            }

            JoinQueryResult result = new JoinQueryResult();
            result.ResultModel = this.QueryModel.ResultModel;
            result.JoinTable = joinTable;

            return result;
        }

        static QueryModel CreateQueryModel(QueryContext queryContext, RootQueryExpression rootQueryExp, ScopeParameterDictionary scopeParameters, StringSet scopeTables, Func<string, string> tableAliasGenerator)
        {
            DbContextProvider dbContext = queryContext.DbContextProvider;
            Type entityType = rootQueryExp.ElementType;

            if (entityType.IsAbstract || entityType.IsInterface)
                throw new ArgumentException("The type of input can not be abstract class or interface.");

            QueryModel queryModel = new QueryModel(scopeParameters, scopeTables);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entityType);

            DbTable dbTable = typeDescriptor.GenDbTable(rootQueryExp.ExplicitTable);
            string alias = null;
            if (tableAliasGenerator != null)
                alias = tableAliasGenerator(UtilConstants.DefaultTableAlias);
            else
                alias = queryModel.GenerateUniqueTableAlias();

            queryModel.FromTable = CreateRootTable(dbTable, alias, rootQueryExp.Lock);

            ComplexObjectModel objectModel = typeDescriptor.GenObjectModel(alias, queryContext, queryModel.Options);
            objectModel.AssociatedTable = queryModel.FromTable;

            queryModel.ResultModel = objectModel;

            ParseFilters(queryContext, queryModel, rootQueryExp.GlobalFilters, rootQueryExp.ContextFilters);

            return queryModel;
        }
        static DbFromTableExpression CreateRootTable(DbTable table, string alias, LockType lockType)
        {
            DbTableExpression tableExp = new DbTableExpression(table);
            DbTableSegment tableSeg = new DbTableSegment(tableExp, alias, lockType);
            var fromTableExp = new DbFromTableExpression(tableSeg);
            return fromTableExp;
        }
        static void ParseFilters(QueryContext queryContext, QueryModel queryModel, IList<LambdaExpression> globalFilters, IList<LambdaExpression> contextFilters)
        {
            for (int i = 0; i < globalFilters.Count; i++)
            {
                queryModel.GlobalFilters.Add(ParseFilter(queryContext, queryModel, globalFilters[i]));
            }

            for (int i = 0; i < contextFilters.Count; i++)
            {
                queryModel.ContextFilters.Add(ParseFilter(queryContext, queryModel, contextFilters[i]));
            }
        }
        static DbExpression ParseFilter(QueryContext queryContext, QueryModel queryModel, LambdaExpression filter)
        {
            ScopeParameterDictionary scopeParameters = queryModel.ScopeParameters.Clone(filter.Parameters[0], queryModel.ResultModel);
            DbExpression filterCondition = FilterPredicateParser.Parse(queryContext, filter, scopeParameters, queryModel.ScopeTables, queryModel);
            return filterCondition;
        }
    }
}

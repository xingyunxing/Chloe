using Chloe.Query;
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;
using Chloe.Sharding.Queries;
using Chloe.Utility;
using System.Linq.Expressions;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingQueryStateBase : IQueryState
    {
        protected ShardingQueryStateBase(ShardingQueryContext context, ShardingQueryModel queryModel)
        {
            this.Context = context;
            this.QueryModel = queryModel;
        }

        public ShardingQueryContext Context { get; set; }
        public ShardingQueryModel QueryModel { get; set; }

        public virtual IQueryState Accept(WhereExpression exp)
        {
            throw new NotSupportedException($"Cannot call '{nameof(IQuery<object>.Where)}' method except root query.");
        }

        public virtual IQueryState Accept(OrderExpression exp)
        {
            throw new NotSupportedException("Cannot call sorting method except root query.");
        }

        public virtual IQueryState Accept(SelectExpression exp)
        {
            throw new NotSupportedException("Cannot call 'Select' method except root query.");
        }

        public virtual IQueryState Accept(SkipExpression exp)
        {
            throw new NotSupportedException("Cannot call 'Skip' method except root query.");
        }

        public virtual IQueryState Accept(TakeExpression exp)
        {
            throw new NotSupportedException("Cannot call 'Take' method except root query.");
        }

        public virtual IQueryState Accept(AggregateQueryExpression exp)
        {
            throw new NotSupportedException($"{exp.Method.Name}");
        }

        public virtual IQueryState Accept(GroupingQueryExpression exp)
        {
            throw new NotSupportedException();
        }

        public virtual IQueryState Accept(DistinctExpression exp)
        {
            throw new NotSupportedException($"{nameof(IQuery<object>.Distinct)}");
        }

        public virtual IQueryState Accept(IncludeExpression exp)
        {
            throw new NotSupportedException($"{nameof(IQuery<object>.Include)}");
        }

        public virtual IQueryState Accept(IgnoreAllFiltersExpression exp)
        {
            this.QueryModel.IgnoreAllFilters = true;
            return this;
        }

        public virtual IQueryState Accept(TrackingExpression exp)
        {
            this.QueryModel.IsTracking = true;
            return this;
        }

        public async Task<IFeatureEnumerable<object>> CreateQuery()
        {
            ShardingQueryPlan queryPlan = this.CreateQueryPlan();

            if (queryPlan.Tables.Count > 1)
            {
                //主键或唯一索引查询
                bool isUniqueDataQuery = UniqueDataQueryAuthenticator.IsUniqueDataQuery(queryPlan.ShardingContext, queryPlan.QueryModel.GetFinalConditions());

                if (isUniqueDataQuery)
                {
                    UniqueDataQuery query = new UniqueDataQuery(queryPlan);
                    return query;
                }
            }

            if (queryPlan.IsOrderedTables && queryPlan.QueryModel.HasSkip())
            {
                //走分页逻辑，对程序性能有可能好点？
                var pagingResult = await this.ExecutePaging(queryPlan);
                return pagingResult.Result;
            }

            if (queryPlan.IsOrderedTables && !queryPlan.QueryModel.HasSkip())
            {
                //走串行？
            }

            if (!queryPlan.QueryModel.HasSkip())
            {
                //未指定 skip
                return new NonPagingQuery(queryPlan);
            }

            OrdinaryQuery ordinaryQuery = new OrdinaryQuery(queryPlan);
            return ordinaryQuery;
        }
        async Task<PagingExecuteResult<object>> ExecutePaging(ShardingQueryPlan queryPlan)
        {
            throw new NotImplementedException();
            //AggregateQuery<T, long> countQuery = this.GetCountQuery(queryPlan);

            //List<QueryResult<long>> routeTableCounts = await countQuery.ToListAsync();
            //long totals = routeTableCounts.Select(a => a.Result).Sum();

            //if (queryPlan.IsOrderedTables)
            //{
            //    OrderedTableQuery<T> orderedTableQuery = new OrderedTableQuery<T>(queryPlan, routeTableCounts);
            //    return new PagingExecuteResult<T>(totals, orderedTableQuery);
            //}

            //OrdinaryQuery<T> ordinaryQuery = new OrdinaryQuery<T>(queryPlan);
            //return new PagingExecuteResult<T>(totals, ordinaryQuery);
        }

        public ShardingQueryPlan CreateQueryPlan()
        {
            IShardingContext shardingContext = this.Context.DbContext.CreateShardingContext(this.QueryModel.RootEntityType);
            List<RouteTable> routeTables = ShardingTableDiscoverer.GetRouteTables(this.QueryModel.GetFinalConditions(), shardingContext).ToList();
            List<Ordering> orderings = this.QueryModel.Orderings;

            //对物理表重排
            SortResult sortResult;
            if (orderings.Count == 0)
            {
                sortResult = new SortResult() { IsOrdered = true, Tables = routeTables };
            }
            else
            {
                sortResult = shardingContext.SortTables(routeTables, orderings);
            }

            ShardingQueryPlan queryPlan = new ShardingQueryPlan();
            queryPlan.ShardingContext = shardingContext;
            queryPlan.QueryModel = this.QueryModel;
            queryPlan.IsOrderedTables = sortResult.IsOrdered;
            queryPlan.Tables.AddRange(sortResult.Tables.Select(a => new PhysicTable(a)));

            return queryPlan;
        }

        public virtual MappingData GenerateMappingData()
        {
            throw new NotSupportedException();
        }

        public QueryModel ToFromQueryModel()
        {
            throw new NotSupportedException();
        }

        public JoinQueryResult ToJoinQueryResult(JoinType joinType, LambdaExpression conditionExpression, ScopeParameterDictionary scopeParameters, StringSet scopeTables, Func<string, string> tableAliasGenerator)
        {
            throw new NotSupportedException();
        }

        public virtual IFeatureEnumerable<object> CreateFeatureQuery()
        {
            throw new NotImplementedException();
        }
    }
}

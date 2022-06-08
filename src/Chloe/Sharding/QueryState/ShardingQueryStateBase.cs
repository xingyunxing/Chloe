using Chloe.Query;
using Chloe.QueryExpressions;
using Chloe.Sharding.Enumerables;
using Chloe.Sharding.Routing;
using Chloe.Sharding.Visitors;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingQueryStateBase : IQueryState
    {
        protected ShardingQueryStateBase(ShardingQueryStateBase prevQueryState) : this(prevQueryState.Context, prevQueryState.QueryModel)
        {

        }
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

        public virtual IQueryState Accept(PagingExpression exp)
        {
            throw new NotSupportedException($"{nameof(IQuery<object>.Paging)}");
        }

        public IQueryState Accept(JoinQueryExpression exp)
        {
            throw new NotSupportedException($"{nameof(IQuery<object>.Join)}");
        }

        public virtual IFeatureEnumerable<object> CreateQuery()
        {
            ShardingQueryPlan queryPlan = this.CreateQueryPlan();

            if (queryPlan.Tables.Count == 1)
            {
                return new SingleTableQueryEnumerable(queryPlan);
            }

            if (queryPlan.IsOrderedTables)
            {
                //走分页逻辑，对程序性能有可能好点？

                var pagingQueryEnumerable = new PagingQueryEnumerable(this.CreateQueryPlan());
                return new PagingResultDataListEnumerable(pagingQueryEnumerable);
            }

            OrdinaryQueryEnumerable ordinaryQuery = new OrdinaryQueryEnumerable(queryPlan);
            return ordinaryQuery;
        }

        protected virtual IFeatureEnumerable<object> CreateNoPagingQuery()
        {
            ShardingQueryPlan queryPlan = this.CreateQueryPlan();

            if (queryPlan.Tables.Count == 1)
            {
                return new SingleTableQueryEnumerable(queryPlan);
            }

            if (queryPlan.Tables.Count > 1)
            {
                //主键或唯一索引查询
                bool isUniqueDataQuery = UniqueDataQueryAuthenticator.IsUniqueDataQuery(queryPlan.ShardingContext, queryPlan.QueryModel.GetFinalConditions(queryPlan.ShardingContext));

                if (isUniqueDataQuery)
                {
                    UniqueDataQueryEnumerable query = new UniqueDataQueryEnumerable(queryPlan);
                    return query;
                }
            }

            return new NonPagingQueryEnumerable(queryPlan);
        }

        public ShardingQueryPlan CreateQueryPlan()
        {
            IShardingContext shardingContext = this.Context.DbContextProvider.CreateShardingContext(this.QueryModel.RootEntityType);

            List<RouteTable> routeTables = ShardingTableDiscoverer.GetRouteTables(this.QueryModel.GetFinalConditions(shardingContext), shardingContext).ToList();
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
            queryPlan.Tables.AppendRange(sortResult.Tables.Select(a => new PhysicTable(a)));

            return queryPlan;
        }

    }
}

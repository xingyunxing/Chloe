using Chloe.Sharding.Queries;

namespace Chloe.Sharding
{
    internal partial class ShardingQuery<T> : IQuery<T>
    {
        async Task<IFeatureEnumerable<T>> Execute()
        {
            ShardingQueryPlan queryPlan = this.MakeQueryPlan(this);

            if (queryPlan.Tables.Count > 1)
            {
                //主键或唯一索引查询
                bool isUniqueDataQuery = UniqueDataQueryAuthenticator.IsUniqueDataQuery(queryPlan.ShardingContext, queryPlan.Condition);

                if (isUniqueDataQuery)
                {
                    UniqueDataQuery<T> query = new UniqueDataQuery<T>(queryPlan);
                    return query;
                }
            }

            if (((queryPlan.QueryModel.Skip ?? 0) == 0) && queryPlan.IsOrderedTables)
            {
                //走串行？
            }

            if (queryPlan.IsOrderedTables && queryPlan.QueryModel.Skip.HasValue)
            {
                //走分页逻辑，对程序性能有可能好点？
                var pagingResult = await this.ExecutePaging(queryPlan);
                return pagingResult.Result;
            }

            DisorderedMultTableDataQuery<T> dataQuery = new DisorderedMultTableDataQuery<T>(queryPlan);
            return dataQuery;
        }

        async Task<PagingExecuteResult<T>> ExecutePaging(int pageNumber, int pageSize)
        {
            ShardingQueryPlan queryPlan = this.MakeQueryPlan(this.TakePage(pageNumber, pageSize) as ShardingQuery<T>);
            return await this.ExecutePaging(queryPlan);
        }
        async Task<PagingExecuteResult<T>> ExecutePaging(ShardingQueryPlan queryPlan)
        {
            MultTableCountQuery<T> countQuery = new MultTableCountQuery<T>(queryPlan);

            List<MultTableCountQueryResult> routeTableCounts = await countQuery.ToListAsync();
            long totals = routeTableCounts.Select(a => a.Count).Sum();

            if (queryPlan.IsOrderedTables)
            {
                OrderedMultTableDataQuery<T> orderlyMultTableDataQuery = new OrderedMultTableDataQuery<T>(queryPlan, routeTableCounts);
                return new PagingExecuteResult<T>(totals, orderlyMultTableDataQuery);
            }

            DisorderedMultTableDataQuery<T> disorderedMultTableDataQuery = new DisorderedMultTableDataQuery<T>(queryPlan);
            return new PagingExecuteResult<T>(totals, disorderedMultTableDataQuery);
        }
        async Task<long> QueryCount()
        {
            ShardingQueryPlan queryPlan = this.MakeQueryPlan(this);
            MultTableCountQuery<T> countQuery = new MultTableCountQuery<T>(queryPlan);

            List<MultTableCountQueryResult> routeTableCounts = await countQuery.ToListAsync();
            long totals = routeTableCounts.Select(a => a.Count).Sum();
            return totals;
        }

        ShardingQueryPlan MakeQueryPlan(ShardingQuery<T> query)
        {
            ShardingQueryPlan queryPlan = new ShardingQueryPlan();
            queryPlan.QueryModel = ShardingQueryModelPeeker.Peek(query.InnerQuery.QueryExpression);

            IShardingContext shardingContext = (query.InnerQuery.DbContext as ShardingDbContext).CreateShardingContext(typeof(T));

            queryPlan.ShardingContext = shardingContext;

            var condition = ShardingHelpers.ConditionCombine(queryPlan.QueryModel);
            queryPlan.Condition = condition;
            List<RouteTable> routeTables = ShardingTableDiscoverer.GetRouteTables(condition, shardingContext).ToList();

            List<Ordering> orderings = queryPlan.QueryModel.Orderings;

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

            queryPlan.IsOrderedTables = sortResult.IsOrdered;
            queryPlan.Tables.AddRange(sortResult.Tables.Select(a => new PhysicTable(a)));

            queryPlan.IsTrackingQuery = query.InnerQuery._trackEntity;

            return queryPlan;
        }
    }
}

using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.Sharding.Queries;
using System.Threading.Tasks;

namespace Chloe.Sharding
{
    internal partial class ShardingQuery<T> : IQuery<T>
    {
        async Task<IFeatureEnumerable<T>> Execute()
        {
            ShardingQueryPlan queryPlan = this.MakeQueryPlan(this);

            //针对主键查询
            bool isUniqueDataQuery = UniqueDataQueryAuthenticator.IsUniqueDataQuery(queryPlan.Condition, queryPlan.ShardingContext.TypeDescriptor.PrimaryKeys.First().Property);

            if (isUniqueDataQuery)
            {
                UniqueDataQuery<T> query = new UniqueDataQuery<T>(queryPlan);
                return query;
            }

            if (((queryPlan.QueryModel.Skip ?? 0) == 0) && queryPlan.IsOrderedRouteTables)
            {
                //走串行？
            }

            if (queryPlan.IsOrderedRouteTables && queryPlan.QueryModel.Skip.HasValue)
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

            if (queryPlan.IsOrderedRouteTables)
            {
                OrderedMultTableDataQuery<T> orderlyMultTableDataQuery = new OrderedMultTableDataQuery<T>(queryPlan, routeTableCounts);
                return new PagingExecuteResult<T>(totals, orderlyMultTableDataQuery);
            }

            DisorderedMultTableDataQuery<T> disorderedMultTableDataQuery = new DisorderedMultTableDataQuery<T>(queryPlan);
            return new PagingExecuteResult<T>(totals, disorderedMultTableDataQuery);
        }

        ShardingQueryPlan MakeQueryPlan(ShardingQuery<T> query)
        {
            ShardingQueryPlan queryPlan = new ShardingQueryPlan();
            queryPlan.QueryModel = ShardingQueryModelPeeker.Peek(query.InnerQuery.QueryExpression);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(T));
            IShardingConfig shardingConfig = ShardingConfigContainer.Get(typeof(T));
            IShardingContext shardingContext = new ShardingContext((ShardingDbContext)query.InnerQuery.DbContext, shardingConfig, typeDescriptor);

            queryPlan.ShardingContext = shardingContext;

            var condition = ShardingHelpers.ConditionCombine(queryPlan.QueryModel);
            queryPlan.Condition = condition;
            List<RouteTable> routeTables = ShardingTablePeeker.Peek(condition, shardingContext);

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

            foreach (var routeTable in sortResult.Tables)
            {
                routeTable.DataSource.DbContextFactory = new RouteDbContextFactoryWrapper(routeTable.DataSource.DbContextFactory, shardingContext.DbContext);
            }

            queryPlan.RouteTables = sortResult.Tables;
            queryPlan.IsOrderedRouteTables = sortResult.IsOrdered;

            return queryPlan;
        }
    }
}

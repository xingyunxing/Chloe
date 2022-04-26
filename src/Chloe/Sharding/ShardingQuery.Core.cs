using Chloe.Reflection;
using Chloe.Sharding.Models;
using Chloe.Sharding.Queries;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    internal partial class ShardingQuery<T> : IQuery<T>
    {
        async Task<IFeatureEnumerable<T>> Execute()
        {
            ShardingQueryPlan queryPlan = this.MakeQueryPlan(this);

            if (queryPlan.QueryModel.GroupKeySelectors.Count > 0)
            {
                //分组查询
                var groupAggQueryType = typeof(GroupAggregateQuery).MakeGenericType(queryPlan.QueryModel.Selector.Parameters[0].Type, typeof(T));
                var groupAggQuery = groupAggQueryType.GetConstructor(new Type[] { queryPlan.GetType() }).FastCreateInstance(queryPlan);

                return (IFeatureEnumerable<T>)groupAggQuery;
            }

            if (queryPlan.Tables.Count > 1)
            {
                //主键或唯一索引查询
                bool isUniqueDataQuery = UniqueDataQueryAuthenticator.IsUniqueDataQuery(queryPlan.ShardingContext, queryPlan.QueryModel.GetFinalConditions());

                if (isUniqueDataQuery)
                {
                    UniqueDataQuery query = new UniqueDataQuery(queryPlan);
                    return (IFeatureEnumerable<T>)query;
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
                return (IFeatureEnumerable<T>)new NonPagingQuery(queryPlan);
            }

            OrdinaryQuery ordinaryQuery = new OrdinaryQuery(queryPlan);
            return (IFeatureEnumerable<T>)ordinaryQuery;
        }

        async Task<PagingExecuteResult<T>> ExecutePaging(int pageNumber, int pageSize)
        {
            ShardingQueryPlan queryPlan = this.MakeQueryPlan(this.TakePage(pageNumber, pageSize) as ShardingQuery<T>);
            return await this.ExecutePaging(queryPlan);
        }
        async Task<PagingExecuteResult<T>> ExecutePaging(ShardingQueryPlan queryPlan)
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
        async Task<long> QueryCount()
        {
            throw new NotImplementedException();
            //AggregateQuery<T, long> countQuery = this.GetCountQuery();
            //return await countQuery.AsAsyncEnumerable().Select(a => a.Result).SumAsync();
        }
        AggregateQuery GetCountQuery(ShardingQueryPlan queryPlan = null)
        {
            throw new NotImplementedException();
            //Func<IQuery<T>, bool, Task<long>> executor = async (query, @async) =>
            //{
            //    long result = @async ? await query.LongCountAsync() : query.LongCount();
            //    return result;
            //};

            //AggregateQuery<T, long> aggQuery = new AggregateQuery<T, long>(queryPlan ?? this.MakeQueryPlan(this), executor);
            //return aggQuery;
        }

        async Task<bool> QueryAny()
        {
            ShardingQueryPlan queryPlan = this.MakeQueryPlan(this);
            AnyQuery anyQuery = new AnyQuery(queryPlan);

            List<QueryResult<object>> results = await anyQuery.ToListAsync();
            bool hasData = results.Any(a => (bool)a.Result == true);
            return hasData;
        }

        async Task<decimal?> QueryAverageAsync(LambdaExpression selector)
        {
            throw new NotImplementedException();
            var aggSelector = ShardingHelpers.MakeAggregateSelector(selector);

            Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            {
                var q = query.Select(aggSelector);
                var result = @async ? await q.FirstAsync() : q.First();
                return result;
            };

            AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);

            decimal? sum = null;
            long count = 0;

            await aggQuery.AsAsyncEnumerable().Select(a => (AggregateModel)a.Result).ForEach(a =>
            {
                if (a.Sum == null)
                    return;

                sum = (sum ?? 0) + a.Sum.Value;
                count = count + a.Count;
            });

            if (sum == null)
                return null;

            decimal avg = sum.Value / count;
            return avg;
        }

        ShardingQueryPlan MakeQueryPlan(ShardingQuery<T> query)
        {
            ShardingQueryPlan queryPlan = new ShardingQueryPlan();
            queryPlan.QueryModel = ShardingQueryModelPeeker.Peek(query.InnerQuery.QueryExpression);

            //TODO get dbContext

            throw new NotImplementedException();
            ShardingDbContext dbContext = null;

            IShardingContext shardingContext = dbContext.CreateShardingContext(queryPlan.QueryModel.RootEntityType);

            queryPlan.ShardingContext = shardingContext;

            List<RouteTable> routeTables = ShardingTableDiscoverer.GetRouteTables(queryPlan.QueryModel.GetFinalConditions(), shardingContext).ToList();

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

            return queryPlan;
        }
    }
}

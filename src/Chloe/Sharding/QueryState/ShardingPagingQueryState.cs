using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;
using Chloe.Sharding.Queries;
using Chloe.Reflection;
using System.Threading;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingPagingQueryState : ShardingQueryStateBase
    {
        public ShardingPagingQueryState(ShardingQueryContext context, ShardingQueryModel queryModel, int skipCount, int takeCount) : base(context, queryModel)
        {
            this.QueryModel.Skip = skipCount;
            this.QueryModel.Take = takeCount;
        }

        protected override async Task<IFeatureEnumerable<object>> CreateQuery(ShardingQueryPlan queryPlan, CancellationToken cancellationToken)
        {
            AggregateQuery countQuery = this.GetCountQuery(queryPlan);

            List<QueryResult<long>> routeTableCounts = await countQuery.AsAsyncEnumerable().Select(a => new QueryResult<long>() { Table = a.Table, Result = (long)a.Result }).ToListAsync();
            long totals = routeTableCounts.Select(a => a.Result).Sum();

            if (queryPlan.IsOrderedTables)
            {
                OrderedTableQuery orderedTableQuery = new OrderedTableQuery(queryPlan, routeTableCounts);
                return await this.MakeFeatureEnumerable(totals, orderedTableQuery, cancellationToken);
            }

            OrdinaryQuery ordinaryQuery = new OrdinaryQuery(queryPlan);
            return await this.MakeFeatureEnumerable(totals, ordinaryQuery, cancellationToken);
        }

        async Task<IFeatureEnumerable<object>> MakeFeatureEnumerable(long totals, IFeatureEnumerable<object> dataQuery, CancellationToken cancellationToken)
        {
            List<object> dataList = await dataQuery.ToListAsync(cancellationToken);

            Type elementType = this.QueryModel.Selector == null ? this.QueryModel.RootEntityType : this.QueryModel.Selector.Body.Type;

            var pagingResultEnumerable = typeof(ShardingPagingQueryState).GetMethod(nameof(ShardingPagingQueryState.MakePagingResultEnumerable)).MakeGenericMethod(elementType).FastInvoke(totals, dataList);

            return (IFeatureEnumerable<object>)pagingResultEnumerable;
        }


        IFeatureEnumerable<PagingResult<TElement>> MakePagingResultEnumerable<TElement>(long totals, List<object> dataList)
        {
            PagingResult<TElement> pagingResult = new PagingResult<TElement>();

            pagingResult.Count = totals;
            foreach (var data in dataList)
            {
                pagingResult.DataList.Add((TElement)data);
            }

            return new ScalarFeatureEnumerable<PagingResult<TElement>>(pagingResult);
        }


        AggregateQuery GetCountQuery(ShardingQueryPlan queryPlan)
        {

            Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            {
                long result = @async ? await query.LongCountAsync() : query.LongCount();
                return result;
            };

            AggregateQuery aggQuery = new AggregateQuery(queryPlan, executor);
            return aggQuery;

            throw new NotImplementedException();
        }
    }
}

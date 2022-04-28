using Chloe.Query.QueryExpressions;
using Chloe.Sharding.Queries;
using Chloe.Reflection;
using System.Threading;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingPagingQueryState : ShardingQueryStateBase
    {
        public ShardingPagingQueryState(ShardingQueryStateBase prevQueryState, PagingExpression exp) : base(prevQueryState)
        {
            this.QueryModel.Skip = exp.Skip;
            this.QueryModel.Take = exp.Take;
        }

        public override IFeatureEnumerable<object> CreateQuery()
        {
            var pagingQueryEnumerable = new PagingQueryEnumerable(this.CreateQueryPlan());
            return pagingQueryEnumerable.Select(a => a.MakeTypedPagingResultObject(this.QueryModel.GetElementType()));
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
        AggregateQuery<long> GetCountQuery(ShardingQueryPlan queryPlan)
        {
            Func<IQuery, bool, Task<long>> executor = async (query, @async) =>
            {
                long result = @async ? await query.LongCountAsync() : query.LongCount();
                return result;
            };

            var aggQuery = new AggregateQuery<long>(queryPlan, executor);
            return aggQuery;
        }

        class QueryEnumerable : FeatureEnumerable<object>
        {
            ShardingPagingQueryState QueryState;

            public QueryEnumerable(ShardingPagingQueryState queryState)
            {
                this.QueryState = queryState;
            }

            public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            class Enumerator : FeatureEnumerator<object>
            {
                QueryEnumerable _enumerable;
                CancellationToken _cancellationToken;
                public Enumerator(QueryEnumerable enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
                {
                    ShardingQueryPlan queryPlan = this._enumerable.QueryState.CreateQueryPlan();
                    AggregateQuery<long> countQuery = this._enumerable.QueryState.GetCountQuery(queryPlan);

                    List<QueryResult<long>> routeTableCounts = await countQuery.AsAsyncEnumerable().ToListAsync();
                    long totals = routeTableCounts.Select(a => a.Result).Sum();

                    if (queryPlan.IsOrderedTables)
                    {
                        OrderedTableQuery orderedTableQuery = new OrderedTableQuery(queryPlan, routeTableCounts);
                        var featureEnumerable = await this._enumerable.QueryState.MakeFeatureEnumerable(totals, orderedTableQuery, this._cancellationToken);
                        return featureEnumerable.GetFeatureEnumerator(this._cancellationToken);
                    }
                    else
                    {
                        OrdinaryQuery ordinaryQuery = new OrdinaryQuery(queryPlan);
                        var featureEnumerable = await this._enumerable.QueryState.MakeFeatureEnumerable(totals, ordinaryQuery, this._cancellationToken);
                        return featureEnumerable.GetFeatureEnumerator(this._cancellationToken);
                    }
                }
            }
        }


    }
}

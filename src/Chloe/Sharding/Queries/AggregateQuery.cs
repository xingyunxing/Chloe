using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    class AggregateQueryPlan<TResult>
    {
        public DataQueryModel QueryModel { get; set; }
        public ShardingTableAggregateQuery<TResult> Query { get; set; }
    }

    internal class AggregateQuery<TResult> : FeatureEnumerable<QueryResult<TResult>>
    {
        ShardingQueryPlan _queryPlan;
        List<IPhysicTable> _tables;
        Func<IQuery, bool, Task<TResult>> _executor;
        Func<ParallelQueryContext> _parallelQueryContextFactory;

        public AggregateQuery(ShardingQueryPlan queryPlan, Func<IQuery, bool, Task<TResult>> executor) : this(queryPlan, executor, () => { return new ParallelQueryContext(queryPlan.ShardingContext); })
        {
            this._queryPlan = queryPlan;
            this._tables = queryPlan.Tables;
            this._executor = executor;
        }

        public AggregateQuery(ShardingQueryPlan queryPlan, Func<IQuery, bool, Task<TResult>> executor, Func<ParallelQueryContext> parallelQueryContextFactory)
        {
            this._queryPlan = queryPlan;
            this._tables = queryPlan.Tables;
            this._executor = executor;
            this._parallelQueryContextFactory = parallelQueryContextFactory;
        }

        public override IFeatureEnumerator<QueryResult<TResult>> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<QueryResult<TResult>>
        {
            AggregateQuery<TResult> _enumerable;
            CancellationToken _cancellationToken;

            IFeatureEnumerator<TResult> _innerEnumerator;

            int _currentIdx = 0;
            QueryResult<TResult> _current;

            public Enumerator(AggregateQuery<TResult> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            public QueryResult<TResult> Current => this._current;

            object IEnumerator.Current => this._current;

            public void Dispose()
            {
                this._innerEnumerator?.Dispose();
            }

            public async ValueTask DisposeAsync()
            {
                if (this._innerEnumerator == null)
                    return;

                await this._innerEnumerator.DisposeAsync();
            }

            public bool MoveNext()
            {
                return this.MoveNext(false).GetResult();
            }

            public BoolResultTask MoveNextAsync()
            {
                return this.MoveNext(true);
            }

            void Init()
            {
                var tables = this._enumerable._tables;
                var queryPlan = this._enumerable._queryPlan;

                List<AggregateQueryPlan<TResult>> aggQueryPlans = new List<AggregateQueryPlan<TResult>>(tables.Count);
                foreach (IPhysicTable table in tables)
                {
                    var aggQueryPlan = new AggregateQueryPlan<TResult>();

                    DataQueryModel dataQueryModel = new DataQueryModel(queryPlan.QueryModel.RootEntityType);
                    dataQueryModel.Table = table;
                    dataQueryModel.IgnoreAllFilters = queryPlan.QueryModel.IgnoreAllFilters;

                    dataQueryModel.Conditions.AppendRange(queryPlan.QueryModel.Conditions);

                    aggQueryPlan.QueryModel = dataQueryModel;

                    aggQueryPlans.Add(aggQueryPlan);
                }

                ParallelQueryContext queryContext = this._enumerable._parallelQueryContextFactory();

                try
                {
                    foreach (var group in aggQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                    {
                        int count = group.Count();

                        ISharedDbContextProviderPool dbContextProviderPool = queryContext.GetDbContextProviderPool(group.First().QueryModel.Table.DataSource);

                        foreach (AggregateQueryPlan<TResult> aggQueryPlan in group)
                        {
                            var shardingQuery = new ShardingTableAggregateQuery<TResult>(queryContext, dbContextProviderPool, aggQueryPlan.QueryModel, this._enumerable._executor);
                            aggQueryPlan.Query = shardingQuery;
                        }
                    }
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }

                ParallelConcatEnumerable<TResult> aggQueryEnumerable = new ParallelConcatEnumerable<TResult>(queryContext, aggQueryPlans.Select(a => a.Query));
                this._innerEnumerator = aggQueryEnumerable.GetFeatureEnumerator(this._cancellationToken);
            }
            async BoolResultTask MoveNext(bool @async)
            {
                if (this._innerEnumerator == null)
                {
                    this.Init();
                }

                bool hasNext = await this._innerEnumerator.MoveNext(@async);

                if (!hasNext)
                {
                    this._current = default;
                    return false;
                }

                IPhysicTable table = this._enumerable._tables[this._currentIdx++];
                this._current = new QueryResult<TResult>() { Table = table, Result = this._innerEnumerator.GetCurrent() };
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}

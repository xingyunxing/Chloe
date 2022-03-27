using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    class AggregateQueryPlan<T, TResult>
    {
        public DataQueryModel QueryModel { get; set; }
        public SingleTableAggregateQuery<T, TResult> Query { get; set; }
    }

    internal class AggregateQuery<TEntity, TResult> : FeatureEnumerable<QueryResult<TResult>>
    {
        ShardingQueryPlan _queryPlan;
        List<IPhysicTable> _tables;
        Func<IQuery<TEntity>, bool, Task<TResult>> _executor;

        public AggregateQuery(ShardingQueryPlan queryPlan, Func<IQuery<TEntity>, bool, Task<TResult>> executor)
        {
            this._queryPlan = queryPlan;
            this._tables = queryPlan.Tables;
            this._executor = executor;
        }

        public override IFeatureEnumerator<QueryResult<TResult>> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<QueryResult<TResult>>
        {
            AggregateQuery<TEntity, TResult> _enumerable;
            CancellationToken _cancellationToken;

            IFeatureEnumerator<TResult> _innerEnumerator;

            int _currentIdx = 0;
            QueryResult<TResult> _current;

            public Enumerator(AggregateQuery<TEntity, TResult> enumerable, CancellationToken cancellationToken = default)
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

                List<AggregateQueryPlan<TEntity, TResult>> aggQueryPlans = new List<AggregateQueryPlan<TEntity, TResult>>(tables.Count);
                foreach (IPhysicTable table in tables)
                {
                    AggregateQueryPlan<TEntity, TResult> aggQueryPlan = new AggregateQueryPlan<TEntity, TResult>();

                    DataQueryModel dataQueryModel = new DataQueryModel();
                    dataQueryModel.Table = table;
                    dataQueryModel.IgnoreAllFilters = queryPlan.QueryModel.IgnoreAllFilters;
                    dataQueryModel.Conditions.AddRange(queryPlan.QueryModel.Conditions);

                    aggQueryPlan.QueryModel = dataQueryModel;

                    aggQueryPlans.Add(aggQueryPlan);
                }

                ParallelQueryContext queryContext = new ParallelQueryContext();

                foreach (var group in aggQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    ShareDbContextPool dbContextPool = ShardingHelpers.CreateDbContextPool(this._enumerable._queryPlan.ShardingContext, group.First().QueryModel.Table.DataSource, count);
                    queryContext.AddManagedResource(dbContextPool);

                    foreach (AggregateQueryPlan<TEntity, TResult> aggQueryPlan in group)
                    {
                        SingleTableAggregateQuery<TEntity, TResult> query = new SingleTableAggregateQuery<TEntity, TResult>(dbContextPool, aggQueryPlan.QueryModel, this._enumerable._executor);
                        aggQueryPlan.Query = query;
                    }
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

using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Chloe.Sharding.Queries
{
    class TableCountQueryPlan<T>
    {
        public DataQueryModel QueryModel { get; set; }
        public SingleTableCountQuery<T> Query { get; set; }
    }

    /// <summary>
    /// 求各分表的数据量
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class MultTableCountQuery<TEntity> : FeatureEnumerable<MultTableCountQueryResult>
    {
        ShardingQueryPlan _queryPlan;
        IShardingContext _shardingContext;
        List<RouteTable> _tables;

        public MultTableCountQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
            this._shardingContext = queryPlan.ShardingContext;
            this._tables = queryPlan.RouteTables;
        }

        public override IFeatureEnumerator<MultTableCountQueryResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<MultTableCountQueryResult>
        {
            MultTableCountQuery<TEntity> _enumerable;
            CancellationToken _cancellationToken;

            IFeatureEnumerator<long> _innerEnumerator;

            int _currentIdx = 0;
            MultTableCountQueryResult _current;

            public Enumerator(MultTableCountQuery<TEntity> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            public MultTableCountQueryResult Current => this._current;

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
                int maxConnectionsPerDatabase = this._enumerable._shardingContext.MaxConnectionsPerDatabase;

                List<TableCountQueryPlan<TEntity>> countQueryPlans = new List<TableCountQueryPlan<TEntity>>(tables.Count);
                foreach (RouteTable table in tables)
                {
                    TableCountQueryPlan<TEntity> countQuery = new TableCountQueryPlan<TEntity>();

                    DataQueryModel dataQueryModel = new DataQueryModel();
                    dataQueryModel.Table = table;
                    dataQueryModel.IgnoreAllFilters = queryPlan.QueryModel.IgnoreAllFilters;
                    dataQueryModel.Conditions.AddRange(queryPlan.QueryModel.Conditions);

                    countQuery.QueryModel = dataQueryModel;

                    countQueryPlans.Add(countQuery);
                }

                ParallelQueryContext queryContext = new ParallelQueryContext();

                foreach (var group in countQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    ShareDbContextPool dbContextPool = ShardingHelpers.CreateDbContextPool(group.First().QueryModel.Table.DataSource.DbContextFactory, count, maxConnectionsPerDatabase);
                    queryContext.AddManagedResource(dbContextPool);

                    foreach (TableCountQueryPlan<TEntity> countQuery in group)
                    {
                        SingleTableCountQuery<TEntity> query = new SingleTableCountQuery<TEntity>(dbContextPool, countQuery.QueryModel);
                        countQuery.Query = query;
                    }
                }

                ParallelConcatEnumerable<long> countQueryEnumerable = new ParallelConcatEnumerable<long>(queryContext, countQueryPlans.Select(a => a.Query));
                this._innerEnumerator = countQueryEnumerable.GetFeatureEnumerator(this._cancellationToken);
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

                RouteTable table = this._enumerable._tables[this._currentIdx++];
                this._current = new MultTableCountQueryResult() { Table = table, Count = this._innerEnumerator.GetCurrent() };
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}

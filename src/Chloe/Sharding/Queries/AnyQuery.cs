using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    class AnyQueryPlan
    {
        public DataQueryModel QueryModel { get; set; }
        public SingleTableAnyQuery Query { get; set; }
    }

    internal class AnyQuery : FeatureEnumerable<QueryResult<bool>>
    {
        ShardingQueryPlan _queryPlan;
        List<IPhysicTable> _tables;

        public AnyQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
            this._tables = queryPlan.Tables;
        }

        public override IFeatureEnumerator<QueryResult<bool>> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<QueryResult<bool>>
        {
            AnyQuery _enumerable;
            CancellationToken _cancellationToken;

            IFeatureEnumerator<bool> _innerEnumerator;

            int _currentIdx = 0;
            QueryResult<bool> _current;

            public Enumerator(AnyQuery enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            public QueryResult<bool> Current => this._current;

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

                List<AnyQueryPlan> countQueryPlans = new List<AnyQueryPlan>(tables.Count);
                foreach (IPhysicTable table in tables)
                {
                    AnyQueryPlan anyQueryPlan = new AnyQueryPlan();

                    DataQueryModel dataQueryModel = new DataQueryModel(queryPlan.QueryModel.RootEntityTypeDescriptor);
                    dataQueryModel.Table = table;
                    dataQueryModel.IgnoreAllFilters = queryPlan.QueryModel.IgnoreAllFilters;
                    dataQueryModel.Conditions.AddRange(queryPlan.QueryModel.Conditions);

                    anyQueryPlan.QueryModel = dataQueryModel;

                    countQueryPlans.Add(anyQueryPlan);
                }

                AnyQueryParallelQueryContext queryContext = new AnyQueryParallelQueryContext();

                foreach (var group in countQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    ShareDbContextPool dbContextPool = ShardingHelpers.CreateDbContextPool(this._enumerable._queryPlan.ShardingContext, group.First().QueryModel.Table.DataSource, count);
                    queryContext.AddManagedResource(dbContextPool);

                    foreach (AnyQueryPlan anyQueryPlan in group)
                    {
                        SingleTableAnyQuery query = new SingleTableAnyQuery(queryContext, dbContextPool, anyQueryPlan.QueryModel);
                        anyQueryPlan.Query = query;
                    }
                }

                var queryEnumerable = new ParallelConcatEnumerable<bool>(queryContext, countQueryPlans.Select(a => a.Query));
                this._innerEnumerator = queryEnumerable.GetFeatureEnumerator(this._cancellationToken);
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
                this._current = new QueryResult<bool>() { Table = table, Result = this._innerEnumerator.GetCurrent() };
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}

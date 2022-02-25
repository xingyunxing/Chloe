using Chloe.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chloe.Sharding.Queries
{
    class PhysicTableCountQueryModel<T>
    {
        public PhysicTable Table { get; set; }
        public DataQueryModel QueryModel { get; set; }
        public SingleTableCountQuery<T> Query { get; set; }
    }

    /// <summary>
    /// 求各分表的数据量
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class MultTableCountQuery<TEntity> : FeatureEnumerable<MultTableCountQueryResult>
    {
        List<PhysicTable> _tables;
        ShardingQueryModel _queryModel;
        int _maxConnectionsPerDatabase;

        public MultTableCountQuery(List<PhysicTable> tables, ShardingQueryModel queryModel, int maxConnectionsPerDatabase)
        {
            this._tables = tables;
            this._queryModel = queryModel;
            this._maxConnectionsPerDatabase = maxConnectionsPerDatabase;
        }

        public override IFeatureEnumerator<MultTableCountQueryResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<MultTableCountQueryResult>
        {
            MultTableCountQuery<TEntity> _enumerable;
            List<PhysicTable> _tables;
            ShardingQueryModel _queryModel;
            int _maxConnectionsPerDatabase;

            CancellationToken _cancellationToken;

            IFeatureEnumerator<int> _innerEnumerator;

            int _currentIdx = 0;
            MultTableCountQueryResult _current;

            public Enumerator(MultTableCountQuery<TEntity> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._tables = enumerable._tables;
                this._queryModel = enumerable._queryModel;
                this._maxConnectionsPerDatabase = enumerable._maxConnectionsPerDatabase;

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
                var tables = this._tables;
                var queryModel = this._queryModel;
                int maxConnectionsPerDatabase = this._maxConnectionsPerDatabase;

                List<PhysicTableCountQueryModel<TEntity>> countQueryList = new List<PhysicTableCountQueryModel<TEntity>>(tables.Count);
                foreach (PhysicTable table in tables)
                {
                    PhysicTableCountQueryModel<TEntity> countQuery = new PhysicTableCountQueryModel<TEntity>();
                    countQuery.Table = table;

                    DataQueryModel dataQueryModel = new DataQueryModel();
                    dataQueryModel.Table = countQuery.Table;
                    dataQueryModel.IgnoreAllFilters = queryModel.IgnoreAllFilters;
                    dataQueryModel.Conditions.AddRange(queryModel.Conditions);

                    countQuery.QueryModel = dataQueryModel;

                    countQueryList.Add(countQuery);
                }

                foreach (var group in countQueryList.GroupBy(a => a.Table.DataSource.Name))
                {
                    int count = group.Count();
                    List<IDbContext> dbContexts = ShardingHelpers.CreateDbContexts(group.First().Table.DataSource.DbContextFactory, count, maxConnectionsPerDatabase);
                    ShareDbContextPool dbContextPool = new ShareDbContextPool(dbContexts);

                    foreach (PhysicTableCountQueryModel<TEntity> countQuery in group)
                    {
                        SingleTableCountQuery<TEntity> query = new SingleTableCountQuery<TEntity>(dbContextPool, countQuery.QueryModel);
                        countQuery.Query = query;
                    }
                }

                ParallelConcatEnumerable<int> countQueryEnumerable = new ParallelConcatEnumerable<int>(countQueryList.Select(a => a.Query));
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

                PhysicTable table = this._tables[this._currentIdx++];
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

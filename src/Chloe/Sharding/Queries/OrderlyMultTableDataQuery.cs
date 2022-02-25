using Chloe.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 有序的表数据查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OrderlyMultTableDataQuery<T> : FeatureEnumerable<T>
    {
        List<MultTableCountQueryResult> _countQueryResults;
        ShardingQueryModel _queryModel;
        int _maxConnectionsPerDatabase;

        public OrderlyMultTableDataQuery(List<MultTableCountQueryResult> countQueryResults, ShardingQueryModel queryModel, int maxConnectionsPerDatabase)
        {
            this._countQueryResults = countQueryResults;
            this._queryModel = queryModel;
            this._maxConnectionsPerDatabase = maxConnectionsPerDatabase;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<T>, IFeatureEnumerator<T>
        {
            OrderlyMultTableDataQuery<T> _enumerable;
            List<MultTableCountQueryResult> _countQueryResults;
            ShardingQueryModel _queryModel;
            int _maxConnectionsPerDatabase;

            CancellationToken _cancellationToken;

            public Enumerator(OrderlyMultTableDataQuery<T> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._countQueryResults = enumerable._countQueryResults;
                this._queryModel = enumerable._queryModel;
                this._maxConnectionsPerDatabase = enumerable._maxConnectionsPerDatabase;

                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<T>> CreateEnumerator(bool @async)
            {
                List<MultTableCountQueryResult> countQueryResults = this._countQueryResults;
                var queryModel = this._queryModel;
                int maxConnectionsPerDatabase = this._maxConnectionsPerDatabase;

                List<TableDataQueryModel<T>> dataQueries = new List<TableDataQueryModel<T>>();

                int nextTableSkip = queryModel.Skip.Value;
                int nextTableTake = queryModel.Take.Value;
                for (int i = 0; i < countQueryResults.Count; i++)
                {
                    var countQueryResult = countQueryResults[i];
                    int canTake = countQueryResult.Count - nextTableSkip;
                    if (canTake > 0)
                    {
                        TableDataQueryModel<T> query = new TableDataQueryModel<T>();
                        DataQueryModel dataQueryModel = new DataQueryModel();
                        dataQueryModel.Table = countQueryResult.Table;
                        dataQueryModel.Skip = nextTableSkip;
                        dataQueryModel.Take = canTake >= nextTableTake ? nextTableTake : canTake;
                        dataQueryModel.IgnoreAllFilters = queryModel.IgnoreAllFilters;
                        dataQueryModel.Conditions.AddRange(queryModel.Conditions);
                        dataQueryModel.Orderings.AddRange(queryModel.Orderings);

                        query.QueryModel = dataQueryModel;
                        query.Table = countQueryResult.Table;

                        nextTableSkip = 0;
                        nextTableTake = nextTableTake - dataQueryModel.Take.Value;

                        dataQueries.Add(query);
                    }
                    else
                    {
                        nextTableSkip = nextTableSkip - countQueryResult.Count;
                    }

                    if (nextTableTake <= 0)
                    {
                        break;
                    }
                }

                foreach (var group in dataQueries.GroupBy(a => a.Table.DataSource.Name))
                {
                    int count = group.Count();

                    List<IDbContext> dbContexts = ShardingHelpers.CreateDbContexts(group.First().Table.DataSource.DbContextFactory, count, maxConnectionsPerDatabase);
                    ShareDbContextPool dbContextPool = new ShareDbContextPool(dbContexts);

                    bool lazyQuery = dbContexts.Count >= count;

                    foreach (var dataQuery in group)
                    {
                        SingleTableDataQuery<T> query = new SingleTableDataQuery<T>(dbContextPool, dataQuery.QueryModel, lazyQuery);
                        dataQuery.Query = query;
                    }
                }

                ParallelConcatEnumerable<T> concatEnumerable = new ParallelConcatEnumerable<T>(dataQueries.Select(a => a.Query));
                var enumerator = concatEnumerable.GetFeatureEnumerator(this._cancellationToken);
                return enumerator;
            }
        }
    }
}

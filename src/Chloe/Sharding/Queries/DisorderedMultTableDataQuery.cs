using Chloe.Core.Visitors;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.Reflection;
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
    /// <summary>
    /// 无序的表数据查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DisorderedMultTableDataQuery<T> : FeatureEnumerable<T>
    {
        List<PhysicTable> _tables;
        ShardingQueryModel _queryModel;
        int _maxConnectionsPerDatabase;
        int _maxInItems;

        public DisorderedMultTableDataQuery(List<PhysicTable> tables, ShardingQueryModel queryModel, int maxConnectionsPerDatabase, int maxInItems)
        {
            this._tables = tables;
            this._queryModel = queryModel;
            this._maxConnectionsPerDatabase = maxConnectionsPerDatabase;
            this._maxInItems = maxInItems;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<T>
        {
            DisorderedMultTableDataQuery<T> _enumerable;
            List<PhysicTable> _tables;
            ShardingQueryModel _queryModel;
            int _maxConnectionsPerDatabase;
            int _maxInItems;

            CancellationToken _cancellationToken;

            public Enumerator(DisorderedMultTableDataQuery<T> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._tables = enumerable._tables;
                this._queryModel = enumerable._queryModel;
                this._maxConnectionsPerDatabase = enumerable._maxConnectionsPerDatabase;
                this._maxInItems = enumerable._maxInItems;

                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<T>> CreateEnumerator(bool @async)
            {
                List<PhysicTable> tables = this._tables;
                ShardingQueryModel queryModel = this._queryModel;
                int maxConnectionsPerDatabase = this._maxConnectionsPerDatabase;
                int maxInItems = this._maxInItems;

                TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(T));

                MultTableKeyQuery<T> keyQuery = new MultTableKeyQuery<T>(tables, queryModel, maxConnectionsPerDatabase);

                List<MultTableKeyQueryResult> keyResult = await keyQuery.ToListAsync(this._cancellationToken);

                //构建 in (id1,id2...) 查询

                List<TableDataQueryModel<T>> dataQueries = ShardingHelpers.MakeEntityQueries<T>(queryModel, keyResult, typeDescriptor, maxInItems);

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

                List<OrderProperty> orderList = new List<OrderProperty>(queryModel.Orderings.Count);
                for (int i = 0; i < queryModel.Orderings.Count; i++)
                {
                    var ordering = queryModel.Orderings[i];
                    var valueGetter = MemberGetterContainer.Get(ordering.Member);
                    var orderProperty = new OrderProperty() { Member = ordering.Member, Ascending = ordering.Ascending, ValueGetter = valueGetter };
                    orderList.Add(orderProperty);
                }

                ParallelMergeEnumerable<T> mergeResult = new ParallelMergeEnumerable<T>(dataQueries.Select(a => new OrderedFeatureEnumerable<T>(a.Query, orderList)));

                var enumerator = mergeResult.GetFeatureEnumerator(this._cancellationToken);

                return enumerator;
            }
        }
    }
}

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
    class DynamicDataQueryModel<TEntity>
    {
        public PhysicTable Table { get; set; }
        public DataQueryModel QueryModel { get; set; }

        public DynamicQueryEnumerable<TEntity> Query { get; set; }

        //public List<object> DataList { get; set; }
        public List<object> Keys { get; set; } = new List<object>();
    }

    /// <summary>
    /// 表主键查询
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class MultTableKeyQuery<TEntity> : FeatureEnumerable<MultTableKeyQueryResult>
    {
        List<PhysicTable> _tables;
        ShardingQueryModel _queryModel;
        int _maxConnectionsPerDatabase;

        public MultTableKeyQuery(List<PhysicTable> tables, ShardingQueryModel queryModel, int maxConnectionsPerDatabase)
        {
            this._tables = tables;
            this._queryModel = queryModel;
            this._maxConnectionsPerDatabase = maxConnectionsPerDatabase;
        }

        public override IFeatureEnumerator<MultTableKeyQueryResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<MultTableKeyQueryResult>
        {
            MultTableKeyQuery<TEntity> _enumerable;
            List<PhysicTable> _tables;
            ShardingQueryModel _queryModel;
            int _maxConnectionsPerDatabase;

            CancellationToken _cancellationToken;

            public Enumerator(MultTableKeyQuery<TEntity> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._tables = enumerable._tables;
                this._queryModel = enumerable._queryModel;
                this._maxConnectionsPerDatabase = enumerable._maxConnectionsPerDatabase;

                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<MultTableKeyQueryResult>> CreateEnumerator(bool @async)
            {
                List<PhysicTable> tables = this._tables;
                ShardingQueryModel queryModel = this._queryModel;
                int maxConnectionsPerDatabase = this._maxConnectionsPerDatabase;

                List<Type> dynamicTypeProperties = new List<Type>(2 + queryModel.Orderings.Count);
                TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(TEntity));
                dynamicTypeProperties.Add(typeDescriptor.PrimaryKeys.First().PropertyType);
                dynamicTypeProperties.Add(typeof(int));
                dynamicTypeProperties.AddRange(queryModel.Orderings.Select(a => a.Member.GetMemberType()));

                //new Dynamic() { Id,Ordinal,Order1,Order2... }
                DynamicType dynamicType = DynamicTypeContainer.Get(dynamicTypeProperties);

                List<DynamicDataQueryModel<TEntity>> dataQueries = new List<DynamicDataQueryModel<TEntity>>(tables.Count);
                for (int i = 0; i < tables.Count; i++)
                {
                    var table = tables[i];
                    LambdaExpression selector = ShardingHelpers.MakeDynamicSelector<TEntity>(queryModel, dynamicType, typeDescriptor, i);

                    DynamicDataQueryModel<TEntity> dataQuery = new DynamicDataQueryModel<TEntity>();
                    DataQueryModel dataQueryModel = new DataQueryModel();
                    dataQueryModel.Table = table;
                    dataQueryModel.IgnoreAllFilters = queryModel.IgnoreAllFilters;
                    dataQueryModel.Conditions.AddRange(queryModel.Conditions);
                    dataQueryModel.Orderings.AddRange(queryModel.Orderings);
                    dataQueryModel.Selector = selector;

                    dataQuery.QueryModel = dataQueryModel;
                    dataQuery.Table = table;

                    dataQueries.Add(dataQuery);
                }

                foreach (var group in dataQueries.GroupBy(a => a.Table.DataSource.Name))
                {
                    int count = group.Count();

                    List<IDbContext> dbContexts = ShardingHelpers.CreateDbContexts(group.First().Table.DataSource.DbContextFactory, count, maxConnectionsPerDatabase);
                    ShareDbContextPool dbContextPool = new ShareDbContextPool(dbContexts);

                    bool lazyQuery = dbContexts.Count >= count;

                    foreach (var dataQuery in group)
                    {
                        DynamicQueryEnumerable<TEntity> query = new DynamicQueryEnumerable<TEntity>(dbContextPool, dataQuery.QueryModel, lazyQuery);
                        dataQuery.Query = query;
                    }
                }

                List<OrderProperty> orders = new List<OrderProperty>(queryModel.Orderings.Count);

                for (int i = 0; i < queryModel.Orderings.Count; i++)
                {
                    var ordering = queryModel.Orderings[i];
                    var mapDynamicProperty = dynamicType.Properties[i + 2];
                    var orderProperty = new OrderProperty() { Member = mapDynamicProperty.Property, Ascending = ordering.Ascending, ValueGetter = mapDynamicProperty.Getter };
                    orders.Add(orderProperty);
                }

                ParallelMergeEnumerable<object> mergeEnumerable = new ParallelMergeEnumerable<object>(dataQueries.Select(a => new OrderedFeatureEnumerable<object>(a.Query, orders)));

                MemberGetter keyGetter = dynamicType.GetPrimaryKeyGetter();

                using (var mergeEnumerator = mergeEnumerable.GetFeatureEnumerator(this._cancellationToken))
                {
                    var tableIndexGetter = dynamicType.GetTableIndexGetter();
                    int idx = 0;

                    while (await mergeEnumerator.MoveNextAsync())
                    {
                        object obj = mergeEnumerator.GetCurrent();
                        int tableIndex = (int)tableIndexGetter(obj);
                        var stub = dataQueries[tableIndex];
                        //stub.DataList.Add(obj);

                        object key = keyGetter(obj);
                        stub.Keys.Add(key);
                        idx++;
                    }
                }

                var tableKeyResult = dataQueries.Select(a => new MultTableKeyQueryResult() { Table = a.Table, Keys = a.Keys });

                return new FeatureEnumeratorAdapter<MultTableKeyQueryResult>(tableKeyResult.GetEnumerator());
            }
        }
    }
}

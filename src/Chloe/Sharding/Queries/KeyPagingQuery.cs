using Chloe.Descriptors;
using Chloe.Reflection;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    class DynamicDataQueryPlan
    {
        public DataQueryModel QueryModel { get; set; }

        public ShardTableDataQuery Query { get; set; }

        public List<object> Keys { get; set; } = new List<object>();
    }

    /// <summary>
    /// 表主键查询
    /// </summary>
    internal class KeyPagingQuery : FeatureEnumerable<KeyQueryResult>
    {
        ShardingQueryPlan _queryPlan;

        public KeyPagingQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<KeyQueryResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<KeyQueryResult>
        {
            KeyPagingQuery _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(KeyPagingQuery enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            ShardingQueryPlan QueryPlan { get { return this._enumerable._queryPlan; } }
            IShardingContext ShardingContext { get { return this._enumerable._queryPlan.ShardingContext; } }
            ShardingQueryModel QueryModel { get { return this._enumerable._queryPlan.QueryModel; } }
            List<IPhysicTable> Tables { get { return this._enumerable._queryPlan.Tables; } }
            TypeDescriptor EntityTypeDescriptor { get { return this._enumerable._queryPlan.ShardingContext.TypeDescriptor; } }

            protected override async Task<IFeatureEnumerator<KeyQueryResult>> CreateEnumerator(bool @async)
            {
                DynamicType dynamicType = this.GetDynamicType();
                List<DynamicDataQueryPlan> dataQueryPlans;
                ParallelQueryContext queryContext = new ParallelQueryContext(this.ShardingContext);
                try
                {
                    dataQueryPlans = this.MakeDynamicDataQueryPlans(queryContext, dynamicType);
                    List<OrderProperty> orders = this.MakeOrderings(dynamicType);
                    await this.ExecuteQuery(queryContext, dataQueryPlans, dynamicType, orders);
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }

                var tableKeyResult = dataQueryPlans.Select(a => new KeyQueryResult() { Table = a.QueryModel.Table, Result = a.Keys });

                return new FeatureEnumeratorAdapter<KeyQueryResult>(tableKeyResult.GetEnumerator());

            }

            DynamicType GetDynamicType()
            {
                List<Type> dynamicTypeProperties = new List<Type>(2 + this.QueryModel.Orderings.Count);

                dynamicTypeProperties.Add(this.ShardingContext.TypeDescriptor.PrimaryKeys.First().PropertyType);
                dynamicTypeProperties.Add(typeof(int));
                dynamicTypeProperties.AppendRange(this.QueryModel.Orderings.Select(a => a.KeySelector.Body.Type));

                //new Dynamic() { Id,Ordinal,Order1,Order2... }
                DynamicType dynamicType = DynamicTypeContainer.Get(dynamicTypeProperties);

                return dynamicType;
            }
            List<DynamicDataQueryPlan> MakeDynamicDataQueryPlans(ParallelQueryContext queryContext, DynamicType dynamicType)
            {
                List<DynamicDataQueryPlan> dataQueryPlans = new List<DynamicDataQueryPlan>(this.Tables.Count);
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    var table = this.Tables[i];
                    LambdaExpression selector = ShardingHelpers.MakeDynamicSelector(this.QueryPlan, dynamicType, this.EntityTypeDescriptor, i);
                    DataQueryModel dataQueryModel = ShardingHelpers.MakeDataQueryModel(table, this.QueryPlan.QueryModel);
                    dataQueryModel.Selector = selector;

                    DynamicDataQueryPlan dataQueryPlan = new DynamicDataQueryPlan();
                    dataQueryPlan.QueryModel = dataQueryModel;

                    dataQueryPlans.Add(dataQueryPlan);
                }

                foreach (var group in dataQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    ISharedDbContextProviderPool dbContextProviderPool = queryContext.GetDbContextProviderPool(group.First().QueryModel.Table.DataSource);
                    bool lazyQuery = dbContextProviderPool.Size >= count;

                    foreach (var dataQuery in group)
                    {
                        ShardTableDataQuery shardTableQuery = new ShardTableDataQuery(queryContext, dbContextProviderPool, dataQuery.QueryModel, lazyQuery);
                        dataQuery.Query = shardTableQuery;
                    }
                }

                return dataQueryPlans;
            }

            List<OrderProperty> MakeOrderings(DynamicType dynamicType)
            {
                var queryModel = this._enumerable._queryPlan.QueryModel;

                List<OrderProperty> orders = new List<OrderProperty>(queryModel.Orderings.Count);

                for (int i = 0; i < queryModel.Orderings.Count; i++)
                {
                    var ordering = queryModel.Orderings[i];
                    var mapDynamicProperty = dynamicType.Properties[i + 2];
                    var orderProperty = new OrderProperty() { Ascending = ordering.Ascending, ValueGetter = mapDynamicProperty.Getter };
                    orders.Add(orderProperty);
                }

                return orders;
            }

            async Task ExecuteQuery(ParallelQueryContext queryContext, List<DynamicDataQueryPlan> dataQueryPlans, DynamicType dynamicType, List<OrderProperty> orders)
            {
                ParallelMergeOrderedEnumerable<object> mergeEnumerable = new ParallelMergeOrderedEnumerable<object>(queryContext, dataQueryPlans.Select(a => new OrderedFeatureEnumerable<object>(a.Query, orders)));

                MemberGetter keyGetter = dynamicType.GetPrimaryKeyGetter();

                IAsyncEnumerable<object> asyncEnumerable = mergeEnumerable;

                if (this.QueryModel.Skip != null)
                {
                    asyncEnumerable = asyncEnumerable.Skip(this.QueryModel.Skip.Value);
                }
                if (this.QueryModel.Take != null)
                {
                    asyncEnumerable = asyncEnumerable.Take(this.QueryModel.Take.Value);
                }

                var tableIndexGetter = dynamicType.GetTableIndexGetter();
                await asyncEnumerable.ForEach(obj =>
                {
                    int tableIndex = (int)tableIndexGetter(obj);
                    object key = keyGetter(obj);

                    var stub = dataQueryPlans[tableIndex];
                    stub.Keys.Add(key);

                }, this._cancellationToken);
            }
        }
    }
}

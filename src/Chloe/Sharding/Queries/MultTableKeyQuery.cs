using Chloe.Descriptors;
using Chloe.Reflection;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Chloe.Sharding.Queries
{
    class DynamicDataQueryPlan<TEntity>
    {
        public DataQueryModel QueryModel { get; set; }

        public DynamicModelQuery<TEntity> Query { get; set; }

        //public List<object> DataList { get; set; }
        public List<object> Keys { get; set; } = new List<object>();
    }

    /// <summary>
    /// 表主键查询
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class MultTableKeyQuery<TEntity> : FeatureEnumerable<MultTableKeyQueryResult>
    {
        ShardingQueryPlan _queryPlan;

        public MultTableKeyQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<MultTableKeyQueryResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<MultTableKeyQueryResult>
        {
            MultTableKeyQuery<TEntity> _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(MultTableKeyQuery<TEntity> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            ShardingQueryPlan QueryPlan { get { return this._enumerable._queryPlan; } }
            IShardingContext ShardingContext { get { return this._enumerable._queryPlan.ShardingContext; } }
            ShardingQueryModel QueryModel { get { return this._enumerable._queryPlan.QueryModel; } }
            List<RouteTable> Tables { get { return this._enumerable._queryPlan.RouteTables; } }
            TypeDescriptor EntityTypeDescriptor { get { return this._enumerable._queryPlan.ShardingContext.TypeDescriptor; } }

            protected override async Task<IFeatureEnumerator<MultTableKeyQueryResult>> CreateEnumerator(bool @async)
            {
                ParallelQueryContext queryContext = new ParallelQueryContext();

                DynamicType dynamicType = this.GetDynamicType();
                try
                {
                    List<DynamicDataQueryPlan<TEntity>> dataQueryPlans = this.MakeDynamicDataQueryPlans(queryContext, dynamicType);
                    List<OrderProperty> orders = this.MakeOrderings(dynamicType);

                    await this.ExecuteQuery(queryContext, dataQueryPlans, dynamicType, orders);

                    var tableKeyResult = dataQueryPlans.Select(a => new MultTableKeyQueryResult() { Table = a.QueryModel.Table, Keys = a.Keys });

                    return new FeatureEnumeratorAdapter<MultTableKeyQueryResult>(tableKeyResult.GetEnumerator());
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }

            DynamicType GetDynamicType()
            {
                List<Type> dynamicTypeProperties = new List<Type>(2 + this.QueryModel.Orderings.Count);

                dynamicTypeProperties.Add(this.ShardingContext.TypeDescriptor.PrimaryKeys.First().PropertyType);
                dynamicTypeProperties.Add(typeof(int));
                dynamicTypeProperties.AddRange(this.QueryModel.Orderings.Select(a => a.Member.GetMemberType()));

                //new Dynamic() { Id,Ordinal,Order1,Order2... }
                DynamicType dynamicType = DynamicTypeContainer.Get(dynamicTypeProperties);

                return dynamicType;
            }
            List<DynamicDataQueryPlan<TEntity>> MakeDynamicDataQueryPlans(ParallelQueryContext queryContext, DynamicType dynamicType)
            {
                List<DynamicDataQueryPlan<TEntity>> dataQueryPlans = new List<DynamicDataQueryPlan<TEntity>>(this.Tables.Count);
                for (int i = 0; i < this.Tables.Count; i++)
                {
                    var table = this.Tables[i];
                    LambdaExpression selector = ShardingHelpers.MakeDynamicSelector<TEntity>(this.QueryPlan, dynamicType, this.EntityTypeDescriptor, i);
                    DataQueryModel dataQueryModel = ShardingHelpers.MakeDataQueryModel(table, this.QueryPlan.QueryModel);
                    dataQueryModel.Selector = selector;

                    DynamicDataQueryPlan<TEntity> dataQueryPlan = new DynamicDataQueryPlan<TEntity>();
                    dataQueryPlan.QueryModel = dataQueryModel;

                    dataQueryPlans.Add(dataQueryPlan);
                }

                foreach (var group in dataQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    ShareDbContextPool dbContextPool = ShardingHelpers.CreateDbContextPool(group.First().QueryModel.Table.DataSource.DbContextFactory, count, this.QueryPlan.ShardingContext.MaxConnectionsPerDatabase);
                    queryContext.AddManagedResource(dbContextPool);

                    bool lazyQuery = dbContextPool.Size >= count;

                    foreach (var dataQuery in group)
                    {
                        DynamicModelQuery<TEntity> query = new DynamicModelQuery<TEntity>(dbContextPool, dataQuery.QueryModel, lazyQuery);
                        dataQuery.Query = query;
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
                    var orderProperty = new OrderProperty() { Member = mapDynamicProperty.Property, Ascending = ordering.Ascending, ValueGetter = mapDynamicProperty.Getter };
                    orders.Add(orderProperty);
                }

                return orders;
            }

            async Task ExecuteQuery(ParallelQueryContext queryContext, List<DynamicDataQueryPlan<TEntity>> dataQueryPlans, DynamicType dynamicType, List<OrderProperty> orders)
            {
                ParallelMergeEnumerable<object> mergeEnumerable = new ParallelMergeEnumerable<object>(queryContext, dataQueryPlans.Select(a => new OrderedFeatureEnumerable<object>(a.Query, orders)));

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
                    //stub.DataList.Add(obj);

                    //Console.WriteLine($"key: {key} index: {tableIndex} skips: {skips} {stub.QueryModel.Table.Name} {stub.QueryModel.Table.DataSource.Name}");

                }, this._cancellationToken);
            }
        }
    }
}

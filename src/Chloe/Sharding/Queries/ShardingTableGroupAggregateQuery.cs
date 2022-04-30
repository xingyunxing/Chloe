using Chloe.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    class GroupAggregateQueryModel
    {
        public GroupAggregateQueryModel(Type rootEntityType)
        {
            this.RootEntityType = rootEntityType;
        }
        public Type RootEntityType { get; set; }
        public IPhysicTable Table { get; set; }
        public List<LambdaExpression> Conditions { get; set; } = new List<LambdaExpression>();
        public List<LambdaExpression> GroupKeySelectors { get; set; } = new List<LambdaExpression>();
        public LambdaExpression Selector { get; set; }
    }

    internal class ShardingTableGroupAggregateQuery : FeatureEnumerable<object>
    {
        IShareDbContextPool DbContextPool;
        GroupAggregateQueryModel QueryModel;
        bool LazyQuery;

        public ShardingTableGroupAggregateQuery(IShareDbContextPool dbContextPool, GroupAggregateQueryModel queryModel, bool lazyQuery)
        {
            this.DbContextPool = dbContextPool;
            this.QueryModel = queryModel;
            this.LazyQuery = lazyQuery;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : QueryEnumerator<object>
        {
            ShardingTableGroupAggregateQuery _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(ShardingTableGroupAggregateQuery enumerable, CancellationToken cancellationToken = default) : base(enumerable.DbContextPool)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)> CreateQuery(IDbContext dbContext, bool @async)
            {
                var q = this.MakeGroupAggregateQuery(dbContext);

                if (!this._enumerable.LazyQuery)
                {
                    IEnumerable dataList = null;
                    if (@async)
                    {
                        dataList = await q.ToListAsync();
                    }
                    else
                    {
                        dataList = q.ToList();
                    }

                    return (new FeatureEnumerableAdapter<object>(dataList), false);
                }

                var lazyEnumerable = q.FastInvokeMethod(nameof(IQuery<object>.AsEnumerable)) as IEnumerable;
                return (new FeatureEnumerableAdapter<object>(lazyEnumerable), true);
            }

            IQuery MakeGroupAggregateQuery(IDbContext dbContext)
            {
                GroupAggregateQueryModel queryModel = this._enumerable.QueryModel;
                var method = this.GetType().GetMethod(nameof(Enumerator.MakeTypedGroupAggregateQuery), BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).MakeGenericMethod(queryModel.RootEntityType);
                var query = (IQuery)method.FastInvoke(null, dbContext, queryModel);
                return query;
            }

            static IQuery MakeTypedGroupAggregateQuery<T>(IDbContext dbContext, GroupAggregateQueryModel queryModel)
            {
                var query = dbContext.Query<T>(queryModel.Table.Name);

                foreach (var condition in queryModel.Conditions)
                {
                    query = query.Where((Expression<Func<T, bool>>)condition);
                }

                object groupQuery = null;
                foreach (var groupKeySelector in queryModel.GroupKeySelectors)
                {
                    if (groupQuery == null)
                    {
                        var groupMethod = typeof(IQuery<T>).GetMethod(nameof(IQuery<object>.GroupBy)).MakeGenericMethod(groupKeySelector.Body.Type);
                        groupQuery = groupMethod.FastInvoke(query, groupKeySelector);
                        continue;
                    }

                    var andByMethod = groupQuery.GetType().GetMethod(nameof(IGroupingQuery<object>.AndBy)).MakeGenericMethod(groupKeySelector.Body.Type);
                    groupQuery = andByMethod.FastInvoke(groupQuery, groupKeySelector);
                }

                var selectMethod = groupQuery.GetType().GetMethod(nameof(IGroupingQuery<object>.Select)).MakeGenericMethod(queryModel.Selector.Body.Type);

                var q = (IQuery)selectMethod.FastInvoke(groupQuery, queryModel.Selector);
                return q;
            }
        }
    }
}

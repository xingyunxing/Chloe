using System.Collections;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    class DynamicModelQuery : FeatureEnumerable<object>
    {
        IShareDbContextPool DbContextPool;
        DataQueryModel QueryModel;
        bool LazyQuery;

        public DynamicModelQuery(IShareDbContextPool dbContextPool, DataQueryModel queryModel, bool lazyQuery)
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
            DynamicModelQuery _enumerable;

            public Enumerator(DynamicModelQuery enumerable, CancellationToken cancellationToken) : base(enumerable.DbContextPool, cancellationToken)
            {
                this._enumerable = enumerable;
            }

            protected override async Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)> CreateQuery(IDbContext dbContext, bool @async)
            {
                var query = ShardingHelpers.MakeQuery(dbContext, this._enumerable.QueryModel, false);

                if (!this._enumerable.LazyQuery)
                {
                    IEnumerable dataList = null;
                    if (@async)
                    {
                        dataList = await query.ToListAsync();
                    }
                    else
                    {
                        dataList = query.ToList();
                    }

                    return (new FeatureEnumerableAdapter<object>(dataList), false);
                }

                var lazyEnumerable = query.AsEnumerable();
                return (new FeatureEnumerableAdapter<object>(lazyEnumerable), true);
            }
        }
    }
}

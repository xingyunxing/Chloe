﻿using Chloe.Reflection;
using System.Collections;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    class DynamicModelQuery<TEntity> : FeatureEnumerable<object>
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
            DynamicModelQuery<TEntity> _enumerable;

            public Enumerator(DynamicModelQuery<TEntity> enumerable, CancellationToken cancellationToken) : base(enumerable.DbContextPool, cancellationToken)
            {
                this._enumerable = enumerable;
            }

            protected override async Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)> CreateQuery(IDbContext dbContext, bool @async)
            {
                var query = ShardingHelpers.MakeQueryWithoutSkipAndTake<TEntity>(dbContext, this._enumerable.QueryModel);

                var q = this.MakeDynamicQuery(query);

                if (!this._enumerable.LazyQuery)
                {
                    IEnumerable dataList = null;
                    if (@async)
                    {
                        var task = (Task)q.FastInvokeMethod(nameof(IQuery<object>.ToListAsync));
                        await task;
                        dataList = (IEnumerable)task.GetType().GetProperty(nameof(Task<object>.Result)).FastGetMemberValue(task);
                    }
                    else
                    {
                        dataList = (IEnumerable)q.FastInvokeMethod(nameof(IQuery<object>.ToList));
                    }

                    return (new FeatureEnumerableAdapter<object>(dataList), false);
                }

                var lazyEnumerable = q.FastInvokeMethod(nameof(IQuery<object>.AsEnumerable)) as IEnumerable;
                return (new FeatureEnumerableAdapter<object>(lazyEnumerable), true);
            }

            object MakeDynamicQuery(IQuery<TEntity> q)
            {
                var queryModel = this._enumerable.QueryModel;

                var queryType = typeof(IQuery<TEntity>);
                var selectMethod = queryType.GetMethod(nameof(IQuery<object>.Select)).MakeGenericMethod(queryModel.Selector.ReturnType);
                var query = selectMethod.FastInvoke(q, queryModel.Selector);
                if (queryModel.Skip != null)
                {
                    query = query.FastInvokeMethod(nameof(IQuery<object>.Skip), queryModel.Skip.Value);
                }
                if (queryModel.Take != null)
                {
                    query = query.FastInvokeMethod(nameof(IQuery<object>.Take), queryModel.Take.Value);
                }

                return query;
            }
        }
    }
}

﻿using System.Threading;

namespace Chloe.Sharding.Queries
{
    class SingleTableAnyQuery<T> : FeatureEnumerable<bool>
    {
        IParallelQueryContext _queryContext;
        IShareDbContextPool _dbContextPool;
        DataQueryModel _queryModel;

        public SingleTableAnyQuery(IParallelQueryContext queryContext, IShareDbContextPool dbContextPool, DataQueryModel queryModel)
        {
            this._queryContext = queryContext;
            this._dbContextPool = dbContextPool;
            this._queryModel = queryModel;
        }

        public override IFeatureEnumerator<bool> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : TableQueryEnumerator<bool>
        {
            SingleTableAnyQuery<T> _enumerable;

            public Enumerator(SingleTableAnyQuery<T> enumerable, CancellationToken cancellationToken = default) : base(enumerable._dbContextPool, enumerable._queryModel, cancellationToken)
            {
                this._enumerable = enumerable;
            }

            protected override async Task<(IFeatureEnumerable<bool> Query, bool IsLazyQuery)> CreateQuery(IQuery query, bool async)
            {
                var queryContext = this._enumerable._queryContext;

                bool canceled = queryContext.BeforeExecuteCommand();
                if (canceled)
                {
                    return (NullFeatureEnumerable<bool>.Instance, false);
                }

                var q = (IQuery<T>)query;
                bool hasData = @async ? await q.AnyAsync() : q.Any();

                queryContext.AfterExecuteCommand(hasData);

                var featureEnumerable = new ScalarFeatureEnumerable<bool>(hasData);
                return (featureEnumerable, false);
            }
        }
    }
}
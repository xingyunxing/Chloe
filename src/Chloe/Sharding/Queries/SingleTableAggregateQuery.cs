using Chloe.Threading.Tasks;
using System.Collections;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    class SingleTableAggregateQuery<T, TResult> : FeatureEnumerable<TResult>
    {
        IShareDbContextPool _dbContextPool;
        DataQueryModel _queryModel;
        Func<IQuery<T>, bool, Task<TResult>> _executor;

        public SingleTableAggregateQuery(IShareDbContextPool dbContextPool, DataQueryModel queryModel, Func<IQuery<T>, bool, Task<TResult>> executor)
        {
            this._dbContextPool = dbContextPool;
            this._queryModel = queryModel;
            this._executor = executor;
        }

        public override IFeatureEnumerator<TResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<TResult>
        {
            SingleTableAggregateQuery<T, TResult> _enumerable;
            CancellationToken _cancellationToken;
            bool _hasCompleted;
            TResult Result = default;

            public Enumerator(SingleTableAggregateQuery<T, TResult> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            public TResult Current => this.Result;

            object IEnumerator.Current => this.Result;

            public void Dispose()
            {

            }

            public ValueTask DisposeAsync()
            {
                return default;
            }

            public bool MoveNext()
            {
                return this.MoveNext(false).GetResult();
            }

            public BoolResultTask MoveNextAsync()
            {
                return this.MoveNext(true);
            }

            async BoolResultTask MoveNext(bool @async)
            {
                if (this._hasCompleted)
                {
                    this.Result = default;
                    return false;
                }

                using var poolResource = await this._enumerable._dbContextPool.GetOne(@async);

                var dbContext = poolResource.Resource;
                var q = dbContext.Query<T>(this._enumerable._queryModel.Table.Name);

                foreach (var condition in this._enumerable._queryModel.Conditions)
                {
                    q = q.Where((Expression<Func<T, bool>>)condition);
                }

                if (this._enumerable._queryModel.IgnoreAllFilters)
                {
                    q = q.IgnoreAllFilters();
                }

                this.Result = await this._enumerable._executor(q, @async);
                this._hasCompleted = true;

                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}

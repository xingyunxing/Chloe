using Chloe.Threading.Tasks;
using System.Collections;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 获取单个表的数据量
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SingleTableCountQuery<T> : FeatureEnumerable<long>
    {
        IShareDbContextPool _dbContextPool;
        DataQueryModel _queryModel;

        public SingleTableCountQuery(IShareDbContextPool dbContextPool, DataQueryModel queryModel)
        {
            this._dbContextPool = dbContextPool;
            this._queryModel = queryModel;
        }

        public override IFeatureEnumerator<long> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<long>
        {
            SingleTableCountQuery<T> _enumerable;
            CancellationToken _cancellationToken;
            long Result = -1;

            public Enumerator(SingleTableCountQuery<T> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            public long Current => this.Result;

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
                if (this.Result != -1)
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

                long count = @async ? await q.LongCountAsync() : q.LongCount();
                this.Result = count;

                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}

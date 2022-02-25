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
    /// 获取单个表的数据量
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SingleTableCountQuery<T> : FeatureEnumerable<int>
    {
        IShareDbContextPool DbContextPool;
        DataQueryModel QueryModel;

        public SingleTableCountQuery(IShareDbContextPool dbContextPool, DataQueryModel queryModel)
        {
            this.DbContextPool = dbContextPool;
            this.QueryModel = queryModel;
        }

        public override IFeatureEnumerator<int> GetFeatureEnumerator()
        {
            return this.GetFeatureEnumerator(default(CancellationToken));
        }

        public override IFeatureEnumerator<int> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this);
        }

        class Enumerator : IFeatureEnumerator<int>
        {
            IShareDbContextPool DbContextPool;
            DataQueryModel QueryModel;
            int Result = -1;

            public Enumerator(SingleTableCountQuery<T> enumerable)
            {
                this.DbContextPool = enumerable.DbContextPool;
                this.QueryModel = enumerable.QueryModel;
            }

            public int Current => this.Result;

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

                using var poolResource = await this.DbContextPool.GetOne(@async);


                var dbContext = poolResource.Resource;
                var q = dbContext.Query<T>(this.QueryModel.Table.Name);

                foreach (var condition in this.QueryModel.Conditions)
                {
                    q = q.Where((Expression<Func<T, bool>>)condition);
                }

                if (this.QueryModel.IgnoreAllFilters)
                {
                    q = q.IgnoreAllFilters();
                }

                var count = @async ? await q.CountAsync() : q.Count();
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

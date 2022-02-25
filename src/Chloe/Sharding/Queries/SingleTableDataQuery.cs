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
    /// 单表数据查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SingleTableDataQuery<T> : FeatureEnumerable<T>
    {
        IShareDbContextPool DbContextPool;
        DataQueryModel QueryModel;
        bool LazyQuery;

        public SingleTableDataQuery(IShareDbContextPool dbContextPool, DataQueryModel queryModel, bool lazyQuery)
        {
            this.DbContextPool = dbContextPool;
            this.QueryModel = queryModel;
            this.LazyQuery = lazyQuery;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator()
        {
            return this.GetFeatureEnumerator(default(CancellationToken));
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this);
        }

        class Enumerator : IFeatureEnumerator<T>
        {
            IShareDbContextPool DbContextPool;
            DataQueryModel QueryModel;
            bool LazyQuery;

            IFeatureEnumerator<T> _enumerator;
            IPoolResource<IDbContext> PoolResource;

            public Enumerator(SingleTableDataQuery<T> enumerable)
            {
                this.DbContextPool = enumerable.DbContextPool;
                this.QueryModel = enumerable.QueryModel;
                this.LazyQuery = enumerable.LazyQuery;
            }

            public T Current => (this._enumerator as IEnumerator<T>).Current;

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                this.PoolResource?.Dispose();
            }

            public ValueTask DisposeAsync()
            {
                this.PoolResource?.Dispose();
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

            async ValueTask LazyInit(bool @async)
            {
                if (this._enumerator == null)
                {
                    IPoolResource<IDbContext> poolResource = await this.DbContextPool.GetOne(@async);
                    this.PoolResource = poolResource;
                    try
                    {
                        var q = this.MakeQuery(poolResource.Resource);

                        if (!this.LazyQuery)
                        {
                            var dataList = @async ? await q.ToListAsync() : q.ToList();
                            this._enumerator = new FeatureEnumeratorAdapter<T>(dataList.GetEnumerator());
                        }
                        else
                        {
                            this._enumerator = new FeatureEnumeratorAdapter<T>(q.AsEnumerable().GetEnumerator());

                        }
                    }
                    finally
                    {
                        if (!this.LazyQuery)
                        {
                            poolResource.Dispose();
                        }
                    }
                }
            }
            async BoolResultTask MoveNext(bool @async)
            {
                await this.LazyInit(@async);
                return @async ? await this._enumerator.MoveNextAsync() : this._enumerator.MoveNext();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            IQuery<T> MakeQuery(IDbContext dbContext)
            {
                var queryModel = this.QueryModel;
                var q = dbContext.Query<T>(queryModel.Table.Name);

                //var whereMethod = q.GetType().GetMethod(nameof(IQuery<int>.Where));

                foreach (var condition in queryModel.Conditions)
                {
                    q = q.Where((Expression<Func<T, bool>>)condition);
                    //q = whereMethod.FastInvoke(q, condition);
                }

                if (queryModel.IgnoreAllFilters)
                {
                    q = q.IgnoreAllFilters();
                }

                IOrderedQuery<T> orderedQuery = null;
                foreach (var ordering in queryModel.Orderings)
                {
                    if (orderedQuery == null)
                        orderedQuery = q.InnerOrderBy(ordering);
                    else
                        orderedQuery = orderedQuery.InnerThenBy(ordering);

                    q = orderedQuery;
                }

                if (queryModel.Skip != null)
                {
                    q = q.Skip(queryModel.Skip.Value);
                }
                if (queryModel.Take != null)
                {
                    q = q.Take(queryModel.Take.Value);
                }

                return q;
            }
        }
    }
}

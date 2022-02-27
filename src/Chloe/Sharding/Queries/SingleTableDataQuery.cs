using Chloe.Threading.Tasks;
using System.Collections;
using System.Linq.Expressions;
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
        IParallelQueryContext QueryContext;
        IShareDbContextPool DbContextPool;
        DataQueryModel QueryModel;
        bool LazyQuery;

        public SingleTableDataQuery(IParallelQueryContext queryContext, IShareDbContextPool dbContextPool, DataQueryModel queryModel, bool lazyQuery)
        {
            this.QueryContext = queryContext;
            this.DbContextPool = dbContextPool;
            this.QueryModel = queryModel;
            this.LazyQuery = lazyQuery;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this);
        }

        class Enumerator : IFeatureEnumerator<T>
        {
            SingleTableDataQuery<T> _enumerable;

            IFeatureEnumerator<T> _enumerator;
            IPoolItem<IDbContext> _poolItem;

            public Enumerator(SingleTableDataQuery<T> enumerable)
            {
                this._enumerable = enumerable;
            }

            public T Current => (this._enumerator as IEnumerator<T>).Current;

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                this.Dispose(false).GetResult();
            }

            public ValueTask DisposeAsync()
            {
                return this.Dispose(true);
            }

            async ValueTask Dispose(bool @async)
            {
                this._poolItem?.Dispose();
                if (@async)
                {
                    if (this._enumerator != null)
                    {
                        await this._enumerator.DisposeAsync();
                    }
                }
                else
                {
                    this._enumerator.Dispose();
                }
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
                    IPoolItem<IDbContext> poolItem = await this._enumerable.DbContextPool.GetOne(@async);

                    this._poolItem = poolItem;
                    try
                    {
                        var q = this.MakeQuery(poolItem.Resource);

                        bool canceled = this._enumerable.QueryContext.BeforeExecuteCommand();
                        if (canceled)
                        {
                            this._enumerator = new NullFeatureEnumerator<T>();
                            poolItem.Dispose();
                            return;
                        }

                        if (this._enumerable.LazyQuery)
                        {
                            var en = q.AsEnumerable();
                            this._enumerable.QueryContext.AfterExecuteCommand(en);
                            this._enumerator = new FeatureEnumeratorAdapter<T>(en.GetEnumerator());
                        }
                        else
                        {

                            var dataList = @async ? await q.ToListAsync() : q.ToList();
                            this._enumerable.QueryContext.AfterExecuteCommand(dataList);
                            this._enumerator = new FeatureEnumeratorAdapter<T>(dataList.GetEnumerator());
                        }
                    }
                    finally
                    {
                        if (!this._enumerable.LazyQuery)
                        {
                            poolItem.Dispose();
                            this._poolItem = null;
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
                var queryModel = this._enumerable.QueryModel;
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

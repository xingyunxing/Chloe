using Chloe.Reflection;
using Chloe.Threading.Tasks;
using System.Collections;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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
            return new Enumerator(this);
        }

        class Enumerator : IFeatureEnumerator<object>
        {
            DynamicModelQuery<TEntity> _enumerable;

            DynamicFeatureEnumerator _enumerator;
            IPoolItem<IDbContext> _poolItem;

            public Enumerator(DynamicModelQuery<TEntity> enumerable)
            {
                this._enumerable = enumerable;
            }

            public object Current => this._enumerator.GetCurrent();

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                this.Dispose(false).GetResult();
            }

            public async ValueTask DisposeAsync()
            {
                await this.Dispose(true);
            }
            async ValueTask Dispose(bool @async)
            {
                this._poolItem?.Dispose();

                if (this._enumerator != null)
                {
                    await this._enumerator.Dispose(@async);
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
                if (this._enumerator != null)
                {
                    return;
                }

                IPoolItem<IDbContext> poolItem = await this._enumerable.DbContextPool.GetOne(@async);
                this._poolItem = poolItem;
                try
                {
                    var q = this.MakeQuery(poolItem.Resource);

                    if (this._enumerable.LazyQuery)
                    {
                        object enumerator = (q.FastInvokeMethod(nameof(IQuery<int>.AsEnumerable)) as IEnumerable).GetEnumerator();
                        this._enumerator = new DynamicFeatureEnumerator(enumerator);
                    }
                    else
                    {
                        IEnumerable dataList = null;
                        if (@async)
                        {
                            var task = (Task)q.FastInvokeMethod(nameof(IQuery<int>.ToListAsync));
                            await task;
                            dataList = (IEnumerable)task.GetType().GetProperty(nameof(Task<int>.Result)).FastGetMemberValue(task);
                        }
                        else
                        {
                            dataList = (IEnumerable)q.FastInvokeMethod(nameof(IQuery<int>.ToList));
                        }

                        this._enumerator = new DynamicFeatureEnumerator(dataList.GetEnumerator());
                    }
                }
                finally
                {
                    if (!this._enumerable.LazyQuery)
                    {
                        poolItem.Dispose();
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

            object MakeQuery(IDbContext dbContext)
            {
                var queryModel = this._enumerable.QueryModel;
                var q = dbContext.Query<TEntity>(queryModel.Table.Name);

                //var whereMethod = q.GetType().GetMethod(nameof(IQuery<int>.Where));

                foreach (var condition in queryModel.Conditions)
                {
                    q = q.Where((Expression<Func<TEntity, bool>>)condition);
                    //q = whereMethod.FastInvoke(q, condition);
                }

                if (queryModel.IgnoreAllFilters)
                {
                    q = q.IgnoreAllFilters();
                }

                IOrderedQuery<TEntity> orderedQuery = null;
                foreach (var ordering in queryModel.Orderings)
                {
                    if (orderedQuery == null)
                        orderedQuery = q.InnerOrderBy(ordering);
                    else
                        orderedQuery = orderedQuery.InnerThenBy(ordering);

                    q = orderedQuery;
                }

                var queryType = typeof(IQuery<TEntity>);
                var selectMethod = queryType.GetMethod(nameof(IQuery<int>.Select)).MakeGenericMethod(queryModel.Selector.ReturnType);
                var query = selectMethod.FastInvoke(q, queryModel.Selector);
                if (queryModel.Skip != null)
                {
                    query = query.FastInvokeMethod(nameof(IQuery<int>.Skip), queryModel.Skip.Value);
                }
                if (queryModel.Take != null)
                {
                    query = query.FastInvokeMethod(nameof(IQuery<int>.Take), queryModel.Take.Value);
                }

                return query;
            }
        }
    }
}

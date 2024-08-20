using Chloe.Query.SplitQuery;
using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Chloe.Query.Internals
{
    class InternalSplitQuery<T> : FeatureEnumerable<T>, IEnumerable<T>, IAsyncEnumerable<T>
    {
        Query<T> _query;
        QueryContext _queryContext;

        internal InternalSplitQuery(Query<T> query)
        {
            this._query = query;
            this._queryContext = new QueryContext(this._query.DbContextProvider);
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new SplitQueryEnumerator(this);
        }

        RootQueryExecutor CreateQueryExecutor()
        {
            SplitQueryRootNode queryRootNode = SplitQueryNodeCollector.GetSplitQueryNode(this._query.QueryExpression);
            RootQueryExecutor queryExecutor = queryRootNode.CreateQueryExecutor(this._queryContext);
            return queryExecutor;
        }

        class SplitQueryEnumerator : IFeatureEnumerator<T>
        {
            bool _disposed;
            InternalSplitQuery<T> _splitQuery;
            IEnumerator<object> _entityEnumerator;

            public SplitQueryEnumerator(InternalSplitQuery<T> splitQuery)
            {
                this._splitQuery = splitQuery;
            }

            public T Current => (T)this._entityEnumerator.Current;

            object IEnumerator.Current => this._entityEnumerator.Current;

            public void Dispose()
            {
                if (this._disposed)
                    return;

                if (this._entityEnumerator != null)
                    this._entityEnumerator.Dispose();

                this._disposed = true;
            }

            public async ValueTask DisposeAsync()
            {
                this.Dispose();
            }

            public bool MoveNext()
            {
                return this.MoveNext(false).GetResult();
            }

            public BoolResultTask MoveNextAsync()
            {
                return this.MoveNext(true);
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            async BoolResultTask MoveNext(bool @async)
            {
                if (this._disposed)
                    return false;

                if (this._entityEnumerator == null)
                {
                    RootQueryExecutor queryExecutor = this._splitQuery.CreateQueryExecutor();
                    await queryExecutor.Execute(@async);
                    this._entityEnumerator = queryExecutor.Entities.GetEnumerator();
                }

                return this._entityEnumerator.MoveNext();
            }
        }
    }
}

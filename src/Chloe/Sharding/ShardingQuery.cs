using Chloe.Query;
using Chloe.Sharding.Queries;
using Chloe.Threading.Tasks;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    internal partial class ShardingQuery<T> : IQuery<T>
    {
        internal Query<T> InnerQuery { get; set; }

        public ShardingQuery(IDbContextInternal dbContext, string explicitTable, LockType @lock)
           : this(new Query<T>(dbContext, explicitTable, @lock))
        {
        }
        public ShardingQuery(Query<T> query)
        {
            this.InnerQuery = query;
        }
        public ShardingQuery(IQuery<T> query) : this((Query<T>)query)
        {

        }

        ShardingQuery<T> AsShardingQuery(IQuery<T> query)
        {
            return query as ShardingQuery<T>;
        }

        Type IQuery.ElementType { get { return typeof(T); } }

        public bool Any()
        {
            return this.AnyAsync().GetResult();
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return this.AnyAsync(predicate).GetResult();
        }

        public Task<bool> AnyAsync()
        {
            return this.QueryAny();
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).AnyAsync();
        }

        public IEnumerable<T> AsEnumerable()
        {
            return this.Execute().GetResult();
        }

        public IQuery<T> AsTracking()
        {
            return new ShardingQuery<T>(this.InnerQuery.AsTracking());
        }

        public double? Average(Expression<Func<T, int>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public double? Average(Expression<Func<T, int?>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public double? Average(Expression<Func<T, long>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public double? Average(Expression<Func<T, long?>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public decimal? Average(Expression<Func<T, decimal>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public double? Average(Expression<Func<T, double>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public double? Average(Expression<Func<T, double?>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public float? Average(Expression<Func<T, float>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public float? Average(Expression<Func<T, float?>> selector)
        {
            return this.AverageAsync(selector).GetResult();
        }

        public async Task<double?> AverageAsync(Expression<Func<T, int>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return (double?)avg;
        }

        public async Task<double?> AverageAsync(Expression<Func<T, int?>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return (double?)avg;
        }

        public async Task<double?> AverageAsync(Expression<Func<T, long>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return (double?)avg;
        }

        public async Task<double?> AverageAsync(Expression<Func<T, long?>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return (double?)avg;
        }

        public async Task<decimal?> AverageAsync(Expression<Func<T, decimal>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return avg;
        }

        public async Task<decimal?> AverageAsync(Expression<Func<T, decimal?>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return avg;
        }

        public async Task<double?> AverageAsync(Expression<Func<T, double>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return (double?)avg;
        }

        public async Task<double?> AverageAsync(Expression<Func<T, double?>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return (double?)avg;
        }

        public async Task<float?> AverageAsync(Expression<Func<T, float>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return (float?)avg;
        }

        public async Task<float?> AverageAsync(Expression<Func<T, float?>> selector)
        {
            var avg = await this.QueryAverageAsync(selector);
            return (float?)avg;
        }

        public IQuery<T> Distinct()
        {
            throw new NotImplementedException();
        }

        public T First()
        {
            return this.Take(1).AsEnumerable().First();
        }

        public T First(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).First();
        }

        public async Task<T> FirstAsync()
        {
            var q = this.Take(1) as ShardingQuery<T>;
            return await (await q.Execute()).FirstAsync();
        }

        public Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).FirstAsync();
        }

        public T FirstOrDefault()
        {
            return this.Take(1).AsEnumerable().FirstOrDefault();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).FirstOrDefault();
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            var q = this.Take(1) as ShardingQuery<T>;
            return await (await q.Execute()).FirstOrDefaultAsync();
        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).FirstOrDefaultAsync();
        }

        public IJoinQuery<T, TOther> FullJoin<TOther>(Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T, TOther> FullJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IGroupingQuery<T> GroupBy<K>(Expression<Func<T, K>> keySelector)
        {
            return new ShardingGroupingQuery<T>(this.InnerQuery.GroupBy(keySelector));
        }

        public IQuery<T> IgnoreAllFilters()
        {
            return new ShardingQuery<T>(this.InnerQuery.IgnoreAllFilters());
        }

        public IIncludableQuery<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> p)
        {
            throw new NotImplementedException();
        }

        public IQuery<T> IncludeAll()
        {
            throw new NotImplementedException();
        }

        public IIncludableQuery<T, TCollectionItem> IncludeMany<TCollectionItem>(Expression<Func<T, IEnumerable<TCollectionItem>>> p)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T, TOther> Join<TOther>(JoinType joinType, Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T, TOther> Join<TOther>(IQuery<TOther> q, JoinType joinType, Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            return (int)this.LongCount();
        }

        public async Task<int> CountAsync()
        {
            return (int)await this.LongCountAsync();
        }

        public long LongCount()
        {
            return this.LongCountAsync().GetResult();
        }

        public async Task<long> LongCountAsync()
        {
            return await this.QueryCount();
        }

        public TResult Max<TResult>(Expression<Func<T, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public TResult Min<TResult>(Expression<Func<T, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public IOrderedQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector)
        {
            return new ShardingOrderedQuery<T>(this.InnerQuery.OrderBy(keySelector));
        }

        public IOrderedQuery<T> OrderByDesc<K>(Expression<Func<T, K>> keySelector)
        {
            return new ShardingOrderedQuery<T>(this.InnerQuery.OrderByDesc(keySelector));
        }

        public IJoinQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public IQuery<T> Skip(int count)
        {
            return new ShardingQuery<T>(this.InnerQuery.Skip(count));
        }

        public int? Sum(Expression<Func<T, int>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public int? Sum(Expression<Func<T, int?>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public long? Sum(Expression<Func<T, long>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public long? Sum(Expression<Func<T, long?>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public decimal? Sum(Expression<Func<T, decimal>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public double? Sum(Expression<Func<T, double>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public double? Sum(Expression<Func<T, double?>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public float? Sum(Expression<Func<T, float>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public float? Sum(Expression<Func<T, float?>> selector)
        {
            return this.SumAsync(selector).GetResult();
        }

        public async Task<int?> SumAsync(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    int? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (int?)a.Result).SumAsync();
        }

        public async Task<int?> SumAsync(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    int? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (int?)a.Result).SumAsync();
        }

        public async Task<long?> SumAsync(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    long? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (long?)a.Result).SumAsync();
        }

        public async Task<long?> SumAsync(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    long? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (long?)a.Result).SumAsync();
        }

        public async Task<decimal?> SumAsync(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    decimal? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (decimal?)a.Result).SumAsync();
        }

        public async Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    decimal? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (decimal?)a.Result).SumAsync();
        }

        public async Task<double?> SumAsync(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    double? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (double?)a.Result).SumAsync();
        }

        public async Task<double?> SumAsync(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    double? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (double?)a.Result).SumAsync();
        }

        public async Task<float?> SumAsync(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    float? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (float?)a.Result).SumAsync();
        }

        public async Task<float?> SumAsync(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
            //Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
            //{
            //    var q = (query as IQuery<T>);
            //    float? result = @async ? await q.SumAsync(selector) : q.Sum(selector);
            //    return result;
            //};

            //AggregateQuery aggQuery = new AggregateQuery(this.MakeQueryPlan(this), executor);
            //return await aggQuery.AsAsyncEnumerable().Select(a => (float?)a.Result).SumAsync();
        }

        public IQuery<T> Take(int count)
        {
            return new ShardingQuery<T>(this.InnerQuery.Take(count));
        }

        public IQuery<T> TakePage(int pageNumber, int pageSize)
        {
            return new ShardingQuery<T>(this.InnerQuery.TakePage(pageNumber, pageSize));
        }

        public PagingResult<T> Paging(int pageNumber, int pageSize)
        {
            return this.PagingAsync(pageNumber, pageSize).GetResult();
        }
        public async Task<PagingResult<T>> PagingAsync(int pageNumber, int pageSize)
        {
            var pagingResult = await this.ExecutePaging(pageNumber, pageSize);

            PagingResult<T> result = new PagingResult<T>();
            result.Count = pagingResult.Count;
            result.DataList = await pagingResult.Result.ToListAsync();

            return result;
        }

        public List<T> ToList()
        {
            return this.ToListAsync().GetResult();
        }
        public async Task<List<T>> ToListAsync()
        {
            return await (await this.Execute()).ToListAsync();
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            return new ShardingQuery<T>(this.InnerQuery.Where(predicate));
        }
    }
}

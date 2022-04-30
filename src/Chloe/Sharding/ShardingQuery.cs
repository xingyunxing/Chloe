using Chloe.Query;
using Chloe.Query.QueryExpressions;
using Chloe.Threading.Tasks;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    internal partial class ShardingQuery<T> : IQuery<T>
    {
        public ShardingQuery(ShardingDbContextProvider dbContextProvider, string explicitTable, LockType @lock) : this(CreateRootQueryExpression(dbContextProvider, explicitTable, @lock))
        {
        }
        public ShardingQuery(QueryExpression exp)
        {
            this.QueryExpression = exp;
        }

        static RootQueryExpression CreateRootQueryExpression(ShardingDbContextProvider dbContextProvider, string explicitTable, LockType @lock)
        {
            Type entityType = typeof(T);
            RootQueryExpression ret = new RootQueryExpression(entityType, dbContextProvider, explicitTable, @lock);
            return ret;
        }

        Type IQuery.ElementType { get { return typeof(T); } }
        public QueryExpression QueryExpression { get; private set; }

        public IEnumerable<T> AsEnumerable()
        {
            return this.GenerateIterator();
        }

        public IQuery<T> AsTracking()
        {
            TrackingExpression e = new TrackingExpression(typeof(T), this.QueryExpression);
            return new ShardingQuery<T>(e);
        }

        public IQuery<T> Distinct()
        {
            throw new NotImplementedException();
        }

        public IQuery<T> IgnoreAllFilters()
        {
            IgnoreAllFiltersExpression e = new IgnoreAllFiltersExpression(typeof(T), this.QueryExpression);
            return new ShardingQuery<T>(e);
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


        public IJoinQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
        {
            throw new NotImplementedException();
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
            return new ShardingGroupingQuery<T>(this, keySelector);
        }

        public IOrderedQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector)
        {
            PublicHelper.CheckNull(keySelector);
            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.OrderBy, keySelector);
            return new ShardingOrderedQuery<T>(e);
        }
        public IOrderedQuery<T> OrderByDesc<K>(Expression<Func<T, K>> keySelector)
        {
            PublicHelper.CheckNull(keySelector);
            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.OrderByDesc, keySelector);
            return new ShardingOrderedQuery<T>(e);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            PublicHelper.CheckNull(selector);
            SelectExpression e = new SelectExpression(typeof(TResult), this.QueryExpression, selector);
            return new ShardingQuery<TResult>(e);
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            PublicHelper.CheckNull(predicate);
            WhereExpression e = new WhereExpression(typeof(T), this.QueryExpression, predicate);
            return new ShardingQuery<T>(e);
        }
        public IQuery<T> Skip(int count)
        {
            SkipExpression e = new SkipExpression(typeof(T), this.QueryExpression, count);
            return new ShardingQuery<T>(e);
        }
        public IQuery<T> Take(int count)
        {
            TakeExpression e = new TakeExpression(typeof(T), this.QueryExpression, count);
            return new ShardingQuery<T>(e);
        }
        public IQuery<T> TakePage(int pageNumber, int pageSize)
        {
            int skipCount = (pageNumber - 1) * pageSize;
            int takeCount = pageSize;

            IQuery<T> q = this.Skip(skipCount).Take(takeCount);
            return q;
        }


        public bool Any()
        {
            return this.AnyAsync().GetResult();
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).AnyAsync().GetResult();
        }

        public Task<bool> AnyAsync()
        {
            return this.ExecuteAggregateQueryAsync<bool>(GetCalledMethod(() => default(IQuery<T>).Any()), null, false);
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).AnyAsync();
        }


        public int Count()
        {
            return this.CountAsync().GetResult();
        }
        public Task<int> CountAsync()
        {
            return this.ExecuteAggregateQueryAsync<int>(GetCalledMethod(() => default(IQuery<T>).Count()), null, false);
        }

        public long LongCount()
        {
            return this.LongCountAsync().GetResult();
        }
        public Task<long> LongCountAsync()
        {
            return this.ExecuteAggregateQueryAsync<long>(GetCalledMethod(() => default(IQuery<T>).LongCount()), null, false);
        }

        public TResult Max<TResult>(Expression<Func<T, TResult>> selector)
        {
            return this.MaxAsync(selector).GetResult();
        }
        public Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return this.ExecuteAggregateQueryAsync<TResult>(GetCalledMethod(() => default(IQuery<T>).Max(default(Expression<Func<T, TResult>>))), selector);
        }

        public TResult Min<TResult>(Expression<Func<T, TResult>> selector)
        {
            return this.MinAsync(selector).GetResult();
        }
        public Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return this.ExecuteAggregateQueryAsync<TResult>(GetCalledMethod(() => default(IQuery<T>).Min(default(Expression<Func<T, TResult>>))), selector);
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

        public Task<double?> AverageAsync(Expression<Func<T, int>> selector)
        {
            return this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, int>>))), selector);
        }

        public Task<double?> AverageAsync(Expression<Func<T, int?>> selector)
        {
            return this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, int?>>))), selector);
        }

        public Task<double?> AverageAsync(Expression<Func<T, long>> selector)
        {
            return this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, long>>))), selector);
        }

        public Task<double?> AverageAsync(Expression<Func<T, long?>> selector)
        {
            return this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, long?>>))), selector);
        }

        public Task<decimal?> AverageAsync(Expression<Func<T, decimal>> selector)
        {
            return this.ExecuteAggregateQueryAsync<decimal?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, decimal>>))), selector);
        }

        public async Task<decimal?> AverageAsync(Expression<Func<T, decimal?>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<decimal?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, decimal?>>))), selector);
        }

        public async Task<double?> AverageAsync(Expression<Func<T, double>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, double>>))), selector);
        }

        public async Task<double?> AverageAsync(Expression<Func<T, double?>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, double?>>))), selector);
        }

        public async Task<float?> AverageAsync(Expression<Func<T, float>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<float?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, float>>))), selector);
        }

        public async Task<float?> AverageAsync(Expression<Func<T, float?>> selector)
        {
            return await this.ExecuteAggregateQueryAsync<float?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, float?>>))), selector);
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
            var sum = await this.ExecuteAggregateQueryAsync<int?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, int>>))), selector);
            return sum;
        }

        public async Task<int?> SumAsync(Expression<Func<T, int?>> selector)
        {
            var sum = await this.ExecuteAggregateQueryAsync<int?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, int?>>))), selector);
            return sum;
        }

        public async Task<long?> SumAsync(Expression<Func<T, long>> selector)
        {
            var sum = await this.ExecuteAggregateQueryAsync<long?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, long>>))), selector);
            return sum;
        }

        public async Task<long?> SumAsync(Expression<Func<T, long?>> selector)
        {
            var sum = await this.ExecuteAggregateQueryAsync<long?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, long?>>))), selector);
            return sum;
        }

        public async Task<decimal?> SumAsync(Expression<Func<T, decimal>> selector)
        {
            var sum = await this.ExecuteAggregateQueryAsync<decimal?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, decimal>>))), selector);
            return sum;
        }

        public async Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector)
        {
            var sum = await this.ExecuteAggregateQueryAsync<decimal?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, decimal?>>))), selector);
            return sum;
        }

        public async Task<double?> SumAsync(Expression<Func<T, double>> selector)
        {
            var sum = await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, double>>))), selector);
            return sum;
        }

        public async Task<double?> SumAsync(Expression<Func<T, double?>> selector)
        {
            var sum = await this.ExecuteAggregateQueryAsync<double?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, double?>>))), selector);
            return sum;
        }

        public async Task<float?> SumAsync(Expression<Func<T, float>> selector)
        {
            var sum = await this.ExecuteAggregateQueryAsync<float?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, float>>))), selector);
            return sum;
        }

        public async Task<float?> SumAsync(Expression<Func<T, float?>> selector)
        {
            var sum = await this.ExecuteAggregateQueryAsync<float?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, float?>>))), selector);
            return sum;
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
            return await q.GenerateIterator().FirstAsync();
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
            return await q.GenerateIterator().FirstOrDefaultAsync();
        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).FirstOrDefaultAsync();
        }

        public PagingResult<T> Paging(int pageNumber, int pageSize)
        {
            return this.PagingAsync(pageNumber, pageSize).GetResult();
        }
        public async Task<PagingResult<T>> PagingAsync(int pageNumber, int pageSize)
        {
            PagingExpression pagingExpression = new PagingExpression(typeof(PagingResult<T>), this.QueryExpression, pageNumber, pageSize);
            var shardingQuery = new ShardingQuery<PagingResult<T>>(pagingExpression);
            var pagingResult = await shardingQuery.GenerateIterator().FirstAsync();
            return pagingResult;
        }


        public List<T> ToList()
        {
            return this.ToListAsync().GetResult();
        }
        public async Task<List<T>> ToListAsync()
        {
            IFeatureEnumerable<T> iterator = this.GenerateIterator();
            return await iterator.ToListAsync();
        }

    }
}

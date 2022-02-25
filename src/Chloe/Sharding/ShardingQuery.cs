using Chloe.Core.Visitors;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.Query;
using Chloe.Query.QueryExpressions;
using Chloe.Reflection;
using Chloe.Sharding.Queries;
using Chloe.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Sharding
{
    internal class ShardingQuery<T> : IQuery<T>
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

        Query<T> AsQuery(IQuery<T> query)
        {
            return query as Query<T>;
        }

        Type IQuery.ElementType { get { return typeof(T); } }

        public bool Any()
        {
            throw new NotImplementedException();
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> AsEnumerable()
        {
            throw new NotImplementedException();
        }

        public IQuery<T> AsTracking()
        {
            return new ShardingQuery<T>(this.InnerQuery.AsTracking());
        }

        public double? Average(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
        }

        public double? Average(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
        }

        public double? Average(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }

        public double? Average(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }

        public decimal? Average(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }

        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }

        public double? Average(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }

        public double? Average(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }

        public float? Average(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }

        public float? Average(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<double?> AverageAsync(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<double?> AverageAsync(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<double?> AverageAsync(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<double?> AverageAsync(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<decimal?> AverageAsync(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<decimal?> AverageAsync(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<double?> AverageAsync(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<double?> AverageAsync(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<float?> AverageAsync(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<float?> AverageAsync(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync()
        {
            throw new NotImplementedException();
        }

        public IQuery<T> Distinct()
        {
            throw new NotImplementedException();
        }

        public T First()
        {
            throw new NotImplementedException();
        }

        public T First(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<T> FirstAsync()
        {
            throw new NotImplementedException();
        }

        public Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault()
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<T> FirstOrDefaultAsync()
        {
            throw new NotImplementedException();
        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
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
            throw new NotImplementedException();
        }

        public IQuery<T> IgnoreAllFilters()
        {
            throw new NotImplementedException();
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

        public long LongCount()
        {
            throw new NotImplementedException();
        }

        public Task<long> LongCountAsync()
        {
            throw new NotImplementedException();
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

        public int Sum(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
        }

        public int? Sum(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
        }

        public long Sum(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }

        public long? Sum(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }

        public decimal Sum(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }

        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }

        public double Sum(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }

        public double? Sum(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }

        public float Sum(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }

        public float? Sum(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<int> SumAsync(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<int?> SumAsync(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<long> SumAsync(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<long?> SumAsync(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> SumAsync(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<decimal?> SumAsync(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<double> SumAsync(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<double?> SumAsync(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<float> SumAsync(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }

        public Task<float?> SumAsync(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
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
            var paging = await this.Execute(this.TakePage(pageNumber, pageSize) as ShardingQuery<T>);

            PagingResult<T> result = new PagingResult<T>();
            result.Count = paging.Count;
            result.DataList = await paging.Result.ToListAsync();

            return result;
        }

        public List<T> ToList()
        {
            throw new NotImplementedException();
        }

        LambdaExpression ConditionCombine(IEnumerable<LambdaExpression> conditions)
        {
            ParameterExpression parameterExpression = null;
            Expression conditionBody = null;
            foreach (var condition in conditions)
            {
                if (parameterExpression == null)
                {
                    parameterExpression = condition.Parameters[0];
                    conditionBody = condition.Body;
                    continue;
                }

                var newBody = ParameterExpressionReplacer.Replace(condition.Body, parameterExpression);
                conditionBody = Expression.AndAlso(conditionBody, newBody);
            }

            if (conditionBody == null)
            {
                return null;
            }

            return Expression.Lambda(conditionBody, parameterExpression);
        }

        async Task<QueryExecuteResult<T>> Execute(ShardingQuery<T> shardingQuery)
        {
            ShardingQueryModel queryModel = ShardingQueryModelPeeker.Peek(shardingQuery.InnerQuery.QueryExpression);

            IShardingConfig shardingConfig = ShardingConfigContainer.Get(typeof(T));
            IShardingContext shardingContext = new ShardingContext((ShardingDbContext)this.InnerQuery.DbContext, shardingConfig);

            IEnumerable<LambdaExpression> conditions = queryModel.Conditions;
            if (!queryModel.IgnoreAllFilters)
            {
                conditions = conditions.Concat(queryModel.GlobalFilters).Concat(queryModel.ContextFilters);
            }

            var condition = this.ConditionCombine(conditions);
            List<PhysicTable> physicTables = ShardingTablePeeker.Peek(condition, shardingContext);

            foreach (var physicTable in physicTables)
            {
                physicTable.DataSource.DbContextFactory = new PhysicDbContextFactoryWrapper(physicTable.DataSource.DbContextFactory, shardingContext.DbContext);
            }

            List<Ordering> orderings = queryModel.Orderings;

            //对物理表重排
            SortResult sortResult;
            if (orderings.Count == 0)
            {
                sortResult = new SortResult() { IsSorted = true, Tables = physicTables };
            }
            else
            {
                sortResult = shardingContext.SortTables(physicTables, orderings);
            }

            List<PhysicTable> tables = sortResult.Tables;

            int maxConnectionsPerDatabase = 12;
            int maxInItems = 1000;

            MultTableCountQuery<T> countQuery = new MultTableCountQuery<T>(tables, queryModel, maxConnectionsPerDatabase);

            List<MultTableCountQueryResult> physicTableCounts = await countQuery.ToListAsync();
            int totals = physicTableCounts.Select(a => a.Count).Sum();

            if (sortResult.IsSorted)
            {
                OrderlyMultTableDataQuery<T> tableDataQuery = new OrderlyMultTableDataQuery<T>(physicTableCounts, queryModel, maxConnectionsPerDatabase);

                return new QueryExecuteResult<T>(totals, tableDataQuery);
            }

            if (!sortResult.IsSorted)
            {
                DisorderedMultTableDataQuery<T> tableDataQuery = new DisorderedMultTableDataQuery<T>(tables, queryModel, maxConnectionsPerDatabase, maxInItems);

                return new QueryExecuteResult<T>(totals, tableDataQuery);
            }

            /*
             * 1. 需要 count 的情况，则先取所有的 count
             *    1.1 无排序，则根据 count 情况定位指定的表查询即可
             *    1.2 有排序
             *        1.2.1 无顺序的表，则全部路由，在内存中进行归并（会有连接过多，内存爆炸问题）
             *        1.2.2 有顺序的表，则根据 count 情况定位指定的表查询即可
             *        
             * 2. 不需要 count 的情况
             *    2.1 无排序，则先从第一个表中分页，数据不够则再从第二个表中再取，直到数据足够为止
             *    2.2 有排序
             *        2.2.1 无顺序的表，则全部路由，在内存中进行归并（会有连接过多，内存爆炸问题）
             *        2.2.2 有顺序的表，则先从第一个表中分页，数据不够则再从第二个表中再取，直到数据足够为止
             */

            /*
             * 查询的表过多，可能产生的问题：
             * 1. 采用流式分页，一个表一个连接，会出现连接数量爆炸问题
             * 2. 不用流式分页，如果将每个表的 skip + take 数据加载进内存在归并排序，会出现内存爆炸
             * 
             * 解决方法：
             * 表过多，肯定不能采用流式分页了！！！
             * 1. 同库内的表使用 union 查询结果集，也就是利用了数据库进行归并排序
             * 2. 不用流式分页
             *    2.1 先将每个表的 id 和对应的排序字段 skip + take  数据加载进内存，在内存里归并排序取出最终的数据 id
             *    2.2 拿到 id 后再根据 id 通过构建 in 条件再去数据库查询表数据
             *        2.2.1 如何避免 in 数据太多？因为 in 有数量限制？
             *              分批 in 即可
             * 
             * 
             */


            throw new NotImplementedException();
        }

        public Task<List<T>> ToListAsync()
        {
            throw new NotImplementedException();
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            return new ShardingQuery<T>(this.InnerQuery.Where(predicate));
        }
    }
}

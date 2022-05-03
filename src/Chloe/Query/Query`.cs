using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.QueryExpressions;
using Chloe.Reflection;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    partial class Query<T> : IQuery<T>, IQuery
    {
        QueryExpression _expression;

        Type IQuery.ElementType { get { return typeof(T); } }
        public QueryExpression QueryExpression { get { return this._expression; } }

        static RootQueryExpression CreateRootQueryExpression(DbContextProvider dbContextProvider, string explicitTable, LockType @lock)
        {
            Type entityType = typeof(T);
            RootQueryExpression ret = new RootQueryExpression(entityType, dbContextProvider, explicitTable, @lock);
            return ret;
        }
        public Query(DbContextProvider dbContextProvider, string explicitTable, LockType @lock) : this(CreateRootQueryExpression(dbContextProvider, explicitTable, @lock))
        {
        }
        public Query(QueryExpression exp)
        {
            this._expression = exp;
        }

        public IQuery<T> AsTracking()
        {
            TrackingExpression e = new TrackingExpression(typeof(T), this.QueryExpression);
            return new Query<T>(e);
        }
        public IEnumerable<T> AsEnumerable()
        {
            return this.GenerateIterator();
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            PublicHelper.CheckNull(selector);
            SelectExpression e = new SelectExpression(typeof(TResult), _expression, selector);
            return new Query<TResult>(e);
        }

        public IQuery<T> IncludeAll()
        {
            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(T));

            object lastQuery = this;
            List<Type> chains = new List<Type>(2) { typeof(T) }; //用于记录 Include 链路，避免循环依赖导致无限递归
            for (int i = 0; i < typeDescriptor.NavigationPropertyDescriptors.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = typeDescriptor.NavigationPropertyDescriptors[i];
                lastQuery = this.Include(typeDescriptor, lastQuery, propertyDescriptor, chains);
            }

            return (IQuery<T>)lastQuery;
        }
        object Include(TypeDescriptor typeDescriptor, object lastQuery, PropertyDescriptor propertyDescriptor, List<Type> chains)
        {
            //entity.TOther or entity.List
            TypeDescriptor navTypeDescriptor = propertyDescriptor.GetPropertyTypeDescriptor();

            Func<object, object> queryBuilder = query =>
            {
                return this.CallIncludeMethod(query, propertyDescriptor);
            };

            chains.Add(navTypeDescriptor.EntityType);
            lastQuery = this.ThenInclude(navTypeDescriptor, queryBuilder(lastQuery), typeDescriptor, queryBuilder, chains);
            chains.RemoveAt(chains.Count - 1);

            return lastQuery;
        }
        object ThenInclude(TypeDescriptor typeDescriptor, object lastQuery, TypeDescriptor declaringTypeDescriptor, Func<object, object> queryBuilder, List<Type> chains)
        {
            int navCount = typeDescriptor.NavigationPropertyDescriptors.Count;

            bool needRebuildQuery = false;
            for (int i = 0; i < typeDescriptor.NavigationPropertyDescriptors.Count; i++)
            {
                //entity.TOther
                PropertyDescriptor propertyDescriptor = typeDescriptor.NavigationPropertyDescriptors[i];
                TypeDescriptor navTypeDescriptor = propertyDescriptor.GetPropertyTypeDescriptor();
                if (declaringTypeDescriptor != null && navTypeDescriptor == declaringTypeDescriptor)
                {
                    continue;
                }

                //避免循环依赖导致无限递归
                if (chains.Any(a => a == navTypeDescriptor.EntityType))
                {
                    continue;
                }

                Func<object, object> includableQueryBuilder = query =>
                {
                    return this.CallThenIncludeMethod(queryBuilder(query), propertyDescriptor);
                };

                if (needRebuildQuery)
                    lastQuery = queryBuilder(lastQuery);

                //lastQuery = lastQuery.ThenInclude(a => a.propertyDescriptor);
                lastQuery = this.CallThenIncludeMethod(lastQuery, propertyDescriptor);

                chains.Add(navTypeDescriptor.EntityType);
                lastQuery = this.ThenInclude(navTypeDescriptor, lastQuery, typeDescriptor, includableQueryBuilder, chains);
                chains.RemoveAt(chains.Count - 1);

                needRebuildQuery = true;
            }

            return lastQuery;
        }
        object CallIncludeMethod(object query, PropertyDescriptor propertyDescriptor)
        {
            Type queryType = typeof(IQuery<T>);
            MethodInfo includeMethod;
            if (propertyDescriptor is ComplexPropertyDescriptor)
            {
                includeMethod = queryType.GetMethod("Include");
                includeMethod = includeMethod.MakeGenericMethod(propertyDescriptor.PropertyType);
            }
            else
            {
                includeMethod = queryType.GetMethod("IncludeMany");
                includeMethod = includeMethod.MakeGenericMethod((propertyDescriptor as CollectionPropertyDescriptor).ElementType);
            }

            var includeMethodArgument = this.MakeIncludeMethodArgument(includeMethod, typeof(T), propertyDescriptor.Property);

            // query.Include<property>(a => a.property)
            var includableQuery = includeMethod.FastInvoke(query, new object[] { includeMethodArgument });
            return includableQuery;
        }
        object CallThenIncludeMethod(object includableQuery, PropertyDescriptor propertyDescriptor)
        {
            Type includableQueryType = includableQuery.GetType().GetInterface("IIncludableQuery`2");
            MethodInfo thenIncludeMethod;
            if (propertyDescriptor is ComplexPropertyDescriptor)
            {
                thenIncludeMethod = includableQueryType.GetMethod("ThenInclude");
                thenIncludeMethod = thenIncludeMethod.MakeGenericMethod(propertyDescriptor.PropertyType);
            }
            else
            {
                thenIncludeMethod = includableQueryType.GetMethod("ThenIncludeMany");
                thenIncludeMethod = thenIncludeMethod.MakeGenericMethod((propertyDescriptor as CollectionPropertyDescriptor).ElementType);
            }

            var lambdaParameterType = includableQueryType.GetGenericArguments()[1];
            var includeMethodArgument = this.MakeIncludeMethodArgument(thenIncludeMethod, lambdaParameterType, propertyDescriptor.Property);

            // includableQuery.ThenInclude<property>(a => a.property)
            includableQuery = thenIncludeMethod.FastInvoke(includableQuery, new object[] { includeMethodArgument });
            return includableQuery;
        }
        LambdaExpression MakeIncludeMethodArgument(MethodInfo includeMethod, Type lambdaParameterType, PropertyInfo includeProperty)
        {
            var p = Expression.Parameter(lambdaParameterType, "a");
            var propertyAccess = Expression.MakeMemberAccess(p, includeProperty);
            Type funcType = includeMethod.GetParameters()[0].ParameterType.GetGenericArguments()[0];
            var lambda = Expression.Lambda(funcType, propertyAccess, p);

            return lambda;
        }

        public IIncludableQuery<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> navigationPath)
        {
            return new IncludableQuery<T, TProperty>(this.QueryExpression, navigationPath);
        }
        public IIncludableQuery<T, TCollectionItem> IncludeMany<TCollectionItem>(Expression<Func<T, IEnumerable<TCollectionItem>>> navigationPath)
        {
            return new IncludableQuery<T, TCollectionItem>(this.QueryExpression, navigationPath);
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            PublicHelper.CheckNull(predicate);
            WhereExpression e = new WhereExpression(typeof(T), this._expression, predicate);
            return new Query<T>(e);
        }
        public IOrderedQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector)
        {
            PublicHelper.CheckNull(keySelector);
            OrderExpression e = new OrderExpression(typeof(T), this._expression, QueryExpressionType.OrderBy, keySelector);
            return new OrderedQuery<T>(e);
        }
        public IOrderedQuery<T> OrderByDesc<K>(Expression<Func<T, K>> keySelector)
        {
            PublicHelper.CheckNull(keySelector);
            OrderExpression e = new OrderExpression(typeof(T), this._expression, QueryExpressionType.OrderByDesc, keySelector);
            return new OrderedQuery<T>(e);
        }
        public IQuery<T> Skip(int count)
        {
            SkipExpression e = new SkipExpression(typeof(T), this._expression, count);
            return new Query<T>(e);
        }
        public IQuery<T> Take(int count)
        {
            TakeExpression e = new TakeExpression(typeof(T), this._expression, count);
            return new Query<T>(e);
        }
        public IQuery<T> TakePage(int pageNumber, int pageSize)
        {
            int skipCount = (pageNumber - 1) * pageSize;
            int takeCount = pageSize;

            IQuery<T> q = this.Skip(skipCount).Take(takeCount);
            return q;
        }
        public PagingResult<T> Paging(int pageNumber, int pageSize)
        {
            PagingResult<T> result = new PagingResult<T>();
            result.Totals = this.Count();
            result.DataList = this.TakePage(pageNumber, pageSize).ToList();

            return result;
        }
        public async Task<PagingResult<T>> PagingAsync(int pageNumber, int pageSize)
        {
            PagingResult<T> result = new PagingResult<T>();
            result.Totals = await this.CountAsync();
            result.DataList = await this.TakePage(pageNumber, pageSize).ToListAsync();

            return result;
        }

        public IGroupingQuery<T> GroupBy<K>(Expression<Func<T, K>> keySelector)
        {
            PublicHelper.CheckNull(keySelector);
            return new GroupingQuery<T>(this, keySelector);
        }
        public IQuery<T> Distinct()
        {
            DistinctExpression e = new DistinctExpression(typeof(T), this._expression);
            return new Query<T>(e);
        }
        public IQuery<T> IgnoreAllFilters()
        {
            IgnoreAllFiltersExpression e = new IgnoreAllFiltersExpression(typeof(T), this._expression);
            return new Query<T>(e);
        }

        public IJoinQuery<T, TOther> Join<TOther>(JoinType joinType, Expression<Func<T, TOther, bool>> on)
        {
            IDbContextProvider dbContextProvider = this.QueryExpression.GetRootDbContextProvider();
            return this.Join<TOther>(dbContextProvider.Query<TOther>(), joinType, on);
        }
        public IJoinQuery<T, TOther> Join<TOther>(IQuery<TOther> q, JoinType joinType, Expression<Func<T, TOther, bool>> on)
        {
            PublicHelper.CheckNull(q);
            PublicHelper.CheckNull(on);
            return new JoinQuery<T, TOther>(this, (Query<TOther>)q, joinType, on);
        }

        public IJoinQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> on)
        {
            IDbContextProvider dbContextProvider = this.QueryExpression.GetRootDbContextProvider();
            return this.InnerJoin<TOther>(dbContextProvider.Query<TOther>(), on);
        }
        public IJoinQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> on)
        {
            IDbContextProvider dbContextProvider = this.QueryExpression.GetRootDbContextProvider();
            return this.LeftJoin<TOther>(dbContextProvider.Query<TOther>(), on);
        }
        public IJoinQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> on)
        {
            IDbContextProvider dbContextProvider = this.QueryExpression.GetRootDbContextProvider();
            return this.RightJoin<TOther>(dbContextProvider.Query<TOther>(), on);
        }
        public IJoinQuery<T, TOther> FullJoin<TOther>(Expression<Func<T, TOther, bool>> on)
        {
            IDbContextProvider dbContextProvider = this.QueryExpression.GetRootDbContextProvider();
            return this.FullJoin<TOther>(dbContextProvider.Query<TOther>(), on);
        }

        public IJoinQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
        {
            return this.Join<TOther>(q, JoinType.InnerJoin, on);
        }
        public IJoinQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
        {
            return this.Join<TOther>(q, JoinType.LeftJoin, on);
        }
        public IJoinQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
        {
            return this.Join<TOther>(q, JoinType.RightJoin, on);
        }
        public IJoinQuery<T, TOther> FullJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
        {
            return this.Join<TOther>(q, JoinType.FullJoin, on);
        }

        public T First()
        {
            var q = (Query<T>)this.Take(1);
            IEnumerable<T> iterator = q.GenerateIterator();
            return iterator.First();
        }
        public T First(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).First();
        }
        public T FirstOrDefault()
        {
            var q = (Query<T>)this.Take(1);
            IEnumerable<T> iterator = q.GenerateIterator();
            return iterator.FirstOrDefault();
        }
        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).FirstOrDefault();
        }

        public List<T> ToList()
        {
            IEnumerable<T> iterator = this.GenerateIterator();
            return iterator.ToList();
        }

        public bool Any()
        {
            string v = "1";
            var q = (Query<string>)this.Select(a => v).Take(1);
            return q.GenerateIterator().Any();
        }
        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).Any();
        }

        public int Count()
        {
            return this.ExecuteAggregateQuery<int>(GetCalledMethod(() => default(IQuery<T>).Count()), null, false);
        }
        public long LongCount()
        {
            return this.ExecuteAggregateQuery<long>(GetCalledMethod(() => default(IQuery<T>).LongCount()), null, false);
        }

        public TResult Max<TResult>(Expression<Func<T, TResult>> selector)
        {
            return this.ExecuteAggregateQuery<TResult>(GetCalledMethod(() => default(IQuery<T>).Max(default(Expression<Func<T, TResult>>))), selector);
        }
        public TResult Min<TResult>(Expression<Func<T, TResult>> selector)
        {
            return this.ExecuteAggregateQuery<TResult>(GetCalledMethod(() => default(IQuery<T>).Min(default(Expression<Func<T, TResult>>))), selector);
        }

        public int? Sum(Expression<Func<T, int>> selector)
        {
            return this.ExecuteAggregateQuery<int?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, int>>))), selector);
        }
        public int? Sum(Expression<Func<T, int?>> selector)
        {
            return this.ExecuteAggregateQuery<int?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, int?>>))), selector);
        }
        public long? Sum(Expression<Func<T, long>> selector)
        {
            return this.ExecuteAggregateQuery<long?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, long>>))), selector);
        }
        public long? Sum(Expression<Func<T, long?>> selector)
        {
            return this.ExecuteAggregateQuery<long?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, long?>>))), selector);
        }
        public decimal? Sum(Expression<Func<T, decimal>> selector)
        {
            return this.ExecuteAggregateQuery<decimal?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, decimal>>))), selector);
        }
        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            return this.ExecuteAggregateQuery<decimal?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, decimal?>>))), selector);
        }
        public double? Sum(Expression<Func<T, double>> selector)
        {
            return this.ExecuteAggregateQuery<double?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, double>>))), selector);
        }
        public double? Sum(Expression<Func<T, double?>> selector)
        {
            return this.ExecuteAggregateQuery<double?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, double?>>))), selector);
        }
        public float? Sum(Expression<Func<T, float>> selector)
        {
            return this.ExecuteAggregateQuery<float?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, float>>))), selector);
        }
        public float? Sum(Expression<Func<T, float?>> selector)
        {
            return this.ExecuteAggregateQuery<float?>(GetCalledMethod(() => default(IQuery<T>).Sum(default(Expression<Func<T, float?>>))), selector);
        }

        public double? Average(Expression<Func<T, int>> selector)
        {
            return this.ExecuteAggregateQuery<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, int>>))), selector);
        }
        public double? Average(Expression<Func<T, int?>> selector)
        {
            return this.ExecuteAggregateQuery<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, int?>>))), selector);
        }
        public double? Average(Expression<Func<T, long>> selector)
        {
            return this.ExecuteAggregateQuery<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, long>>))), selector);
        }
        public double? Average(Expression<Func<T, long?>> selector)
        {
            return this.ExecuteAggregateQuery<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, long?>>))), selector);
        }
        public decimal? Average(Expression<Func<T, decimal>> selector)
        {
            return this.ExecuteAggregateQuery<decimal?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, decimal>>))), selector);
        }
        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            return this.ExecuteAggregateQuery<decimal?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, decimal?>>))), selector);
        }
        public double? Average(Expression<Func<T, double>> selector)
        {
            return this.ExecuteAggregateQuery<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, double>>))), selector);
        }
        public double? Average(Expression<Func<T, double?>> selector)
        {
            return this.ExecuteAggregateQuery<double?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, double?>>))), selector);
        }
        public float? Average(Expression<Func<T, float>> selector)
        {
            return this.ExecuteAggregateQuery<float?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, float>>))), selector);
        }
        public float? Average(Expression<Func<T, float?>> selector)
        {
            return this.ExecuteAggregateQuery<float?>(GetCalledMethod(() => default(IQuery<T>).Average(default(Expression<Func<T, float?>>))), selector);
        }
    }
}

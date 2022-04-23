//using Chloe.Descriptors;
//using Chloe.Infrastructure;
//using Chloe.Query.Internals;
//using Chloe.Query.QueryExpressions;
//using Chloe.Reflection;
//using System.Linq.Expressions;
//using System.Reflection;

//namespace Chloe.Query
//{
//    abstract class QueryAbstract<T>
//    {
//        public QueryExpression QueryExpression { get; set; }
//        public Type ElementType { get { return typeof(T); } }

//        static RootQueryExpression CreateRootQueryExpression(IDbContextInternal dbContext, string explicitTable, LockType @lock)
//        {
//            Type entityType = typeof(T);
//            RootQueryExpression ret = new RootQueryExpression(entityType, dbContext, explicitTable, @lock);
//            List<LambdaExpression> filters = dbContext.QueryFilters.FindValue(entityType);
//            if (filters != null)
//                ret.ContextFilters.AddRange(filters);

//            return ret;
//        }
//        public QueryAbstract(IDbContextInternal dbContext, string explicitTable, LockType @lock) : this(CreateRootQueryExpression(dbContext, explicitTable, @lock))
//        {
//        }
//        public QueryAbstract(QueryExpression exp)
//        {
//            this.QueryExpression = exp;
//        }

//        public IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
//        {
//            PublicHelper.CheckNull(selector);
//            SelectExpression e = new SelectExpression(typeof(TResult), this.QueryExpression, selector);
//            return new Query<TResult>(e);
//        }

//        public IQuery<T> IncludeAll()
//        {
//            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(T));

//            object lastQuery = this;
//            for (int i = 0; i < typeDescriptor.NavigationPropertyDescriptors.Count; i++)
//            {
//                PropertyDescriptor propertyDescriptor = typeDescriptor.NavigationPropertyDescriptors[i];
//                lastQuery = this.Include(typeDescriptor, lastQuery, propertyDescriptor);
//            }

//            return (IQuery<T>)lastQuery;
//        }
//        object Include(TypeDescriptor typeDescriptor, object lastQuery, PropertyDescriptor propertyDescriptor)
//        {
//            //entity.TOther or entity.List
//            TypeDescriptor navTypeDescriptor = propertyDescriptor.GetPropertyTypeDescriptor();

//            Func<object, object> queryBuilder = query =>
//            {
//                return this.CallIncludeMethod(query, propertyDescriptor);
//            };

//            lastQuery = this.ThenInclude(navTypeDescriptor, queryBuilder(lastQuery), typeDescriptor, queryBuilder);

//            return lastQuery;
//        }
//        object ThenInclude(TypeDescriptor typeDescriptor, object lastQuery, TypeDescriptor declaringTypeDescriptor, Func<object, object> queryBuilder)
//        {
//            int navCount = typeDescriptor.NavigationPropertyDescriptors.Count;

//            bool needRebuildQuery = false;
//            for (int i = 0; i < typeDescriptor.NavigationPropertyDescriptors.Count; i++)
//            {
//                //entity.TOther
//                PropertyDescriptor propertyDescriptor = typeDescriptor.NavigationPropertyDescriptors[i];
//                TypeDescriptor navTypeDescriptor = propertyDescriptor.GetPropertyTypeDescriptor();
//                if (declaringTypeDescriptor != null && navTypeDescriptor == declaringTypeDescriptor)
//                {
//                    continue;
//                }

//                Func<object, object> includableQueryBuilder = query =>
//                {
//                    return this.CallThenIncludeMethod(queryBuilder(query), propertyDescriptor);
//                };

//                if (needRebuildQuery)
//                    lastQuery = queryBuilder(lastQuery);

//                //lastQuery = lastQuery.ThenInclude(a => a.propertyDescriptor);
//                lastQuery = this.CallThenIncludeMethod(lastQuery, propertyDescriptor);
//                lastQuery = this.ThenInclude(navTypeDescriptor, lastQuery, typeDescriptor, includableQueryBuilder);

//                needRebuildQuery = true;
//            }

//            return lastQuery;
//        }
//        object CallIncludeMethod(object query, PropertyDescriptor propertyDescriptor)
//        {
//            Type queryType = typeof(IQuery<T>);
//            MethodInfo includeMethod;
//            if (propertyDescriptor is ComplexPropertyDescriptor)
//            {
//                includeMethod = queryType.GetMethod("Include");
//                includeMethod = includeMethod.MakeGenericMethod(propertyDescriptor.PropertyType);
//            }
//            else
//            {
//                includeMethod = queryType.GetMethod("IncludeMany");
//                includeMethod = includeMethod.MakeGenericMethod((propertyDescriptor as CollectionPropertyDescriptor).ElementType);
//            }

//            var includeMethodArgument = this.MakeIncludeMethodArgument(includeMethod, typeof(T), propertyDescriptor.Property);

//            // query.Include<property>(a => a.property)
//            var includableQuery = includeMethod.FastInvoke(query, new object[] { includeMethodArgument });
//            return includableQuery;
//        }
//        object CallThenIncludeMethod(object includableQuery, PropertyDescriptor propertyDescriptor)
//        {
//            Type includableQueryType = includableQuery.GetType().GetInterface("IIncludableQuery`2");
//            MethodInfo thenIncludeMethod;
//            if (propertyDescriptor is ComplexPropertyDescriptor)
//            {
//                thenIncludeMethod = includableQueryType.GetMethod("ThenInclude");
//                thenIncludeMethod = thenIncludeMethod.MakeGenericMethod(propertyDescriptor.PropertyType);
//            }
//            else
//            {
//                thenIncludeMethod = includableQueryType.GetMethod("ThenIncludeMany");
//                thenIncludeMethod = thenIncludeMethod.MakeGenericMethod((propertyDescriptor as CollectionPropertyDescriptor).ElementType);
//            }

//            var lambdaParameterType = includableQueryType.GetGenericArguments()[1];
//            var includeMethodArgument = this.MakeIncludeMethodArgument(thenIncludeMethod, lambdaParameterType, propertyDescriptor.Property);

//            // includableQuery.ThenInclude<property>(a => a.property)
//            includableQuery = thenIncludeMethod.FastInvoke(includableQuery, new object[] { includeMethodArgument });
//            return includableQuery;
//        }
//        LambdaExpression MakeIncludeMethodArgument(MethodInfo includeMethod, Type lambdaParameterType, PropertyInfo includeProperty)
//        {
//            var p = Expression.Parameter(lambdaParameterType, "a");
//            var propertyAccess = Expression.MakeMemberAccess(p, includeProperty);
//            Type funcType = includeMethod.GetParameters()[0].ParameterType.GetGenericArguments()[0];
//            var lambda = Expression.Lambda(funcType, propertyAccess, p);

//            return lambda;
//        }

//        public IIncludableQuery<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> navigationPath)
//        {
//            return new IncludableQuery<T, TProperty>(this.QueryExpression, navigationPath);
//        }
//        public IIncludableQuery<T, TCollectionItem> IncludeMany<TCollectionItem>(Expression<Func<T, IEnumerable<TCollectionItem>>> navigationPath)
//        {
//            return new IncludableQuery<T, TCollectionItem>(this.QueryExpression, navigationPath);
//        }

//        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
//        {
//            PublicHelper.CheckNull(predicate);
//            WhereExpression e = new WhereExpression(typeof(T), this.QueryExpression, predicate);
//            return new Query<T>(e);
//        }
//        public IOrderedQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector)
//        {
//            PublicHelper.CheckNull(keySelector);
//            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.OrderBy, keySelector);
//            return new OrderedQuery<T>(e);
//        }
//        public IOrderedQuery<T> OrderByDesc<K>(Expression<Func<T, K>> keySelector)
//        {
//            PublicHelper.CheckNull(keySelector);
//            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.OrderByDesc, keySelector);
//            return new OrderedQuery<T>(e);
//        }
//        public IQuery<T> Skip(int count)
//        {
//            SkipExpression e = new SkipExpression(typeof(T), this.QueryExpression, count);
//            return new Query<T>(e);
//        }
//        public IQuery<T> Take(int count)
//        {
//            TakeExpression e = new TakeExpression(typeof(T), this.QueryExpression, count);
//            return new Query<T>(e);
//        }
//        public IQuery<T> TakePage(int pageNumber, int pageSize)
//        {
//            int skipCount = (pageNumber - 1) * pageSize;
//            int takeCount = pageSize;

//            IQuery<T> q = this.Skip(skipCount).Take(takeCount);
//            return q;
//        }

//        public IGroupingQuery<T> GroupBy<K>(Expression<Func<T, K>> keySelector)
//        {
//            PublicHelper.CheckNull(keySelector);
//            return new GroupingQuery<T>(this, keySelector);
//        }
//        public IQuery<T> Distinct()
//        {
//            DistinctExpression e = new DistinctExpression(typeof(T), this.QueryExpression);
//            return new Query<T>(e);
//        }
//        public IQuery<T> IgnoreAllFilters()
//        {
//            IgnoreAllFiltersExpression e = new IgnoreAllFiltersExpression(typeof(T), this.QueryExpression);
//            return new Query<T>(e);
//        }

//        public IJoinQuery<T, TOther> Join<TOther>(JoinType joinType, Expression<Func<T, TOther, bool>> on)
//        {
//            IDbContext dbContext = this.QueryExpression.GetRootDbContext();
//            return this.Join<TOther>(dbContext.Query<TOther>(), joinType, on);
//        }
//        public IJoinQuery<T, TOther> Join<TOther>(IQuery<TOther> q, JoinType joinType, Expression<Func<T, TOther, bool>> on)
//        {
//            PublicHelper.CheckNull(q);
//            PublicHelper.CheckNull(on);
//            return new JoinQuery<T, TOther>(this, (Query<TOther>)q, joinType, on);
//        }

//        public IJoinQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> on)
//        {
//            IDbContext dbContext = this.QueryExpression.GetRootDbContext();
//            return this.InnerJoin<TOther>(dbContext.Query<TOther>(), on);
//        }
//        public IJoinQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> on)
//        {
//            IDbContext dbContext = this.QueryExpression.GetRootDbContext();
//            return this.LeftJoin<TOther>(dbContext.Query<TOther>(), on);
//        }
//        public IJoinQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> on)
//        {
//            IDbContext dbContext = this.QueryExpression.GetRootDbContext();
//            return this.RightJoin<TOther>(dbContext.Query<TOther>(), on);
//        }
//        public IJoinQuery<T, TOther> FullJoin<TOther>(Expression<Func<T, TOther, bool>> on)
//        {
//            IDbContext dbContext = this.QueryExpression.GetRootDbContext();
//            return this.FullJoin<TOther>(dbContext.Query<TOther>(), on);
//        }

//        public IJoinQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
//        {
//            return this.Join<TOther>(q, JoinType.InnerJoin, on);
//        }
//        public IJoinQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
//        {
//            return this.Join<TOther>(q, JoinType.LeftJoin, on);
//        }
//        public IJoinQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
//        {
//            return this.Join<TOther>(q, JoinType.RightJoin, on);
//        }
//        public IJoinQuery<T, TOther> FullJoin<TOther>(IQuery<TOther> q, Expression<Func<T, TOther, bool>> on)
//        {
//            return this.Join<TOther>(q, JoinType.FullJoin, on);
//        }
//    }
//}

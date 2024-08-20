using Chloe.Reflection;
using Chloe.Threading.Tasks;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe
{
    static class QueryExtension
    {
        static object CallMethod(object obj, string methodName, object arg)
        {
            var paramterTypes = arg == null ? Type.EmptyTypes : new Type[1] { arg.GetType() };

            var method = obj.GetType().GetMethod(methodName, paramterTypes);
            var result = method.FastInvoke(obj, arg == null ? PublicConstants.EmptyArray : new object[1] { arg });
            return result;
        }
        static TResult CallMethod<TResult>(object obj, string methodName, object arg)
        {
            var result = CallMethod(obj, methodName, arg);
            return (TResult)result;
        }
        static async Task<object> CallMethodAsync(object obj, string methodName, object arg)
        {
            var paramterTypes = arg == null ? Type.EmptyTypes : new Type[1] { arg.GetType() };

            var method = obj.GetType().GetMethod(methodName, paramterTypes);
            var task = (Task)method.FastInvoke(obj, arg == null ? PublicConstants.EmptyArray : new object[1] { arg });
            await task;
            var result = task.GetCompletedTaskResult();
            return result;
        }
        static async Task<TResult> CallMethodAsync<TResult>(object obj, string methodName, object arg)
        {
            var result = await CallMethodAsync(obj, methodName, arg);
            return (TResult)result;
        }

        static object CallGenericMethod(object obj, string methodName, LambdaExpression selector)
        {
            var method = obj.GetType().GetMethod(methodName).MakeGenericMethod(selector.Body.Type);
            var result = method.FastInvoke(obj, new object[1] { selector });
            return result;
        }
        static TResult CallGenericMethod<TResult>(object obj, string methodName, LambdaExpression selector)
        {
            var result = CallGenericMethod(obj, methodName, selector);
            return (TResult)result;
        }
        static async Task<object> CallGenericMethodAsync(object obj, string methodName, LambdaExpression selector)
        {
            var method = obj.GetType().GetMethod(methodName).MakeGenericMethod(selector.Body.Type);
            var task = (Task)method.FastInvoke(obj, new object[1] { selector });
            await task;
            var result = task.GetCompletedTaskResult();
            return result;
        }
        static async Task<TResult> CallGenericMethodAsync<TResult>(object obj, string methodName, LambdaExpression selector)
        {
            var result = await CallGenericMethodAsync(obj, methodName, selector);
            return (TResult)result;
        }

        public static IQuery Where(this IQuery query, LambdaExpression predicate)
        {
            query = CallMethod<IQuery>(query, nameof(IQuery<object>.Where), predicate);
            return query;
        }

        public static IQuery Select(this IQuery query, LambdaExpression selector)
        {
            query = CallGenericMethod<IQuery>(query, nameof(IQuery<object>.Select), selector);
            return query;
        }

        public static IQuery OrderBy(this IQuery query, LambdaExpression selector)
        {
            query = CallGenericMethod<IQuery>(query, nameof(IQuery<object>.OrderBy), selector);
            return query;
        }
        public static IQuery OrderByDesc(this IQuery query, LambdaExpression selector)
        {
            query = CallGenericMethod<IQuery>(query, nameof(IQuery<object>.OrderByDesc), selector);
            return query;
        }

        public static IQuery ThenBy(this IQuery query, LambdaExpression selector)
        {
            query = CallGenericMethod<IQuery>(query, nameof(IOrderedQuery<object>.ThenBy), selector);
            return query;
        }
        public static IQuery ThenByDesc(this IQuery query, LambdaExpression selector)
        {
            query = CallGenericMethod<IQuery>(query, nameof(IOrderedQuery<object>.ThenByDesc), selector);
            return query;
        }

        public static IQuery Skip(this IQuery query, int count)
        {
            var result = CallMethod<IQuery>(query, nameof(IQuery<object>.Skip), count);
            return result;
        }

        public static IQuery Take(this IQuery query, int count)
        {
            var result = CallMethod<IQuery>(query, nameof(IQuery<object>.Take), count);
            return result;
        }

        public static IEnumerable AsEnumerable(this IQuery query)
        {
            var result = CallMethod<IEnumerable>(query, nameof(IQuery<object>.AsEnumerable), null);
            return result;
        }
        public static IList ToList(this IQuery query)
        {
            var result = CallMethod<IList>(query, nameof(IQuery<object>.ToList), null);
            return result;
        }
        public static Task<IList> ToListAsync(this IQuery query)
        {
            return CallMethodAsync<IList>(query, nameof(IQuery<object>.ToListAsync), null);
        }

        public static int Count(this IQuery query)
        {
            return CallMethod<int>(query, nameof(IQuery<object>.Count), null);
        }
        public static Task<int> CountAsync(this IQuery query)
        {
            return CallMethodAsync<int>(query, nameof(IQuery<object>.CountAsync), null);
        }

        public static long LongCount(this IQuery query)
        {
            return CallMethod<long>(query, nameof(IQuery<object>.LongCount), null);
        }
        public static Task<long> LongCountAsync(this IQuery query)
        {
            return CallMethodAsync<long>(query, nameof(IQuery<object>.LongCountAsync), null);
        }

        public static object Sum(this IQuery query, LambdaExpression selector)
        {
            var result = CallMethod(query, nameof(IQuery<object>.Sum), selector);
            return result;
        }
        public static Task<object> SumAsync(this IQuery query, LambdaExpression selector)
        {
            return CallMethodAsync(query, nameof(IQuery<object>.SumAsync), selector);
        }

        public static object Max(this IQuery query, LambdaExpression selector)
        {
            var result = CallGenericMethod(query, nameof(IQuery<object>.Max), selector);
            return result;
        }
        public static Task<object> MaxAsync(this IQuery query, LambdaExpression selector)
        {
            var result = CallGenericMethodAsync(query, nameof(IQuery<object>.MaxAsync), selector);
            return result;
        }
        public static object Min(this IQuery query, LambdaExpression selector)
        {
            var result = CallGenericMethod(query, nameof(IQuery<object>.Min), selector);
            return result;
        }
        public static Task<object> MinAsync(this IQuery query, LambdaExpression selector)
        {
            var result = CallGenericMethodAsync(query, nameof(IQuery<object>.MinAsync), selector);
            return result;
        }


        public static bool Any(this IQuery query)
        {
            return CallMethod<bool>(query, nameof(IQuery<object>.Any), null);
        }
        public static Task<bool> AnyAsync(this IQuery query)
        {
            return CallMethodAsync<bool>(query, nameof(IQuery<object>.AnyAsync), null);
        }

        public static object First(this IQuery query)
        {
            return CallMethod(query, nameof(IQuery<object>.First), null);
        }
        public static Task<object> FirstAsync(this IQuery query)
        {
            return CallMethodAsync(query, nameof(IQuery<object>.FirstAsync), null);
        }

        public static IQuery Include(this IQuery q, LambdaExpression navigationProperty)
        {
            var method = q.GetType().GetMethod(nameof(IQuery<object>.Include));
            method = method.MakeGenericMethod(new Type[] { navigationProperty.Body.Type });
            IQuery query = (IQuery)method.FastInvoke(q, new object[] { navigationProperty });
            return query;
        }
        public static IQuery IncludeMany(this IQuery q, LambdaExpression navigationProperty)
        {
            var method = q.GetType().GetMethod(nameof(IQuery<object>.IncludeMany));
            method = method.MakeGenericMethod(new Type[] { navigationProperty.Body.Type.GetGenericArguments()[0] });
            IQuery query = (IQuery)method.FastInvoke(q, new object[] { navigationProperty });
            return query;
        }
        public static IQuery ThenInclude(this IQuery includableQuery, LambdaExpression navigationProperty)
        {
            var method = includableQuery.GetType().GetMethod(nameof(IIncludedObjectQuery<object, object>.ThenInclude));
            method = method.MakeGenericMethod(new Type[] { navigationProperty.Body.Type });
            IQuery query = (IQuery)method.FastInvoke(includableQuery, new object[] { navigationProperty });
            return query;
        }
        public static IQuery ThenIncludeMany(this IQuery includableQuery, LambdaExpression navigationProperty)
        {
            var method = includableQuery.GetType().GetMethod(nameof(IIncludedCollectionQuery<object, object>.ThenIncludeMany));
            method = method.MakeGenericMethod(new Type[] { navigationProperty.Body.Type.GetGenericArguments()[0] });
            IQuery query = (IQuery)method.FastInvoke(includableQuery, new object[] { navigationProperty });
            return query;
        }
        public static IQuery Filter(this IQuery includableQuery, LambdaExpression predicate)
        {
            var method = includableQuery.GetType().GetMethod(nameof(IIncludedCollectionQuery<object, object>.Filter));
            IQuery query = (IQuery)method.FastInvoke(includableQuery, new object[] { predicate });
            return query;
        }
        public static IQuery ExcludeField(this IQuery includableQuery, LambdaExpression field)
        {
            var method = includableQuery.GetType().GetMethod(nameof(IIncludedCollectionQuery<object, object>.ExcludeField));
            method = method.MakeGenericMethod(new Type[] { field.Body.Type });
            IQuery query = (IQuery)method.FastInvoke(includableQuery, new object[] { field });
            return query;
        }

        public static object Join(this IQuery q1, IQuery q2, JoinType joinType, LambdaExpression on)
        {
            var q2Type = q2.GetType().GetInterfaces().Where(a => a.GetGenericTypeDefinition() == typeof(IQuery<>)).First();
            var method = GetJoinMethod(q1).MakeGenericMethod(q2Type.GetGenericArguments()[0]);

            object joinQuery = method.FastInvoke(q1, new object[] { q2, joinType, on });
            return joinQuery;
        }

        public static IQuery Select(object joinQuery, LambdaExpression selector)
        {
            var selectMethod = GetJoinQuerySelectMethod(joinQuery);
            selectMethod = selectMethod.MakeGenericMethod(selector.Body.Type);
            IQuery query = (IQuery)selectMethod.FastInvoke(joinQuery, selector);
            return query;
        }

        public static IQuery<T> Exclude<T>(this IQuery<T> q, LambdaExpression field)
        {
            IQuery<T> query = (IQuery<T>)Exclude((IQuery)q, field);
            return query;
        }
        public static IQuery Exclude(this IQuery q, LambdaExpression field)
        {
            var method = q.GetType().GetMethod(nameof(IQuery<int>.Exclude));
            method = method.MakeGenericMethod(new Type[] { field.Body.Type });
            IQuery query = (IQuery)method.FastInvoke(q, new object[] { field });
            return query;
        }

        public static IQuery AsTracking(this IQuery query)
        {
            return CallMethod<IQuery>(query, nameof(IQuery<object>.AsTracking), null);
        }

        public static IQuery IgnoreAllFilters(this IQuery query)
        {
            return CallMethod<IQuery>(query, nameof(IQuery<object>.IgnoreAllFilters), null);
        }

        public static IQuery BindTwoWay(this IQuery query)
        {
            return CallMethod<IQuery>(query, nameof(IQuery<object>.BindTwoWay), null);
        }

        static MethodInfo GetJoinMethod(IQuery q)
        {
            var IQueryType = q.GetType().GetInterfaces().Where(a => a.GetGenericTypeDefinition() == typeof(IQuery<>)).First();
            var method = IQueryType.GetMethods().Where(a =>
            {
                if (a.Name != nameof(IQuery<object>.Join))
                    return false;

                var parameters = a.GetParameters();
                if (parameters.Length != 3)
                    return false;

                var firstParameter = parameters[0];

                if (!firstParameter.ParameterType.IsGenericType)
                    return false;

                var isIQueryType = firstParameter.ParameterType.GetGenericTypeDefinition() == typeof(IQuery<>);
                return isIQueryType;
            }).First();

            return method;
        }
        static MethodInfo GetJoinQuerySelectMethod(object joinQuery)
        {
            var IJoinQueryType = joinQuery.GetType().GetInterfaces().Where(a => a.GetGenericTypeDefinition() == typeof(IJoinQuery<,>)).First();
            var method = IJoinQueryType.GetMethod(nameof(IJoinQuery<object, object>.Select));

            return method;
        }
    }
}

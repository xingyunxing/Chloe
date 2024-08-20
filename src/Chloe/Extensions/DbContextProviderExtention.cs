using Chloe.Reflection;
using System.Reflection;

namespace Chloe
{
    internal static class DbContextProviderExtention
    {
        static MethodInfo QueryMethod = typeof(IDbContextProvider).GetMethod(nameof(IDbContextProvider.Query), new Type[] { typeof(string), typeof(LockType) });

        public static IQuery<TEntity> Query<TEntity>(this IDbContextProvider dbContextProvider)
        {
            return dbContextProvider.Query<TEntity>(null, LockType.Unspecified);
        }

        public static IQuery Query(this IDbContextProvider dbContextProvider, Type entityType, string tableName, LockType @lock)
        {
            MethodInfo method = QueryMethod.MakeGenericMethod(entityType);
            return (IQuery)method.FastInvoke(dbContextProvider, new object[] { tableName, @lock });
        }
    }
}

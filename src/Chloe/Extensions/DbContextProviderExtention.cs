namespace Chloe
{
    internal static class DbContextProviderExtention
    {
        public static IQuery<TEntity> Query<TEntity>(this IDbContextProvider dbContextProvider)
        {
            return dbContextProvider.Query<TEntity>(null, LockType.Unspecified);
        }
    }
}

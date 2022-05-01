namespace Chloe.SqlServer
{
    public static class MsSqlContextExtension
    {
        public static void BulkInsert<TEntity>(this IDbContext dbContext, List<TEntity> entities, string table = null, int? batchSize = null, int? bulkCopyTimeout = null, bool keepIdentity = false)
        {
            MsSqlContext msSqlContext = (MsSqlContext)dbContext;
            MsSqlContextProvider msSqlContextProvider = (MsSqlContextProvider)msSqlContext.DefaultDbContextProvider;
            msSqlContextProvider.BulkInsert<TEntity>(entities, table, batchSize, bulkCopyTimeout, keepIdentity);
        }
    }
}

using System.Linq.Expressions;

namespace Chloe.MySql
{
    public static class MySqlContextExtension
    {
        public static int Update<TEntity>(this IDbContext dbContext, Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, int limits)
        {
            return Update<TEntity>(dbContext, condition, content, null, limits);
        }
        public static int Update<TEntity>(this IDbContext dbContext, Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table, int limits)
        {
            MySqlContext mySqlContext = (MySqlContext)dbContext;
            MySqlContextProvider mySqlContextProvider = (MySqlContextProvider)mySqlContext.DefaultDbContextProvider;
            return mySqlContextProvider.Update<TEntity>(condition, content, table, limits);
        }

        public static int Delete<TEntity>(this IDbContext dbContext, Expression<Func<TEntity, bool>> condition, int limits)
        {
            return Delete<TEntity>(dbContext, condition, null, limits);
        }
        public static int Delete<TEntity>(this IDbContext dbContext, Expression<Func<TEntity, bool>> condition, string table, int limits)
        {
            MySqlContext mySqlContext = (MySqlContext)dbContext;
            MySqlContextProvider mySqlContextProvider = (MySqlContextProvider)mySqlContext.DefaultDbContextProvider;
            return mySqlContextProvider.Delete<TEntity>(condition, table, limits);
        }
    }
}

namespace Chloe.Extension
{
    static class Utils
    {
        public static DbParam[] BuildParams(IDbContext dbContext, object parameter)
        {
            DbContext dbContext1 = dbContext as DbContext;
            if (dbContext1 == null)
            {
                DbContextDecorator dbContextDecorator = dbContext as DbContextDecorator;

                if (dbContextDecorator != null)
                {
                    dbContext = dbContextDecorator.PersistedDbContext;
                    return BuildParams(dbContext, parameter);
                }

                var holdDbContextProp = dbContext.GetType().GetProperty("PersistedDbContext");
                if (holdDbContextProp != null)
                {
                    dbContext = Chloe.Reflection.ReflectionExtension.FastGetMemberValue(holdDbContextProp, dbContext) as IDbContext;
                    return BuildParams(dbContext, parameter);
                }
            }

            DbContextProvider dbContextProvider = dbContext1.DefaultDbContextProvider as DbContextProvider;
            return PublicHelper.BuildParams(dbContextProvider, parameter);
        }
    }
}

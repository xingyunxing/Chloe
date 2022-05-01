namespace Chloe.Extension
{
    static class Utils
    {
        public static DbParam[] BuildParams(IDbContext dbContext, object parameter)
        {
            var dbContextProvider = (dbContext as DbContext).DefaultDbContextProvider;
            return BuildParams(dbContextProvider, parameter);
        }
        public static DbParam[] BuildParams(IDbContextProvider dbContextProvider, object parameter)
        {
            DbContextProvider contextProvider = dbContextProvider as DbContextProvider;
            if (contextProvider == null)
            {
                var holdDbContextProp = dbContextProvider.GetType().GetProperty("HoldDbContext");
                if (holdDbContextProp != null)
                {
                    dbContextProvider = Chloe.Reflection.ReflectionExtension.FastGetMemberValue(holdDbContextProp, dbContextProvider) as IDbContextProvider;
                    return BuildParams(dbContextProvider, parameter);
                }
            }

            return PublicHelper.BuildParams(contextProvider, parameter);
        }
    }
}

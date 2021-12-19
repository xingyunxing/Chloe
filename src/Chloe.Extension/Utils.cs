namespace Chloe.Extension
{
    static class Utils
    {
        public static DbParam[] BuildParams(IDbContext dbContext, object parameter)
        {
            return PublicHelper.BuildParams((DbContext)dbContext, parameter);
        }
    }
}

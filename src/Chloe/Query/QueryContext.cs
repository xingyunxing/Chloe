namespace Chloe.Query
{
    public class QueryContext
    {
        public QueryContext(DbContextProvider dbContextProvider)
        {
            this.DbContextProvider = dbContextProvider;
        }

        public DbContextProvider DbContextProvider { get; set; }
    }
}

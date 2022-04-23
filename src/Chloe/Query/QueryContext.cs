namespace Chloe.Query
{
    class QueryContext
    {
        public QueryContext(DbContext dbContext)
        {
            this.DbContext = dbContext;
        }
        public DbContext DbContext { get; set; }
    }
}

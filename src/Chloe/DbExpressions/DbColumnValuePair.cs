namespace Chloe.DbExpressions
{
    public struct DbColumnValuePair
    {
        public DbColumnValuePair(DbColumn column, DbExpression value)
        {
            this.Column = column;
            this.Value = value;
        }

        public DbColumn Column { get; set; }
        public DbExpression Value { get; set; }
    }
}

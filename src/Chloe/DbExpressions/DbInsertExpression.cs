namespace Chloe.DbExpressions
{
    public class DbInsertExpression : DbExpression
    {
        public DbInsertExpression(DbTable table) : base(DbExpressionType.Insert, PublicConstants.TypeOfVoid)
        {
            PublicHelper.CheckNull(table);

            this.Table = table;
        }

        public DbTable Table { get; private set; }
        public List<DbColumnValuePair> InsertColumns { get; private set; } = new List<DbColumnValuePair>();
        public List<DbColumn> Returns { get; private set; } = new List<DbColumn>();
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public void AppendInsertColumn(DbColumn column, DbExpression value)
        {
            this.InsertColumns.Add(new DbColumnValuePair(column, value));
        }
    }
}

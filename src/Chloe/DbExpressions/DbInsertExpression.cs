namespace Chloe.DbExpressions
{
    public class DbInsertExpression : DbExpression
    {
        public DbInsertExpression(DbTable table) : this(table, 0, 0)
        {

        }

        public DbInsertExpression(DbTable table, int insertColumnCount, int returnCount) : base(DbExpressionType.Insert, PublicConstants.TypeOfVoid)
        {
            PublicHelper.CheckNull(table);

            this.Table = table;
            this.InsertColumns = new List<DbColumnValuePair>(insertColumnCount);
            this.Returns = new List<DbColumn>(returnCount);
        }

        public DbTable Table { get; private set; }
        public List<DbColumnValuePair> InsertColumns { get; private set; }
        public List<DbColumn> Returns { get; private set; }

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

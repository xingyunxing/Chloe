namespace Chloe.DbExpressions
{
    public class DbUpdateExpression : DbExpression
    {
        DbTable _table;

        public DbUpdateExpression(DbTable table) : this(table, null)
        {

        }

        public DbUpdateExpression(DbTable table, DbExpression condition) : base(DbExpressionType.Update, PublicConstants.TypeOfVoid)
        {
            PublicHelper.CheckNull(table);

            this._table = table;
            this.Condition = condition;
        }

        public DbTable Table { get { return this._table; } }
        public List<DbColumnValuePair> UpdateColumns { get; private set; } = new List<DbColumnValuePair>();
        public List<DbColumn> Returns { get; private set; } = new List<DbColumn>();
        public DbExpression Condition { get; set; }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public void AppendUpdateColumn(DbColumn column, DbExpression value)
        {
            this.UpdateColumns.Add(new DbColumnValuePair(column, value));
        }
    }
}

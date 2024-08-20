namespace Chloe.DbExpressions
{
    public class DbSubqueryExpression : DbExpression
    {
        DbSqlQueryExpression _sqlQuery;

        public DbSubqueryExpression(DbSqlQueryExpression sqlQuery) : base(DbExpressionType.Subquery)
        {
            this._sqlQuery = sqlQuery;
        }

        public DbSqlQueryExpression SqlQuery { get { return this._sqlQuery; } }
        public override Type Type { get { return this.SqlQuery.Type; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.VisitSubquery(this);
        }

    }
}

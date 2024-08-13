namespace Chloe.DbExpressions
{
    public class DbSqlQueryExpression : DbExpression
    {
        public DbSqlQueryExpression() : this(PublicConstants.TypeOfVoid)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">作为子句时，有时候可以指定返回的 Type，如 select A = (select id from users)，(select id from users) 就可以表示拥有一个返回的类型 Int</param>
        public DbSqlQueryExpression(Type type) : this(type, 0, 0, 0)
        {

        }

        public DbSqlQueryExpression(int columnSegmentCount, int groupSegmentCount, int orderingCount) : this(PublicConstants.TypeOfVoid, columnSegmentCount, groupSegmentCount, orderingCount)
        {

        }

        public DbSqlQueryExpression(Type type, int columnSegmentCount, int groupSegmentCount, int orderingCount) : base(DbExpressionType.SqlQuery, type)
        {
            this.ColumnSegments = new List<DbColumnSegment>(columnSegmentCount);
            this.GroupSegments = new List<DbExpression>(groupSegmentCount);
            this.Orderings = new List<DbOrdering>(orderingCount);
        }

        public bool IsDistinct { get; set; }
        public int? TakeCount { get; set; }
        public int? SkipCount { get; set; }
        public List<DbColumnSegment> ColumnSegments { get; private set; }
        public DbFromTableExpression Table { get; set; }
        public DbExpression Condition { get; set; }
        public List<DbExpression> GroupSegments { get; private set; }
        public DbExpression HavingCondition { get; set; }
        public List<DbOrdering> Orderings { get; private set; }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.VisitSqlQuery(this);
        }

        public DbSqlQueryExpression Update(Type type)
        {
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression(type, this.ColumnSegments.Count, this.GroupSegments.Count, this.Orderings.Count)
            {
                TakeCount = this.TakeCount,
                SkipCount = this.SkipCount,
                Table = this.Table,
                Condition = this.Condition,
                HavingCondition = this.HavingCondition,
                IsDistinct = this.IsDistinct
            };

            for (int i = 0; i < this.ColumnSegments.Count; i++)
            {
                sqlQuery.ColumnSegments.Add(this.ColumnSegments[i]);
            }

            for (int i = 0; i < this.GroupSegments.Count; i++)
            {
                sqlQuery.GroupSegments.Add(this.GroupSegments[i]);
            }

            for (int i = 0; i < this.Orderings.Count; i++)
            {
                sqlQuery.Orderings.Add(this.Orderings[i]);
            }

            return sqlQuery;
        }
    }
}

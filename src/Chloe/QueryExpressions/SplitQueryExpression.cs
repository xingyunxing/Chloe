namespace Chloe.QueryExpressions
{
    public class SplitQueryExpression : QueryExpression
    {
        public SplitQueryExpression(QueryExpression prevExpression) : base(QueryExpressionType.SplitQuery, prevExpression.ElementType, prevExpression)
        {
        }
        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.VisitSplitQuery(this);
        }
    }
}

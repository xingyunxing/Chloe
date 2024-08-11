namespace Chloe.QueryExpressions
{
    public class DistinctExpression : QueryExpression
    {
        public DistinctExpression(Type elementType, QueryExpression prevExpression) : base(QueryExpressionType.Distinct, elementType, prevExpression)
        {
        }
        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}

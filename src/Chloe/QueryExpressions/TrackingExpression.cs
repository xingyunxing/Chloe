namespace Chloe.QueryExpressions
{
    public class TrackingExpression : QueryExpression
    {
        public TrackingExpression(Type elementType, QueryExpression prevExpression) : base(QueryExpressionType.Tracking, elementType, prevExpression)
        {

        }

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.VisitTracking(this);
        }
    }
}

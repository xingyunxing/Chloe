namespace Chloe.QueryExpressions
{
    public class TrackingExpression : QueryExpression
    {
        public TrackingExpression(QueryExpression prevExpression) : base(QueryExpressionType.Tracking, prevExpression.ElementType, prevExpression)
        {

        }

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.VisitTracking(this);
        }
    }
}

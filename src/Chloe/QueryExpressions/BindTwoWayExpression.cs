namespace Chloe.QueryExpressions
{
    public class BindTwoWayExpression : QueryExpression
    {
        public BindTwoWayExpression(QueryExpression prevExpression) : base(QueryExpressionType.BindTwoWay, prevExpression.ElementType, prevExpression)
        {

        }

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.VisitBindTwoWay(this);
        }
    }
}

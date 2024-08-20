namespace Chloe.QueryExpressions
{
    public class IgnoreAllFiltersExpression : QueryExpression
    {
        public IgnoreAllFiltersExpression(QueryExpression prevExpression) : base(QueryExpressionType.IgnoreAllFilters, prevExpression.ElementType, prevExpression)
        {

        }

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.VisitIgnoreAllFilters(this);
        }
    }
}

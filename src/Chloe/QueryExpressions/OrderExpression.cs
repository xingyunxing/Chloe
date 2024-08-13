using System.Linq.Expressions;

namespace Chloe.QueryExpressions
{
    public class OrderExpression : QueryExpression
    {
        LambdaExpression _keySelector;
        public OrderExpression(Type elementType, QueryExpression prevExpression, QueryExpressionType expressionType, LambdaExpression keySelector) : base(expressionType, elementType, prevExpression)
        {
            this._keySelector = keySelector;
        }
        public LambdaExpression KeySelector
        {
            get { return this._keySelector; }
        }

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.VisitOrder(this);
        }
    }

}

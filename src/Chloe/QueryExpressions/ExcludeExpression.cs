using System.Linq.Expressions;

namespace Chloe.QueryExpressions
{
    public class ExcludeExpression : QueryExpression
    {
        LambdaExpression _field;

        public ExcludeExpression(Type elementType, QueryExpression prevExpression, LambdaExpression field) : base(QueryExpressionType.Exclude, elementType, prevExpression)
        {
            this._field = field;
        }

        /// <summary>
        /// a => a.Name || a => new { a.Name, a.Age } || a => new object[] { a.Name, a.Age }
        /// </summary>
        public LambdaExpression Field { get { return this._field; } }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}

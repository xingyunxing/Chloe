using System.Linq.Expressions;

namespace Chloe.QueryExpressions
{
    public class RootQueryExpression : QueryExpression
    {
        public RootQueryExpression(Type entityType, string explicitTable, LockType @lock) : base(QueryExpressionType.Root, entityType, null)
        {
            this.ExplicitTable = explicitTable;
            this.Lock = @lock;
        }

        public string ExplicitTable { get; private set; }
        public LockType Lock { get; private set; }

        public List<LambdaExpression> GlobalFilters { get; private set; } = new List<LambdaExpression>();
        public List<LambdaExpression> ContextFilters { get; private set; } = new List<LambdaExpression>();

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}

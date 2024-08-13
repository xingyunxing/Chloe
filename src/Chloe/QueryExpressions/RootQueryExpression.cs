using System.Linq.Expressions;

namespace Chloe.QueryExpressions
{
    public class RootQueryExpression : QueryExpression
    {
        public RootQueryExpression(Type entityType, string explicitTable, LockType @lock) : this(entityType, explicitTable, @lock, 0, 0)
        {

        }

        public RootQueryExpression(Type entityType, string explicitTable, LockType @lock, int globalFilterCount, int contextFilterCount) : base(QueryExpressionType.Root, entityType, null)
        {
            this.ExplicitTable = explicitTable;
            this.Lock = @lock;
            this.GlobalFilters = new List<LambdaExpression>(globalFilterCount);
            this.ContextFilters = new List<LambdaExpression>(contextFilterCount);
        }

        public string ExplicitTable { get; private set; }
        public LockType Lock { get; private set; }

        public List<LambdaExpression> GlobalFilters { get; private set; }
        public List<LambdaExpression> ContextFilters { get; private set; }

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.VisitRootQuery(this);
        }
    }
}

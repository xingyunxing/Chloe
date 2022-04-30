using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    class RootQueryExpression : QueryExpression
    {
        public RootQueryExpression(Type entityType, object provider, string explicitTable, LockType @lock) : base(QueryExpressionType.Root, entityType, null)
        {
            this.Provider = provider;
            this.ExplicitTable = explicitTable;
            this.Lock = @lock;
        }

        public object Provider { get; set; }

        public string ExplicitTable { get; private set; }
        public LockType Lock { get; private set; }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}

using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.QueryExpressions
{
    public class IncludeExpression : QueryExpression
    {
        public IncludeExpression(Type elementType, QueryExpression prevExpression, NavigationNode navigationNode) : base(QueryExpressionType.Include, elementType, prevExpression)
        {
            this.NavigationNode = navigationNode;
        }
        public NavigationNode NavigationNode { get; private set; }

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.VisitInclude(this);
        }
    }

    public class NavigationNode
    {
        public NavigationNode(PropertyInfo property) : this(property, 0, 0)
        {

        }

        public NavigationNode(PropertyInfo property, int globalFilterCount, int contextFilterCount)
        {
            this.Property = property;
            this.GlobalFilters = new List<LambdaExpression>(globalFilterCount);
            this.ContextFilters = new List<LambdaExpression>(contextFilterCount);
        }

        public PropertyInfo Property { get; set; }
        public List<LambdaExpression> ExcludedFields { get; private set; } = new List<LambdaExpression>();

        //只有导航属性是集合的时候才可能有值
        public LambdaExpression Condition { get; set; }

        public List<LambdaExpression> GlobalFilters { get; private set; }
        public List<LambdaExpression> ContextFilters { get; private set; }

        public NavigationNode Next { get; set; }

        public NavigationNode Clone()
        {
            NavigationNode current = new NavigationNode(this.Property, this.GlobalFilters.Count, this.ContextFilters.Count) { Condition = this.Condition };
            current.ExcludedFields.AppendRange(this.ExcludedFields);
            current.GlobalFilters.AddRange(this.GlobalFilters);
            current.ContextFilters.AddRange(this.ContextFilters);
            if (this.Next != null)
            {
                current.Next = this.Next.Clone();
            }

            return current;
        }

        public NavigationNode GetLast()
        {
            if (this.Next == null)
                return this;

            return this.Next.GetLast();
        }

        public override string ToString()
        {
            if (this.Next == null)
                return this.Property.Name;

            return $"{this.Property.Name}.{this.Next.ToString()}";
        }
    }
}

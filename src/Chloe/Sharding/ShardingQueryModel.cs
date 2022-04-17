using Chloe.Reflection;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    internal class ShardingQueryModel
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }

        public bool IgnoreAllFilters { get; set; }

        public List<LambdaExpression> Conditions { get; set; } = new List<LambdaExpression>();
        public List<Ordering> Orderings { get; set; } = new List<Ordering>();
        public List<LambdaExpression> GroupKeySelectors { get; private set; } = new List<LambdaExpression>();
        public LambdaExpression Selector { get; set; }

        public List<LambdaExpression> GlobalFilters { get; set; } = new List<LambdaExpression>();
        public List<LambdaExpression> ContextFilters { get; set; } = new List<LambdaExpression>();

        public List<OrderProperty> MakeOrderProperties()
        {
            List<OrderProperty> orders = new List<OrderProperty>(this.Orderings.Count);
            for (int i = 0; i < this.Orderings.Count; i++)
            {
                var ordering = this.Orderings[i];
                var valueGetter = MemberGetterContainer.Get(ordering.Member);
                var orderProperty = new OrderProperty() { Member = ordering.Member, Ascending = ordering.Ascending, ValueGetter = valueGetter };
                orders.Add(orderProperty);
            }

            return orders;
        }

        public bool HasSkip()
        {
            return this.Skip.HasValue && this.Skip.Value > 0;
        }
        public bool HasTake()
        {
            return this.Take.HasValue && this.Take.Value > 0;
        }
    }
}

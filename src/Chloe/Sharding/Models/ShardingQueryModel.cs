using System.Linq.Expressions;

namespace Chloe.Sharding
{
    internal class ShardingQueryModel
    {
        public ShardingQueryModel(Type rootEntityType)
        {
            this.RootEntityType = rootEntityType;
        }

        public Type RootEntityType { get; set; }

        public LockType Lock { get; set; }

        public int? Skip { get; set; }
        public int? Take { get; set; }

        public bool IsTracking { get; set; }
        public bool IgnoreAllFilters { get; set; }

        public List<LambdaExpression> Conditions { get; set; } = new List<LambdaExpression>();
        public List<Ordering> Orderings { get; set; } = new List<Ordering>();
        public List<LambdaExpression> GroupKeySelectors { get; private set; } = new List<LambdaExpression>();
        public LambdaExpression Selector { get; set; }

        public List<LambdaExpression> GlobalFilters { get; set; } = new List<LambdaExpression>();
        public List<LambdaExpression> ContextFilters { get; set; } = new List<LambdaExpression>();

        public IEnumerable<LambdaExpression> GetFinalConditions()
        {
            IEnumerable<LambdaExpression> conditions = this.Conditions;
            if (!this.IgnoreAllFilters)
            {
                conditions = conditions.Concat(this.GlobalFilters).Concat(this.ContextFilters);
            }

            return conditions;
        }

        public Type GetElementType()
        {
            if (this.Selector == null)
            {
                return this.RootEntityType;
            }

            return this.Selector.Body.Type;
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

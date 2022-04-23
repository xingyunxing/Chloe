using System.Linq.Expressions;

namespace Chloe.Sharding
{
    class DataQueryModel
    {
        public DataQueryModel(Type rootEntityType)
        {
            this.RootEntityType = rootEntityType;
        }

        public Type RootEntityType { get; set; }
        public IPhysicTable Table { get; set; }

        public int? Skip { get; set; }
        public int? Take { get; set; }

        public bool IgnoreAllFilters { get; set; }

        public List<LambdaExpression> Conditions { get; set; } = new List<LambdaExpression>();
        public List<Ordering> Orderings { get; set; } = new List<Ordering>();
        public LambdaExpression Selector { get; set; }
    }
}

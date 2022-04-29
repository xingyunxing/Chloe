using Chloe.Reflection;
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
        public List<LambdaExpression> GroupKeySelectors { get; set; } = new List<LambdaExpression>();
        public LambdaExpression Selector { get; set; }
    }

    class QueryProjection
    {
        public QueryProjection(Type rootEntityType)
        {
            this.RootEntityType = rootEntityType;
        }

        public Type RootEntityType { get; set; }

        public int? Skip { get; set; }
        public int? Take { get; set; }

        public bool IgnoreAllFilters { get; set; }

        public List<LambdaExpression> Conditions { get; set; } = new List<LambdaExpression>();
        public List<Ordering> Orderings { get; set; } = new List<Ordering>();
        public LambdaExpression Selector { get; set; }

        public List<OrderProperty> OrderProperties { get; set; } = new List<OrderProperty>();
        public MemberGetter ResultMapper { get; set; }

        public DataQueryModel CreateQueryModel(IPhysicTable table)
        {
            DataQueryModel queryModel = new DataQueryModel(this.RootEntityType);
            queryModel.Table = table;
            queryModel.Skip = this.Skip;
            queryModel.Take = this.Take;
            queryModel.IgnoreAllFilters = this.IgnoreAllFilters;

            queryModel.Conditions.Capacity = this.Conditions.Count;
            queryModel.Conditions.AddRange(this.Conditions);

            queryModel.Orderings.Capacity = this.Orderings.Capacity;
            queryModel.Orderings.AddRange(this.Orderings);

            queryModel.Selector = this.Selector;


            return queryModel;
        }
    }
}

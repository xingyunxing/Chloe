using Chloe.Descriptors;
using Chloe.Reflection;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    class DataQueryModel
    {
        public DataQueryModel(TypeDescriptor rootEntityTypeDescriptor)
        {
            this.RootEntityTypeDescriptor = rootEntityTypeDescriptor;
        }

        public TypeDescriptor RootEntityTypeDescriptor { get; set; }
        public Type RootEntityType => this.RootEntityTypeDescriptor.Definition.Type;

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
        public QueryProjection(TypeDescriptor rootEntityTypeDescriptor)
        {
            this.RootEntityTypeDescriptor = rootEntityTypeDescriptor;
        }

        public TypeDescriptor RootEntityTypeDescriptor { get; set; }
        public Type RootEntityType => this.RootEntityTypeDescriptor.Definition.Type;

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
            DataQueryModel queryModel = new DataQueryModel(this.RootEntityTypeDescriptor);
            queryModel.Table = table;
            queryModel.Skip = this.Skip;
            queryModel.Take = this.Take;
            queryModel.IgnoreAllFilters = this.IgnoreAllFilters;
            queryModel.Conditions.AddRange(this.Conditions);
            queryModel.Orderings.AddRange(this.Orderings);
            queryModel.Selector = this.Selector;


            return queryModel;
        }
    }
}

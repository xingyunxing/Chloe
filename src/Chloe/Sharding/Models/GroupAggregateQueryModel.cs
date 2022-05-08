using System.Linq.Expressions;

namespace Chloe.Sharding.Models
{
    class GroupAggregateQueryModel
    {
        public GroupAggregateQueryModel(Type rootEntityType)
        {
            this.RootEntityType = rootEntityType;
        }

        public Type RootEntityType { get; set; }
        public IPhysicTable Table { get; set; }
        public LockType Lock { get; set; }
        public List<LambdaExpression> Conditions { get; set; } = new List<LambdaExpression>();
        public List<LambdaExpression> GroupKeySelectors { get; set; } = new List<LambdaExpression>();
        public LambdaExpression Selector { get; set; }
    }
}

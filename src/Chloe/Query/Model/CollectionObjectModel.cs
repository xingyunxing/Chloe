using Chloe.DbExpressions;
using Chloe.Query.Mapping;
using System.Reflection;

namespace Chloe.Query
{
    public class CollectionObjectModel : ObjectModelBase
    {
        Type _collectionType;

        public CollectionObjectModel(QueryOptions queryOptions, Type ownerType, PropertyInfo associatedProperty, ComplexObjectModel elementModel) : base(queryOptions, associatedProperty.PropertyType)
        {
            this.OwnerType = ownerType;
            this.AssociatedProperty = associatedProperty;
            this._collectionType = associatedProperty.PropertyType;
            this.ElementModel = elementModel;
        }

        public override TypeKind TypeKind { get { return TypeKind.Collection; } }
        public ComplexObjectModel ElementModel { get; private set; }
        public Type OwnerType { get; private set; }
        public PropertyInfo AssociatedProperty { get; private set; }

        public override IObjectActivatorCreator GenarateObjectActivatorCreator(List<DbColumnSegment> columns, HashSet<string> aliasSet)
        {
            IObjectActivatorCreator elementActivatorCreator = this.ElementModel.GenarateObjectActivatorCreator(columns, aliasSet);
            CollectionObjectActivatorCreator ret = new CollectionObjectActivatorCreator(this._collectionType, this.OwnerType, elementActivatorCreator, this.QueryOptions.BindTwoWay);
            return ret;
        }

        public override void ExcludePrimitiveMember(LinkeNode<MemberInfo> memberLink)
        {
            this.ElementModel.ExcludePrimitiveMember(memberLink);
        }

        public override void ExcludePrimitiveMembers(IEnumerable<LinkeNode<MemberInfo>> memberLinks)
        {
            this.ElementModel.ExcludePrimitiveMembers(memberLinks);
        }
    }
}

using Chloe.Descriptors;
using Chloe.Reflection;
using System.Collections;

namespace Chloe.Query.SplitQuery
{
    /// <summary>
    /// 集合填充器
    /// </summary>
    public class CollectionStuffer
    {
        CollectionNavigationQueryExecutor _queryExecutor;
        ComplexPropertyDescriptor _thisSideNavigationDescriptor; //a.Owner

        InstanceCreator _collectionCreator;
        Dictionary<object, Tuple<object, IList>> _collectionMap; //以 owner.Id 为 key，(owner,owner.Collection) 为 value
        PrimitivePropertyDescriptor _foreignKeyDescriptor;

        PrimitivePropertyDescriptor _ownerIdDescriptor;
        CollectionPropertyDescriptor _collectionPropertyDescriptor;

        public CollectionStuffer(CollectionNavigationQueryExecutor queryExecutor, ComplexPropertyDescriptor thisSideNavigationDescriptor, PrimitivePropertyDescriptor ownerIdDescriptor)
        {
            this._queryExecutor = queryExecutor;
            this._thisSideNavigationDescriptor = thisSideNavigationDescriptor; //a.Owner
            this._foreignKeyDescriptor = thisSideNavigationDescriptor.ForeignKeyProperty; //a.OwnerId，外键
            this._ownerIdDescriptor = ownerIdDescriptor;
            this._collectionPropertyDescriptor = (CollectionPropertyDescriptor)this._queryExecutor.QueryNode.PropertyDescriptor;
        }

        public void InitCollection()
        {
            this._collectionCreator = InstanceCreatorContainer.Get(this._collectionPropertyDescriptor.PropertyType.GetDefaultConstructor());
            this._collectionMap = new Dictionary<object, Tuple<object, IList>>(this._queryExecutor.OwnerQueryExecutor.EntityCount);
            foreach (var owner in this._queryExecutor.OwnerQueryExecutor.Entities)
            {
                IList collection = (IList)this._collectionCreator();
                this._collectionPropertyDescriptor.SetValue(owner, collection);
                var ownerId = this._ownerIdDescriptor.GetValue(owner);
                this._collectionMap[ownerId] = new Tuple<object, IList>(owner, collection);
            }
        }

        public void BackFill(object entity)
        {
            object foreignKey = this._foreignKeyDescriptor.GetValue(entity);

            Tuple<object, IList> collection;
            if (!this._collectionMap.TryGetValue(foreignKey, out collection))
            {
                return;
            }

            collection.Item2.Add(entity);
            if (this._queryExecutor.QueryNode.BindTwoWay)
            {
                this._thisSideNavigationDescriptor.SetValue(entity, collection.Item1);
            }
        }
    }
}

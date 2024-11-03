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
        Dictionary<object, Tuple<object, IList, HashSet<object>>> _collectionMap; //以 owner.Id 为 key，(owner,owner.Collection, HashSet<object>) 为 value
        TypeDescriptor _elementTypeDescriptor;
        PrimitivePropertyDescriptor _foreignKeyDescriptor;

        PrimitivePropertyDescriptor _ownerIdDescriptor;
        CollectionPropertyDescriptor _collectionPropertyDescriptor;

        public CollectionStuffer(CollectionNavigationQueryExecutor queryExecutor, TypeDescriptor elementTypeDescriptor, ComplexPropertyDescriptor thisSideNavigationDescriptor, PrimitivePropertyDescriptor ownerIdDescriptor)
        {
            this._queryExecutor = queryExecutor;
            this._elementTypeDescriptor = elementTypeDescriptor;
            this._thisSideNavigationDescriptor = thisSideNavigationDescriptor; //a.Owner
            this._foreignKeyDescriptor = thisSideNavigationDescriptor.ForeignKeyProperty; //a.OwnerId，外键
            this._ownerIdDescriptor = ownerIdDescriptor;
            this._collectionPropertyDescriptor = (CollectionPropertyDescriptor)this._queryExecutor.QueryNode.PropertyDescriptor;
        }

        public void InitCollection()
        {
            this._collectionCreator = InstanceCreatorContainer.Get(this._collectionPropertyDescriptor.PropertyType.GetDefaultConstructor());
            this._collectionMap = new Dictionary<object, Tuple<object, IList, HashSet<object>>>(this._queryExecutor.PrevQueryExecutor.EntityCount);
            foreach (var owner in this._queryExecutor.PrevQueryExecutor.Entities)
            {
                IList collection = (IList)this._collectionCreator();
                this._collectionPropertyDescriptor.SetValue(owner, collection);
                var ownerId = this._ownerIdDescriptor.GetValue(owner);
                this._collectionMap[ownerId] = new Tuple<object, IList, HashSet<object>>(owner, collection, new HashSet<object>());
            }
        }

        public void BackFill(object entity)
        {
            object foreignKey = this._foreignKeyDescriptor.GetValue(entity);

            Tuple<object, IList, HashSet<object>> collection;
            if (!this._collectionMap.TryGetValue(foreignKey, out collection))
            {
                return;
            }

            object id = this._elementTypeDescriptor.PrimaryKeys[0].GetValue(entity);

            if (collection.Item3.Contains(id))
            {
                //已经存在则不重复添加。因为连接查询结果集有重复的数据
                return;
            }

            collection.Item2.Add(entity);
            collection.Item3.Add(id);
            if (this._queryExecutor.QueryNode.BindTwoWay)
            {
                this._thisSideNavigationDescriptor.SetValue(entity, collection.Item1);
            }
        }
    }
}

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
        Dictionary<object, List<Tuple<object, IList, HashSet<object>>>> _collectionMap; //以 owner.Id 为 key，List<(owner,owner.Collection, HashSet<object>)> 为 value
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
            this._collectionMap = new Dictionary<object, List<Tuple<object, IList, HashSet<object>>>>(this._queryExecutor.PrevQueryExecutor.EntityCount);
            foreach (var owner in this._queryExecutor.PrevQueryExecutor.Entities)
            {
                var ownerId = this._ownerIdDescriptor.GetValue(owner);

                /*
                 * 要用集合存
                 * 比如此类查询：dbContext.Query<GoodsSku>().Include(a => a.Goods).ThenIncludeMany(a => a.GoodsSupplierMaps).ThenInclude(a => a.Supplier).Where(a => a.GoodsId == 18220).SplitQuery().ToList();
                 * 保证每个 Goods 的 GoodsSupplierMaps 都能填充上
                 */
                List<Tuple<object, IList, HashSet<object>>> ownerContainer;
                if (!this._collectionMap.TryGetValue(ownerId, out ownerContainer))
                {
                    ownerContainer = new List<Tuple<object, IList, HashSet<object>>>();
                    this._collectionMap[ownerId] = ownerContainer;
                }

                IList collection = (IList)this._collectionCreator();//导航集合
                this._collectionPropertyDescriptor.SetValue(owner, collection);

                var ownerBak = new Tuple<object, IList, HashSet<object>>(owner, collection, new HashSet<object>()/* 用于记录已经往导航集合添加过的数据。因为连接查询结果集有重复的数据，所以要记录起来，防止重复添加 */);
                ownerContainer.Add(ownerBak);
            }
        }

        public void BackFill(object entity)
        {
            object foreignKey = this._foreignKeyDescriptor.GetValue(entity);

            List<Tuple<object, IList, HashSet<object>>> ownerContainer;
            if (!this._collectionMap.TryGetValue(foreignKey, out ownerContainer))
            {
                return;
            }

            for (int i = 0; i < ownerContainer.Count; i++)
            {
                Tuple<object, IList, HashSet<object>> ownerBak = ownerContainer[i];

                object id = this._elementTypeDescriptor.PrimaryKeys[0].GetValue(entity);
                if (ownerBak.Item3.Contains(id))
                {
                    //已经存在则不重复添加。因为连接查询结果集有重复的数据
                    return;
                }

                ownerBak.Item2.Add(entity);
                ownerBak.Item3.Add(id);
                if (this._queryExecutor.QueryNode.BindTwoWay)
                {
                    this._thisSideNavigationDescriptor.SetValue(entity, ownerBak.Item1);
                }
            }
        }
    }
}

using Chloe.Descriptors;
using Chloe.Exceptions;
using Chloe.Extensions;
using Chloe.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query.SplitQuery
{
    public class CollectionNavigationQueryExecutor : SplitQueryExecutor
    {
        QueryContext _queryContext;
        IList _entities;

        SplitQueryExecutor _prevQueryExecutor;
        CollectionStuffer _stuffer;
        SplitQueryNavigationNode _queryNode;

        PrimitivePropertyDescriptor _ownerIdDescriptor;

        //Item.Owner，外键对应的导航属性 
        ComplexPropertyDescriptor _thisSideNavigationDescriptor;

        public CollectionNavigationQueryExecutor(QueryContext queryContext, SplitQueryNavigationNode queryNode, SplitQueryExecutor prevQueryExecutor, List<SplitQueryExecutor> navigationQueryExecutors) : base(navigationQueryExecutors)
        {
            this._queryContext = queryContext;
            this._queryNode = queryNode;
            this._prevQueryExecutor = prevQueryExecutor;
            this._ownerIdDescriptor = queryNode.PrevNode.ElementTypeDescriptor.PrimaryKeys.FirstOrDefault();

            //a.Owner
            this._thisSideNavigationDescriptor = queryNode.ElementTypeDescriptor.ComplexPropertyDescriptors.Where(a => a.PropertyType == queryNode.PrevNode.ElementTypeDescriptor.EntityType).FirstOrDefault();
            if (this._thisSideNavigationDescriptor == null)
            {
                throw new ChloeException($"You have to define a navigation property which type is '{queryNode.PrevNode.ElementTypeDescriptor.EntityType.FullName}' on class '{queryNode.ElementTypeDescriptor.Definition.Type.FullName}'.");
            }

            this._stuffer = new CollectionStuffer(this, queryNode.ElementTypeDescriptor, this._thisSideNavigationDescriptor, this._ownerIdDescriptor);
        }

        public SplitQueryNavigationNode QueryNode { get { return this._queryNode; } }

        public SplitQueryExecutor PrevQueryExecutor { get { return this._prevQueryExecutor; } }

        public override IEnumerable<object> Entities { get { return this._entities.AsGenericEnumerable(); } }

        public override int EntityCount { get { return this._entities.Count; } }

        public override async Task ExecuteQuery(bool @async)
        {
            this._entities = await this.LoadEntities(@async);
            await base.ExecuteQuery(@async);
        }

        public override void ExecuteBackFill()
        {
            this._stuffer.InitCollection();
            foreach (var entity in this._entities)
            {
                this._stuffer.BackFill(entity);
            }

            base.ExecuteBackFill();
        }

        async Task<IList> LoadEntities(bool @async)
        {
            if (this._prevQueryExecutor.EntityCount == 0)
            {
                return new List<object>();
            }

            IQuery query = this.MakeQuery(false);

            IList entities;
            if (@async)
            {
                entities = await query.ToListAsync();
            }
            else
            {
                entities = query.ToList();
            }

            return entities;
        }

        public override IQuery GetDependQuery(SplitQueryNode fromNode)
        {
            IQuery query = this.MakeQuery(true);

            TypeDescriptor entityTypeDescriptor = this._queryNode.ElementTypeDescriptor;

            //thisNode:fromNode 的关系是 1:N
            ComplexPropertyDescriptor fromNodeElementDotOwner_Descriptor = fromNode.ElementTypeDescriptor.GetComplexPropertyDescriptorByPropertyType(this._queryNode.ElementType);

            PropertyInfo associatedThisSideKey = fromNodeElementDotOwner_Descriptor.GetOtherSideProperty(entityTypeDescriptor);  //this.AssociatedKey

            var a = Expression.Parameter(entityTypeDescriptor.EntityType, "a");
            var associatedThisSideKeyExp = Expression.MakeMemberAccess(a, associatedThisSideKey); //a.AssociatedKey
            var associatedThisSideKeySelector = Expression.Lambda(associatedThisSideKeyExp, a); //a => a.AssociatedKey

            query = query.Select(associatedThisSideKeySelector);
            return query;
        }

        IQuery MakeQuery(bool ignoreIncludedNavigations)
        {
            SplitQueryNavigationNode queryNode = this._queryNode;

            IQuery query = this._queryContext.DbContextProvider.Query(queryNode.ElementType, null, queryNode.Lock);

            if (queryNode.IsTrackingQuery)
            {
                query = query.AsTracking();
            }

            if (queryNode.IgnoreAllFilters)
            {
                query = query.IgnoreAllFilters();
            }

            if (queryNode.BindTwoWay)
            {
                query = query.BindTwoWay();
            }

            for (int i = 0; i < queryNode.Conditions.Count; i++)
            {
                query = query.Where(queryNode.Conditions[i]);
            }

            for (int i = 0; i < queryNode.ExcludedFields.Count; i++)
            {
                query = query.Exclude(queryNode.ExcludedFields[i]);
            }

            if (!ignoreIncludedNavigations)
                query = IncludeNavigation(query, queryNode, Array.Empty<LambdaExpression>());

            if (this._prevQueryExecutor.EntityCount != 0 && this._prevQueryExecutor.EntityCount <= 512)
            {
                //少于一定数量，直接用 in 查询
                query = this.MakeInQuery(query, this._thisSideNavigationDescriptor);
                return query;
            }

            IQuery dependQuery = this._prevQueryExecutor.GetDependQuery(this._queryNode);

            ParameterExpression p1 = Expression.Parameter(dependQuery.ElementType, "p1"); //p1, p1 is owner.Id
            ParameterExpression p2 = Expression.Parameter(query.ElementType, "p2"); //p2

            Expression foreignKey = Expression.MakeMemberAccess(p2, this._thisSideNavigationDescriptor.ForeignKeyProperty.Property); //p2.OwnerId
            if (foreignKey.Type != p1.Type)
                foreignKey = Expression.Convert(foreignKey, p1.Type); //(int)p2.OwnerId

            Expression eq = Expression.Equal(p1, foreignKey); //p1 == p2.OwnerId

            Type delegateType = typeof(Func<,,>).MakeGenericType(p1.Type, p2.Type, typeof(bool)); //Func<P1, P2, bool>
            LambdaExpression on = Expression.Lambda(delegateType, eq, p1, p2); //(p1, p2) => p1 == p2.OwnerId

            Type selectorDelegateType = typeof(Func<,,>).MakeGenericType(p1.Type, p2.Type, p2.Type); //Func<P1, P2, P2>
            LambdaExpression selector = Expression.Lambda(selectorDelegateType, p2, p1, p2); //(p1, p2) => p2

            object joinQuery = dependQuery.Join(query, JoinType.InnerJoin, on); //dependQuery.Join(query, (p1, p2) => p1 == p2.OwnerId)
            IQuery retQuery = QueryExtension.Select(joinQuery, selector); //joinQuery.Select((p1, p2) => p2)

            return retQuery;
        }

        IQuery MakeInQuery(IQuery query, ComplexPropertyDescriptor navigationDescriptor)
        {
            var listConstructor = typeof(List<>).MakeGenericType(this._ownerIdDescriptor.PropertyType).GetConstructor(Type.EmptyTypes);
            InstanceCreator listCreator = InstanceCreatorContainer.Get(listConstructor);
            IList ownerIds = (IList)listCreator();
            foreach (object owner in this._prevQueryExecutor.Entities)
            {
                var ownerId = this._ownerIdDescriptor.GetValue(owner);
                ownerIds.Add(ownerId);
            }

            Expression conditionBody = null;

            var a = Expression.Parameter(query.ElementType); //a
            Expression foreignKeyAccess = Expression.MakeMemberAccess(a, navigationDescriptor.ForeignKeyProperty.Property); //a.OwnerId

            var foreignKeyAccessExp = foreignKeyAccess;
            if (foreignKeyAccessExp.Type != this._ownerIdDescriptor.PropertyType)
            {
                foreignKeyAccessExp = Expression.Convert(foreignKeyAccessExp, this._ownerIdDescriptor.PropertyType);
            }

            if (ownerIds.Count == 1)
            {
                var ownerId = ExpressionExtension.MakeWrapperAccess(ownerIds[0], this._ownerIdDescriptor.PropertyType);
                conditionBody = Expression.Equal(foreignKeyAccessExp, ownerId); //a.OwnerId == ownerId
            }
            else
            {
                /*
                 * 注：不能用常量表达式（Expression.Constant(ownerIds)）包装，因为查询时框架有对解析结果进行缓存，会根据表达式树计算 hash 存储键，
                 * 如果用常量会则会导致每次查询都是新的键，导致始终不会命中缓存，而且会持续往缓存里添加数据，最后导致程序占用内存持续增长！！！
                 */
                var ownerIdsWrapper = ExpressionExtension.MakeWrapperAccess(ownerIds, ownerIds.GetType());

                Expression containsCall = Expression.Call(ownerIdsWrapper, ownerIds.GetType().GetMethod(nameof(List<object>.Contains)), foreignKeyAccessExp); //ownerIds.Contains(a.OwnerId)
                conditionBody = containsCall;
            }

            LambdaExpression condition = LambdaExpression.Lambda(typeof(Func<,>).MakeGenericType(query.ElementType, typeof(bool)), conditionBody, a); //a => ownerIds.Contains(a.OwnerId)
            query = query.Where(condition);
            return query;
        }
    }
}

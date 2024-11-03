using Chloe.Descriptors;
using Chloe.Exceptions;
using System.Linq.Expressions;

namespace Chloe.Query.SplitQuery
{
    public class ComplexNavigationQueryExecutor : SplitQueryExecutor
    {
        QueryContext _queryContext;
        IEnumerable<object> _entities;

        SplitQueryNavigationNode _queryNode;
        SplitQueryExecutor _prevQueryExecutor;

        public ComplexNavigationQueryExecutor(QueryContext queryContext, SplitQueryNavigationNode queryNode, SplitQueryExecutor prevQueryExecutor, List<SplitQueryExecutor> navigationQueryExecutors) : base(navigationQueryExecutors)
        {
            this._queryContext = queryContext;
            this._queryNode = queryNode;
            this._prevQueryExecutor = prevQueryExecutor;
        }

        public override IEnumerable<object> Entities { get { return this._entities; } }

        public override int EntityCount
        {
            get
            {
                /* 
                 * 理应返回 this.Entities.Count()，但为了减少一次循环，所以直接使用 this._ownerQueryExecutor.EntityCount
                 */
                return this._prevQueryExecutor.EntityCount;
            }
        }

        public override async Task ExecuteQuery(bool @async)
        {
            ComplexPropertyDescriptor propertyDescriptor = (ComplexPropertyDescriptor)this._queryNode.PropertyDescriptor;
            this._entities = this._prevQueryExecutor.Entities.Select(a => propertyDescriptor.GetValue(a)).Where(a => a != null); //因为做了 null 过滤，所以有可能 this.EntityCount != this.Entities.Count()
            await base.ExecuteQuery(@async);
        }

        public override IQuery GetDependQuery(SplitQueryNode fromNode)
        {
            IQuery query = this.MakeQuery();

            var collectionPropertyDescriptor = this._queryNode.ElementTypeDescriptor.CollectionPropertyDescriptors.Where(a => a.ElementType == fromNode.ElementType).FirstOrDefault();

            if (collectionPropertyDescriptor != null)
            {
                //thisNode:fromNode 的关系是 1:N
                TypeDescriptor entityTypeDescriptor = this._queryNode.ElementTypeDescriptor;
                var a = Expression.Parameter(entityTypeDescriptor.EntityType, "a");
                var id = Expression.MakeMemberAccess(a, entityTypeDescriptor.PrimaryKeys[0].Property); //a.Id
                var idSelector = Expression.Lambda(id, a); //a => a.Id

                query = query.Select(idSelector);
                return query;
            }
            else
            {
                var complexPropertyDescriptor = this._queryNode.ElementTypeDescriptor.ComplexPropertyDescriptors.Where(a => a.PropertyType == fromNode.ElementType).FirstOrDefault();

                //thisNode:fromNode 的关系是 N:1
                TypeDescriptor entityTypeDescriptor = this._queryNode.ElementTypeDescriptor;
                var a = Expression.Parameter(entityTypeDescriptor.EntityType, "a");
                var ownerId = Expression.MakeMemberAccess(a, complexPropertyDescriptor.ForeignKeyProperty.Property); //a.OwnerId
                var ownerIdSelector = Expression.Lambda(ownerId, a); //a => a.OwnerId

                query = query.Select(ownerIdSelector);
                return query;
            }
        }

        IQuery MakeQuery()
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

            for (int i = 0; i < queryNode.ExcludedFields.Count; i++)
            {
                query = query.Exclude(queryNode.ExcludedFields[i]);
            }

            ComplexPropertyDescriptor navigationDescriptor = queryNode.ElementTypeDescriptor.ComplexPropertyDescriptors.Where(a => a.PropertyType == queryNode.PrevNode.ElementTypeDescriptor.EntityType).FirstOrDefault();

            if (navigationDescriptor == null)
            {
                var collectionPropertyDescriptor = queryNode.ElementTypeDescriptor.CollectionPropertyDescriptors.Where(a => a.ElementType == queryNode.PrevNode.ElementTypeDescriptor.EntityType).FirstOrDefault();

                if (collectionPropertyDescriptor == null)
                {
                    throw new ChloeException($"Can not find navigation property which type is '{queryNode.PrevNode.ElementTypeDescriptor.EntityType.FullName}' on class '{queryNode.ElementTypeDescriptor.Definition.Type.FullName}'.");
                }

                //thisNode:prevNode 的关系是 1:N
                IQuery dependQuery = this._prevQueryExecutor.GetDependQuery(this._queryNode);

                ParameterExpression p1 = Expression.Parameter(dependQuery.ElementType, "p1"); //p1, p1 is prevNode.OwnerId
                ParameterExpression p2 = Expression.Parameter(query.ElementType, "p2"); //p2

                Expression keySelector = Expression.MakeMemberAccess(p2, this._queryNode.ElementTypeDescriptor.PrimaryKeys[0].Property); //p2.Id
                if (keySelector.Type != p1.Type)
                    keySelector = Expression.Convert(keySelector, p1.Type); //(int)p2.Id

                Expression eq = Expression.Equal(p1, keySelector); //p1 == p2.Id

                Type delegateType = typeof(Func<,,>).MakeGenericType(p1.Type, p2.Type, typeof(bool)); //Func<P1, P2, bool>
                LambdaExpression on = Expression.Lambda(delegateType, eq, p1, p2); //(p1, p2) => p1 == p2.Id

                Type selectorDelegateType = typeof(Func<,,>).MakeGenericType(p1.Type, p2.Type, p2.Type); //Func<P1, P2, P2>
                LambdaExpression selector = Expression.Lambda(selectorDelegateType, p2, p1, p2); //(p1, p2) => p2

                object joinQuery = dependQuery.Join(query, JoinType.InnerJoin, on); //dependQuery.Join(query, (p1, p2) => p1 == p2.OwnerId)
                IQuery retQuery = QueryExtension.Select(joinQuery, selector); //joinQuery.Select((p1, p2) => p2)

                return retQuery;
            }
            else
            {
                //thisNode:prevNode 的关系是 N:1
                IQuery dependQuery = this._prevQueryExecutor.GetDependQuery(this._queryNode);

                ParameterExpression p1 = Expression.Parameter(dependQuery.ElementType, "p1"); //p1, p1 is owner.Id
                ParameterExpression p2 = Expression.Parameter(query.ElementType, "p2"); //p2

                Expression foreignKey = Expression.MakeMemberAccess(p2, navigationDescriptor.ForeignKeyProperty.Property); //p2.OwnerId
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
        }

    }
}

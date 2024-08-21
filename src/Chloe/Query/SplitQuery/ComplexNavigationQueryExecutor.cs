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
        SplitQueryExecutor _ownerQueryExecutor;

        public ComplexNavigationQueryExecutor(QueryContext queryContext, SplitQueryNavigationNode queryNode, SplitQueryExecutor ownerQueryExecutor, List<SplitQueryExecutor> navigationQueryExecutors) : base(navigationQueryExecutors)
        {
            this._queryContext = queryContext;
            this._queryNode = queryNode;
            this._ownerQueryExecutor = ownerQueryExecutor;
        }

        public override IEnumerable<object> Entities { get { return this._entities; } }

        public override int EntityCount
        {
            get
            {
                /* 
                 * 理应返回 this.Entities.Count()，但为了减少一次循环，所以直接使用 this._ownerQueryExecutor.EntityCount
                 */
                return this._ownerQueryExecutor.EntityCount;
            }
        }

        public override async Task ExecuteQuery(bool @async)
        {
            ComplexPropertyDescriptor propertyDescriptor = (ComplexPropertyDescriptor)this._queryNode.PropertyDescriptor;
            this._entities = this._ownerQueryExecutor.Entities.Select(a => propertyDescriptor.GetValue(a)).Where(a => a != null); //因为做了 null 过滤，所以有可能 this.EntityCount != this.Entities.Count()
            await base.ExecuteQuery(@async);
        }

        public override IQuery GetDependQuery()
        {
            IQuery query = this.MakeQuery();

            TypeDescriptor entityTypeDescriptor = this._queryNode.ElementTypeDescriptor;
            var a = Expression.Parameter(entityTypeDescriptor.EntityType, "a");
            var id = Expression.MakeMemberAccess(a, entityTypeDescriptor.PrimaryKeys[0].Property); //a.Id
            var idSelector = Expression.Lambda(id, a); //a => a.Id

            query = query.Select(idSelector);
            return query;
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

            //a.Owner
            ComplexPropertyDescriptor navigationDescriptor = queryNode.ElementTypeDescriptor.ComplexPropertyDescriptors.Where(a => a.PropertyType == queryNode.Owner.ElementTypeDescriptor.EntityType).FirstOrDefault();

            if (navigationDescriptor == null)
            {
                throw new ChloeException($"You have to define a navigation property which type is '{queryNode.Owner.ElementTypeDescriptor.EntityType.FullName}' on class '{queryNode.ElementTypeDescriptor.Definition.Type.FullName}'.");
            }

            IQuery dependQuery = this._ownerQueryExecutor.GetDependQuery();

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

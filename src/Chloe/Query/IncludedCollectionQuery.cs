using Chloe.Extensions;
using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Query
{
    class IncludedCollectionQuery<TEntity, TItem> : IncludeQueryBase<TEntity, TItem>, IIncludedCollectionQuery<TEntity, TItem>
    {
        public IncludedCollectionQuery(DbContextProvider dbContextProvider, QueryExpression prevExpression, LambdaExpression navigationPath) : this(dbContextProvider, BuildIncludeExpression(dbContextProvider, prevExpression, navigationPath))
        {

        }

        public IncludedCollectionQuery(DbContextProvider dbContextProvider, QueryExpression exp) : base(dbContextProvider, exp)
        {

        }

        public IIncludedCollectionQuery<TEntity, TItem> Filter(Expression<Func<TItem, bool>> predicate)
        {
            IncludeExpression prevIncludeExpression = this.QueryExpression as IncludeExpression;
            NavigationNode startNavigation = prevIncludeExpression.NavigationNode.Clone();
            NavigationNode lastNavigation = startNavigation.GetLast();
            lastNavigation.Condition = lastNavigation.Condition.AndAlso(predicate);

            IncludeExpression includeExpression = new IncludeExpression(typeof(TEntity), prevIncludeExpression.PrevExpression, startNavigation);

            return new IncludedCollectionQuery<TEntity, TItem>(this.DbContextProvider, includeExpression);
        }

        public IIncludedCollectionQuery<TEntity, TItem> ExcludeField<TField>(Expression<Func<TItem, TField>> field)
        {
            IncludeExpression includeExpression = this.BuildExcludeFieldIncludeExpression(field);
            return new IncludedCollectionQuery<TEntity, TItem>(this.DbContextProvider, includeExpression);
        }
    }
}

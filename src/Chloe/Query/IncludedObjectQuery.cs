using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Query
{
    class IncludedObjectQuery<TEntity, TNavigation> : IncludeQueryBase<TEntity, TNavigation>, IIncludedObjectQuery<TEntity, TNavigation>
    {
        public IncludedObjectQuery(DbContextProvider dbContextProvider, QueryExpression prevExpression, LambdaExpression navigationPath) : this(dbContextProvider, BuildIncludeExpression(dbContextProvider, prevExpression, navigationPath))
        {

        }
        public IncludedObjectQuery(DbContextProvider dbContextProvider, QueryExpression exp) : base(dbContextProvider, exp)
        {

        }

        public IIncludedObjectQuery<TEntity, TNavigation> ExcludeField<TField>(Expression<Func<TNavigation, TField>> field)
        {
            IncludeExpression includeExpression = this.BuildExcludeFieldIncludeExpression(field);
            return new IncludedObjectQuery<TEntity, TNavigation>(this.DbContextProvider, includeExpression);
        }
    }
}

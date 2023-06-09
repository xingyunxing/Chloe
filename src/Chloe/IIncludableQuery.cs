using System.Linq.Expressions;

namespace Chloe
{
    public interface IIncludableQuery<TEntity, TNavigation> : IQuery<TEntity>
    {
        /// <summary>
        /// Exclude specified field
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field">a => a.Name || a => new { a.Name, a.Age } || a => new object[] { a.Name, a.Age }</param>
        /// <returns></returns>
        IIncludableQuery<TEntity, TNavigation> ExcludeField<TField>(Expression<Func<TNavigation, TField>> field);
        /// <summary>
        /// 对导航属性过滤：dbContext.Query&lt;City&gt;().IncludeMany(a =&gt; a.Users).Filter(a =&gt; a.Age &gt;= 18) --&gt; select ... from City left join User on City.Id=User.CityId and User.Age &gt;= 18
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IIncludableQuery<TEntity, TNavigation> Filter(Expression<Func<TNavigation, bool>> predicate);
        IIncludableQuery<TEntity, TProperty> ThenInclude<TProperty>(Expression<Func<TNavigation, TProperty>> navigationPath);
        IIncludableQuery<TEntity, TCollectionItem> ThenIncludeMany<TCollectionItem>(Expression<Func<TNavigation, IEnumerable<TCollectionItem>>> navigationPath);
    }
}

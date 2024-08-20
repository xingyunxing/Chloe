using System.Linq.Expressions;

namespace Chloe
{
    public interface IIncludedCollectionQuery<TEntity, TItem> : IQuery<TEntity>
    {
        /// <summary>
        /// 对导航属性过滤：dbContext.Query&lt;City&gt;().IncludeMany(a =&gt; a.Users).Filter(a =&gt; a.Age &gt;= 18) --&gt; select ... from City left join User on City.Id=User.CityId and User.Age &gt;= 18
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IIncludedCollectionQuery<TEntity, TItem> Filter(Expression<Func<TItem, bool>> predicate);

        /// <summary>
        /// Exclude specified field
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field">a => a.Name || a => new { a.Name, a.Age } || a => new object[] { a.Name, a.Age }</param>
        /// <returns></returns>
        IIncludedCollectionQuery<TEntity, TItem> ExcludeField<TField>(Expression<Func<TItem, TField>> field);

        IIncludedObjectQuery<TEntity, TProperty> ThenInclude<TProperty>(Expression<Func<TItem, TProperty>> navigationPath);

        IIncludedCollectionQuery<TEntity, TCollectionItem> ThenIncludeMany<TCollectionItem>(Expression<Func<TItem, IEnumerable<TCollectionItem>>> navigationPath);
    }
}

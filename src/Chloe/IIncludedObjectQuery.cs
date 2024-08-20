using System.Linq.Expressions;

namespace Chloe
{
    public interface IIncludedObjectQuery<TEntity, TNavigation> : IQuery<TEntity>
    {
        /// <summary>
        /// Exclude specified field
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field">a => a.Name || a => new { a.Name, a.Age } || a => new object[] { a.Name, a.Age }</param>
        /// <returns></returns>
        IIncludedObjectQuery<TEntity, TNavigation> ExcludeField<TField>(Expression<Func<TNavigation, TField>> field);
        IIncludedObjectQuery<TEntity, TProperty> ThenInclude<TProperty>(Expression<Func<TNavigation, TProperty>> navigationPath);
        IIncludedCollectionQuery<TEntity, TCollectionItem> ThenIncludeMany<TCollectionItem>(Expression<Func<TNavigation, IEnumerable<TCollectionItem>>> navigationPath);
    }
}

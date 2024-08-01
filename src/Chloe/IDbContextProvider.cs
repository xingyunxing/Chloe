using System.Data;
using System.Linq.Expressions;

namespace Chloe
{
    public interface IDbContextProvider : IDisposable
    {
        IDbSessionProvider Session { get; }
        void TrackEntity(object entity);

        /// <summary>
        /// 针对当前上下文设置过滤器。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="filter"></param>
        void HasQueryFilter<TEntity>(Expression<Func<TEntity, bool>> filter);
        void HasQueryFilter(Type entityType, LambdaExpression filter);

        IQuery<TEntity> Query<TEntity>(string table, LockType @lock);

        List<T> SqlQuery<T>(string sql, CommandType cmdType, params DbParam[] parameters);
        Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, params DbParam[] parameters);

        List<T> SqlQuery<T>(string sql, CommandType cmdType, object parameter);
        Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, object parameter);

        /// <summary>
        /// 插入数据，连同导航属性一并插入。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        TEntity Save<TEntity>(TEntity entity);
        Task<TEntity> SaveAsync<TEntity>(TEntity entity);

        /// <summary>
        /// 插入数据，但不包括导航属性。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        TEntity Insert<TEntity>(TEntity entity, string table);
        object Insert<TEntity>(Expression<Func<TEntity>> content, string table);

        Task<TEntity> InsertAsync<TEntity>(TEntity entity, string table);
        Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content, string table);

        void InsertRange<TEntity>(List<TEntity> entities, int? batchSize, string table);
        Task InsertRangeAsync<TEntity>(List<TEntity> entities, int? batchSize, string table);

        int Update<TEntity>(TEntity entity, string table);
        /// <summary>
        /// context.Update&lt;User&gt;(a => a.Id == 1, a => new User() { Name = "lu", Age = a.Age + 1, Gender = Gender.Female, OpTime = DateTime.Now })
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="condition"></param>
        /// <param name="content"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table);

        Task<int> UpdateAsync<TEntity>(TEntity entity, string table);
        Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table);

        int Delete<TEntity>(TEntity entity, string table);
        /// <summary>
        /// context.Delete&lt;User&gt;(a => a.Id == 1)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        int Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table);

        Task<int> DeleteAsync<TEntity>(TEntity entity, string table);
        Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition, string table);
    }
}

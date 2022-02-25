using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Sharding
{
    public class ShardingDbContext : IDbContextInternal, IDbContext
    {
        Dictionary<Type, List<LambdaExpression>> _queryFilters = new Dictionary<Type, List<LambdaExpression>>();

        public IDbSession Session => throw new NotImplementedException();

        Dictionary<Type, List<LambdaExpression>> IDbContextInternal.QueryFilters => this._queryFilters;

        public ITransientTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public ITransientTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public int Delete<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Delete<TEntity>(TEntity entity, string table)
        {
            throw new NotImplementedException();
        }

        public int Delete<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public int Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync<TEntity>(TEntity entity, string table)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            throw new NotImplementedException();
        }

        public int DeleteByKey<TEntity>(object key)
        {
            throw new NotImplementedException();
        }

        public int DeleteByKey<TEntity>(object key, string table)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteByKeyAsync<TEntity>(object key)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteByKeyAsync<TEntity>(object key, string table)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void HasQueryFilter<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            Type entityType = typeof(TEntity);
            this.HasQueryFilter(entityType, filter);
        }
        public void HasQueryFilter(Type entityType, LambdaExpression filter)
        {
            PublicHelper.CheckNull(filter, nameof(filter));
            List<LambdaExpression> filters;
            if (!this._queryFilters.TryGetValue(entityType, out filters))
            {
                filters = new List<LambdaExpression>(1);
                this._queryFilters.Add(entityType, filters);
            }

            filters.Add(filter);
        }

        public TEntity Insert<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public TEntity Insert<TEntity>(TEntity entity, string table)
        {
            throw new NotImplementedException();
        }

        public object Insert<TEntity>(Expression<Func<TEntity>> content)
        {
            throw new NotImplementedException();
        }

        public object Insert<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> InsertAsync<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> InsertAsync<TEntity>(TEntity entity, string table)
        {
            throw new NotImplementedException();
        }

        public Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content)
        {
            throw new NotImplementedException();
        }

        public Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            throw new NotImplementedException();
        }

        public void InsertRange<TEntity>(List<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public void InsertRange<TEntity>(List<TEntity> entities, string table)
        {
            throw new NotImplementedException();
        }

        public Task InsertRangeAsync<TEntity>(List<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public Task InsertRangeAsync<TEntity>(List<TEntity> entities, string table)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T1, T2> JoinQuery<T1, T2>(Expression<Func<T1, T2, object[]>> joinInfo)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T1, T2, T3> JoinQuery<T1, T2, T3>(Expression<Func<T1, T2, T3, object[]>> joinInfo)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T1, T2, T3, T4> JoinQuery<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object[]>> joinInfo)
        {
            throw new NotImplementedException();
        }

        public IJoinQuery<T1, T2, T3, T4, T5> JoinQuery<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object[]>> joinInfo)
        {
            throw new NotImplementedException();
        }

        public IQuery<TEntity> Query<TEntity>()
        {
            return this.Query<TEntity>(null, LockType.Unspecified);
        }

        public IQuery<TEntity> Query<TEntity>(string table)
        {
            return this.Query<TEntity>(table, LockType.Unspecified);
        }

        public IQuery<TEntity> Query<TEntity>(LockType @lock)
        {
            return this.Query<TEntity>(null, @lock);
        }

        public IQuery<TEntity> Query<TEntity>(string table, LockType @lock)
        {
            return new ShardingQuery<TEntity>(this, table, @lock);
        }

        public TEntity QueryByKey<TEntity>(object key)
        {
            throw new NotImplementedException();
        }

        public TEntity QueryByKey<TEntity>(object key, bool tracking)
        {
            throw new NotImplementedException();
        }

        public TEntity QueryByKey<TEntity>(object key, string table)
        {
            throw new NotImplementedException();
        }

        public TEntity QueryByKey<TEntity>(object key, string table, bool tracking)
        {
            throw new NotImplementedException();
        }

        public TEntity QueryByKey<TEntity>(object key, string table, LockType @lock, bool tracking)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> QueryByKeyAsync<TEntity>(object key)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> QueryByKeyAsync<TEntity>(object key, bool tracking)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table, bool tracking)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table, LockType @lock, bool tracking)
        {
            throw new NotImplementedException();
        }

        public TEntity Save<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> SaveAsync<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public List<T> SqlQuery<T>(string sql, params DbParam[] parameters)
        {
            throw new NotImplementedException();
        }

        public List<T> SqlQuery<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotImplementedException();
        }

        public List<T> SqlQuery<T>(string sql, object parameter)
        {
            throw new NotImplementedException();
        }

        public List<T> SqlQuery<T>(string sql, CommandType cmdType, object parameter)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> SqlQueryAsync<T>(string sql, params DbParam[] parameters)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> SqlQueryAsync<T>(string sql, object parameter)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, object parameter)
        {
            throw new NotImplementedException();
        }

        public void TrackEntity(object entity)
        {
            throw new NotImplementedException();
        }

        public int Update<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Update<TEntity>(TEntity entity, string table)
        {
            throw new NotImplementedException();
        }

        public int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content)
        {
            throw new NotImplementedException();
        }

        public int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync<TEntity>(TEntity entity, string table)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            throw new NotImplementedException();
        }

        public void UseTransaction(Action action)
        {
            throw new NotImplementedException();
        }

        public void UseTransaction(Action action, IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public Task UseTransaction(Func<Task> func)
        {
            throw new NotImplementedException();
        }

        public Task UseTransaction(Func<Task> func, IsolationLevel il)
        {
            throw new NotImplementedException();
        }
    }
}

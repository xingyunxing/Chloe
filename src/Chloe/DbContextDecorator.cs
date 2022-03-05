using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Chloe
{
    internal class DbContextDecorator : IDbContext
    {
        bool _disposed = false;

        public DbContextDecorator(IDbContext dbContext)
        {
            this.HoldDbContext = dbContext;
        }

        public virtual IDbContext HoldDbContext { get; private set; }

        public virtual IDbSession Session => this.HoldDbContext.Session;

        public void Dispose()
        {
            if (this._disposed)
                return;

            this.Dispose(true);
            this._disposed = true;
        }
        protected virtual void Dispose(bool disposing)
        {
            this.HoldDbContext.Dispose();
        }

        public virtual ITransientTransaction BeginTransaction()
        {
            return this.HoldDbContext.BeginTransaction();
        }

        public virtual ITransientTransaction BeginTransaction(IsolationLevel il)
        {
            return this.HoldDbContext.BeginTransaction(il);
        }

        public virtual int Delete<TEntity>(TEntity entity)
        {
            return this.HoldDbContext.Delete(entity);
        }

        public virtual int Delete<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContext.Delete(entity, table);
        }

        public virtual int Delete<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            return this.HoldDbContext.Delete(condition);
        }

        public virtual int Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.HoldDbContext.Delete(condition, table);
        }

        public virtual Task<int> DeleteAsync<TEntity>(TEntity entity)
        {
            return this.HoldDbContext.DeleteAsync(entity);
        }

        public virtual Task<int> DeleteAsync<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContext.DeleteAsync(entity, table);
        }

        public virtual Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            return this.HoldDbContext.DeleteAsync(condition);
        }

        public virtual Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.HoldDbContext.DeleteAsync(condition, table);
        }

        public virtual int DeleteByKey<TEntity>(object key)
        {
            return this.HoldDbContext.DeleteByKey<TEntity>(key);
        }

        public virtual int DeleteByKey<TEntity>(object key, string table)
        {
            return this.HoldDbContext.DeleteByKey<TEntity>(key, table);
        }

        public virtual Task<int> DeleteByKeyAsync<TEntity>(object key)
        {
            return this.HoldDbContext.DeleteByKeyAsync<TEntity>(key);
        }

        public virtual Task<int> DeleteByKeyAsync<TEntity>(object key, string table)
        {
            return this.HoldDbContext.DeleteByKeyAsync<TEntity>(key, table);
        }

        public virtual void HasQueryFilter<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            this.HoldDbContext.HasQueryFilter<TEntity>(filter);
        }

        public virtual void HasQueryFilter(Type entityType, LambdaExpression filter)
        {
            this.HoldDbContext.HasQueryFilter(entityType, filter);
        }

        public virtual TEntity Insert<TEntity>(TEntity entity)
        {
            return this.HoldDbContext.Insert<TEntity>(entity);
        }

        public virtual TEntity Insert<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContext.Insert<TEntity>(entity, table);
        }

        public virtual object Insert<TEntity>(Expression<Func<TEntity>> content)
        {
            return this.HoldDbContext.Insert<TEntity>(content);
        }

        public virtual object Insert<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.HoldDbContext.Insert<TEntity>(content, table);
        }

        public virtual Task<TEntity> InsertAsync<TEntity>(TEntity entity)
        {
            return this.HoldDbContext.InsertAsync<TEntity>(entity);
        }

        public virtual Task<TEntity> InsertAsync<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContext.InsertAsync<TEntity>(entity, table);
        }

        public virtual Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content)
        {
            return this.HoldDbContext.InsertAsync<TEntity>(content);
        }

        public virtual Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.HoldDbContext.InsertAsync<TEntity>(content, table);
        }

        public virtual void InsertRange<TEntity>(List<TEntity> entities)
        {
            this.HoldDbContext.InsertRange<TEntity>(entities);
        }

        public virtual void InsertRange<TEntity>(List<TEntity> entities, string table)
        {
            this.HoldDbContext.InsertRange<TEntity>(entities, table);
        }

        public virtual Task InsertRangeAsync<TEntity>(List<TEntity> entities)
        {
            return this.HoldDbContext.InsertRangeAsync<TEntity>(entities);
        }

        public virtual Task InsertRangeAsync<TEntity>(List<TEntity> entities, string table)
        {
            return this.HoldDbContext.InsertRangeAsync<TEntity>(entities, table);
        }

        public virtual IJoinQuery<T1, T2> JoinQuery<T1, T2>(Expression<Func<T1, T2, object[]>> joinInfo)
        {
            return this.HoldDbContext.JoinQuery<T1, T2>(joinInfo);
        }

        public virtual IJoinQuery<T1, T2, T3> JoinQuery<T1, T2, T3>(Expression<Func<T1, T2, T3, object[]>> joinInfo)
        {
            return this.HoldDbContext.JoinQuery<T1, T2, T3>(joinInfo);
        }

        public virtual IJoinQuery<T1, T2, T3, T4> JoinQuery<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object[]>> joinInfo)
        {
            return this.HoldDbContext.JoinQuery<T1, T2, T3, T4>(joinInfo);
        }

        public virtual IJoinQuery<T1, T2, T3, T4, T5> JoinQuery<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object[]>> joinInfo)
        {
            return this.HoldDbContext.JoinQuery<T1, T2, T3, T4, T5>(joinInfo);
        }

        public virtual IQuery<TEntity> Query<TEntity>()
        {
            return this.HoldDbContext.Query<TEntity>();
        }

        public virtual IQuery<TEntity> Query<TEntity>(string table)
        {
            return this.HoldDbContext.Query<TEntity>(table);
        }

        public virtual IQuery<TEntity> Query<TEntity>(LockType @lock)
        {
            return this.HoldDbContext.Query<TEntity>(@lock);
        }

        public virtual IQuery<TEntity> Query<TEntity>(string table, LockType @lock)
        {
            return this.HoldDbContext.Query<TEntity>(table, @lock);
        }

        public virtual TEntity QueryByKey<TEntity>(object key)
        {
            return this.HoldDbContext.QueryByKey<TEntity>(key);
        }

        public virtual TEntity QueryByKey<TEntity>(object key, bool tracking)
        {
            return this.HoldDbContext.QueryByKey<TEntity>(key, tracking);
        }

        public virtual TEntity QueryByKey<TEntity>(object key, string table)
        {
            return this.HoldDbContext.QueryByKey<TEntity>(key, table);
        }

        public virtual TEntity QueryByKey<TEntity>(object key, string table, bool tracking)
        {
            return this.HoldDbContext.QueryByKey<TEntity>(key, table, tracking);
        }

        public virtual TEntity QueryByKey<TEntity>(object key, string table, LockType @lock, bool tracking)
        {
            return this.HoldDbContext.QueryByKey<TEntity>(key, table, @lock, tracking);
        }

        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key)
        {
            return this.HoldDbContext.QueryByKeyAsync<TEntity>(key);
        }

        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key, bool tracking)
        {
            return this.HoldDbContext.QueryByKeyAsync<TEntity>(key, tracking);
        }

        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table)
        {
            return this.HoldDbContext.QueryByKeyAsync<TEntity>(key, table);
        }

        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table, bool tracking)
        {
            return this.HoldDbContext.QueryByKeyAsync<TEntity>(key, table, tracking);
        }

        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table, LockType @lock, bool tracking)
        {
            return this.HoldDbContext.QueryByKeyAsync<TEntity>(key, table, @lock, tracking);
        }

        public virtual TEntity Save<TEntity>(TEntity entity)
        {
            return this.HoldDbContext.Save<TEntity>(entity);
        }

        public virtual Task<TEntity> SaveAsync<TEntity>(TEntity entity)
        {
            return this.HoldDbContext.SaveAsync<TEntity>(entity);
        }

        public virtual List<T> SqlQuery<T>(string sql, params DbParam[] parameters)
        {
            return this.HoldDbContext.SqlQuery<T>(sql, parameters);
        }

        public virtual List<T> SqlQuery<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            return this.HoldDbContext.SqlQuery<T>(sql, cmdType, parameters);
        }

        public virtual List<T> SqlQuery<T>(string sql, object parameter)
        {
            return this.HoldDbContext.SqlQuery<T>(sql, parameter);
        }

        public virtual List<T> SqlQuery<T>(string sql, CommandType cmdType, object parameter)
        {
            return this.HoldDbContext.SqlQuery<T>(sql, cmdType, parameter);
        }

        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, params DbParam[] parameters)
        {
            return this.HoldDbContext.SqlQueryAsync<T>(sql, parameters);
        }

        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            return this.HoldDbContext.SqlQueryAsync<T>(sql, cmdType, parameters);
        }

        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, object parameter)
        {
            return this.HoldDbContext.SqlQueryAsync<T>(sql, parameter);
        }

        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, object parameter)
        {
            return this.HoldDbContext.SqlQueryAsync<T>(sql, cmdType, parameter);
        }

        public virtual void TrackEntity(object entity)
        {
            this.HoldDbContext.TrackEntity(entity);
        }

        public virtual int Update<TEntity>(TEntity entity)
        {
            return this.HoldDbContext.Update<TEntity>(entity);
        }

        public virtual int Update<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContext.Update<TEntity>(entity, table);
        }

        public virtual int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content)
        {
            return this.HoldDbContext.Update<TEntity>(condition, content);
        }

        public virtual int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.HoldDbContext.Update<TEntity>(condition, content, table);
        }

        public virtual Task<int> UpdateAsync<TEntity>(TEntity entity)
        {
            return this.HoldDbContext.UpdateAsync<TEntity>(entity);
        }

        public virtual Task<int> UpdateAsync<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContext.UpdateAsync<TEntity>(entity, table);
        }

        public virtual Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content)
        {
            return this.HoldDbContext.UpdateAsync<TEntity>(condition, content);
        }

        public virtual Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.HoldDbContext.UpdateAsync<TEntity>(condition, content, table);
        }

        public virtual void UseTransaction(Action action)
        {
            this.HoldDbContext.UseTransaction(action);
        }

        public virtual void UseTransaction(Action action, IsolationLevel il)
        {
            this.HoldDbContext.UseTransaction(action, il);
        }

        public virtual Task UseTransaction(Func<Task> func)
        {
            return this.HoldDbContext.UseTransaction(func);
        }

        public virtual Task UseTransaction(Func<Task> func, IsolationLevel il)
        {
            return this.HoldDbContext.UseTransaction(func, il);
        }
    }
}

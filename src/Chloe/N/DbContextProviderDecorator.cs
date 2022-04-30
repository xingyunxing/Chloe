using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Chloe
{
    internal class DbContextProviderDecorator : IDbContextProvider
    {
        bool _disposed = false;

        public DbContextProviderDecorator(IDbContextProvider dbContextProvider)
        {
            this.HoldDbContextProvider = dbContextProvider;
        }

        public virtual IDbContextProvider HoldDbContextProvider { get; private set; }

        public virtual IDbSessionProvider Session => this.HoldDbContextProvider.Session;

        public void Dispose()
        {
            if (this._disposed)
                return;

            this.Dispose(true);
            this._disposed = true;
        }
        protected virtual void Dispose(bool disposing)
        {
            this.HoldDbContextProvider.Dispose();
        }

        public virtual void TrackEntity(object entity)
        {
            this.HoldDbContextProvider.TrackEntity(entity);
        }

        public virtual IQuery<TEntity> Query<TEntity>(string table, LockType @lock)
        {
            return this.HoldDbContextProvider.Query<TEntity>(table, @lock);
        }

        public virtual List<T> SqlQuery<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            return this.HoldDbContextProvider.SqlQuery<T>(sql, cmdType, parameters);
        }

        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            return this.HoldDbContextProvider.SqlQueryAsync<T>(sql, cmdType, parameters);
        }

        public virtual List<T> SqlQuery<T>(string sql, CommandType cmdType, object parameter)
        {
            return this.HoldDbContextProvider.SqlQuery<T>(sql, cmdType, parameter);
        }

        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, object parameter)
        {
            return this.HoldDbContextProvider.SqlQueryAsync<T>(sql, cmdType, parameter);
        }

        public virtual TEntity Save<TEntity>(TEntity entity)
        {
            return this.HoldDbContextProvider.Save<TEntity>(entity);
        }

        public virtual Task<TEntity> SaveAsync<TEntity>(TEntity entity)
        {
            return this.HoldDbContextProvider.SaveAsync<TEntity>(entity);
        }

        public virtual TEntity Insert<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContextProvider.Insert<TEntity>(entity, table);
        }

        public virtual object Insert<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.HoldDbContextProvider.Insert<TEntity>(content, table);
        }

        public virtual Task<TEntity> InsertAsync<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContextProvider.InsertAsync<TEntity>(entity, table);
        }

        public virtual Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.HoldDbContextProvider.InsertAsync<TEntity>(content, table);
        }

        public virtual void InsertRange<TEntity>(List<TEntity> entities, string table)
        {
            this.HoldDbContextProvider.InsertRange<TEntity>(entities, table);
        }

        public virtual Task InsertRangeAsync<TEntity>(List<TEntity> entities, string table)
        {
            return this.HoldDbContextProvider.InsertRangeAsync<TEntity>(entities, table);
        }

        public virtual int Update<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContextProvider.Update<TEntity>(entity, table);
        }
        public virtual int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.HoldDbContextProvider.Update<TEntity>(condition, content, table);
        }

        public virtual Task<int> UpdateAsync<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContextProvider.UpdateAsync<TEntity>(entity, table);
        }
        public virtual Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.HoldDbContextProvider.UpdateAsync<TEntity>(condition, content, table);
        }

        public virtual int Delete<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContextProvider.Delete<TEntity>(entity, table);
        }
        public virtual int Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.HoldDbContextProvider.Delete<TEntity>(condition, table);
        }

        public virtual Task<int> DeleteAsync<TEntity>(TEntity entity, string table)
        {
            return this.HoldDbContextProvider.DeleteAsync<TEntity>(entity, table);
        }

        public virtual Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.HoldDbContextProvider.DeleteAsync<TEntity>(condition, table);
        }
    }
}

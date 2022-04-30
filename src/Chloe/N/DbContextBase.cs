using Chloe.Descriptors;
using Chloe.Threading.Tasks;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Chloe
{
    public abstract class DbContextBase : IDbContextFacade
    {
        bool _disposed = false;

        protected DbContextBase()
        {

        }

        public abstract IDbSessionFacade Session { get; }
        protected Dictionary<Type, List<LambdaExpression>> QueryFilters { get; } = new Dictionary<Type, List<LambdaExpression>>();

        public virtual void Dispose()
        {
            if (this._disposed)
                return;

            this.Dispose(true);
            this._disposed = true;
        }
        protected virtual void Dispose(bool disposing)
        {

        }

        public virtual void TrackEntity(object entity)
        {
            throw new NotImplementedException();
        }

        public virtual void HasQueryFilter<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            Type entityType = typeof(TEntity);
            this.HasQueryFilter(entityType, filter);
        }
        public virtual void HasQueryFilter(Type entityType, LambdaExpression filter)
        {
            PublicHelper.CheckNull(filter, nameof(filter));
            List<LambdaExpression> filters;
            if (!this.QueryFilters.TryGetValue(entityType, out filters))
            {
                filters = new List<LambdaExpression>(1);
                this.QueryFilters.Add(entityType, filters);
            }

            filters.Add(filter);
        }

        public virtual IQuery<TEntity> Query<TEntity>()
        {
            return this.Query<TEntity>(null);
        }
        public virtual IQuery<TEntity> Query<TEntity>(string table)
        {
            return this.Query<TEntity>(table, LockType.Unspecified);
        }
        public virtual IQuery<TEntity> Query<TEntity>(LockType @lock)
        {
            return this.Query<TEntity>(null, @lock);
        }
        public virtual IQuery<TEntity> Query<TEntity>(string table, LockType @lock)
        {
            throw new NotImplementedException();
        }

        public virtual TEntity QueryByKey<TEntity>(object key)
        {
            return this.QueryByKey<TEntity>(key, false);
        }
        public virtual TEntity QueryByKey<TEntity>(object key, bool tracking)
        {
            return this.QueryByKey<TEntity>(key, null, tracking);
        }
        public virtual TEntity QueryByKey<TEntity>(object key, string table)
        {
            return this.QueryByKey<TEntity>(key, table, false);
        }
        public virtual TEntity QueryByKey<TEntity>(object key, string table, bool tracking)
        {
            return this.QueryByKey<TEntity>(key, table, LockType.Unspecified, tracking);
        }
        public virtual TEntity QueryByKey<TEntity>(object key, string table, LockType @lock, bool tracking)
        {
            return this.QueryByKey<TEntity>(key, table, @lock, tracking, false).GetResult();
        }

        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key)
        {
            return this.QueryByKeyAsync<TEntity>(key, false);
        }
        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key, bool tracking)
        {
            return this.QueryByKeyAsync<TEntity>(key, null, tracking);
        }
        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table)
        {
            return this.QueryByKeyAsync<TEntity>(key, table, false);
        }
        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table, bool tracking)
        {
            return this.QueryByKeyAsync<TEntity>(key, table, LockType.Unspecified, tracking);
        }
        public virtual Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table, LockType @lock, bool tracking)
        {
            return this.QueryByKey<TEntity>(key, table, @lock, tracking, true);
        }
        protected virtual Task<TEntity> QueryByKey<TEntity>(object key, string table, LockType @lock, bool tracking, bool @async)
        {
            return DbContextHelper.QueryByKey<TEntity>(this, key, table, @lock, tracking, @async);
        }

        public virtual IJoinQuery<T1, T2> JoinQuery<T1, T2>(Expression<Func<T1, T2, object[]>> joinInfo)
        {
            KeyValuePairList<JoinType, Expression> joinInfos = Helpers.ResolveJoinInfo(joinInfo);
            var ret = this.Query<T1>()
                .Join<T2>(joinInfos[0].Key, (Expression<Func<T1, T2, bool>>)joinInfos[0].Value);

            return ret;
        }
        public virtual IJoinQuery<T1, T2, T3> JoinQuery<T1, T2, T3>(Expression<Func<T1, T2, T3, object[]>> joinInfo)
        {
            KeyValuePairList<JoinType, Expression> joinInfos = Helpers.ResolveJoinInfo(joinInfo);
            var ret = this.Query<T1>()
                .Join<T2>(joinInfos[0].Key, (Expression<Func<T1, T2, bool>>)joinInfos[0].Value)
                .Join<T3>(joinInfos[1].Key, (Expression<Func<T1, T2, T3, bool>>)joinInfos[1].Value);

            return ret;
        }
        public virtual IJoinQuery<T1, T2, T3, T4> JoinQuery<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object[]>> joinInfo)
        {
            KeyValuePairList<JoinType, Expression> joinInfos = Helpers.ResolveJoinInfo(joinInfo);
            var ret = this.Query<T1>()
                .Join<T2>(joinInfos[0].Key, (Expression<Func<T1, T2, bool>>)joinInfos[0].Value)
                .Join<T3>(joinInfos[1].Key, (Expression<Func<T1, T2, T3, bool>>)joinInfos[1].Value)
                .Join<T4>(joinInfos[2].Key, (Expression<Func<T1, T2, T3, T4, bool>>)joinInfos[2].Value);

            return ret;
        }
        public virtual IJoinQuery<T1, T2, T3, T4, T5> JoinQuery<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object[]>> joinInfo)
        {
            KeyValuePairList<JoinType, Expression> joinInfos = Helpers.ResolveJoinInfo(joinInfo);
            var ret = this.Query<T1>()
                .Join<T2>(joinInfos[0].Key, (Expression<Func<T1, T2, bool>>)joinInfos[0].Value)
                .Join<T3>(joinInfos[1].Key, (Expression<Func<T1, T2, T3, bool>>)joinInfos[1].Value)
                .Join<T4>(joinInfos[2].Key, (Expression<Func<T1, T2, T3, T4, bool>>)joinInfos[2].Value)
                .Join<T5>(joinInfos[3].Key, (Expression<Func<T1, T2, T3, T4, T5, bool>>)joinInfos[3].Value);

            return ret;
        }

        public virtual List<T> SqlQuery<T>(string sql, params DbParam[] parameters)
        {
            return this.SqlQuery<T>(sql, CommandType.Text, parameters);
        }
        public virtual List<T> SqlQuery<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotImplementedException();
        }
        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, params DbParam[] parameters)
        {
            return this.SqlQueryAsync<T>(sql, CommandType.Text, parameters);
        }
        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotImplementedException();
        }

        public virtual List<T> SqlQuery<T>(string sql, object parameter)
        {
            /*
             * Usage:
             * dbContext.SqlQuery<User>("select * from Users where Id=@Id", new { Id = 1 });
             */

            return this.SqlQuery<T>(sql, CommandType.Text, parameter);
        }
        public virtual List<T> SqlQuery<T>(string sql, CommandType cmdType, object parameter)
        {
            /*
             * Usage:
             * dbContext.SqlQuery<User>("select * from Users where Id=@Id", CommandType.Text, new { Id = 1 });
             */
            throw new NotImplementedException();
            //return this.SqlQuery<T>(sql, cmdType, PublicHelper.BuildParams(this, parameter));
        }
        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, object parameter)
        {
            return this.SqlQueryAsync<T>(sql, CommandType.Text, parameter);
        }
        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, object parameter)
        {
            throw new NotImplementedException();
            //return this.SqlQueryAsync<T>(sql, cmdType, PublicHelper.BuildParams(this, parameter));
        }

        public virtual TEntity Save<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }
        public virtual Task<TEntity> SaveAsync<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual TEntity Insert<TEntity>(TEntity entity)
        {
            return this.Insert(entity, null);
        }
        public virtual TEntity Insert<TEntity>(TEntity entity, string table)
        {
            return this.Insert(entity, table, false).GetResult();
        }
        public virtual Task<TEntity> InsertAsync<TEntity>(TEntity entity)
        {
            return this.InsertAsync(entity, null);
        }
        public virtual Task<TEntity> InsertAsync<TEntity>(TEntity entity, string table)
        {
            return this.Insert(entity, table, true);
        }
        protected virtual Task<TEntity> Insert<TEntity>(TEntity entity, string table, bool @async)
        {
            throw new NotImplementedException();
        }

        public virtual object Insert<TEntity>(Expression<Func<TEntity>> content)
        {
            return this.Insert(content, null);
        }
        public virtual object Insert<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.Insert(content, table, false).GetResult();
        }
        public virtual Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content)
        {
            return this.InsertAsync(content, null);
        }
        public virtual Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.Insert(content, table, true);
        }
        protected virtual Task<object> Insert<TEntity>(Expression<Func<TEntity>> content, string table, bool @async)
        {
            throw new NotImplementedException();
        }

        public virtual void InsertRange<TEntity>(List<TEntity> entities)
        {
            this.InsertRange(entities, null);
        }
        public virtual void InsertRange<TEntity>(List<TEntity> entities, string table)
        {
            this.InsertRange(entities, table, false).GetResult();
        }
        public virtual Task InsertRangeAsync<TEntity>(List<TEntity> entities)
        {
            return this.InsertRangeAsync(entities, null);
        }
        public virtual Task InsertRangeAsync<TEntity>(List<TEntity> entities, string table)
        {
            return this.InsertRange(entities, table, true);
        }
        protected virtual Task InsertRange<TEntity>(List<TEntity> entities, string table, bool @async)
        {
            throw new NotImplementedException();
        }

        public virtual int Update<TEntity>(TEntity entity)
        {
            return this.Update(entity, null);
        }
        public virtual int Update<TEntity>(TEntity entity, string table)
        {
            return this.Update<TEntity>(entity, table, false).GetResult();
        }
        public virtual Task<int> UpdateAsync<TEntity>(TEntity entity)
        {
            return this.UpdateAsync(entity, null);
        }
        public virtual Task<int> UpdateAsync<TEntity>(TEntity entity, string table)
        {
            return this.Update<TEntity>(entity, table, true);
        }
        protected virtual async Task<int> Update<TEntity>(TEntity entity, string table, bool @async)
        {
            throw new NotImplementedException();
        }

        public virtual int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content)
        {
            return this.Update(condition, content, null);
        }
        public virtual int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.Update<TEntity>(condition, content, table, false).GetResult();
        }
        public virtual Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content)
        {
            return this.UpdateAsync(condition, content, null);
        }
        public virtual Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.Update<TEntity>(condition, content, table, true);
        }
        protected virtual Task<int> Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table, bool @async)
        {
            throw new NotImplementedException();
        }

        public virtual int Delete<TEntity>(TEntity entity)
        {
            return this.Delete(entity, null);
        }
        public virtual int Delete<TEntity>(TEntity entity, string table)
        {
            return this.Delete<TEntity>(entity, table, false).GetResult();
        }
        public virtual Task<int> DeleteAsync<TEntity>(TEntity entity)
        {
            return this.DeleteAsync(entity, null);
        }
        public virtual Task<int> DeleteAsync<TEntity>(TEntity entity, string table)
        {
            return this.Delete<TEntity>(entity, table, true);
        }
        protected virtual Task<int> Delete<TEntity>(TEntity entity, string table, bool @async)
        {
            throw new NotImplementedException();
        }

        public virtual int Delete<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            return this.Delete(condition, null);
        }
        public virtual int Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.Delete<TEntity>(condition, table, false).GetResult();
        }
        public virtual Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            return this.DeleteAsync(condition, null);
        }
        public virtual Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.Delete<TEntity>(condition, table, true);
        }
        protected virtual Task<int> Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table, bool @async)
        {
            throw new NotImplementedException();
        }

        public virtual int DeleteByKey<TEntity>(object key)
        {
            return this.DeleteByKey<TEntity>(key, null);
        }
        public virtual int DeleteByKey<TEntity>(object key, string table)
        {
            return this.DeleteByKey<TEntity>(key, table, false).GetResult();
        }
        public virtual Task<int> DeleteByKeyAsync<TEntity>(object key)
        {
            return this.DeleteByKeyAsync<TEntity>(key, null);
        }
        public virtual Task<int> DeleteByKeyAsync<TEntity>(object key, string table)
        {
            return this.DeleteByKey<TEntity>(key, table, true);
        }
        protected virtual Task<int> DeleteByKey<TEntity>(object key, string table, bool @async)
        {
            Expression<Func<TEntity, bool>> condition = PrimaryKeyHelper.BuildCondition<TEntity>(key);
            return this.Delete<TEntity>(condition, table, @async);
        }

        public virtual ITransientTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public virtual ITransientTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public virtual void UseTransaction(Action action)
        {
            throw new NotImplementedException();
        }

        public virtual void UseTransaction(Action action, IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public virtual Task UseTransaction(Func<Task> func)
        {
            throw new NotImplementedException();
        }

        public virtual Task UseTransaction(Func<Task> func, IsolationLevel il)
        {
            throw new NotImplementedException();
        }
    }
}

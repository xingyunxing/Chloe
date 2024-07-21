using Chloe.Sharding;
using System.Data;
using System.Linq.Expressions;

namespace Chloe
{
    public class DbContextDecorator : DbContextBase, IDbContext
    {
        public DbContextDecorator(IDbContext dbContext)
        {
            PublicHelper.CheckNull(dbContext, nameof(dbContext));
            this.PersistedDbContext = dbContext;
        }

        public IDbContext PersistedDbContext { get; private set; }
        public override IDbSession Session { get { return this.PersistedDbContext.Session; } }

        protected override void Dispose(bool disposing)
        {
            this.PersistedDbContext.Dispose();
        }

        public override object Clone()
        {
            return this.PersistedDbContext.Clone();
        }

        public override void TrackEntity(object entity)
        {
            this.PersistedDbContext.TrackEntity(entity);
        }

        public override void HasQueryFilter(Type entityType, LambdaExpression filter)
        {
            this.PersistedDbContext.HasQueryFilter(entityType, filter);
        }

        public override void HasShardingConfig(Type entityType, IShardingConfig shardingConfig)
        {
            this.PersistedDbContext.HasShardingConfig(entityType, shardingConfig);
        }

        public override IQuery<TEntity> Query<TEntity>(string table, LockType @lock)
        {
            return this.PersistedDbContext.Query<TEntity>(table, @lock);
        }

        protected override Task<TEntity> QueryByKey<TEntity>(object key, string table, LockType @lock, bool tracking, bool @async)
        {
            if (@async)
            {
                return this.PersistedDbContext.QueryByKeyAsync<TEntity>(key, table, @lock, tracking);
            }

            return Task.FromResult(this.PersistedDbContext.QueryByKey<TEntity>(key, table, @lock, tracking));
        }

        public override IJoinQuery<T1, T2> JoinQuery<T1, T2>(Expression<Func<T1, T2, object[]>> joinInfo)
        {
            return this.PersistedDbContext.JoinQuery<T1, T2>(joinInfo);
        }
        public override IJoinQuery<T1, T2, T3> JoinQuery<T1, T2, T3>(Expression<Func<T1, T2, T3, object[]>> joinInfo)
        {
            return this.PersistedDbContext.JoinQuery<T1, T2, T3>(joinInfo);
        }
        public override IJoinQuery<T1, T2, T3, T4> JoinQuery<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object[]>> joinInfo)
        {
            return this.PersistedDbContext.JoinQuery<T1, T2, T3, T4>(joinInfo);
        }
        public override IJoinQuery<T1, T2, T3, T4, T5> JoinQuery<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object[]>> joinInfo)
        {
            return this.PersistedDbContext.JoinQuery<T1, T2, T3, T4, T5>(joinInfo);
        }

        public override List<T> SqlQuery<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            return this.PersistedDbContext.SqlQuery<T>(sql, cmdType, parameters);
        }
        public override List<T> SqlQuery<T>(string sql, CommandType cmdType, object parameter)
        {
            return this.PersistedDbContext.SqlQuery<T>(sql, cmdType, parameter);
        }
        public override Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            return this.PersistedDbContext.SqlQueryAsync<T>(sql, cmdType, parameters);
        }
        public override Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, object parameter)
        {
            return this.PersistedDbContext.SqlQueryAsync<T>(sql, cmdType, parameter);
        }

        public override TEntity Save<TEntity>(TEntity entity)
        {
            return this.PersistedDbContext.Save(entity);
        }
        public override Task<TEntity> SaveAsync<TEntity>(TEntity entity)
        {
            return this.PersistedDbContext.SaveAsync(entity);
        }

        protected override Task<TEntity> Insert<TEntity>(TEntity entity, string table, bool @async)
        {
            if (@async)
            {
                return this.PersistedDbContext.InsertAsync(entity, table);
            }

            return Task.FromResult(this.PersistedDbContext.Insert(entity, table));
        }
        protected override Task<object> Insert<TEntity>(Expression<Func<TEntity>> content, string table, bool @async)
        {
            if (@async)
            {
                return this.PersistedDbContext.InsertAsync(content, table);
            }

            return Task.FromResult(this.PersistedDbContext.Insert(content, table));
        }
        protected override Task InsertRange<TEntity>(List<TEntity> entities, int? insertCountPerBatch, string table, bool @async)
        {
            if (@async)
            {
                return this.PersistedDbContext.InsertRangeAsync(entities, insertCountPerBatch, table);
            }

            this.PersistedDbContext.InsertRange(entities, insertCountPerBatch, table);
            return Task.CompletedTask;
        }

        protected override Task<int> Update<TEntity>(TEntity entity, string table, bool @async)
        {
            if (@async)
            {
                return this.PersistedDbContext.UpdateAsync(entity, table);
            }

            return Task.FromResult(this.PersistedDbContext.Update(entity, table));
        }
        protected override Task<int> Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table, bool @async)
        {
            if (@async)
            {
                return this.PersistedDbContext.UpdateAsync(condition, content, table);
            }

            return Task.FromResult(this.PersistedDbContext.Update(condition, content, table));
        }

        protected override Task<int> Delete<TEntity>(TEntity entity, string table, bool @async)
        {
            if (@async)
            {
                return this.PersistedDbContext.DeleteAsync(entity, table);
            }

            return Task.FromResult(this.PersistedDbContext.Delete(entity, table));
        }
        protected override Task<int> Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table, bool @async)
        {
            if (@async)
            {
                return this.PersistedDbContext.DeleteAsync(condition, table);
            }

            return Task.FromResult(this.PersistedDbContext.Delete(condition, table));
        }
    }
}

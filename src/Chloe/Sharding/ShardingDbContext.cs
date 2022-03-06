using Chloe.Exceptions;
using Chloe.Threading.Tasks;
using Chloe.Utility;
using System.Data;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    /*
     * TODO:
     * # lambda Insert
     * # 实现 InsertRange
     * # 嵌套 TrackEntity
     * # lambda update 和 lambda delete 时对表排序
     * # 
     */
    public partial class ShardingDbContext : IDbContextInternal, IDbContext
    {
        bool _disposed = false;
        Dictionary<Type, List<LambdaExpression>> _queryFilters = new Dictionary<Type, List<LambdaExpression>>();

        public ShardingDbContext() : this(new ShardingOptions())
        {

        }

        public ShardingDbContext(ShardingOptions options)
        {
            this.Options = options;
            this.DbSessionProvider = new ShardingDbSessionProvider(this);
            this.Session = new ShardingDbSession(this);
        }

        internal ShardingDbSessionProvider DbSessionProvider { get; private set; }

        public ShardingOptions Options { get; set; }

        public IDbSession Session { get; private set; }

        Dictionary<Type, List<LambdaExpression>> IDbContextInternal.QueryFilters => this._queryFilters;

        public void Dispose()
        {
            if (this._disposed)
                return;

            this.Dispose(true);
            this._disposed = true;
        }
        protected virtual void Dispose(bool disposing)
        {
            this.DbSessionProvider.Dispose();
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

        public IQuery<TEntity> Query<TEntity>()
        {
            return this.Query<TEntity>(null);
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
            return this.QueryByKey<TEntity>(key, false);
        }
        public TEntity QueryByKey<TEntity>(object key, bool tracking)
        {
            return this.QueryByKey<TEntity>(key, null, tracking);
        }
        public TEntity QueryByKey<TEntity>(object key, string table)
        {
            return this.QueryByKey<TEntity>(key, table, false);
        }
        public TEntity QueryByKey<TEntity>(object key, string table, bool tracking)
        {
            return this.QueryByKey<TEntity>(key, table, LockType.Unspecified, tracking);
        }
        public TEntity QueryByKey<TEntity>(object key, string table, LockType @lock, bool tracking)
        {
            return this.QueryByKey<TEntity>(key, table, @lock, tracking, false).GetResult();
        }

        public Task<TEntity> QueryByKeyAsync<TEntity>(object key)
        {
            return this.QueryByKeyAsync<TEntity>(key, false);
        }
        public Task<TEntity> QueryByKeyAsync<TEntity>(object key, bool tracking)
        {
            return this.QueryByKeyAsync<TEntity>(key, null, tracking);
        }
        public Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table)
        {
            return this.QueryByKeyAsync<TEntity>(key, table, false);
        }
        public Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table, bool tracking)
        {
            return this.QueryByKeyAsync<TEntity>(key, table, LockType.Unspecified, tracking);
        }
        public Task<TEntity> QueryByKeyAsync<TEntity>(object key, string table, LockType @lock, bool tracking)
        {
            return this.QueryByKey<TEntity>(key, table, @lock, tracking, true);
        }
        Task<TEntity> QueryByKey<TEntity>(object key, string table, LockType @lock, bool tracking, bool @async)
        {
            return DbContextHelper.QueryByKey<TEntity>(this, key, table, @lock, tracking, @async);
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

        public TEntity Insert<TEntity>(TEntity entity)
        {
            return this.Insert(entity, null);
        }
        public TEntity Insert<TEntity>(TEntity entity, string table)
        {
            return this.Insert(entity, table, false).GetResult();
        }
        public Task<TEntity> InsertAsync<TEntity>(TEntity entity)
        {
            return this.InsertAsync(entity, null);
        }
        public Task<TEntity> InsertAsync<TEntity>(TEntity entity, string table)
        {
            return this.Insert(entity, table, true);
        }
        async Task<TEntity> Insert<TEntity>(TEntity entity, string table, bool @async)
        {
            PublicHelper.CheckNull(entity);

            IDbContext dbContext = this.GetPersistedDbContextProvider(entity);
            if (@async)
            {
                return await dbContext.InsertAsync(entity);
            }

            return dbContext.Insert(entity);
        }

        public object Insert<TEntity>(Expression<Func<TEntity>> content)
        {
            return this.Insert(content, null);
        }
        public object Insert<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.Insert(content, table, false).GetResult();
        }
        public Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content)
        {
            return this.InsertAsync(content, null);
        }
        public Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.Insert(content, table, true);
        }
        Task<object> Insert<TEntity>(Expression<Func<TEntity>> content, string table, bool @async)
        {
            throw new NotImplementedException();
        }

        public void InsertRange<TEntity>(List<TEntity> entities)
        {
            this.InsertRange(entities, null);
        }
        public void InsertRange<TEntity>(List<TEntity> entities, string table)
        {
            this.InsertRange(entities, table, false).GetResult();
        }
        public Task InsertRangeAsync<TEntity>(List<TEntity> entities)
        {
            return this.InsertRangeAsync(entities, null);
        }
        public Task InsertRangeAsync<TEntity>(List<TEntity> entities, string table)
        {
            return this.InsertRange(entities, table, true);
        }
        protected virtual Task InsertRange<TEntity>(List<TEntity> entities, string table, bool @async)
        {
            throw new NotImplementedException();
        }

        public TEntity Save<TEntity>(TEntity entity)
        {
            return DbContextHelper.Save<TEntity>(this, entity, false).GetResult();
        }
        public Task<TEntity> SaveAsync<TEntity>(TEntity entity)
        {
            return DbContextHelper.Save<TEntity>(this, entity, true);
        }


        public int Update<TEntity>(TEntity entity)
        {
            return this.Update(entity, null);
        }
        public int Update<TEntity>(TEntity entity, string table)
        {
            return this.Update<TEntity>(entity, table, false).GetResult();
        }
        public Task<int> UpdateAsync<TEntity>(TEntity entity)
        {
            return this.UpdateAsync(entity, null);
        }
        public Task<int> UpdateAsync<TEntity>(TEntity entity, string table)
        {
            return this.Update<TEntity>(entity, table, true);
        }
        async Task<int> Update<TEntity>(TEntity entity, string table, bool @async)
        {
            PublicHelper.CheckNull(entity);

            IDbContext dbContext = this.GetPersistedDbContextProvider(entity);
            if (@async)
            {
                return await dbContext.UpdateAsync(entity);
            }

            return dbContext.Update(entity);
        }

        public int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content)
        {
            return this.Update(condition, content, null);
        }
        public int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.Update<TEntity>(condition, content, table, false).GetResult();
        }
        public Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content)
        {
            return this.UpdateAsync(condition, content, null);
        }
        public Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.Update<TEntity>(condition, content, table, true);
        }
        async Task<int> Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table, bool @async)
        {
            PublicHelper.CheckNull(condition, nameof(condition));
            PublicHelper.CheckNull(content, nameof(content));

            IShardingContext shardingContext = this.CreateShardingContext(typeof(TEntity));

            List<RouteTable> routeTables = ShardingTablePeeker.Peek(condition, shardingContext);
            var groupedTables = ShardingHelpers.GroupTables(routeTables.Select(a => (IPhysicTable)new PhysicTable(a)));

            bool isUniqueDataQuery = UniqueDataQueryAuthenticator.IsUniqueDataQuery(shardingContext, condition);
            int rowsAffectedLimit = isUniqueDataQuery ? 1 : int.MaxValue;

            int rowsAffected = 0;

            if (routeTables.Count > 1 && !this.DbSessionProvider.IsInTransaction)
            {
                //开启事务
                using (var tran = this.BeginTransaction())
                {
                    rowsAffected = await this.ExecuteUpdate(groupedTables, condition, content, rowsAffectedLimit, @async);

                    tran.Commit();
                }
            }
            else
            {
                rowsAffected = await this.ExecuteUpdate(groupedTables, condition, content, rowsAffectedLimit, @async);
            }

            return rowsAffected;
        }

        public int Delete<TEntity>(TEntity entity)
        {
            return this.Delete(entity, null);
        }
        public int Delete<TEntity>(TEntity entity, string table)
        {
            return this.Delete<TEntity>(entity, table, false).GetResult();
        }
        public Task<int> DeleteAsync<TEntity>(TEntity entity)
        {
            return this.DeleteAsync(entity, null);
        }
        public Task<int> DeleteAsync<TEntity>(TEntity entity, string table)
        {
            return this.Delete<TEntity>(entity, table, true);
        }
        async Task<int> Delete<TEntity>(TEntity entity, string table, bool @async)
        {
            PublicHelper.CheckNull(entity);

            IDbContext dbContext = this.GetPersistedDbContextProvider(entity);
            if (@async)
            {
                return await dbContext.DeleteAsync(entity);
            }

            return dbContext.Delete(entity);
        }

        public int Delete<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            return this.Delete(condition, null);
        }
        public int Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.Delete<TEntity>(condition, table, false).GetResult();
        }
        public Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            return this.DeleteAsync(condition, null);
        }
        public Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.Delete<TEntity>(condition, table, true);
        }
        async Task<int> Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table, bool @async)
        {
            PublicHelper.CheckNull(condition, nameof(condition));

            IShardingContext shardingContext = this.CreateShardingContext(typeof(TEntity));
            List<RouteTable> routeTables = ShardingTablePeeker.Peek(condition, shardingContext);
            var groupedTables = ShardingHelpers.GroupTables(routeTables.Select(a => (IPhysicTable)new PhysicTable(a)));

            bool isUniqueDataQuery = UniqueDataQueryAuthenticator.IsUniqueDataQuery(shardingContext, condition);
            int rowsAffectedLimit = isUniqueDataQuery ? 1 : int.MaxValue;

            int rowsAffected = 0;

            if (routeTables.Count > 1 && !this.DbSessionProvider.IsInTransaction)
            {
                //开启事务
                using (var tran = this.BeginTransaction())
                {
                    rowsAffected = await this.ExecuteDelete(groupedTables, condition, rowsAffectedLimit, @async);

                    tran.Commit();
                }
            }
            else
            {
                rowsAffected = await this.ExecuteDelete(groupedTables, condition, rowsAffectedLimit, @async);
            }

            return rowsAffected;
        }


        public int DeleteByKey<TEntity>(object key)
        {
            return this.DeleteByKey<TEntity>(key, null);
        }
        public int DeleteByKey<TEntity>(object key, string table)
        {
            return this.DeleteByKey<TEntity>(key, table, false).GetResult();
        }
        public Task<int> DeleteByKeyAsync<TEntity>(object key)
        {
            return this.DeleteByKeyAsync<TEntity>(key, null);
        }
        public Task<int> DeleteByKeyAsync<TEntity>(object key, string table)
        {
            return this.DeleteByKey<TEntity>(key, table, true);
        }
        Task<int> DeleteByKey<TEntity>(object key, string table, bool @async)
        {
            Expression<Func<TEntity, bool>> condition = PrimaryKeyHelper.BuildCondition<TEntity>(key);
            return this.Delete<TEntity>(condition, null, @async);
        }

        public ITransientTransaction BeginTransaction()
        {
            /*
             * using(ITransientTransaction tran = dbContext.BeginTransaction())
             * {
             *      dbContext.Insert()...
             *      dbContext.Update()...
             *      dbContext.Delete()...
             *      tran.Commit();
             * }
             */
            return new TransientTransaction(this);
        }
        public ITransientTransaction BeginTransaction(IsolationLevel il)
        {
            return new TransientTransaction(this, il);
        }
        public void UseTransaction(Action action)
        {
            /*
             * dbContext.UseTransaction(() =>
             * {
             *     dbContext.Insert()...
             *     dbContext.Update()...
             *     dbContext.Delete()...
             * });
             */

            PublicHelper.CheckNull(action);
            using (ITransientTransaction tran = this.BeginTransaction())
            {
                action();
                tran.Commit();
            }
        }
        public void UseTransaction(Action action, IsolationLevel il)
        {
            PublicHelper.CheckNull(action);
            using (ITransientTransaction tran = this.BeginTransaction(il))
            {
                action();
                tran.Commit();
            }
        }
        public async Task UseTransaction(Func<Task> func)
        {
            /*
             * await dbContext.UseTransaction(async () =>
             * {
             *     await dbContext.InsertAsync()...
             *     await dbContext.UpdateAsync()...
             *     await dbContext.DeleteAsync()...
             * });
             */

            PublicHelper.CheckNull(func);
            using (ITransientTransaction tran = this.BeginTransaction())
            {
                await func();
                tran.Commit();
            }
        }
        public async Task UseTransaction(Func<Task> func, IsolationLevel il)
        {
            PublicHelper.CheckNull(func);
            using (ITransientTransaction tran = this.BeginTransaction(il))
            {
                await func();
                tran.Commit();
            }
        }

        public void TrackEntity(object entity)
        {
            PublicHelper.CheckNull(entity);

            IShardingContext shardingContext = this.CreateShardingContext(entity.GetType());

            var shardingPropertyDescriptor = shardingContext.TypeDescriptor.GetPrimitivePropertyDescriptor(shardingContext.ShardingConfig.ShardingKey);

            var shardingKeyValue = shardingPropertyDescriptor.GetValue(entity);
            RouteTable routeTable = shardingContext.GetTable(shardingKeyValue);

            if (routeTable == null)
            {
                throw new ChloeException($"Corresponding table not found for entity '{entity.GetType().FullName}' with sharding key '{shardingKeyValue}'.");
            }

            IDbContext persistedDbContext = this.GetPersistedDbContextProvider(routeTable);

            //TODO 考虑嵌套
            persistedDbContext.TrackEntity(entity);
        }
    }
}

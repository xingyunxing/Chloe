using Chloe.Core.Visitors;
using Chloe.Extensions;
using Chloe.Sharding.Routing;
using Chloe.Sharding.Visitors;
using Chloe.Threading.Tasks;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    public partial class ShardingDbContextProvider : IDbContextProvider
    {
        bool _disposed = false;

        public ShardingDbContextProvider(DbContext dbContext)
        {
            this.DbContext = dbContext;
            this.Session = new ShardingDbSessionProvider(this);
        }

        public DbContext DbContext { get; private set; }
        public IDbSessionProvider Session { get; private set; }

        public void Dispose()
        {
            if (this._disposed)
                return;

            this.Dispose(true);
            this._disposed = true;
        }
        protected virtual void Dispose(bool disposing)
        {

        }

        public void TrackEntity(object entity)
        {
            throw new NotSupportedException();
        }

        public void HasQueryFilter<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public void HasQueryFilter(Type entityType, LambdaExpression filter)
        {
            throw new NotImplementedException();
        }

        public IQuery<TEntity> Query<TEntity>(string table, LockType @lock)
        {
            return new ShardingQuery<TEntity>(this, table, @lock); ;
        }

        public TEntity Save<TEntity>(TEntity entity)
        {
            return Helpers.Save<TEntity>(this, entity, false).GetResult();
        }

        public Task<TEntity> SaveAsync<TEntity>(TEntity entity)
        {
            return Helpers.Save<TEntity>(this, entity, true);
        }

        public List<T> SqlQuery<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotImplementedException();
        }
        public List<T> SqlQuery<T>(string sql, CommandType cmdType, object parameter)
        {
            throw new NotImplementedException();
        }
        public Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotImplementedException();
        }
        public Task<List<T>> SqlQueryAsync<T>(string sql, CommandType cmdType, object parameter)
        {
            throw new NotImplementedException();
        }


        public TEntity Insert<TEntity>(TEntity entity, string table)
        {
            return this.Insert(entity, false).GetResult();
        }
        public Task<TEntity> InsertAsync<TEntity>(TEntity entity, string table)
        {
            return this.Insert(entity, true);
        }
        Task<TEntity> Insert<TEntity>(TEntity entity, bool @async)
        {
            PublicHelper.CheckNull(entity);

            RouteTable routeTable = this.GetRouteTable(entity);
            IDbContextProvider dbContextProvider = this.GetPersistedDbContextProvider(routeTable);
            if (@async)
            {
                return dbContextProvider.InsertAsync(entity, routeTable.Name);
            }

            return Task.FromResult(dbContextProvider.Insert(entity, routeTable.Name));
        }

        public object Insert<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.Insert(content, false).GetResult();
        }
        public Task<object> InsertAsync<TEntity>(Expression<Func<TEntity>> content, string table)
        {
            return this.Insert(content, true);
        }
        async Task<object> Insert<TEntity>(Expression<Func<TEntity>> content, bool @async)
        {
            PublicHelper.CheckNull(content, nameof(content));

            IShardingContext shardingContext = this.CreateShardingContext(typeof(TEntity));

            Dictionary<MemberInfo, Expression> insertColumns = InitMemberExtractor.Extract(content);

            List<ShardingKey> shardingKeys = this.GetShardingKeys(shardingContext.ShardingConfig, insertColumns);

            RouteTable routeTable = shardingContext.GetTable(shardingKeys);
            IDbContextProvider persistedDbContextProvider = this.GetPersistedDbContextProvider(routeTable);

            if (@async)
            {
                return await persistedDbContextProvider.InsertAsync<TEntity>(content, routeTable.Name);
            }

            return persistedDbContextProvider.Insert<TEntity>(content, routeTable.Name);
        }
        List<ShardingKey> GetShardingKeys(IShardingConfig shardingConfig, Dictionary<MemberInfo, Expression> insertColumns)
        {
            List<ShardingKey> shardingKeys = new List<ShardingKey>(shardingConfig.ShardingKeys.Count);

            for (int i = 0; i < shardingConfig.ShardingKeys.Count; i++)
            {
                MemberInfo shardingKeyMember = shardingConfig.ShardingKeys[i];

                var shardingKeyExp = insertColumns.FindValue(shardingKeyMember);
                if (shardingKeyExp == null)
                {
                    throw new ArgumentException($"Sharding key not found from content.");
                }

                if (shardingKeyExp.IsEvaluable())
                {
                    throw new ArgumentException($"Unable to get sharding key value from expression '{shardingKeyExp.ToString()}'.");
                }

                var shardingKeyValue = shardingKeyExp.Evaluate();

                if (shardingKeyValue == null)
                {
                    throw new ArgumentException($"The sharding key '{shardingKeyExp.ToString()}' value can not be null.");
                }

                ShardingKey shardingKey = new ShardingKey() { Member = shardingKeyMember, Value = shardingKeyValue };
                shardingKeys.Add(shardingKey);
            }

            return shardingKeys;
        }

        public void InsertRange<TEntity>(List<TEntity> entities, string table)
        {
            this.InsertRange(entities, false).GetResult();
        }
        public Task InsertRangeAsync<TEntity>(List<TEntity> entities, string table)
        {
            return this.InsertRange(entities, true);
        }
        protected virtual async Task InsertRange<TEntity>(List<TEntity> entities, bool @async)
        {
            PublicHelper.CheckNull(entities, nameof(entities));

            List<(TEntity Entity, IPhysicTable Table)> entityMap = this.MakeEntityMap(entities);

            if (this.Session.IsInTransaction)
            {
                await this.InsertRange(entityMap, @async);
            }
            else
            {
                using (var tran = this.DbContext.BeginTransaction())
                {
                    await this.InsertRange(entityMap, @async);
                    tran.Commit();
                }
            }
        }
        List<(TEntity Entity, IPhysicTable Table)> MakeEntityMap<TEntity>(List<TEntity> entities)
        {
            IShardingContext shardingContext = this.CreateShardingContext(typeof(TEntity));

            List<(TEntity Entity, IPhysicTable Table)> entityMap = new List<(TEntity Entity, IPhysicTable Table)>(entities.Count);

            foreach (var entity in entities)
            {
                RouteTable routeTable = shardingContext.GetEntityTable(entity);
                entityMap.Add((entity, new PhysicTable(routeTable)));
            }

            return entityMap;
        }
        async Task InsertRange<TEntity>(List<(TEntity Entity, IPhysicTable Table)> entityMap, bool @async)
        {
            //TODO 对库排序，然后在对表排序
            var groupedEntities = entityMap.GroupBy(a => a.Table.DataSource.Name);

            foreach (var group in groupedEntities)
            {
                var dataSource = group.First().Table.DataSource;

                var dbContext = this.GetPersistedDbContextProvider(dataSource);

                var tableGroups = group.GroupBy(a => a.Table.Name);
                foreach (var tableGroup in tableGroups)
                {
                    var tableEntities = tableGroup.Select(a => a.Entity).ToList();

                    if (@async)
                    {
                        await dbContext.InsertRangeAsync(tableEntities, tableGroup.Key);
                    }
                    else
                    {
                        dbContext.InsertRange(tableEntities, tableGroup.Key);
                    }
                }
            }
        }


        public int Update<TEntity>(TEntity entity, string table)
        {
            return this.Update(entity, false).GetResult();
        }
        public Task<int> UpdateAsync<TEntity>(TEntity entity, string table)
        {
            return this.Update(entity, true);
        }
        async Task<int> Update<TEntity>(TEntity entity, bool @async)
        {
            PublicHelper.CheckNull(entity);

            RouteTable routeTable = this.GetRouteTable(entity);
            IDbContextProvider dbContextProvider = this.GetPersistedDbContextProvider(routeTable);
            if (@async)
            {
                return await dbContextProvider.UpdateAsync(entity, routeTable.Name);
            }

            return dbContextProvider.Update(entity, routeTable.Name);
        }

        public int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.Update(condition, content, false).GetResult();
        }
        public Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, string table)
        {
            return this.Update(condition, content, true);
        }
        async Task<int> Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, bool @async)
        {
            PublicHelper.CheckNull(condition, nameof(condition));
            PublicHelper.CheckNull(content, nameof(content));

            IShardingContext shardingContext = this.CreateShardingContext(typeof(TEntity));

            List<RouteTable> routeTables = ShardingTableDiscoverer.GetRouteTables(condition, shardingContext).ToList();
            var groupedTables = ShardingHelpers.GroupTables(routeTables.Select(a => (IPhysicTable)new PhysicTable(a)));

            int rowsAffectedLimit = int.MaxValue;
            if (routeTables.Count > 1)
            {
                bool isUniqueDataQuery = UniqueDataQueryAuthenticator.IsUniqueDataQuery(shardingContext, condition);
                if (isUniqueDataQuery)
                {
                    rowsAffectedLimit = 1;
                }
            }

            int rowsAffected = 0;

            if (routeTables.Count > 1 && !this.DbContext.Butler.IsInTransaction)
            {
                //开启事务
                using (var tran = this.DbContext.BeginTransaction())
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


        public int Delete<TEntity>(TEntity entity, string table)
        {
            return this.Delete(entity, table, false).GetResult();
        }
        public Task<int> DeleteAsync<TEntity>(TEntity entity, string table)
        {
            return this.Delete(entity, table, true);
        }
        async Task<int> Delete<TEntity>(TEntity entity, string table, bool @async)
        {
            PublicHelper.CheckNull(entity);

            RouteTable routeTable = this.GetRouteTable(entity);
            IDbContextProvider dbContextProvider = this.GetPersistedDbContextProvider(routeTable);
            if (@async)
            {
                return await dbContextProvider.DeleteAsync(entity, routeTable.Name);
            }

            return dbContextProvider.Delete(entity, routeTable.Name);
        }

        public int Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.Delete<TEntity>(condition, table, false).GetResult();
        }
        public Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> condition, string table)
        {
            return this.Delete<TEntity>(condition, table, true);
        }
        async Task<int> Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table, bool @async)
        {
            PublicHelper.CheckNull(condition, nameof(condition));

            IShardingContext shardingContext = this.CreateShardingContext(typeof(TEntity));
            List<RouteTable> routeTables = ShardingTableDiscoverer.GetRouteTables(condition, shardingContext).ToList();
            var groupedTables = ShardingHelpers.GroupTables(routeTables.Select(a => (IPhysicTable)new PhysicTable(a)));

            int rowsAffectedLimit = int.MaxValue;
            if (routeTables.Count > 1)
            {
                bool isUniqueDataQuery = UniqueDataQueryAuthenticator.IsUniqueDataQuery(shardingContext, condition);
                if (isUniqueDataQuery)
                {
                    rowsAffectedLimit = 1;
                }
            }

            int rowsAffected = 0;

            if (routeTables.Count > 1 && !this.DbContext.Butler.IsInTransaction)
            {
                //开启事务
                using (var tran = this.DbContext.BeginTransaction())
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
    }
}

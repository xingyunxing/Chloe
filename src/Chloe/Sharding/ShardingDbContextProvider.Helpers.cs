using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.Sharding.Routing;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    public partial class ShardingDbContextProvider
    {
        Dictionary<Type, IShardingContext> _shardingContextMap = new Dictionary<Type, IShardingContext>();

        RouteTable GetRouteTable<TEntity>(TEntity entity, bool throwExceptionIfNotFound = false)
        {
            IShardingContext shardingContext = this.CreateShardingContext(entity.GetType());
            RouteTable routeTable = shardingContext.GetEntityTable(entity, throwExceptionIfNotFound);
            return routeTable;
        }

        internal IShardingContext CreateShardingContext(Type entityType)
        {
            IShardingContext shardingContext = this._shardingContextMap.FindValue(entityType);
            if (shardingContext == null)
            {
                TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entityType);
                IShardingConfig shardingConfig = ShardingConfigContainer.Get(entityType);
                shardingContext = new ShardingContextFacade(this, shardingConfig, typeDescriptor);

                this._shardingContextMap.Add(entityType, shardingContext);
            }

            return shardingContext;
        }

        internal IDbContextProvider GetPersistedDbContextProvider(RouteTable routeTable)
        {
            return this.GetPersistedDbContextProvider(new PhysicDataSource(routeTable.DataSource));
        }
        internal IDbContextProvider GetPersistedDbContextProvider(IPhysicDataSource dataSource)
        {
            return this.DbContext.Butler.GetPersistedDbContextProvider(dataSource);
        }

        async Task<int> ExecuteUpdate<TEntity>(IEnumerable<(IPhysicDataSource DataSource, List<IPhysicTable> Tables)> groups, Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, int rowsAffectedLimit, bool @async)
        {
            int totalRowsAffected = 0;

            foreach (var group in groups)
            {
                var dataSource = group.DataSource;
                var tables = group.Tables;

                var dbContextProvider = this.GetPersistedDbContextProvider(dataSource);

                foreach (var table in tables)
                {
                    int rowsAffected = 0;
                    if (@async)
                    {
                        rowsAffected = await dbContextProvider.UpdateAsync<TEntity>(condition, content, table.Name);
                    }
                    else
                    {
                        rowsAffected = dbContextProvider.Update<TEntity>(condition, content, table.Name);
                    }

                    totalRowsAffected += rowsAffected;

                    if (totalRowsAffected >= rowsAffectedLimit)
                    {
                        goto End;
                    }
                }
            }

        End:
            return totalRowsAffected;
        }
        async Task<int> ExecuteDelete<TEntity>(IEnumerable<(IPhysicDataSource DataSource, List<IPhysicTable> Tables)> groups, Expression<Func<TEntity, bool>> condition, int rowsAffectedLimit, bool @async)
        {
            int totalRowsAffected = 0;

            foreach (var group in groups)
            {
                var dataSource = group.DataSource;
                var tables = group.Tables;

                var dbContextProvider = this.GetPersistedDbContextProvider(dataSource);

                foreach (var table in tables)
                {
                    int rowsAffected = 0;
                    if (@async)
                    {
                        rowsAffected = await dbContextProvider.DeleteAsync<TEntity>(condition, table.Name);
                    }
                    else
                    {
                        rowsAffected = dbContextProvider.Delete<TEntity>(condition, table.Name);
                    }

                    totalRowsAffected += rowsAffected;

                    if (totalRowsAffected >= rowsAffectedLimit)
                    {
                        goto End;
                    }
                }
            }

        End:
            return totalRowsAffected;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Chloe.Descriptors;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using Chloe.Routing;

namespace Chloe.Sharding
{
    public partial class ShardingDbContextProvider
    {
        Dictionary<Type, IShardingContext> _shardingContextMap = new Dictionary<Type, IShardingContext>();

        Chloe.Routing.RouteTable GetRouteTable<TEntity>(TEntity entity, bool throwExceptionIfNotFound = false)
        {
            IShardingContext shardingContext = this.CreateShardingContext(entity.GetType());
            Chloe.Routing.RouteTable routeTable = shardingContext.GetEntityTable(entity, throwExceptionIfNotFound);
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

        internal IDbContextProvider GetPersistedDbContextProvider(object entity)
        {
            PublicHelper.CheckNull(entity);

            IShardingContext shardingContext = this.CreateShardingContext(entity.GetType());

            var shardingPropertyDescriptor = shardingContext.TypeDescriptor.GetPrimitivePropertyDescriptor(shardingContext.ShardingConfig.ShardingKey);

            var shardingKeyValue = shardingPropertyDescriptor.GetValue(entity);
            Chloe.Routing.RouteTable routeTable = shardingContext.GetTable(shardingKeyValue);

            if (routeTable == null)
            {
                throw new ChloeException($"Corresponding table not found for entity '{entity.GetType().FullName}' with sharding key '{shardingKeyValue}'.");
            }

            IDbContextProvider persistedDbContextProvider = this.GetPersistedDbContextProvider(routeTable);
            return persistedDbContextProvider;
        }
        internal IDbContextProvider GetPersistedDbContextProvider(Chloe.Routing.RouteTable routeTable)
        {
            return this.GetPersistedDbContextProvider(new Chloe.Routing.PhysicDataSource(routeTable.DataSource));
        }
        internal IDbContextProvider GetPersistedDbContextProvider(Chloe.Routing.IPhysicDataSource dataSource)
        {
            var dbContextProviderFactory = (dataSource as Chloe.Routing.PhysicDataSource).DataSource.DbContextProviderFactory;

            DataSourceDbContextPair pair = this.DbContext.Butler.PersistedDbContextProviders.FirstOrDefault(a => a.DataSource.Name == dataSource.Name);
            if (pair == null)
            {
                IDbContextProvider dbContextProvider = dbContextProviderFactory.CreateDbContextProvider();
                this.AppendFeatures(dbContextProvider);

                pair = new DataSourceDbContextPair(dataSource, dbContextProvider);

                this.DbContext.Butler.PersistedDbContextProviders.Add(pair);
            }

            if (this.DbContext.Butler.IsInTransaction)
            {
                if (!pair.DbContextProvider.Session.IsInTransaction)
                {
                    pair.DbContextProvider.Session.BeginTransaction(this.DbContext.Butler.IL);
                }
            }

            return new PersistedDbContextProvider(pair.DbContextProvider);
        }

        internal List<IDbContextProvider> CreateDbContextProviders(Chloe.Routing.IPhysicDataSource dataSource, int desiredCount)
        {
            if (this.DbContext.Butler.IsInTransaction)
            {
                return new List<IDbContextProvider>(1) { this.GetPersistedDbContextProvider(dataSource) };
            }

            return this.CreateTransientDbContextProviders(dataSource, desiredCount);
        }
        internal List<IDbContextProvider> CreateTransientDbContextProviders(Chloe.Routing.IPhysicDataSource dataSource, int desiredCount)
        {
            var routeDbContextProviderFactory = (dataSource as Chloe.Routing.PhysicDataSource).DataSource.DbContextProviderFactory;

            int connectionCount = Math.Min(desiredCount, this.Options.MaxConnectionsPerDataSource);

            List<IDbContextProvider> dbContexts = new List<IDbContextProvider>(connectionCount);

            for (int i = 0; i < connectionCount; i++)
            {
                IDbContextProvider dbContextProvider = routeDbContextProviderFactory.CreateDbContextProvider();
                this.AppendFeatures(dbContextProvider);

                dbContexts.Add(dbContextProvider);
            }

            return dbContexts;
        }
        void AppendFeatures(IDbContextProvider dbContextProvider)
        {
            dbContextProvider.Session.CommandTimeout = this.Session.CommandTimeout;
            this.AppendQueryFilters(dbContextProvider);
            this.AppendSessionInterceptors(dbContextProvider);
        }
        void AppendQueryFilters(IDbContextProvider dbContextProvider)
        {
            foreach (var kv in this.DbContext.Butler.QueryFilters)
            {
                foreach (var filter in kv.Value)
                {
                    //TODO
                    throw new NotImplementedException();
                    //dbContextProvider.HasQueryFilter(kv.Key, filter);
                }
            }
        }
        void AppendSessionInterceptors(IDbContextProvider dbContextProvider)
        {
            foreach (var interceptor in this.DbContext.Butler.Interceptors)
            {
                dbContextProvider.Session.AddInterceptor(interceptor);
            }
        }


        async Task<int> ExecuteUpdate<TEntity>(IEnumerable<(Chloe.Routing.IPhysicDataSource DataSource, List<Chloe.Routing.IPhysicTable> Tables)> groups, Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> content, int rowsAffectedLimit, bool @async)
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
        async Task<int> ExecuteDelete<TEntity>(IEnumerable<(Chloe.Routing.IPhysicDataSource DataSource, List<Chloe.Routing.IPhysicTable> Tables)> groups, Expression<Func<TEntity, bool>> condition, int rowsAffectedLimit, bool @async)
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


        class PersistedDbContextProvider : DbContextProviderDecorator, IDbContextProvider
        {
            public PersistedDbContextProvider(IDbContextProvider dbContextProvider) : base(dbContextProvider)
            {
            }

            protected override void Dispose(bool disposing)
            {

            }
        }
    }
}

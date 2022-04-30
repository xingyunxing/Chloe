using Chloe.Exceptions;
using Chloe.Infrastructure.Interception;
using Chloe.Routing;
using Chloe.Sharding;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Chloe
{
    class DbContextButler : IDisposable
    {
        bool _disposed = false;

        int _commandTimeout = 30;
        IDbContextProvider _defaultDbContextProvider;
        IDbContextProvider _shardingDbContextProvider;

        public DbContextButler(DbContextFacade dbContext)
        {
            this.DbContext = dbContext;
        }

        public DbContextFacade DbContext { get; private set; }

        public List<DataSourceDbContextPair> PersistedDbContextProviders { get; set; } = new List<DataSourceDbContextPair>();

        public List<IDbCommandInterceptor> Interceptors { get; } = new List<IDbCommandInterceptor>();

        public Dictionary<Type, List<LambdaExpression>> QueryFilters { get; } = new Dictionary<Type, List<LambdaExpression>>();

        public IsolationLevel? IL { get; private set; }
        public bool IsInTransaction { get; private set; }
        public int CommandTimeout
        {
            get
            {
                return this._commandTimeout;
            }
            set
            {
                this.SetCommandTimeout(value);
            }
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            if (this.IsInTransaction)
            {
                try
                {
                    this.RollbackTransactionImpl();
                }
                catch
                {
                }
            }

            foreach (var pair in PersistedDbContextProviders)
            {
                pair.DbContextProvider.Dispose();
            }

            this.PersistedDbContextProviders.Clear();

            this._disposed = true;
        }

        void SetCommandTimeout(int commandTimeout)
        {
            this._commandTimeout = commandTimeout;

            foreach (var pair in this.PersistedDbContextProviders)
            {
                pair.DbContextProvider.Session.CommandTimeout = commandTimeout;
            }
        }

        public void AddInterceptor(IDbCommandInterceptor interceptor)
        {
            PublicHelper.CheckNull(interceptor, nameof(interceptor));
            this.Interceptors.Add(interceptor);
            foreach (var pair in this.PersistedDbContextProviders)
            {
                pair.DbContextProvider.Session.AddInterceptor(interceptor);
            }
        }
        public void RemoveInterceptor(IDbCommandInterceptor interceptor)
        {
            PublicHelper.CheckNull(interceptor, nameof(interceptor));
            this.Interceptors.Remove(interceptor);
            foreach (var pair in this.PersistedDbContextProviders)
            {
                pair.DbContextProvider.Session.RemoveInterceptor(interceptor);
            }
        }

        public void BeginTransaction(IsolationLevel? il)
        {
            if (this.IsInTransaction)
            {
                throw new ChloeException("The current session has opened a transaction.");
            }

            try
            {
                foreach (var pair in this.PersistedDbContextProviders)
                {
                    pair.DbContextProvider.Session.BeginTransaction(il);
                }
            }
            catch
            {
                try
                {
                    this.RollbackTransactionImpl();
                }
                catch
                {
                }

                throw;
            }

            this.IL = il;
            this.IsInTransaction = true;
        }

        public void CommitTransaction()
        {
            if (!this.IsInTransaction)
            {
                throw new ChloeException("Current session does not open a transaction.");
            }

            foreach (var pair in this.PersistedDbContextProviders)
            {
                var dbContextProvider = pair.DbContextProvider;

                if (!dbContextProvider.Session.IsInTransaction)
                {
                    continue;
                }

                dbContextProvider.Session.CommitTransaction();
            }

            this.IsInTransaction = false;
        }
        public void RollbackTransaction()
        {
            if (!this.IsInTransaction)
            {
                throw new ChloeException("Current session does not open a transaction.");
            }

            this.RollbackTransactionImpl();
        }
        void RollbackTransactionImpl()
        {
            List<Exception> exceptions = null;

            foreach (var pair in this.PersistedDbContextProviders)
            {
                var dbContextProvider = pair.DbContextProvider;
                if (!dbContextProvider.Session.IsInTransaction)
                {
                    continue;
                }

                try
                {
                    dbContextProvider.Session.RollbackTransaction();
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            this.IsInTransaction = false;

            if (exceptions != null && exceptions.Count > 0)
            {
                AggregateException aggregateException = new AggregateException("One or more exceptions occurred when rolling back the transaction.", exceptions);
                throw aggregateException;
            }
        }

        public void UseTransaction(IDbTransaction dbTransaction)
        {
            foreach (var pair in this.PersistedDbContextProviders)
            {
                pair.DbContextProvider.Session.UseTransaction(dbTransaction);
            }
        }


        public IDbContextProvider GetDefaultDbContextProvider()
        {
            if (this._defaultDbContextProvider != null)
            {
                return this._defaultDbContextProvider;
            }

            if (this.DbContext.DbContextProviderFactory == null)
            {
                throw new InvalidOperationException("No provider specified.");
            }

            var defaultDbContextProvider = this.DbContext.DbContextProviderFactory.CreateDbContextProvider();
            this.AppendFeatures(defaultDbContextProvider);

            var routeDataSource = new RouteDataSource()
            {
                Name = DbContextFacade.DefaultDataSourceName,
                DbContextProviderFactory = this.DbContext.DbContextProviderFactory
            };
            var physicDataSource = new PhysicDataSource(routeDataSource);
            var pair = new DataSourceDbContextPair(physicDataSource, defaultDbContextProvider);
            this.PersistedDbContextProviders.Add(pair);

            if (this.IsInTransaction)
            {
                defaultDbContextProvider.Session.BeginTransaction(this.IL);
            }

            this._defaultDbContextProvider = defaultDbContextProvider;
            return defaultDbContextProvider;
        }
        public IDbContextProvider GetShardingDbContextProvider()
        {
            if (this._shardingDbContextProvider == null)
            {
                this._shardingDbContextProvider = new ShardingDbContextProvider(this.DbContext);
            }

            return this._shardingDbContextProvider;
        }

        void AppendFeatures(IDbContextProvider dbContextProvider)
        {
            dbContextProvider.Session.CommandTimeout = this.CommandTimeout;
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
    }

}

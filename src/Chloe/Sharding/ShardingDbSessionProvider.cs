using Chloe.Exceptions;
using Chloe.Infrastructure.Interception;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Sharding
{
    class DataSourceDbContextPair
    {
        public DataSourceDbContextPair(IPhysicDataSource dataSource, IDbContext dbContext)
        {
            this.DataSource = dataSource;
            this.DbContext = dbContext;
        }

        public IPhysicDataSource DataSource { get; set; }
        public IDbContext DbContext { get; set; }
    }

    internal class ShardingDbSessionProvider : IDbSession
    {
        bool _disposed = false;
        ShardingDbContext _dbContext;

        public ShardingDbSessionProvider(ShardingDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public IsolationLevel? IL { get; private set; }
        public List<DataSourceDbContextPair> HoldDbContexts { get; set; } = new List<DataSourceDbContextPair>();

        List<IDbCommandInterceptor> _sessionInterceptors;
        public List<IDbCommandInterceptor> SessionInterceptors
        {
            get
            {
                if (this._sessionInterceptors == null)
                    this._sessionInterceptors = new List<IDbCommandInterceptor>(1);

                return this._sessionInterceptors;
            }
        }

        public IDbContext DbContext => this._dbContext;

        public IDbConnection CurrentConnection => throw new NotSupportedException();

        public IDbTransaction CurrentTransaction => throw new NotSupportedException();

        public bool IsInTransaction { get; private set; }

        public int CommandTimeout { get; set; } = 30;

        public void Dispose()
        {
            if (this._disposed)
                return;

            this.Dispose(true);
            this._disposed = true;
        }
        protected virtual void Dispose(bool disposing)
        {
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
        }

        public void AddInterceptor(IDbCommandInterceptor interceptor)
        {
            PublicHelper.CheckNull(interceptor, nameof(interceptor));
            this.SessionInterceptors.Add(interceptor);
        }
        public void RemoveInterceptor(IDbCommandInterceptor interceptor)
        {
            PublicHelper.CheckNull(interceptor, nameof(interceptor));
            this.SessionInterceptors.Remove(interceptor);
        }

        public void BeginTransaction()
        {
            this.BeginTransaction(null);
        }
        public void BeginTransaction(IsolationLevel il)
        {
            this.BeginTransaction(il);
        }
        void BeginTransaction(IsolationLevel? il)
        {
            if (this.IsInTransaction)
            {
                throw new ChloeException("The current session has opened a transaction.");
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

            foreach (var pair in this.HoldDbContexts.ToArray())
            {
                var dbContext = pair.DbContext;
                dbContext.Session.CommitTransaction();
                dbContext.Dispose();
                this.HoldDbContexts.Remove(pair);
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

            foreach (var pair in this.HoldDbContexts.ToArray())
            {
                var dbContext = pair.DbContext;
                try
                {
                    dbContext.Session.RollbackTransaction();
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
                finally
                {
                    dbContext.Dispose();
                    this.HoldDbContexts.Remove(pair);
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
            throw new NotSupportedException();
        }

        public int ExecuteNonQuery(string cmdText, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public int ExecuteNonQuery(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public int ExecuteNonQuery(string cmdText, object parameter)
        {
            throw new NotSupportedException();
        }

        public int ExecuteNonQuery(string cmdText, CommandType cmdType, object parameter)
        {
            throw new NotSupportedException();
        }

        public Task<int> ExecuteNonQueryAsync(string cmdText, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public Task<int> ExecuteNonQueryAsync(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public Task<int> ExecuteNonQueryAsync(string cmdText, object parameter)
        {
            throw new NotSupportedException();
        }

        public Task<int> ExecuteNonQueryAsync(string cmdText, CommandType cmdType, object parameter)
        {
            throw new NotSupportedException();
        }

        public IDataReader ExecuteReader(string cmdText, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public IDataReader ExecuteReader(string cmdText, object parameter)
        {
            throw new NotSupportedException();
        }

        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, object parameter)
        {
            throw new NotSupportedException();
        }

        public Task<IDataReader> ExecuteReaderAsync(string cmdText, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public Task<IDataReader> ExecuteReaderAsync(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public Task<IDataReader> ExecuteReaderAsync(string cmdText, object parameter)
        {
            throw new NotSupportedException();
        }

        public Task<IDataReader> ExecuteReaderAsync(string cmdText, CommandType cmdType, object parameter)
        {
            throw new NotSupportedException();
        }

        public object ExecuteScalar(string cmdText, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public object ExecuteScalar(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public object ExecuteScalar(string cmdText, object parameter)
        {
            throw new NotSupportedException();
        }

        public object ExecuteScalar(string cmdText, CommandType cmdType, object parameter)
        {
            throw new NotSupportedException();
        }

        public Task<object> ExecuteScalarAsync(string cmdText, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public Task<object> ExecuteScalarAsync(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            throw new NotSupportedException();
        }

        public Task<object> ExecuteScalarAsync(string cmdText, object parameter)
        {
            throw new NotSupportedException();
        }

        public Task<object> ExecuteScalarAsync(string cmdText, CommandType cmdType, object parameter)
        {
            throw new NotSupportedException();
        }
    }
}

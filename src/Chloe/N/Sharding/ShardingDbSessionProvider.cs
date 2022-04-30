using Chloe.Infrastructure.Interception;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.Sharding
{
    internal class ShardingDbSessionProvider : IDbSessionProvider
    {
        ShardingDbContextProvider _dbContextProvider;

        public ShardingDbSessionProvider(ShardingDbContextProvider dbContextProvider)
        {
            this._dbContextProvider = dbContextProvider;
        }

        public IDbContextProvider DbContextProvider { get { return this._dbContextProvider; } }

        public IDbConnection CurrentConnection => throw new NotSupportedException();
        public IDbTransaction CurrentTransaction => throw new NotSupportedException();

        public bool IsInTransaction => throw new NotSupportedException();
        public int CommandTimeout { get; set; }

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
        public async Task<int> ExecuteNonQueryAsync(string cmdText, CommandType cmdType, object parameter)
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

        public void UseTransaction(IDbTransaction dbTransaction)
        {
            throw new NotSupportedException();
        }
        public void BeginTransaction()
        {
            throw new NotSupportedException();
        }
        public void BeginTransaction(IsolationLevel il)
        {
            throw new NotSupportedException();
        }
        public void CommitTransaction()
        {
            throw new NotSupportedException();
        }
        public void RollbackTransaction()
        {
            throw new NotSupportedException();
        }

        public void AddInterceptor(IDbCommandInterceptor interceptor)
        {
            throw new NotSupportedException();
        }
        public void RemoveInterceptor(IDbCommandInterceptor interceptor)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {

        }
    }
}

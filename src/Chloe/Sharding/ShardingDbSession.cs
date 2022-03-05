using Chloe.Infrastructure.Interception;
using System.Data;
using System.Threading.Tasks;

namespace Chloe.Sharding
{
    internal class ShardingDbSession : IDbSession
    {
        ShardingDbContext _dbContext;

        public ShardingDbSession(ShardingDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public IDbContext DbContext { get { return this._dbContext; } }
        public IDbConnection CurrentConnection { get { return this._dbContext.DbSessionProvider.CurrentConnection; } }

        public IDbTransaction CurrentTransaction { get { return this._dbContext.DbSessionProvider.CurrentTransaction; } }
        public bool IsInTransaction { get { return this._dbContext.DbSessionProvider.IsInTransaction; } }
        public int CommandTimeout { get { return this._dbContext.DbSessionProvider.CommandTimeout; } set { this._dbContext.DbSessionProvider.CommandTimeout = value; } }

        public int ExecuteNonQuery(string cmdText, params DbParam[] parameters)
        {
            return this._dbContext.DbSessionProvider.ExecuteNonQuery(cmdText, parameters);
        }
        public int ExecuteNonQuery(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            return this._dbContext.DbSessionProvider.ExecuteNonQuery(cmdText, cmdType, parameters);
        }
        public int ExecuteNonQuery(string cmdText, object parameter)
        {
            return this._dbContext.DbSessionProvider.ExecuteNonQuery(cmdText, parameter);
        }
        public int ExecuteNonQuery(string cmdText, CommandType cmdType, object parameter)
        {
            return this._dbContext.DbSessionProvider.ExecuteNonQuery(cmdText, cmdType, parameter);
        }

        public async Task<int> ExecuteNonQueryAsync(string cmdText, params DbParam[] parameters)
        {
            return await this._dbContext.DbSessionProvider.ExecuteNonQueryAsync(cmdText, parameters);
        }
        public async Task<int> ExecuteNonQueryAsync(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            return await this._dbContext.DbSessionProvider.ExecuteNonQueryAsync(cmdText, cmdType, parameters);
        }
        public async Task<int> ExecuteNonQueryAsync(string cmdText, object parameter)
        {
            return await this._dbContext.DbSessionProvider.ExecuteNonQueryAsync(cmdText, parameter);
        }
        public async Task<int> ExecuteNonQueryAsync(string cmdText, CommandType cmdType, object parameter)
        {
            return await this._dbContext.DbSessionProvider.ExecuteNonQueryAsync(cmdText, cmdType, parameter);
        }

        public object ExecuteScalar(string cmdText, params DbParam[] parameters)
        {
            return this._dbContext.DbSessionProvider.ExecuteScalar(cmdText, parameters);
        }
        public object ExecuteScalar(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            return this._dbContext.DbSessionProvider.ExecuteScalar(cmdText, cmdType, parameters);
        }
        public object ExecuteScalar(string cmdText, object parameter)
        {
            return this._dbContext.DbSessionProvider.ExecuteScalar(cmdText, parameter);
        }
        public object ExecuteScalar(string cmdText, CommandType cmdType, object parameter)
        {
            return this._dbContext.DbSessionProvider.ExecuteScalar(cmdText, cmdType, parameter);
        }

        public async Task<object> ExecuteScalarAsync(string cmdText, params DbParam[] parameters)
        {
            return await this._dbContext.DbSessionProvider.ExecuteScalarAsync(cmdText, CommandType.Text, parameters);
        }
        public async Task<object> ExecuteScalarAsync(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            return await this._dbContext.DbSessionProvider.ExecuteScalarAsync(cmdText, cmdType, parameters);
        }
        public async Task<object> ExecuteScalarAsync(string cmdText, object parameter)
        {
            return await this._dbContext.DbSessionProvider.ExecuteScalarAsync(cmdText, parameter);
        }
        public async Task<object> ExecuteScalarAsync(string cmdText, CommandType cmdType, object parameter)
        {
            return await this._dbContext.DbSessionProvider.ExecuteScalarAsync(cmdText, cmdType, parameter);
        }

        public IDataReader ExecuteReader(string cmdText, params DbParam[] parameters)
        {
            return this._dbContext.DbSessionProvider.ExecuteReader(cmdText, parameters);
        }
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            return this._dbContext.DbSessionProvider.ExecuteReader(cmdText, cmdType, parameters);
        }
        public IDataReader ExecuteReader(string cmdText, object parameter)
        {
            return this._dbContext.DbSessionProvider.ExecuteReader(cmdText, parameter);
        }
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, object parameter)
        {
            return this._dbContext.DbSessionProvider.ExecuteReader(cmdText, cmdType, parameter);
        }

        public async Task<IDataReader> ExecuteReaderAsync(string cmdText, params DbParam[] parameters)
        {
            return await this._dbContext.DbSessionProvider.ExecuteReaderAsync(cmdText, parameters);
        }
        public async Task<IDataReader> ExecuteReaderAsync(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            return await this._dbContext.DbSessionProvider.ExecuteReaderAsync(cmdText, cmdType, parameters);
        }
        public async Task<IDataReader> ExecuteReaderAsync(string cmdText, object parameter)
        {
            return await this._dbContext.DbSessionProvider.ExecuteReaderAsync(cmdText, parameter);
        }
        public async Task<IDataReader> ExecuteReaderAsync(string cmdText, CommandType cmdType, object parameter)
        {
            return await this._dbContext.DbSessionProvider.ExecuteReaderAsync(cmdText, cmdType, parameter);
        }

        public void UseTransaction(IDbTransaction dbTransaction)
        {
            this._dbContext.DbSessionProvider.UseTransaction(dbTransaction);
        }
        public void BeginTransaction()
        {
            this._dbContext.DbSessionProvider.BeginTransaction();
        }
        public void BeginTransaction(IsolationLevel il)
        {
            this._dbContext.DbSessionProvider.BeginTransaction(il);
        }
        public void CommitTransaction()
        {
            this._dbContext.DbSessionProvider.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            this._dbContext.DbSessionProvider.RollbackTransaction();
        }

        public void AddInterceptor(IDbCommandInterceptor interceptor)
        {
            this._dbContext.DbSessionProvider.SessionInterceptors.Add(interceptor);
        }
        public void RemoveInterceptor(IDbCommandInterceptor interceptor)
        {
            this._dbContext.DbSessionProvider.SessionInterceptors.Remove(interceptor);
        }

        public void Dispose()
        {
            this._dbContext.Dispose();
        }
    }
}

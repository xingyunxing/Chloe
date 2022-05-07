using Chloe.Infrastructure.Interception;
using System.Data;

namespace Chloe
{
    class DbSessionProvider : IDbSessionProvider
    {
        DbContextProvider _dbContextProvider;
        internal DbSessionProvider(DbContextProvider dbContextProvider)
        {
            this._dbContextProvider = dbContextProvider;
        }

        public IDbContextProvider DbContextProvider { get { return this._dbContextProvider; } }
        public IDbConnection CurrentConnection { get { return this._dbContextProvider.AdoSession.DbConnection; } }

        public IDbTransaction CurrentTransaction { get { return this._dbContextProvider.AdoSession.DbTransaction; } }
        public bool IsInTransaction { get { return this._dbContextProvider.AdoSession.IsInTransaction; } }
        public int CommandTimeout { get { return this._dbContextProvider.AdoSession.CommandTimeout; } set { this._dbContextProvider.AdoSession.CommandTimeout = value; } }

        public int ExecuteNonQuery(string cmdText, params DbParam[] parameters)
        {
            return this.ExecuteNonQuery(cmdText, CommandType.Text, parameters);
        }
        public int ExecuteNonQuery(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            PublicHelper.CheckNull(cmdText, nameof(cmdText));
            return this._dbContextProvider.AdoSession.ExecuteNonQuery(cmdText, parameters, cmdType);
        }
        public int ExecuteNonQuery(string cmdText, object parameter)
        {
            return this.ExecuteNonQuery(cmdText, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }
        public int ExecuteNonQuery(string cmdText, CommandType cmdType, object parameter)
        {
            return this.ExecuteNonQuery(cmdText, cmdType, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }

        public async Task<int> ExecuteNonQueryAsync(string cmdText, params DbParam[] parameters)
        {
            return await this.ExecuteNonQueryAsync(cmdText, CommandType.Text, parameters);
        }
        public async Task<int> ExecuteNonQueryAsync(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            PublicHelper.CheckNull(cmdText, nameof(cmdText));
            return await this._dbContextProvider.AdoSession.ExecuteNonQueryAsync(cmdText, parameters, cmdType);
        }
        public async Task<int> ExecuteNonQueryAsync(string cmdText, object parameter)
        {
            return await this.ExecuteNonQueryAsync(cmdText, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }
        public async Task<int> ExecuteNonQueryAsync(string cmdText, CommandType cmdType, object parameter)
        {
            return await this.ExecuteNonQueryAsync(cmdText, cmdType, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }

        public object ExecuteScalar(string cmdText, params DbParam[] parameters)
        {
            return this.ExecuteScalar(cmdText, CommandType.Text, parameters);
        }
        public object ExecuteScalar(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            PublicHelper.CheckNull(cmdText, nameof(cmdText));
            return this._dbContextProvider.AdoSession.ExecuteScalar(cmdText, parameters, cmdType);
        }
        public object ExecuteScalar(string cmdText, object parameter)
        {
            return this.ExecuteScalar(cmdText, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }
        public object ExecuteScalar(string cmdText, CommandType cmdType, object parameter)
        {
            return this.ExecuteScalar(cmdText, cmdType, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }

        public async Task<object> ExecuteScalarAsync(string cmdText, params DbParam[] parameters)
        {
            return await this.ExecuteScalarAsync(cmdText, CommandType.Text, parameters);
        }
        public async Task<object> ExecuteScalarAsync(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            PublicHelper.CheckNull(cmdText, nameof(cmdText));
            return await this._dbContextProvider.AdoSession.ExecuteScalarAsync(cmdText, parameters, cmdType);
        }
        public async Task<object> ExecuteScalarAsync(string cmdText, object parameter)
        {
            return await this.ExecuteScalarAsync(cmdText, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }
        public async Task<object> ExecuteScalarAsync(string cmdText, CommandType cmdType, object parameter)
        {
            return await this.ExecuteScalarAsync(cmdText, cmdType, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }

        public IDataReader ExecuteReader(string cmdText, params DbParam[] parameters)
        {
            return this.ExecuteReader(cmdText, CommandType.Text, parameters);
        }
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            PublicHelper.CheckNull(cmdText, nameof(cmdText));
            return this._dbContextProvider.AdoSession.ExecuteReader(cmdText, parameters, cmdType);
        }
        public IDataReader ExecuteReader(string cmdText, object parameter)
        {
            return this.ExecuteReader(cmdText, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, object parameter)
        {
            return this.ExecuteReader(cmdText, cmdType, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }

        public async Task<IDataReader> ExecuteReaderAsync(string cmdText, params DbParam[] parameters)
        {
            return await this.ExecuteReaderAsync(cmdText, CommandType.Text, parameters);
        }
        public async Task<IDataReader> ExecuteReaderAsync(string cmdText, CommandType cmdType, params DbParam[] parameters)
        {
            PublicHelper.CheckNull(cmdText, nameof(cmdText));
            return await this._dbContextProvider.AdoSession.ExecuteReaderAsync(cmdText, parameters, cmdType);
        }
        public async Task<IDataReader> ExecuteReaderAsync(string cmdText, object parameter)
        {
            return await this.ExecuteReaderAsync(cmdText, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }
        public async Task<IDataReader> ExecuteReaderAsync(string cmdText, CommandType cmdType, object parameter)
        {
            return await this.ExecuteReaderAsync(cmdText, cmdType, PublicHelper.BuildParams(this._dbContextProvider, parameter));
        }

        public void UseTransaction(IDbTransaction dbTransaction)
        {
            this._dbContextProvider.AdoSession.UseExternalTransaction(dbTransaction);
        }
        public void BeginTransaction()
        {
            this._dbContextProvider.AdoSession.BeginTransaction(null);
        }
        public void BeginTransaction(IsolationLevel il)
        {
            this._dbContextProvider.AdoSession.BeginTransaction(il);
        }
        public void CommitTransaction()
        {
            this._dbContextProvider.AdoSession.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            this._dbContextProvider.AdoSession.RollbackTransaction();
        }

        public void AddInterceptor(IDbCommandInterceptor interceptor)
        {
            PublicHelper.CheckNull(interceptor, nameof(interceptor));
            this._dbContextProvider.AdoSession.SessionInterceptors.Add(interceptor);
        }
        public void RemoveInterceptor(IDbCommandInterceptor interceptor)
        {
            PublicHelper.CheckNull(interceptor, nameof(interceptor));
            this._dbContextProvider.AdoSession.SessionInterceptors.Remove(interceptor);
        }

        public void Dispose()
        {
            this._dbContextProvider.Dispose();
        }
    }
}

using System.Data;

namespace Chloe
{
    public static class DbSessionProviderExtension
    {
        public static void BeginTransaction(this IDbSessionProvider dbSessionProvider, IsolationLevel? il)
        {
            if (il == null)
            {
                dbSessionProvider.BeginTransaction();
                return;
            }

            dbSessionProvider.BeginTransaction(il.Value);
        }

        public static Task<IDataReader> ExecuteReader(this IDbSessionProvider dbSessionProvider, string cmdText, DbParam[] parameters, bool @async)
        {
            if (@async)
                return dbSessionProvider.ExecuteReaderAsync(cmdText, parameters);

            IDataReader dataReader = dbSessionProvider.ExecuteReader(cmdText, parameters);
            return Task.FromResult(dataReader);
        }
        public static Task<IDataReader> ExecuteReader(this IDbSessionProvider dbSessionProvider, string cmdText, CommandType cmdType, DbParam[] parameters, bool @async)
        {
            if (@async)
                return dbSessionProvider.ExecuteReaderAsync(cmdText, cmdType, parameters);

            IDataReader dataReader = dbSessionProvider.ExecuteReader(cmdText, cmdType, parameters);
            return Task.FromResult(dataReader);
        }

        public static Task<int> ExecuteNonQuery(this IDbSessionProvider dbSessionProvider, string cmdText, DbParam[] parameters, bool @async)
        {
            if (@async)
                return dbSessionProvider.ExecuteNonQueryAsync(cmdText, parameters);

            int rowsAffected = dbSessionProvider.ExecuteNonQuery(cmdText, parameters);
            return Task.FromResult(rowsAffected);
        }
        public static Task<int> ExecuteNonQuery(this IDbSessionProvider dbSessionProvider, string cmdText, CommandType cmdType, DbParam[] parameters, bool @async)
        {
            if (@async)
                return dbSessionProvider.ExecuteNonQueryAsync(cmdText, cmdType, parameters);

            int rowsAffected = dbSessionProvider.ExecuteNonQuery(cmdText, cmdType, parameters);
            return Task.FromResult(rowsAffected);
        }

        public static Task<object> ExecuteScalar(this IDbSessionProvider dbSessionProvider, string cmdText, DbParam[] parameters, bool @async)
        {
            if (@async)
                return dbSessionProvider.ExecuteScalarAsync(cmdText, parameters);

            object scalar = dbSessionProvider.ExecuteScalar(cmdText, parameters);
            return Task.FromResult(scalar);
        }
        public static Task<object> ExecuteScalar(this IDbSessionProvider dbSessionProvider, string cmdText, CommandType cmdType, DbParam[] parameters, bool @async)
        {
            if (@async)
                return dbSessionProvider.ExecuteScalarAsync(cmdText, cmdType, parameters);

            object scalar = dbSessionProvider.ExecuteScalar(cmdText, cmdType, parameters);
            return Task.FromResult(scalar);
        }
    }
}

using Chloe.Data;
using Chloe.Infrastructure;
using Chloe.Oracle;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    public class OracleConnectionFactory : IDbConnectionFactory
    {
        string _connString = null;
        public OracleConnectionFactory(string connString)
        {
            this._connString = connString;
        }
        public IDbConnection CreateConnection()
        {
            /*
             * 修改参数绑定方式有两个途径：
             * 1. 使用如下 OracleConnectionDecorator 的方式
             * 2. 使用拦截器修改，在 IDbCommandInterceptor.ReaderExecuting()，IDbCommandInterceptor.NonQueryExecuting()，IDbCommandInterceptor.ScalarExecuting() 方法里对 DbCommand 做处理，参考 ChloeDemo.DbCommandInterceptor 类
             */

            OracleConnection oracleConnection = new OracleConnection(this._connString);
#if NETCORE

            oracleConnection.SqlNetAllowedLogonVersionClient = OracleAllowedLogonVersionClient.Version8;
#endif
            OracleConnectionDecorator conn = new OracleConnectionDecorator(oracleConnection);
            return conn;
        }
    }

    /// <summary>
    /// 该装饰器主要修改参数绑定方式。
    /// </summary>
    internal class OracleConnectionDecorator : DbConnectionDecorator
    {
        private readonly OracleConnection _oracleConnection;

        public OracleConnectionDecorator(OracleConnection oracleConnection) : base(oracleConnection)
        {
            _oracleConnection = oracleConnection;
        }

        public override IDbCommand CreateCommand()
        {
            return new OracleCommandDecorator(_oracleConnection.CreateCommand());
        }

        public override ConnectionState State
        {
            get
            {
                try
                {
                    return _oracleConnection.State;//m_oracleConnectionImpl有可能为空
                }
                catch (Exception)
                {
                    return ConnectionState.Closed;
                }
            }
        }
    }

    internal class OracleCommandDecorator : DbCommandDecorator
    {
        private readonly OracleCommand _oracleCommand;

        public OracleCommandDecorator(OracleCommand oracleCommand) : base(oracleCommand)
        {
            _oracleCommand = oracleCommand;
            _oracleCommand.BindByName = true;
            _oracleCommand.InitialLONGFetchSize = -1;//立即查询LONG和LONG RAW
            _oracleCommand.InitialLOBFetchSize = -1;//立即查询CLOB
        }

        public override IDataReader ExecuteReader()
        {
            var reader = _oracleCommand.ExecuteReader();
#if NETCORE

            reader.SuppressGetDecimalInvalidCastException = true;
#endif
            return reader;
        }

        public override IDataReader ExecuteReader(CommandBehavior behavior)
        {
            var reader = _oracleCommand.ExecuteReader(behavior);
#if NETCORE

            reader.SuppressGetDecimalInvalidCastException = true;
#endif
            return reader;
        }

        public override async Task<IDataReader> ExecuteReaderAsync()
        {
            var reader = await _oracleCommand.ExecuteReaderAsync() as OracleDataReader;
#if NETCORE

            reader.SuppressGetDecimalInvalidCastException = true;
#endif
            return reader;
        }

        public override async Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior)
        {
            var reader = await _oracleCommand.ExecuteReaderAsync(behavior) as OracleDataReader;
#if NETCORE

            reader.SuppressGetDecimalInvalidCastException = true;
#endif
            return reader;
        }
    }
}

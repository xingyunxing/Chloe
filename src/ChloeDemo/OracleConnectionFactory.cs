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
            OracleConnectionDecorator conn = new OracleConnectionDecorator(oracleConnection);
            return conn;
        }
    }

    /// <summary>
    /// 该装饰器主要修改参数绑定方式。
    /// </summary>
    internal class OracleConnectionDecorator : DbConnectionDecorator, IDbConnection, IDisposable
    {
        private readonly OracleConnection _oracleConnection;

        public OracleConnectionDecorator(OracleConnection oracleConnection) : base(oracleConnection)
        {
            _oracleConnection = oracleConnection;
        }

        public override IDbCommand CreateCommand()
        {
            return new OracleCommandDecorator(_oracleConnection);
        }
    }

    internal class OracleCommandDecorator : IDbCommand
    {
        private readonly OracleCommand _oracleCommand;

        public OracleCommandDecorator(OracleConnection oracleConnection)
        {
            _oracleCommand = oracleConnection.CreateCommand();
            _oracleCommand.BindByName = true;
            _oracleCommand.InitialLONGFetchSize = -1;//立即查询LONG和LONG RAW
            _oracleCommand.InitialLOBFetchSize = -1;//立即查询CLOB
        }

        public string CommandText { get => _oracleCommand.CommandText; set => _oracleCommand.CommandText = value; }
        public int CommandTimeout { get => _oracleCommand.CommandTimeout; set => _oracleCommand.CommandTimeout = value; }
        public CommandType CommandType { get => _oracleCommand.CommandType; set => _oracleCommand.CommandType = value; }
        public IDbConnection? Connection { get => _oracleCommand.Connection; set => _oracleCommand.Connection = value as OracleConnection; }

        public IDataParameterCollection Parameters => _oracleCommand.Parameters;

        public IDbTransaction? Transaction { get => _oracleCommand.Transaction; set => _oracleCommand.Transaction = value as OracleTransaction; }
        public UpdateRowSource UpdatedRowSource { get => _oracleCommand.UpdatedRowSource; set => _oracleCommand.UpdatedRowSource = value; }

        public void Cancel() => _oracleCommand.Cancel();

        public IDbDataParameter CreateParameter() => _oracleCommand.CreateParameter();

        public void Dispose() => _oracleCommand.Dispose();

        public int ExecuteNonQuery() => _oracleCommand.ExecuteNonQuery();

        public IDataReader ExecuteReader()
        {
            var reader = _oracleCommand.ExecuteReader();
            reader.SuppressGetDecimalInvalidCastException = true;
            return reader;
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            var reader = _oracleCommand.ExecuteReader();
            reader.SuppressGetDecimalInvalidCastException = true;
            return reader;
        }

        public object? ExecuteScalar() => _oracleCommand.ExecuteScalar();

        public void Prepare() => _oracleCommand.Prepare();
    }
}

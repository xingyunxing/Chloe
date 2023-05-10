using Chloe.Infrastructure.Interception;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    /// <summary>
    /// sql 拦截器。可以输出 sql 语句极其相应的参数
    /// </summary>
    class DbCommandInterceptor : IDbCommandInterceptor
    {
        /// <summary>
        /// oracle：修改参数绑定方式
        /// </summary>
        /// <param name="command"></param>
        void BindByName(IDbCommand command)
        {
            if (command is OracleCommand)
            {
                (command as OracleCommand).BindByName = true;
            }
        }

        /// <summary>
        /// 解决 odbc 日期类型报精度溢出错误问题
        /// </summary>
        /// <param name="command"></param>
        void FixOdbcDateTimePrecision(IDbCommand command)
        {
            if (command is not OdbcCommand)
            {
                return;
            }

            foreach (IDbDataParameter param in command.Parameters)
            {


                var value = param.Value;
                if (value == null)
                {
                    continue;
                }

                var valueType = value.GetType();
                if (valueType == typeof(DateTime))
                {
                    //解决日期类型报精度溢出错误问题
                    param.Precision = 23;
                    param.Scale = 3;
                }
            }
        }

        public void ReaderExecuting(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            this.BindByName(command);
            this.FixOdbcDateTimePrecision(command);

            //interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
        }
        public void ReaderExecuted(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            //DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            //Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            //if (interceptionContext.Exception == null)
            //    Console.WriteLine(interceptionContext.Result.FieldCount);
        }

        public void NonQueryExecuting(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            this.BindByName(command);
            this.FixOdbcDateTimePrecision(command);

            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
        }
        public void NonQueryExecuted(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }

        public void ScalarExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            this.BindByName(command);
            this.FixOdbcDateTimePrecision(command);

            //interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
        }
        public void ScalarExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            //Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }


        public static string AppendDbCommandInfo(IDbCommand command)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IDbDataParameter param in command.Parameters)
            {
                if (param == null)
                    continue;

                object value = null;
                if (param.Value == null || param.Value == DBNull.Value)
                {
                    value = "NULL";
                }
                else
                {
                    value = param.Value;

                    if (param.DbType == DbType.String || param.DbType == DbType.AnsiString || param.DbType == DbType.DateTime)
                        value = "'" + value + "'";
                }

                sb.AppendFormat("{3} {0} {1} = {2};", Enum.GetName(typeof(DbType), param.DbType), param.ParameterName, value, Enum.GetName(typeof(ParameterDirection), param.Direction));
                sb.AppendLine();
            }

            sb.AppendLine(command.CommandText);

            return sb.ToString();
        }
    }
}

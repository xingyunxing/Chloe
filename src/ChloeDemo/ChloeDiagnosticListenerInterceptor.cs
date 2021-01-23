using Chloe.Infrastructure.Interception;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Chloe
{
    /// <summary>
    /// 性能诊断
    /// </summary>
    public class ChloeDiagnosticListenerInterceptor : IDbCommandInterceptor
    {
        static readonly DiagnosticListener _diagnosticListener =
            new DiagnosticListener(ChloeDiagnosticListenerNames.DiagnosticListenerName);

        public void ReaderExecuting(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            interceptionContext.DataBag.Add("startTime", DateTime.Now);
            if (_diagnosticListener.IsEnabled(ChloeDiagnosticListenerNames.ReaderExecuting))
            {
                var eventData = new ChloeDbCommandEventData()
                {
                    CommandText = command.CommandText,
                    ElapsedTime = null,
                    Exception = null
                };

                this.FillDbCommandParams(eventData, command);

                _diagnosticListener.Write(ChloeDiagnosticListenerNames.ReaderExecuting, eventData);
            }
        }
        public void ReaderExecuted(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            if (_diagnosticListener.IsEnabled(ChloeDiagnosticListenerNames.ReaderExecuted))
            {
                DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
                var eventData = new ChloeDbCommandEventData()
                {
                    CommandText = command.CommandText,
                    ElapsedTime = (long)DateTime.Now.Subtract(startTime).TotalMilliseconds,
                    Exception = interceptionContext.Exception
                };

                this.FillDbCommandParams(eventData, command);

                _diagnosticListener.Write(ChloeDiagnosticListenerNames.ReaderExecuted, eventData);
            }
        }

        public void NonQueryExecuting(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            interceptionContext.DataBag.Add("startTime", DateTime.Now);
            if (_diagnosticListener.IsEnabled(ChloeDiagnosticListenerNames.NonQueryExecuting))
            {
                var eventData = new ChloeDbCommandEventData()
                {
                    CommandText = command.CommandText,
                    ElapsedTime = null,
                    Exception = null
                };

                this.FillDbCommandParams(eventData, command);

                _diagnosticListener.Write(ChloeDiagnosticListenerNames.NonQueryExecuting, eventData);
            }
        }
        public void NonQueryExecuted(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            if (_diagnosticListener.IsEnabled(ChloeDiagnosticListenerNames.NonQueryExecuted))
            {
                DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
                var eventData = new ChloeDbCommandEventData()
                {
                    CommandText = command.CommandText,
                    ElapsedTime = (long)DateTime.Now.Subtract(startTime).TotalMilliseconds,
                    Exception = interceptionContext.Exception
                };

                this.FillDbCommandParams(eventData, command);

                _diagnosticListener.Write(ChloeDiagnosticListenerNames.NonQueryExecuted, eventData);
            }
        }

        public void ScalarExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            interceptionContext.DataBag.Add("startTime", DateTime.Now);
            if (_diagnosticListener.IsEnabled(ChloeDiagnosticListenerNames.ScalarExecuting))
            {
                var eventData = new ChloeDbCommandEventData()
                {
                    CommandText = command.CommandText,
                    ElapsedTime = null,
                    Exception = null
                };

                this.FillDbCommandParams(eventData, command);

                _diagnosticListener.Write(ChloeDiagnosticListenerNames.ScalarExecuting, eventData);
            }
        }
        public void ScalarExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            if (_diagnosticListener.IsEnabled(ChloeDiagnosticListenerNames.ScalarExecuted))
            {
                DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
                var eventData = new ChloeDbCommandEventData()
                {
                    CommandText = command.CommandText,
                    ElapsedTime = (long)DateTime.Now.Subtract(startTime).TotalMilliseconds,
                    Exception = interceptionContext.Exception
                };

                this.FillDbCommandParams(eventData, command);

                _diagnosticListener.Write(ChloeDiagnosticListenerNames.ScalarExecuted, eventData);
            }
        }

        void FillDbCommandParams(ChloeDbCommandEventData eventData, IDbCommand command)
        {
            foreach (IDbDataParameter item in command.Parameters)
            {
                DbCommandParam p = new DbCommandParam();
                p.Name = item.ParameterName;
                p.Value = item.Value == DBNull.Value ? null : item.Value;
                p.DbType = item.DbType;

                eventData.Parameters.Add(p);
            }
        }
    }
}

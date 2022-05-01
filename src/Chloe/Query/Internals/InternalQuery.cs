using Chloe.Core;
using Chloe.Data;
using Chloe.Infrastructure;
using Chloe.Mapper;
using Chloe.Mapper.Activators;
using Chloe.Query.QueryState;
using Chloe.Query.Visitors;
using Chloe.Reflection;
using Chloe.Utility;
using System.Data;
using System.Threading;

namespace Chloe.Query.Internals
{
    class InternalQuery<T> : FeatureEnumerable<T>, IEnumerable<T>, IAsyncEnumerable<T>
    {
        Query<T> _query;

        internal InternalQuery(Query<T> query)
        {
            this._query = query;
        }

        DbCommandFactor GenerateCommandFactor()
        {
            QueryStateBase qs = QueryExpressionResolver.Resolve(this._query.QueryExpression, new ScopeParameterDictionary(), new StringSet());
            MappingData data = qs.GenerateMappingData();

            IObjectActivator objectActivator;
            if (data.IsTrackingQuery)
                objectActivator = data.ObjectActivatorCreator.CreateObjectActivator(data.Context.DbContextProvider);
            else
                objectActivator = data.ObjectActivatorCreator.CreateObjectActivator();

            IDbExpressionTranslator translator = data.Context.DbContextProvider.DatabaseProvider.CreateDbExpressionTranslator();
            DbCommandInfo dbCommandInfo = translator.Translate(data.SqlQuery);

            DbCommandFactor commandFactor = new DbCommandFactor(data.Context.DbContextProvider, objectActivator, dbCommandInfo.CommandText, dbCommandInfo.GetParameters());
            return commandFactor;
        }

        public override string ToString()
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            return AppendDbCommandInfo(commandFactor.CommandText, commandFactor.Parameters);
        }

        static IDataReader DataReaderReady(IDataReader dataReader, IObjectActivator objectActivator)
        {
            if (objectActivator is RootEntityActivator)
            {
                dataReader = new QueryDataReader(dataReader);
            }

            objectActivator.Prepare(dataReader);

            return dataReader;
        }

        static string AppendDbCommandInfo(string cmdText, DbParam[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (parameters != null)
            {
                foreach (DbParam param in parameters)
                {
                    if (param == null)
                        continue;

                    string typeName = null;
                    object value = null;
                    Type parameterType;
                    if (param.Value == null || param.Value == DBNull.Value)
                    {
                        parameterType = param.Type;
                        value = "NULL";
                    }
                    else
                    {
                        value = param.Value;
                        parameterType = param.Value.GetType();

                        if (parameterType == typeof(string) || parameterType == typeof(DateTime))
                            value = "'" + value + "'";
                    }

                    if (parameterType != null)
                        typeName = GetTypeName(parameterType);

                    sb.AppendFormat("{0} {1} = {2};", typeName, param.Name, value);
                    sb.AppendLine();
                }
            }

            sb.AppendLine(cmdText);

            return sb.ToString();
        }
        static string GetTypeName(Type type)
        {
            Type underlyingType;
            if (ReflectionExtension.IsNullable(type, out underlyingType))
            {
                return string.Format("Nullable<{0}>", GetTypeName(underlyingType));
            }

            return type.Name;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            QueryEnumerator<T> enumerator = new QueryEnumerator<T>(async (@async) =>
            {
                IDataReader dataReader = await commandFactor.DbContext.Session.ExecuteReader(commandFactor.CommandText, CommandType.Text, commandFactor.Parameters, @async);

                return DataReaderReady(dataReader, commandFactor.ObjectActivator);
            }, commandFactor.ObjectActivator, cancellationToken);

            return enumerator;
        }
    }
}

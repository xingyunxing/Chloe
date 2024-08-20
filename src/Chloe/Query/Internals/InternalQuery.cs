using Chloe.Core;
using Chloe.Data;
using Chloe.DbExpressions;
using Chloe.Infrastructure;
using Chloe.Mapper;
using Chloe.Mapper.Activators;
using Chloe.Query.QueryState;
using Chloe.Query.Visitors;
using Chloe.QueryExpressions;
using Chloe.Reflection;
using Chloe.Utility;
using Chloe.Visitors;
using System.Data;
using System.Threading;

namespace Chloe.Query.Internals
{
    class InternalQuery<T> : FeatureEnumerable<T>, IEnumerable<T>, IAsyncEnumerable<T>
    {
        Query<T> _query;
        QueryContext _queryContext;

        internal InternalQuery(Query<T> query)
        {
            this._query = query;
            this._queryContext = new QueryContext(this._query.DbContextProvider);
        }

        DbCommandFactor GenerateCommandFactor()
        {
            QueryExpression queryExpression = QueryObjectExpressionTransformer.Transform(this._query.QueryExpression);

            List<object> variables;
            queryExpression = ExpressionVariableReplacer.Replace(queryExpression, out variables);


            /*
             * 注：任何时候千万不能用 Expression.Constant(variable) 包装变量，因为如果使用 ConstantExpression 包装变量，ExpressionEqualityComparer 计算表达式树时始终返回一个新的哈希值，会导致 QueryPlanContainer 的缓存无限暴涨。
             * 同理，在任何时候都不要用 ConstantExpression 来包装你的变量
             */
            QueryPlan queryPlan = QueryPlanContainer.GetOrAdd(queryExpression, () =>
            {
                return MakeQueryPlan(this._queryContext, queryExpression);
            });

            DbExpression sqlQuery = queryPlan.SqlQuery;
            IObjectActivator objectActivator = queryPlan.ObjectActivator.Clone();

            IDbExpressionTranslator translator = this._queryContext.DbContextProvider.DatabaseProvider.CreateDbExpressionTranslator();
            DbCommandInfo dbCommandInfo = translator.Translate(sqlQuery, variables);

            DbCommandFactor commandFactor = new DbCommandFactor(this._queryContext.DbContextProvider, objectActivator, dbCommandInfo.CommandText, dbCommandInfo.GetParameters());
            return commandFactor;
        }

        static QueryPlan MakeQueryPlan(QueryContext queryContext, QueryExpression queryExpression)
        {
            QueryStateBase qs = QueryExpressionResolver.Resolve(queryContext, queryExpression, new ScopeParameterDictionary(), new StringSet());
            MappingData data = qs.GenerateMappingData();

            IObjectActivator objectActivator = data.ObjectActivatorCreator.CreateObjectActivator(data.IsTrackingQuery);

            return new QueryPlan() { KeyStub = queryExpression, ObjectActivator = objectActivator, SqlQuery = data.SqlQuery, CanBeCachced = data.CanBeCachced };
        }


        DbCommandFactor GenerateCommandFactorWithoutCache()
        {
            QueryExpression queryExpression = QueryObjectExpressionTransformer.Transform(this._query.QueryExpression);
            QueryStateBase qs = QueryExpressionResolver.Resolve(this._queryContext, queryExpression, new ScopeParameterDictionary(), new StringSet());
            MappingData data = qs.GenerateMappingData();

            IObjectActivator objectActivator = data.ObjectActivatorCreator.CreateObjectActivator(data.IsTrackingQuery);

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
            QueryEnumerator<T> enumerator = new QueryEnumerator<T>(this._queryContext, async (@async) =>
            {
                IDataReader dataReader = await commandFactor.DbContext.Session.ExecuteReader(commandFactor.CommandText, CommandType.Text, commandFactor.Parameters, @async);

                return DataReaderReady(dataReader, commandFactor.ObjectActivator);
            }, commandFactor.ObjectActivator, cancellationToken);

            return enumerator;
        }
    }
}

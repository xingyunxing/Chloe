using Chloe.DbExpressions;
using Chloe.Extensions;
using Chloe.Infrastructure;
using Chloe.Query;
using Chloe.Query.QueryState;
using Chloe.Query.Visitors;
using Chloe.QueryExpressions;
using Chloe.Utility;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Visitors
{
    /// <summary>
    /// 注：使用此 parser 处理的表达式树中如果包含了 IQuery.First、IQuery.FirstOrDefault、IQuery.ToList、IQuery.Any 以及一些聚合函数的调用，
    /// 请先使用 QueryObjectExpressionTransformer 将表达式树中的 IQuery 对象进行转换
    /// </summary>
    internal class GeneralExpressionParser : ExpressionVisitorBase
    {
        static List<string> AggregateMethods = QueryObjectExpressionTransformer.AggregateMethods;

        QueryContext _queryContext;
        ScopeParameterDictionary _scopeParameters;
        StringSet _scopeTables;

        public GeneralExpressionParser(QueryContext queryContext, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            this._queryContext = queryContext;
            this._scopeParameters = scopeParameters;
            this._scopeTables = scopeTables;
        }
        public static DbExpression Parse(QueryContext queryContext, Expression exp, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            GeneralExpressionParser visitor = new GeneralExpressionParser(queryContext, scopeParameters, scopeTables);
            return visitor.Visit(exp);
        }

        IObjectModel FindModel(ParameterExpression exp)
        {
            IObjectModel model = this._scopeParameters.Get(exp);
            return model;
        }

        protected override DbExpression VisitLambda(LambdaExpression lambda)
        {
            return base.VisitLambda(lambda);
        }

        protected override DbExpression VisitMemberAccess(MemberExpression exp)
        {
            ParameterExpression p;
            if (ExpressionExtension.IsDerivedFromParameter(exp, out p))
            {
                IObjectModel model = this.FindModel(p);
                return model.GetDbExpression(exp);
            }

            return base.VisitMemberAccess(exp);
        }

        protected override DbExpression VisitParameter(ParameterExpression exp)
        {
            //只支持 MappingFieldExpression 类型，即类似 q.Select(a=> a.Id).Where(a=> a > 0) 这种情况，也就是 ParameterExpression.Type 为基本映射类型。

            if (MappingTypeSystem.IsMappingType(exp.Type))
            {
                IObjectModel model = this.FindModel(exp);
                PrimitiveObjectModel resultModel = (PrimitiveObjectModel)model;
                return resultModel.Expression;
            }
            else
                throw new NotSupportedException(exp.ToString());
        }

        protected override DbExpression VisitMethodCall(MethodCallExpression exp)
        {
            /*
             * query.First()，query.FirstOrDefault() --> (select top 1 T.Name from T)
             * query.ToList() --> select T.Id from T
             * query.Any() --> exists 查询
             */

            if (exp.Object != null && Utils.IsIQueryType(exp.Object.Type))
            {
                string methodName = exp.Method.Name;
                if (methodName == nameof(IQuery<int>.First) || methodName == nameof(IQuery<int>.FirstOrDefault))
                {
                    return this.Process_MethodCall_First_Or_FirstOrDefault(exp);
                }
                else if (methodName == nameof(IQuery<int>.ToList))
                {
                    return this.ConvertToDbSqlQueryExpression(UnwrapperQueryExpression(exp.Object), exp.Type);
                }
                else if (methodName == nameof(IQuery<int>.Any))
                {
                    /* query.Any() --> exists 查询 */
                    DbSqlQueryExpression sqlQuery = this.ConvertToDbSqlQueryExpression(UnwrapperQueryExpression(exp.Object), exp.Type);
                    return new DbExistsExpression(sqlQuery);
                }
                else if (AggregateMethods.Contains(methodName))
                {
                    return this.Process_MethodCall_Aggregate(exp);
                }
            }

            return base.VisitMethodCall(exp);
        }

        DbExpression Process_MethodCall_First_Or_FirstOrDefault(MethodCallExpression exp)
        {
            /*
             * query.First() | query.FirstOrDefault() -> (select top 1 T.Name from T)
             */

            QueryExpression queryExpression = UnwrapperQueryExpression(exp.Object);
            TakeExpression takeExpression = new TakeExpression(queryExpression.ElementType, queryExpression, 1);
            DbSubQueryExpression subQueryExpression = ConvertToDbSubQueryExpression(takeExpression, exp.Type);
            return subQueryExpression;
        }
        DbExpression Process_MethodCall_Aggregate(MethodCallExpression exp)
        {
            QueryExpression queryExpression = UnwrapperQueryExpression(exp.Object);
            MethodInfo calledAggregateMethod = exp.Method;
            List<Expression> arguments = exp.Arguments.Select(a => a.StripQuotes()).ToList();

            AggregateQueryExpression aggregateQueryExpression = new AggregateQueryExpression(queryExpression, exp.Method, arguments);

            return this.ConvertToDbSubQueryExpression(aggregateQueryExpression, calledAggregateMethod.ReturnType);
        }

        DbSubQueryExpression ConvertToDbSubQueryExpression(QueryExpression queryExpression, Type resultType)
        {
            DbSqlQueryExpression sqlQueryExpression = ConvertToDbSqlQueryExpression(queryExpression, resultType);
            DbSubQueryExpression subQueryExpression = new DbSubQueryExpression(sqlQueryExpression);
            return subQueryExpression;
        }
        DbSqlQueryExpression ConvertToDbSqlQueryExpression(QueryExpression queryExpression, Type resultType)
        {
            QueryStateBase qs = QueryExpressionResolver.Resolve(this._queryContext, queryExpression, this._scopeParameters, this._scopeTables);
            MappingData mappingData = qs.GenerateMappingData();

            DbSqlQueryExpression sqlQueryExpression = mappingData.SqlQuery.Update(resultType);
            return sqlQueryExpression;
        }

        QueryExpression UnwrapperQueryExpression(Expression exp)
        {
            //提取 QueryObjectExpressionTransformer 包装的 QueryExpression 对象
            ConstantExpression constantExpression = (ConstantExpression)exp.StripConvert();
            return (QueryExpression)constantExpression.Value;
        }

    }
}

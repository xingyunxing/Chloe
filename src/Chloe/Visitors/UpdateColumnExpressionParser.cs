using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Query;
using Chloe.Utility;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    /// <summary>
    /// 解析 a => new User() { a.Name = name + "1" } lambda 中的 name + "1" 部分
    /// </summary>
    public class UpdateColumnExpressionParser
    {
        QueryContext _queryContext;
        TypeDescriptor _typeDescriptor;
        DbTable _dbTable;
        ParameterExpression _parameterExp;
        public UpdateColumnExpressionParser(TypeDescriptor typeDescriptor, DbTable dbTable, ParameterExpression parameterExp, QueryContext queryContext)
        {
            this._typeDescriptor = typeDescriptor;
            this._dbTable = dbTable;
            this._parameterExp = parameterExp;
            this._queryContext = queryContext;
        }

        public DbExpression Parse(Expression updateColumnExp)
        {
            ComplexObjectModel objectModel = this._typeDescriptor.GenObjectModel(this._dbTable, this._queryContext, new QueryOptions());
            ScopeParameterDictionary scopeParameters = new ScopeParameterDictionary(1);
            scopeParameters.Add(this._parameterExp, objectModel);

            StringSet scopeTables = new StringSet();
            scopeTables.Add(this._dbTable.Name);

            DbExpression conditionExp = UpdateColumnExpressionParserImpl.Parse(updateColumnExp, this._queryContext, scopeParameters, scopeTables);
            return conditionExp;
        }

        class UpdateColumnExpressionParserImpl : ExpressionVisitor<DbExpression>
        {
            public static DbExpression Parse(Expression updateColumnExp, QueryContext queryContext, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
            {
                return GeneralExpressionParser.Parse(queryContext, updateColumnExp, scopeParameters, scopeTables);
            }

            public static DbExpression Parse(ParameterExpression parameterExp, Expression updateColumnExp, TypeDescriptor typeDescriptor, DbTable dbTable, QueryContext queryContext, QueryOptions queryOptions)
            {
                ComplexObjectModel objectModel = typeDescriptor.GenObjectModel(dbTable, queryContext, queryOptions);
                ScopeParameterDictionary scopeParameters = new ScopeParameterDictionary(1);
                scopeParameters.Add(parameterExp, objectModel);

                StringSet scopeTables = new StringSet();
                scopeTables.Add(dbTable.Name);

                DbExpression conditionExp = UpdateColumnExpressionParserImpl.Parse(updateColumnExp, queryContext, scopeParameters, scopeTables);
                return conditionExp;
            }
        }
    }
}

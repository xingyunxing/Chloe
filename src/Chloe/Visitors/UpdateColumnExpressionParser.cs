using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Query;
using Chloe.Utility;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    public class UpdateColumnExpressionParser
    {
        TypeDescriptor _typeDescriptor;
        DbTable _dbTable;
        ParameterExpression _parameterExp;
        public UpdateColumnExpressionParser(TypeDescriptor typeDescriptor, DbTable dbTable, ParameterExpression parameterExp)
        {
            this._typeDescriptor = typeDescriptor;
            this._dbTable = dbTable;
            this._parameterExp = parameterExp;
        }

        public DbExpression Parse(Expression updateColumnExp)
        {
            ComplexObjectModel objectModel = this._typeDescriptor.GenObjectModel(this._dbTable);
            ScopeParameterDictionary scopeParameters = new ScopeParameterDictionary(1);
            scopeParameters.Add(this._parameterExp, objectModel);

            StringSet scopeTables = new StringSet();
            scopeTables.Add(this._dbTable.Name);

            DbExpression conditionExp = UpdateColumnExpressionParserImpl.Parse(updateColumnExp, scopeParameters, scopeTables);
            return conditionExp;
        }

        class UpdateColumnExpressionParserImpl : ExpressionVisitor<DbExpression>
        {
            public static DbExpression Parse(Expression updateColumnExp, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
            {
                return GeneralExpressionParser.Parse(updateColumnExp, scopeParameters, scopeTables);
            }

            public static DbExpression Parse(ParameterExpression parameterExp, Expression updateColumnExp, TypeDescriptor typeDescriptor, DbTable dbTable)
            {
                ComplexObjectModel objectModel = typeDescriptor.GenObjectModel(dbTable);
                ScopeParameterDictionary scopeParameters = new ScopeParameterDictionary(1);
                scopeParameters.Add(parameterExp, objectModel);

                StringSet scopeTables = new StringSet();
                scopeTables.Add(dbTable.Name);

                DbExpression conditionExp = UpdateColumnExpressionParserImpl.Parse(updateColumnExp, scopeParameters, scopeTables);
                return conditionExp;
            }
        }
    }
}

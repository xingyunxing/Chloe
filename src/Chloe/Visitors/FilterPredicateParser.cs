using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Query;
using Chloe.Utility;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    public class FilterPredicateParser : ExpressionVisitor<DbExpression>
    {
        public static DbExpression Parse(LambdaExpression filterPredicate, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            return GeneralExpressionParser.Parse(BooleanResultExpressionTransformer.Transform(filterPredicate), scopeParameters, scopeTables);
        }

        public static DbExpression Parse(LambdaExpression filterPredicate, TypeDescriptor typeDescriptor, DbTable dbTable)
        {
            return Parse(filterPredicate, typeDescriptor, dbTable, new QueryOptions());
        }

        public static DbExpression Parse(LambdaExpression filterPredicate, TypeDescriptor typeDescriptor, DbTable dbTable, QueryOptions queryOptions)
        {
            ComplexObjectModel objectModel = typeDescriptor.GenObjectModel(dbTable, queryOptions);
            ScopeParameterDictionary scopeParameters = new ScopeParameterDictionary(1);
            scopeParameters.Add(filterPredicate.Parameters[0], objectModel);

            StringSet scopeTables = new StringSet();
            scopeTables.Add(dbTable.Name);

            DbExpression conditionExp = FilterPredicateParser.Parse(filterPredicate, scopeParameters, scopeTables);
            return conditionExp;
        }
    }
}

using Chloe.DbExpressions;

namespace Chloe.RDBMS.MethodHandlers
{
    public class IsNullOrEmpty_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_IsNullOrEmpty)
                return false;

            return true;
        }
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            DbExpression e = exp.Arguments.First();

            DbExpression expression;

            DbEqualExpression equalNullExpression = new DbEqualExpression(e, DbConstantExpression.StringNull);
            expression = equalNullExpression;

            if (!generator.Options.RegardEmptyStringAsNull)
            {
                DbEqualExpression equalEmptyExpression = new DbEqualExpression(e, DbConstantExpression.StringEmpty);
                DbOrExpression orExpression = new DbOrExpression(equalNullExpression, equalEmptyExpression);
                expression = orExpression;
            }

            expression.Accept(generator);
        }
    }
}

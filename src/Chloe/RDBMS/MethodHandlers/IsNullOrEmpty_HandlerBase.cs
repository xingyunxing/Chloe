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
            DbEqualExpression equalNullExpression = DbExpression.Equal(e, DbConstantExpression.StringNull);
            DbEqualExpression equalEmptyExpression = DbExpression.Equal(e, DbConstantExpression.StringEmpty);

            DbOrExpression orExpression = DbExpression.Or(equalNullExpression, equalEmptyExpression);

            orExpression.Accept(generator);
        }
    }
}

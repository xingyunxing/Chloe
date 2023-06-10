using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.Oracle.MethodHandlers
{
    class IsEqual_Handler : IsEqual_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            DbExpression left = exp.Arguments[0];
            DbExpression right = exp.Arguments[1];

            left = DbExpressionExtension.StripInvalidConvert(left);
            right = DbExpressionExtension.StripInvalidConvert(right);

            //明确 left right 其中一边一定为 null
            if (DbExpressionHelper.AffirmExpressionRetValueIsNullOrEmpty(right))
            {
                left.Accept(generator);
                generator.SqlBuilder.Append(" IS NULL");
                return;
            }

            if (DbExpressionHelper.AffirmExpressionRetValueIsNullOrEmpty(left))
            {
                right.Accept(generator);
                generator.SqlBuilder.Append(" IS NULL");
                return;
            }

            PublicHelper.AmendDbInfo(left, right);

            left.Accept(generator);
            generator.SqlBuilder.Append(" = ");
            right.Accept(generator);
        }
    }
}

using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.Oracle.MethodHandlers
{
    class Contains_Handler : Contains_HandlerBase
    {
        protected override void Method_String_Contains(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" LIKE '%' || ");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(" || '%'");
        }

        protected override void In(SqlGeneratorBase generator, List<DbExpression> elementExps, DbExpression operand)
        {
            if (elementExps.Count == 0)
            {
                generator.SqlBuilder.Append("1=0");
                return;
            }

            List<List<DbExpression>> batches = Utils.InBatches(elementExps, UtilConstants.InElements);

            if (batches.Count > 1)
                generator.SqlBuilder.Append("(");

            for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++)
            {
                if (batchIndex > 0)
                    generator.SqlBuilder.Append(" OR ");

                List<DbExpression> batch = batches[batchIndex];

                operand.Accept(generator);
                generator.SqlBuilder.Append(" IN (");

                for (int i = 0; i < batch.Count; i++)
                {
                    if (i > 0)
                        generator.SqlBuilder.Append(",");

                    batch[i].Accept(generator);
                }

                generator.SqlBuilder.Append(")");
            }

            if (batches.Count > 1)
                generator.SqlBuilder.Append(")");
        }
    }
}

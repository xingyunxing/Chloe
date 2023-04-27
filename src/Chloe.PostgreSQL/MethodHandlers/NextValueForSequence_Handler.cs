using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.PostgreSQL.MethodHandlers
{
    class NextValueForSequence_Handler : NextValueForSequence_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            /* select nextval('public.users_auto_id')  */

            string sequenceName = (string)exp.Arguments[0].Evaluate();
            if (string.IsNullOrEmpty(sequenceName))
                throw new ArgumentException("The sequence name cannot be empty.");

            string sequenceSchema = (string)exp.Arguments[1].Evaluate();

            generator.SqlBuilder.Append("nextval('");

            if (!string.IsNullOrEmpty(sequenceSchema))
            {
                generator.SqlBuilder.Append(sequenceSchema, ".");
            }

            generator.SqlBuilder.Append(sequenceName);
            generator.SqlBuilder.Append("')");
        }
    }
}

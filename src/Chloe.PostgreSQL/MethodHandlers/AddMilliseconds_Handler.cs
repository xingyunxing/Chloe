using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.PostgreSQL.MethodHandlers
{
    class AddMilliseconds_Handler : AddMilliseconds_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" + interval ");
            generator.SqlBuilder.Append("'");

            var argExp = exp.Arguments[0];
            if (!argExp.IsEvaluable())
                throw PublicHelper.MakeNotSupportedMethodException(exp.Method);

            var arg = argExp.Evaluate();
            generator.SqlBuilder.Append(arg.ToString());
            generator.SqlBuilder.Append(" ");
            generator.SqlBuilder.Append("milliseconds");
            generator.SqlBuilder.Append("'");
        }
    }
}

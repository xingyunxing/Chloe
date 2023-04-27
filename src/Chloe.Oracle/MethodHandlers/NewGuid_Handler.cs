using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.Oracle.MethodHandlers
{
    class NewGuid_Handler : NewGuid_HandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            return false;
        }

        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            //返回的是一个长度为 16 的 byte[]
            generator.SqlBuilder.Append("SYS_GUID()");
        }
    }
}

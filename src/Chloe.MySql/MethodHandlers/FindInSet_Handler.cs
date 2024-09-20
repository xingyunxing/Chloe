#region 
/*----------------------------------------------------------------
      Copyright (C) 2024   Sheldon

      文件名：ZSiteConfigModel
      文件功能描述：

      创建标识：Sheldon - 2024/04/11 11:19

      修改标识：Sheldon - 2024/04/11 11:19
      修改描述：[v1.0.1] 添加“xx”功能： ()

      修改标识：LG - 2024/04/11 11:19
      修改描述：处理偶然出现的 xxx 问题
  ----------------------------------------------------------------*/
#endregion

using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.MySql.MethodHandlers
{
    class FindInSet_Handler: FindInSet_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append(" FIND_IN_SET(");
            exp.Arguments[1].Accept(generator);
            generator.SqlBuilder.Append(",");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}

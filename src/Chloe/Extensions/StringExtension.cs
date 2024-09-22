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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Extensions
{
    public static class StringExtension
    {
        public static bool FindInSet(this string str, string value)
        {
            return str.Split(',').Contains(value);
        }
    }
}

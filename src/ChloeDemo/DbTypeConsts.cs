using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace System.Data
{
    public static class DbTypeConsts
    {
        /// <summary>
        /// 自定义一个 DbType 表示 pgsql 的 json 类型
        /// </summary>
        public const DbType NpgJson = (DbType)135;
        /// <summary>
        /// 自定义一个 DbType 表示 pgsql 的 jsonb 类型
        /// </summary>
        public const DbType NpgJsonb = (DbType)136;
    }
}

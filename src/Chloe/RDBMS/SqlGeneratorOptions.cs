using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.RDBMS
{
    public class SqlGeneratorOptions
    {
        public SqlGeneratorOptions()
        {

        }

        public string LeftQuoteChar { get; set; }

        public string RightQuoteChar { get; set; }

        /// <summary>
        /// in 参数最大个数
        /// </summary>
        public int MaxInItems { get; set; } = int.MaxValue;

        /// <summary>
        /// 是否将空字符串视为 null
        /// </summary>
        public bool RegardEmptyStringAsNull { get; set; }
    }
}

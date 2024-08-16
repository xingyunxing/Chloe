using Chloe.RDBMS;

namespace Chloe.Oracle
{
    class OracleSqlGeneratorOptions : SqlGeneratorOptions
    {
        public OracleSqlGeneratorOptions()
        {
            this.RegardEmptyStringAsNull = true;
        }

        /// <summary>
        /// 是否将 sql 中的表名/字段名转成大写。
        /// </summary>
        public bool ConvertToUppercase { get; set; }
    }
}

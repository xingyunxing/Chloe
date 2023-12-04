using Chloe.RDBMS;

namespace Chloe.KingbaseES
{
    class KingbaseESSqlGeneratorOptions : SqlGeneratorOptions
    {
        public KingbaseESSqlGeneratorOptions()
        {

        }

        /// <summary>
        /// 是否将 sql 中的表名/字段名转成小写。
        /// </summary>
        public bool ConvertToLowercase { get; set; }
    }
}

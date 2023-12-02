
namespace Chloe.PostgreSQL
{
    public class PostgreSQLOptions : DbOptions
    {
        /// <summary>
        /// 是否将 sql 中的表名/字段名转成小写。默认为 true。
        /// </summary>
        public bool ConvertToLowercase { get; set; } = true;
    }
}

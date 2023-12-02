
namespace Chloe.Oracle
{
    public class OracleOptions : DbOptions
    {
        public OracleOptions()
        {
            this.MaxInItems = 1000;
        }

        /// <summary>
        /// 是否将 sql 中的表名/字段名转成大写。默认为 true。
        /// </summary>
        public bool ConvertToUppercase { get; set; } = true;
    }
}

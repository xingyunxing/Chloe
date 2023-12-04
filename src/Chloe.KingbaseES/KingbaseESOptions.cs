namespace Chloe.KingbaseES
{
    public class KingbaseESOptions : DbOptions
    {
        public KingbaseESOptions()
        {
        }

        /// <summary>
        /// 是否将 sql 中的表名/字段名转成小写。默认为 true。
        /// </summary>
        public bool ConvertToLowercase { get; set; } = true;
    }
}
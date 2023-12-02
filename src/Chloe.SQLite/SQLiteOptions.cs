
namespace Chloe.SQLite
{
    public class SQLiteOptions : DbOptions
    {
        /// <summary>
        /// 是否开启读写并发安全模式。
        /// </summary>
        public bool ConcurrencyMode { get; set; } = true;
    }
}

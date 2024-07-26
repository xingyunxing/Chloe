
namespace Chloe.SQLite
{
    public class SQLiteOptions : DbOptions
    {
        public SQLiteOptions()
        {
            this.MaxNumberOfParameters = 999;
            this.MaxInItems = 999;
        }

        /// <summary>
        /// 是否开启读写并发安全模式。
        /// </summary>
        public bool ConcurrencyMode { get; set; } = true;
    }
}

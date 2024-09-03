
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

        public SQLiteOptions Clone()
        {
            SQLiteOptions options = new SQLiteOptions()
            {
                DbConnectionFactory = this.DbConnectionFactory,
                InsertStrategy = this.InsertStrategy,
                MaxNumberOfParameters = this.MaxNumberOfParameters,
                MaxInItems = this.MaxInItems,
                DefaultBatchSizeForInsertRange = this.DefaultBatchSizeForInsertRange,
                RegardEmptyStringAsNull = this.RegardEmptyStringAsNull,
                ConcurrencyMode = this.ConcurrencyMode
            };

            return options;
        }
    }
}

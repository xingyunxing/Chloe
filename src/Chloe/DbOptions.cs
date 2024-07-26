using Chloe.Infrastructure;

namespace Chloe
{
    /// <summary>
    /// 插入策略
    /// </summary>
    [Flags]
    public enum InsertStrategy
    {
        Default = 0,
        /// <summary>
        /// null 值属性不参与插入
        /// </summary>
        IgnoreNull = 1,
        /// <summary>
        /// 空值字符串属性不参与插入
        /// </summary>
        IgnoreEmptyString = 2
    }

    public class DbOptions
    {
        public IDbConnectionFactory DbConnectionFactory { get; set; }

        public InsertStrategy InsertStrategy { get; set; } = InsertStrategy.Default;

        /// <summary>
        /// 参数最大个数
        /// </summary>
        public int MaxNumberOfParameters { get; set; } = int.MaxValue;

        /// <summary>
        /// in 条件参数最大个数
        /// </summary>
        public int MaxInItems { get; set; } = int.MaxValue;

        /// <summary>
        /// 执行 InsertRange 插入数据时，默认分批插入的数据条数
        /// </summary>
        public int DefaultInsertCountPerBatchForInsertRange { get; set; } = 10000;
    }
}


namespace Chloe.Sharding
{
    /// <summary>
    /// 分片相关的配置
    /// </summary>
    public class ShardingOptions
    {
        /// <summary>
        /// 每次查询单个数据库最多可打开的连接数。
        /// 注：如果类似 SQLite 不支持多线程读写的数据库，一定要将此属性值设置为 1。
        /// </summary>
        public int MaxConnectionsPerDataSource { get; set; } = 12;
    }
}

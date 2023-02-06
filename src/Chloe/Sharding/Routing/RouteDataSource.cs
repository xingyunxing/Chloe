namespace Chloe.Sharding.Routing
{
    /// <summary>
    /// 表示一个数据源(数据库)
    /// </summary>
    public class RouteDataSource
    {
        /// <summary>
        /// 数据源(数据库)唯一标识（不要求必须是库名，只要是唯一标识就行），必填，不能为空。
        /// </summary>
        public string Name { get; set; }

        public IDbContextProviderFactory DbContextProviderFactory { get; set; }
    }
}

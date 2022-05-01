namespace Chloe.Sharding.Routing
{
    public class RouteDataSource
    {
        /// <summary>
        /// 数据源(数据库)名，必填，不能为空。
        /// </summary>
        public string Name { get; set; }
        public IDbContextProviderFactory DbContextProviderFactory { get; set; }
    }
}

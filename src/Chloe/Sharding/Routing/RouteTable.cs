namespace Chloe.Sharding.Routing
{
    public class RouteTable
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }
        public string Schema { get; set; }
        /// <summary>
        /// 数据源(数据库)
        /// </summary>
        public RouteDataSource DataSource { get; set; }
    }
}

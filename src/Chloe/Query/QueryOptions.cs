
namespace Chloe.Query
{
    public class QueryOptions
    {
        public QueryOptions()
        {

        }

        public bool IsTracking { get; set; }

        public bool IgnoreFilters { get; set; }

        /// <summary>
        /// 导航查询时，子对象将父对象设置到子对象对应的属性上
        /// </summary>
        public bool BindTwoWay { get; set; }
    }
}

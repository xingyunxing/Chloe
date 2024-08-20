using Chloe.DbExpressions;
using Chloe.Query.Mapping;

namespace Chloe.Query
{
    class MappingData
    {
        public MappingData()
        {
        }
        public QueryContext Context { get; set; }
        public IObjectActivatorCreator ObjectActivatorCreator { get; set; }
        public DbSqlQueryExpression SqlQuery { get; set; }
        public bool IsTrackingQuery { get; set; }
        /// <summary>
        /// 表示当前解析结果是否可以缓存
        /// </summary>
        public bool CanBeCachced { get; set; }
    }
}

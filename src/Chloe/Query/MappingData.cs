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
    }
}

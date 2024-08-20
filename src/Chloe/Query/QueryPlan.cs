using Chloe.DbExpressions;
using Chloe.Mapper;
using Chloe.QueryExpressions;

namespace Chloe.Query
{
    public static class QueryPlanContainer
    {
        static readonly Dictionary<object, QueryPlan> Cache = new Dictionary<object, QueryPlan>();

        public static QueryPlan GetOrAdd(QueryExpression query, Func<QueryPlan> planFactory)
        {
            PublicHelper.CheckNull(query, nameof(query));
            object key = new QueryPlanCacheKey(query);
            return GetOrAdd(key, planFactory);
        }

        static QueryPlan GetOrAdd(object key, Func<QueryPlan> planFactory)
        {
            PublicHelper.CheckNull(key, nameof(key));

            QueryPlan queryPlan;
            if (!Cache.TryGetValue(key, out queryPlan))
            {
#if DEBUG
                Console.WriteLine("no query plan cache match ...");
#endif
                QueryPlan plan = planFactory();

                if (!plan.CanBeCachced)
                {
#if DEBUG
                    Console.WriteLine("query plan cannot be cahced ...");
#endif
                    return plan;
                }

                lock (Cache)
                {
                    if (!Cache.TryGetValue(key, out queryPlan))
                    {
                        queryPlan = plan;
                        Cache.Add(key, queryPlan);
                    }
                }
            }
#if DEBUG
            else
            {
                Console.WriteLine("match query plan cache...");
            }
#endif

            return queryPlan;
        }

    }

    public class QueryPlan
    {
        public QueryExpression KeyStub { get; set; }
        public IObjectActivator ObjectActivator { get; set; }
        public DbSqlQueryExpression SqlQuery { get; set; }

        /// <summary>
        /// 表示当前查询计划是否可以缓存
        /// </summary>
        public bool CanBeCachced { get; set; }
    }

    public struct QueryPlanCacheKey : IEquatable<QueryPlanCacheKey>
    {
        QueryExpression _query;

        public QueryPlanCacheKey(QueryExpression query)
        {
            this._query = query;
        }

        public override bool Equals(object? obj)
        {
            return obj is QueryPlanCacheKey other && Equals(other);
        }

        public bool Equals(QueryPlanCacheKey other)
        {
            return QueryExpressionEqualityComparer.Instance.Equals(this._query, other._query);
        }

        public override int GetHashCode()
        {
            return QueryExpressionEqualityComparer.Instance.GetHashCode(this._query);
        }

    }
}

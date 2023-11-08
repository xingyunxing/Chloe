using Chloe;
using Chloe.MySql;
using Chloe.Sharding;
using Chloe.Sharding.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo.Sharding
{
    public class OrderRouteTable : RouteTable
    {
        public OrderRouteTable(int month)
        {
            this.Month = month;

            //根据月份构造表名
            this.Name = ShardingTest.BuildTableName(month);
        }

        public int Month { get; set; }
    }

    public class OrderRouteDataSource : RouteDataSource
    {
        public OrderRouteDataSource(int year)
        {
            this.Year = year;

            //设置数据源唯一标识
            this.Name = year.ToString();
        }

        public int Year { get; set; }
    }

    /*
     关于路由的一些说明：
       1. Chloe 不负责创建数据库或数据表，你需要根据自己的分片情况维护所有分表集合。
          框架不知道有哪些分表，因此你要实现 IShardingRoute.GetTables() 方法返回所有的分表信息（分表信息包括表名，表所在的库信息【IDbContextProviderFactory 对象】）。

       2. 框架将路由策略做了抽象，所以路由到哪些表需要自己实现(按时序，取模还是其他算法自行实现)，目前支持（==，<=,>=,!=,<,>）等操作符路由。
          比如假设根据 CreateTime 字段分表，如果类似 query.Where(a => a.CreateTime == 2020-10-10 ) 查询，要定位到哪些表呢？框架是不知道的，需要你告诉框架！
          这时你要从你维护的所有分表集合中筛选出符合条件的分表来，所以你需要实现 IShardingRoute.GetStrategy(MemberInfo member) 接口，根据分片字段获取路由策略接口 IRoutingStrategy。
          IRoutingStrategy 负责通过分片值【即 2020-10-10】筛选符合条件的分表，目前支持（==，<=,>=,!=,<,>）等操作符筛选。

       3. 假设根据时间分表（CreateTime），查询时又根据 CreateTime 排序（如 query.OrderBy(a => a.CreateTime)）,因为数据储存是按时间段有序存储的，所以针对这种情况下的查询可以优化一下避免无谓的查询，
          因此你需要实现 IShardingRoute.SortTables() 方法，对定位到的表进行一次重排，以提升查询效率（可以看看这篇文章，说得很好 https://www.cnblogs.com/xuejiaming/p/15237878.html） 。
          ps：如果不需要重排，IShardingRoute.SortTables() 实现里直接返回传入的参数即可。

       4. 只要实现了 IShardingRoute 接口，把 IShardingRoute 对象注册进框架，然后就可以像常规使用方式愉快的做增删查改了。
     */

    /// <summary>
    /// Order 表路由。有关路由的一些说明请参考：https://github.com/shuxinqin/Chloe/issues/330
    /// </summary>
    public class OrderShardingRoute : IShardingRoute
    {
        ShardingTest _shardingTest;
        Dictionary<string, IRoutingStrategy> _routingStrategies = new Dictionary<string, IRoutingStrategy>();

        public OrderShardingRoute(ShardingTest shardingTest, List<int> years)
        {
            this._shardingTest = shardingTest;

            //添加路由规则(支持一个或多个字段联合分片)。ps：分片字段的路由规则必须要添加以外，也可以添加非分片字段路由规则作为辅助，以便缩小表范围，提高查询效率。

            //CreateTime 是分片字段，分片字段的路由规则必须要添加
            this._routingStrategies.Add(nameof(Order.CreateTime), new OrderCreateTimeRoutingStrategy(this));

            //非分片字段路由规则，根据实际情况可选择性添加
            this._routingStrategies.Add(nameof(Order.CreateDate), new OrderCreateDateRoutingStrategy(this));
            this._routingStrategies.Add(nameof(Order.CreateYear), new OrderCreateYearRoutingStrategy(this));
            this._routingStrategies.Add(nameof(Order.CreateMonth), new OrderCreateMonthRoutingStrategy(this));

            //所有分表
            this.AllTables = years.SelectMany(a => GetTablesByYear(a)).Reverse().ToList();

            //this.AllTables = GetRouteTablesByYear(2020).ToList();
            //this.AllTables = GetRouteTablesByYear(2020).Concat(GetRouteTablesByYear(2021)).Take(23).Reverse().ToList();
            //this.AllTables = GetTablesByYear(2020).Concat(GetTablesByYear(2021)).Reverse().ToList();
            //this.AllTables = this.AllTables.Take(2).ToList();
        }

        /// <summary>
        /// 所有分片表
        /// </summary>
        List<RouteTable> AllTables { get; set; }

        /// <summary>
        /// 创建所有分表对象，并设置数据源
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        IEnumerable<RouteTable> GetTablesByYear(int year)
        {
            /*
             * 根据年分库
             * 根据月份分表
             */

            for (int month = 1; month <= 12; month++)
            {
                var dbContextProviderFactory = new OrderDbContextProviderFactory(this._shardingTest, year);  //根据年份创建连接数据库对象
                RouteTable table = new OrderRouteTable(month)
                {
                    /* 设置 RouteTable 所在的数据库 */
                    DataSource = new OrderRouteDataSource(year)
                    {
                        DbContextProviderFactory = dbContextProviderFactory
                    }
                };

                yield return table;
            }
        }

        static IEnumerable<DateTime> GetDates(int year)
        {
            DateTime date = new DateTime(year, 1, 1).AddDays(-1);
            DateTime lastDate = new DateTime(year, 12, 31);

            while (true)
            {
                date = date.AddDays(1);
                if (date > lastDate)
                    break;

                yield return date;
            }
        }

        /// <summary>
        /// 获取所有的分片表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RouteTable> GetTables()
        {
            return this.AllTables;
        }

        /// <summary>
        /// 根据实体属性获取相应的路由规则，如果传入的 member 没有路由规则，返回 null 即可。
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public IRoutingStrategy GetStrategy(MemberInfo member)
        {
            this._routingStrategies.TryGetValue(member.Name, out var routingStrategy);
            return routingStrategy;
        }

        /// <summary>
        /// 根据排序字段对路由表重排。
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="orderings"></param>
        /// <returns></returns>
        public SortResult SortTables(List<RouteTable> tables, List<Ordering> orderings)
        {
            var firstOrdering = orderings.FirstOrDefault();

            MemberInfo member = null;
            if (firstOrdering.KeySelector.Body is System.Linq.Expressions.MemberExpression memberExp)
            {
                member = memberExp.Member;
            }

            if (member != null)
            {
                if (member == typeof(Order).GetProperty(nameof(Order.CreateTime)))
                {
                    //因为按照日期分表，在根据日期排序的情况下，我们对路由到的表进行一个排序，对分页有查询优化。
                    if (firstOrdering.Ascending)
                    {
                        return new SortResult() { IsOrdered = true, Tables = tables.OrderBy(a => (a.DataSource as OrderRouteDataSource).Year).ThenBy(a => (a as OrderRouteTable).Month).ToList() };
                    }

                    return new SortResult() { IsOrdered = true, Tables = tables.OrderByDescending(a => (a.DataSource as OrderRouteDataSource).Year).ThenByDescending(a => (a as OrderRouteTable).Month).ToList() };
                }
            }

            return new SortResult() { IsOrdered = false, Tables = tables };
        }
    }

    /// <summary>
    /// CreateTime 字段路由规则
    /// </summary>
    public class OrderCreateTimeRoutingStrategy : RoutingStrategy<DateTime>
    {
        public OrderCreateTimeRoutingStrategy(OrderShardingRoute route) : base(route)
        {

        }

        /// <summary>
        /// 当查询如 query.Where(a => a.CreateTime == createTime) 时，从所有分表中筛选出满足条件的分表
        /// </summary>
        /// <param name="createTime"></param>
        /// <returns></returns>
        public override IEnumerable<RouteTable> ForEqual(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month == createTime.Month);
        }

        /// <summary>
        /// 当查询如 query.Where(a => a.CreateTime != createTime) 时，从所有分表中筛选出满足条件的分表
        /// </summary>
        /// <param name="createTime"></param>
        /// <returns></returns>
        public override IEnumerable<RouteTable> ForNotEqual(DateTime createTime)
        {
            return this.Route.GetTables();
        }

        /// <summary>
        /// 当查询如 query.Where(a => a.CreateTime > createTime) 时，从所有分表中筛选出满足条件的分表
        /// </summary>
        /// <param name="createTime"></param>
        /// <returns></returns>
        public override IEnumerable<RouteTable> ForGreaterThan(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year > createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month >= createTime.Month));
        }

        /// <summary>
        /// 当查询如 query.Where(a => a.CreateTime >= createTime) 时，从所有分表中筛选出满足条件的分表
        /// </summary>
        /// <param name="createTime"></param>
        /// <returns></returns>
        public override IEnumerable<RouteTable> ForGreaterThanOrEqual(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year > createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month >= createTime.Month));
        }

        /// <summary>
        /// 当查询如 query.Where(a => a.CreateTime &lt; createTime) 时，从所有分表中筛选出满足条件的分表
        /// </summary>
        /// <param name="createTime"></param>
        /// <returns></returns>
        public override IEnumerable<RouteTable> ForLessThan(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year < createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month <= createTime.Month));
        }

        /// <summary>
        /// 当查询如 query.Where(a => a.CreateTime &lt;= createTime) 时，从所有分表中筛选出满足条件的分表
        /// </summary>
        /// <param name="createTime"></param>
        /// <returns></returns>
        public override IEnumerable<RouteTable> ForLessThanOrEqual(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year < createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month <= createTime.Month));
        }
    }
    /// <summary>
    /// CreateDate 字段路由规则
    /// </summary>
    public class OrderCreateDateRoutingStrategy : RoutingStrategy<int>
    {
        public OrderCreateDateRoutingStrategy(OrderShardingRoute route) : base(route)
        {

        }

        int ParseCreateMonth(int createDate)
        {
            int month = int.Parse(createDate.ToString().Substring(4, 2));
            return month;
        }
        int GetCreateYear(int createDate)
        {
            int year = int.Parse(createDate.ToString().Substring(0, 4));
            return year;
        }

        public override IEnumerable<RouteTable> ForEqual(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year == this.GetCreateYear(createDate) && (a as OrderRouteTable).Month == this.ParseCreateMonth(createDate));
        }

        public override IEnumerable<RouteTable> ForNotEqual(int createDate)
        {
            return base.ForNotEqual(createDate);
        }

        public override IEnumerable<RouteTable> ForGreaterThan(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year >= this.GetCreateYear(createDate) && (a as OrderRouteTable).Month >= this.ParseCreateMonth(createDate));
        }

        public override IEnumerable<RouteTable> ForGreaterThanOrEqual(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year >= this.GetCreateYear(createDate) && (a as OrderRouteTable).Month >= this.ParseCreateMonth(createDate));
        }

        public override IEnumerable<RouteTable> ForLessThan(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year <= this.GetCreateYear(createDate) && (a as OrderRouteTable).Month <= this.ParseCreateMonth(createDate));
        }

        public override IEnumerable<RouteTable> ForLessThanOrEqual(int createDate)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year <= this.GetCreateYear(createDate) && (a as OrderRouteTable).Month <= this.ParseCreateMonth(createDate));
        }
    }
    /// <summary>
    /// CreateYear 字段路由规则
    /// </summary>
    public class OrderCreateYearRoutingStrategy : RoutingStrategy<int>
    {
        public OrderCreateYearRoutingStrategy(OrderShardingRoute route) : base(route)
        {

        }

        public override IEnumerable<RouteTable> ForEqual(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year == createYear);
        }

        public override IEnumerable<RouteTable> ForNotEqual(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year != createYear);
        }

        public override IEnumerable<RouteTable> ForGreaterThan(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year > createYear);
        }

        public override IEnumerable<RouteTable> ForGreaterThanOrEqual(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year >= createYear);
        }

        public override IEnumerable<RouteTable> ForLessThan(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year < createYear);
        }

        public override IEnumerable<RouteTable> ForLessThanOrEqual(int createYear)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year <= createYear);
        }
    }
    /// <summary>
    /// CreateMonth 字段路由规则
    /// </summary>
    public class OrderCreateMonthRoutingStrategy : RoutingStrategy<int>
    {
        public OrderCreateMonthRoutingStrategy(OrderShardingRoute route) : base(route)
        {

        }

        public override IEnumerable<RouteTable> ForEqual(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month == createMonth);
        }

        public override IEnumerable<RouteTable> ForNotEqual(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month != createMonth);
        }

        public override IEnumerable<RouteTable> ForGreaterThan(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month > createMonth);
        }

        public override IEnumerable<RouteTable> ForGreaterThanOrEqual(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month >= createMonth);
        }

        public override IEnumerable<RouteTable> ForLessThan(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month < createMonth);
        }

        public override IEnumerable<RouteTable> ForLessThanOrEqual(int createMonth)
        {
            return this.Route.GetTables().Where(a => (a as OrderRouteTable).Month <= createMonth);
        }
    }
}

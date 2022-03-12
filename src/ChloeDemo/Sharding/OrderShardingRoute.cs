using Chloe;
using Chloe.MySql;
using Chloe.Sharding;
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
            this.Name = ShardingTest.BuildTableName(month);
        }

        public int Month { get; set; }
    }

    public class OrderRouteDataSource : RouteDataSource
    {
        public OrderRouteDataSource(int year)
        {
            this.Year = year;
            this.Name = year.ToString();
        }

        public int Year { get; set; }
    }

    public class OrderShardingRoute : IShardingRoute
    {
        Dictionary<string, IRoutingStrategy> _routingStrategies = new Dictionary<string, IRoutingStrategy>();

        public OrderShardingRoute(List<int> years)
        {
            //多字段路由
            this._routingStrategies.Add(nameof(Order.CreateTime), new OrderCreateTimeRoutingStrategy(this));
            this._routingStrategies.Add(nameof(Order.CreateDate), new OrderCreateDateRoutingStrategy(this));
            this._routingStrategies.Add(nameof(Order.CreateYear), new OrderCreateYearRoutingStrategy(this));
            this._routingStrategies.Add(nameof(Order.CreateMonth), new OrderCreateMonthRoutingStrategy(this));

            this.AllTables = years.SelectMany(a => GetTablesByYear(a)).Reverse().ToList();

            //this.AllTables = GetRouteTablesByYear(2020).ToList();
            //this.AllTables = GetRouteTablesByYear(2020).Concat(GetRouteTablesByYear(2021)).Take(23).Reverse().ToList();
            //this.AllTables = GetTablesByYear(2020).Concat(GetTablesByYear(2021)).Reverse().ToList();
            //this.AllTables = this.AllTables.Take(2).ToList();
        }
        List<RouteTable> AllTables { get; set; }

        IEnumerable<RouteTable> GetTablesByYear(int year)
        {
            for (int month = 1; month <= 12; month++)
            {
                var dbContextFactory = new OrderRouteDbContextFactory(year);
                RouteTable table = new OrderRouteTable(month) { DataSource = new OrderRouteDataSource(year) { DbContextFactory = dbContextFactory } };

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

        public IEnumerable<RouteTable> GetTables()
        {
            return this.AllTables;
        }

        public IRoutingStrategy GetStrategy(MemberInfo member)
        {
            this._routingStrategies.TryGetValue(member.Name, out var routingStrategy);
            return routingStrategy;
        }

        public SortResult SortTables(List<RouteTable> tables, List<Ordering> orderings)
        {
            var firstOrdering = orderings.FirstOrDefault();

            if (firstOrdering.Member != typeof(Order).GetProperty(nameof(Order.CreateTime)))
            {
                return new SortResult() { IsOrdered = false, Tables = tables };
            }

            if (firstOrdering.Ascending)
            {
                return new SortResult() { IsOrdered = true, Tables = tables.OrderBy(a => (a.DataSource as OrderRouteDataSource).Year).ThenBy(a => (a as OrderRouteTable).Month).ToList() };
            }

            return new SortResult() { IsOrdered = true, Tables = tables.OrderByDescending(a => (a.DataSource as OrderRouteDataSource).Year).ThenByDescending(a => (a as OrderRouteTable).Month).ToList() };
        }
    }

    public class OrderCreateTimeRoutingStrategy : RoutingStrategy<DateTime>
    {
        public OrderCreateTimeRoutingStrategy(OrderShardingRoute route) : base(route)
        {

        }

        public override IEnumerable<RouteTable> ForEqual(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month == createTime.Month);
        }

        public override IEnumerable<RouteTable> ForNotEqual(DateTime createTime)
        {
            return base.ForNotEqual(createTime);
        }

        public override IEnumerable<RouteTable> ForGreaterThan(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year > createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month >= createTime.Month));
        }

        public override IEnumerable<RouteTable> ForGreaterThanOrEqual(DateTime createTime)
        {
            return this.ForGreaterThan(createTime);
        }

        public override IEnumerable<RouteTable> ForLessThan(DateTime createTime)
        {
            return this.Route.GetTables().Where(a => (a.DataSource as OrderRouteDataSource).Year < createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month <= createTime.Month));
        }

        public override IEnumerable<RouteTable> ForLessThanOrEqual(DateTime createTime)
        {
            return this.ForLessThan(createTime);
        }
    }
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

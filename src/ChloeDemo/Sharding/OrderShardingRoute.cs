using Chloe;
using Chloe.MySql;
using Chloe.Sharding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo.Sharding
{
    public class OrderRouteDbContextFactory : IRouteDbContextFactory
    {
        int _year;
        public OrderRouteDbContextFactory(int year)
        {
            this._year = year;
        }

        public IDbContext CreateDbContext()
        {
            string connString = $"Server=localhost;Port=3306;Database=order{this._year};Uid=root;Password=sasa;Charset=utf8; Pooling=True; Max Pool Size=200;Allow User Variables=True;SslMode=none;";

            MySqlContext dbContext = new MySqlContext(new MySqlConnectionFactory(connString));
            return dbContext;
        }
    }

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
        public OrderShardingRoute()
        {
            //this.AllTables = GetRouteTablesByYear(2020).ToList();
            //this.AllTables = GetRouteTablesByYear(2020).Concat(GetRouteTablesByYear(2021)).Take(23).Reverse().ToList();
            this.AllTables = GetTablesByYear(2020).Concat(GetTablesByYear(2021)).Reverse().ToList();
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

        public RouteTable GetTable(ShardingDbContext shardingDbContext, object shardingValue)
        {
            DateTime createTime = (DateTime)shardingValue;

            return this.AllTables.Where(a => (a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month == createTime.Month).FirstOrDefault();
        }

        public List<RouteTable> GetTablesByKey(ShardingDbContext shardingDbContext, object keyValue)
        {
            return this.AllTables;
        }

        public List<RouteTable> GetTables(ShardingDbContext shardingDbContext)
        {
            return this.AllTables.ToList();
        }

        public List<RouteTable> GetTables(ShardingDbContext shardingDbContext, object shardingValue, ShardingOperator shardingOperator)
        {
            DateTime createTime = (DateTime)shardingValue;

            if (shardingOperator == ShardingOperator.Equal)
            {
                return this.AllTables.Where(a => (a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month == createTime.Month).ToList();
            }

            if (shardingOperator == ShardingOperator.NotEqual)
            {
                return this.AllTables.ToList();
            }

            if (shardingOperator == ShardingOperator.GreaterThan || shardingOperator == ShardingOperator.GreaterThanOrEqual)
            {
                return this.AllTables.Where(a => (a.DataSource as OrderRouteDataSource).Year > createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month >= createTime.Month)).ToList();
            }

            if (shardingOperator == ShardingOperator.LessThan || shardingOperator == ShardingOperator.LessThanOrEqual)
            {
                return this.AllTables.Where(a => (a.DataSource as OrderRouteDataSource).Year < createTime.Year || ((a.DataSource as OrderRouteDataSource).Year == createTime.Year && (a as OrderRouteTable).Month <= createTime.Month)).ToList();
            }

            throw new NotImplementedException();
        }

        public SortResult SortTables(ShardingDbContext shardingDbContext, List<RouteTable> tables, List<Ordering> orderings)
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
}

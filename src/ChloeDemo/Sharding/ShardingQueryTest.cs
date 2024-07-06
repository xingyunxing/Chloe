using Chloe;
using Chloe.MySql;
using Chloe.Sharding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo.Sharding
{
    internal class ShardingQueryTest
    {
        ShardingTest _shardingTest;
        public ShardingQueryTest(ShardingTest shardingTest)
        {
            this._shardingTest = shardingTest;
        }

        public async Task Run()
        {
            ShardingConfigBuilder<Order> shardingConfigBuilder = new ShardingConfigBuilder<Order>();
            shardingConfigBuilder.HasShardingKey(a => a.CreateTime);  //配置分片字段
            shardingConfigBuilder.HasRoute(new OrderShardingRoute(this._shardingTest, new List<int>() { 2018, 2019 }));  //设置分片路由

            ShardingConfigContainer.Add(shardingConfigBuilder.Build());  //注册分片配置信息

            await this.NormalQueryTest();
            await this.PageQueryByShardingKeyOrderByAscTest();
            await this.PageQueryByShardingKeyOrderByDescTest();
            await this.PageQueryInSingleTableTest();
            await this.PageQueryByShardingKeyInSingleDatabaseOrderByDescTest();
            await this.QueryOrderByShardingKeyTest();
            await this.PageQueryOrderByNonShardingKeyTest();
            await this.QueryByPrimaryKeyTest();
            await this.QueryByPrimaryKeyAndShardingKeyTest();
            await this.RouteByNonShardingKeyTest();
            await this.RouteBySomeCSharpMethodTest();
            await this.ProjectionTest();
            await this.AnyQueryTest();
            await this.CountQueryTest();
            await this.SumQueryTest();
            await this.SumNullQueryTest();
            await this.AvgQueryTest();
            await this.AvgNullQueryTest();
            await this.MaxMinQueryTest();
            await this.GroupQueryTest();
            await this.ExcludeFieldQueryTest();

            Console.WriteLine("query test over...");
            Console.ReadKey();
        }

        IDbContext CreateDbContext()
        {
            /*
             * 创建一个普通的 DbContext。
             * 注：实际应用中应该使用 MsSqlContext、MySqlContext、OracleContext 等上下文类，但因为这只是演示 sharding 功能，操作的全是分表，
             * 由于最终创建分表数据库连接以及分表操作（增删查改）调用的是 OrderShardingRoute.cs 里 AllTables 集合中的 RouteTable 对象的 RouteTable.DataSource.DbContextProviderFactory 这个工厂创建出来的 IDbContextProvider 对象，
             * 所以只使用普通的 DbContext（如果有非分表，请使用 MsSqlContext、MySqlContext、OracleContext 等上下文类）
             */
            DbContext dbContext = new DbContext();
            dbContext.ShardingOptions.MaxConnectionsPerDataSource = 6;
            return dbContext;
        }

        async Task NormalQueryTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            var orders = await q.ToListAsync();
            Debug.Assert(orders.Count == 1460);


            orders = await q.Where(a => a.CreateMonth == 1).ToListAsync();
            Debug.Assert(orders.Count == 31 * 2 * 2);

            orders = await q.Where(a => a.CreateMonth == 1).Take(63).ToListAsync();
            Debug.Assert(orders.Count == 63);
            Debug.Assert(orders.First().CreateTime == DateTime.Parse("2019-01-01 10:00"));
            Debug.Assert(orders.Last().CreateTime == DateTime.Parse("2018-01-01 10:00"));


            orders = await q.Take(100).ToListAsync();
            Debug.Assert(orders.Count == 100);


            orders = await q.OrderByDesc(a => a.CreateTime).Take(63).ToListAsync();
            Debug.Assert(orders.Count == 63);
            Debug.Assert(orders.First().CreateTime == DateTime.Parse("2019-12-31 12:00"));
            Debug.Assert(orders.Last().CreateTime == DateTime.Parse("2019-11-30 12:00"));


            orders = await q.OrderBy(a => a.CreateTime).Take(63).ToListAsync();
            Debug.Assert(orders.Count == 63);
            Debug.Assert(orders.First().CreateTime == DateTime.Parse("2018-01-01 10:00"));
            Debug.Assert(orders.Last().CreateTime == DateTime.Parse("2018-02-01 10:00"));


            orders = await q.Where(a => a.CreateMonth == 1).OrderByDesc(a => a.CreateTime).Take(63).ToListAsync();
            Debug.Assert(orders.Count == 63);
            Debug.Assert(orders.First().CreateTime == DateTime.Parse("2019-01-31 12:00"));
            Debug.Assert(orders[1].CreateTime == DateTime.Parse("2019-01-31 10:00"));
            Debug.Assert(orders.Last().CreateTime == DateTime.Parse("2018-01-31 12:00"));

            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// 根据分片字段升序分页查询
        /// </summary>
        /// <returns></returns>
        async Task PageQueryByShardingKeyOrderByAscTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();
            q = q.OrderBy(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            Helpers.PrintResult(result);

            var dataList = result.DataList;

            Debug.Assert(result.Totals == 1460);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2018-01-01 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2018-01-01 12:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2018-01-10 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2018-01-10 12:00"));

            Helpers.PrintSplitLine();

            /*
             * 取第二页
             */
            result = await q.PagingAsync(2, 20);
            Helpers.PrintResult(result);

            dataList = result.DataList;

            Debug.Assert(result.Totals == 1460);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2018-01-11 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2018-01-11 12:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2018-01-20 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2018-01-20 12:00"));

            Helpers.PrintSplitLine();
        }
        /// <summary>
        /// 根据分片字段降序分页查询
        /// </summary>
        /// <returns></returns>
        async Task PageQueryByShardingKeyOrderByDescTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();
            q = q.OrderByDesc(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            Helpers.PrintResult(result);

            var dataList = result.DataList;

            Debug.Assert(result.Totals == 1460);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2019-12-31 12:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2019-12-31 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2019-12-22 12:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2019-12-22 10:00"));

            Helpers.PrintSplitLine();

            /*
             * 取第二页
             */
            result = await q.PagingAsync(2, 20);
            dataList = result.DataList;
            Helpers.PrintResult(result);

            Debug.Assert(result.Totals == 1460);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2019-12-21 12:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2019-12-21 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2019-12-12 12:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2019-12-12 10:00"));

            Helpers.PrintSplitLine();
        }
        /// <summary>
        /// 在单表内分页查询，不会重写 sql
        /// </summary>
        /// <returns></returns>
        async Task PageQueryInSingleTableTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            int pageSize = 10;

            var orders = await q.Where(a => a.CreateYear == 2018 && a.CreateMonth == 1).OrderBy(a => a.CreateDate).TakePage(2, pageSize).ToListAsync();

            Debug.Assert(orders.Count == pageSize);
            Debug.Assert(orders[0].CreateYear == 2018 && orders[0].CreateMonth == 1);
            Debug.Assert(orders[0].CreateTime == DateTime.Parse("2018-01-06 10:00"));

            var result = await q.Where(a => a.CreateYear == 2018 && a.CreateMonth == 1).OrderBy(a => a.CreateDate).PagingAsync(2, pageSize);

            orders = result.DataList;
            Helpers.PrintResult(result);

            Debug.Assert(result.Totals == 31 * 2);
            Debug.Assert(orders.Count == pageSize);
            Debug.Assert(orders[0].CreateYear == 2018 && orders[0].CreateMonth == 1);
            Debug.Assert(orders[0].CreateTime == DateTime.Parse("2018-01-06 10:00"));

            Helpers.PrintSplitLine();
        }
        /// <summary>
        /// 在单库内分页查询
        /// </summary>
        /// <returns></returns>
        async Task PageQueryByShardingKeyInSingleDatabaseOrderByDescTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            DateTime dt = new DateTime(2019, 12, 2);
            q = q.Where(a => a.CreateTime < dt);

            q = q.OrderByDesc(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            var dataList = result.DataList;
            Helpers.PrintResult(result);

            Debug.Assert(result.Totals == 1400);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2019-12-01 12:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2019-12-01 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2019-11-22 12:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2019-11-22 10:00"));

            Helpers.PrintSplitLine();
        }
        /// <summary>
        /// 根据分片字段排序查询
        /// </summary>
        /// <returns></returns>
        async Task QueryOrderByShardingKeyTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            DateTime dt = new DateTime(2018, 12, 31);
            q = q.Where(a => a.CreateTime >= dt);

            q = q.OrderBy(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            var dataList = result.DataList;
            Helpers.PrintResult(result);

            Debug.Assert(result.Totals == 732);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2018-12-31 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2018-12-31 12:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2019-01-09 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2019-01-09 12:00"));

            Helpers.PrintSplitLine();
        }
        /// <summary>
        /// 根据非分片字段排序分页
        /// </summary>
        /// <returns></returns>
        async Task PageQueryOrderByNonShardingKeyTest()
        {
            PagingResult<Order> result;
            List<Order> dataList;

            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            q = q.OrderBy(a => a.Amount).ThenBy(a => a.Id);

            result = await q.PagingAsync(1, 20);
            dataList = result.DataList;
            Helpers.PrintResult(result);

            Debug.Assert(result.Totals == 1460);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2018-01-01 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2018-01-02 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2018-01-19 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2018-01-20 10:00"));

            Helpers.PrintSplitLine();

            /*
             * 取第二页
             */
            result = await q.PagingAsync(2, 20);
            Helpers.PrintResult(result);

            dataList = result.DataList;

            Debug.Assert(result.Totals == 1460);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2018-01-21 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2018-01-22 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2018-02-08 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2018-02-09 10:00"));

            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <returns></returns>
        async Task QueryByPrimaryKeyTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            string id = "2019-12-01 10:00";

            Order entity = null;

            entity = await q.Where(a => a.Id == id).FirstOrDefaultAsync();

            Debug.Assert(entity.Id == id);

            Helpers.PrintSplitLine();
        }
        /// <summary>
        /// 根据主键和分片字段查询
        /// </summary>
        /// <returns></returns>
        async Task QueryByPrimaryKeyAndShardingKeyTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            /*
             * 主键 + 分片字段查询，会精确路由到所在的表
             */

            string id = "2019-12-01 10:00";
            DateTime createTime = DateTime.Parse(id);

            Order entity = await q.Where(a => a.Id == id && a.CreateTime == createTime).FirstOrDefaultAsync();

            Debug.Assert(entity.Id == id);

            Helpers.PrintSplitLine();
        }
        /// <summary>
        /// 根据非分片字段路由
        /// </summary>
        /// <returns></returns>
        async Task RouteByNonShardingKeyTest()
        {
            /*
             * 根据非分片字段路由
             */

            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            //根据 CreateYear 查询，虽然 CreateYear 不是分片字段，但是可以给 CreateYear 字段设置路由规则
            q = q.Where(a => a.CreateYear == 2018);
            q = q.Where(a => a.CreateMonth == 6);

            q = q.OrderBy(a => a.CreateTime);

            var result = await q.ToListAsync();
            Debug.Assert(result.Count == 60);
            Debug.Assert(result.First().CreateYear == 2018);
            Debug.Assert(result.First().CreateMonth == 6);

            Debug.Assert(result.Last().CreateYear == 2018);
            Debug.Assert(result.Last().CreateMonth == 6);

            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// In, Contains, Equals, Sql.IsEqual 等方法路由
        /// </summary>
        /// <returns></returns>
        async Task RouteBySomeCSharpMethodTest()
        {
            List<Order> orders = new List<Order>();

            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            List<int> createDates = new List<int>() { 20180101, 20190201 };

            orders = await q.Where(a => createDates.Contains(a.CreateDate)).ToListAsync();
            Debug.Assert(orders.Count == 4);


            IEnumerable<int> source = createDates;
            orders = await q.Where(a => source.Contains(a.CreateDate)).ToListAsync();

            Debug.Assert(orders.Count == 4);


            orders = await q.Where(a => a.CreateDate.Equals(20180101)).ToListAsync();
            Debug.Assert(orders.Count == 2);

            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// Select 查询
        /// </summary>
        /// <returns></returns>
        async Task ProjectionTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            var results = await q.Where(a => a.CreateMonth == 4 || a.CreateMonth == 6).OrderBy(a => a.CreateMonth).Select(a => new { Id = a.Id, CreateMonth = a.CreateMonth, Order = a }).ToListAsync();

            Debug.Assert(results.Count == 2 * 30 * 4); //一天两条数据，一个月60条，总共4个月
            Debug.Assert(results[0].Id == results[0].Order.Id);

            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// Any 查询
        /// </summary>
        /// <returns></returns>
        async Task AnyQueryTest()
        {
            List<Order> orders = new List<Order>();

            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            bool hasData = false;

            hasData = await q.Where(a => a.CreateDate == 20180101).AnyAsync();

            Debug.Assert(hasData == true);

            hasData = await q.Where(a => a.UserId == "chloe").AnyAsync();

            Debug.Assert(hasData == true);

            hasData = await q.Where(a => a.UserId == "none").AnyAsync();

            Debug.Assert(hasData == false);

            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// Count 查询
        /// </summary>
        /// <returns></returns>
        async Task CountQueryTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            long count = await q.LongCountAsync();

            Debug.Assert(count == 1460);

            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// Sum 查询
        /// </summary>
        /// <returns></returns>
        async Task SumQueryTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            decimal? sum = 0;

            sum = await q.SumAsync(a => a.Amount);

            int count = await q.CountAsync();

            //每天有 2 条数据，一条 Amount=10，一条 Amount=20
            int s = (count / 2) * (10 + 20);

            Debug.Assert(sum == s);


            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// 当 Sum 结果为 null 时查询
        /// </summary>
        /// <returns></returns>
        async Task SumNullQueryTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            decimal? sum = 0;

            sum = await q.Where(a => a.Id == null).SumAsync(a => a.Amount);

            Debug.Assert(sum == null || sum == 0);


            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// 平均值查询
        /// </summary>
        /// <returns></returns>
        async Task AvgQueryTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            decimal? avg = 0;

            var avg1 = await q.AverageAsync(a => a.CreateMonth);

            avg = await q.AverageAsync(a => a.Amount);

            //每天有 2 条数据，一条 Amount=10，一条 Amount=20
            decimal s = (10 + 20) / (1 + 1);

            Debug.Assert(avg == s);


            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// 平均值结果为 null 时查询
        /// </summary>
        /// <returns></returns>
        async Task AvgNullQueryTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            decimal? avg = 0;

            avg = await q.Where(a => a.Id == null).AverageAsync(a => a.Amount);

            Debug.Assert(avg == null);


            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// 最大或最小值查询
        /// </summary>
        /// <returns></returns>
        async Task MaxMinQueryTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            int maxMonth = await q.MaxAsync(a => a.CreateMonth);
            int minMonth = await q.MinAsync(a => a.CreateMonth);

            Debug.Assert(maxMonth == 12);
            Debug.Assert(minMonth == 1);


            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// 分组聚合
        /// </summary>
        /// <returns></returns>
        async Task GroupQueryTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            var results = await q.Where(a => a.Amount > 0)
                .GroupBy(a => a.CreateMonth)
                .Select(a => new
                {
                    a.CreateMonth,
                    Count = Sql.Count(),
                    Sum = Sql.Sum(a.Amount),
                    AmountCount = Sql.Count(a.Amount),
                    Avg = Sql.Average(a.Amount),
                    MaxAmount = Sql.Max(a.Amount),
                    MinAmount = Sql.Min(a.Amount)
                }).ToListAsync();

            Debug.Assert(results.Count == 12);

            var result_6 = results.Where(a => a.CreateMonth == 6).First();

            Debug.Assert(result_6.Count == 30 * 2 * 2); //每天有 2 条数据，一个月30天，两年则 30 * 2 * 2
            Debug.Assert(result_6.Sum == (10 + 20) * 30 * 2); //每天有 2 条数据，一条 Amount=10，一条 Amount=20，两年数据则是 (10 + 20) * 30 * 2
            Debug.Assert(result_6.AmountCount == 2 * 30 * 2); //每天有 2 条数据，一个月 30 天，两年数据则是 2 * 30 * 2
            Debug.Assert(result_6.Avg == (10 + 20) / 2); //每天有 2 条数据，一条 Amount=10，一条 Amount=20，平均则是 (10 + 20) / 2
            Debug.Assert(result_6.MaxAmount == 20);
            Debug.Assert(result_6.MinAmount == 10);

            Helpers.PrintSplitLine();
        }

        /// <summary>
        /// 排除指定字段查询
        /// </summary>
        /// <returns></returns>
        async Task ExcludeFieldQueryTest()
        {
            IDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            var orders = await q.Exclude(a => new { a.UserId, a.Amount }).OrderByDesc(a => a.CreateTime).Take(10).ToListAsync();

            foreach (var order in orders)
            {
                Debug.Assert(order.UserId == default(string));
                Debug.Assert(order.Amount == default(decimal));
            }


            var result = await q.Exclude(a => new { a.UserId, a.Amount }).OrderBy(a => a.CreateTime).PagingAsync(1, 20);

            foreach (var order in result.DataList)
            {
                Debug.Assert(order.UserId == default(string));
                Debug.Assert(order.Amount == default(decimal));
            }

            Helpers.PrintSplitLine();
        }
    }
}

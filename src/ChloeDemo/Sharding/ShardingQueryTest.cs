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
        public async Task Run()
        {
            ShardingConfigBuilder<Order> shardingConfigBuilder = new ShardingConfigBuilder<Order>();
            shardingConfigBuilder.HasShardingKey(a => a.CreateTime);
            shardingConfigBuilder.HasRoute(new OrderShardingRoute(new List<int>() { 2018, 2019 }));

            ShardingConfigContainer.Add(shardingConfigBuilder.Build());

            await this.Test1();
            await this.Test2();
            await this.Test3();
            await this.Test4();
            await this.Test5();
            await this.Test6();
            await this.Test7();
            await this.Test8();
            await this.AnyQueryTest();
            await this.CountQueryTest();
            await this.SumQueryTest();

            Console.WriteLine("query test over...");
            Console.ReadKey();
        }

        ShardingDbContext CreateDbContext()
        {
            ShardingDbContext dbContext = new ShardingDbContext(new ShardingOptions() { MaxConnectionsPerDataSource = 12 });

            return dbContext;
        }

        async Task Test1()
        {
            /*
             * 根据分片字段升序
             */
            ShardingDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();
            q = q.OrderBy(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            Helpers.PrintResult(result);

            var dataList = result.DataList;

            Debug.Assert(result.Count == 1460);
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

            Debug.Assert(result.Count == 1460);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2018-01-11 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2018-01-11 12:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2018-01-20 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2018-01-20 12:00"));

            Helpers.PrintSplitLine();
        }
        async Task Test2()
        {
            /*
             * 根据分片字段降序
             */
            ShardingDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();
            q = q.OrderByDesc(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            Helpers.PrintResult(result);

            var dataList = result.DataList;

            Debug.Assert(result.Count == 1460);
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

            Debug.Assert(result.Count == 1460);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2019-12-21 12:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2019-12-21 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2019-12-12 12:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2019-12-12 10:00"));

            Helpers.PrintSplitLine();
        }
        async Task Test3()
        {
            /*
             * 根据分片字段降序，单库内跨表
             */
            ShardingDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            DateTime dt = new DateTime(2019, 12, 2);
            q = q.Where(a => a.CreateTime < dt);

            q = q.OrderByDesc(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            var dataList = result.DataList;
            Helpers.PrintResult(result);

            Debug.Assert(result.Count == 1400);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2019-12-01 12:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2019-12-01 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2019-11-22 12:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2019-11-22 10:00"));

            Helpers.PrintSplitLine();
        }
        async Task Test4()
        {
            /*
             * 根据分片字段排序，并跨库测试
             */

            ShardingDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            DateTime dt = new DateTime(2018, 12, 31);
            q = q.Where(a => a.CreateTime >= dt);

            q = q.OrderBy(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            var dataList = result.DataList;
            Helpers.PrintResult(result);

            Debug.Assert(result.Count == 732);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2018-12-31 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2018-12-31 12:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2019-01-09 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2019-01-09 12:00"));

            Helpers.PrintSplitLine();
        }
        async Task Test5()
        {
            /*
             * 根据非分片字段排序
             */

            PagingResult<Order> result;
            List<Order> dataList;

            ShardingDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            q = q.OrderBy(a => a.Amount).ThenBy(a => a.Id);

            result = await q.PagingAsync(1, 20);
            dataList = result.DataList;
            Helpers.PrintResult(result);

            Debug.Assert(result.Count == 1460);
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

            Debug.Assert(result.Count == 1460);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2018-01-21 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2018-01-22 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2018-02-08 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2018-02-09 10:00"));

            Helpers.PrintSplitLine();
        }
        async Task Test6()
        {
            /*
             * 根据主键查询
             */

            ShardingDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            string id = "2019-12-01 10:00";

            Order entity = null;

            entity = await q.Where(a => a.Id == id).FirstOrDefaultAsync();

            Debug.Assert(entity.Id == id);

            Helpers.PrintSplitLine();

            /*
             * 主键 + 分片字段查询，会精确路由到所在的表
             */

            DateTime createTime = DateTime.Parse(id);
            entity = await q.Where(a => a.Id == id && a.CreateTime == createTime).FirstOrDefaultAsync();

            Debug.Assert(entity.Id == id);

            Helpers.PrintSplitLine();
        }
        async Task Test7()
        {
            /*
             * 根据非分片字段路由
             */

            ShardingDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            //根据 CreateYear 查询
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

        async Task Test8()
        {
            /*
             * In, Contains, Equals, Sql.Equals 等方法路由
             */

            List<Order> orders = new List<Order>();

            ShardingDbContext dbContext = this.CreateDbContext();
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

        async Task AnyQueryTest()
        {
            List<Order> orders = new List<Order>();

            ShardingDbContext dbContext = this.CreateDbContext();
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
        async Task CountQueryTest()
        {
            ShardingDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            long count = await q.LongCountAsync();

            Debug.Assert(count == 1460);

            Helpers.PrintSplitLine();
        }
        async Task SumQueryTest()
        {
            ShardingDbContext dbContext = this.CreateDbContext();
            var q = dbContext.Query<Order>();

            decimal? sum = 0;

            sum = await q.SumAsync(a => a.Amount);

            int count = await q.CountAsync();

            //每天有 2 条数据，一条 Amount=10，一条 Amount=20
            int s = (count / 2) * (10 + 20);

            Debug.Assert(sum == s);


            Helpers.PrintSplitLine();
        }
    }
}

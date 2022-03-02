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
    internal class ShardingTest
    {
        public async Task Run()
        {
            //await InitData();
            //Console.ReadKey();

            ShardingConfigBuilder<Order> shardingConfigBuilder = new ShardingConfigBuilder<Order>();
            shardingConfigBuilder.HasShardingKey(a => a.CreateTime);
            shardingConfigBuilder.HasRoute(new OrderShardingRoute());

            ShardingConfigContainer.Add(shardingConfigBuilder.Build());

            await this.Test1();
            await this.Test2();
            await this.Test3();
            await this.Test4();
            await this.Test5();
            await this.Test6();

            Console.WriteLine("test over...");

            //DateTime dt = new DateTime(2020, 12, 31);

            //ShardingDbContext dbContext = new ShardingDbContext();
            //var q = dbContext.Query<Order>();
            ////q = q.Where(a => a.CreateTime >= dt);
            ////q = q.OrderByDesc(a => a.CreateTime);
            //q = q.OrderBy(a => a.CreateTime);
            //var result = q.Paging(4, 20);

            //var dataList = result.DataList;

            //Debug.Assert(dataList.First().CreateTime == DateTime.Parse("2020-1-30 10:00"));

            //Console.WriteLine($"Totals: {result.Count} result.Count: {result.DataList.Count}");

            Console.ReadKey();
        }

        static void PrintSplitLine()
        {
            Console.WriteLine("--------------------------------------------------------------------------------------");
        }
        static void PrintResult(PagingResult<Order> result)
        {
            var dataList = result.DataList;

            Console.WriteLine($"Totals: {result.Count} Takens: {result.DataList.Count}");

            Console.WriteLine(dataList[0].CreateTime.ToString("yyyy-MM-dd HH:mm"));
            Console.WriteLine(dataList[1].CreateTime.ToString("yyyy-MM-dd HH:mm"));
            Console.WriteLine(dataList[dataList.Count - 2].CreateTime.ToString("yyyy-MM-dd HH:mm"));
            Console.WriteLine(dataList[dataList.Count - 1].CreateTime.ToString("yyyy-MM-dd HH:mm"));
        }

        async Task Test1()
        {
            /*
             * 根据分片字段升序
             */
            ShardingDbContext dbContext = new ShardingDbContext();
            var q = dbContext.Query<Order>();
            q = q.OrderBy(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            PrintResult(result);

            var dataList = result.DataList;

            Debug.Assert(result.Count == 1462);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2020-01-01 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2020-01-01 12:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2020-01-10 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2020-01-10 12:00"));

            PrintSplitLine();

            /*
             * 取第二页
             */
            result = await q.PagingAsync(2, 20);
            PrintResult(result);

            dataList = result.DataList;

            Debug.Assert(result.Count == 1462);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2020-01-11 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2020-01-11 12:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2020-01-20 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2020-01-20 12:00"));

            PrintSplitLine();
        }
        async Task Test2()
        {
            /*
             * 根据分片字段降序
             */
            ShardingDbContext dbContext = new ShardingDbContext();
            var q = dbContext.Query<Order>();
            q = q.OrderByDesc(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            PrintResult(result);

            var dataList = result.DataList;

            Debug.Assert(result.Count == 1462);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2021-12-31 12:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2021-12-31 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2021-12-22 12:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2021-12-22 10:00"));

            PrintSplitLine();

            /*
             * 取第二页
             */
            result = await q.PagingAsync(2, 20);
            dataList = result.DataList;
            PrintResult(result);

            Debug.Assert(result.Count == 1462);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2021-12-21 12:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2021-12-21 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2021-12-12 12:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2021-12-12 10:00"));

            PrintSplitLine();
        }
        async Task Test3()
        {
            /*
             * 根据分片字段降序，单库内跨表
             */
            ShardingDbContext dbContext = new ShardingDbContext();
            var q = dbContext.Query<Order>();

            DateTime dt = new DateTime(2021, 12, 2);
            q = q.Where(a => a.CreateTime < dt);

            q = q.OrderByDesc(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            var dataList = result.DataList;
            PrintResult(result);

            Debug.Assert(result.Count == 1402);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2021-12-01 12:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2021-12-01 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2021-11-22 12:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2021-11-22 10:00"));

            PrintSplitLine();
        }
        async Task Test4()
        {
            /*
             * 根据分片字段排序，并跨库测试
             */

            ShardingDbContext dbContext = new ShardingDbContext();
            var q = dbContext.Query<Order>();

            DateTime dt = new DateTime(2020, 12, 31);
            q = q.Where(a => a.CreateTime >= dt);

            q = q.OrderBy(a => a.CreateTime);

            var result = await q.PagingAsync(1, 20);
            var dataList = result.DataList;
            PrintResult(result);

            Debug.Assert(result.Count == 732);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2020-12-31 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2020-12-31 12:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2021-01-09 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2021-01-09 12:00"));

            PrintSplitLine();
        }
        async Task Test5()
        {
            /*
             * 根据非分片字段排序
             */

            PagingResult<Order> result;
            List<Order> dataList;

            ShardingDbContext dbContext = new ShardingDbContext();
            var q = dbContext.Query<Order>();

            q = q.OrderBy(a => a.Amount).ThenBy(a => a.Id);

            result = await q.PagingAsync(1, 20);
            dataList = result.DataList;
            PrintResult(result);

            Debug.Assert(result.Count == 1462);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2020-01-01 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2020-01-02 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2020-01-19 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2020-01-20 10:00"));

            PrintSplitLine();

            /*
             * 取第二页
             */
            result = await q.PagingAsync(2, 20);
            PrintResult(result);

            dataList = result.DataList;

            Debug.Assert(result.Count == 1462);
            Debug.Assert(result.DataList.Count == 20);

            Debug.Assert(dataList[0].CreateTime == DateTime.Parse("2020-01-21 10:00"));
            Debug.Assert(dataList[1].CreateTime == DateTime.Parse("2020-01-22 10:00"));

            Debug.Assert(dataList[dataList.Count - 2].CreateTime == DateTime.Parse("2020-02-08 10:00"));
            Debug.Assert(dataList.Last().CreateTime == DateTime.Parse("2020-02-09 10:00"));

            PrintSplitLine();
        }
        async Task Test6()
        {
            /*
             * 根据主键查询
             */

            ShardingDbContext dbContext = new ShardingDbContext(new ShardingOptions() { MaxConnectionsPerDataSource = 6 });
            var q = dbContext.Query<Order>();

            string id = "2021-12-01 10:00";

            Order entity = null;

            entity = await q.Where(a => a.Id == id).FirstOrDefaultAsync();

            Debug.Assert(entity.Id == id);

            PrintSplitLine();

            /*
             * 主键 + 分片字段查询，会精确路由到所在的表
             */

            DateTime createTime = DateTime.Parse(id);
            entity = await q.Where(a => a.Id == id && a.CreateTime == createTime).FirstOrDefaultAsync();

            Debug.Assert(entity.Id == id);

            PrintSplitLine();
        }

        public static async Task InitData()
        {
            await InitData(2020);
            await InitData(2021);
            Console.WriteLine("InitData over");
        }

        public static string BuildTableName(int month)
        {
            string suffix = month.ToString();
            if (suffix.Length == 1)
            {
                suffix = $"0{suffix}";
            }

            string table = $"order{suffix}";

            return table;
        }

        public static async Task InitData(int year)
        {
            MySqlContext dbContext = new MySqlContext(new MySqlConnectionFactory($"Server=localhost;Port=3306;Database=order{year};Uid=root;Password=sasa;Charset=utf8; Pooling=True; Max Pool Size=200;Allow User Variables=True;SslMode=none;"));

            for (int month = 1; month <= 12; month++)
            {
                string table = BuildTableName(month);
                dbContext.Delete<Order>(a => true, table);

                DateTime firstDate = new DateTime(year, month, 1);

                int day = 0;
                while (true)
                {
                    DateTime date = firstDate.AddDays(day);

                    if (date.Month != month)
                    {
                        break;
                    }

                    Order order1 = new Order();
                    order1.UserId = "chloe";
                    order1.Amount = 10;
                    order1.CreateTime = date.AddHours(10);
                    order1.Id = order1.CreateTime.ToString("yyyy-MM-dd HH:mm");

                    Order order2 = new Order();
                    order1.UserId = "shuxin";
                    order2.Amount = 20;
                    order2.CreateTime = date.AddHours(12);
                    order2.Id = order2.CreateTime.ToString("yyyy-MM-dd HH:mm");

                    dbContext.Insert(order1, table);
                    dbContext.Insert(order2, table);

                    day++;
                }
            }
        }
    }
}

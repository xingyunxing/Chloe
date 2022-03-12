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

            ShardingQueryTest queryTest = new ShardingQueryTest();
            await queryTest.Run();


            ShardingConfigBuilder<Order> shardingConfigBuilder = new ShardingConfigBuilder<Order>();
            shardingConfigBuilder.HasShardingKey(a => a.CreateTime);
            shardingConfigBuilder.HasRoute(new OrderShardingRoute(new List<int>() { 2020, 2021 }));

            ShardingConfigContainer.Add(shardingConfigBuilder.Build());

            await this.Test1();
            await this.Test2();

            Console.WriteLine("over...");
            Console.ReadKey();
        }

        public static async Task InitData()
        {
            await InitData(2018);
            await InitData(2019);
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
                    order1.SetCreateTime(date.AddHours(10));

                    Order order2 = new Order();
                    order2.UserId = "shuxin";
                    order2.Amount = 20;
                    order2.SetCreateTime(date.AddHours(12));

                    dbContext.Insert(order1, table);
                    dbContext.Insert(order2, table);

                    day++;
                }
            }
        }


        async Task Test1()
        {
            /*
             * 增删查改
             */
            ShardingDbContext dbContext = new ShardingDbContext();

            int rowsAffected = 0;
            DateTime createTime = new DateTime(2021, 1, 1, 1, 1, 1);

            Order order = new Order();
            order.UserId = "chloe";
            order.Amount = 10;
            order.SetCreateTime(createTime);

            rowsAffected = await dbContext.DeleteAsync(order);
            Console.WriteLine($"deleted: {rowsAffected}");

            await dbContext.InsertAsync(order);

            var q = dbContext.Query<Order>();
            q = q.Where(a => a.Id == order.Id);

            Order entity = await q.FirstOrDefaultAsync();
            Debug.Assert(entity.Id == order.Id);

            entity.Amount = 100;
            rowsAffected = await dbContext.UpdateAsync(entity);

            Debug.Assert(rowsAffected == 1);

            entity = await q.FirstOrDefaultAsync();
            Debug.Assert(entity.Amount == 100);

            rowsAffected = await dbContext.DeleteAsync(order);
            Debug.Assert(rowsAffected == 1);

            entity = await q.FirstOrDefaultAsync();
            Debug.Assert(entity == null);

            Helpers.PrintSplitLine();
        }

        async Task Test2()
        {
            /*
             * 按条件删除和更新
             */
            ShardingDbContext dbContext = new ShardingDbContext();

            int rowsAffected = 0;

            List<Order> orders = await dbContext.Query<Order>().Where(a => a.CreateYear == 2021).ToListAsync();

            rowsAffected = await dbContext.DeleteAsync<Order>(a => a.CreateYear == 2021);

            Debug.Assert(rowsAffected == orders.Count);

            await InitData(2021);

            string newUserId = "chloe2021";
            rowsAffected = await dbContext.UpdateAsync<Order>(a => a.CreateYear == 2021, a => new Order()
            {
                UserId = newUserId,
            });

            Debug.Assert(rowsAffected == 730);

            orders = await dbContext.Query<Order>().Where(a => a.CreateYear == 2021).ToListAsync();
            Debug.Assert(orders.All(a => a.UserId == newUserId));

            rowsAffected = await dbContext.UpdateAsync<Order>(a => a.CreateYear == 2021 && a.CreateMonth == 12, a => new Order()
            {
                UserId = "chloe2021",
            });

            Debug.Assert(rowsAffected == 62);

            Helpers.PrintSplitLine();
        }
    }
}

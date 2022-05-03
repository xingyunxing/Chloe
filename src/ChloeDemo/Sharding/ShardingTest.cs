using Chloe;
using Chloe.MySql;
using Chloe.MySql.DDL;
using Chloe.Sharding;
using Chloe.SqlServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo.Sharding
{
    public abstract class ShardingTest
    {
        protected ShardingTest()
        {

        }

        public async Task Run()
        {
            await InitData();

            ShardingQueryTest queryTest = new ShardingQueryTest(this);
            await queryTest.Run();

            ShardingConfigBuilder<Order> shardingConfigBuilder = new ShardingConfigBuilder<Order>();
            shardingConfigBuilder.HasShardingKey(a => a.CreateTime);     //配置分片字段
            shardingConfigBuilder.HasRoute(new OrderShardingRoute(this, new List<int>() { 2020, 2021 })); //设置分片路由

            ShardingConfigContainer.Add(shardingConfigBuilder.Build());     //注册分片配置信息

            await this.CrudTest();
            await this.UpdateByLambdaTest();
            await this.DeleteByLambdaTest();

            Console.WriteLine("over...");
            Console.ReadKey();
        }

        public abstract DbContext CreateInitDataDbContext(int year);
        public abstract IDbContextProvider CreateDbContextProvider(int year);
        public abstract void CreateTable<TEntity>(DbContext dbContext, string table);

        IDbContext CreateDbContext()
        {
            DbContext dbContext = new DbContext();
            dbContext.ShardingOptions.MaxConnectionsPerDataSource = 6;
            return dbContext;
        }

        public async Task InitData()
        {
            /*
             * 初始化测试数据：
             * 按年分库，按月分表
             * 每天两条数据
             * 注：需要手动建库
             */

            await InitData(2018);
            await InitData(2019);
            await InitData(2020);
            await InitData(2021);
            Console.WriteLine("InitData over");
        }
        public async Task InitData(int year)
        {
            /*
             * 初始化测试数据：
             * 按月分表
             * 每天两条数据
             * 注：需要手动建库
             */
            DbContext dbContext = this.CreateInitDataDbContext(year);
            dbContext.ShardingEnabled = false;


            for (int month = 1; month <= 12; month++)
            {
                string table = BuildTableName(month);

                this.CreateTable<Order>(dbContext, table);

                bool hasData = dbContext.Query<Order>(table).Any();
                if (hasData)
                {
                    continue;
                }
                //Console.WriteLine($"insert year:{year} table:{table}");
                //Console.ReadKey();
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



        async Task CrudTest()
        {
            /*
             * 增删查改
             */
            IDbContext dbContext = this.CreateDbContext();

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

        async Task UpdateByLambdaTest()
        {
            /*
             * 按条件删除和更新
             */
            IDbContext dbContext = this.CreateDbContext();

            await InitData(2021);

            int rowsAffected = 0;

            string newUserId = "chloe2021";
            rowsAffected = await dbContext.UpdateAsync<Order>(a => a.CreateYear == 2021, a => new Order()
            {
                UserId = newUserId,
            });

            Debug.Assert(rowsAffected == 730);

            List<Order> orders = await dbContext.Query<Order>().Where(a => a.CreateYear == 2021).ToListAsync();
            Debug.Assert(orders.All(a => a.UserId == newUserId));

            rowsAffected = await dbContext.UpdateAsync<Order>(a => a.CreateYear == 2021 && a.CreateMonth == 12, a => new Order()
            {
                UserId = "chloe2021",
            });

            Debug.Assert(rowsAffected == 62);

            Helpers.PrintSplitLine();
        }

        async Task DeleteByLambdaTest()
        {
            /*
             * 按条件删除
             */
            IDbContext dbContext = this.CreateDbContext();

            int rowsAffected = 0;

            List<Order> orders = await dbContext.Query<Order>().Where(a => a.CreateYear == 2021).ToListAsync();

            rowsAffected = await dbContext.DeleteAsync<Order>(a => a.CreateYear == 2021);

            Debug.Assert(rowsAffected == orders.Count);

            Helpers.PrintSplitLine();
        }
    }
}

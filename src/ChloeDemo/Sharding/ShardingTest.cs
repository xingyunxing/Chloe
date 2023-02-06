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
            /*
             * 注：
             * Chloe.ORM 不负责创建数据库和表，
             * 运行此测试前请手动创建好四个数据库（order2018、order2019、order2020、order2021），然后修改 ShardingTestImpl.cs 里的数据库连接字符串
             */

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

        /// <summary>
        /// 创建数据源
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
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
             * 注：运行程序前请手动创建好四个数据库（order2018、order2019、order2020、order2021），然后修改 ShardingTestImpl.cs 里的数据库连接字符串
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

        /// <summary>
        /// 根据月份构造表名
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
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

            Order entity = null;
            entity = await q.Where(a => a.Id == order.Id).FirstOrDefaultAsync();
            Debug.Assert(entity.Id == order.Id);

            //加上分片字段，则会精准定位到所在的表
            entity = await q.Where(a => a.Id == order.Id && a.CreateTime == order.CreateTime).FirstOrDefaultAsync();
            Debug.Assert(entity.Id == order.Id);

            entity.Amount = 100;
            rowsAffected = await dbContext.UpdateAsync(entity);

            Debug.Assert(rowsAffected == 1);

            entity = await q.Where(a => a.Id == entity.Id).FirstOrDefaultAsync();
            Debug.Assert(entity.Amount == 100);

            rowsAffected = await dbContext.DeleteAsync(entity);
            Debug.Assert(rowsAffected == 1);

            entity = await q.Where(a => a.Id == entity.Id).FirstOrDefaultAsync();
            Debug.Assert(entity == null);

            Helpers.PrintSplitLine();
        }

        async Task UpdateByLambdaTest()
        {
            /*
             * 按条件更新
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

            List<Order> orders = await dbContext.Query<Order>().Where(a => a.CreateMonth == 1 || a.CreateMonth == 2).ToListAsync();

            Debug.Assert(orders.Count > 0);

            rowsAffected = await dbContext.DeleteAsync<Order>(a => a.CreateMonth == 1 || a.CreateMonth == 2);

            Debug.Assert(rowsAffected == orders.Count);

            Helpers.PrintSplitLine();
        }
    }
}

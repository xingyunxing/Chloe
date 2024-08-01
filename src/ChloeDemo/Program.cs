using Chloe;
using Chloe.Infrastructure;
using Chloe.Infrastructure.Interception;
using Chloe.PostgreSQL;
using Chloe.Reflection.Emit;
using Chloe.Sharding;
using Chloe.Sharding.Routing;
using ChloeDemo.Sharding;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ChloeDemo
{
    public class Program
    {
        /* documentation：https://github.com/shuxinqin/Chloe/wiki */
        public static void Main(string[] args)
        {
            /*
             * Q: 查询如 q.Where(a=> a.Name == "Chloe") 为什么生成的不是参数化 sql ？
             * A: 因为框架内部解析 lambda 时对于常量（ConstantExpression）不做参数化处理（刻意的，不要问为什么，问我也不告诉你），如需参数化，请使用变量，如：
             * var name = "Chloe";
             * q = q.Where(a=> a.Name == name);
             * ...
             * Tip: 自行拼接 lambda 表达式树的注意了，##千万不要用 ConstantExpression 包装你的变量，否则会生成非参数化 sql，存在 sql 注入风险哦！！！##包装变量方式参考这个 MakeWrapperAccess 方法：
             * https://github.com/shuxinqin/Chloe/blob/master/src/Chloe/Extensions/ExpressionExtension.cs#L130
             */

            /* 添加拦截器，输出 sql 语句极其相应的参数 */
            IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            DbConfiguration.UseInterceptors(interceptor);
            DbConfiguration.UseInterceptors(new ChloeDiagnosticListenerInterceptor());

            ConfigureMappingType();
            ConfigureMethodHandler();

            /* fluent mapping */
            DbConfiguration.UseTypeBuilders(typeof(PersonMap));
            DbConfiguration.UseTypeBuilders(typeof(PersonExMap));
            DbConfiguration.UseTypeBuilders(typeof(CityMap));
            DbConfiguration.UseTypeBuilders(typeof(ProvinceMap));
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));


            /*
             * 运行##分库分表测试##前请手动创建好四个数据库（order2018、order2019、order2020、order2021，不用建表，程序会生成相应的表并初始化数据），然后修改 Sharding/ShardingTestImpl.cs 里的数据库连接字符串
             */

            ////sqlserver 分库分表测试
            MsSqlShardingTest msSqlShardingTest = new MsSqlShardingTest();
            msSqlShardingTest.Run().GetAwaiter().GetResult();

            ////mysql 分库分表测试
            MySqlShardingTest mySqlShardingTest = new MySqlShardingTest();
            mySqlShardingTest.Run().GetAwaiter().GetResult();

            ////oracle 分库分表测试
            //OracleShardingTest oracleShardingTest = new OracleShardingTest();
            //oracleShardingTest.Run().GetAwaiter().GetResult();

            ////postgreSQL 分库分表测试
            //PostgreSQLShardingTest postgreSQLShardingTest = new PostgreSQLShardingTest();
            //postgreSQLShardingTest.Run().GetAwaiter().GetResult();

            ////Dameng 需要装多个数据库实现多库，不过自带分区表，不大需要分库
            ////DamengShardingTest damengShardingTest = new DamengShardingTest();
            ////damengShardingTest.Run().GetAwaiter().GetResult();

            ////KingbaseES 分库分表测试
            //KingbaseESShardingTest kingbaseESShardingTest = new KingbaseESShardingTest();
            //kingbaseESShardingTest.Run().GetAwaiter().GetResult();


            /*
             * 运行以下测试前请手动创建好数据库(不用建表，程序会生成相应的表并初始化数据)，然后修改各个 XXXDemo.cs 文件里的数据库连接字符串
             */

            RunDemo<SQLiteDemo>();
            RunDemo<MsSqlDemo>();
            RunDemo<MsSqlOdbcDemo>();
            RunDemo<MySqlDemo>();
            //RunDemo<PostgreSQLDemo>();
            //RunDemo<OracleDemo>();
            //RunDemo<DamengDemo>();
            //RunDemo<KingbaseESDemo>();
        }

        static void RunDemo<TDemo>() where TDemo : DemoBase, new()
        {
            Console.WriteLine($"Start {typeof(TDemo).Name}...");

            using (TDemo demo = new TDemo())
            {
                demo.Run();
            }

            ConsoleHelper.WriteLineAndReadKey($"End {typeof(TDemo).Name}...");
        }

        /// <summary>
        /// 配置映射类型。
        /// </summary>
        static void ConfigureMappingType()
        {
            MappingTypeBuilder stringTypeBuilder = DbConfiguration.ConfigureMappingType<string>();
            stringTypeBuilder.HasDbParameterAssembler<String_MappingType>();
            stringTypeBuilder.HasDbValueConverter<String_MappingType>();
        }

        /// <summary>
        /// 配置方法翻译解析器。
        /// </summary>
        static void ConfigureMethodHandler()
        {
            PostgreSQLContext.SetMethodHandler("StringLike", new PostgreSQL_StringLike_MethodHandler());
        }
    }
}

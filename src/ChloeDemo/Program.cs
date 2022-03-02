using Chloe;
using Chloe.Infrastructure;
using Chloe.Infrastructure.Interception;
using Chloe.PostgreSQL;
using Chloe.Sharding;
using ChloeDemo.Sharding;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChloeDemo
{


    public class AA //: IEqualityComparer<AA>][
    {
        //public bool Equals([AllowNull] AA x, [AllowNull] AA y)
        //{
        //    return true;
        //}

        //public int GetHashCode([DisallowNull] AA obj)
        //{
        //    return 0;
        //}

        public override int GetHashCode()
        {
            return 1;
        }

        //public override bool Equals(object obj)
        //{
        //    return true;
        //}
    }

    public class Program
    {
        /* documentation：https://github.com/shuxinqin/Chloe/wiki */
        public static void Main(string[] args)
        {
            /*
             * Q: 查询如 q.Where(a=> a.Name == “Chloe”) 为什么生成的不是参数化 sql ？
             * A: 因为框架内部解析 lambda 时对于常量（ConstantExpression）不做参数化处理（刻意的，不要问为什么，问我也不告诉你），如需参数化，请使用变量，如：
             * var name = "Chloe";
             * q = q.Where(a=> a.Name == name);
             * ...
             * Tip: 自行拼接 lambda 表达式树的注意了，##千万不要用 ConstantExpression 包装你的变量，否则会生成非参数化 sql，存在 sql 注入风险哦！！！##包装变量方式参考这个 MakeWrapperAccess 方法：
             * https://github.com/shuxinqin/Chloe/blob/master/src/Chloe/Extensions/ExpressionExtension.cs#L117
             */

            /* 添加拦截器，输出 sql 语句极其相应的参数 */
            IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            DbConfiguration.UseInterceptors(interceptor);
            DbConfiguration.UseInterceptors(new ChloeDiagnosticListenerInterceptor());

            ConfigureMappingType();
            ConfigureMethodHandler();

            List<RouteTable> list1 = new List<RouteTable>() { };
            list1.Add(new RouteTable() { Name = "order01", DataSource = new RouteDataSource() { Name = "order" } });
            list1.Add(new RouteTable() { Name = "order02", DataSource = new RouteDataSource() { Name = "order" } });

            List<RouteTable> list2 = new List<RouteTable>() { };
            list2.Add(new RouteTable() { Name = "order02", DataSource = new RouteDataSource() { Name = "order" } });
            list2.Add(new RouteTable() { Name = "order03", DataSource = new RouteDataSource() { Name = "order" } });

            var aaa = list1.Union(list2, RouteTableEqualityComparer.Instance).ToList();
            var aaa1 = list1.Intersect(list2, RouteTableEqualityComparer.Instance).ToList();

            /* fluent mapping */
            DbConfiguration.UseTypeBuilders(typeof(PersonMap));
            DbConfiguration.UseTypeBuilders(typeof(PersonExMap));
            DbConfiguration.UseTypeBuilders(typeof(CityMap));
            DbConfiguration.UseTypeBuilders(typeof(ProvinceMap));
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));

            ShardingTest shardingTest = new ShardingTest();
            shardingTest.Run().GetAwaiter().GetResult();

            RunDemo<SQLiteDemo>();
            //RunDemo<MsSqlDemo>();
            //RunDemo<MsSqlOdbcDemo>();
            //RunDemo<MySqlDemo>();
            //RunDemo<PostgreSQLDemo>();
            //RunDemo<OracleDemo>();
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

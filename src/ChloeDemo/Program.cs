using Chloe.Infrastructure;
using Chloe.Infrastructure.Interception;
using Chloe.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ChloeDemo
{
    public class Program
    {
        /* documentation：http://www.52chloe.com/Wiki/Document */
        public static void Main(string[] args)
        {
            /* 添加拦截器，输出 sql 语句极其相应的参数 */
            IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            DbConfiguration.UseInterceptors(interceptor);

            ConfigureMappingType();
            ConfigureMethodHandler();

            /* fluent mapping */
            DbConfiguration.UseTypeBuilders(typeof(PersonMap));
            DbConfiguration.UseTypeBuilders(typeof(CityMap));
            DbConfiguration.UseTypeBuilders(typeof(ProvinceMap));
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));

            RunDemo<SQLiteDemo>();
            RunDemo<MsSqlDemo>();
            RunDemo<MySqlDemo>();
            RunDemo<PostgreSQLDemo>();
            RunDemo<OracleDemo>();
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
            //stringTypeBuilder.HasDbValueConverter<String_MappingType>();
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

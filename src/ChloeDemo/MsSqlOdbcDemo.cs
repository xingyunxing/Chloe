using Chloe;
using Chloe.Core;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.SqlServer;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.SqlServer.DDL;
using Chloe.DDL;
using System.Data.Odbc;

namespace ChloeDemo
{
    /*
     * 如果参数化 sql 参数出现日期类型报精度溢出错误问题，请参考 ./DbCommandInterceptor.cs 文件，在拦截器里对日期参数进行处理
     */
    class MsSqlOdbcDemo : MsSqlDemo
    {
        public MsSqlOdbcDemo()
        {

        }

        protected override IDbContext CreateDbContext()
        {
            string connStr = "Driver={ODBC Driver 17 for SQL Server};Server=.;Database=Chloe;UID=sa;PWD=sa;";
            MsSqlContext dbContext = new MsSqlContext(() => new OdbcConnection(connStr));
            dbContext.PagingMode = PagingMode.ROW_NUMBER;
            dbContext.BindParameterByName = false;
            return dbContext;
        }

        public override void ExecuteCommandText()
        {
            List<Person> persons = this.DbContext.SqlQuery<Person>("select * from Person where Age > ?", DbParam.Create("@age", 1)).ToList();

            int rowsAffected = this.DbContext.Session.ExecuteNonQuery("update Person set name=? where Id = 1", DbParam.Create("@name", "Chloe"));

            /* 
             * 执行存储过程:
             * Person person = this.DbContext.SqlQuery<Person>("Proc_GetPerson", CommandType.StoredProcedure, DbParam.Create("@id", 1)).FirstOrDefault();
             * rowsAffected = this.DbContext.Session.ExecuteNonQuery("Proc_UpdatePersonName", CommandType.StoredProcedure, DbParam.Create("@name", "Chloe"));
             */

            ConsoleHelper.WriteLineAndReadKey();
        }
    }
}

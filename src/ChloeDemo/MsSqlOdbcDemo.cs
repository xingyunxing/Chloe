using Chloe;
using Chloe.Core;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.SqlServer.Odbc;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.SqlServer.Odbc.DDL;
using Chloe.DDL;
using System.Data.Odbc;

namespace ChloeDemo
{
    class MsSqlOdbcDemo : DemoBase
    {
        public MsSqlOdbcDemo()
        {
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));
        }

        protected override IDbContext CreateDbContext()
        {
            string connStr = "Driver={ODBC Driver 17 for SQL Server};Server=.;Database=Chloe;UID=sa;PWD=sa;";
            MsSqlContext dbContext = new MsSqlContext(() => new OdbcConnection(connStr));
            dbContext.PagingMode = PagingMode.ROW_NUMBER;
            return dbContext;
        }

        public override void InitDatabase()
        {
            new SqlServerTableGenerator(this.DbContext).CreateTables(TableCreateMode.CreateNew);
        }

        public override void Method()
        {
            IQuery<Person> q = this.DbContext.Query<Person>();

            var space = new char[] { ' ' };

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddDays(1);
            q.Select(a => new
            {
                Id = a.Id,

                //CustomFunction = DbFunctions.MyFunction(a.Id), //自定义函数

                String_Length = (int?)a.Name.Length,//LEN([Person].[Name])
                Substring = a.Name.Substring(0),//SUBSTRING([Person].[Name],0 + 1,LEN([Person].[Name]))
                Substring1 = a.Name.Substring(1),//SUBSTRING([Person].[Name],1 + 1,LEN([Person].[Name]))
                Substring1_2 = a.Name.Substring(1, 2),//SUBSTRING([Person].[Name],1 + 1,2)
                ToLower = a.Name.ToLower(),//LOWER([Person].[Name])
                ToUpper = a.Name.ToUpper(),//UPPER([Person].[Name])
                IsNullOrEmpty = string.IsNullOrEmpty(a.Name),//too long
                Contains = (bool?)a.Name.Contains("s"),//
                Trim = a.Name.Trim(),//RTRIM(LTRIM([Person].[Name]))
                TrimStart = a.Name.TrimStart(space),//LTRIM([Person].[Name])
                TrimEnd = a.Name.TrimEnd(space),//RTRIM([Person].[Name])
                StartsWith = (bool?)a.Name.StartsWith("s"),//
                EndsWith = (bool?)a.Name.EndsWith("s"),//
                Replace = a.Name.Replace("l", "L"),

                DiffYears = Sql.DiffYears(startTime, endTime),//DATEDIFF(YEAR,@P_0,@P_1)
                DiffMonths = Sql.DiffMonths(startTime, endTime),//DATEDIFF(MONTH,@P_0,@P_1)
                DiffDays = Sql.DiffDays(startTime, endTime),//DATEDIFF(DAY,@P_0,@P_1)
                DiffHours = Sql.DiffHours(startTime, endTime),//DATEDIFF(HOUR,@P_0,@P_1)
                DiffMinutes = Sql.DiffMinutes(startTime, endTime),//DATEDIFF(MINUTE,@P_0,@P_1)
                DiffSeconds = Sql.DiffSeconds(startTime, endTime),//DATEDIFF(SECOND,@P_0,@P_1)
                DiffMilliseconds = Sql.DiffMilliseconds(startTime, endTime),//DATEDIFF(MILLISECOND,@P_0,@P_1)
                //DiffMicroseconds = Sql.DiffMicroseconds(startTime, endTime),//DATEDIFF(MICROSECOND,@P_0,@P_1)  Exception

                /* No longer support method 'DateTime.Subtract(DateTime d)', instead of using 'Sql.DiffXX' */
                //SubtractTotalDays = endTime.Subtract(startTime).TotalDays,//CAST(DATEDIFF(DAY,@P_0,@P_1)
                //SubtractTotalHours = endTime.Subtract(startTime).TotalHours,//CAST(DATEDIFF(HOUR,@P_0,@P_1)
                //SubtractTotalMinutes = endTime.Subtract(startTime).TotalMinutes,//CAST(DATEDIFF(MINUTE,@P_0,@P_1)
                //SubtractTotalSeconds = endTime.Subtract(startTime).TotalSeconds,//CAST(DATEDIFF(SECOND,@P_0,@P_1)
                //SubtractTotalMilliseconds = endTime.Subtract(startTime).TotalMilliseconds,//CAST(DATEDIFF(MILLISECOND,@P_0,@P_1)

                AddYears = startTime.AddYears(1),//DATEADD(YEAR,1,@P_0)
                AddMonths = startTime.AddMonths(1),//DATEADD(MONTH,1,@P_0)
                AddDays = startTime.AddDays(1),//DATEADD(DAY,1,@P_0)
                AddHours = startTime.AddHours(1),//DATEADD(HOUR,1,@P_0)
                AddMinutes = startTime.AddMinutes(2),//DATEADD(MINUTE,2,@P_0)
                AddSeconds = startTime.AddSeconds(120),//DATEADD(SECOND,120,@P_0)
                AddMilliseconds = startTime.AddMilliseconds(20000),//DATEADD(MILLISECOND,20000,@P_0)

                Now = DateTime.Now,//GETDATE()
                UtcNow = DateTime.UtcNow,//GETUTCDATE()
                Today = DateTime.Today,//CAST(GETDATE() AS DATE)
                Date = DateTime.Now.Date,//CAST(GETDATE() AS DATE)
                Year = DateTime.Now.Year,//DATEPART(YEAR,GETDATE())
                Month = DateTime.Now.Month,//DATEPART(MONTH,GETDATE())
                Day = DateTime.Now.Day,//DATEPART(DAY,GETDATE())
                Hour = DateTime.Now.Hour,//DATEPART(HOUR,GETDATE())
                Minute = DateTime.Now.Minute,//DATEPART(MINUTE,GETDATE())
                Second = DateTime.Now.Second,//DATEPART(SECOND,GETDATE())
                Millisecond = DateTime.Now.Millisecond,//DATEPART(MILLISECOND,GETDATE())
                DayOfWeek = DateTime.Now.DayOfWeek,//(DATEPART(WEEKDAY,GETDATE()) - 1)

                Int_Parse = int.Parse("1"),//CAST(N'1' AS INT)
                Int16_Parse = Int16.Parse("11"),//CAST(N'11' AS SMALLINT)
                Long_Parse = long.Parse("2"),//CAST(N'2' AS BIGINT)
                Double_Parse = double.Parse("3"),//CAST(N'3' AS FLOAT)
                Float_Parse = float.Parse("4"),//CAST(N'4' AS REAL)
                //Decimal_Parse = decimal.Parse("5"),//CAST(N'5' AS DECIMAL)  ps: 'Decimal.Parse(string s)' is not supported now,because we don't know the precision and scale information.
                Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),//CAST(N'D544BC4C-739E-4CD3-A3D3-7BF803FCE179' AS UNIQUEIDENTIFIER) AS [Guid_Parse]

                Bool_Parse = bool.Parse("1"),//CASE WHEN CAST(N'1' AS BIT) = CAST(1 AS BIT) THEN CAST(1 AS BIT) WHEN NOT (CAST(N'1' AS BIT) = CAST(1 AS BIT)) THEN CAST(0 AS BIT) ELSE NULL END AS [Bool_Parse]
                DateTime_Parse = DateTime.Parse("1992-1-16"),//CAST(N'1992-1-16' AS DATETIME) AS [DateTime_Parse]

                B = a.Age == null ? false : a.Age > 1, //三元表达式
                CaseWhen = Case.When(a.Id > 100).Then(1).Else(0) //case when
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
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

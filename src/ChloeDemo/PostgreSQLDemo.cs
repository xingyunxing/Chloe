using Chloe;
using Chloe.RDBMS.DDL;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.PostgreSQL;
using Chloe.PostgreSQL.DDL;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    class PostgreSQLDemo : DemoBase
    {
        public PostgreSQLDemo()
        {
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));
        }

        protected override IDbContext CreateDbContext()
        {
            IDbContext dbContext = new PostgreSQLContext(new PostgreSQLConnectionFactory("User ID=postgres;Password=sasa;Host=localhost;Port=5432;Database=Chloe;Pooling=true;"));

            return dbContext;
        }

        public override void InitDatabase()
        {
            new PostgreSQLTableGenerator(this.DbContext).CreateTables(TableCreateMode.CreateNew);
        }

        public override void Method()
        {
            IQuery<Person> q = this.DbContext.Query<Person>();

            var space = new char[] { ' ' };

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddDays(1);
            var result = q.OrderBy(a => a.Id).Select(a => new
            {
                Id = a.Id,

                //CustomFunction = DbFunctions.MyFunction(a.Id), //自定义函数

                String_Length = (int?)a.Name.Length,//
                Substring = a.Name.Substring(0),//
                Substring1 = a.Name.Substring(1),//
                Substring1_2 = a.Name.Substring(1, 2),//
                ToLower = a.Name.ToLower(),//
                ToUpper = a.Name.ToUpper(),//
                IsNullOrEmpty = string.IsNullOrEmpty(a.Name),//
                Contains = (bool?)a.Name.Contains("s"),// ILIKE(不区分大小写匹配)
                Like = (bool?)a.Name.StringLike("s"),// LIKE(区分大小写匹配)
                Trim = a.Name.Trim(),//
                TrimStart = a.Name.TrimStart(space),//
                TrimEnd = a.Name.TrimEnd(space),//
                StartsWith = (bool?)a.Name.StartsWith("s"),//
                EndsWith = (bool?)a.Name.EndsWith("s"),//
                Replace = a.Name.Replace("l", "L"),

                DateTimeSubtract = endTime.Subtract(startTime),

                /* pgsql does not support Sql.DiffXX methods. */
                //DiffYears = Sql.DiffYears(startTime, endTime),//DATEDIFF(YEAR,@P_0,@P_1)
                //DiffMonths = Sql.DiffMonths(startTime, endTime),//DATEDIFF(MONTH,@P_0,@P_1)
                //DiffDays = Sql.DiffDays(startTime, endTime),//DATEDIFF(DAY,@P_0,@P_1)
                //DiffHours = Sql.DiffHours(startTime, endTime),//DATEDIFF(HOUR,@P_0,@P_1)
                //DiffMinutes = Sql.DiffMinutes(startTime, endTime),//DATEDIFF(MINUTE,@P_0,@P_1)
                //DiffSeconds = Sql.DiffSeconds(startTime, endTime),//DATEDIFF(SECOND,@P_0,@P_1)
                //DiffMilliseconds = Sql.DiffMilliseconds(startTime, endTime),//DATEDIFF(MILLISECOND,@P_0,@P_1)
                //DiffMicroseconds = Sql.DiffMicroseconds(startTime, endTime),//DATEDIFF(MICROSECOND,@P_0,@P_1)  Exception

                AddYears = startTime.AddYears(1),//
                AddMonths = startTime.AddMonths(1),//
                AddDays = startTime.AddDays(1),//
                AddHours = startTime.AddHours(1),//
                AddMinutes = startTime.AddMinutes(2),//
                AddSeconds = startTime.AddSeconds(120),//
                AddMilliseconds = startTime.AddMilliseconds(20000),//

                Now = DateTime.Now,//NOW()
                //UtcNow = DateTime.UtcNow,//GETUTCDATE()
                Today = DateTime.Today,//
                Date = DateTime.Now.Date,//
                Year = DateTime.Now.Year,//
                Month = DateTime.Now.Month,//
                Day = DateTime.Now.Day,//
                Hour = DateTime.Now.Hour,//
                Minute = DateTime.Now.Minute,//
                Second = DateTime.Now.Second,//
                Millisecond = DateTime.Now.Millisecond,//
                DayOfWeek = DateTime.Now.DayOfWeek,//

                Int_Parse = int.Parse("32"),//
                Int16_Parse = Int16.Parse("16"),//
                Long_Parse = long.Parse("64"),//
                Double_Parse = double.Parse("3.123"),//
                Float_Parse = float.Parse("4.123"),//
                Decimal_Parse = decimal.Parse("5.123"),//
                //Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),//

                Bool_Parse = bool.Parse("1"),//
                DateTime_Parse = DateTime.Parse("1992-1-16"),//

                B = a.Age == null ? false : a.Age > 1, //三元表达式
                CaseWhen = Case.When(a.Id > 100).Then(1).Else(0) //case when
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }

    }

}

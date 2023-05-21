using Chloe;
using Chloe.RDBMS.DDL;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.MySql;
using Chloe.MySql.DDL;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    class MySqlDemo : DemoBase
    {
        public MySqlDemo()
        {
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));
        }

        protected override IDbContext CreateDbContext()
        {
            MySqlContext dbContext = new MySqlContext(new MySqlConnectionFactory("Server=localhost;Port=3306;Database=Chloe;Uid=root;Password=sasa;Charset=utf8; Pooling=True; Max Pool Size=200;Allow User Variables=True;SslMode=none;AllowPublicKeyRetrieval=True"));

            return dbContext;
        }

        public override void InitDatabase()
        {
            new MySqlTableGenerator(this.DbContext).CreateTables(TableCreateMode.CreateNew);
        }

        public override void Method()
        {
            IQuery<Person> q = this.DbContext.Query<Person>();

            var space = new char[] { ' ' };

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddDays(1);

            var ret = q.Select(a => new
            {
                Id = a.Id,

                //CustomFunction = DbFunctions.MyFunction(a.Id), //自定义函数

                String_Length = (int?)a.Name.Length,//LENGTH(`Person`.`Name`)
                Substring = a.Name.Substring(0),//SUBSTRING(`Person`.`Name`,0 + 1,LENGTH(`Person`.`Name`))
                Substring1 = a.Name.Substring(1),//SUBSTRING(`Person`.`Name`,1 + 1,LENGTH(`Person`.`Name`))
                Substring1_2 = a.Name.Substring(1, 2),//SUBSTRING(`Person`.`Name`,1 + 1,2)
                ToLower = a.Name.ToLower(),//LOWER(`Person`.`Name`)
                ToUpper = a.Name.ToUpper(),//UPPER(`Person`.`Name`)
                IsNullOrEmpty = string.IsNullOrEmpty(a.Name),//CASE WHEN (`Person`.`Name` IS NULL OR `Person`.`Name` = N'') THEN 1 ELSE 0 END = 1
                Contains = (bool?)a.Name.Contains("s"),//`Person`.`Name` LIKE CONCAT('%',N's','%')
                Trim = a.Name.Trim(),//TRIM(`Person`.`Name`)
                TrimStart = a.Name.TrimStart(space),//LTRIM(`Person`.`Name`)
                TrimEnd = a.Name.TrimEnd(space),//RTRIM(`Person`.`Name`)
                StartsWith = (bool?)a.Name.StartsWith("s"),//`Person`.`Name` LIKE CONCAT(N's','%')
                EndsWith = (bool?)a.Name.EndsWith("s"),//`Person`.`Name` LIKE CONCAT('%',N's')
                Replace = a.Name.Replace("l", "L"),

                DiffYears = Sql.DiffYears(startTime, endTime),//TIMESTAMPDIFF(YEAR,?P_0,?P_1)
                DiffMonths = Sql.DiffMonths(startTime, endTime),//TIMESTAMPDIFF(MONTH,?P_0,?P_1)
                DiffDays = Sql.DiffDays(startTime, endTime),//TIMESTAMPDIFF(DAY,?P_0,?P_1)
                DiffHours = Sql.DiffHours(startTime, endTime),//TIMESTAMPDIFF(HOUR,?P_0,?P_1)
                DiffMinutes = Sql.DiffMinutes(startTime, endTime),//TIMESTAMPDIFF(MINUTE,?P_0,?P_1)
                DiffSeconds = Sql.DiffSeconds(startTime, endTime),//TIMESTAMPDIFF(SECOND,?P_0,?P_1)
                //DiffMilliseconds = Sql.DiffMilliseconds(startTime, endTime),//MySql 不支持 Millisecond
                //DiffMicroseconds = Sql.DiffMicroseconds(startTime, endTime),//ex

                Now = DateTime.Now,//NOW()
                UtcNow = DateTime.UtcNow,//UTC_TIMESTAMP()
                Today = DateTime.Today,//CURDATE()
                Date = DateTime.Now.Date,//DATE(NOW())
                Year = DateTime.Now.Year,//YEAR(NOW())
                Month = DateTime.Now.Month,//MONTH(NOW())
                Day = DateTime.Now.Day,//DAY(NOW())
                Hour = DateTime.Now.Hour,//HOUR(NOW())
                Minute = DateTime.Now.Minute,//MINUTE(NOW())
                Second = DateTime.Now.Second,//SECOND(NOW())
                Millisecond = DateTime.Now.Millisecond,//?P_2 AS `Millisecond`
                DayOfWeek = DateTime.Now.DayOfWeek,//(DAYOFWEEK(NOW()) - 1)

                //Byte_Parse = byte.Parse("1"),//不支持
                Int_Parse = int.Parse("1"),//CAST(N'1' AS SIGNED)
                Int16_Parse = Int16.Parse("11"),//CAST(N'11' AS SIGNED)
                Long_Parse = long.Parse("2"),//CAST(N'2' AS SIGNED)
                //Double_Parse = double.Parse("3"),//N'3' 不支持，否则可能会成为BUG
                //Float_Parse = float.Parse("4"),//N'4' 不支持，否则可能会成为BUG
                //Decimal_Parse = decimal.Parse("5"),//不支持
                Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),//N'D544BC4C-739E-4CD3-A3D3-7BF803FCE179'

                Bool_Parse = bool.Parse("1"),//CAST(N'1' AS SIGNED)
                DateTime_Parse = DateTime.Parse("2014-1-1"),//CAST(N'2014-1-1' AS DATETIME)

                B = a.Age == null ? false : a.Age > 1, //三元表达式
                CaseWhen = Case.When(a.Id > 100).Then(1).Else(0) //case when
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }

    }

}

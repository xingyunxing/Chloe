using Chloe;
using Chloe.Infrastructure;
using Chloe.KingbaseES;
using Chloe.KingbaseES.DDL;
using Chloe.RDBMS.DDL;
using System;

namespace ChloeDemo
{
    internal class KingbaseESDemo : DemoBase
    {
        public KingbaseESDemo()
        {
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));
        }

        protected override IDbContext CreateDbContext()
        {
            KingbaseESContext dbContext = new KingbaseESContext(new KingbaseESConnectionFactory("Server=localhost;User Id=sa;Password=sa;Database=Chloe;Port=54321;"));
            dbContext.Options.DefaultBatchSizeForInsertRange = 500;

            return dbContext;
        }

        public override void InitDatabase()
        {
            new KingbaseESTableGenerator(this.DbContext).CreateTables(TableCreateMode.CreateNew);
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

                String_Length = (int?)a.Name.Length,//LENGTH("person"."name")
                Substring = a.Name.Substring(0),//SUBSTRING("person"."name", 1)
                Substring1 = a.Name.Substring(1),//SUBSTRING("person"."name", 2)
                Substring1_2 = a.Name.Substring(1, 2),//SUBSTRING("person"."name", 2, 2)
                ToLower = a.Name.ToLower(),//LOWER("person"."name")
                ToUpper = a.Name.ToUpper(),//UPPER("person"."name")
                IsNullOrEmpty = string.IsNullOrEmpty(a.Name),//CASE WHEN ( "person"."name" IS NULL OR "person"."name" = N'' ) THEN TRUE WHEN NOT ( ( "person"."name" IS NULL OR "person"."name" = N'' ) ) THEN FALSE ELSE NULL END
                Contains = (bool?)a.Name.Contains("s"),//"person"."name" LIKE '%' || N's' || '%'
                StartsWith = (bool?)a.Name.StartsWith("s"),//"person"."name" LIKE N's' || '%'
                EndsWith = (bool?)a.Name.EndsWith("s"),//"person"."name" LIKE '%' || N's'
                Trim = a.Name.Trim(),//TRIM("person"."name")
                TrimStart = a.Name.TrimStart(space),//LTRIM("person"."name")
                TrimEnd = a.Name.TrimEnd(space),//RTRIM("person"."name")
                Replace = a.Name.Replace("l", "L"),//REPLACE("person"."name", N'l', N'L')

                DiffYears = Sql.DiffYears(startTime, endTime),//EXTRACT(year FROM (@P_0 - @P_1))
                DiffMonths = Sql.DiffMonths(startTime, endTime),//EXTRACT(month FROM (@P_0 - @P_1))
                DiffDays = Sql.DiffDays(startTime, endTime),//EXTRACT(day FROM (@P_0 - @P_1))
                DiffHours = Sql.DiffHours(startTime, endTime),//EXTRACT(hour FROM (@P_0 - @P_1))
                DiffMinutes = Sql.DiffMinutes(startTime, endTime),//EXTRACT(minute FROM (@P_0 - @P_1))
                DiffSeconds = Sql.DiffSeconds(startTime, endTime),//EXTRACT(second FROM (@P_0 - @P_1))
                DiffMilliseconds = Sql.DiffMilliseconds(startTime, endTime),//EXTRACT(milliseconds FROM (@P_0 - @P_1))
                DiffMicroseconds = Sql.DiffMicroseconds(startTime, endTime),//EXTRACT(microseconds FROM (@P_0 - @P_1))

                SubtractTotalDays = endTime.Subtract(startTime).TotalDays,
                SubtractTotalHours = endTime.Subtract(startTime).TotalHours,
                SubtractTotalMinutes = endTime.Subtract(startTime).TotalMinutes,
                SubtractTotalSeconds = endTime.Subtract(startTime).TotalSeconds,
                SubtractTotalMilliseconds = endTime.Subtract(startTime).TotalMilliseconds,

                AddYears = startTime.AddYears(1),//(@P_1 + MAKE_INTERVAL(years:=1)) 
                AddMonths = startTime.AddMonths(1),//(@P_1 + MAKE_INTERVAL(months:=1)) 
                AddDays = startTime.AddDays(1),//(@P_1 + MAKE_INTERVAL(days:=1)) 
                AddHours = startTime.AddHours(1),//(@P_1 + MAKE_INTERVAL(hours:=1)) 
                AddMinutes = startTime.AddMinutes(2),//(@P_1 + MAKE_INTERVAL(mins:=1)) 
                AddSeconds = startTime.AddSeconds(120),//(@P_1 + MAKE_INTERVAL(secs:=1)) 
                AddMilliseconds = startTime.AddMilliseconds(2000),//(@P_1 + MAKE_INTERVAL(secs :=(2000 / 1000))) 

                Now = DateTime.Now,//SYSTIMESTAMP
                UtcNow = DateTime.UtcNow,//( current_timestamp AT TIME ZONE 'UTC' )
                Today = DateTime.Today,//TRUNC(SYSTIMESTAMP, 'dd')
                Date = DateTime.Now.Date,//TRUNC(SYSTIMESTAMP, 'dd')
                Year = DateTime.Now.Year,//DATE_PART('year', SYSTIMESTAMP)
                Month = DateTime.Now.Month,//DATE_PART('month', SYSTIMESTAMP)
                Day = DateTime.Now.Day,//DATE_PART('day', SYSTIMESTAMP)
                Hour = DateTime.Now.Hour,//DATE_PART('hour', SYSTIMESTAMP)
                Minute = DateTime.Now.Minute,//DATE_PART('minute', SYSTIMESTAMP)
                Second = DateTime.Now.Second,//DATE_PART('second', SYSTIMESTAMP)
                Millisecond = DateTime.Now.Millisecond,//DATE_PART('millisecond', SYSTIMESTAMP)
                DayOfWeek = DateTime.Now.DayOfWeek,//CAST(DATE_PART('dow', SYSTIMESTAMP) AS SMALLINT)

                Byte_Parse = byte.Parse("1"),//CAST(N'1' AS SMALLINT)
                Int_Parse = int.Parse("1"),//CAST(N'1' AS integer)
                Int16_Parse = Int16.Parse("11"),//CAST(N'11' AS SMALLINT)
                Long_Parse = long.Parse("2"),//CAST(N'2' AS bigint)
                Double_Parse = double.Parse("3.1"),//CAST(N'3.1' AS double) 
                Float_Parse = float.Parse("4.1"),//CAST(N'4.1' AS REAL)
                Decimal_Parse = decimal.Parse("5"),//CAST(N'5' AS NUMERIC)
                Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),//CAST(N'D544BC4C-739E-4CD3-A3D3-7BF803FCE179' AS uuid)

                Bool_Parse = bool.Parse("1"),//CASE WHEN CAST(N'1' AS boolean) THEN TRUE WHEN NOT ( CAST(N'1' AS boolean) ) THEN FALSE ELSE NULL END
                DateTime_Parse = DateTime.Parse("2014-01-01"),//CAST(N'2014-01-01' AS datetime)

                B = a.Age == null ? false : a.Age > 1, //CASE WHEN "person"."age" IS NULL THEN FALSE WHEN NOT ( "person"."age" IS NULL ) THEN "person"."age" > 1 ELSE NULL END
                CaseWhen = Case.When(a.Id > 100).Then(1).Else(0) //CASE WHEN "person"."id" > 100 THEN 1 ELSE 0 END
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }
    }
}
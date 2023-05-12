using Chloe;
using Chloe.Dameng;
using Chloe.Dameng.DDL;
using Chloe.DDL;
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
    class DamengDemo : DemoBase
    {
        public DamengDemo()
        {
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));
        }

        protected override IDbContext CreateDbContext()
        {
            //DAMENG DMSERVER 5236  SYSDBA dm12345678
            IDbContext dbContext = new DamengContext(new DamengConnectionFactory("Server=localhost; User Id=SYSDBA; PWD=dm12345678;"));
            return dbContext;
        }

        public override void InitDatabase()
        {
            new DamengTableGenerator(this.DbContext).CreateTables(TableCreateMode.CreateNew);
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

                String_Length = (int?)a.Name.Length,
                Substring = a.Name.Substring(0),
                Substring1 = a.Name.Substring(1),
                Substring1_2 = a.Name.Substring(1, 2),
                ToLower = a.Name.ToLower(),
                ToUpper = a.Name.ToUpper(),
                IsNullOrEmpty = string.IsNullOrEmpty(a.Name),
                Contains = (bool?)a.Name.Contains("s"),
                StartsWith = (bool?)a.Name.StartsWith("s"),
                EndsWith = (bool?)a.Name.EndsWith("s"),
                Trim = a.Name.Trim(),
                TrimStart = a.Name.TrimStart(space),
                TrimEnd = a.Name.TrimEnd(space),
                Replace = a.Name.Replace("l", "L"),

                DiffYears = Sql.DiffYears(startTime, endTime),
                DiffMonths = Sql.DiffMonths(startTime, endTime),
                DiffDays = Sql.DiffDays(startTime, endTime),
                DiffHours = Sql.DiffHours(startTime, endTime),
                DiffMinutes = Sql.DiffMinutes(startTime, endTime),
                DiffSeconds = Sql.DiffSeconds(startTime, endTime),

                SubtractTotalDays = endTime.Subtract(startTime).TotalDays,
                SubtractTotalHours = endTime.Subtract(startTime).TotalHours,
                SubtractTotalMinutes = endTime.Subtract(startTime).TotalMinutes,
                SubtractTotalSeconds = endTime.Subtract(startTime).TotalSeconds,
                SubtractTotalMilliseconds = endTime.Subtract(startTime).TotalMilliseconds,

                AddYears = startTime.AddYears(1),
                AddMonths = startTime.AddMonths(1),
                AddDays = startTime.AddDays(1),
                AddHours = startTime.AddHours(1),
                AddMinutes = startTime.AddMinutes(2),
                AddSeconds = startTime.AddSeconds(120),

                Now = DateTime.Now,
                UtcNow = DateTime.UtcNow,
                Today = DateTime.Today,
                Date = DateTime.Now.Date,
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month,
                Day = DateTime.Now.Day,
                Hour = DateTime.Now.Hour,
                Minute = DateTime.Now.Minute,
                Second = DateTime.Now.Second,
                Millisecond = DateTime.Now.Millisecond,
                DayOfWeek = DateTime.Now.DayOfWeek,

                Byte_Parse = byte.Parse("1"),
                Int_Parse = int.Parse("1"),
                Int16_Parse = Int16.Parse("11"),
                Long_Parse = long.Parse("2"),
                Double_Parse = double.Parse("3.1"),
                Float_Parse = float.Parse("4.1"),
                Decimal_Parse = decimal.Parse("5"),
                Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),

                Bool_Parse = bool.Parse("1"),
                DateTime_Parse = DateTime.Parse("2014-01-01"),

                B = a.Age == null ? false : a.Age > 1,
                CaseWhen = Case.When(a.Id > 100).Then(1).Else(0)
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }

        public override void ExecuteCommandText()
        {
            List<Person> persons = this.DbContext.SqlQuery<Person>("select * from Person where Age > :age", DbParam.Create(":age", 1)).ToList();

            int rowsAffected = this.DbContext.Session.ExecuteNonQuery("update Person set name=:name where Id = 1", DbParam.Create(":name", "Chloe"));

            /* 
             * 执行存储过程:
             * Person person = this.DbContext.SqlQuery<Person>("Proc_GetPerson", CommandType.StoredProcedure, DbParam.Create("@id", 1)).FirstOrDefault();
             * rowsAffected = this.DbContext.Session.ExecuteNonQuery("Proc_UpdatePersonName", CommandType.StoredProcedure, DbParam.Create("@name", "Chloe"));
             */

            ConsoleHelper.WriteLineAndReadKey();
        }

    }

}

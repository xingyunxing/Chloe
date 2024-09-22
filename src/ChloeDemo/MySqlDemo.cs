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
using System.Diagnostics;

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
            base.Method();

            IQuery<Person> q = this.DbContext.Query<Person>();

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddDays(1);
            var result = q.Select(a => new
            {
                Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),//N'D544BC4C-739E-4CD3-A3D3-7BF803FCE179'
                FindInSetResult = MySqlFunctions.FindInSet("1", "1,2,3")
            }).ToList();

            Debug.Assert(result.First().FindInSetResult == true);

            ConsoleHelper.WriteLineAndReadKey("MySqlDemo.Method over...");
        }

    }

}

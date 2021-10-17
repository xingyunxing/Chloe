using Chloe;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.SQLite;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.DDL;
using Chloe.SQLite.DDL;

namespace ChloeDemo
{
    class SQLiteDemo : DemoBase
    {
        public SQLiteDemo()
        {
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));
        }

        protected override IDbContext CreateDbContext()
        {
            IDbContext dbContext = new SQLiteContext(new SQLiteConnectionFactory("Data Source=..\\..\\..\\Chloe.db;"));

            return dbContext;
        }

        public override void InitDatabase()
        {
            new SQLiteTableGenerator(this.DbContext).CreateTables(TableCreateMode.CreateNew);
        }
    }
}

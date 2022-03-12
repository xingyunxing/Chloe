using Chloe;
using Chloe.MySql;
using Chloe.Sharding;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChloeDemo.Sharding
{
    public class OrderRouteDbContextFactory : IRouteDbContextFactory
    {
        int _year;
        public OrderRouteDbContextFactory(int year)
        {
            this._year = year;
        }

        public IDbContext CreateDbContext()
        {
            string connString = $"Server=localhost;Port=3306;Database=order{this._year};Uid=root;Password=sasa;Charset=utf8; Pooling=True; Max Pool Size=200;Allow User Variables=True;SslMode=none;";

            MySqlContext dbContext = new MySqlContext(new MySqlConnectionFactory(connString));
            return dbContext;
        }
    }
}

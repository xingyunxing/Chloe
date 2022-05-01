using Chloe;
using Chloe.MySql;
using Chloe.Sharding;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChloeDemo.Sharding
{
    public class OrderDbContextProviderFactory : IDbContextProviderFactory
    {
        int _year;
        public OrderDbContextProviderFactory(int year)
        {
            this._year = year;
        }

        public IDbContextProvider CreateDbContextProvider()
        {
            string connString = $"Server=localhost;Port=3306;Database=order{this._year};Uid=root;Password=sasa;Charset=utf8; Pooling=True; Max Pool Size=200;Allow User Variables=True;SslMode=none;allowPublicKeyRetrieval=true";

            MySqlContextProvider dbContextProvider = new MySqlContextProvider(new MySqlConnectionFactory(connString));
            return dbContextProvider;
        }
    }
}

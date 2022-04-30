using Chloe.Infrastructure.Interception;
using Chloe.Routing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe
{
    class DataSourceDbContextPair
    {
        public DataSourceDbContextPair(IPhysicDataSource dataSource, IDbContextProvider dbContextProvider)
        {
            this.DataSource = dataSource;
            this.DbContextProvider = dbContextProvider;
        }

        public IPhysicDataSource DataSource { get; set; }
        public IDbContextProvider DbContextProvider { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Sharding
{
    public class RouteDataSource
    {
        public string Name { get; set; }
        public IRouteDbContextFactory DbContextFactory { get; set; }
    }
}

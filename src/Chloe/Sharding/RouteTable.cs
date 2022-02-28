using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Sharding
{
    public class RouteTable
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public RouteDataSource DataSource { get; set; }
    }
}

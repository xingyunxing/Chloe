using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Sharding
{
    class ShardingQueryContext
    {
        public ShardingQueryContext(ShardingDbContext dbContext)
        {
            this.DbContext = dbContext;
        }
        public ShardingDbContext DbContext { get; set; }
    }
}

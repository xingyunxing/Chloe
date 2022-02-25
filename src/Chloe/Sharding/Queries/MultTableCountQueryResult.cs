using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Sharding.Queries
{
    class MultTableCountQueryResult
    {
        public PhysicTable Table { get; set; }
        public int Count { get; set; }
    }
}

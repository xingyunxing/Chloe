using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Sharding.Queries
{
    class TableDataQueryModel<T>
    {
        public PhysicTable Table { get; set; }
        public DataQueryModel QueryModel { get; set; }

        public SingleTableDataQuery<T> Query { get; set; }
    }
}

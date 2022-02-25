using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Sharding
{
    internal class QueryExecuteResult<T>
    {
        public QueryExecuteResult()
        {

        }
        public QueryExecuteResult(int count, IFeatureEnumerable<T> result)
        {
            this.Count = count;
            this.Result = result;
        }

        public int Count { get; set; }
        public IFeatureEnumerable<T> Result { get; set; }
    }
}

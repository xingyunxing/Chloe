using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe
{
    public class PagingResult<T>
    {
        public int Count { get; set; }
        public List<T> DataList { get; set; }
    }
}

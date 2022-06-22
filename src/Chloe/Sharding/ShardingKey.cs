using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Chloe.Sharding
{
    class ShardingKey
    {
        public MemberInfo Member { get; set; }
        public object Value { get; set; }
    }
}

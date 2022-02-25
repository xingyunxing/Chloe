using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Chloe.Extensions;
using Chloe.Reflection;

namespace Chloe.Sharding
{
    public class OrderProperty
    {
        public MemberInfo Member { get; set; }
        public bool Ascending { get; set; }
        public MemberGetter ValueGetter { get; set; }
    }
}

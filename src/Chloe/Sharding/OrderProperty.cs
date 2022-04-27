using System.Reflection;
using Chloe.Reflection;

namespace Chloe.Sharding
{
    public class OrderProperty
    {
        public bool Ascending { get; set; }
        public MemberGetter ValueGetter { get; set; }
    }
}

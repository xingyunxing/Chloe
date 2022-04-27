using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    public class Ordering
    {
        public LambdaExpression KeySelector { get; set; }
        public bool Ascending { get; set; }
    }
}

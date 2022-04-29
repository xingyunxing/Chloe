using System.Linq.Expressions;

namespace Chloe.Sharding
{
    public class Ordering
    {
        public LambdaExpression KeySelector { get; set; }
        public bool Ascending { get; set; }
    }
}

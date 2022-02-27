using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    public class Ordering
    {
        /// <summary>
        /// 必须是属性成员，不然没办法在内存里计算排序值
        /// </summary>
        public MemberInfo Member { get; set; }
        public LambdaExpression KeySelector { get; set; }
        public bool Ascending { get; set; }
    }
}

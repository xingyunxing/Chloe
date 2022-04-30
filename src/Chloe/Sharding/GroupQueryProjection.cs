using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    public class GroupQueryProjection
    {
        public GroupQueryProjection()
        {

        }

        public ConstructorInfo Constructor { get; set; }
        public List<Expression> ConstructorArgExpressions { get; set; } = new List<Expression>();
        public List<Func<Func<object, object>, IEnumerable<object>, object>> ConstructorArgGetters { get; set; } = new List<Func<Func<object, object>, IEnumerable<object>, object>>();

        public List<Expression> MemberExpressions { get; set; } = new List<Expression>();
        public List<Action<Func<object, object>, IEnumerable<object>, object>> MemberBinders { get; set; } = new List<Action<Func<object, object>, IEnumerable<object>, object>>();
    }

    public class GroupKeyEqualityComparer : IEqualityComparer<object>
    {
        public GroupKeyEqualityComparer(List<Func<object, object>> groupKeyValueGetters)
        {
            this.GroupKeyValueGetters = groupKeyValueGetters;
        }

        public List<Func<object, object>> GroupKeyValueGetters { get; set; }

        public new bool Equals(object x, object y)
        {
            foreach (var valueGetter in this.GroupKeyValueGetters)
            {
                var keyValueX = valueGetter(x);
                var keyValueY = valueGetter(y);

                bool equal = PublicHelper.AreEqual(keyValueX, keyValueY);
                if (!equal)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(object obj)
        {
            int hash;
            unchecked
            {
                hash = (int)2166136261; // 不是素数
                foreach (var valueGetter in this.GroupKeyValueGetters)
                {
                    var keyValue = valueGetter(obj);

                    // 16777619是素数
                    hash = (hash * 16777619) ^ keyValue.GetHashCode();
                }
            }

            return hash;
        }
    }
}

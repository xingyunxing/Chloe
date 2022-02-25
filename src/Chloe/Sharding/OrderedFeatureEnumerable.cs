using Chloe.Reflection;
using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Chloe.Sharding
{
    /// <summary>
    /// 对数据源支持排序对比功能
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class OrderedFeatureEnumerable<T> : FeatureEnumerable<T>
    {
        IFeatureEnumerable<T> _enumerable;
        List<OrderProperty> _orders;

        public OrderedFeatureEnumerable(IFeatureEnumerable<T> enumerable, List<OrderProperty> orders)
        {
            this._enumerable = enumerable;
            this._orders = orders;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator()
        {
            return this.GetFeatureEnumerator(default(CancellationToken));
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<T>, IOrderedFeatureEnumerator<T>
        {
            OrderedFeatureEnumerable<T> _enumerable;
            List<OrderProperty> _orders;

            List<object> _orderValues;
            CancellationToken _cancellationToken;

            public Enumerator(OrderedFeatureEnumerable<T> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
                this._orders = enumerable._orders;
                this._orderValues = new List<object>(this._orders.Count);
            }

            public List<object> GetOrderValues()
            {
                return this._orderValues;
            }
            void SetOrderValues(bool hasNext)
            {
                if (!hasNext)
                {
                    this._orderValues.Clear();
                    return;
                }

                T current = this.Current;
                for (int i = 0; i < this._orders.Count; i++)
                {
                    var order = this._orders[i];

                    object orderValue = order.ValueGetter(current);
                    this._orderValues[i] = orderValue;
                }
            }

            public int CompareTo(IOrderedFeatureEnumerator<T> other)
            {
                var orderValues1 = this.GetOrderValues();
                var orderValues2 = other.GetOrderValues();

                for (int i = 0; i < this._orders.Count; i++)
                {
                    var order = this._orders[i];

                    object xOrderValue = orderValues1[i];
                    object yOrderValue = orderValues2[i];

                    int result = SafeCompareToWith((IComparable)xOrderValue, (IComparable)yOrderValue, order.Ascending);
                    if (result == 0)
                    {
                        continue;
                    }

                    return result;
                }

                return 0;
            }
            static int SafeCompareToWith(IComparable value, IComparable other, bool asc)
            {
                if (asc)
                    return SafeCompareTo(value, other);

                return SafeCompareTo(other, value);
            }
            static int SafeCompareTo(IComparable value, IComparable other)
            {
                if (null == value && null == other)
                {
                    return 0;
                }
                if (null == value)
                {
                    return -1;
                }
                if (null == other)
                {
                    return 1;
                }

                return value.CompareTo(other);
            }

            protected override async Task<IFeatureEnumerator<T>> CreateEnumerator(bool async)
            {
                return this._enumerable._enumerable.GetFeatureEnumerator(this._cancellationToken);
            }

            protected override async BoolResultTask MoveNext(bool @async)
            {
                bool hasNext = await base.MoveNext(@async);
                this.SetOrderValues(hasNext);
                return hasNext;
            }
        }
    }
}

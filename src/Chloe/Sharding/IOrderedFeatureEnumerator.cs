using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Sharding
{
    internal interface IOrderedFeatureEnumerator<T> : IFeatureEnumerator<T>, IComparable<IOrderedFeatureEnumerator<T>>
    {
        List<object> GetOrderValues();
    }
}

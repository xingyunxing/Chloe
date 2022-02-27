namespace Chloe.Sharding
{
    internal interface IOrderedFeatureEnumerator<T> : IFeatureEnumerator<T>, IComparable<IOrderedFeatureEnumerator<T>>
    {
        object[] GetOrderValues();
    }
}

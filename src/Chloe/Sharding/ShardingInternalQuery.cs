using System.Threading;

namespace Chloe.Sharding
{
    internal class ShardingInternalQuery<T> : FeatureEnumerable<T>
    {


        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

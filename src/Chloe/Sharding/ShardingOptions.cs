
namespace Chloe.Sharding
{
    public class ShardingOptions
    {
        public int MaxConnectionsPerDataSource { get; set; } = 12;
        public int MaxInItems { get; set; } = 1000;
    }
}

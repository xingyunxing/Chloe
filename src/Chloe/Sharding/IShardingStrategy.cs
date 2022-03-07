namespace Chloe.Sharding
{
    public interface IShardingStrategy
    {
        IEnumerable<RouteTable> ForEqual(object value);
        IEnumerable<RouteTable> ForNotEqual(object value);
        IEnumerable<RouteTable> ForGreaterThan(object value);
        IEnumerable<RouteTable> ForGreaterThanOrEqual(object value);
        IEnumerable<RouteTable> ForLessThan(object value);
        IEnumerable<RouteTable> ForLessThanOrEqual(object value);
    }

    public class ShardingStrategy : IShardingStrategy
    {
        public ShardingStrategy(IShardingRoute route)
        {
            this.Route = route;
        }

        public IShardingRoute Route { get; private set; }

        public virtual IEnumerable<RouteTable> ForEqual(object value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForGreaterThan(object value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForGreaterThanOrEqual(object value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForLessThan(object value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForLessThanOrEqual(object value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForNotEqual(object value)
        {
            return this.Route.GetTables();
        }
    }
    public class ShardingStrategy<TProperty> : IShardingStrategy
    {
        public ShardingStrategy(IShardingRoute route)
        {
            this.Route = route;
        }

        public IShardingRoute Route { get; private set; }

        public virtual IEnumerable<RouteTable> ForEqual(TProperty value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForGreaterThan(TProperty value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForGreaterThanOrEqual(TProperty value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForLessThan(TProperty value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForLessThanOrEqual(TProperty value)
        {
            return this.Route.GetTables();
        }

        public virtual IEnumerable<RouteTable> ForNotEqual(TProperty value)
        {
            return this.Route.GetTables();
        }

        IEnumerable<RouteTable> IShardingStrategy.ForEqual(object value)
        {
            return this.ForEqual((TProperty)value);
        }

        IEnumerable<RouteTable> IShardingStrategy.ForNotEqual(object value)
        {
            return this.ForNotEqual((TProperty)value);
        }

        IEnumerable<RouteTable> IShardingStrategy.ForGreaterThan(object value)
        {
            return this.ForGreaterThan((TProperty)value);
        }

        IEnumerable<RouteTable> IShardingStrategy.ForGreaterThanOrEqual(object value)
        {
            return this.ForGreaterThanOrEqual((TProperty)value);
        }

        IEnumerable<RouteTable> IShardingStrategy.ForLessThan(object value)
        {
            return this.ForLessThan((TProperty)value);
        }

        IEnumerable<RouteTable> IShardingStrategy.ForLessThanOrEqual(object value)
        {
            return this.ForLessThanOrEqual((TProperty)value);
        }
    }

    static class ShardingStrategyExtension
    {
        public static IEnumerable<RouteTable> GetTables(this IShardingStrategy shardingStrategy, object value, ShardingOperator shardingOperator)
        {
            switch (shardingOperator)
            {
                case ShardingOperator.Equal:
                    return shardingStrategy.ForEqual(value);
                case ShardingOperator.NotEqual:
                    return shardingStrategy.ForNotEqual(value);
                case ShardingOperator.GreaterThan:
                    return shardingStrategy.ForGreaterThan(value);
                case ShardingOperator.GreaterThanOrEqual:
                    return shardingStrategy.ForGreaterThanOrEqual(value);
                case ShardingOperator.LessThan:
                    return shardingStrategy.ForLessThan(value);
                case ShardingOperator.LessThanOrEqual:
                    return shardingStrategy.ForLessThanOrEqual(value);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}

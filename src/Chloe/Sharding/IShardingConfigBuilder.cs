using Chloe.Sharding.Routing;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    public interface IShardingConfigBuilder<T>
    {
        IShardingConfig Build();
        IShardingConfigBuilder<T> HasRoute(IShardingRoute route);
        IShardingConfigBuilder<T> HasRouteFactory(IShardingRouteFactory routeFactory);
        IShardingConfigBuilder<T> HasShardingKey<TShardingKey>(Expression<Func<T, TShardingKey>> keySelector);
    }

    public class ShardingConfigBuilder<T> : IShardingConfigBuilder<T>
    {
        IShardingRouteFactory _routeFactory;
        List<LambdaExpression> _shardingKeys = new List<LambdaExpression>();

        public IShardingConfigBuilder<T> HasRoute(IShardingRoute route)
        {
            return this.HasRouteFactory(() => route);
        }
        public IShardingConfigBuilder<T> HasRouteFactory(Func<IShardingRoute> routeFactory)
        {
            return this.HasRouteFactory(new ShardingRouteFactory(routeFactory));
        }
        public IShardingConfigBuilder<T> HasRouteFactory(IShardingRouteFactory routeFactory)
        {
            this._routeFactory = routeFactory;
            return this;
        }

        public IShardingConfigBuilder<T> HasShardingKey<TShardingKey>(Expression<Func<T, TShardingKey>> keySelector)
        {
            var memberExp = keySelector.Body as MemberExpression;

            if (memberExp == null || memberExp.Expression.NodeType != ExpressionType.Parameter)
            {
                throw new ArgumentException();
            }

            this._shardingKeys.Add(keySelector);
            return this;
        }

        public IShardingConfig Build()
        {
            var shardingConfig = new ShardingConfig();
            shardingConfig.EntityType = typeof(T);
            shardingConfig.RouteFactory = this._routeFactory;
            shardingConfig.ShardingKeys = this._shardingKeys.Select(a => (a.Body as MemberExpression).Member).ToList().AsReadOnly();

            return shardingConfig;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.Sharding
{
    public class ShardingConfigContainer
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, IShardingConfig> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, IShardingConfig>();

        public static IShardingConfig Get(Type entityType)
        {
            return InstanceCache[entityType];
        }

        public static IShardingConfig Find(Type entityType)
        {
            InstanceCache.TryGetValue(entityType, out var value);
            return value;
        }

        public static void Add(IShardingConfig config)
        {
            InstanceCache[config.EntityType] = config;
        }

        //public static void Add<TEntity>(Action<ShardingConfigOption> configureOptions)
        //{
        //    Add(typeof(TEntity), configureOptions);
        //}
        //public static void Add(Type entityType, Action<ShardingConfigOption> configureOptions)
        //{
        //    if (configureOptions == null)
        //        throw new ArgumentNullException(nameof(configureOptions));

        //    ShardingConfigOption option = new ShardingConfigOption();
        //    configureOptions(option);

        //    ShardingConfig shardingConfig = new ShardingConfig();
        //    shardingConfig.EntityType = entityType;
        //    shardingConfig.Route = option.Route;

        //    InstanceCache[shardingConfig.EntityType] = shardingConfig;
        //}
    }

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
        LambdaExpression _shardingKey;

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

            this._shardingKey = keySelector;
            return this;
        }

        public IShardingConfig Build()
        {
            var memberExp = this._shardingKey.Body as MemberExpression;

            ShardingConfig shardingConfig = new ShardingConfig();
            shardingConfig.EntityType = typeof(T);
            shardingConfig.RouteFactory = this._routeFactory;
            shardingConfig.ShardingKey = memberExp.Member;

            return shardingConfig;
        }
    }

    public class ShardingConfigOption
    {
        public IShardingRoute Route { get; set; }
    }

    public interface IShardingRouteFactory
    {
        IShardingRoute CreateRoute();
    }
    class ShardingRouteFactory : IShardingRouteFactory
    {
        Func<IShardingRoute> _routeFactory;

        public ShardingRouteFactory(Func<IShardingRoute> routeFactory)
        {
            this._routeFactory = routeFactory;
        }
        public IShardingRoute CreateRoute()
        {
            return this._routeFactory();
        }
    }

    public interface IShardingConfig
    {
        Type EntityType { get; }
        MemberInfo ShardingKey { get; }
        IShardingRouteFactory RouteFactory { get; }
    }

    internal class ShardingConfig : IShardingConfig
    {
        public Type EntityType { get; set; }
        public MemberInfo ShardingKey { get; set; }
        public IShardingRouteFactory RouteFactory { get; set; }
    }
}

using Chloe.Query.QueryExpressions;
using Chloe.Reflection;
using Chloe.Sharding.Enumerables;
using System.Linq.Expressions;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingAggregateQueryState : ShardingQueryStateBase
    {
        public ShardingAggregateQueryState(ShardingQueryStateBase prevQueryState, AggregateQueryExpression exp) : base(prevQueryState)
        {
            this.QueryExpression = exp;
        }

        public AggregateQueryExpression QueryExpression { get; set; }

        public override IFeatureEnumerable<object> CreateQuery()
        {
            if (this.QueryExpression.Method.Name == "Count")
            {
                return this.CreateFeatureEnumerable(typeof(CountQueryEnumerable<>), this, false);
            }

            if (this.QueryExpression.Method.Name == "LongCount")
            {
                return this.CreateFeatureEnumerable(typeof(CountQueryEnumerable<>), this, true);
            }

            if (this.QueryExpression.Method.Name == "Any")
            {
                return this.CreateFeatureEnumerable(typeof(AnyQueryEnumerable<>), this);
            }

            if (this.QueryExpression.Method.Name == "Average")
            {
                return new AverageQueryEnumerable(this);
            }

            if (this.QueryExpression.Method.Name == "Sum")
            {
                return new SumQueryEnumerable(this);
            }

            if (this.QueryExpression.Method.Name == "Max" || this.QueryExpression.Method.Name == "Min")
            {
                var resultType = (this.QueryExpression.Arguments[0] as LambdaExpression).Body.Type;
                var queryEnumerableType = typeof(MaxMinQueryEnumerable<,>).MakeGenericType(this.QueryModel.RootEntityType, resultType);
                var queryEnumerable = queryEnumerableType.GetConstructor(new Type[2] { typeof(ShardingAggregateQueryState), typeof(bool) }).FastCreateInstance(this, this.QueryExpression.Method.Name == "Max");
                return (IFeatureEnumerable<object>)queryEnumerable;
            }

            throw new NotImplementedException();
        }

        IFeatureEnumerable<object> CreateFeatureEnumerable(Type queryEnumerableType, params object[] parameters)
        {
            queryEnumerableType = queryEnumerableType.MakeGenericType(this.QueryModel.RootEntityType);
            var queryEnumerable = queryEnumerableType.GetConstructors()[0].FastCreateInstance(parameters);
            return (IFeatureEnumerable<object>)queryEnumerable;
        }
    }
}

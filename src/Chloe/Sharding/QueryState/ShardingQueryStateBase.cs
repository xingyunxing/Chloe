using Chloe.Query;
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;
using Chloe.Utility;
using System.Linq.Expressions;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingQueryStateBase : IQueryState
    {
        protected ShardingQueryStateBase(ShardingQueryContext context, ShardingQueryModel queryModel)
        {
            this.Context = context;
            this.QueryModel = queryModel;
        }

        public ShardingQueryContext Context { get; set; }
        public ShardingQueryModel QueryModel { get; set; }

        public virtual IQueryState Accept(WhereExpression exp)
        {
            throw new NotSupportedException($"Cannot call '{nameof(IQuery<object>.Where)}' method except root query.");
        }

        public virtual IQueryState Accept(OrderExpression exp)
        {
            throw new NotSupportedException("Cannot call sorting method except root query.");
        }

        public virtual IQueryState Accept(SelectExpression exp)
        {
            throw new NotSupportedException("Cannot call 'Select' method except root query.");
        }

        public virtual IQueryState Accept(SkipExpression exp)
        {
            throw new NotSupportedException("Cannot call 'Skip' method except root query.");
        }

        public virtual IQueryState Accept(TakeExpression exp)
        {
            throw new NotSupportedException("Cannot call 'Take' method except root query.");
        }

        public virtual IQueryState Accept(AggregateQueryExpression exp)
        {
            throw new NotSupportedException($"{exp.Method.Name}");
        }

        public virtual IQueryState Accept(GroupingQueryExpression exp)
        {
            throw new NotSupportedException();
        }

        public virtual IQueryState Accept(DistinctExpression exp)
        {
            throw new NotSupportedException($"{nameof(IQuery<object>.Distinct)}");
        }

        public virtual IQueryState Accept(IncludeExpression exp)
        {
            throw new NotSupportedException($"{nameof(IQuery<object>.Include)}");
        }

        public virtual IQueryState Accept(IgnoreAllFiltersExpression exp)
        {
            this.QueryModel.IgnoreAllFilters = true;
            return this;
        }

        public virtual IQueryState Accept(TrackingExpression exp)
        {
            this.QueryModel.IsTracking = true;
            return this;
        }

        public virtual MappingData GenerateMappingData()
        {
            throw new NotSupportedException();
        }

        public QueryModel ToFromQueryModel()
        {
            throw new NotSupportedException();
        }

        public JoinQueryResult ToJoinQueryResult(JoinType joinType, LambdaExpression conditionExpression, ScopeParameterDictionary scopeParameters, StringSet scopeTables, Func<string, string> tableAliasGenerator)
        {
            throw new NotSupportedException();
        }

        public virtual IFeatureEnumerable<object> CreateFeatureQuery()
        {
            throw new NotImplementedException();
        }
    }
}

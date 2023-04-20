using Chloe.Visitors;
using Chloe.Reflection;
using Chloe.Sharding.Models;
using Chloe.Sharding.Queries;
using Chloe.Sharding.Visitors;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    internal class GroupAggregateQueryEnumerable : FeatureEnumerable<object>
    {
        ShardingQueryPlan _queryPlan;

        public GroupAggregateQueryEnumerable(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<object>
        {
            GroupAggregateQueryEnumerable _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(GroupAggregateQueryEnumerable enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            ShardingQueryPlan QueryPlan { get { return this._enumerable._queryPlan; } }
            ShardingQueryModel QueryModel { get { return this.QueryPlan.QueryModel; } }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                List<LambdaExpression> groupKeySelectors = GroupKeySelectorPeeker.Peek(this.QueryModel.GroupKeySelectors);

                GroupQueryProjection queryProjection = GroupSelectorResolver.Resolve(this.QueryModel.Selector);

                List<Type> dynamicTypeProperties = new List<Type>(queryProjection.ConstructorArgExpressions.Count + queryProjection.MemberExpressions.Count + groupKeySelectors.Count);


                foreach (var constructorArgExpression in queryProjection.ConstructorArgExpressions)
                {
                    dynamicTypeProperties.Add(constructorArgExpression.Type);
                }

                foreach (var memberExpression in queryProjection.MemberExpressions)
                {
                    dynamicTypeProperties.Add(memberExpression.Type);
                }

                foreach (var groupKeySelector in groupKeySelectors)
                {
                    dynamicTypeProperties.Add(groupKeySelector.Body.Type);
                }

                DynamicType dynamicType = DynamicTypeContainer.Get(dynamicTypeProperties);

                List<MemberBinding> bindings = new List<MemberBinding>(dynamicTypeProperties.Count);

                var entityType = this.QueryModel.Selector.Parameters[0].Type;

                ParameterExpression parameterExpression = Expression.Parameter(entityType);

                int idx = 0;

                List<Func<object, object>> constructorArgGetters = new List<Func<object, object>>(queryProjection.ConstructorArgExpressions.Count);
                List<Func<object, object>> memberValueGetters = new List<Func<object, object>>(queryProjection.MemberExpressions.Count);
                List<Func<object, object>> groupKeyValueGetters = new List<Func<object, object>>(groupKeySelectors.Count);

                foreach (var constructorArgExpression in queryProjection.ConstructorArgExpressions)
                {
                    dynamicTypeProperties.Add(constructorArgExpression.Type);
                    var exp = ParameterExpressionReplacer.Replace(constructorArgExpression, parameterExpression);
                    MemberAssignment binding = Expression.Bind(dynamicType.Properties[idx].Property, exp);
                    bindings.Add(binding);

                    constructorArgGetters.Add(MakeGroupKeyValueGetter(dynamicType.Properties[idx]));
                    idx++;
                }

                foreach (var memberExpression in queryProjection.MemberExpressions)
                {
                    dynamicTypeProperties.Add(memberExpression.Type);
                    var exp = ParameterExpressionReplacer.Replace(memberExpression, parameterExpression);
                    MemberAssignment binding = Expression.Bind(dynamicType.Properties[idx].Property, exp);
                    bindings.Add(binding);

                    memberValueGetters.Add(MakeGroupKeyValueGetter(dynamicType.Properties[idx]));
                    idx++;
                }

                foreach (var groupKeySelector in groupKeySelectors)
                {
                    dynamicTypeProperties.Add(groupKeySelector.Body.Type);
                    var exp = ParameterExpressionReplacer.Replace(groupKeySelector.Body, parameterExpression);
                    MemberAssignment binding = Expression.Bind(dynamicType.Properties[idx].Property, exp);
                    bindings.Add(binding);

                    groupKeyValueGetters.Add(MakeGroupKeyValueGetter(dynamicType.Properties[idx]));
                    idx++;
                }

                LambdaExpression dynamicTypeSelector = this.MakeDynamicTypeSelector(dynamicType, bindings, parameterExpression);
                List<GroupAggregateQueryModel> queryModels = this.MakeQueryModels(dynamicTypeSelector, groupKeySelectors);
                ParallelConcatEnumerable<object> parallelConcatEnumerable = this.MakeQueryEnumerable(queryModels);
                var shardingResults = await parallelConcatEnumerable.ToListAsync(this._cancellationToken);

                List<object> groupResults = this.GroupShardingResults(shardingResults, queryProjection, groupKeyValueGetters, constructorArgGetters, memberValueGetters);

                var featureEnumerableAdapter = new FeatureEnumerableAdapter<object>(groupResults);
                return featureEnumerableAdapter.GetFeatureEnumerator();
            }

            LambdaExpression MakeDynamicTypeSelector(DynamicType dynamicType, List<MemberBinding> bindings, ParameterExpression parameterExpression)
            {
                // new DynamicType() { Property1 = a.xx, Property2 = a.xx ... }
                NewExpression newExp = Expression.New(dynamicType.Type);
                MemberInitExpression memberInitExpression = Expression.MemberInit(newExp, bindings);

                Type delType = typeof(Func<,>).MakeGenericType(parameterExpression.Type, dynamicType.Type);
                LambdaExpression dynamicTypeSelector = Expression.Lambda(delType, memberInitExpression, parameterExpression);
                return dynamicTypeSelector;
            }
            List<GroupAggregateQueryModel> MakeQueryModels(LambdaExpression dynamicTypeSelector, List<LambdaExpression> groupKeySelectors)
            {
                List<GroupAggregateQueryModel> queryModels = new List<GroupAggregateQueryModel>(this.QueryPlan.Tables.Count);

                for (int i = 0; i < this.QueryPlan.Tables.Count; i++)
                {
                    var table = this.QueryPlan.Tables[i];

                    GroupAggregateQueryModel groupAggregateQueryModel = new GroupAggregateQueryModel(this.QueryModel.RootEntityType);
                    groupAggregateQueryModel.Table = table;
                    groupAggregateQueryModel.Lock = this.QueryModel.Lock;
                    groupAggregateQueryModel.Conditions.AppendRange(this.QueryModel.Conditions);
                    groupAggregateQueryModel.Selector = dynamicTypeSelector;
                    groupAggregateQueryModel.GroupKeySelectors = groupKeySelectors;

                    queryModels.Add(groupAggregateQueryModel);
                }

                return queryModels;
            }
            ParallelConcatEnumerable<object> MakeQueryEnumerable(List<GroupAggregateQueryModel> queryModels)
            {
                ParallelQueryContext queryContext = new ParallelQueryContext(this.QueryPlan.ShardingContext);
                List<ShardTableGroupAggregateQuery> shardTableQueries = new List<ShardTableGroupAggregateQuery>(this.QueryPlan.Tables.Count);

                try
                {
                    foreach (var group in queryModels.GroupBy(a => a.Table.DataSource.Name))
                    {
                        int count = group.Count();

                        ISharedDbContextProviderPool dbContextProviderPool = queryContext.GetDbContextProviderPool(group.First().Table.DataSource);
                        bool lazyQuery = dbContextProviderPool.Size >= count;

                        foreach (var queryModel in group)
                        {
                            ShardTableGroupAggregateQuery shardTableQuery = new ShardTableGroupAggregateQuery(queryContext, dbContextProviderPool, queryModel, lazyQuery);
                            shardTableQueries.Add(shardTableQuery);
                        }
                    }

                    ParallelConcatEnumerable<object> parallelConcatEnumerable = new ParallelConcatEnumerable<object>(queryContext, shardTableQueries);

                    return parallelConcatEnumerable;
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }
            List<object> GroupShardingResults(List<object> shardingResults, GroupQueryProjection queryProjection, List<Func<object, object>> groupKeyValueGetters, List<Func<object, object>> constructorArgGetters, List<Func<object, object>> memberValueGetters)
            {
                GroupKeyEqualityComparer groupKeyEqualityComparer = new GroupKeyEqualityComparer(groupKeyValueGetters);

                var groups = shardingResults.GroupBy(a => a, groupKeyEqualityComparer).ToList();

                List<object> groupResults = new List<object>();

                foreach (var group in groups)
                {
                    List<object> args = new List<object>(constructorArgGetters.Count);
                    for (int i = 0; i < constructorArgGetters.Count; i++)
                    {
                        var constructorArgGetter = constructorArgGetters[i];

                        var arg = queryProjection.ConstructorArgGetters[i](constructorArgGetter, group);
                        args.Add(arg);
                    }

                    var groupResult = queryProjection.Constructor.FastCreateInstance(args.ToArray());

                    for (int i = 0; i < memberValueGetters.Count; i++)
                    {
                        var memberValueGetter = memberValueGetters[i];
                        queryProjection.MemberBinders[i](memberValueGetter, group, groupResult);
                    }

                    groupResults.Add(groupResult);
                }

                return groupResults;
            }

            static Func<object, object> MakeGroupKeyValueGetter(DynamicTypeProperty dynamicTypeProperty)
            {
                Func<object, object> func = instance =>
                {
                    return dynamicTypeProperty.Getter(instance);
                };

                return func;
            }

        }
    }
}

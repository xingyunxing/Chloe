using Chloe.Core.Visitors;
using Chloe.Reflection;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    internal class GroupAggregateQuery : FeatureEnumerable<object>
    {
        ShardingQueryPlan _queryPlan;

        public GroupAggregateQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : QueryFeatureEnumerator<object>
        {
            GroupAggregateQuery _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(GroupAggregateQuery enumerable, CancellationToken cancellationToken = default) : base(enumerable._queryPlan)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                ParallelQueryContext queryContext = new ParallelQueryContext();

                try
                {
                    var groupKeySelectors = GroupKeySelectorPeeker.Peek(this.QueryModel.GroupKeySelectors);

                    GroupQueryMapper queryMapper = GroupSelectorResolver.Resolve(this.QueryModel.Selector);

                    List<Type> dynamicTypeProperties = new List<Type>(queryMapper.ConstructorArgExpressions.Count + queryMapper.MemberExpressions.Count + groupKeySelectors.Count);


                    foreach (var constructorArgExpression in queryMapper.ConstructorArgExpressions)
                    {
                        dynamicTypeProperties.Add(constructorArgExpression.Type);
                    }

                    foreach (var memberExpression in queryMapper.MemberExpressions)
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

                    var parameterExpression = Expression.Parameter(entityType);

                    int idx = 0;

                    List<Func<object, object>> constructorArgGetters = new List<Func<object, object>>(queryMapper.ConstructorArgExpressions.Count);
                    List<Func<object, object>> memberValueGetters = new List<Func<object, object>>(queryMapper.MemberExpressions.Count);
                    List<Func<object, object>> groupKeyValueGetters = new List<Func<object, object>>(groupKeySelectors.Count);

                    foreach (var constructorArgExpression in queryMapper.ConstructorArgExpressions)
                    {
                        dynamicTypeProperties.Add(constructorArgExpression.Type);
                        var exp = ParameterExpressionReplacer.Replace(constructorArgExpression, parameterExpression);
                        MemberAssignment binding = Expression.Bind(dynamicType.Properties[idx].Property, exp);
                        bindings.Add(binding);

                        constructorArgGetters.Add(MakeGroupKeyValueGetter(dynamicType.Properties[idx]));
                        idx++;
                    }

                    foreach (var memberExpression in queryMapper.MemberExpressions)
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

                    // new DynamicType() { Property1 = a.xx, Property2 = a.xx ... }
                    NewExpression newExp = Expression.New(dynamicType.Type);
                    MemberInitExpression memberInitExpression = Expression.MemberInit(newExp, bindings);

                    var delType = typeof(Func<,>).MakeGenericType(entityType, dynamicType.Type);
                    var dynamicTypeSelector = Expression.Lambda(delType, memberInitExpression, parameterExpression);

                    List<GroupAggregateQueryModel> queryModels = new List<GroupAggregateQueryModel>(this.QueryPlan.Tables.Count);

                    for (int i = 0; i < this.QueryPlan.Tables.Count; i++)
                    {
                        var table = this.QueryPlan.Tables[i];

                        GroupAggregateQueryModel groupAggregateQueryModel = new GroupAggregateQueryModel(this.QueryModel.RootEntityType);
                        groupAggregateQueryModel.Table = table;
                        groupAggregateQueryModel.Conditions = this.QueryModel.Conditions;
                        groupAggregateQueryModel.Selector = dynamicTypeSelector;
                        groupAggregateQueryModel.GroupKeySelectors = groupKeySelectors;

                        queryModels.Add(groupAggregateQueryModel);
                    }

                    List<SingleTableGroupAggregateQuery> queries = new List<SingleTableGroupAggregateQuery>(this.QueryPlan.Tables.Count);

                    foreach (var group in queryModels.GroupBy(a => a.Table.DataSource.Name))
                    {
                        int count = group.Count();

                        ShareDbContextPool dbContextPool = ShardingHelpers.CreateDbContextPool(this.QueryPlan.ShardingContext, group.First().Table.DataSource, count);
                        queryContext.AddManagedResource(dbContextPool);

                        bool lazyQuery = dbContextPool.Size >= count;

                        foreach (var queryModel in group)
                        {
                            SingleTableGroupAggregateQuery singleTableGroupAggregateQuery = new Queries.SingleTableGroupAggregateQuery(dbContextPool, queryModel, lazyQuery);
                            queries.Add(singleTableGroupAggregateQuery);
                        }
                    }

                    ParallelConcatEnumerable<object> parallelConcatEnumerable = new ParallelConcatEnumerable<object>(queryContext, queries);

                    var results = await parallelConcatEnumerable.ToListAsync(this._cancellationToken);

                    GroupKeyEqualityComparer groupKeyEqualityComparer = new GroupKeyEqualityComparer(groupKeyValueGetters);

                    var groups = results.GroupBy(a => a, groupKeyEqualityComparer).ToList();

                    List<object> instances = new List<object>();

                    foreach (var group in groups)
                    {
                        List<object> args = new List<object>(constructorArgGetters.Count);
                        for (int i = 0; i < constructorArgGetters.Count; i++)
                        {
                            var constructorArgGetter = constructorArgGetters[i];

                            var arg = queryMapper.ConstructorArgGetters[i](constructorArgGetter, group);
                            args.Add(arg);
                        }

                        var instance = queryMapper.Constructor.FastCreateInstance(args.ToArray());

                        for (int i = 0; i < memberValueGetters.Count; i++)
                        {
                            var memberValueGetter = memberValueGetters[i];
                            queryMapper.MemberBinders[i](memberValueGetter, group, instance);
                        }

                        instances.Add(instance);
                    }

                    var featureEnumerableAdapter = new FeatureEnumerableAdapter<object>(instances);
                    return featureEnumerableAdapter.GetFeatureEnumerator();

                    throw new NotImplementedException();
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
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

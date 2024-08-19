using Chloe.Descriptors;
using Chloe.Extensions;
using Chloe.Reflection;
using Chloe.Sharding.Models;
using Chloe.Sharding.Queries;
using Chloe.Sharding.Routing;
using Chloe.Visitors;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    internal static class ShardingHelpers
    {
        public static IEnumerable<RouteTable> Intersect(IEnumerable<RouteTable> source1, IEnumerable<RouteTable> source2)
        {
            return source1.Intersect(source2, RouteTableEqualityComparer.Instance);
        }
        public static IEnumerable<RouteTable> Union(IEnumerable<RouteTable> source1, IEnumerable<RouteTable> source2)
        {
            return source1.Union(source2, RouteTableEqualityComparer.Instance);
        }

        public static bool IsParameterMemberAccess(IEnumerable<LambdaExpression> selectors)
        {
            foreach (var selector in selectors)
            {
                if (!IsParameterMemberAccess(selector))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool IsParameterMemberAccess(LambdaExpression selector)
        {
            var selectorBody = selector.Body;
            if (selectorBody.NodeType != ExpressionType.MemberAccess)
                return false;

            var memberExp = selectorBody as MemberExpression;
            if (memberExp.Expression != selector.Parameters[0])
                return false;

            return true;
        }
        public static MemberInfo PeekSortField(LambdaExpression selector)
        {
            var memberExp = selector.Body as MemberExpression;
            return memberExp.Member;
        }
        public static LambdaExpression ConditionCombine(IEnumerable<LambdaExpression> conditions)
        {
            ParameterExpression parameterExpression = null;
            Expression conditionBody = null;
            foreach (var condition in conditions)
            {
                if (parameterExpression == null)
                {
                    parameterExpression = condition.Parameters[0];
                    conditionBody = condition.Body;
                    continue;
                }

                var newBody = ParameterExpressionReplacer.Replace(condition.Body, parameterExpression);
                conditionBody = Expression.AndAlso(conditionBody, newBody);
            }

            if (conditionBody == null)
            {
                return null;
            }

            return Expression.Lambda(conditionBody, parameterExpression);
        }

        public static IEnumerable<(IPhysicDataSource DataSource, List<IPhysicTable> Tables)> GroupTables(IEnumerable<IPhysicTable> tables)
        {
            //TODO 对表排序
            var groupedTables = tables.GroupBy(a => a.DataSource.Name).Select(a => (a.First().DataSource, a.ToList()));
            return groupedTables;
        }
        public static IOrderedQuery<T> OrderBy<T>(this IQuery<T> q, Ordering ordering)
        {
            LambdaExpression keySelector = ordering.KeySelector;

            MethodInfo orderMethod;
            if (ordering.Ascending)
                orderMethod = typeof(IQuery<T>).GetMethod(nameof(IQuery<int>.OrderBy));
            else
                orderMethod = typeof(IQuery<T>).GetMethod(nameof(IQuery<int>.OrderByDesc));

            IOrderedQuery<T> orderedQuery = CallOrderMethod<T>(q, orderMethod, keySelector);
            return orderedQuery;
        }
        public static IOrderedQuery<T> ThenBy<T>(this IOrderedQuery<T> q, Ordering ordering)
        {
            LambdaExpression keySelector = ordering.KeySelector;

            MethodInfo orderMethod;
            if (ordering.Ascending)
                orderMethod = typeof(IOrderedQuery<T>).GetMethod(nameof(IOrderedQuery<int>.ThenBy));
            else
                orderMethod = typeof(IOrderedQuery<T>).GetMethod(nameof(IOrderedQuery<int>.ThenByDesc));

            IOrderedQuery<T> orderedQuery = CallOrderMethod<T>(q, orderMethod, keySelector);
            return orderedQuery;
        }
        public static IOrderedQuery<T> CallOrderMethod<T>(object q, MethodInfo orderMethod, LambdaExpression keySelector)
        {
            orderMethod = orderMethod.MakeGenericMethod(new Type[] { keySelector.Body.Type });
            IOrderedQuery<T> orderedQuery = (IOrderedQuery<T>)orderMethod.FastInvoke(q, new object[] { keySelector });
            return orderedQuery;
        }

        public static LambdaExpression MakeDynamicSelector(ShardingQueryPlan queryPlan, DynamicType dynamicType, TypeDescriptor entityTypeDescriptor, int tableIndex)
        {
            // a => new Dynamic() { P1 = a.Id, P2 = tableIndex, P3 = orderKeySelector1, P4 = orderKeySelector2... }

            var dynamicProperties = dynamicType.Properties;

            ParameterExpression parameter = Expression.Parameter(queryPlan.QueryModel.RootEntityType, "a");

            List<MemberBinding> bindings = new List<MemberBinding>();
            MemberAssignment keyBind = Expression.Bind(dynamicProperties[0].Property, Expression.MakeMemberAccess(parameter, entityTypeDescriptor.PrimaryKeys.First().Definition.Property));
            bindings.Add(keyBind);

            MemberAssignment tableIndexBind = Expression.Bind(dynamicProperties[1].Property, Expression.Constant(tableIndex));
            bindings.Add(tableIndexBind);

            ShardingQueryModel queryModel = queryPlan.QueryModel;
            for (int i = 0; i < queryModel.Orderings.Count; i++)
            {
                var ordering = queryModel.Orderings[i];
                var orderKeySelector = ParameterExpressionReplacer.Replace(ordering.KeySelector, parameter);
                MemberAssignment bind = Expression.Bind(dynamicProperties[i + 2].Property, (orderKeySelector as LambdaExpression).Body);
                bindings.Add(bind);
            }

            NewExpression newExp = Expression.New(dynamicType.Type);
            Expression lambdaBody = Expression.MemberInit(newExp, bindings);
            LambdaExpression selector = Expression.Lambda(typeof(Func<,>).MakeGenericType(queryPlan.QueryModel.RootEntityType, dynamicType.Type), lambdaBody, parameter);

            return selector;
        }

        public static QueryProjection MakeQueryProjection(ShardingQueryModel queryModel)
        {
            QueryProjection queryProjection = new QueryProjection(queryModel.RootEntityType);
            queryProjection.Lock = queryModel.Lock;
            queryProjection.IgnoreAllFilters = queryModel.IgnoreAllFilters;

            queryProjection.ExcludedFields.Capacity = queryModel.ExcludedFields.Count;
            queryProjection.ExcludedFields.AppendRange(queryModel.ExcludedFields);

            queryProjection.Conditions.Capacity = queryModel.Conditions.Count;
            queryProjection.Conditions.AppendRange(queryModel.Conditions);

            queryProjection.Orderings.Capacity = queryModel.Orderings.Count;
            queryProjection.Orderings.AppendRange(queryModel.Orderings);

            int? takeCount = null;

            if (queryModel.Take != null)
            {
                takeCount = (queryModel.Skip ?? 0) + queryModel.Take.Value;
            }
            queryProjection.Take = takeCount;

            if (queryModel.Orderings.Count == 0)
            {
                queryProjection.ResultMapper = a => a;
                queryProjection.Selector = queryModel.Selector;
                return queryProjection;
            }

            if (queryModel.Selector == null)
            {
                bool isParameterMemberAccess = IsParameterMemberAccess(queryModel.Orderings.Select(a => a.KeySelector));
                if (isParameterMemberAccess)
                {
                    queryProjection.ResultMapper = a => a;

                    for (int i = 0; i < queryModel.Orderings.Count; i++)
                    {
                        var ordering = queryModel.Orderings[i];
                        MemberInfo sortField = PeekSortField(ordering.KeySelector);
                        var orderValueGetter = MemberGetterContainer.Get(sortField);
                        OrderProperty orderProperty = new OrderProperty() { ValueGetter = orderValueGetter, Ascending = ordering.Ascending };
                        queryProjection.OrderProperties.Add(orderProperty);
                    }

                    return queryProjection;
                }
            }

            LambdaExpression selector = queryModel.Selector;

            ParameterExpression parameterExp;
            if (selector == null)
            {
                var delType = typeof(Func<,>).MakeGenericType(queryModel.RootEntityType, queryModel.RootEntityType);
                parameterExp = Expression.Parameter(queryModel.RootEntityType);
                selector = Expression.Lambda(delType, parameterExp, parameterExp);
            }

            List<Type> dynamicTypeProperties = new List<Type>(queryModel.Orderings.Count + 1);
            dynamicTypeProperties.Add(selector.Body.Type);
            dynamicTypeProperties.AddRange(queryModel.Orderings.Select(a => a.KeySelector.Body.Type));

            DynamicType dynamicType = DynamicTypeContainer.Get(dynamicTypeProperties);

            queryProjection.ResultMapper = dynamicType.Properties[0].Getter;

            parameterExp = selector.Parameters[0];
            List<MemberBinding> bindings = new List<MemberBinding>(dynamicTypeProperties.Count);
            MemberAssignment resultBind = Expression.Bind(dynamicType.Properties[0].Property, selector.Body);
            bindings.Add(resultBind);

            for (int i = 0; i < queryModel.Orderings.Count; i++)
            {
                var ordering = queryModel.Orderings[i];
                OrderProperty orderProperty = new OrderProperty() { ValueGetter = dynamicType.Properties[i + 1].Getter, Ascending = ordering.Ascending };
                queryProjection.OrderProperties.Add(orderProperty);

                var orderKeySelectorExp = ParameterExpressionReplacer.Replace(ordering.KeySelector.Body, parameterExp);
                MemberAssignment orderingBind = Expression.Bind(dynamicType.Properties[i + 1].Property, orderKeySelectorExp);
                bindings.Add(orderingBind);
            }

            NewExpression newExp = Expression.New(dynamicType.Type);
            Expression lambdaBody = Expression.MemberInit(newExp, bindings);
            LambdaExpression selectorLambda = Expression.Lambda(typeof(Func<,>).MakeGenericType(queryModel.RootEntityType, dynamicType.Type), lambdaBody, parameterExp);
            queryProjection.Selector = selectorLambda;

            return queryProjection;
        }

        public static List<TableDataQueryPlan> MakeEntityQueryPlans(QueryProjection queryProjection, List<KeyQueryResult> keyResults, TypeDescriptor typeDescriptor, int maxInItems)
        {
            List<TableDataQueryPlan> queryPlans = new List<TableDataQueryPlan>();

            var listConstructor = typeof(List<>).MakeGenericType(typeDescriptor.PrimaryKeys.First().PropertyType).GetConstructor(new Type[] { typeof(int) });
            InstanceCreator listCreator = InstanceCreatorContainer.Get(listConstructor);

            ParameterExpression parameter = Expression.Parameter(queryProjection.RootEntityType, "a");
            Expression keyMemberAccess = Expression.MakeMemberAccess(parameter, typeDescriptor.PrimaryKeys.First().Definition.Property);

            int maxBatchSize = 10000;
            int batchSize = maxInItems > maxBatchSize ? maxBatchSize : maxInItems;

            foreach (var keyResult in keyResults.Where(a => a.Keys.Count > 0))
            {
                List<List<object>> batches = Slice(keyResult.Keys, batchSize);

                foreach (var batch in batches)
                {
                    IList keyList = (IList)listCreator(batch.Count);
                    foreach (var inItem in batch)
                    {
                        keyList.Add(inItem);
                    }

                    /*
                     * 注：千万不能用 Expression.Constant(keyList) 包装 keyList，因为如果使用 ConstantExpression 包装变量，ExpressionEqualityComparer 计算表达式树时始终返回一个新的哈希值，会导致 QueryPlanContainer 的缓存无限暴涨。
                     * 同理，在任何时候都不要用 ConstantExpression 来包装你的变量
                     */
                    var keyListWrapper = ExpressionExtension.MakeWrapperAccess(keyList, keyList.GetType());
                    Expression containsCall = Expression.Call(keyListWrapper, keyList.GetType().GetMethod(nameof(List<int>.Contains)), keyMemberAccess);
                    Expression conditionBody = containsCall;

                    LambdaExpression condition = LambdaExpression.Lambda(typeof(Func<,>).MakeGenericType(queryProjection.RootEntityType, typeof(bool)), conditionBody, parameter);

                    DataQueryModel dataQueryModel = new DataQueryModel(queryProjection.RootEntityType);
                    dataQueryModel.Table = keyResult.Table;
                    dataQueryModel.Lock = queryProjection.Lock;
                    dataQueryModel.IgnoreAllFilters = true;

                    dataQueryModel.Orderings.Capacity = queryProjection.Orderings.Count;
                    dataQueryModel.Orderings.AppendRange(queryProjection.Orderings);

                    dataQueryModel.ExcludedFields.Capacity = queryProjection.ExcludedFields.Count;
                    dataQueryModel.ExcludedFields.AddRange(queryProjection.ExcludedFields);

                    dataQueryModel.Conditions.Add(condition);
                    dataQueryModel.Selector = queryProjection.Selector;

                    TableDataQueryPlan queryPlan = new TableDataQueryPlan();
                    queryPlan.QueryModel = dataQueryModel;

                    queryPlans.Add(queryPlan);
                }
            }

            return queryPlans;
        }

        public static DataQueryModel MakeDataQueryModel(IPhysicTable table, ShardingQueryModel queryModel)
        {
            int? takeCount = null;

            if (queryModel.Take != null)
            {
                takeCount = (queryModel.Skip ?? 0) + queryModel.Take.Value;
            }

            DataQueryModel dataQueryModel = MakeDataQueryModel(table, queryModel, null, takeCount);
            return dataQueryModel;
        }
        public static DataQueryModel MakeDataQueryModel(IPhysicTable table, ShardingQueryModel queryModel, int? skip, int? take)
        {
            DataQueryModel dataQueryModel = new DataQueryModel(queryModel.RootEntityType);
            dataQueryModel.Table = table;
            dataQueryModel.Lock = queryModel.Lock;
            dataQueryModel.IgnoreAllFilters = queryModel.IgnoreAllFilters;


            dataQueryModel.ExcludedFields.AppendRange(queryModel.ExcludedFields);
            dataQueryModel.Conditions.AppendRange(queryModel.Conditions);
            dataQueryModel.Orderings.AppendRange(queryModel.Orderings);
            dataQueryModel.Skip = skip;
            dataQueryModel.Take = take;
            dataQueryModel.Selector = queryModel.Selector;

            return dataQueryModel;
        }

        public static AggregateQuery<long> GetCountQuery(ShardingQueryPlan queryPlan)
        {
            Func<IQuery, bool, Task<long>> executor = async (query, @async) =>
            {
                long result = @async ? await query.LongCountAsync() : query.LongCount();
                return result;
            };

            var aggQuery = new AggregateQuery<long>(queryPlan, executor);
            return aggQuery;
        }

        public static IQuery MakeQuery(IDbContextProvider dbContextProvider, DataQueryModel queryModel)
        {
            Type entityType = queryModel.RootEntityType;
            MethodInfo method;

            if (queryModel.Selector == null)
            {
                method = typeof(ShardingHelpers).FindMethod(nameof(ShardingHelpers.MakeTypedQuery)).MakeGenericMethod(entityType);
            }
            else
            {
                method = typeof(ShardingHelpers).FindMethod(nameof(ShardingHelpers.MakeTypedQueryWithSelector)).MakeGenericMethod(entityType, queryModel.Selector.Body.Type);
            }

            var query = (IQuery)method.Invoke(null, new object[2] { dbContextProvider, queryModel });
            return query;
        }
        static IQuery<T> MakeTypedQuery<T>(IDbContextProvider dbContextProvider, DataQueryModel queryModel)
        {
            return MakeTypedQueryCore<T>(dbContextProvider, queryModel, false);
        }
        static IQuery<T> MakeTypedQueryCore<T>(IDbContextProvider dbContextProvider, DataQueryModel queryModel, bool ignoreSkipAndTake)
        {
            var q = dbContextProvider.Query<T>(queryModel.Table.Name, queryModel.Lock);

            foreach (var excludedField in queryModel.ExcludedFields)
            {
                q = q.Exclude(excludedField);
            }

            foreach (var condition in queryModel.Conditions)
            {
                q = q.Where((Expression<Func<T, bool>>)condition);
            }

            if (queryModel.IgnoreAllFilters)
            {
                q = q.IgnoreAllFilters();
            }

            IOrderedQuery<T> orderedQuery = null;
            foreach (var ordering in queryModel.Orderings)
            {
                if (orderedQuery == null)
                    orderedQuery = q.OrderBy(ordering);
                else
                    orderedQuery = orderedQuery.ThenBy(ordering);

                q = orderedQuery;
            }

            if (!ignoreSkipAndTake)
            {
                if (queryModel.Skip != null)
                {
                    q = q.Skip(queryModel.Skip.Value);
                }
                if (queryModel.Take != null)
                {
                    q = q.Take(queryModel.Take.Value);
                }
            }

            return q;
        }
        static IQuery<TResult> MakeTypedQueryWithSelector<T, TResult>(IDbContextProvider dbContextProvider, DataQueryModel queryModel)
        {
            var q = MakeTypedQueryCore<T>(dbContextProvider, queryModel, true);

            Expression<Func<T, TResult>> selector = (Expression<Func<T, TResult>>)queryModel.Selector;
            var query = q.Select<TResult>(selector);

            if (queryModel.Skip != null)
            {
                query = query.Skip(queryModel.Skip.Value);
            }
            if (queryModel.Take != null)
            {
                query = query.Take(queryModel.Take.Value);
            }

            return query;
        }

        public static LambdaExpression MakeAggregateSelector(LambdaExpression selector)
        {
            var parameterType = selector.Parameters[0].Type;
            Expression lambdaBody = ConvertToNewAggregateModelExpression(selector.Body);

            var parameterExp = Expression.Parameter(parameterType);
            lambdaBody = ParameterExpressionReplacer.Replace(lambdaBody, parameterExp);

            var delType = typeof(Func<,>).MakeGenericType(parameterType, typeof(AggregateModel));

            var lambda = Expression.Lambda(delType, lambdaBody, parameterExp);

            return lambda;
        }
        public static MemberInitExpression ConvertToNewAggregateModelExpression(Expression avgSelectorExp)
        {
            var fieldAccessExp = Expression.Convert(avgSelectorExp, typeof(decimal?));

            //Sql.Sum((decimal?)a.Amount)
            var Sql_Sum_Call = Expression.Call(PublicConstants.MethodInfo_Sql_Sum_DecimalN, fieldAccessExp);
            MemberAssignment sumBind = Expression.Bind(typeof(AggregateModel).GetProperty(nameof(AggregateModel.Sum)), Sql_Sum_Call);

            //Sql.LongCount<decimal?>((decimal?)a.Amount)
            var Sql_LongCount_Call = Expression.Call(PublicConstants.MethodInfo_Sql_LongCount.MakeGenericMethod(fieldAccessExp.Type), fieldAccessExp);
            MemberAssignment countBind = Expression.Bind(typeof(AggregateModel).GetProperty(nameof(AggregateModel.Count)), Sql_LongCount_Call);

            List<MemberBinding> bindings = new List<MemberBinding>(2);
            bindings.Add(sumBind);
            bindings.Add(countBind);

            // new AggregateModel() { Sum = Sql.Sum((decimal?)a.Amount), Count = Sql.LongCount<decimal?>((decimal?)a.Amount) }
            NewExpression newExp = Expression.New(typeof(AggregateModel));
            MemberInitExpression memberInitExpression = Expression.MemberInit(newExp, bindings);

            return memberInitExpression;
        }

        static List<List<object>> Slice(List<object> list, int batchSize)
        {
            if (list.Count <= batchSize)
            {
                return new List<List<object>>() { list };
            }

            List<List<object>> ret = new List<List<object>>();

            foreach (var item in list)
            {
                var lastList = ret.LastOrDefault();
                if (lastList == null)
                {
                    lastList = new List<object>();
                    ret.Add(lastList);
                }

                if (lastList.Count == batchSize)
                {
                    lastList = new List<object>();
                    ret.Add(lastList);
                }

                lastList.Add(item);
            }

            return ret;
        }
    }
}

using Chloe.Core.Visitors;
using Chloe.Descriptors;
using Chloe.Reflection;
using Chloe.Sharding.Queries;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    internal static class ShardingHelpers
    {
        public static LambdaExpression ConditionCombine(ShardingQueryModel queryModel)
        {
            IEnumerable<LambdaExpression> conditions = queryModel.Conditions;
            if (!queryModel.IgnoreAllFilters)
            {
                conditions = conditions.Concat(queryModel.GlobalFilters).Concat(queryModel.ContextFilters);
            }

            var condition = ConditionCombine(conditions);
            return condition;
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
            //var tables = routeTables.Select(a => (IPhysicTable)new PhysicTable(a));
            var groupedTables = tables.GroupBy(a => a.DataSource.Name).Select(a => (a.First().DataSource, a.ToList()));
            return groupedTables;

        }
        public static IOrderedQuery<T> InnerOrderBy<T>(this IQuery<T> q, Ordering ordering)
        {
            LambdaExpression keySelector = ordering.KeySelector;

            MethodInfo orderMethod;
            if (ordering.Ascending)
                orderMethod = typeof(IQuery<T>).GetMethod(nameof(IQuery<int>.OrderBy));
            else
                orderMethod = typeof(IQuery<T>).GetMethod(nameof(IQuery<int>.OrderByDesc));

            IOrderedQuery<T> orderedQuery = Invoke<T>(q, orderMethod, keySelector);
            return orderedQuery;
        }
        public static IOrderedQuery<T> InnerThenBy<T>(this IOrderedQuery<T> q, Ordering ordering)
        {
            LambdaExpression keySelector = ordering.KeySelector;

            MethodInfo orderMethod;
            if (ordering.Ascending)
                orderMethod = typeof(IOrderedQuery<T>).GetMethod(nameof(IOrderedQuery<int>.ThenBy));
            else
                orderMethod = typeof(IOrderedQuery<T>).GetMethod(nameof(IOrderedQuery<int>.ThenByDesc));

            IOrderedQuery<T> orderedQuery = Invoke<T>(q, orderMethod, keySelector);
            return orderedQuery;
        }
        public static IOrderedQuery<T> Invoke<T>(object q, MethodInfo orderMethod, LambdaExpression keySelector)
        {
            orderMethod = orderMethod.MakeGenericMethod(new Type[] { keySelector.Body.Type });
            IOrderedQuery<T> orderedQuery = (IOrderedQuery<T>)orderMethod.FastInvoke(q, new object[] { keySelector });
            return orderedQuery;
        }

        public static ShareDbContextPool CreateDbContextPool(IShardingContext shardingContext, IPhysicDataSource dataSource, int count)
        {
            List<IDbContext> dbContexts = shardingContext.CreateDbContextProviders(dataSource, count);
            ShareDbContextPool dbContextPool = new ShareDbContextPool(dbContexts);

            return dbContextPool;
        }

        public static LambdaExpression MakeDynamicSelector<TEntity>(ShardingQueryPlan queryPlan, DynamicType dynamicType, TypeDescriptor entityTypeDescriptor, int tableIndex)
        {
            // a => new Dynamic() { P1 = a.Id, P2 = tableIndex, P3 = orderKeySelector1, P4 = orderKeySelector2... }

            var dynamicProperties = dynamicType.Properties;

            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "a");

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
            LambdaExpression selector = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(TEntity), dynamicType.Type), lambdaBody, parameter);

            return selector;
        }

        public static List<TableDataQueryPlan<TEntity>> MakeEntityQueryPlans<TEntity>(ShardingQueryModel queryModel, List<MultTableKeyQueryResult> keyResults, TypeDescriptor typeDescriptor, int maxInItems)
        {
            List<TableDataQueryPlan<TEntity>> queryPlans = new List<TableDataQueryPlan<TEntity>>();

            var listConstructor = typeof(List<>).MakeGenericType(typeDescriptor.PrimaryKeys.First().PropertyType).GetConstructor(new Type[] { typeof(int) });
            InstanceCreator listCreator = InstanceCreatorContainer.Get(listConstructor);

            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "a");
            Expression keyMemberAccess = Expression.MakeMemberAccess(parameter, typeDescriptor.PrimaryKeys.First().Definition.Property);

            foreach (var keyResult in keyResults.Where(a => a.Keys.Count > 0))
            {
                List<List<object>> batches = Slice(keyResult.Keys, maxInItems);

                foreach (var batch in batches)
                {
                    IList keyList = (IList)listCreator(batch.Count);
                    foreach (var inItem in batch)
                    {
                        keyList.Add(inItem);
                    }

                    Expression containsCall = Expression.Call(Expression.Constant(keyList), keyList.GetType().GetMethod(nameof(List<int>.Contains)), keyMemberAccess);
                    Expression conditionBody = containsCall;

                    LambdaExpression condition = LambdaExpression.Lambda(typeof(Func<TEntity, bool>), conditionBody, parameter);

                    DataQueryModel dataQueryModel = new DataQueryModel();
                    dataQueryModel.Table = keyResult.Table;
                    dataQueryModel.IgnoreAllFilters = true;
                    dataQueryModel.Orderings.AddRange(queryModel.Orderings);
                    dataQueryModel.Conditions.Add(condition);

                    TableDataQueryPlan<TEntity> queryPlan = new TableDataQueryPlan<TEntity>();
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
            DataQueryModel dataQueryModel = new DataQueryModel();
            dataQueryModel.Table = table;
            dataQueryModel.IgnoreAllFilters = queryModel.IgnoreAllFilters;
            dataQueryModel.Conditions.AddRange(queryModel.Conditions);
            dataQueryModel.Orderings.AddRange(queryModel.Orderings);
            dataQueryModel.Skip = skip;
            dataQueryModel.Take = take;

            return dataQueryModel;
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

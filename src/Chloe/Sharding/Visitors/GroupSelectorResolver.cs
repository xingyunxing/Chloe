﻿using Chloe.Core.Visitors;
using Chloe.Extensions;
using Chloe.Reflection;
using Chloe.Sharding.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding.Visitors
{
    class GroupSelectorResolver : ExpressionVisitor<GroupQueryMapper>
    {
        public static readonly GroupSelectorResolver Instance = new GroupSelectorResolver();

        GroupSelectorResolver()
        {

        }


        public static GroupQueryMapper Resolve(Expression exp)
        {
            return Instance.Visit(exp);
        }

        public override GroupQueryMapper Visit(Expression exp)
        {
            if (exp == null)
                return default;
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                default:
                    throw new NotImplementedException();
            }
        }

        protected override GroupQueryMapper VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }
        protected override GroupQueryMapper VisitNew(NewExpression exp)
        {
            GroupQueryMapper groupQueryMapper = new GroupQueryMapper();
            groupQueryMapper.Constructor = exp.Constructor;

            foreach (Expression argExp in exp.Arguments)
            {
                //去掉一些类型转换 (double?)Sql.Average(a.Amout)
                var fixedExp = argExp.StripConvert();
                if (fixedExp.NodeType == ExpressionType.Call)
                {
                    MethodCallExpression callExp = (MethodCallExpression)fixedExp;
                    if (callExp.Method.DeclaringType == typeof(Sql))
                    {
                        if (callExp.Method.Name == nameof(Sql.Average))
                        {
                            var newAggregateModelExpression = ShardingHelpers.ConvertToNewAggregateModelExpression(callExp.Arguments[0]);
                            groupQueryMapper.ConstructorArgExpressions.Add(newAggregateModelExpression);
                            groupQueryMapper.ConstructorArgGetters.Add(MakeAvgGetter(argExp.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.Count))
                        {
                            groupQueryMapper.ConstructorArgExpressions.Add(callExp);
                            groupQueryMapper.ConstructorArgGetters.Add(MakeCountGetter(argExp.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.LongCount))
                        {
                            groupQueryMapper.ConstructorArgExpressions.Add(callExp);
                            groupQueryMapper.ConstructorArgGetters.Add(MakeLongCountGetter(argExp.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.Sum))
                        {
                            groupQueryMapper.ConstructorArgExpressions.Add(callExp);
                            groupQueryMapper.ConstructorArgGetters.Add(MakeSumGetter(argExp.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.Max))
                        {
                            groupQueryMapper.ConstructorArgExpressions.Add(callExp);
                            groupQueryMapper.ConstructorArgGetters.Add(MakeMaxGetter(argExp.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.Min))
                        {
                            groupQueryMapper.ConstructorArgExpressions.Add(callExp);
                            groupQueryMapper.ConstructorArgGetters.Add(MakeMinGetter(argExp.Type));

                            continue;
                        }
                    }
                }

                groupQueryMapper.ConstructorArgExpressions.Add(argExp);
                groupQueryMapper.ConstructorArgGetters.Add(MakeGetter());
            }

            return groupQueryMapper;
        }
        protected override GroupQueryMapper VisitMemberInit(MemberInitExpression exp)
        {
            GroupQueryMapper groupQueryMapper = this.Visit(exp.NewExpression);

            foreach (MemberBinding binding in exp.Bindings)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }

                MemberAssignment memberAssignment = (MemberAssignment)binding;
                MemberInfo member = memberAssignment.Member;
                Type memberType = member.GetMemberType();

                MemberSetter memberSetter = MemberSetterContainer.Get(member);

                //去掉一些类型转换 (double?)Sql.Average(a.Amout)
                var fixedExp = memberAssignment.Expression.StripConvert();
                if (fixedExp.NodeType == ExpressionType.Call)
                {
                    MethodCallExpression callExp = (MethodCallExpression)fixedExp;
                    if (callExp.Method.DeclaringType == typeof(Sql))
                    {
                        if (callExp.Method.Name == nameof(Sql.Average))
                        {
                            var newAggregateModelExpression = ShardingHelpers.ConvertToNewAggregateModelExpression(callExp.Arguments[0]);
                            groupQueryMapper.MemberExpressions.Add(newAggregateModelExpression);
                            groupQueryMapper.MemberBinders.Add(MakeAvgMemberBinder(memberSetter, memberAssignment.Expression.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.Count))
                        {
                            groupQueryMapper.MemberExpressions.Add(callExp);
                            groupQueryMapper.MemberBinders.Add(MakeCountMemberBinder(memberSetter, memberAssignment.Expression.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.LongCount))
                        {
                            groupQueryMapper.MemberExpressions.Add(callExp);
                            groupQueryMapper.MemberBinders.Add(MakeLongCountMemberBinder(memberSetter, memberAssignment.Expression.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.Sum))
                        {
                            groupQueryMapper.MemberExpressions.Add(callExp);
                            groupQueryMapper.MemberBinders.Add(MakeSumMemberBinder(memberSetter, memberAssignment.Expression.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.Max))
                        {
                            groupQueryMapper.MemberExpressions.Add(callExp);
                            groupQueryMapper.MemberBinders.Add(MakeMaxMemberBinder(memberSetter, memberAssignment.Expression.Type));

                            continue;
                        }

                        if (callExp.Method.Name == nameof(Sql.Min))
                        {
                            groupQueryMapper.MemberExpressions.Add(callExp);
                            groupQueryMapper.MemberBinders.Add(MakeMinMemberBinder(memberSetter, memberAssignment.Expression.Type));

                            continue;
                        }
                    }
                }

                groupQueryMapper.MemberExpressions.Add(memberAssignment.Expression);
                groupQueryMapper.MemberBinders.Add(MakeMemberBinder(memberSetter));
            }

            return groupQueryMapper;
        }

        static Func<Func<object, object>, IEnumerable<object>, object> MakeGetter()
        {
            Func<Func<object, object>, IEnumerable<object>, object> getter = (valueGetter, group) =>
            {
                var instance = group.First();
                var arg = valueGetter(instance);
                return arg;
            };

            return getter;
        }
        static Func<Func<object, object>, IEnumerable<object>, object> MakeCountGetter(Type targetType)
        {
            Func<Func<object, object>, IEnumerable<object>, object> getter = (valueGetter, group) =>
            {
                var tableCounts = group.Select(a => Convert.ToInt32(valueGetter(a)));
                int count = tableCounts.Sum();

                if (targetType == typeof(int))
                {
                    return (int)count;
                }

                if (targetType == typeof(long))
                {
                    return (long)count;
                }

                if (targetType == typeof(double))
                {
                    return (double)count;
                }

                if (targetType == typeof(float))
                {
                    return (float)count;
                }

                if (targetType == typeof(decimal))
                {
                    return (decimal)count;
                }

                return count;
            };

            return getter;
        }
        static Func<Func<object, object>, IEnumerable<object>, object> MakeLongCountGetter(Type targetType)
        {
            Func<Func<object, object>, IEnumerable<object>, object> getter = (valueGetter, group) =>
            {
                var tableCounts = group.Select(a => Convert.ToInt64(valueGetter(a)));
                long count = tableCounts.Sum();

                if (targetType == typeof(int))
                {
                    return (int)count;
                }

                if (targetType == typeof(long))
                {
                    return (long)count;
                }

                if (targetType == typeof(double))
                {
                    return (double)count;
                }

                if (targetType == typeof(float))
                {
                    return (float)count;
                }

                if (targetType == typeof(decimal))
                {
                    return (decimal)count;
                }

                return count;
            };

            return getter;
        }
        static Func<Func<object, object>, IEnumerable<object>, object> MakeSumGetter(Type targetType)
        {
            Func<Func<object, object>, IEnumerable<object>, object> getter = (valueGetter, group) =>
            {
                decimal? sum = null;

                var sums = group.Select(a => (decimal?)valueGetter(a));

                sum = sums.Sum();

                if (sum == null)
                    return null;

                targetType = targetType.GetUnderlyingType();
                if (targetType == typeof(decimal))
                {
                    return sum;
                }

                if (targetType == typeof(int))
                {
                    return (int)sum;
                }

                if (targetType == typeof(long))
                {
                    return (long)sum;
                }

                if (targetType == typeof(double))
                {
                    return (double)sum;
                }

                if (targetType == typeof(float))
                {
                    return (float)sum;
                }

                return sum;
            };

            return getter;
        }
        static Func<Func<object, object>, IEnumerable<object>, object> MakeAvgGetter(Type targetType)
        {
            Func<Func<object, object>, IEnumerable<object>, object> argGetter = (valueGetter, group) =>
            {
                decimal? sum = null;
                long count = 0;

                var aggModels = group.Select(a => valueGetter(a) as AggregateModel);

                foreach (AggregateModel aggregateModel in aggModels)
                {
                    if (aggregateModel.Sum == null)
                        continue;

                    sum = (sum ?? 0) + aggregateModel.Sum.Value;
                    count = count + aggregateModel.Count;
                }

                if (sum == null)
                    return null;

                decimal avg = sum.Value / count;

                targetType = targetType.GetUnderlyingType();
                if (targetType == typeof(decimal))
                {
                    return avg;
                }

                if (targetType == typeof(double))
                {
                    return (double)avg;
                }

                if (targetType == typeof(float))
                {
                    return (float)avg;
                }

                //???
                return avg;
            };

            return argGetter;
        }
        static Func<Func<object, object>, IEnumerable<object>, object> MakeMaxGetter(Type targetType)
        {
            Func<Func<object, object>, IEnumerable<object>, object> getter = (valueGetter, group) =>
            {
                var values = group.Select(a => valueGetter(a));
                var max = values.Max(value => value);
                return max;
            };

            return getter;
        }
        static Func<Func<object, object>, IEnumerable<object>, object> MakeMinGetter(Type targetType)
        {
            Func<Func<object, object>, IEnumerable<object>, object> getter = (valueGetter, group) =>
            {
                var values = group.Select(a => valueGetter(a));
                var min = values.Min(value => value);
                return min;
            };

            return getter;
        }


        static Action<Func<object, object>, IEnumerable<object>, object> MakeMemberBinder(MemberSetter memberSetter)
        {
            var getter = MakeGetter();
            Action<Func<object, object>, IEnumerable<object>, object> argGetter = (valueGetter, group, instance) =>
            {
                var value = getter(valueGetter, group);
                memberSetter(instance, value);
            };

            return argGetter;
        }
        static Action<Func<object, object>, IEnumerable<object>, object> MakeCountMemberBinder(MemberSetter memberSetter, Type targetType)
        {
            var getter = MakeCountGetter(targetType);
            Action<Func<object, object>, IEnumerable<object>, object> binder = (valueGetter, group, instance) =>
            {
                object value = getter(valueGetter, group);
                memberSetter(instance, value);
            };

            return binder;
        }
        static Action<Func<object, object>, IEnumerable<object>, object> MakeLongCountMemberBinder(MemberSetter memberSetter, Type targetType)
        {
            var getter = MakeLongCountGetter(targetType);
            Action<Func<object, object>, IEnumerable<object>, object> binder = (valueGetter, group, instance) =>
            {
                object value = getter(valueGetter, group);
                memberSetter(instance, value);
            };

            return binder;
        }
        static Action<Func<object, object>, IEnumerable<object>, object> MakeSumMemberBinder(MemberSetter memberSetter, Type targetType)
        {
            var getter = MakeSumGetter(targetType);
            Action<Func<object, object>, IEnumerable<object>, object> binder = (valueGetter, group, instance) =>
            {
                object value = getter(valueGetter, group);
                memberSetter(instance, value);
            };

            return binder;
        }
        static Action<Func<object, object>, IEnumerable<object>, object> MakeAvgMemberBinder(MemberSetter memberSetter, Type targetType)
        {
            var getter = MakeAvgGetter(targetType);

            Action<Func<object, object>, IEnumerable<object>, object> binder = (valueGetter, group, instance) =>
            {
                object value = getter(valueGetter, group);
                memberSetter(instance, value);
            };

            return binder;
        }
        static Action<Func<object, object>, IEnumerable<object>, object> MakeMaxMemberBinder(MemberSetter memberSetter, Type targetType)
        {
            var getter = MakeMaxGetter(targetType);
            Action<Func<object, object>, IEnumerable<object>, object> binder = (valueGetter, group, instance) =>
            {
                object value = getter(valueGetter, group);
                memberSetter(instance, value);
            };

            return binder;
        }
        static Action<Func<object, object>, IEnumerable<object>, object> MakeMinMemberBinder(MemberSetter memberSetter, Type targetType)
        {
            var getter = MakeMinGetter(targetType);
            Action<Func<object, object>, IEnumerable<object>, object> binder = (valueGetter, group, instance) =>
            {
                object value = getter(valueGetter, group);
                memberSetter(instance, value);
            };

            return binder;
        }
    }
}
using Chloe.Core.Visitors;
using Chloe.Extensions;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding.Visitors
{
    internal class ShardingTableDiscoverer : ExpressionVisitor<IEnumerable<RouteTable>>
    {
        public ShardingTableDiscoverer(IShardingContext shardingContext)
        {
            this.ShardingContext = shardingContext;
        }

        IShardingContext ShardingContext { get; set; }

        public static IEnumerable<RouteTable> GetRouteTables(Expression condition, IShardingContext shardingContext)
        {
            if (condition == null)
                return shardingContext.GetTables();

            ShardingTableDiscoverer peeker = new ShardingTableDiscoverer(shardingContext);
            return peeker.Visit(condition);
        }
        public static IEnumerable<RouteTable> GetRouteTables(IEnumerable<Expression> conditions, IShardingContext shardingContext)
        {
            ShardingTableDiscoverer peeker = new ShardingTableDiscoverer(shardingContext);

            IEnumerable<RouteTable> retRouteTables = null;
            foreach (var condition in conditions)
            {
                IEnumerable<RouteTable> routeTables = peeker.Visit(condition);
                if (retRouteTables == null)
                {
                    retRouteTables = routeTables;
                    continue;
                }

                retRouteTables = Intersect(retRouteTables, routeTables);
            }

            if (retRouteTables == null)
            {
                retRouteTables = shardingContext.GetTables();
            }

            return retRouteTables;
        }

        protected override IEnumerable<RouteTable> VisitExpression(Expression exp)
        {
            return this.ShardingContext.GetTables();
        }

        protected override IEnumerable<RouteTable> VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }

        IEnumerable<RouteTable> VisitComparison(BinaryExpression exp, ShardingOperator shardingOperator, ShardingOperator inversiveShardingOperator)
        {
            MemberInfo member = null;
            IRoutingStrategy routingStrategy = this.GetRoutingStrategy(exp.Left, out member);

            if (routingStrategy != null)
            {
                //TODO: 考虑是否可以翻译成sql的情况
                // a.CreateTime == ???
                if (exp.Right.IsEvaluable())
                {
                    // a.CreateTime == dt
                    object value = exp.Right.Evaluate();

                    if (value == null)
                    {
                        return base.VisitBinary(exp);
                    }

                    return routingStrategy.GetTables(value, shardingOperator);
                }
            }

            routingStrategy = this.GetRoutingStrategy(exp.Right, out member);
            if (routingStrategy != null)
            {
                // ??? == a.CreateTime
                if (exp.Left.IsEvaluable())
                {
                    // dt == a.CreateTime
                    object value = exp.Left.Evaluate();
                    if (value == null)
                    {
                        return base.VisitBinary(exp);
                    }

                    return routingStrategy.GetTables(value, inversiveShardingOperator);
                }
            }

            return base.VisitBinary(exp);
        }

        protected override IEnumerable<RouteTable> VisitBinary(BinaryExpression exp)
        {
            //TODO 考虑 Equal 方法
            switch (exp.NodeType)
            {
                case ExpressionType.LessThan:
                    return this.VisitComparison(exp, ShardingOperator.LessThan, ShardingOperator.GreaterThan);
                case ExpressionType.LessThanOrEqual:
                    return this.VisitComparison(exp, ShardingOperator.LessThanOrEqual, ShardingOperator.GreaterThanOrEqual);
                case ExpressionType.GreaterThan:
                    return this.VisitComparison(exp, ShardingOperator.GreaterThan, ShardingOperator.LessThan);
                case ExpressionType.GreaterThanOrEqual:
                    return this.VisitComparison(exp, ShardingOperator.GreaterThanOrEqual, ShardingOperator.LessThanOrEqual);
                case ExpressionType.Equal:
                    return this.VisitComparison(exp, ShardingOperator.Equal, ShardingOperator.Equal);
                case ExpressionType.NotEqual:
                    return this.VisitComparison(exp, ShardingOperator.NotEqual, ShardingOperator.NotEqual);
                default:
                    return base.VisitBinary(exp);
            }
        }

        protected override IEnumerable<RouteTable> VisitBinary_AndAlso(BinaryExpression exp)
        {
            return Intersect(this.Visit(exp.Left), this.Visit(exp.Right));
        }
        protected override IEnumerable<RouteTable> VisitBinary_OrElse(BinaryExpression exp)
        {
            return Union(this.Visit(exp.Left), this.Visit(exp.Right));
        }

        protected override IEnumerable<RouteTable> VisitMethodCall(MethodCallExpression exp)
        {
            // List.Contains(a.CreateTime)
            if (PublicHelper.Is_List_Contains_MethodCall(exp))
            {
                return this.Handle_List_Contains_MethodCall(exp);
            }

            // Enumerable.Contains(list, a.CreateTime)
            if (PublicHelper.Is_Enumerable_Contains_MethodCall(exp))
            {
                return this.Handle_Enumerable_Contains_MethodCall(exp);
            }

            // Sql.Equals(a.CreateTime, dt)
            if (PublicHelper.Is_Sql_Equals_MethodCall(exp))
            {
                return this.Handle_Sql_Equals_MethodCall(exp);
            }

            // Sql.NotEquals(a.CreateTime, dt)
            if (PublicHelper.Is_Sql_NotEquals_MethodCall(exp))
            {
                return this.Handle_Sql_NotEquals_MethodCall(exp);
            }

            // Sql.Compare(a.CreateTime, compareType, dt)
            if (PublicHelper.Is_Sql_Compare_MethodCall(exp))
            {
                return this.Handle_Sql_Compare_MethodCall(exp);
            }

            // a.CreateTime.Equals(dt)
            if (PublicHelper.Is_Instance_Equals_MethodCall(exp))
            {
                return this.Handle_Instance_Equals_MethodCall(exp);
            }

            // public static bool In<T>(this T obj, IEnumerable<T> source)
            if (PublicHelper.Is_In_Extension_MethodCall(exp))
            {
                return this.Handle_In_Extension_MethodCall(exp);
            }

            return base.VisitMethodCall(exp);
        }

        IEnumerable<RouteTable> GetRouteTables(IEnumerable list, IRoutingStrategy routingStrategy)
        {
            IEnumerable<RouteTable> ret = null;
            foreach (var item in list)
            {
                IEnumerable<RouteTable> routeTables = null;
                if (item == null)
                {
                    routeTables = this.ShardingContext.Route.GetTables();
                }
                else
                {
                    routeTables = routingStrategy.GetTables(item, ShardingOperator.Equal);
                }

                if (ret == null)
                {
                    ret = routeTables;
                    continue;
                }

                ret = Union(ret, routeTables);
            }

            return ret;
        }


        IRoutingStrategy GetRoutingStrategy(Expression exp, out MemberInfo member)
        {
            member = null;

            exp = exp.StripConvert();
            if (exp.NodeType != ExpressionType.MemberAccess)
            {
                return null;
            }

            var memberExp = exp as MemberExpression;
            if (memberExp.Expression.NodeType == ExpressionType.Parameter)
            {
                // a.CreateTime
                member = memberExp.Member;
                return this.ShardingContext.Route.GetStrategy(member);
            }

            return null;
        }

        static IEnumerable<RouteTable> Intersect(IEnumerable<RouteTable> source1, IEnumerable<RouteTable> source2)
        {
            return source1.Intersect(source2, RouteTableEqualityComparer.Instance);
        }
        static IEnumerable<RouteTable> Union(IEnumerable<RouteTable> source1, IEnumerable<RouteTable> source2)
        {
            return source1.Union(source2, RouteTableEqualityComparer.Instance);
        }

        IEnumerable<RouteTable> Handle_List_Contains_MethodCall(MethodCallExpression exp)
        {
            MemberInfo member = null;
            IRoutingStrategy routingStrategy = this.GetRoutingStrategy(exp.Arguments[0], out member);
            if (routingStrategy == null)
            {
                return base.VisitMethodCall(exp);
            }

            if (!exp.Object.IsEvaluable())
            {
                return base.VisitMethodCall(exp);
            }

            IList list = (IList)exp.Object.Evaluate();
            IEnumerable<RouteTable> routeTables = this.GetRouteTables(list, routingStrategy);
            return routeTables;
        }
        IEnumerable<RouteTable> Handle_Enumerable_Contains_MethodCall(MethodCallExpression exp)
        {
            MemberInfo member = null;
            IRoutingStrategy routingStrategy = this.GetRoutingStrategy(exp.Arguments[1], out member);
            if (routingStrategy == null)
            {
                return base.VisitMethodCall(exp);
            }

            if (!exp.Arguments[0].IsEvaluable())
            {
                return base.VisitMethodCall(exp);
            }

            IEnumerable list = (IEnumerable)exp.Arguments[0].Evaluate();
            IEnumerable<RouteTable> routeTables = this.GetRouteTables(list, routingStrategy);
            return routeTables;
        }
        IEnumerable<RouteTable> Handle_Sql_Equals_MethodCall(MethodCallExpression exp)
        {
            var equalExp = Expression.MakeBinary(ExpressionType.Equal, exp.Arguments[0], exp.Arguments[1]);
            return this.VisitBinary(equalExp);
        }
        IEnumerable<RouteTable> Handle_Sql_NotEquals_MethodCall(MethodCallExpression exp)
        {
            var notEqualExp = Expression.MakeBinary(ExpressionType.NotEqual, exp.Arguments[0], exp.Arguments[1]);
            return this.VisitBinary(notEqualExp);
        }
        IEnumerable<RouteTable> Handle_Sql_Compare_MethodCall(MethodCallExpression exp)
        {
            CompareType compareType = (CompareType)exp.Arguments[1].Evaluate();

            ExpressionType expressionType;

            switch (compareType)
            {
                case CompareType.eq:
                    expressionType = ExpressionType.Equal;
                    break;
                case CompareType.neq:
                    expressionType = ExpressionType.NotEqual;
                    break;
                case CompareType.gt:
                    expressionType = ExpressionType.GreaterThan;
                    break;
                case CompareType.gte:
                    expressionType = ExpressionType.GreaterThanOrEqual;
                    break;
                case CompareType.lt:
                    expressionType = ExpressionType.LessThan;
                    break;
                case CompareType.lte:
                    expressionType = ExpressionType.LessThanOrEqual;
                    break;
                default:
                    throw new NotSupportedException(compareType.ToString());
            }

            var newExp = Expression.MakeBinary(expressionType, exp.Arguments[0], exp.Arguments[2]);
            return this.VisitBinary(newExp);
        }
        IEnumerable<RouteTable> Handle_Instance_Equals_MethodCall(MethodCallExpression exp)
        {
            var equalExp = Expression.MakeBinary(ExpressionType.Equal, exp.Object, Expression.Convert(exp.Arguments[0], exp.Object.Type));
            return this.VisitBinary(equalExp);
        }
        IEnumerable<RouteTable> Handle_In_Extension_MethodCall(MethodCallExpression exp)
        {
            MethodInfo method = exp.Method;

            Type[] genericArguments = method.GetGenericArguments();
            Type genericType = genericArguments[0];

            MethodInfo method_Contains = PublicConstants.MethodInfo_Enumerable_Contains.MakeGenericMethod(genericType);
            List<Expression> arguments = new List<Expression>(2) { exp.Arguments[1], exp.Arguments[0] };
            MethodCallExpression newExp = Expression.Call(null, method_Contains, arguments);
            return this.VisitMethodCall(newExp);
        }
    }
}

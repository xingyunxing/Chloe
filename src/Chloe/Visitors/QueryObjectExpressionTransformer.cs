using Chloe.Infrastructure;
using Chloe.QueryExpressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Visitors
{
    /// <summary>
    /// 将 lambda 表达式树中的 IQuery 对象转成 QueryExpression
    /// </summary>
    public class QueryObjectExpressionTransformer : QueryExpressionVisitor
    {
        internal static List<string> AggregateMethods;

        static QueryObjectExpressionTransformer Instance { get; } = new QueryObjectExpressionTransformer();

        static QueryObjectExpressionTransformer()
        {
            List<string> aggregateMethods = new List<string>();
            aggregateMethods.Add(nameof(IQuery<int>.Count));
            aggregateMethods.Add(nameof(IQuery<int>.LongCount));
            aggregateMethods.Add(nameof(IQuery<int>.Max));
            aggregateMethods.Add(nameof(IQuery<int>.Min));
            aggregateMethods.Add(nameof(IQuery<int>.Sum));
            aggregateMethods.Add(nameof(IQuery<int>.Average));
            aggregateMethods.TrimExcess();
            AggregateMethods = aggregateMethods;
        }

        public static QueryExpression Transform(QueryExpression queryExpression)
        {
            if (!DoesQueryObjectExistJudgment.ExistsQueryObject(queryExpression))
                return queryExpression;

            return queryExpression.Accept(Instance);
        }

        public static Expression Transform(Expression expression)
        {
            if (!DoesQueryObjectExistJudgment.ExistsQueryObject(expression))
                return expression;

            return Instance.Visit(expression);
        }

        public static LambdaExpression Transform(LambdaExpression expression)
        {
            if (!DoesQueryObjectExistJudgment.ExistsQueryObject(expression))
                return expression;

            return (LambdaExpression)Instance.Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression exp)
        {
            if (IsComeFrom_First_Or_FirstOrDefault(exp))
            {
                Expression expression = this.Process_MemberAccess_Which_Link_First_Or_FirstOrDefault(exp);
                return expression;
            }

            return base.VisitMember(exp);
        }

        protected override Expression VisitMethodCall(MethodCallExpression exp)
        {
            /*
             * 处理：
             * IQuery<T>.First()
             * IQuery<T>.FirstOrDefault()
             * IQuery<T>.ToList()
             * IQuery<T>.Any()
             * IQuery<T>.Count()
             * IQuery<T>.LongCount()
             * IQuery<T>.Max()
             * IQuery<T>.Min()
             * IQuery<T>.Sum()
             * IQuery<T>.Average()
             */

            if (exp.Object != null && Utils.IsIQueryType(exp.Object.Type))
            {
                string methodName = exp.Method.Name;
                if (methodName == nameof(IQuery<int>.First) || methodName == nameof(IQuery<int>.FirstOrDefault))
                {
                    return this.Process_MethodCall_First_Or_FirstOrDefault(exp);
                }
                else if (methodName == nameof(IQuery<int>.ToList))
                {
                    //IQuery 对象的泛型参数必须是映射类型
                    EnsureIsMappingType(exp.Type.GetGenericArguments()[0], exp);
                    return MakeWrapperCall(exp.Object, exp.Method);
                }
                else if (methodName == nameof(IQuery<int>.Any))
                {
                    /* query.Any() --> exists 查询 */
                    exp = OptimizeCondition(exp);
                    return MakeWrapperCall(exp.Object, exp.Method);
                }
                else if (AggregateMethods.Contains(methodName))
                {
                    return this.Process_MethodCall_Aggregate(exp);
                }
            }

            return base.VisitMethodCall(exp);
        }

        Expression Process_MethodCall_Aggregate(MethodCallExpression exp)
        {
            return MakeWrapperCall(exp.Object, exp.Method, exp.Arguments);
        }

        Expression Process_MethodCall_First_Or_FirstOrDefault(MethodCallExpression exp)
        {
            /*
             * query.First() || query.First(a=> a.Id==1) || query.FirstOrDefault() || query.FirstOrDefault(a=> a.Id==1)
             */

            exp = OptimizeCondition(exp);

            //IQuery 对象的泛型参数必须是映射类型
            EnsureIsMappingType(exp.Type, exp);
            return MakeWrapperCall(exp.Object, exp.Method);
        }

        Expression Process_MemberAccess_Which_Link_First_Or_FirstOrDefault(MemberExpression exp)
        {
            /* 
             * 判断是不是 First().xx，FirstOrDefault().xx 之类的
             * First().Name： 泛型参数如果不是复杂类型，则转成 Select(a=> a.Name).First()
             * First().xx.Name：  如果 xx 是复杂类型，则转成 Select(a=> a.xx.Name).First()
             * First().xx.Name.Length：  如果 xx 是复杂类型，则转成 Select(a=> a.xx.Name).First().Length
             */

            // 分割
            MethodCallExpression methodCall = null;
            Stack<MemberExpression> memberExps = new Stack<MemberExpression>();

            Expression e = exp;
            while (e.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberExpression = (MemberExpression)e;
                memberExps.Push(memberExpression);
                e = memberExpression.Expression;
            }

            methodCall = (MethodCallExpression)e;
            methodCall = OptimizeCondition(methodCall);

            if (!MappingTypeSystem.IsMappingType(methodCall.Type))
            {
                /*
                 * query.First().xx.Name.Length --> query.Select(a=> a.xx.Name).First().Length
                 */

                ParameterExpression parameter = Expression.Parameter(methodCall.Type, "a");
                Expression selectorBody = parameter;
                while (memberExps.Count != 0)
                {
                    MemberExpression memberExpression = memberExps.Pop();
                    selectorBody = Expression.MakeMemberAccess(parameter, memberExpression.Member);

                    if (MappingTypeSystem.IsMappingType(selectorBody.Type))
                        break;
                }

                Type delegateType = typeof(Func<,>).MakeGenericType(parameter.Type, selectorBody.Type);
                LambdaExpression selector = Expression.Lambda(delegateType, selectorBody, parameter);

                Type queryType = ConvertToIQueryType(methodCall.Object.Type);
                var selectMethod = queryType.GetMethod(nameof(IQuery<int>.Select));
                selectMethod = selectMethod.MakeGenericMethod(selectorBody.Type);
                var selectMethodCall = Expression.Call(methodCall.Object, selectMethod, Expression.Quote(selector)); /* query.Select(a=> a.xx.Name) */

                var sameNameMethod = selectMethodCall.Type.GetMethod(methodCall.Method.Name, Type.EmptyTypes);
                var sameNameMethodCall = Expression.Call(selectMethodCall, sameNameMethod); /* query.Select(a=> a.xx.Name).First() */

                methodCall = sameNameMethodCall;
            }

            Expression expression = this.Visit(methodCall);
            while (memberExps.Count != 0)
            {
                MemberExpression memberExpression = memberExps.Pop();
                expression = Expression.MakeMemberAccess(expression, memberExpression.Member);
            }

            return expression;
        }

        static QueryExpression ExtractQueryExpression(Expression queryObjectExpression)
        {
            if (!Utils.IsIQueryType(queryObjectExpression.Type))
            {
                throw new NotSupportedException(queryObjectExpression.ToString());
            }

            IQuery query = ExpressionEvaluator.Evaluate(queryObjectExpression) as IQuery;
            return query.QueryExpression;
        }

        static Expression MakeWrapperCall(Expression queryObjectExpression, MethodInfo method, IEnumerable<Expression>? arguments = null)
        {
            return Expression.Call(MakeWrapper(queryObjectExpression), method, arguments);
        }
        static Expression MakeWrapper(Expression queryObjectExpression)
        {
            //将 IQuery 对象转成 QueryExpression
            QueryExpression queryExpression = ExtractQueryExpression(queryObjectExpression);

            /* queryObjectExpression.Type 为 IQuery<T>，与 queryExpression 的 Type 不等，这里设计不合理，将就这样包装先。注意，在后续解析处理的时候要使用 Convert 与 Constant.Value.Type=QueryExpression 组合判断 */
            var wrapper = Expression.Convert(Expression.Constant(queryExpression, typeof(object)), queryObjectExpression.Type);

            return wrapper;
        }
        static MethodCallExpression OptimizeCondition(MethodCallExpression exp)
        {
            if (exp.Arguments.Count == 1)
            {
                /* 
                 * query.First(a=> a.Id==1) --> query.Where(a=> a.Id==1).First()
                 * query.Any(a=> a.Id==1) --> query.Where(a=> a.Id==1).Any()
                 */
                Type queryType = exp.Object.Type;
                var whereMethod = queryType.GetMethod(nameof(IQuery<int>.Where));
                var whereMethodCall = Expression.Call(exp.Object, whereMethod, exp.Arguments[0]);
                var sameNameMethod = queryType.GetMethod(exp.Method.Name, Type.EmptyTypes);
                var sameNameMethodCall = Expression.Call(whereMethodCall, sameNameMethod);
                exp = sameNameMethodCall;
            }
            return exp;
        }

        static void EnsureIsMappingType(Type type, MethodCallExpression exp)
        {
            if (!MappingTypeSystem.IsMappingType(type))
                throw new NotSupportedException(string.Format("The generic parameter type of method {0} must be mapping type.", exp.Method.Name));
        }

        static Type ConvertToIQueryType(Type type)
        {
            Type queryType = typeof(IQuery<>);
            if (queryType == type.GetGenericTypeDefinition())
                return type;

            Type implementedInterface = type.GetInterface("IQuery`1");
            return implementedInterface;
        }

        static bool IsComeFrom_First_Or_FirstOrDefault(MemberExpression exp)
        {
            Expression e = exp;
            while (e.NodeType == ExpressionType.MemberAccess)
            {
                e = (e as MemberExpression).Expression;
                if (e == null)
                    return false;
            }

            if (e.NodeType != ExpressionType.Call)
            {
                return false;
            }

            MethodCallExpression methodCall = (MethodCallExpression)e;
            if (methodCall.Method.Name != nameof(IQuery<int>.First) && methodCall.Method.Name != nameof(IQuery<int>.FirstOrDefault))
                return false;

            return Utils.IsIQueryType(methodCall.Object.Type);
        }
    }
}

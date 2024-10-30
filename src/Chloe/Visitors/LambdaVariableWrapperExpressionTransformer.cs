using Chloe.QueryExpressions;
using Chloe.Reflection;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    /*

    System.Linq.Expressions.Expression<Func<Person, bool>> mm = a => dbContext.Query<Person>(b => b.Name == "1").Any();
    dbContext.Query<Person>().Where(c => dbContext.Query<Person>().Where(  mm                                                      ).Any()).Count();

    如上操作中表达式树中使用变量的形式将 lambda(mm 变量) 传入表达式树中，编译器会使用变量包装类型把 mm 变量包装起来，此类功能则是去掉 mm 的包装类型：

    dbContext.Query<Person>().Where(c => dbContext.Query<Person>().Where(  mm                                                      ).Any()).Count();
    -->
    dbContext.Query<Person>().Where(c => dbContext.Query<Person>().Where(  a => dbContext.Query<Person>(b => b.Name == "1").Any()  ).Any()).Count();
     
     */

    /// <summary>
    /// 去掉表达式树中对 lambda 的变量包装
    /// </summary>
    public class LambdaVariableWrapperExpressionTransformer : QueryExpressionVisitor
    {
        static LambdaVariableWrapperExpressionTransformer Instance { get; } = new LambdaVariableWrapperExpressionTransformer();

        public static QueryExpression Transform(QueryExpression queryExpression)
        {
            if (!DoesLambdaVariableWrapperExistJudgment.Exists(queryExpression))
            {
                return queryExpression;
            }

            return queryExpression.Accept(Instance);
        }

        public static Expression Transform(Expression expression)
        {
            if (!DoesLambdaVariableWrapperExistJudgment.Exists(expression))
                return expression;

            return Instance.Visit(expression);
        }

        public static LambdaExpression Transform(LambdaExpression expression)
        {
            if (!DoesLambdaVariableWrapperExistJudgment.Exists(expression))
                return expression;

            return (LambdaExpression)Instance.Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression exp)
        {
            if (exp.Expression != null && Utils.IsVariableWrapperType(exp.Expression.Type))
            {
                if (typeof(LambdaExpression).IsAssignableFrom(exp.Type))
                {
                    var lambda = exp.Member.GetMemberValue((exp.Expression as ConstantExpression).Value) as LambdaExpression;
                    return this.Visit(lambda);
                }
            }

            return base.VisitMember(exp);
        }
    }

}

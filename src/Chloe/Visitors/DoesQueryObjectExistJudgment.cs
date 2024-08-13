using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    /// <summary>
    /// 判断表达式树中是否存在 IQuery 对象
    /// </summary>
    public class DoesQueryObjectExistJudgment : ExpressionTraversal
    {
        public static DoesQueryObjectExistJudgment Instance { get; } = new DoesQueryObjectExistJudgment();


        public static bool ExistsQueryObject(QueryExpression queryExpression)
        {
            return Instance.Visit(queryExpression);
        }

        public static bool ExistsQueryObject(Expression expression)
        {
            return Instance.Visit(expression);
        }


        #region Expression

        public override bool Visit(Expression exp)
        {
            if (exp == null)
                return default(bool);

            if (Utils.IsIQueryType(exp.Type))
            {
                return true;
            }

            return base.Visit(exp);
        }

        protected override bool VisitExpression(Expression exp)
        {
            return Utils.IsIQueryType(exp.Type);
        }

        #endregion
    }
}

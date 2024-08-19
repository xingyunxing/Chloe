using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    /// <summary>
    /// 判断表达式树中是否存在 IQuery 对象
    /// </summary>
    public class DoesQueryObjectExistJudgment : ExpressionTraversal
    {
        bool _exists;

        public static bool ExistsQueryObject(QueryExpression queryExpression)
        {
            DoesQueryObjectExistJudgment judgment = new DoesQueryObjectExistJudgment();
            judgment.Visit(queryExpression);
            return judgment._exists;
        }

        public static bool ExistsQueryObject(Expression expression)
        {
            DoesQueryObjectExistJudgment judgment = new DoesQueryObjectExistJudgment();
            judgment.Visit(expression);
            return judgment._exists;
        }


        public override void Visit(Expression exp)
        {
            if (this._exists)
                return;

            if (exp == null)
                return;

            if (Utils.IsIQueryType(exp.Type))
            {
                this._exists = true;
                return;
            }

            base.Visit(exp);
        }

        public override void Visit(QueryExpression exp)
        {
            if (this._exists)
                return;

            base.Visit(exp);
        }

    }
}

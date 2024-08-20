using Chloe.DbExpressions;

namespace Chloe.Visitors
{
    public class DbExpressionEvaluableJudgment : DbExpressionVisitor<bool>
    {
        static DbExpressionEvaluableJudgment _judgment = new DbExpressionEvaluableJudgment();

        public static bool IsEvaluable(DbExpression exp)
        {
            return _judgment.VisitCore(exp);
        }

        public virtual bool VisitCore(DbExpression exp)
        {
            if (exp == null)
                throw new ArgumentNullException();

            switch (exp.NodeType)
            {
                case DbExpressionType.Constant:
                case DbExpressionType.MemberAccess:
                case DbExpressionType.MethodCall:
                case DbExpressionType.Not:
                case DbExpressionType.Convert:
                case DbExpressionType.Parameter:
                    return exp.Accept(_judgment);
                default:
                    break;
            }

            return false;
        }

        public override bool VisitConstant(DbConstantExpression exp)
        {
            return true;
        }
        public override bool VisitMemberAccess(DbMemberAccessExpression exp)
        {
            if (exp.Expression != null)
            {
                return this.VisitCore(exp.Expression);
            }

            return true;
        }
        public override bool VisitMethodCall(DbMethodCallExpression exp)
        {
            if (exp.Object != null)
            {
                if (!this.VisitCore(exp.Object))
                    return false;
            }

            foreach (var argument in exp.Arguments)
            {
                if (!this.VisitCore(argument))
                {
                    return false;
                }
            }

            return true;
        }
        public override bool VisitNot(DbNotExpression exp)
        {
            return this.VisitCore(exp.Operand);
        }
        public override bool VisitConvert(DbConvertExpression exp)
        {
            return this.VisitCore(exp.Operand);
        }
        public override bool VisitParameter(DbParameterExpression exp)
        {
            return true;
        }
    }
}

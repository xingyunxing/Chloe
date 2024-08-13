using System.Linq.Expressions;

namespace Chloe.Visitors
{
    public class ExpressionEvaluableJudgment : ExpressionVisitor<bool>
    {
        static ExpressionEvaluableJudgment _judge = new ExpressionEvaluableJudgment();

        public static bool CanEvaluate(Expression exp)
        {
            return _judge.Visit(exp);
        }

        protected override bool VisitExpression(Expression exp)
        {
            return true;
        }

        protected override bool VisitUnary(UnaryExpression exp)
        {
            return this.Visit(exp.Operand);
        }

        protected override bool VisitBinary(BinaryExpression exp)
        {
            return this.Visit(exp.Left) && this.Visit(exp.Right);
        }

        protected override bool VisitLambda(LambdaExpression exp)
        {
            for (int i = 0; i < exp.Parameters.Count; i++)
            {
                if (!this.Visit(exp.Parameters[i]))
                {
                    return false;
                }
            }

            return this.Visit(exp.Body);
        }
        protected override bool VisitMemberAccess(MemberExpression exp)
        {
            if (exp.Expression != null)
            {
                return this.Visit(exp.Expression);
            }

            return true;
        }
        protected override bool VisitConditional(ConditionalExpression exp)
        {
            return this.Visit(exp.Test) && this.Visit(exp.IfTrue) && this.Visit(exp.IfFalse);
        }
        protected override bool VisitMethodCall(MethodCallExpression exp)
        {
            if (exp.Object != null)
            {
                if (!this.Visit(exp.Object))
                    return false;
            }

            return this.IsAllEvaluable(exp.Arguments);
        }
        protected override bool VisitNew(NewExpression exp)
        {
            return this.IsAllEvaluable(exp.Arguments);
        }
        protected override bool VisitNewArray(NewArrayExpression exp)
        {
            return this.IsAllEvaluable(exp.Expressions);
        }
        protected override bool VisitMemberInit(MemberInitExpression exp)
        {
            return this.Visit(exp.NewExpression);
        }
        protected override bool VisitListInit(ListInitExpression exp)
        {
            return this.Visit(exp.NewExpression);
        }

        bool IsAllEvaluable(IList<Expression> exps)
        {
            for (int i = 0; i < exps.Count; i++)
            {
                if (!this.Visit(exps[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

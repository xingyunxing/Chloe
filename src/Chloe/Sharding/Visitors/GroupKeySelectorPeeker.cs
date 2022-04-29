using Chloe.Core.Visitors;
using System.Linq.Expressions;

namespace Chloe.Sharding.Visitors
{
    class GroupKeySelectorPeeker : ExpressionVisitor<LambdaExpression[]>
    {
        LambdaExpression _rawSelector;

        public GroupKeySelectorPeeker(LambdaExpression rawSelector)
        {
            this._rawSelector = rawSelector;
        }

        public static LambdaExpression[] Peek(LambdaExpression keySelector)
        {
            var peeker = new GroupKeySelectorPeeker(keySelector);
            return peeker.Visit(keySelector);
        }
        public static List<LambdaExpression> Peek(IEnumerable<LambdaExpression> keySelectors)
        {
            List<LambdaExpression> ret = new List<LambdaExpression>();
            foreach (var keySelector in keySelectors)
            {
                ret.AddRange(Peek(keySelector));
            }

            return ret;
        }

        public override LambdaExpression[] Visit(Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                default:
                    {
                        var delType = typeof(Func<,>).MakeGenericType(this._rawSelector.Parameters[0].Type, exp.Type);
                        LambdaExpression keySelector = Expression.Lambda(delType, exp, this._rawSelector.Parameters[0]);
                        return new LambdaExpression[1] { keySelector };
                    }
            }
        }

        protected override LambdaExpression[] VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }
        protected override LambdaExpression[] VisitNew(NewExpression exp)
        {
            LambdaExpression[] ret = new LambdaExpression[exp.Arguments.Count];
            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                var argExp = exp.Arguments[i];
                var delType = typeof(Func<,>).MakeGenericType(this._rawSelector.Parameters[0].Type, argExp.Type);
                LambdaExpression keySelector = Expression.Lambda(delType, argExp, this._rawSelector.Parameters[0]);
                ret[i] = keySelector;
            }

            return ret;
        }
    }
}

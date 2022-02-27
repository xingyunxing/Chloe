using Chloe.Core.Visitors;
using Chloe.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    internal class UniqueDataQueryAuthenticator : ExpressionVisitor<bool>
    {
        MemberInfo _primaryKey;

        public UniqueDataQueryAuthenticator(MemberInfo primaryKey)
        {
            this._primaryKey = primaryKey;
        }

        public static bool IsUniqueDataQuery(Expression exp, MemberInfo primaryKey)
        {
            if (exp == null)
                return false;

            UniqueDataQueryAuthenticator authenticator = new UniqueDataQueryAuthenticator(primaryKey);
            return authenticator.Visit(exp);
        }

        protected override bool VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }

        protected override bool VisitExpression(Expression exp)
        {
            return false;
        }

        protected override bool VisitBinary_Equal(BinaryExpression exp)
        {
            bool isPrimaryKeyMemberAccess = this.IsPrimaryKeyMemberAccess(exp.Left);
            Expression otherSideExp = exp.Right;

            if (!isPrimaryKeyMemberAccess)
            {
                isPrimaryKeyMemberAccess = this.IsPrimaryKeyMemberAccess(exp.Right);
                otherSideExp = exp.Left;
            }

            if (isPrimaryKeyMemberAccess)
            {
                //TODO: 考虑 DateTime.Now 等可翻译情况
                bool isEvaluable = otherSideExp.IsEvaluable();
                return isEvaluable;
            }

            return false;
        }

        protected override bool VisitBinary_AndAlso(BinaryExpression exp)
        {
            return this.Visit(exp.Left) || this.Visit(exp.Right);
        }
        protected override bool VisitMethodCall(MethodCallExpression exp)
        {
            //TODO: 考虑 Equal 等方法调用
            return base.VisitMethodCall(exp);
        }

        bool IsPrimaryKeyMemberAccess(Expression exp)
        {
            exp = exp.StripConvert();
            if (exp.NodeType != ExpressionType.MemberAccess)
            {
                return false;
            }

            var memberExp = exp as MemberExpression;
            if (memberExp.Expression.NodeType == ExpressionType.Parameter)
            {
                // a.Id
                var member = memberExp.Member;
                return this._primaryKey == member;
            }

            return false;
        }
    }
}

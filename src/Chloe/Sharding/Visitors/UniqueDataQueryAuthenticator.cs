using Chloe.Visitors;
using Chloe.Extensions;
using System.Linq.Expressions;

namespace Chloe.Sharding.Visitors
{
    internal class UniqueDataQueryAuthenticator : ExpressionVisitor<bool>
    {
        IShardingContext _shardingContext;

        public UniqueDataQueryAuthenticator(IShardingContext shardingContext)
        {
            this._shardingContext = shardingContext;
        }

        public static bool IsUniqueDataQuery(IShardingContext shardingContext, Expression exp)
        {
            if (exp == null)
                return false;

            UniqueDataQueryAuthenticator authenticator = new UniqueDataQueryAuthenticator(shardingContext);
            return authenticator.Visit(exp);
        }
        public static bool IsUniqueDataQuery(IShardingContext shardingContext, IEnumerable<Expression> exps)
        {
            UniqueDataQueryAuthenticator authenticator = new UniqueDataQueryAuthenticator(shardingContext);
            foreach (Expression exp in exps)
            {
                bool isUniqueQuery = authenticator.Visit(exp);

                if (isUniqueQuery)
                {
                    return true;
                }
            }

            return false;
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
            bool isUniqueDataMemberAccess = this.IsUniqueDataMemberAccess(exp.Left);
            Expression anotherSideExp = exp.Right;

            if (!isUniqueDataMemberAccess)
            {
                isUniqueDataMemberAccess = this.IsUniqueDataMemberAccess(exp.Right);
                anotherSideExp = exp.Left;
            }

            if (isUniqueDataMemberAccess)
            {
                //TODO: 考虑 DateTime.Now 等可翻译情况
                bool isEvaluable = anotherSideExp.IsEvaluable();

                if (!isEvaluable)
                {
                    return false;
                }

                var value = anotherSideExp.Evaluate();

                return value != null;
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

        bool IsUniqueDataMemberAccess(Expression exp)
        {
            exp = exp.StripConvert();
            if (exp.NodeType != ExpressionType.MemberAccess)
            {
                return false;
            }

            var memberExp = exp as MemberExpression;
            if (memberExp.Expression.NodeType == ExpressionType.Parameter)
            {
                // a.Id, a.MobileNumber
                var member = memberExp.Member;
                return this._shardingContext.IsPrimaryKey(member) || this._shardingContext.IsUniqueIndex(member);
            }

            return false;
        }
    }
}

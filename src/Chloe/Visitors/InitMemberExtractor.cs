using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Visitors
{
    public class InitMemberExtractor : ExpressionVisitor<List<KeyValuePair<MemberInfo, Expression>>>
    {
        static readonly InitMemberExtractor _extractor = new InitMemberExtractor();

        InitMemberExtractor()
        {

        }

        public static List<KeyValuePair<MemberInfo, Expression>> Extract(Expression exp)
        {
            return _extractor.Visit(exp);
        }

        public override List<KeyValuePair<MemberInfo, Expression>> Visit(Expression exp)
        {
            if (exp == null)
                return null;

            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }
        protected override List<KeyValuePair<MemberInfo, Expression>> VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }
        protected override List<KeyValuePair<MemberInfo, Expression>> VisitMemberInit(MemberInitExpression exp)
        {
            List<KeyValuePair<MemberInfo, Expression>> ret = new List<KeyValuePair<MemberInfo, Expression>>(exp.Bindings.Count);

            foreach (MemberBinding binding in exp.Bindings)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }

                MemberAssignment memberAssignment = (MemberAssignment)binding;
                MemberInfo member = memberAssignment.Member;

                ret.Add(new KeyValuePair<MemberInfo, Expression>(member, memberAssignment.Expression));
            }

            return ret;
        }
    }
}

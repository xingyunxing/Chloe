using System.Linq.Expressions;

namespace Chloe.Visitors
{
    public abstract class ExpressionVisitor<T>
    {
        protected ExpressionVisitor()
        {
        }

        public virtual T Visit(Expression exp)
        {
            if (exp == null)
                return default(T);

            switch (exp.NodeType)
            {
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Quote:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                //case ExpressionType.TypeIs:
                //    return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                    //case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                //case ExpressionType.Invoke:
                //    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        protected virtual T VisitExpression(Expression exp)
        {
            throw new NotImplementedException(exp.ToString());
        }

        protected virtual T VisitUnary(UnaryExpression exp)
        {
            if (exp == null)
                return default(T);

            switch (exp.NodeType)
            {
                case ExpressionType.Not:
                    return this.VisitUnary_Not(exp);
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return this.VisitUnary_Convert(exp);
                case ExpressionType.Quote:
                    return this.VisitUnary_Quote(exp);
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return this.VisitUnary_Negate(exp);
                case ExpressionType.ArrayLength:
                    return this.VisitUnary_ArrayLength(exp);
                case ExpressionType.TypeAs:
                    return this.VisitUnary_TypeAs(exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }
        protected virtual T VisitUnary_Not(UnaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitUnary_Convert(UnaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitUnary_Quote(UnaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitUnary_Negate(UnaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitUnary_ArrayLength(UnaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitUnary_TypeAs(UnaryExpression exp)
        {
            return this.VisitExpression(exp);
        }

        protected virtual T VisitBinary(BinaryExpression exp)
        {
            if (exp == null)
                return default(T);

            switch (exp.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return this.VisitBinary_Add(exp);
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return this.VisitBinary_Subtract(exp);
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return this.VisitBinary_Multiply(exp);
                case ExpressionType.Divide:
                    return this.VisitBinary_Divide(exp);
                case ExpressionType.Modulo:
                    return this.VisitBinary_Modulo(exp);
                case ExpressionType.And:
                    return this.VisitBinary_And(exp);
                case ExpressionType.AndAlso:
                    return this.VisitBinary_AndAlso(exp);
                case ExpressionType.Or:
                    return this.VisitBinary_Or(exp);
                case ExpressionType.OrElse:
                    return this.VisitBinary_OrElse(exp);
                case ExpressionType.LessThan:
                    return this.VisitBinary_LessThan(exp);
                case ExpressionType.LessThanOrEqual:
                    return this.VisitBinary_LessThanOrEqual(exp);
                case ExpressionType.GreaterThan:
                    return this.VisitBinary_GreaterThan(exp);
                case ExpressionType.GreaterThanOrEqual:
                    return this.VisitBinary_GreaterThanOrEqual(exp);
                case ExpressionType.Equal:
                    return this.VisitBinary_Equal(exp);
                case ExpressionType.NotEqual:
                    return this.VisitBinary_NotEqual(exp);
                case ExpressionType.Coalesce:
                    return this.VisitBinary_Coalesce(exp);
                case ExpressionType.ArrayIndex:
                    return this.VisitBinary_ArrayIndex(exp);
                case ExpressionType.RightShift:
                    return this.VisitBinary_RightShift(exp);
                case ExpressionType.LeftShift:
                    return this.VisitBinary_LeftShift(exp);
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary_ExclusiveOr(exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }
        protected virtual T VisitBinary_Add(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_Subtract(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_Multiply(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_Divide(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_Modulo(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_And(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_AndAlso(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_Or(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_OrElse(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_LessThan(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_LessThanOrEqual(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_GreaterThan(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_GreaterThanOrEqual(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_Equal(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_NotEqual(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_Coalesce(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_ArrayIndex(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_RightShift(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_LeftShift(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitBinary_ExclusiveOr(BinaryExpression exp)
        {
            return this.VisitExpression(exp);
        }

        protected virtual T VisitConstant(ConstantExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitParameter(ParameterExpression exp)
        {
            return this.VisitExpression(exp);
        }

        protected virtual T VisitLambda(LambdaExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitMemberAccess(MemberExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitConditional(ConditionalExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitMethodCall(MethodCallExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitNew(NewExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitNewArray(NewArrayExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitMemberInit(MemberInitExpression exp)
        {
            return this.VisitExpression(exp);
        }
        protected virtual T VisitListInit(ListInitExpression exp)
        {
            return this.VisitExpression(exp);
        }
    }

}

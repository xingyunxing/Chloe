using Chloe.QueryExpressions;
using Chloe.Reflection;
using System.Linq.Expressions;

namespace Chloe.Query
{
    /// <summary>
    /// 用插槽替换表达树中的变量和 Chloe.ConstantWrapper 对象
    /// </summary>
    public class ExpressionVariableReplacer : QueryExpressionVisitor
    {
        List<object> _variables = new List<object>();

        public static QueryExpression Replace(QueryExpression queryExpression, out List<object> variables)
        {
            ExpressionVariableReplacer replacer = new ExpressionVariableReplacer();
            QueryExpression exp = queryExpression.Accept(replacer);
            variables = replacer._variables;
            return exp;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (Utils.IsVariableWrapperType(node.Type) || Utils.IsChloeConstantWrapperType(node.Type))
            {
                Type slotType = typeof(VariableSlot<>).MakeGenericType(node.Type);
                var slot = slotType.GetConstructor(new Type[] { typeof(int) }).FastCreateInstance(this._variables.Count);
                this._variables.Add(node.Value);
                return Expression.MakeMemberAccess(Expression.Constant(slot, slotType), slotType.GetProperty(nameof(VariableSlot<int>.Value)));
            }

            return base.VisitConstant(node);
        }
    }
}

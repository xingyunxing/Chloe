using Chloe.DbExpressions;

namespace Chloe.Query
{
    /// <summary>
    /// 将变量插入对应的插槽
    /// </summary>
    public class DbExpressionVariableInjector : DbExpressionVisitor
    {
        List<object> _variables;

        public DbExpressionVariableInjector(List<object> variables)
        {
            this._variables = variables;
        }

        public static DbExpression Inject(DbExpression exp, List<object> variables)
        {
            DbExpressionVariableInjector injector = new DbExpressionVariableInjector(variables);
            return exp.Accept(injector);
        }

        public override DbExpression VisitMemberAccess(DbMemberAccessExpression exp)
        {
            if (exp.Member.Name == nameof(VariableSlot<int>.Value))
            {
                if (exp.Expression is DbConstantExpression dbConstantExpression)
                {
                    VariableSlot slot = dbConstantExpression.Value as VariableSlot;
                    if (slot != null)
                    {
                        object variable = this._variables[slot.Index];
                        return new DbConstantExpression(variable);
                    }
                }
            }

            return base.VisitMemberAccess(exp);
        }
    }
}

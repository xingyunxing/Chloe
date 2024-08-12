
#if !NET46 && !NETSTANDARD2

using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Query
{
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

        public override DbExpression Visit(DbMemberExpression exp)
        {
            if (exp.Member.Name == nameof(VariableSlot<int>.Value))
            {
                if (exp.Expression is DbConstantExpression dbConstantExpression)
                {
                    VariableSlot solt = dbConstantExpression.Value as VariableSlot;
                    if (solt != null)
                    {
                        object variable = this._variables[solt.Index];
                        return DbExpression.Constant(variable);
                    }
                }
            }

            return base.Visit(exp);
        }
    }
}

#endif
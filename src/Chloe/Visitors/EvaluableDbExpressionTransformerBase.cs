using Chloe.Annotations;
using Chloe.DbExpressions;
using Chloe.Query;
using Chloe.RDBMS;
using System.Reflection;

namespace Chloe.Visitors
{
    /// <summary>
    /// 将 DbExpression 中可求值的表达式计算出来，转换成 DbParameterExpression
    /// </summary>
    public abstract class EvaluableDbExpressionTransformerBase : DbExpressionVisitor
    {
        static List<object> EmptyVariables = new List<object>();
        List<object> _variables = EmptyVariables;

        protected EvaluableDbExpressionTransformerBase()
        {
        }

        protected EvaluableDbExpressionTransformerBase(List<object> variables)  //如果要处理的 DbExpression 中包含了插槽，务必将对应的变量传进来
        {
            this._variables = variables;
        }

        public static bool IsConstantOrParameter(DbExpression exp)
        {
            return exp != null && (exp.NodeType == DbExpressionType.Constant || exp.NodeType == DbExpressionType.Parameter);
        }

        protected virtual Dictionary<string, IPropertyHandler[]> GetPropertyHandlers()
        {
            throw new NotImplementedException();
        }
        protected virtual Dictionary<string, IMethodHandler[]> GetMethodHandlers()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 是否可以将 exp.Member 翻译成数据库对应的语法
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual bool CanTranslateToSql(DbMemberAccessExpression exp)
        {
            Dictionary<string, IPropertyHandler[]> propertyHandlerMap = this.GetPropertyHandlers();
            IPropertyHandler[] propertyHandlers;
            if (propertyHandlerMap.TryGetValue(exp.Member.Name, out propertyHandlers))
            {
                for (int i = 0; i < propertyHandlers.Length; i++)
                {
                    IPropertyHandler propertyHandler = propertyHandlers[i];
                    if (propertyHandler.CanProcess(exp))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// 是否可以将 exp.Method 翻译成数据库对应的语法
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public virtual bool CanTranslateToSql(DbMethodCallExpression exp)
        {
            Dictionary<string, IMethodHandler[]> methodHandlerMap = this.GetMethodHandlers();
            IMethodHandler[] methodHandlers;
            if (methodHandlerMap.TryGetValue(exp.Method.Name, out methodHandlers))
            {
                for (int i = 0; i < methodHandlers.Length; i++)
                {
                    IMethodHandler methodHandler = methodHandlers[i];
                    if (methodHandler.CanProcess(exp))
                    {
                        return true;
                    }
                }
            }

            return false;
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

            if (exp.Expression != null)
            {
                DbExpression caller = exp.Expression.Accept(this);
                if (caller != exp.Expression)
                    exp = new DbMemberAccessExpression(exp.Member, caller);
            }

            if (exp.Expression != null)
            {
                if (!IsConstantOrParameter(exp.Expression))
                    return exp;
            }

            if (this.CanTranslateToSql(exp))
                return exp;

            return new DbParameterExpression(exp.Evaluate(), exp.Type);
        }

        public override DbExpression VisitCoalesce(DbCoalesceExpression exp)
        {
            exp = new DbCoalesceExpression(exp.CheckExpression.Accept(this), exp.ReplacementValue.Accept(this));

            if (IsConstantOrParameter(exp.CheckExpression) && IsConstantOrParameter(exp.ReplacementValue))
            {
                return new DbParameterExpression(exp.Evaluate(), exp.Type);
            }

            return exp;
        }

        public override DbExpression VisitMethodCall(DbMethodCallExpression exp)
        {
            var args = exp.Arguments.Select(a => a.Accept(this)).ToList();
            DbExpression caller = exp.Object;
            if (exp.Object != null)
            {
                caller = exp.Object.Accept(this);
            }

            exp = new DbMethodCallExpression(caller, exp.Method, args);

            if (exp.Object != null)
            {
                if (!IsConstantOrParameter(exp.Object))
                    return exp;
            }

            foreach (var arg in exp.Arguments)
            {
                if (!IsConstantOrParameter(arg))
                    return exp;
            }

            if (this.CanTranslateToSql(exp))
                return exp;

            if (exp.Method.IsDefined(typeof(DbFunctionAttribute)))
            {
                return exp;
            }

            return new DbParameterExpression(exp.Evaluate(), exp.Type);
        }

        public override DbExpression VisitParameter(DbParameterExpression exp)
        {
            return exp;
        }
    }
}

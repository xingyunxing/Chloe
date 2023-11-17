using Chloe.Annotations;
using Chloe.DbExpressions;
using Chloe.RDBMS;
using System.Reflection;

namespace Chloe.Visitors
{
    /// <summary>
    /// 将 DbExpression 中可求值的表达式计算出来，转换成 DbParameterExpression
    /// </summary>
    public abstract class EvaluableDbExpressionTransformerBase : DbExpressionVisitor
    {
        protected EvaluableDbExpressionTransformerBase()
        {
        }

        public static bool IsConstantOrParameter(DbExpression exp)
        {
            return exp != null && (exp.NodeType == DbExpressionType.Constant || exp.NodeType == DbExpressionType.Parameter);
        }

        protected virtual HashSet<MemberInfo> GetToTranslateMembers()
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
        public virtual bool CanTranslateToSql(DbMemberExpression exp)
        {
            HashSet<MemberInfo> toTranslateMembers = this.GetToTranslateMembers();
            return toTranslateMembers.Contains(exp.Member);
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

        public override DbExpression Visit(DbMemberExpression exp)
        {
            if (exp.Expression != null)
            {
                DbExpression caller = exp.Expression.Accept(this);
                if (caller != exp.Expression)
                    exp = DbExpression.MemberAccess(exp.Member, caller);
            }

            if (exp.Expression != null)
            {
                if (!IsConstantOrParameter(exp.Expression))
                    return exp;
            }

            if (this.CanTranslateToSql(exp))
                return exp;

            return DbExpression.Parameter(exp.Evaluate(), exp.Type);
        }

        public override DbExpression Visit(DbCoalesceExpression exp)
        {
            exp = new DbCoalesceExpression(exp.CheckExpression.Accept(this), exp.ReplacementValue.Accept(this));

            if (IsConstantOrParameter(exp.CheckExpression) && IsConstantOrParameter(exp.ReplacementValue))
            {
                return DbExpression.Parameter(exp.Evaluate(), exp.Type);
            }

            return exp;
        }

        public override DbExpression Visit(DbMethodCallExpression exp)
        {
            var args = exp.Arguments.Select(a => a.Accept(this)).ToList();
            DbExpression caller = exp.Object;
            if (exp.Object != null)
            {
                caller = exp.Object.Accept(this);
            }

            exp = DbExpression.MethodCall(caller, exp.Method, args);

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

            return DbExpression.Parameter(exp.Evaluate(), exp.Type);
        }

        public override DbExpression Visit(DbParameterExpression exp)
        {
            return exp;
        }
    }
}

using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using Chloe.RDBMS;
using System.Reflection;

namespace Chloe.MySql.MethodHandlers
{
    class Equals_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            MethodInfo method = exp.Method;

            if (method.ReturnType != PublicConstants.TypeOfBoolean || method.IsStatic || exp.Arguments.Count != 1)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            DbExpression right = exp.Arguments[0];
            if (right.Type != exp.Object.Type)
            {
                right = DbExpression.Convert(right, exp.Object.Type);
            }

            DbExpression.Equal(exp.Object, right).Accept(generator);
        }
    }
}

using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.Visitors;

namespace Chloe.Dameng
{
    class EvaluableDbExpressionTransformer : EvaluableDbExpressionTransformerBase
    {
        static EvaluableDbExpressionTransformer _transformer = new EvaluableDbExpressionTransformer();

        static EvaluableDbExpressionTransformer()
        {

        }

        public EvaluableDbExpressionTransformer()
        {

        }

        public EvaluableDbExpressionTransformer(List<object> variables) : base(variables)
        {

        }

        public static DbExpression Transform(DbExpression exp, List<object>? variables = null)
        {
            if (variables == null || variables.Count == 0)
            {
                return exp.Accept(_transformer);
            }

            return exp.Accept(new EvaluableDbExpressionTransformer(variables));
        }

        protected override Dictionary<string, IPropertyHandler[]> GetPropertyHandlers()
        {
            return SqlGenerator.PropertyHandlerDic;
        }

        protected override Dictionary<string, IMethodHandler[]> GetMethodHandlers()
        {
            return SqlGenerator.MethodHandlerDic;
        }
    }
}

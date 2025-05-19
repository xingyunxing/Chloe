using Chloe.Visitors;
using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Chloe.MySql
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

        public override DbExpression VisitUpdate(DbUpdateExpression exp)
        {
            if (!(exp is MySqlDbUpdateExpression))
            {
                return base.VisitUpdate(exp);
            }

            MySqlDbUpdateExpression ret = new MySqlDbUpdateExpression(exp.Table, this.MakeNewExpression(exp.Condition));

            foreach (var pair in exp.UpdateColumns)
            {
                ret.AppendUpdateColumn(pair.Column, this.MakeNewExpression(pair.Value));
            }

            ret.Limits = (exp as MySqlDbUpdateExpression).Limits;

            return ret;
        }
        public override DbExpression VisitDelete(DbDeleteExpression exp)
        {
            if (!(exp is MySqlDbDeleteExpression))
            {
                return base.VisitDelete(exp);
            }

            var ret = new MySqlDbDeleteExpression(exp.Table, this.MakeNewExpression(exp.Condition));
            ret.Limits = (exp as MySqlDbDeleteExpression).Limits;

            return ret;
        }

        new DbExpression MakeNewExpression(DbExpression exp)
        {
            if (exp == null)
                return null;

            return exp.Accept(this);
        }
    }
}

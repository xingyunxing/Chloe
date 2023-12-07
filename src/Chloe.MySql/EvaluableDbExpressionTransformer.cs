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

        public static DbExpression Transform(DbExpression exp)
        {
            return exp.Accept(_transformer);
        }

        protected override Dictionary<string, IPropertyHandler[]> GetPropertyHandlers()
        {
            return SqlGenerator.PropertyHandlerDic;
        }

        protected override Dictionary<string, IMethodHandler[]> GetMethodHandlers()
        {
            return SqlGenerator.MethodHandlerDic;
        }

        public override DbExpression Visit(DbUpdateExpression exp)
        {
            if (!(exp is MySqlDbUpdateExpression))
            {
                return base.Visit(exp);
            }

            MySqlDbUpdateExpression ret = new MySqlDbUpdateExpression(exp.Table, this.MakeNewExpression(exp.Condition));

            foreach (var pair in exp.UpdateColumns)
            {
                ret.AppendUpdateColumn(pair.Column, this.MakeNewExpression(pair.Value));
            }

            ret.Limits = (exp as MySqlDbUpdateExpression).Limits;

            return ret;
        }
        public override DbExpression Visit(DbDeleteExpression exp)
        {
            if (!(exp is MySqlDbDeleteExpression))
            {
                return base.Visit(exp);
            }

            var ret = new MySqlDbDeleteExpression(exp.Table, this.MakeNewExpression(exp.Condition));
            ret.Limits = (exp as MySqlDbDeleteExpression).Limits;

            return ret;
        }

        DbExpression MakeNewExpression(DbExpression exp)
        {
            if (exp == null)
                return null;

            return exp.Accept(this);
        }
    }
}

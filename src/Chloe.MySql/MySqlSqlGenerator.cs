using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Chloe.MySql
{
    class MySqlSqlGenerator : SqlGenerator
    {
        public MySqlSqlGenerator(SqlGeneratorOptions options) : base(options)
        {

        }

        public static new SqlGenerator CreateInstance()
        {
            var options = new SqlGeneratorOptions()
            {
                LeftQuoteChar = UtilConstants.LeftQuoteChar,
                RightQuoteChar = UtilConstants.RightQuoteChar,
                MaxInItems = UtilConstants.MaxInItems
            };
            return new MySqlSqlGenerator(options);
        }

        public override DbExpression Visit(DbUpdateExpression exp)
        {
            base.Visit(exp);
            if (exp is MySqlDbUpdateExpression)
            {
                this.SqlBuilder.Append(" LIMIT ", (exp as MySqlDbUpdateExpression).Limits.ToString());
            }

            return exp;
        }
        public override DbExpression Visit(DbDeleteExpression exp)
        {
            base.Visit(exp);
            if (exp is MySqlDbDeleteExpression)
            {
                this.SqlBuilder.Append(" LIMIT ", (exp as MySqlDbDeleteExpression).Limits.ToString());
            }

            return exp;
        }
    }
}

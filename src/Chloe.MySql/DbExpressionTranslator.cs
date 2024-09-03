using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Infrastructure;
using Chloe.RDBMS;

namespace Chloe.MySql
{
    class DbExpressionTranslator : IDbExpressionTranslator
    {
        MySqlContextProvider ContextProvider { get; set; }

        public DbExpressionTranslator(MySqlContextProvider contextProvider)
        {
            this.ContextProvider = contextProvider;
        }

        public DbCommandInfo Translate(DbExpression expression, List<object> variables)
        {
            SqlGeneratorOptions options = this.CreateOptions();
            SqlGenerator generator = new SqlGenerator(options);

            expression = EvaluableDbExpressionTransformer.Transform(expression, variables);
            expression = DbExpressionNormalizer.Normalize(expression);
            expression.Accept(generator);

            DbCommandInfo dbCommandInfo = new DbCommandInfo();
            dbCommandInfo.Parameters = generator.Parameters;
            dbCommandInfo.CommandText = generator.SqlBuilder.ToSql();

            return dbCommandInfo;
        }

        SqlGeneratorOptions CreateOptions()
        {
            var options = new SqlGeneratorOptions()
            {
                LeftQuoteChar = UtilConstants.LeftQuoteChar,
                RightQuoteChar = UtilConstants.RightQuoteChar,
                MaxInItems = this.ContextProvider.Options.MaxInItems,
                RegardEmptyStringAsNull = this.ContextProvider.Options.RegardEmptyStringAsNull
            };

            return options;
        }
    }
}

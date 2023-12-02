using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Infrastructure;

namespace Chloe.Oracle
{
    class DbExpressionTranslator : IDbExpressionTranslator
    {
        OracleContextProvider ContextProvider { get; set; }

        public DbExpressionTranslator(OracleContextProvider contextProvider)
        {
            this.ContextProvider = contextProvider;
        }

        public DbCommandInfo Translate(DbExpression expression)
        {
            OracleSqlGeneratorOptions options = this.CreateOptions();
            SqlGenerator generator = new SqlGenerator(options);
            expression = EvaluableDbExpressionTransformer.Transform(expression);
            expression.Accept(generator);

            DbCommandInfo dbCommandInfo = new DbCommandInfo();
            dbCommandInfo.Parameters = generator.Parameters;
            dbCommandInfo.CommandText = generator.SqlBuilder.ToSql();

            return dbCommandInfo;
        }

        OracleSqlGeneratorOptions CreateOptions()
        {
            var options = new OracleSqlGeneratorOptions()
            {
                LeftQuoteChar = UtilConstants.LeftQuoteChar,
                RightQuoteChar = UtilConstants.RightQuoteChar,
                MaxInItems = this.ContextProvider.Options.MaxInItems,
                ConvertToUppercase = this.ContextProvider.Options.ConvertToUppercase
            };

            return options;
        }
    }
}

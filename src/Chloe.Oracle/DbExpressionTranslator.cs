using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Infrastructure;
using Chloe.RDBMS;

namespace Chloe.Oracle
{
    class DbExpressionTranslator : IDbExpressionTranslator
    {
        OracleContextProvider ContextProvider { get; set; }

        public DbExpressionTranslator(OracleContextProvider contextProvider)
        {
            this.ContextProvider = contextProvider;
        }

        public DbCommandInfo Translate(DbExpression expression, List<object> variables)
        {
            OracleSqlGeneratorOptions options = this.CreateOptions();
            SqlGenerator generator = new SqlGenerator(options);

            expression = EvaluableDbExpressionTransformer.Transform(expression, variables);
            expression = DbExpressionNormalizer.Normalize(expression);
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
                RegardEmptyStringAsNull = this.ContextProvider.Options.RegardEmptyStringAsNull,
                ConvertToUppercase = this.ContextProvider.Options.ConvertToUppercase
            };

            return options;
        }
    }
}

using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Infrastructure;
using Chloe.RDBMS;

namespace Chloe.KingbaseES
{
    class DbExpressionTranslator : IDbExpressionTranslator
    {
        KingbaseESContextProvider ContextProvider { get; set; }

        public DbExpressionTranslator(KingbaseESContextProvider contextProvider)
        {
            this.ContextProvider = contextProvider;
        }

        public DbCommandInfo Translate(DbExpression expression, List<object> variables)
        {
            KingbaseESSqlGeneratorOptions options = this.CreateOptions();
            SqlGenerator generator = new SqlGenerator(options);

            expression = EvaluableDbExpressionTransformer.Transform(expression, variables);
            expression = DbExpressionNormalizer.Normalize(expression);
            expression.Accept(generator);

            DbCommandInfo dbCommandInfo = new DbCommandInfo();
            dbCommandInfo.Parameters = generator.Parameters;
            dbCommandInfo.CommandText = generator.SqlBuilder.ToSql();

            return dbCommandInfo;
        }

        KingbaseESSqlGeneratorOptions CreateOptions()
        {
            var options = new KingbaseESSqlGeneratorOptions()
            {
                LeftQuoteChar = UtilConstants.LeftQuoteChar,
                RightQuoteChar = UtilConstants.RightQuoteChar,
                MaxInItems = this.ContextProvider.Options.MaxInItems,
                ConvertToLowercase = this.ContextProvider.Options.ConvertToLowercase
            };

            return options;
        }
    }
}

using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Infrastructure;
using Chloe.RDBMS;

namespace Chloe.Dameng
{
    class DbExpressionTranslator : IDbExpressionTranslator
    {
        DamengContextProvider ContextProvider { get; set; }

        public DbExpressionTranslator(DamengContextProvider contextProvider)
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

            var dbCommandInfo = new DbCommandInfo
            {
                Parameters = generator.Parameters,
                CommandText = generator.SqlBuilder.ToSql()
            };

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

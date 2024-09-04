using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Infrastructure;
using Chloe.RDBMS;

namespace Chloe.SqlServer
{
    class DbExpressionTranslator : IDbExpressionTranslator
    {
        MsSqlContextProvider ContextProvider { get; set; }

        public DbExpressionTranslator(MsSqlContextProvider contextProvider)
        {
            this.ContextProvider = contextProvider;
        }

        public DbCommandInfo Translate(DbExpression expression, List<object> variables)
        {
            SqlServerSqlGeneratorOptions options = this.CreateOptions();
            SqlGenerator generator = new SqlGenerator(options);

            expression = EvaluableDbExpressionTransformer.Transform(expression, variables);
            expression = DbExpressionNormalizer.Normalize(expression);
            expression.Accept(generator);

            DbCommandInfo dbCommandInfo = new DbCommandInfo();
            dbCommandInfo.Parameters = generator.Parameters;
            dbCommandInfo.CommandText = generator.SqlBuilder.ToSql();

            return dbCommandInfo;
        }

        SqlServerSqlGeneratorOptions CreateOptions()
        {
            var options = new SqlServerSqlGeneratorOptions()
            {
                LeftQuoteChar = UtilConstants.LeftQuoteChar,
                RightQuoteChar = UtilConstants.RightQuoteChar,
                MaxInItems = this.ContextProvider.Options.MaxInItems,
                TreatEmptyStringAsNull = this.ContextProvider.Options.TreatEmptyStringAsNull,
                PagingMode = this.ContextProvider.Options.PagingMode,
                BindParameterByName = this.ContextProvider.Options.BindParameterByName
            };

            return options;
        }
    }
}
